using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DemoController : MonoBehaviour {
    /*
    DataLoader dataLoader;

    // Use this for initialization
    void Start() {
        dataLoader = GetComponent<DataLoader>();
        VRTK.VRTK_SDKManager.instance.LoadedSetupChanged += AutoLoadFiles;
    }

    private void AutoLoadFiles(VRTK.VRTK_SDKManager sender, VRTK.VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
        SerialFile columbia = dataLoader.dataFiles.Find(currentFile => currentFile.identifier == "Columbia07");
        //DataFile haig = dataLoader.dataFiles.Find(currentFile => currentFile.identifier == "Haig");
        //DataFile slr = dataLoader.dataFiles.Find(currentFile => currentFile.identifier == "Slr");
        dataLoader.CreateDataObject(columbia, new Vector3(0, 1, 0.2f), Vector3.zero);
        //dataLoader.CreateDataObject(haig, new Vector3(0.8f, 1, -0.2f),  Vector3.zero);
        //dataLoader.CreateDataObject(slr, new Vector3(0, 1, 0), Vector3.zero);
        VRTK.VRTK_SDKManager.instance.LoadedSetupChanged -= AutoLoadFiles;
    }
    */
}
