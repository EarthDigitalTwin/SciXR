using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientObject : MonoBehaviour
{
    public static string[] GetFileNames()
    {
        MyService service = new MyService();
        return service.GetFileNames();
    }

    public static string[] GetFile(string path)
    {
        Debug.Log("Requesting file " + path + " from server.");
        MyService service = new MyService();
        return service.GetFile(path);
    }
}
