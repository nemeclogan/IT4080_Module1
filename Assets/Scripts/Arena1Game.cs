using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Arena1Game : NetworkBehaviour
{
    public Player playerPrefab;
    public Camera arenaCamera;
    public Player hostPrefab;

    private NetworkedPlayers networkedPlayers;

    private int positionIndex = 0;
    private Vector3[] startPositions = new Vector3[]
    {
        new Vector3(4, 2, 0),
        new Vector3(-4, 2, 0),
        new Vector3(0, 2, 4),
        new Vector3(0, 2, -4)
    };

    // Start is called before the first frame update
    void Start()
    {
        arenaCamera.enabled = !IsClient;
        arenaCamera.GetComponent<AudioListener>().enabled = !IsClient;

        networkedPlayers = GameObject.Find("NetworkedPlayers").GetComponent<networkedPlayers>();
        NetworkHelper.Log($"Players = {networkedPlayers.allNetPlayers.Count}");

        if (IsServer) {
            SpawnPlayers();
        }
    }

    private Vector3 NextPosition() {
        Vector3 pos = startPositions[positionIndex];
        positionIndex += 1;
        if (positionIndex > startPositions.Length - 1) {
            positionIndex = 0;
        }
        return pos;
    }

    private void SpawnPlayers() { 
        foreach(NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            Player prefab = playerPrefab;
            //if(clientId == NetworkManager.LocalClientId) {
              //  prefab = hostPrefab;
            //}
            Player playerSpawn = Instantiate(prefab, NextPosition(), Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            playerSpawn.playerColorNetVar.Value = info.color;
        }
    }

}
