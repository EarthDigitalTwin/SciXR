using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Dropdown = TMPro.TMP_Dropdown;
using VRTK;

public class FileLoad2DMenu : MonoBehaviour {

    public GameObject fileObjectPrefab;
    public GameObject filesContainer;
    public int page = 0;
    public string sortBy = "Last Modified";

    CanvasGroup menuCanvas;

 //   void Start () {

	//}

    void OnEnable() {
        menuCanvas = GetComponent<CanvasGroup>();
        if (menuCanvas == null)
            menuCanvas = gameObject.AddComponent<CanvasGroup>();
        fileObjectPrefab.SetActive(false);

        Refresh("");
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
                
                //LeanTween.delayedCall(delay, () => {
                    fileObject.gameObject.SetActive(true);
                    fileObject.GetComponent<FileLoad2DObject>().FadeIn();
                //});
                delay += 0.1f;
            }
        }
    }

    public void Refresh(string filter) {
        int position = 0;
        int numSlots = 10;
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
        //int endCount = ((page + 1) * numSlots < filteredFiles) ? (page + 1) * numSlots : filteredFiles;
        int endCount = DataLoader.instance.dataFiles.Count;
        for (int fileCount = page * numSlots; fileCount < endCount; fileCount++) {
            Debug.Log(DataLoader.instance.dataFiles[fileCount].fileName);
            if (DataLoader.instance.dataFiles[fileCount].fileName.Contains(filter)) {
                Debug.Log("Filtered" + DataLoader.instance.dataFiles[fileCount].fileName);
                GameObject newFileObj = Instantiate(fileObjectPrefab, filesContainer.transform);
                FileLoad2DObject file = newFileObj.GetComponent<FileLoad2DObject>();
                file.file = DataLoader.instance.dataFiles[fileCount];
                file.RefreshMetadata();
                newFileObj.name = DataLoader.instance.dataFiles[fileCount].runtimeName;
                newFileObj.SetActive(true);
                position++;
            }
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
}
