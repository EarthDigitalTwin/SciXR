using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Dropdown = TMPro.TMP_Dropdown;
using TMPro;
using UI.Dates;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Net;

public class FileLoad2DMenu : MonoBehaviour {

    public GameObject fileObjectPrefab;
    public GameObject filesContainer;
    public FileLoad2DMenu userInput;
    public FileLoad2DMenu userInputVR;

    public int page = 0;
    public string sortBy = "Last Modified";

    CanvasGroup menuCanvas;
    //FileLoad2DMenu userInput;

    public void OnEnable() {
        Debug.Log("on enable!");
        menuCanvas = GetComponent<CanvasGroup>();
        Debug.Log("menu on enable: " + menuCanvas);
        if (menuCanvas == null)
            menuCanvas = gameObject.AddComponent<CanvasGroup>();
        //fileObjectPrefab.SetActive(false);

        if(menuCanvas.name == "FileSelection_2D"){
            Refresh("");
        }

        // if(menuCanvas.name == "FileSelection_2D_VR"){
        //     RefreshVR("");
        // }

        foreach (Transform fileObject in filesContainer.transform) {
            fileObject.gameObject.SetActive(false);
        }

        if(menuCanvas != null) {
            menuCanvas.alpha = 0;
            LeanTween.alphaCanvas(menuCanvas, 1, 0.4f);
            EnableFileObjects();
        }
    }

    public void EnableUserInputMenu(){
        menuCanvas = GetComponent<CanvasGroup>();
        Debug.Log("enable user input menu: " + menuCanvas);

        menuCanvas.alpha = 0; // fully opaque //new originally 0
        LeanTween.alphaCanvas(menuCanvas, 1, 0.4f);
        menuCanvas.gameObject.SetActive(true);
        //LeanTween.alphaCanvas(canvas, 0, 0.41f).setOnComplete(() => { gameObject.SetActive(false); }); ;
    }

    public void DisableUserMenu(){
        LeanTween.alphaCanvas(menuCanvas, 0, 0.41f).setOnComplete(() => { gameObject.SetActive(false); });
    }

    public void BeginDisable() {
        LeanTween.alphaCanvas(menuCanvas, 0, 0.41f).setOnComplete(() => { gameObject.SetActive(false); }); ;

        foreach (Transform fileObject in filesContainer.transform) {
            if (fileObject.name != "FileObjectPrefab") {
                fileObject.GetComponent<FileLoad2DObject>().BeginDisable(0.4f);
            }
        }
    }

    public void EnableFileObjects() {
        float delay = 0.1f;

        foreach (Transform fileObject in filesContainer.transform) {
            if (fileObject != fileObjectPrefab.transform) {
                
                //LeanTween.delayedCall(delay, () => {
                    fileObject.gameObject.SetActive(true);
                    fileObject.GetComponent<FileLoad2DObject>().FadeIn();
                //});
                delay += 0.1f;
            }
        }

        // foreach (Transform fileObject in filesContainerVR.transform) {
        //     if (fileObject != fileObjectPrefab.transform) {
        //         fileObject.gameObject.SetActive(true);
        //         fileObject.GetComponent<FileLoad2DObject>().FadeIn();
        //         delay += 0.1f;
        //     }
        // }
    }

    public void SDAPFiles(){
        int position = 0;
        int numSlots = 10;

        // SDAP FILES
        Debug.Log("Doing Nexus request...");
        try {
            StartCoroutine(NexusRequest.PerformRequest((List<NexusObject> NexusObjects) => {
                Debug.Log("NexusObjects:");
                Debug.Log(NexusObjects);

                for (int fileCount = 0; fileCount < NexusObjects.Count; fileCount++)
                {
                    Debug.Log(DataLoader.instance.dataFiles[fileCount].fileName);
                    NexusObject obj = NexusObjects[fileCount];
                    //if (obj.shortName.Contains(filter)) {
                    GameObject newFileObj = Instantiate(fileObjectPrefab, filesContainer.transform);
                    newFileObj.name = obj.shortName; // sets button name

                    FileLoad2DObject file = newFileObj.GetComponent<FileLoad2DObject>();
                    string name = obj.shortName;
                    file.fileName.text = name;

                    if (obj.iso_start != null && obj.iso_end != null)
                    {
                        string iso_start = obj.iso_start.Substring(0, 10);
                        string iso_end = obj.iso_end.Substring(0, 10);
                        string date = iso_start + " - " + iso_end;
                        file.secondRowInfo.text = date;
                    }
                    else
                    {
                        file.secondRowInfo.text = " ";
                    }
                    //file.RefreshMetadata();

                    newFileObj.SetActive(true);
                    position++;
                }
            }));

        } catch (WebException ex) {
            Debug.LogError("Error occurred while loading SDAP Nexus: " + ex.Message);
        }
    }

