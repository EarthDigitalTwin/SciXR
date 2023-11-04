using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
//using UnityEditor;

public class SceneController : MonoBehaviour {
    public bool manualSceneLoad = false;

    public Scene[] scenes;
	public GameObject mainScreenUI;

    public bool disableVR = true;

    void Awake() {
        FindObjectOfType<DataObject>()?.gameObject.SetActive(false);
        XRSettings.enabled = false;
        XRSettings.enabled = true;
        if (disableVR) {
            XRSettings.enabled = false;
            XRSettings.LoadDeviceByName("None");
        }
    }

    // Use this for initialization
    void Start () {
		mainScreenUI.SetActive (true);

        // GetComponent<VRTK_SDKManager> ().TryLoadSDKSetup (0, true);


        // Load Scenery!
        //bool defaultLoaded = false;
        //for(int count=0; count <SceneManager.sceneCount; count ++) {
        //    defaultLoaded = defaultLoaded || SceneManager.GetSceneAt(count).name == "MetalRoom";
        //}
        //if (!defaultLoaded && !manualSceneLoad) {
        //    SceneManager.LoadScene("MetalRoom", LoadSceneMode.Additive);
        //}
    }

    public void Reset() {
        Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
    }
}
