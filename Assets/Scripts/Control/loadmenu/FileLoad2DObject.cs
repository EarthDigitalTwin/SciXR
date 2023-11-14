using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;
using TMPro;
    
public class FileLoad2DObject : MonoBehaviour {

    public SerialFile file;
    public float fadeTime = 0.4f;

    [Header("References")]
    public Text fileName;
    public Text secondRowInfo;
    public CanvasGroup metadata;
    public Text metadataLabels;
    public Text metadataValues;

    FileLoad2DMenu menu;

    FileLoad2DMenu vrMenu;

    private void Start() {
        menu = GetComponentInParent<FileLoad2DMenu>();
        vrMenu = GetComponentInParent<FileLoad2DMenu>();
        Debug.Log("menutest: " + menu);
        Debug.Log("vrmenutest: " + vrMenu);
    }

    public void FadeIn() {
        // Fade in canvas
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        LeanTween.alphaCanvas(canvasGroup, 1, fadeTime);

    }

    public void BeginDisable(float disableTime = 0) {

        // Fade out canvas
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        LeanTween.alphaCanvas(canvasGroup, 0, disableTime);
        
    }

    public void RefreshMetadata() {
        menu = GetComponentInParent<FileLoad2DMenu>();
        Debug.Log("Metadata Menu: " + menu);
        if(menu.sortBy == "Identifier") {
            fileName.text = file.identifier;
        }
        else {
            fileName.text = file.fileName;
        }
        secondRowInfo.text = file.lastModified.ToString();
        //string hasResults = (file.hasResults) ? "Yes" : "No";
        //string hasOverlay = (file.hasOverlay) ? "Yes" : "No";
        //string type = "Unknown";
        //switch(file.type) {
        //    case SerialData.DataType.flat:
        //        type = "2D Gridded";
        //        break;
        //    case SerialData.DataType.globe:
        //        type = "Global";
        //        break;
        //    case SerialData.DataType.pointcloud:
        //        type = "Point Cloud";
        //        break;
        //}
        //string metadataLabelsString = "File: " + "\n"
        //    + "Modified:" + "\n"
        //    + "Type:" + "\n"
        //    + "Has Results:" + "\n"
        //    + "Has Overlay:" + "\n"
        //    + "Runtime:" + "\n"
        //    + "Triangles:" + "\n"
        //    + "Vertices:";
        //string metadataValuesString = file.fileName + "\n"
        //   + file.lastModified + "\n"
        //   + type + "\n"
        //   + hasResults + "\n"
        //   + hasOverlay + "\n"
        //   + file.runtimeName + "\n"
        //   + file.triangleCount + "\n"
        //   + file.vertexCount;
        
        //metadataLabels.text = metadataLabelsString;
        //metadataValues.text = metadataValuesString;
        
    }
    
