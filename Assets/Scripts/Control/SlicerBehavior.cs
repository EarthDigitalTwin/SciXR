using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerBehavior : MonoBehaviour {

    public List<Material> materials = new List<Material>();

    private int previousMode = 1;
    private Camera clippingCamera;

    void Start() {

        clippingCamera = GetComponentInChildren<Camera>();
        if (clippingCamera) {
            clippingCamera.nearClipPlane = 0.315f;
            clippingCamera.farClipPlane = 0.3155f;
        }
    }

    // Update is called once per frame
    void Update(){
        foreach(Material material in materials){
            material.SetVector("_PlanePoint", this.transform.position);
            material.SetVector("_PlaneNormal", -this.transform.up);
        }
    }

    public void ProcessNewLoad(GameObject newObj) {
        materials.Add(newObj.GetComponent<DataObject>().material);
        if(isActiveAndEnabled) {
            newObj.GetComponent<DataObject>().material.SetInt("_ExtraClipMode", 1);
        }
    }

    private void OnDisable() {
        foreach (Material material in materials) {
            material.SetInt("_ExtraClipMode", previousMode);
        }
    }

    private void OnEnable() {
        materials = DataLoader.instance.GetMaterials();

        foreach (Material material in materials) {
            previousMode = material.GetInt("_ExtraClipMode");
            material.SetInt("_ExtraClipMode", 1);
        }
    }



}
