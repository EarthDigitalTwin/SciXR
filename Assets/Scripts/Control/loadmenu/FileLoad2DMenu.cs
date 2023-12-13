using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Dropdown = TMPro.TMP_Dropdown;
using TMPro;
using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Net;

public class FileLoad2DMenu : MonoBehaviour {

    public GameObject fileObjectPrefab;
    public GameObject filesContainer;
    public GameObject userInputCanvas;


    public int page = 0;
    public string sortBy = "Last Modified";

    CanvasGroup menuCanvas;

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
        Debug.Log("enable user input menu: " + userInputCanvas);

        userInputCanvas.gameObject.SetActive(true);
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
        userInputCanvas.gameObject.SetActive(false);  // disable user input menu
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

                foreach (NexusObject obj in NexusObjects)
                {
                    Debug.Log("In foreach with NexusObject: " + obj.shortName);
                    //if (obj.shortName.Contains(filter)) {
                    GameObject newFileObj = Instantiate(fileObjectPrefab, filesContainer.transform);
                    newFileObj.name = obj.shortName; // sets button name

                    FileLoad2DObject file = newFileObj.GetComponent<FileLoad2DObject>();
                    string name = obj.shortName;
                    if (name.Length > 20) {
                        // shorten name if too long
                        name = name.Substring(0, 20) + "...";
                    }
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
                    Debug.Log("In SDAPFiles, File: " + file);

                    newFileObj.SetActive(true);
                    Debug.Log("In SDAPFiles, newFileObj: " + newFileObj);
                    Debug.Log("Its transform: " + newFileObj.transform);
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
        Debug.Log("in generateSDAPInstance");
        bool autoPosition = true;
        Vector3 position = Vector3.zero;
        Vector3 eulerAngles = Vector3.zero;

        if (autoPosition) {
            float distance = 1;
            Debug.Log("finding camera transform");
            Transform cameraTransform = GameObject.Find("Main Camera").transform;
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
        if (userInputCanvas) {
            userInputCanvas.SetActive(false);
        }

        // make data instance
        SerialFile sdapFile = null;
        try
        {   
            Debug.Log("entered try");
            // for android, we stored the file here
            string path = Path.Combine(Application.persistentDataPath, identifier + ".ply");
            if (Application.platform != RuntimePlatform.Android) {
                // non-android
                path = "Assets/Scripts/Control/processing/" + identifier + ".ply";
            }
            // Because we don't have to access persistentDataPath files through the WebRequest
            // API, we can just use normal file i/o to read SDAP files.
            Debug.Log("Reading SDAP file from path: " + path);
            sdapFile = PLYReader.MetadataFromPath(path);
            Debug.Log("Successfully read file, calling CreateDataObject...");
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
        // TODO The below code is for using TMP InputFields. They don't work in XR. So for now
        // we have to use Dropdowns, because they work. Ideally we would use dedicated form components
        // for all of this.

        // InputField variableField = GameObject.Find("SDAPVariable")?.GetComponent<InputField>();
        // InputField units = GameObject.Find("SDAPUnits")?.GetComponent<InputField>();
        // InputField identifier = GameObject.Find("SDAPIdentifier")?.GetComponent<InputField>();

        // InputField Box = GameObject.Find("SDAPBBox")?.GetComponent<InputField>();
        // // Retrieve the value from the input field
        // string[] bBox = Box.text.Split(',');

        // // Handle date range selection
        // // TODO This should really be handled by a dedicated datepicker component. But the one
        // // currently included in this project is broken, so I'm doing it brittlely here.
        // // We expect the date range to be in the format "yyyy-MM-dd" (e.g. "2023-12-25")
        // InputField startDateField = GameObject.Find("SDAPDateFrom")?.GetComponent<InputField>();
        // InputField endDateField = GameObject.Find("SDAPDateTo")?.GetComponent<InputField>();
        // string startTimeRaw = startDateField.text;
        // string endTimeRaw = endDateField.text;

        // // Ensure that the date range is valid
        // DateTime startDate, endDate;
        // string dateFormat = "yyyy-MM-dd";

        // bool isStartDateValid = DateTime.TryParseExact(startTimeRaw, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate);
        // bool isEndDateValid = DateTime.TryParseExact(endTimeRaw, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate);

        // if (!isStartDateValid || !isEndDateValid)
        // {
        //     Debug.LogError("Invalid date format. Dates must be in the format: " + dateFormat);
        //     return;
        // }

        // if (startDate > endDate)
        // {
        //     Debug.LogError("Invalid date range: " + startTimeRaw + " - " + endTimeRaw);
        //     return;
        // }

        TMP_Text datasetField = GameObject.Find("SDAPDatasetTitle").GetComponent<TMP_Text>(); // find dataset label

        TMP_Dropdown unitsDropdown = GameObject.Find("SDAPUnits")?.GetComponent<TMP_Dropdown>();
        string units = unitsDropdown.options[unitsDropdown.value].text;
        TMP_Dropdown identifierDropdown = GameObject.Find("SDAPIdentifier")?.GetComponent<TMP_Dropdown>();
        string identifier = identifierDropdown.options[identifierDropdown.value].text;
        if (identifier == "(Default)") {
            identifier = "_" + datasetField.text + "_";  // TODO make this more unique
        }
        TMP_Dropdown variableDropdown = GameObject.Find("SDAPVariable")?.GetComponent<TMP_Dropdown>();
        string variable = variableDropdown.options[variableDropdown.value].text;
        TMP_Dropdown minLonDropdown = GameObject.Find("SDAPBBoxMinLon")?.GetComponent<TMP_Dropdown>();
        string minLon = minLonDropdown.options[minLonDropdown.value].text;
        TMP_Dropdown minLatDropdown = GameObject.Find("SDAPBBoxMinLat")?.GetComponent<TMP_Dropdown>();
        string minLat = minLatDropdown.options[minLatDropdown.value].text;
        TMP_Dropdown maxLonDropdown = GameObject.Find("SDAPBBoxMaxLon")?.GetComponent<TMP_Dropdown>();
        string maxLon = maxLonDropdown.options[maxLonDropdown.value].text;
        TMP_Dropdown maxLatDropdown = GameObject.Find("SDAPBBoxMaxLat")?.GetComponent<TMP_Dropdown>();
        string maxLat = maxLatDropdown.options[maxLatDropdown.value].text;

        TMP_Dropdown dateFromYearDropdown = GameObject.Find("SDAPDateFromYear")?.GetComponent<TMP_Dropdown>();
        TMP_Dropdown dateFromMonthDropdown = GameObject.Find("SDAPDateFromMonth")?.GetComponent<TMP_Dropdown>();
        TMP_Dropdown dateFromDayDropdown = GameObject.Find("SDAPDateFromDay")?.GetComponent<TMP_Dropdown>();
        TMP_Dropdown dateToYearDropdown = GameObject.Find("SDAPDateToYear")?.GetComponent<TMP_Dropdown>();
        TMP_Dropdown dateToMonthDropdown = GameObject.Find("SDAPDateToMonth")?.GetComponent<TMP_Dropdown>();
        TMP_Dropdown dateToDayDropdown = GameObject.Find("SDAPDateToDay")?.GetComponent<TMP_Dropdown>();
        string dateFromYear = dateFromYearDropdown.options[dateFromYearDropdown.value].text;
        string dateFromMonth = dateFromMonthDropdown.options[dateFromMonthDropdown.value].text;
        string dateFromDay = dateFromDayDropdown.options[dateFromDayDropdown.value].text;
        string dateToYear = dateToYearDropdown.options[dateToYearDropdown.value].text;
        string dateToMonth = dateToMonthDropdown.options[dateToMonthDropdown.value].text;
        string dateToDay = dateToDayDropdown.options[dateToDayDropdown.value].text;

        string startTimeRaw = dateFromYear + "-" + dateFromMonth + "-" + dateFromDay;
        string endTimeRaw = dateToYear + "-" + dateToMonth + "-" + dateToDay;

        int interpolation = 1;
        string labelTime = startTimeRaw;
        string sdap_url = "https://ideas-digitaltwin.jpl.nasa.gov/nexus";

        Debug.Log("In handleInput, done parsing form. ");

        // generate data file
        if (Application.platform == RuntimePlatform.Android) {
            // android stuff
            SdapConfiguration config = new SdapConfiguration
            {
                SdapUrl = sdap_url,
                Variables = new List<string> { variable },
                Units = units,
                Dataset = datasetField.text,
                Identifier = identifier,
                Description = "",
                Instrument = "",
                Bbox = new BoundingBox {
                    MinLon = double.Parse(minLon),
                    MaxLon = double.Parse(maxLon),
                    MinLat = double.Parse(minLat),
                    MaxLat = double.Parse(maxLat)
                },
                StartTime = DateTime.ParseExact(startTimeRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                EndTime = DateTime.ParseExact(endTimeRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                LabelTime = DateTime.ParseExact(labelTime, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Interpolation = interpolation
            };
            Debug.Log("In handleInput, calling sdapToPly.ProcessSdap...");
            GameObject dummy = new GameObject();
            SdapToPly sdapToPly = dummy.AddComponent<SdapToPly>();
            sdapToPly.ProcessSdap(config, () => {
                Debug.Log("In handleInput, calling generateSDAPInstance...");
                // calling this as callback to make sure file is written
                generateSDAPInstance(config.Identifier);
                Destroy(dummy);
            });
        } else {
            //format bbox
            //TODO fix bounding box 
            string content = "sdap_url: \"" + sdap_url 
                        + "\"\n" + "variables: \n  -\"" 
                        + variable + "\"\n" 
                        + "units: \"" + units + "\"\n" 
                        + "dataset: \"" + datasetField.text + "\"\n" 
                        + "identifier: \"" + identifier + "\"\n"
                        + "description: \"" + "" + "\"\n"
                        + "instrument: " + "\"" + "\"\n" // -81, -65, 34, 50
                        + "bbox: {" + "'min_lon': " + minLon + ", 'max_lon': " + maxLon + ", 'min_lat': " + minLat + ", 'max_lat': " + maxLat + "}\n"
                        + "start_time: \"" + startTimeRaw + "\"\n"
                        + "end_time: \"" + endTimeRaw + "\"\n"
                        + "label_time: \"" + labelTime + "\"\n"
                        + "interpolation: " + interpolation;
            // generate data file
            string filePath = "Assets/Scripts/Control/processing/test_config.yaml";
            File.WriteAllText(filePath, content);
            Debug.Log("In handleInput, calling shell script");
            CallShellScript();
            generateSDAPInstance(identifier);
        }
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

    public void TestSdap()
    {
        Debug.Log("Test SDAP called");
        // Testing. This uses values from sample_config.yaml
        SdapConfiguration config = new SdapConfiguration
        {
            SdapUrl = "https://ideas-digitaltwin.jpl.nasa.gov/nexus",
            Dataset = "TROPOMI_CO_global",
            Variables = new List<string> { "CO" },
            Units = "",
            Identifier = "TROPOMI_CO_NE_USA",
            Description = "TROPOMI Carbon Monoxide Northeast USA",
            Instrument = "TROPOMI",
            Bbox = new BoundingBox
            {
                MinLon = -81,
                MaxLon = -65,
                MinLat = -34,
                MaxLat = 50
            },
            StartTime = DateTime.ParseExact("2023-06-07", "yyyy-MM-dd", CultureInfo.InvariantCulture),
            EndTime = DateTime.ParseExact("2023-06-08", "yyyy-MM-dd", CultureInfo.InvariantCulture),
            LabelTime = DateTime.ParseExact("2023-06-07", "yyyy-MM-dd", CultureInfo.InvariantCulture),
            Interpolation = 5
        };

        GameObject dummy = new GameObject();
        SdapToPly sdapToPly = dummy.AddComponent<SdapToPly>();
        sdapToPly.ProcessSdap(config, () =>
        {
            Debug.Log("In handleInput, calling generateSDAPInstance...");
            // calling this as callback to make sure file is written
            generateSDAPInstance(config.Identifier);
            Destroy(dummy);
        });
    }

}