    public void UserFiles(string filter){
        int position = 0;
        int numSlots = 10;

        int endCount = DataLoader.instance.dataFiles.Count;
        for (int fileCount = page * numSlots; fileCount < endCount; fileCount++) {
            //Debug.Log(DataLoader.instance.dataFiles[fileCount].fileName);
            if (DataLoader.instance.dataFiles[fileCount].fileName.Contains(filter)) {
                //Debug.Log("Filtered" + DataLoader.instance.dataFiles[fileCount].fileName);
                Debug.Log("FC: " + filesContainer.name);
                GameObject newFileObj = Instantiate(fileObjectPrefab, filesContainer.transform);
                FileLoad2DObject file = newFileObj.GetComponent<FileLoad2DObject>(); // this line 
                Debug.Log("FILE: " + file);
                file.file = DataLoader.instance.dataFiles[fileCount];
                file.RefreshMetadata();
                newFileObj.name = DataLoader.instance.dataFiles[fileCount].runtimeName;
                newFileObj.SetActive(true);
                position++;
            }
        }
    }

    // public void RefreshVR(string filter){
    //     int position = 0;
    //     int numSlots = 10;
    //     GameObject filesContainer = GameObject.Find("ContentVR");
    //     Debug.Log("fcn" + filesContainer);

    //     foreach (Transform child in filesContainer.transform) {
    //         if (child != fileObjectPrefab.transform)
    //             Destroy(child.gameObject);
    //     }

    //     if (DataLoader.instance?.dataFiles == null)
    //         return;

    //     int filteredFiles = 0;
    //     foreach (SerialFile dataFile in DataLoader.instance.dataFiles) {
    //         if (dataFile.fileName.Contains(filter)) {
    //             filteredFiles++;
    //         }
    //     }

    //     Slider FilesSliderVR = GameObject.Find("FilesSliderVR")?.GetComponent<Slider>();
    //     Debug.Log("RefreshVR " + FilesSliderVR);
    //     if(FilesSliderVR != null){
    //         if(FilesSliderVR.value == 0){
    //             Debug.Log("VR SDAP files");

    //             // SDAP FILES
    //             List<NexusObject> NexusObjects = NexusRequest.PerformRequest();

    //             for (int fileCount = 0; fileCount < NexusObjects.Count; fileCount++) {
    //                 //Debug.Log(DataLoader.instance.dataFiles[fileCount].fileName);
    //                 NexusObject obj = NexusObjects[fileCount];
    //                 //if (obj.shortName.Contains(filter)) {
    //                 GameObject newFileObj = Instantiate(fileObjectPrefab, filesContainer.transform);
    //                 newFileObj.name = obj.shortName; // sets button name

    //                 FileLoad2DObject file = newFileObj.GetComponent<FileLoad2DObject>();
    //                 string name = obj.shortName;
    //                 file.fileName.text = name;

    //                 if(obj.iso_start != null && obj.iso_end != null){
    //                     string iso_start = obj.iso_start.Substring(0, 10);
    //                     string iso_end = obj.iso_end.Substring(0, 10);
    //                     string date = iso_start + " - " + iso_end; 
    //                     file.secondRowInfo.text = date;
    //                 }
    //                 else {
    //                     file.secondRowInfo.text = " ";
    //                 }
    //                 //file.RefreshMetadata();

    //                 newFileObj.SetActive(true);
    //                 position++;
    //             }
    //         }
    //         else{
    //             Debug.Log("VR user files");
                
