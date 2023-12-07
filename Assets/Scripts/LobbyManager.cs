using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    public Button startButton;
    public TMPro.TMP_Text statusLabel;

    void Start()
    {
        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
        startButton.onClick.AddListener(OnStartButtonClicked);

        startButton.gameObject.SetActive(false);
        statusLabel.text = "Start Client/Host/Server";
    }

    private void OnServerStarted() {
        //StartGame();
        //startButton.gameObject.SetActive(true);
        //statusLabel.text = "Press Start";
        GotoLobby();
    }

    private void OnClientStarted() {
        if (!IsHost) {
            statusLabel.text = "Waiting for game to start";
        }
    }

    private void OnStartButtonClicked()
    {
        StartGame();
    }

    public void GotoLobby() {
        NetworkManager.SceneManager.LoadScene(
            "Lobby",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void StartGame()
    {
        NetworkManager.SceneManager.LoadScene(
            "FinalGame",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

}
