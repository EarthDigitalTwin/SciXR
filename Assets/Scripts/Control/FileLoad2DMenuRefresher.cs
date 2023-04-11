using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileLoad2DMenuRefresher : MonoBehaviour
{
    private void OnEnable()
    {
        transform.parent.gameObject.GetComponent<FileLoad2DMenu>().Refresh();
    }
}
