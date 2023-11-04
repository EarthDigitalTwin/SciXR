using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
//Aliases for TMPro
using Text = TMPro.TMP_Text;
//using Reader = ModelJSReader;

public class DataLoader : MonoBehaviour {
    [Serializable]
    public class DataLoadEvent : UnityEvent<GameObject> { };

    // Singleton
    public static DataLoader instance;

    // Properties
    public string dataPath = "Data";
    public string colorbarPath = "Data/Colorbars/colorbars.js";
    public string dataPreviewSubPath = "DataPreviews";
    public string dataPreviewPath { get { return Path.Combine(Application.streamingAssetsPath, dataPreviewSubPath); } }
    public GameObject screenFileLoadContainer;
    public GameObject vrFileLoadContainer;
    public GameObject buttonPrefab;
    public GameObject dataPrefab;


    public Material defaultMaterial;
    public Material pointCloudMaterial;
	public Material smallPointCloudMaterial;
    public Material defaultMaterialUI;
    public List<Texture2D> overlayTextures;

    public DataLoadEvent OnDataLoad;

    [HideInInspector] public List<SerialFile> dataFiles;
    [HideInInspector] public List<Colorbar> presetColorbars;
    [HideInInspector] public List<GameObject> loadedData = new List<GameObject>();
    public List<String> SDAPLinks;

    // Use this for initialization
    void Start() {

        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = dataPath;
        watcher.Filter = "*.js";
        watcher.NotifyFilter = NotifyFilters.LastAccess |
                         NotifyFilters.LastWrite |
                         NotifyFilters.FileName |
                         NotifyFilters.DirectoryName |
                         NotifyFilters.CreationTime |
                         NotifyFilters.Size |
                         NotifyFilters.Attributes ;

        watcher.IncludeSubdirectories = true;
        watcher.Changed += OnDirectoryChange;
        watcher.Created += OnDirectoryChange;
        watcher.EnableRaisingEvents = true;

        instance = this;
        LoadColormaps();
        LoadFiles();
    }

    private void OnDirectoryChange(object sender, FileSystemEventArgs e) {
        Debug.Log("Data Directory change detected!");
        ThreadManager.instance.callbacks.Add(() => LoadFiles());
    }

    // Callbacks
    void LoadColormaps() {
        ColorbarReader colorbarReader = new ColorbarReader();
        presetColorbars = colorbarReader.ReadColorbarsFromPath(colorbarPath);
    }

    void LoadFiles() {
        //DataSet ds = DataSet.Open("filepath.nc?openMode=create");

        dataFiles = new List<SerialFile>();
        string[] files = Directory.GetFiles(dataPath);

        foreach (string fileFull in files) {
            if (fileFull.Contains(".js")) {
                SerialFile newDataFile = ModelJSReader.MetadataFromPath(fileFull);
                dataFiles.Add(newDataFile);
            }
            else if (fileFull.Contains(".ply")) {
                SerialFile newDataFile = PLYReader.MetadataFromPath(fileFull);
                dataFiles.Add(newDataFile);
            } 
            else if (fileFull.Contains(".mat")) {
                SerialFile newDataFile = MatLabReader.MetadataFromPath(fileFull);
                dataFiles.Add(newDataFile);
            }
            else if (fileFull.Contains(".sdap"))
            {
                SerialFile newDataFile = PLYReader.MetadataFromPath(fileFull);
                dataFiles.Add(newDataFile);
            }
        }
        if (screenFileLoadContainer.activeSelf) {
            //screenFileLoadContainer.GetComponent<FileLoadMenu>()?.Refresh();
            screenFileLoadContainer.GetComponent<FileLoad2DMenu>()?.Refresh("");
        }
        else if (vrFileLoadContainer.activeSelf) {
            //vrFileLoadContainer.GetComponent<FileLoadMenu>()?.Refresh();
            vrFileLoadContainer.GetComponent<FileLoad2DMenu>()?.Refresh("");
        }
        else {
            //screenFileLoadContainer.SetActive(true);
            //vrFileLoadContainer.SetActive(true);
        }
    }

    void LoadSDAP() {
        SDAPLinks = new List<String>();
        // hardcode links for demo
        SDAPLinks.Add("GLDAS_VIC10_3H_2_1_global_Qs_acc");
        SDAPLinks.Add("GLDAS_VIC10_3H_2_1_global_Qsb_acc");
        SDAPLinks.Add("GPM-3IMERGHHR-06-daily-global-precipitationCal_transposed");
        SDAPLinks.Add("LIS-ESoil-tavg");
        SDAPLinks.Add("LIS_gar_1x_Qs_tavg");
        SDAPLinks.Add("LIS_gar_1x_Qsb_tavg");
        SDAPLinks.Add("LIS_miss_1x_Qs_tavg");
        SDAPLinks.Add("LIS_miss_1x_Qsb_tavg");
    }

