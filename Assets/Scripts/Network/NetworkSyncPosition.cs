using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSyncPosition : MonoBehaviour
{
    NetworkDataManager manager;

    public float syncRate;
    public bool clientCanMoveData = false;
    public bool clientCanInteract = true;

    public GameObject grabPoint;

    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            manager = GameObject.Find("DataManager(Clone)").GetComponent<NetworkDataManager>();
        }
        catch (Exception)
        {
            enabled = false;
        }

        if (manager == null || manager.manager == null)
            enabled = false;
        else
        {
            // Don't allow clients to move networked datasets
            if (manager.manager.mode != Mirror.NetworkManagerMode.Host && !clientCanMoveData)
            {
                grabPoint.SetActive(false);
            }

            StartCoroutine("Sync");
        }
    }
    #endregion

    private IEnumerator Sync()
    {
        while (true)
        {
            manager.MoveData(this.gameObject);
            // Debug.Log("Sending position update...", this);
            yield return new WaitForSeconds(syncRate);
        }
    }
}
