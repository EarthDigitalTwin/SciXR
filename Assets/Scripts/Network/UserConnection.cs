using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class UserConnection : NetworkBehaviour {

    public GameObject vrPrefab;
    public GameObject desktopPrefab;

    void Start() {
        if (isLocalPlayer) {
            Debug.Log("LocalPlayer Connected!");
            gameObject.name = "UserConnection (LocalPlayer)";
            CmdInstantiateVR();
            CmdInstantiateDesktop();
        } else {
            Debug.Log("RemotePlayer Connected!");
            gameObject.name = "UserConnection (RemotePlayer)";
        }
    }

    [Command]
    public void CmdInstantiateVR() {
        GameObject newAvatar = Instantiate(vrPrefab);
        NetworkServer.SpawnWithClientAuthority(newAvatar, connectionToClient);
    }
    [Command]
    public void CmdInstantiateDesktop() {
        GameObject newAvatar = Instantiate(desktopPrefab);
        NetworkServer.SpawnWithClientAuthority(newAvatar, connectionToClient);
    }
}