    //             int endCount = DataLoader.instance.dataFiles.Count;
    //             for (int fileCount = page * numSlots; fileCount < endCount; fileCount++) {
    //                 //Debug.Log(DataLoader.instance.dataFiles[fileCount].fileName);
    //                 if (DataLoader.instance.dataFiles[fileCount].fileName.Contains(filter)) {
    //                     //Debug.Log("Filtered" + DataLoader.instance.dataFiles[fileCount].fileName);
    //                     Debug.Log("FC: " + filesContainer.name);
    //                     GameObject newFileObj = Instantiate(fileObjectPrefab, filesContainer.transform);
    //                     FileLoad2DObject file = newFileObj.GetComponent<FileLoad2DObject>(); // this line 

    //                     file.file = DataLoader.instance.dataFiles[fileCount];
    //                     file.RefreshMetadata();
    //                     newFileObj.name = DataLoader.instance.dataFiles[fileCount].runtimeName;
    //                     newFileObj.SetActive(true);
    //                     position++;
    //                 }
    //             }
    //         }
    //     }
    // }

    public void Refresh(string filter) {

        Debug.Log("File Container: " + filesContainer.name);
        foreach (Transform child in filesContainer.transform) {
            if (child != fileObjectPrefab.transform)
                Destroy(child.gameObject);
        }

        if (DataLoader.instance?.dataFiles == null)
            return;

        int filteredFiles = 0;
        Debug.Log(filter);
        foreach (SerialFile dataFile in DataLoader.instance.dataFiles) {
            Debug.Log(dataFile.fileName);
            if (dataFile.fileName.Contains(filter)) {
                Debug.Log("Filterd" + dataFile.fileName);
                filteredFiles++;
            }
        }


        Slider FilesSlider = GameObject.Find("FilesSlider")?.GetComponent<Slider>();
        if(FilesSlider != null){
        //if(filesContainer.name == "Content"){
            Debug.Log("Desktop File Container: " + filesContainer.name);
            if (FilesSlider.value == 0){ // generated SDAP files
                Debug.Log("SDAP files");
                SDAPFiles();
            }
       
            else { 
                Debug.Log("User files");
                UserFiles(filter);
            }
        } else {
            Debug.Log("No slider found, defaulting to user files");
            UserFiles(filter);
        }
    }

    public static void CallShellScript()
    {
        string scriptPath = "Assets/Scripts/Control/processing/script.sh";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = scriptPath,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        StreamReader reader = process.StandardOutput;
        string output = reader.ReadToEnd();
        process.WaitForExit();

        Debug.Log("out: " + output);
    }
    
    public void generateSDAPInstance(string identifier){
        bool autoPosition = true;
        Vector3 position = Vector3.zero;
        Vector3 eulerAngles = Vector3.zero;

        if (autoPosition) {
            float distance = 1;
            Transform cameraTransform = DesktopInterface.instance.transform;
            Vector3 headsetForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
            headsetForward.Normalize();
            position = cameraTransform.position + headsetForward * distance;
            position = new Vector3(position.x, 1, position.z);

            Vector3 directionVector = position - cameraTransform.position;
            directionVector.y = 0;
            eulerAngles = Quaternion.LookRotation(directionVector).eulerAngles;
        }
        else {
            position = transform.position + transform.forward * 0.4f;
            eulerAngles = Quaternion.LookRotation(position - transform.position).eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
        }
        
        // hide user input
        // GameObject userInputObject = GameObject.FindWithTag("SDAPInput");
        // userInputObject.SetActive(false);

        // DO: Determine which window is calling (Menu or menuVR)
        // // THEN: Disable the correct Menu
        if(userInput){
           userInput.DisableUserMenu();
        }

        // if(userInputVR)
        //     userInputVR.DisableUserMenu();

        // make data instance
        SerialFile sdapFile = null;
        try
        {   
            Debug.Log("entered try");
            sdapFile = PLYReader.MetadataFromPath("Assets/Scripts/Control/processing/" + identifier + ".ply");
            DataLoader.instance.CreateDataObject(sdapFile, position, eulerAngles);
        }
        catch (System.Exception e) // this doesnt loop properly should just reset everything 
        {
            Debug.LogError("Error occurred while loading the file: " + e.Message);
            // Additional error handling if needed
            OnEnable();
            Refresh("");
        }

        // SerialFile sdapFile = PLYReader.MetadataFromPath("Assets/Scripts/Control/processing/" + identifier + ".ply");
        // if(sdapFile == null){
        //     Refresh("");
        // }
        // else {
        //     DataLoader.instance.CreateDataObject(sdapFile, position, eulerAngles);
        // }
    }

