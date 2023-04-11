using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController : NetworkBehaviour
{
    public GameObject headset;
    public TMPro.TMP_InputField nameInput;
    public TextMesh nametag;
    private NetworkIdentity nID;
    [SyncVar]
    public string playerName;

    [Command]
    public void CmdSendNameToServer(string name)
    {
        playerName = name;
        nametag.text = name;
        RpcSetName(name);
    }

    [ClientRpc]
    public void RpcSetName(string name)
    {
        playerName = name;
        nametag.text = name;
    }

    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        nID = GetComponent<NetworkIdentity>();

        if (nID.isLocalPlayer)
        {
            nameInput = GameObject.Find("UsernameInput").GetComponent<TMPro.TMP_InputField>();
            nametag.text = nameInput.text;
            nameInput.gameObject.SetActive(false);

            if (nID.isClient)
            {
                CmdSendNameToServer(nameInput.text);
            }
            else
            {
                RpcSetName(nameInput.text);
            }

            headset = GameObject.FindGameObjectWithTag("VRHeadset");
            if (headset == null)
            {
                // VR is not enabled
                headset = GameObject.FindGameObjectWithTag("MainScreenCamera");
            }

            // Don't display the avatar for the local player or it will cover their view
            transform.Find("Head").gameObject.SetActive(false);
        }
        else
        {
            nametag.text = playerName;
        }
    }

    private void Update()
    {
        if (nID.isLocalPlayer && headset != null)
        {
            transform.position = headset.transform.position;
            transform.rotation = headset.transform.rotation;
        }
    }

    private void OnDestroy()
    {
        // Allow the user to reenter their username
        if (nameInput != null)
            nameInput.gameObject.SetActive(true);
    }
    #endregion
}
