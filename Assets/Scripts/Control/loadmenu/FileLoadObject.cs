using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public class FileLoadObject : MonoBehaviour{
    
    public SerialFile file;
    public float fadeTime = 0.4f;

    [Header("References")]
    public Text fileName;
    public CanvasGroup metadata;
    public Text metadataLabels;
    public Text metadataValues;
    public Renderer fileBase;

    [Header("Icon Previews")]
    public GameObject meshIcon;
    public GameObject globeIcon;
    public GameObject pointCloudIcon;
    public GameObject newIcon;

    PointerEventData lastEventData;
    FileLoadMenu menu;
    GameObject currentIcon;

    private void Start() {
        menu = GetComponentInParent<FileLoadMenu>();
    }

    public void FadeIn() {
        // Fade in base
        fileBase.material.color = new Color(fileBase.sharedMaterial.color.r, fileBase.sharedMaterial.color.g, fileBase.sharedMaterial.color.b, 0);
        LeanTween.alpha(fileBase.gameObject, 1, fadeTime);

        // Fade in canvas
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        LeanTween.alphaCanvas(canvasGroup, 1, fadeTime);

        // Fade in preview
        if (currentIcon != null) {
            Renderer iconRend = currentIcon.GetComponentInChildren<Renderer>();
            iconRend.material.color = new Color(iconRend.sharedMaterial.color.r, iconRend.sharedMaterial.color.g, iconRend.sharedMaterial.color.b, 0);
            LeanTween.alpha(currentIcon.gameObject, 0.8f, fadeTime);
        }
    }

    public void BeginDisable(float disableTime = 0) {

        // Fade out canvas
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        LeanTween.alphaCanvas(canvasGroup, 0, disableTime);

        // Fade out preview
        if (currentIcon != null) {
            Renderer iconRend = currentIcon.GetComponentInChildren<Renderer>();
            LeanTween.alpha(currentIcon.gameObject, 0, disableTime);
        }
        
        // Fade out base
        LeanTween.alpha(fileBase.gameObject, 0, disableTime).setOnComplete(() => { gameObject.SetActive(false); });
    }

    public void RefreshMetadata() {
        menu = GetComponentInParent<FileLoadMenu>();
        if(menu.sortBy == "Identifier") {
            fileName.text = file.identifier;
        }
        else {
            fileName.text = file.fileName;
        }
        string hasResults = (file.hasResults) ? "Yes" : "No";
        string hasOverlay = (file.hasOverlay) ? "Yes" : "No";
        string type = "Unknown";
        switch(file.type) {
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

        // Load Preview
        currentIcon = meshIcon;
        if (FileLoadMenu.loadedMeshes.ContainsKey(file.identifier)) {
            meshIcon.GetComponentInChildren<MeshFilter>().mesh = FileLoadMenu.loadedMeshes[file.identifier];
        }
        else {
            string filePath = Path.Combine(DataLoader.instance.dataPreviewPath, file.identifier + ".json");
            if (File.Exists(filePath)) {
                SerialMesh meshData = JsonUtility.FromJson<SerialMesh>(File.ReadAllText(filePath));
                Mesh previewMesh = SerialMesh.MeshDataToMesh(meshData);
                FileLoadMenu.loadedMeshes[file.identifier] = previewMesh;
                meshIcon.GetComponentInChildren<MeshFilter>().mesh = previewMesh;
            } else {
                currentIcon = newIcon;
            }
        }
        if (currentIcon == meshIcon) {
            if (file.type == SerialData.DataType.flat) {
                meshIcon.transform.parent.localEulerAngles = new Vector3(-35, 0, 0);
                meshIcon.transform.parent.localPosition = new Vector3(0, 0.0094f, 0.0028f);
            }
            else if (file.type == SerialData.DataType.globe) {
                meshIcon.transform.localPosition += new Vector3(0, 0.013f, 0);
                meshIcon.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
        }
        currentIcon.SetActive(true);
        
    }

    // VR pointer handling
    /*
    private void OnVRPointerEnter(object sender, DestinationMarkerEventArgs e) {
        if (e.target == this.transform) {
            OnPointerEnter(null);
        }
    }

    private void OnVRPointerExit(object sender, DestinationMarkerEventArgs e) {
        if (e.target == this.transform) {
            OnPointerExit(null);
        }
    }
    

    bool isClicked = false;
    bool frameClick = false;
    private void OnVRPointerClick(object sender, DestinationMarkerEventArgs e) {
        if(!isClicked && e.controllerReference.scriptAlias.GetComponent<VRTK_ControllerEvents>().triggerPressed) {
            isClicked = true;
            frameClick = true;
        }
        else if(isClicked && !e.controllerReference.scriptAlias.GetComponent<VRTK_ControllerEvents>().triggerPressed) {
            isClicked = false;
        }
            
        if (e.target == this.transform && frameClick) {
            OnPointerClick(null);
            frameClick = false;
        }
    }
    */
    #region Pointer Handling

    // AR pointer handling
    public void OnARPointerEnter()
    {
        OnPointerEnter(null);
    }

    public void OnARPointerExit()
    {
        OnPointerExit(null);
    }

    public void OnARPointerClick()
    {
        LoadData();
    }

    // Screen space pointing handling 
    public void OnPointerEnter(PointerEventData eventData) {
        LeanTween.cancel(metadata.gameObject);
        metadata.gameObject.SetActive(true);
        metadata.alpha = 0;
        LeanTween.alphaCanvas(metadata, 1, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        LeanTween.cancel(metadata.gameObject);
        LeanTween.alphaCanvas(metadata, 0, 0.2f).setOnComplete(() => { metadata.gameObject.SetActive(false); });
    }

    public void OnPointerClick(PointerEventData eventData) {
        //if(eventData.button == PointerEventData.InputButton.Left) {
        //    Vector3 position = DataLoader.GetFrontOfCamera(1);//GetNextStartingPosition();
        //    Quaternion rotation = DataLoader.RotateTowardsCamera(position);
        //    DataLoader.instance.CreateDataObject(file, position, rotation);
        //    GetComponentInParent<FileObjectMenu>().gameObject.SetActive(false);
        //}
    }

    #endregion

    public void LoadData(bool autoPosition = true) {
        metadata.gameObject.SetActive(false);
        Vector3 position = Vector3.zero;
        Vector3 eulerAnglers = Vector3.zero;

        if (autoPosition) {
            float distance = 1;
            Transform cameraTransform = DesktopInterface.instance.transform;
            Vector3 headsetForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
            headsetForward.Normalize();
            position = cameraTransform.position + headsetForward * distance;
            position = new Vector3(position.x, 1, position.z);

            Vector3 directionVector = position - cameraTransform.position;
            directionVector.y = 0;
            eulerAnglers = Quaternion.LookRotation(directionVector).eulerAngles;
        }
        else {
            position = transform.position + transform.forward * 0.4f;
            eulerAnglers = Quaternion.LookRotation(position - transform.position).eulerAngles;
            eulerAnglers.z = 0;
        }
        menu.BeginDisable();
        DataLoader.instance.CreateDataObject(file, position, eulerAnglers);
    }
    
}