    public void handleInput()
    {
        DatePicker_DateRange dateField = GameObject.Find("DatePicker")?.GetComponent<DatePicker_DateRange>();
        UI.Dates.SerializableDate startDate = dateField.FromDate;
        UI.Dates.SerializableDate endDate = dateField.ToDate;

        string startTimeRaw = startDate.ToString(); //.Substring(0, 9); // Convert SerializableDate to string
        string endTimeRaw = endDate.ToString(); //.Substring(0, 9); // Convert SerializableDate to string

        string[] startTimeSplit = startTimeRaw.Split('/');
        string[] endTimeSplit = endTimeRaw.Split('/');

        string startTime = startTimeSplit[2].Substring(0, 4) + "-" + startTimeSplit[0].PadLeft(2, '0') + "-" + startTimeSplit[1].PadLeft(2, '0');
        string endTime = endTimeSplit[2].Substring(0, 4) + "-" + endTimeSplit[0].PadLeft(2, '0') + "-" + endTimeSplit[1].PadLeft(2, '0');

        InputField Box = GameObject.Find("BBox")?.GetComponent<InputField>();
        // Retrieve the value from the input field
        string[] bBox = Box.text.Split(',');


        InputField variableField = GameObject.Find("Variable")?.GetComponent<InputField>();
        InputField units = GameObject.Find("Units")?.GetComponent<InputField>();
        InputField identifier = GameObject.Find("Identifier")?.GetComponent<InputField>();

        int interpolation = 1;
        string labelTime = startTime;
        string sdap_url = "https://ideas-digitaltwin.jpl.nasa.gov/nexus";

        Button datasetButton = GameObject.Find("DatasetButton").GetComponent<Button>(); // find button text ui
        TMP_Text datasetField = datasetButton.GetComponentInChildren<TMP_Text>(); // extract text

        //format bbox
        //TODO fix bounding box 
        string filePath = "Assets/Scripts/Control/processing/test_config.yaml";
        string content = "sdap_url: \"" + sdap_url 
                        + "\"\n" + "variables: \n  -\"" 
                        + variableField.text + "\"\n" 
                        + "units: \"" + units.text + "\"\n" 
                        + "dataset: \"" + datasetField.text + "\"\n" 
                        + "identifier: \"" + identifier.text + "\"\n"
                        + "description: \"" + "" + "\"\n"
                        + "instrument: " + "\"" + "\"\n" // -81, -65, 34, 50
                        + "bbox: {" + "'min_lon': " + bBox[0] + ", 'max_lon': " + bBox[1] + ", 'min_lat': " + bBox[2] + ", 'max_lat': " + bBox[3] + "}\n"
                        + "start_time: \"" + startTime + "\"\n"
                        + "end_time: \"" + endTime + "\"\n"
                        + "label_time: \"" + labelTime + "\"\n"
                        + "interpolation: " + interpolation;

        File.WriteAllText(filePath, content);

        // generate data file
        CallShellScript();
        generateSDAPInstance(identifier.text);
    }

    public void OnSortChange(int dropdownVal) {
        Dropdown dd = GetComponentInChildren<Dropdown>();
        sortBy = dd.options[dd.value].text;
        switch(sortBy) {
            case "Last Modified":
                SortFilesByLastModified();
                break;
            case "Identifier":
                SortFilesByIdentifier();
                break;
            case "File Name":
                SortFilesByFileName();
                break;
            case "SDAP":
                SortFilesBySDAP();
                break;
        }
    }
    public void SortFilesByIdentifier() {
        DataLoader.instance.dataFiles = DataLoader.instance.dataFiles.OrderBy(o => o.identifier).ToList();
        Refresh("");
    }

    public void SortFilesByFileName() {
        DataLoader.instance.dataFiles = DataLoader.instance.dataFiles.OrderBy(o => o.fileName).ToList();
        Refresh("");
    }
    
    public void SortFilesByLastModified() {
        DataLoader.instance.dataFiles = DataLoader.instance.dataFiles.OrderByDescending(o => o.lastModified).ToList();
        Refresh("");
    }

    public void SortFilesBySDAP()
    {
        DataLoader.instance.dataFiles = DataLoader.instance.dataFiles.OrderBy(o => o.identifier).ToList();
        Refresh("sdap");
    }

}
