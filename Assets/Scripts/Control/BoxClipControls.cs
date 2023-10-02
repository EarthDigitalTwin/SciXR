using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class BoxClipControls : MonoBehaviour {
    public GameObject highlightBox;
    public OverlayUI overlayUI;

    private DataObject data;
    private BoxCollider col;
    private VRTK_InteractableObject inter;
    private Material mat;

    Vector3 ogCenter;
    Vector3 ogSize;
    List<GameObject> currentTouching = new List<GameObject>();
    private void Start() {
        data = GetComponentInParent<DataObject>();
        mat = highlightBox.GetComponent<Renderer>().material;

        col = GetComponent<BoxCollider>();
        ogCenter = col.center;
        ogSize = col.size;
        inter = GetComponent<VRTK_InteractableObject>();
        inter.InteractableObjectGrabbed += ExpandBox;
        inter.InteractableObjectUngrabbed += ResetBox;
        inter.InteractableObjectTouched += Touched;
        inter.InteractableObjectUntouched += Untouched;
    }

    private void Update() {
        transform.localScale = new Vector3(data.xyScale, data.xyScale * data.zScale, data.xyScale);
    }

    public void ExpandBox(object sender, InteractableObjectEventArgs e) {
        col.size = new Vector3(3,col.size.y,3);
    }

    public void ResetBox() {
        ResetBox(null, new InteractableObjectEventArgs());
    }
    public void ResetBox(object sender, InteractableObjectEventArgs e) {
        //if (!inter.IsGrabbed()) {
            Vector3 newCenter = ogCenter - new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            col.center = new Vector3(newCenter.x / transform.localScale.x, newCenter.y / transform.localScale.y, newCenter.z / transform.localScale.z);
            col.size = new Vector3(ogSize.x / transform.localScale.x, ogSize.y / transform.localScale.y, ogSize.z / transform.localScale.z);
        //}
        overlayUI.SetupContainers();
    }

    public void Touched(object sender, InteractableObjectEventArgs e) {
        if (currentTouching.Count == 0)
            LeanTween.value(0.05f, 1, 0.3f).setOnUpdate((float alpha) => { mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha); });
        if (!currentTouching.Contains(e.interactingObject)) {
            currentTouching.Add(e.interactingObject);
        }

    }
    public void Untouched(object sender, InteractableObjectEventArgs e) {
        currentTouching.Remove(e.interactingObject);
        if(currentTouching.Count == 0) {
            LeanTween.value(1, 0.05f, 0.3f).setOnUpdate((float alpha) => { mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha); });
        }
    }
}
