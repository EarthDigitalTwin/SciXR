using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkUIManager : MonoBehaviour
{
    public NetworkManager manager;

    public GameObject startContainer;
    public GameObject serverStart;
    public GameObject serverUnavailable;
    public TextMeshPro addressText;

    public GameObject connectingContainer;
    public TextMeshPro connectingAddress;

    public GameObject statusContainer;
    public TextMeshPro statusText;

    public GameObject clientReadyButton;
    public GameObject stopButton;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            serverStart.SetActive(false);
            serverUnavailable.SetActive(true);
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        statusText.text = "";
        startContainer.SetActive(false);
        connectingContainer.SetActive(false);
        statusContainer.SetActive(false);
        clientReadyButton.SetActive(false);
        stopButton.SetActive(false);

        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (!NetworkClient.active)
            {
                startContainer.SetActive(true);
                addressText.text = manager.networkAddress;
            }
            else
            {
                connectingContainer.SetActive(true);
                connectingAddress.text = manager.networkAddress;
            }
        }
        else
        {
            if (NetworkServer.active)
            {
                statusContainer.SetActive(true);
                statusText.text += "Server: active. Transport: " + Transport.activeTransport + "\n";
            }
            if (NetworkClient.active)
            {
                statusContainer.SetActive(true);
                statusText.text += "Client: address = " + manager.networkAddress;
            }
        }

        if (NetworkClient.isConnected && !ClientScene.ready)
        {
            clientReadyButton.SetActive(true);
        }

        if (NetworkServer.active || NetworkClient.active)
        {
            stopButton.SetActive(true);
        }
    }

    public void Update()
    {
        if (connectingContainer.activeSelf)
        {
            // Note: might need to change this if it prevents cancel button from being clickable
            UpdateUI();
        }
    }

    public void OnClientReady()
    {
        ClientScene.Ready(NetworkClient.connection);

        if (ClientScene.localPlayer == null)
        {
            ClientScene.AddPlayer();
        }
    }

    public void OnStop()
    {
        manager.StopHost();
    }

    public void ChangeAddress(KeyboardInputField addressField)
    {
        manager.networkAddress = addressField.input;
        UpdateUI();
    }
}
