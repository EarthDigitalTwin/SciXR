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


    private void Start() {
        menu = GetComponentInParent<FileLoad2DMenu>();
        Debug.Log("menutest: " + menu);
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
    
    public void LoadData(bool autoPosition = true) {
        Debug.Log("LoadData called");

        string parentName = transform.parent.name;
        Debug.Log("xyz: " + parentName);

        if(parentName == "Content"){
            Vector3 position = Vector3.zero;
            Vector3 eulerAngles = Vector3.zero;

            if (autoPosition) {
                float distance = 1;
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
                    // this is such a janky way of passing the dataset name to the user input menu lol

                    // if button clicked enable user input menu 
                    Debug.Log("Load data loading user input menu");
                    menu.EnableUserInputMenu();  // activate SDAP user options menu

                    // fill in dataset text with dataset that was clicked on 
                    // Access the text component of the button
                    Text bText = button.GetComponentInChildren<Text>();
                    Debug.Log("In LoadData btext:" + bText);
                    string datasetName = bText.text; // get dataset name
                    Debug.Log("In LoadData dataset name: " + datasetName);

                    TMP_Text datasetText = GameObject.Find("SDAPUserInputCanvas/SDAPDatasetTitle").GetComponent<TMP_Text>(); // find dataset label
                    Debug.Log("In LoadData dataset text: " + datasetText);
                    datasetText.text = datasetName; // set text 

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