    public void LoadUserInputMenu(){
            Debug.Log("LoadUserInputMenu called");

            FileLoad2DMenu[] menus = Resources.FindObjectsOfTypeAll<FileLoad2DMenu>();
            FileLoad2DMenu userInputVR = menus[2]; //user input 2
            userInputVR.EnableUserInputMenu();
    }

    
    public void LoadData(bool autoPosition = true) {
        Debug.Log("LoadData called");

        // metadata.gameObject.SetActive(false);
        // Vector3 position = Vector3.zero;
        // Vector3 eulerAngles = Vector3.zero;

        // if (autoPosition) {
        //     float distance = 1;
        //     Transform cameraTransform = DesktopInterface.instance.transform;
        //     Vector3 headsetForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
        //     headsetForward.Normalize();
        //     position = cameraTransform.position + headsetForward * distance;
        //     position = new Vector3(position.x, 1, position.z);

        //     Vector3 directionVector = position - cameraTransform.position;
        //     directionVector.y = 0;
        //     eulerAngles = Quaternion.LookRotation(directionVector).eulerAngles;
        // }
        // else {
        //     position = transform.position + transform.forward * 0.4f;
        //     eulerAngles = Quaternion.LookRotation(position - transform.position).eulerAngles;
        //     eulerAngles.x = 0;
        //     eulerAngles.z = 0;
        // }

        string parentName = transform.parent.name;
        Debug.Log("xyz: " + parentName);

        if(parentName == "Content"){
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

            menu.BeginDisable();
            Slider userFilesSlider = GameObject.Find("FilesSlider")?.GetComponent<Slider>();
        
            if (userFilesSlider?.value == 0){  //SDAP 
                Debug.Log("LoadData SDAP Files called");
                
                // Retrieve the clicked button
                GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
                Button button = clickedButton.GetComponent<Button>();

                // set dataset text
                if (button != null)
                {
                    // if button clicked enable user input menu 
                    Debug.Log("Load data loading user input menu");
                    LoadUserInputMenu(); // menu.EnableUserInputMenu();//LoadUserInputMenu(); // instantiate a new menu with user options

                    // fill in dataset text with dataset that was clicked on 
                    // Access the text component of the button
                    Text bText = button.GetComponentInChildren<Text>();
                    string textValue = bText.text; // get dataset name

                    Button datasetButton = GameObject.Find("DatasetButton").GetComponent<Button>(); // find button text ui
                    Text buttonText = datasetButton.GetComponentInChildren<Text>(); // extract text
                    buttonText.text = textValue; // set text 
                }
            }

            else { // user input file 
                Debug.Log("LoadData User Files called");
                DataLoader.instance.CreateDataObject(file, position, eulerAngles);
            }
        }

        if(parentName == "ContentVR"){
            Debug.Log("CONTENTVR");

            Vector3 position = Vector3.zero;
            Vector3 eulerAngles = Vector3.zero;
            position = transform.position + transform.forward * 0.4f;
            eulerAngles = Quaternion.LookRotation(position - transform.position).eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;

            Slider userFilesSliderVR = GameObject.Find("FilesSliderVR")?.GetComponent<Slider>();
            if (userFilesSliderVR?.value == 0){  //SDAP 

                // Retrieve the clicked button
                GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
                Button button = clickedButton.GetComponent<Button>();

                // set dataset text
                if (button != null)
                {
                    // fill in dataset button with dataset chosen (VR UserInputMenu)
                    Button test = vrMenu.GetComponentInParent<Button>();
                    Text bText = button.GetComponentInChildren<Text>();
                    string textValue = bText.text; // get dataset name

                    Button vrMenuButton = vrMenu.GetComponentInChildren<Button>();
                    Text vrMenuText = vrMenuButton.GetComponentInChildren<Text>(); // extract text
                    
                    vrMenuText.text = textValue; // set text 

                    FileLoad2DMenu VRUserMenu = GameObject.Find("UserInputVR").GetComponent<FileLoad2DMenu>();

                    //Do
                    // - disable vrMenu
                    Debug.Log("t1: " + vrMenu);
                    vrMenu.BeginDisable();

                    // - enable userInputMenu
                    Debug.Log("t2: " + VRUserMenu);
                    VRUserMenu.EnableUserInputMenu();

                    // CanvasGroup vrMenuCanvas = VRUserMenu.GetComponent<CanvasGroup>();
                    // LeanTween.alphaCanvas(vrMenuCanvas, 1, 0.41f);
                }
            }

            else { // user input file 
                Debug.Log("LoadData User Files called");
                DataLoader.instance.CreateDataObject(file, position, eulerAngles);
            }
        }
        
    }

    public void HandlePointerEnter(BaseEventData data) {
        string hasResults = (file.hasResults) ? "Yes" : "No";
        string hasOverlay = (file.hasOverlay) ? "Yes" : "No";
        string type = "Unknown";
        switch (file.type) {
            case SerialData.DataType.flat:
                type = "2D Gridded";
                break;
            case SerialData.DataType.globe:
                type = "Global";
                break;
            case SerialData.DataType.pointcloud:
                type = "Point Cloud";
                break;
        }
        string metadataLabelsString = "File: " + "\n"
            + "Modified:" + "\n"
            + "Type:" + "\n"
            + "Has Results:" + "\n"
            + "Has Overlay:" + "\n"
            + "Runtime:" + "\n"
            + "Triangles:" + "\n"
            + "Vertices:";
        string metadataValuesString = file.fileName + "\n"
           + file.lastModified + "\n"
           + type + "\n"
           + hasResults + "\n"
           + hasOverlay + "\n"
           + file.runtimeName + "\n"
           + file.triangleCount + "\n"
           + file.vertexCount;

        metadataLabels.text = metadataLabelsString;
        metadataValues.text = metadataValuesString;
        LeanTween.cancel(metadata.gameObject);
        LeanTween.alphaCanvas(metadata, 1, 0.2f);

    }

    public void HandlePointerExit(BaseEventData data) {
        LeanTween.cancel(metadata.gameObject);
        LeanTween.alphaCanvas(metadata, 0, 0.2f);
    }
}
