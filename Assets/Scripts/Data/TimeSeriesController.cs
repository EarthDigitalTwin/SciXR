using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TimeSeriesController : MonoBehaviour
{
    [SerializeField]
    private List<SerialMesh> loadedMeshes = new List<SerialMesh>();
    private int activeIdx = 0;
    private List<SerialFile> dataFiles = new List<SerialFile>();
    //[SerializeField]
    //private int loadBuffer = 0;

    private DataObject obj;

    [SerializeField]
    private TextMeshProUGUI dataNameText;

    #region MonoBehavior Callbacks
    // Start is called before the first frame update
    void Start()
    {
        obj = transform.parent.gameObject.GetComponent<DataObject>();
        obj.timeButton.SetActive(true);

        // Load the SerialFiles for each time step
        // NOTE: Might need to load serialfiles as-needed like the meshes if this is too slow
        string[] fileNames = Directory.GetFiles(obj.filePath);
        foreach (string fileFull in fileNames)
        {
            int bs_idx = fileFull.LastIndexOf("\\");
            int fs_idx = fileFull.LastIndexOf("/");
            string filename;
            if (bs_idx > 0 || fs_idx > 0)
                filename = fileFull.Substring(Math.Max(bs_idx, fs_idx) + 1);
            else
                filename = fileFull;

            if (fileFull.Contains(".js"))
            {
                SerialFile newDataFile = ModelJSReader.MetadataFromPath(fileFull);
                newDataFile.path = fileFull;
                newDataFile.fileName = filename;
                newDataFile.identifier = newDataFile.fileName;
                dataFiles.Add(newDataFile);
            }
            else if (fileFull.Contains(".ply"))
            {
                SerialFile newDataFile = PLYReader.MetadataFromPath(fileFull);
                newDataFile.path = fileFull;
                newDataFile.fileName = filename;
                newDataFile.identifier = newDataFile.fileName;
                dataFiles.Add(newDataFile);
            }
            else if (fileFull.Contains(".mat"))
            {
                SerialFile newDataFile = MatLabReader.MetadataFromPath(fileFull);
                newDataFile.path = fileFull;
                newDataFile.fileName = filename;
                newDataFile.identifier = newDataFile.fileName;
                dataFiles.Add(newDataFile);
            }
        }

        Debug.Log("TimeSeriesController is finishing DataObject creation...", this);
        loadedMeshes.Add(null);
        FinishCreateDataObject(dataFiles[0]);

        // StartCoroutine("LoadFiles");

        /*
        // Load loadBuffer more meshes and keep them as inactive
        Debug.Log("TimeSeriesController has " + dataFiles.Count + " meshes. Loading meshes within buffer.", this);
        for (int i = 1; i < loadBuffer + 1; i++)
        {
            if (i < dataFiles.Count())
            {
                Debug.Log("Loading " + dataFiles[i].fileName);
                SerialMesh s_mesh = LoadMeshAsync(dataFiles[i], i);
                loadedMeshes.Add(null);
                loadedMeshes[i] = s_mesh;
            }
            else
                break;
        }
        */
    }
    #endregion

    #region Data Loading
    // Finish creating the DataObject
    void FinishCreateDataObject(SerialFile dataFile)
    {
        if (dataFile.type == SerialData.DataType.pointcloud)
        {
            if (dataFile.vertexCount < 500000 && SystemInfo.graphicsShaderLevel >= 50)
            {
                obj.material = Material.Instantiate(DataLoader.instance.smallPointCloudMaterial);
            }
            else
            {
                obj.material = Material.Instantiate(DataLoader.instance.pointCloudMaterial);
            }
        }
        else
        {
            obj.material = Material.Instantiate(DataLoader.instance.defaultMaterial);
        }
        obj.material.SetTexture("_MainTex", DataLoader.instance.presetColorbars[0].texture);
        obj.materialUI = Material.Instantiate(DataLoader.instance.defaultMaterialUI);
        obj.materialUI.SetTexture("_MainTex", DataLoader.instance.presetColorbars[0].texture);
        obj.materialOverlay = Material.Instantiate(DataLoader.instance.defaultMaterial);

        obj.currentColorbar = DataLoader.instance.presetColorbars[0];

        obj.type = dataFile.type;

        obj.vertexCount = dataFile.vertexCount;
        obj.triangleCount = dataFile.triangleCount;
        obj.notes = dataFile.notes;

        obj.runtimeName = dataFile.runtimeName;

        DataLoader.instance.loadedData.Add(obj.gameObject);

        //DesktopInterface.instance.RefreshLoadedMeshesMenu();
        //DesktopInterface.instance.SetTarget(obj.GetComponentInChildren<MeshVRControls>().transform);

        // Load the initial file as normal
        Debug.Log("Loading initial file...");

        Task initRead = Task.Run(() => {
            DataLoader.instance.LoadDataAsync(dataFile, obj);
            Debug.Log("Loaded initial mesh. Adding to the list of loaded meshes...");
            loadedMeshes[0] = LoadMeshAsync(dataFile, 0);
            dataNameText.text = dataFile.fileName;
        });

        /*
        // Load other meshes within the buffer size if needed
        for (int i = activeIdx + 1; i < activeIdx + loadBuffer + 1; i++)
        {
            if (loadedMeshes.Count <= i)
            {
                if (i < dataFiles.Count())
                {
                    Task backgroundRead = Task.Run(() =>
                    {
                        SerialMesh newMesh = LoadMeshAsync(dataFiles[i], i);
                        loadedMeshes.Add(null);
                        loadedMeshes[i] = newMesh;
                    });
                }
                else
                    break;
            }
        }
        */
    }

    SerialMesh MeshFromData(SerialData serialData, int dataIdx)
    {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        SerialMesh newSerialMesh = new SerialMesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector2> uv2 = new List<Vector2>();
        List<int> triangles = serialData.triangles;

        float[] x = serialData.vars[serialData.xIndexDefault];
        float[] y = serialData.vars[serialData.yIndexDefault];
        float[] z = (obj.hasExtrudeResults) ? serialData.results[0].vars[serialData.zIndexDefault] : serialData.vars[serialData.zIndexDefault];
        float[] colors = (obj.hasColorResults) ? serialData.results[0].vars[serialData.colorIndexDefault] : serialData.vars[serialData.colorIndexDefault];

        // Validate input array sizes
        if (x.Count() != y.Count() || x.Count() != z.Count() || y.Count() != z.Count())
        {
            throw new UnityException("Input arrays do not have matching lengths");
        }

        float xDelta = obj.XMax - obj.XMin;
        float yDelta = obj.YMax - obj.YMin;
        float zDelta = obj.ZMax - obj.ZMin;
        zDelta = (zDelta == 0) ? 1 : zDelta;

        float zScale = 1;
        switch (serialData.type)
        {
            case SerialData.DataType.flat:
                zScale = (obj.scaleSizeMax / 6) / zDelta;
                break;
            case SerialData.DataType.globe:
                zScale = obj.scaleSizeMax / zDelta;
                break;
            case SerialData.DataType.pointcloud:
                zScale = (obj.scaleSizeMax / 2) / zDelta;
                break;

        }
        if (serialData.overlayX != null && serialData.overlayX.Length > 0)
        {
            float overlayZMin = serialData.overlayHeight.Min();
            float overlayZMax = serialData.overlayHeight.Max();
            if (overlayZMin < obj.ZMin)
            {
                zScale = (obj.scaleSizeMax / 4) / (overlayZMax - overlayZMin);
            }
        }

        float XYScale = (xDelta >= yDelta) ? obj.scaleSizeMax / xDelta : obj.scaleSizeMax / yDelta;

        // Construct vertices and texture uv's
        for (int index = 0; index < serialData.vertexCount; index++)
        {
            //if (index % 1000 == 0 || index == vertexCount - 1) {
            //    ThreadManager.instance.callbacks.Add(() => {
            //        UpdateLoadPercent(0.8f + (float)index / vertexCount * 0.2f, "Generating Mesh");
            //    });
            //}
            Vector3 oldPoint = new Vector3(x[index], y[index], z[index]);
            Vector3 newPoint = oldPoint - obj.offset;
            newPoint.Scale(obj.scale);
            vertices.Add(newPoint);

            Vector2 newUV = new Vector2(Mathf.InverseLerp(obj.ColorMin, obj.ColorMax, colors[index]), 0);
            uv.Add(newUV);
            Vector2 newUV2 = new Vector2(Mathf.InverseLerp(obj.YMin, obj.YMax, y[index]), Mathf.InverseLerp(obj.XMin, obj.XMax, x[index]));
            uv2.Add(newUV2);
        }


        if (obj.triColorMode)
        {
            List<Vector3> triModeVertices = new List<Vector3>();
            List<Vector3> triModeNormals = new List<Vector3>();
            List<Vector2> triModeUV = new List<Vector2>();
            List<Vector2> triModeUV2 = new List<Vector2>();
            List<int> triModeTris = new List<int>(serialData.triangles);
            List<int> newDataIndex = new List<int>();

            for (int triIndex = 0; triIndex < serialData.triangles.Count; triIndex = triIndex + 3)
            {
                newDataIndex.Add(serialData.triangles[triIndex]);
                newDataIndex.Add(serialData.triangles[triIndex + 1]);
                newDataIndex.Add(serialData.triangles[triIndex + 2]);
                triModeVertices.Add(vertices[serialData.triangles[triIndex]]);
                triModeVertices.Add(vertices[serialData.triangles[triIndex + 1]]);
                triModeVertices.Add(vertices[serialData.triangles[triIndex + 2]]);
                triModeNormals.Add(vertices[serialData.triangles[triIndex]].normalized);
                triModeNormals.Add(vertices[serialData.triangles[triIndex + 1]].normalized);
                triModeNormals.Add(vertices[serialData.triangles[triIndex + 2]].normalized);
                triModeUV.Add(uv[serialData.triangles[triIndex]]);
                triModeUV.Add(uv[serialData.triangles[triIndex + 1]]);
                triModeUV.Add(uv[serialData.triangles[triIndex + 2]]);
                triModeUV2.Add(uv2[serialData.triangles[triIndex]]);
                triModeUV2.Add(uv2[serialData.triangles[triIndex + 1]]);
                triModeUV2.Add(uv2[serialData.triangles[triIndex + 2]]);
            }
            vertices = triModeVertices;
            uv = triModeUV;
            uv2 = triModeUV2;
            triangles = Enumerable.Range(0, serialData.triangles.Count).ToList();
            serialData.vertexCount = triModeVertices.Count;
            newSerialMesh.normals = triModeNormals.ToArray();
            newSerialMesh.dataIndex = newDataIndex;
        }
        else
        {
            newSerialMesh.dataIndex = Enumerable.Range(0, serialData.vertexCount).ToList();
        }

        newSerialMesh.vertices = vertices.ToArray();
        newSerialMesh.uv = uv.ToArray();
        newSerialMesh.uv2 = uv2.ToArray();
        if (triangles != null)
            newSerialMesh.triangles = triangles.ToArray();
        newSerialMesh.offset = obj.offset;
        newSerialMesh.scale = obj.scale;

        watch.Stop();
        Debug.Log("Main Mesh generation: " + watch.Elapsed);

        // Don't finish the task until previous calls have finished
        //while (dataIdx > loadedMeshes.Length) { }
        //AddLoadedMesh(dataIdx);
        
        return newSerialMesh;
    }

    SerialMesh LoadMeshAsync(SerialFile file, int dataIdx)
    {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        UnityEngine.Debug.Log(string.Format("Task={0}, Thread={1}", Task.CurrentId, Thread.CurrentThread.ManagedThreadId));
        Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;

        // Populate serialData from data file and assign it (and a few other fields) to dataObject
        watch.Start();
        SerialData serialData = null;
        // Pass in null to the dataObject argument to prevent editing the container
        if (file.fileName.EndsWith(".js"))
        {
            serialData = ModelJSReader.ReadModelFromPath(file.path, file, null, (file.hasResults) ? 0.9f : 1);
        }
        else if (file.fileName.EndsWith(".ply"))
        {
            serialData = PLYReader.ReadModelFromPath(file.path, file, null, (file.hasResults) ? 0.9f : 1);
        }
        string currentExtrude = serialData.zIndexDefault;
        string currentColor = serialData.colorIndexDefault;
        watch.Stop();
        Debug.Log("Reader data generation finished in " + watch.Elapsed);

        // Populate serialMesh from serialData created above
        watch.Reset();
        watch.Start();
        SerialMesh serialMesh = MeshFromData(serialData, dataIdx);
        watch.Stop();
        Debug.Log("Mesh from data generation finished in " + watch.Elapsed);

        return serialMesh;
    }

    void SwapView(SerialMesh serialMesh)
    {
        DataSubMesh subMesh = obj.subMeshOriginal.GetComponent<DataSubMesh>();
        // Assume a point cloud, might need to change later
        Mesh mesh = SerialMesh.MeshDataToMesh(serialMesh, false);
        mesh.SetIndices(Enumerable.Range(0, serialMesh.vertices.Length).ToArray(),
            MeshTopology.Points, 0);
        
        obj.CurrentMesh = mesh;
        subMesh.dataIndex = serialMesh.dataIndex;
        subMesh.subVertexCount = mesh.vertexCount;
        subMesh.blendUV.Add(serialMesh.uv);
        subMesh.flatUV = serialMesh.uv2.ToList();

        MeshCollider mc = subMesh.GetComponent<MeshCollider>();
        mc.sharedMesh = mesh;

        if (serialMesh.overlayVertices != null && serialMesh.overlayVertices.Length > 0)
        {
            obj.dataOverlay.GetComponent<MeshFilter>().sharedMesh = SerialMesh.MeshOverlayToMesh(serialMesh);
            obj.overlayButton.SetActive(true);
        }
    }

    private IEnumerator LoadFiles()
    {
        Debug.Log("Loading files in background...");

        // All files are loaded, shut down coroutine
        while (loadedMeshes.Count != dataFiles.Count)
        {
            int idx = loadedMeshes.Count;
            SerialMesh newMesh = LoadMeshAsync(dataFiles[idx], idx);
            loadedMeshes.Add(null);
            loadedMeshes[idx] = newMesh;

            yield return new WaitForSeconds(1f);
        }
    }
    #endregion

    #region Timestep Controls
    public void StepForward()
    {
        if (NetworkManager.singleton.isNetworkActive)
        {
            NetworkDataManager dm = GameObject.Find("DataManager(Clone)").GetComponent<NetworkDataManager>();
            dm.UpdateData(this.gameObject.transform.parent.gameObject.name, "StepForward", "");
        }

        // This is the last time step, so we can't step forward
        if (activeIdx + 1 == dataFiles.Count()) { return; }

        activeIdx++;
        // Check that the next time step has been generated
        if (loadedMeshes.Count <= activeIdx )
        {
            // This mesh hasn't been loaded
            SerialMesh thisMesh = LoadMeshAsync(dataFiles[activeIdx], activeIdx);
            loadedMeshes.Add(null);
            loadedMeshes[activeIdx] = thisMesh;
        }

        dataNameText.text = dataFiles[activeIdx].fileName;

        SwapView(loadedMeshes[activeIdx]);

        /*
        // Load other meshes within the buffer size if needed
        for (int i = activeIdx + 1; i < activeIdx + loadBuffer + 1; i++)
        {
            if (loadedMeshes.Count <= i)
            {
                if (i < dataFiles.Count())
                {
                    Task backgroundRead = Task.Run(() =>
                    {
                        SerialMesh newMesh = LoadMeshAsync(dataFiles[i], i);
                        loadedMeshes.Add(null);
                        loadedMeshes[i] = newMesh;
                    }); 
                }
                else
                    break;
            }
        }
        */
    }

    public void StepBackward()
    {
        if (NetworkManager.singleton.isNetworkActive)
        {
            NetworkDataManager dm = GameObject.Find("DataManager(Clone)").GetComponent<NetworkDataManager>();
            dm.UpdateData(this.gameObject.transform.parent.gameObject.name, "StepBackward", "");
        }

        // This is the first time step, so we can't step backward
        if (activeIdx == 0) { return; }

        activeIdx--;

        SwapView(loadedMeshes[activeIdx]);

        dataNameText.text = dataFiles[activeIdx].fileName;
    }
    #endregion
}
