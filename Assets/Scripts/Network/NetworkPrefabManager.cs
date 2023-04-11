using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPrefabManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] spawnOnConnected;

    private void Update()
    {
        // Nothing needs to be spawned
        if (spawnOnConnected == null || spawnOnConnected.Length == 0)
            enabled = false;

        if (NetworkManager.singleton.isNetworkActive && NetworkManager.singleton.mode == NetworkManagerMode.Host)
        {
            foreach (GameObject prefab in spawnOnConnected)
            {
                GameObject obj = Instantiate(prefab);
                NetworkServer.Spawn(obj);
            }
            enabled = false;
        }
    }
}