    public void CreateDataObject(SerialFile dataFile, Vector3 position, Vector3 eulerAngles) {
        //Instantiate data object with metadata from file load, then fire multithreaded load
        GameObject newDataObj = Instantiate(dataPrefab);
        newDataObj.SetActive(true);

        DataObject dataObject = newDataObj.GetComponent<DataObject>();
        dataObject.transform.position = position;
        dataObject.transform.eulerAngles = eulerAngles;
        //dataObject.transform.localEulerAngles = new Vector3(dataObject.transform.localEulerAngles.x, 0, dataObject.transform.localEulerAngles.z);
        if (dataFile.type == SerialData.DataType.pointcloud) {
			if (dataFile.vertexCount < 500000 && SystemInfo.graphicsShaderLevel >= 50) {
				dataObject.material = Material.Instantiate (smallPointCloudMaterial);
			} else {
				dataObject.material = Material.Instantiate (pointCloudMaterial);
			}
        } else {
            dataObject.material = Material.Instantiate(defaultMaterial);
        }
        dataObject.material.SetTexture("_MainTex", presetColorbars[0].texture);
        dataObject.materialUI = Material.Instantiate(defaultMaterialUI);
        dataObject.materialUI.SetTexture("_MainTex", presetColorbars[0].texture);
        dataObject.materialOverlay = Material.Instantiate(defaultMaterial);

        dataObject.currentColorbar = presetColorbars[0];
        dataObject.identifier = dataFile.identifier;
        dataObject.vertexCount = dataFile.vertexCount;
        dataObject.triangleCount = dataFile.triangleCount;
        dataObject.notes = dataFile.notes;

        dataObject.fileName = dataFile.fileName;
        dataObject.filePath = dataFile.path;
        dataObject.lastModified = dataFile.lastModified;
        dataObject.runtimeName = dataFile.runtimeName;
        dataObject.type = dataFile.type;

        dataObject.gameObject.name = dataObject.identifier + "_instance";

        loadedData.Add(dataObject.gameObject);

        //MenuHandVRControls vrControls = FindObjectOfType<MenuHandVRControls>();
        //if (vrControls!= null && VRTK.VRTK_SDKManager.instance.loadedSetup != null) {
        //    vrControls.LoadMenuCloseClick();
        //    vrControls.CloseClick();
        //}
        DesktopInterface.instance.RefreshLoadedMeshesMenu();
        DesktopInterface.instance.SetTarget(dataObject.GetComponentInChildren<MeshVRControls>().transform);

        // Fire off data reading task
        Task dataRead = Task.Run(() => { LoadDataAsync(dataFile, dataObject); });

        // Make it synchronouse instead!
        //LoadDataAsync(dataFile, dataObject);
    }

    // Populate serialData and serialMesh from fileToLoad, then send back to main thread for processing
    void LoadDataAsync(SerialFile fileToLoad, DataObject dataObject) {
        Stopwatch watch = new Stopwatch();
        Debug.Log(string.Format("Task={0}, Thread={1}", Task.CurrentId, Thread.CurrentThread.ManagedThreadId));
        Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;

        // Populate serialData from data file and assign it (and a few other fields) to dataObject
        watch.Start();
        SerialData serialData = null;
        if (fileToLoad.fileName.EndsWith(".js")) {
            serialData = ModelJSReader.ReadModelFromPath(fileToLoad.path, fileToLoad, dataObject, (fileToLoad.hasResults) ? 0.9f : 1);
        }
        else if (fileToLoad.fileName.EndsWith(".ply")) {
            serialData = PLYReader.ReadModelFromPath(fileToLoad.path, fileToLoad, dataObject, (fileToLoad.hasResults) ? 0.9f : 1);
        }
        else if (fileToLoad.fileName.EndsWith(".sdap"))
        {
            serialData = PLYReader.ReadModelFromPath(fileToLoad.path, fileToLoad, dataObject, (fileToLoad.hasResults) ? 0.9f : 1);
        }
        dataObject.data = serialData;
        dataObject.currentExtrude = serialData.zIndexDefault;
        dataObject.currentColor = serialData.colorIndexDefault;
        watch.Stop();
        Debug.Log("Reader data generation finished in " + watch.Elapsed);

        // Populate serialMesh from serialData created above
        watch.Reset();
        watch.Start();
        SerialMesh serialMesh = dataObject.GenerateMeshData(serialData);
        watch.Stop();
        Debug.Log("Mesh from data generation finished in " + watch.Elapsed);

        //Process results of two big process above
        dataObject.type = serialData.type;
        if (serialData.type == SerialData.DataType.globe) {
            dataObject.triColorMode = true;
        }

        // Send back to main thread
        ThreadManager.instance.callbacks.Add(() => LoadDataCallback(dataObject, serialMesh));
    }

