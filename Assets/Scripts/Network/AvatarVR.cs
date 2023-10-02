using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class AvatarVR : NetworkBehaviour {

    public GameObject Head;
    public GameObject LeftHand;
    public GameObject RightHand;
    
    public override void OnStartAuthority() {
        Debug.Log("Creating Local VRAvatar");
        GetComponentInChildren<SteamVR_ControllerManager>().enabled = true;
        GetComponentsInChildren<SteamVR_TrackedObject>(true).ToList().ForEach(x => x.enabled = true);
        gameObject.name = "VR Avatar (LocalPlayer)";
        LeftHand.GetComponentsInChildren<Renderer>().ToList().ForEach(rend => { rend.enabled = false; });
        RightHand.GetComponentsInChildren<Renderer>().ToList().ForEach(rend => { rend.enabled = false; });
        int newLayer = LayerMask.NameToLayer("2D Only");
        ChangeLayer(Head, newLayer);

    }

    void Start() {
        gameObject.name = "VR Avatar (RemotePlayer)";
    }

    void OnDestroy() {
        GetComponentInChildren<SteamVR_ControllerManager>().enabled = false;
        GetComponentsInChildren<SteamVR_TrackedObject>(true).ToList().ForEach(x => x.enabled = false);
    }

    void ChangeLayer(GameObject obj, int layer) {
        obj.layer = layer;

        foreach (Transform c in obj.transform) {
            ChangeLayer(c.gameObject, layer);
        }
    }
}
