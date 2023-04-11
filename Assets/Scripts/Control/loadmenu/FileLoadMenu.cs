using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Dropdown = TMPro.TMP_Dropdown;

public class FileLoadMenu : MonoBehaviour {
    public static Dictionary<string, Mesh> loadedMeshes = new Dictionary<string, Mesh>();

    public GameObject fileObjectPrefab;
    public GameObject filesContainer;
    public GameObject menuBase;
    public CanvasGroup menuCanvas;

    public int page = 0;
    public string sortBy = "Last Modified";

    private List<GameObject> loadedPreviews = new List<GameObject>();

    void Start () {
        fileObjectPrefab.SetActive(false);
	}

    void OnEnable() {
        if(filesContainer.transform.childCount <= 1) {
            Refresh();
        }
        foreach (Transform fileObject in filesContainer.transform) {
            fileObject.gameObject.SetActive(false);
        }
        
        menuCanvas.alpha = 0;
        LeanTween.alpha(menuBase.gameObject, 1, 0.4f);
        LeanTween.alphaCanvas(menuCanvas, 1, 0.4f); 
        EnableFileObjects();

    }
    public void BeginDisable() {
        //foreach(Renderer rend in GetComponentsInChildren<Renderer>()) {
        //    rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 1);
            
        //}
        LeanTween.alpha(menuBase.gameObject, 0.5f, 0.41f);  
        LeanTween.alphaCanvas(menuCanvas, 0, 0.41f).setOnComplete(() => { gameObject.SetActive(false); }); ;

        foreach (Transform fileObject in filesContainer.transform) {
            if (fileObject.name != "FileObjectPrefab") {
                fileObject.GetComponent<FileLoadObject>().BeginDisable(0.4f);
            }
        }

    }
    
    public void EnableFileObjects() {
        float delay = 0.1f;

        foreach (Transform fileObject in filesContainer.transform) {
            if (fileObject.name != "FileObjectPrefab") {
                
                LeanTween.delayedCall(delay, () => {
                    fileObject.gameObject.SetActive(true);
                    //fileObject.GetComponent<FileLoadObject>().assignedDropzone.ForceSnap(fileObject.gameObject);
                    fileObject.GetComponent<FileLoadObject>().FadeIn();
                });
                delay += 0.1f;
            }
        }
    }

    public void Refresh() {
        int position = 0;
        int numSlots = 5;

        foreach (Transform child in filesContainer.transform) {
            if (child.name != "FileObjectPrefab")
                Destroy(child.gameObject);
        }
        if (DataLoader.instance?.dataFiles == null)
            return;

        int endCount = ((page + 1) * numSlots  < DataLoader.instance.dataFiles.Count) ? (page + 1) * numSlots : DataLoader.instance.dataFiles.Count;
        for (int fileCount = page * numSlots; fileCount < endCount; fileCount++) {
            GameObject newFileObj = Instantiate(fileObjectPrefab, filesContainer.transform);
            FileLoadObject file = newFileObj.GetComponent<FileLoadObject>();
            file.file = DataLoader.instance.dataFiles[fileCount];
            file.RefreshMetadata();
            newFileObj.name = DataLoader.instance.dataFiles[fileCount].runtimeName;
            newFileObj.SetActive(true);
            position++;
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
        }
    }
    public void SortFilesByIdentifier() {
        DataLoader.instance.dataFiles = DataLoader.instance.dataFiles.OrderBy(o => o.identifier).ToList();
        Refresh();
    }

    public void SortFilesByFileName() {
        DataLoader.instance.dataFiles = DataLoader.instance.dataFiles.OrderBy(o => o.fileName).ToList();
        Refresh();
    }
    
    public void SortFilesByLastModified() {
        DataLoader.instance.dataFiles = DataLoader.instance.dataFiles.OrderBy(o => o.lastModified).ToList();
        Refresh();
    }

    //public void ToggleFade() {
    //    foreach (Material material in materials) {
    //        material.ToggleMode();
    //    }
    //}
}
