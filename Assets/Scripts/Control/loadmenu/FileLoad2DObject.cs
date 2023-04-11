using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

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
        menu = GameObject.Find("FileSelection_3D").GetComponent<FileLoad2DMenu>();
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
        menu = GameObject.Find("FileSelection_3D").GetComponent<FileLoad2DMenu>();
        if(menu.sortBy == "Identifier") {
            fileName.text = file.identifier;
        }
        else {
            fileName.text = file.fileName;
        }
        secondRowInfo.text = file.lastModified.ToString();
    }
    
    public void LoadData(bool autoPosition = true) {
        Vector3 position = Vector3.zero;
        Vector3 eulerAngles = Vector3.zero;

        if (autoPosition) {
            float distance = 2;
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
            position = transform.position + transform.forward * 0.6f;
            eulerAngles = Quaternion.LookRotation(position - transform.position).eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
        }
        menu.BeginDisable();
        DataLoader.instance.CreateDataObject(file, position, eulerAngles);
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
