using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Reflection;

public class NetworkDataManager : NetworkBehaviour
{
    public NetworkManager manager;

    #region Monobehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        if (manager == null)
        {
            manager = NetworkManager.singleton;
            if (manager == null)
            {
                Debug.LogError("NetworkManager instance not found!", this);
            }
        }
    }
    #endregion

    #region RPCs
    [ClientRpc]
    private void RpcRequestLoadData(NetworkSerialFile toLoad, Vector3 position, Vector3 eulerAngles)
    {
        if (manager.mode != NetworkManagerMode.Host)
        {
            Debug.Log("Received load data request from host. Attempting to load data...", this);
            GameObject.Find("DataLoader").GetComponent<DataLoader>().CreateDataObject(FromNetworkSerialFile(toLoad), position, eulerAngles);
        }
    }

    [ClientRpc]
    private void RpcRequestUpdateData(string instanceName, string methodName, string argument)
    {
        if (manager.mode != NetworkManagerMode.Host)
        {
            DataObject d = GameObject.Find(instanceName).GetComponent<DataObject>();
            MethodInfo method = d.GetType().GetMethod(methodName);

            // Methods with string arguments
            if (methodName.Equals("SwapZ") || methodName.Equals("SwapColor"))
            {
                object[] arg = { argument };
                method.Invoke(d, arg);
            }

            // Float arguments
            else if (methodName.Equals("SetFrame") || methodName.Equals("SetColorMin") ||
                methodName.Equals("SetColorMax") || methodName.Equals("SetOverlayZ"))
            {
                object[] arg = { float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture) };
                method.Invoke(d, arg);
            }

            // Int arguments
            else if (methodName.Equals("SetInterpMethod") || methodName.Equals("SetColorbar"))
            {
                object[] arg = { int.Parse(argument) };
                method.Invoke(d, arg);
            }

            // Bool arguments
            else if (methodName.Equals("SetColorModeBool") || methodName.Equals("SetInvert") ||
                methodName.Equals("SetDataClip"))
            {
                object[] arg = { Convert.ToBoolean(argument) };
                method.Invoke(d, arg);
            }

            // Vector2 arguments
            else if (methodName.Equals("ManualSwapUV"))
            {
                object[] arg = { JsonUtility.FromJson<Vector2[]>(argument) };
                method.Invoke(d, arg);
            }

            // ColorMode arguments
            else if (methodName.Equals("SetColorMode"))
            {
                object[] arg = { (DataObject.ColorMode)int.Parse(argument) };
                method.Invoke(d, arg);
            }

            // No arguments (called from TimeSeriesController)
            else if (methodName.Equals("StepForward") || methodName.Equals("StepBackward"))
            {
                TimeSeriesController t = d.gameObject.transform.Find("TimeController").gameObject.GetComponent<TimeSeriesController>();
                method = t.GetType().GetMethod(methodName);
                object[] arg = { };
                method.Invoke(t, arg);
            }

            // Unrecognized method name
            else
            {
                Debug.LogError("Unrecognized method name.", this);
            }
        }
    }

    [ClientRpc]
    private void RpcRequestMoveData(string instanceName, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        if (manager.mode != NetworkManagerMode.Host)
        {
            Debug.Log("Received request to move data.", this);
            GameObject obj = GameObject.Find(instanceName);
            obj.transform.position = position;
            obj.transform.rotation = Quaternion.Euler(rotation);
            obj.transform.localScale = scale;
        }
        //else
        //    Debug.Log("Called ClientRpc from host.", this);
    }
    #endregion

    // If data was edited on the server, asks the clients to make the same changes to corresponding instance
    public void UpdateData(string instanceName, string methodName, string argument)
    {
        if (!manager.isNetworkActive)
        {
            Debug.Log("Not connected to network.", this);
            return;
        }

        // Only send RPC if the host made changes, otherwise ignore
        if (manager.mode == NetworkManagerMode.Host)
        {
            // Tell clients to make the change
            RpcRequestUpdateData(instanceName, methodName, argument);
        }
    }

    // If data was loaded on the server, spawns a copy of the data for all clients
    public void SyncData(SerialFile file, Vector3 position, Vector3 eulerAngles)
    {
        if (!manager.isNetworkActive)
        {
            Debug.Log("Not connected to network.", this);
            return;
        }

        // Only send RPC if the host is loading, otherwise ignore
        if (manager.mode == NetworkManagerMode.Host)
        {
            // Tell clients to load the same dataset
            RpcRequestLoadData(ToNetworkSerialFile(file), position, eulerAngles);
        }
    }

    // If the host moves the position of the data on the server, moves the position for the clients
    public void MoveData(GameObject data)
    {
        if (!manager.isNetworkActive)
        {
            Debug.Log("Not connected to network.", this);
            return;
        }

        // Only send RPC if the host is loading, otherwise ignore
        if (manager.mode == NetworkManagerMode.Host)
        {
            RpcRequestMoveData(data.name, data.transform.position, data.transform.rotation.eulerAngles, data.transform.localScale);
        }
    }

    #region File Conversion
    public static SerialFile FromNetworkSerialFile(NetworkSerialFile networkFile)
    {
        SerialFile sf = new SerialFile();

        sf.path = networkFile.path;
        sf.lastModified = Convert.ToDateTime(networkFile.lastModified);
        sf.identifier = networkFile.identifier;
        sf.vertexCount = networkFile.vertexCount;
        sf.triangleCount = networkFile.triangleCount;
        sf.runtimeName = networkFile.runtimeName;
        sf.fileName = networkFile.fileName;
        sf.notes = networkFile.notes;
        sf.description = networkFile.description;
        sf.instrument = networkFile.instrument;
        sf.time = networkFile.time;
        sf.type = (SerialData.DataType)networkFile.type;

        sf.min = networkFile.min;
        sf.max = networkFile.max;
        sf.variable = networkFile.variable;
        sf.x_label = networkFile.x_label;
        sf.y_label = networkFile.y_label;
        sf.z_label = networkFile.z_label;
        sf.val_label = networkFile.val_label;

        return sf;
    }

    public static NetworkSerialFile ToNetworkSerialFile(SerialFile file)
    {
        NetworkSerialFile nf = new NetworkSerialFile();

        nf.path = file.path;
        nf.lastModified = file.lastModified.ToString("s");
        nf.identifier = file.identifier;
        nf.vertexCount = file.vertexCount;
        nf.triangleCount = file.triangleCount;
        nf.runtimeName = file.runtimeName;
        nf.fileName = file.fileName;
        nf.notes = file.notes;
        nf.description = file.description;
        nf.instrument = file.instrument;
        nf.time = file.time;
        nf.type = (int)file.type;

        nf.min = file.min;
        nf.max = file.max;
        nf.variable = file.variable;
        nf.x_label = file.x_label;
        nf.y_label = file.y_label;
        nf.z_label = file.z_label;
        nf.val_label = file.val_label;

        return nf;
    }
    #endregion
}
