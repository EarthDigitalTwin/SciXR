using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Dropdown = TMPro.TMP_Dropdown;

public class FileLoad2DMenu : MonoBehaviour {

    public GameObject fileObjectPrefab;
    public GameObject filesContainer;
    public int page = 0;
    public string sortBy = "Last Modified";

    CanvasGroup menuCanvas;

    void OnEnable() {
        menuCanvas = GetComponent<CanvasGroup>();
        if (menuCanvas == null)
            menuCanvas = gameObject.AddComponent<CanvasGroup>();
        fileObjectPrefab.SetActive(false);

        Refresh();
        foreach (Transform fileObject in filesContainer.transform) {
            fileObject.gameObject.SetActive(false);
        }
        if(menuCanvas != null) {
            menuCanvas.alpha = 0;
            LeanTween.alphaCanvas(menuCanvas, 1, 0.4f);
            EnableFileObjects();
        }
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
                fileObject.gameObject.SetActive(true);
                fileObject.GetComponent<FileLoad2DObject>().FadeIn();
                delay += 0.1f;
            }
        }
    }

    public void Refresh() {
        float pos_y = 0.012f;
        float pos_y_delta = 0.137f;
        
        foreach (Transform child in filesContainer.transform) {
            if (child != fileObjectPrefab.transform)
                child.gameObject.SetActive(false); //Destroy(child.gameObject);
        }
        if (DataLoader.instance?.dataFiles == null)
            return;

        List<GameObject> fileObjects = new List<GameObject>();
        int numSlots = DataLoader.instance.dataFiles.Count;
        int endCount = ((page + 1) * numSlots  < DataLoader.instance.dataFiles.Count) ? (page + 1) * numSlots : DataLoader.instance.dataFiles.Count;
        for (int fileCount = page * numSlots; fileCount < endCount; fileCount++) {
            GameObject newFileObj = Instantiate(fileObjectPrefab, filesContainer.transform);
            FileLoad2DObject file = newFileObj.GetComponent<FileLoad2DObject>();
            file.file = DataLoader.instance.dataFiles[fileCount];
            file.RefreshMetadata();
            newFileObj.name = DataLoader.instance.dataFiles[fileCount].runtimeName;
            newFileObj.transform.localPosition = new Vector3(0.3f, pos_y, 0);
            newFileObj.SetActive(false);
            pos_y -= pos_y_delta;
            fileObjects.Add(newFileObj);
        }

        filesContainer.GetComponent<PageObjectCollection>().SetUp(fileObjects);
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
        DataLoader.instance.dataFiles = DataLoader.instance.dataFiles.OrderByDescending(o => o.lastModified).ToList();
        Refresh();
    }
}