    //Process data results and assign to dataObject (generate the Unity Mesh and preview)
    void LoadDataCallback(DataObject dataObject, SerialMesh serialMesh) {
        Stopwatch watch = new Stopwatch();

        if (dataObject.data.projection == null) {
            dataObject.data.projection = "0";
        }
        if (dataObject.identifier == "Columbia07") // for testing
        {
            dataObject.data.projection = "32406";
        }

		if (dataObject.data.time == null) { // default to yesterday
			DateTime defaultTime = DateTime.Today.AddDays (-1);
			dataObject.data.time = defaultTime.ToString ("yyyy-MM-dd");
		}

        float[] xVals = dataObject.data.vars[dataObject.data.xIndexDefault];
        float[] yVals = dataObject.data.vars[dataObject.data.yIndexDefault];

		if (dataObject.data.projection == "0") { // assume EPSG:4326 if within lat/lon range
			Debug.Log (xVals.Max());
			Debug.Log (xVals.Min());
			Debug.Log (yVals.Max());
			Debug.Log (yVals.Min());
			if (xVals.Min() > 180 && xVals.Max() > 180) { // Assume min X starts at 0
				for (int i = 0; i < xVals.Count(); i++) {
					xVals [i] = xVals [i] - 360;
				}
			}
			if (xVals.Min() >= -180f && xVals.Max() <= 180f && yVals.Min() >= -90f && yVals.Max () <= 90f) {
				Debug.Log ("Assuming EPSG:4326");
				dataObject.data.projection = "4326";
			}
		}

        OverlayLoader.BBox bbox = OverlayLoader.GetObjectBounds(xVals, yVals, dataObject.data.projection);

        foreach (Texture2D tex in overlayTextures) {
            if (dataObject.identifier.ToLower().Contains(tex.name)) {
                dataObject.materialOverlay.SetTexture("_MainTex", tex);
            }
        }

        if (bbox != null)
        {
            if (dataObject.identifier == "Columbia07")
            {
                // string targetProjection = "4326";
				// StartCoroutine(OverlayLoader.LoadImageryFromBounds(dataObject.materialOverlay, "ASTER_GDEM_Color_Shaded_Relief", targetProjection, bbox, dataObject.data.time));
            }
            if (dataObject.identifier.Contains("Argo") == true)
            {
                //float scale = (float) (bbox.maxY / bbox.maxX);
                //var localScale = dataObject.dataOverlay.transform.localScale;
                //localScale.x = dataObject.dataOverlay.transform.localScale.x * scale;
                //localScale.y = dataObject.dataOverlay.transform.localScale.y * scale;
                //dataObject.dataOverlay.transform.localScale = localScale;
                if (dataObject.identifier.Contains("2015-08-16") == true) {
                    Texture2D texture;
                    Debug.Log("Loading texture 2015-08-16.png");
                    texture = (Texture2D)Resources.Load("2015-08-16.png");
                    dataObject.materialOverlay.SetTexture("_MainTex", texture);
                } 
                else {
                    StartCoroutine(OverlayLoader.LoadImageryFromBounds(dataObject.materialOverlay, "BlueMarble_ShadedRelief_Bathymetry", dataObject.data.projection, bbox, dataObject.data.time));
                }
            }
            else if (dataObject.data.projection == "4326") {
                
                // StartCoroutine(OverlayLoader.LoadImageryFromBounds(dataObject.materialOverlay, "HLS_L30_Nadir_BRDF_Adjusted_Reflectance", dataObject.data.projection, bbox, dataObject.data.time));
                StartCoroutine(OverlayLoader.LoadImageryFromBounds(dataObject.materialOverlay, "VIIRS_SNPP_CorrectedReflectance_TrueColor", dataObject.data.projection, bbox, dataObject.data.time));
			}
            if (dataObject.data.type == SerialData.DataType.pointcloud) {
                dataObject.materialOverlay.shader = Shader.Find("UI/Default");
            }
        }

        watch.Start();
        dataObject.GenerateView(serialMesh);
        watch.Stop();
        Debug.Log("Generate View finished in " + watch.Elapsed);

        //Generate Preview
        //string filePath = Path.Combine(dataPreviewPath, dataObject.identifier + ".json");
        //if (!File.Exists(filePath)) {
        //    SerialMesh previewSerialMesh = new SerialMesh();
        //    previewSerialMesh.vertices = serialMesh.vertices;
        //    previewSerialMesh.triangles = serialMesh.triangles;
        //    previewSerialMesh.uv = serialMesh.uv;
        //    previewSerialMesh.name = serialMesh.name;
        //    File.WriteAllText(filePath, JsonUtility.ToJson(previewSerialMesh));
        //}

        // Fire events that initialization for DataObject complete
        dataObject.InitComplete();
        OnDataLoad.Invoke(dataObject.gameObject);
    }


    public List<Material> GetMaterials() {
        List<Material> newMats = new List<Material>();
        foreach (GameObject dataObj in loadedData) {
            newMats.Add(dataObj.GetComponent<DataObject>().material);
        }
        return newMats;
    }

    // Public callback methods

    public void RemoveMesh(GameObject dataMesh) {
        loadedData.Remove(dataMesh);
        DesktopInterface controls = DesktopInterface.instance;
        if (controls.target == dataMesh) {
            controls.target = null;

        }
        Destroy(dataMesh);
        controls.RefreshLoadedMeshesMenu();
    }
    

}
