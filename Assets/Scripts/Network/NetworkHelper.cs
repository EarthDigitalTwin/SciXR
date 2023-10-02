using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkHelper : MonoBehaviour {

    string address;
    int port;

    NetworkManager manager;


	// Use this for initialization
	void Start () {
        manager = GetComponent<NetworkManager>();
	}
	
    public void InitServer() {
        ConnectionConfig config = new ConnectionConfig();
        //TODO: set up the connection config legit

        NetworkClient client = manager.StartHost();
        
    }

    public void InitClient() {
        manager.StartClient();

    }
}
