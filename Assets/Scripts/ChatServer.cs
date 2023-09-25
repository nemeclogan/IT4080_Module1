using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatServer : NetworkBehaviour
{
    public ChatUi chatUi;
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];

    void Start()
    {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
            if (IsHost) {
                DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.ServerClientId}");
            } else {
                DisplayMessageLocally(SYSTEM_ID, "You are the server");
            }
        } else {
            DisplayMessageLocally(SYSTEM_ID, $"You are a client {NetworkManager.LocalClientId}");
        }
    }

    private void ServerOnClientConnected(ulong clientId)
    {
       DisplayMessageToAllClients($"Player {clientId} has connected.");
       ServerSendDirectMessage($"Welcome to the server, Player {clientId}!", SYSTEM_ID, clientId);
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        DisplayMessageToAllClients($"Player {clientId} has disconnected.");
    }

    private void DisplayMessageToAllClients(string message)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.Value.ClientId != NetworkManager.ServerClientId)
            {
                ServerSendDirectMessage(message, SYSTEM_ID, client.Key);
            }
        }
    }

    private void DisplayMessageLocally(ulong from, string message) {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;

        if(from == NetworkManager.LocalClientId) {
            fromStr = "you";
            textColor = Color.magenta;
        } else if(from == SYSTEM_ID) {
            fromStr = "SYS";
            textColor = Color.green;
        }
        chatUi.addEntry(fromStr, message, textColor);
    }

    private void OnChatUiMessageEntered(string message)
    {
        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        if (message.StartsWith("@"))
        {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            ulong toClientId;
            if (ulong.TryParse(clientIdStr, out toClientId))
            {
                if (NetworkManager.Singleton.ConnectedClients.ContainsKey(toClientId)) {
                    ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);
                } else {
                    // Notify the sender that the message could not be sent
                    ServerSendDirectMessage($"Message to {toClientId} could not be sent.", SYSTEM_ID, serverRpcParams.Receive.SenderClientId);
                }
            } else {
                // Notify the sender that the message format is invalid
                ServerSendDirectMessage($"Invalid message format: {message}", SYSTEM_ID, serverRpcParams.Receive.SenderClientId);
            }
        } else {
            ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default)
    {
        DisplayMessageLocally(from, message);
    }

    private void ServerSendDirectMessage(string message, ulong from, ulong to) {
        dmClientIds[0] = from;
        dmClientIds[1] = to;

        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;

        ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
    }
}