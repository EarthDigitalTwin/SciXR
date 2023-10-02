using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class AvatarDesktop : NetworkBehaviour {
    public override void OnStartAuthority() {
        //Debug.Log("Creating Local DesktopAvatar");
        gameObject.name = "Desktop Avatar (LocalPlayer)";
        GetComponentInChildren<TransformAttach>().attachedTransform = DesktopInterface.instance.screenCamera.transform;
        GetComponentInChildren<TransformAttach>().enabled = true;

    }

    void Start() {
            //Debug.Log("Creating Remote DesktopAvatar");
            gameObject.name = "Desktop Avatar (RemotePlayer)";
    }

    //void OnDestroy() {
    //    GetComponentInChildren<TransformAttach>().enabled = false;
    //}

}
