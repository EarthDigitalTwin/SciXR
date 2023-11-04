using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoxClipControls : MonoBehaviour {
    public GameObject highlightBox;
    public OverlayUI overlayUI;

    private DataObject data;
    private BoxCollider col;
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
    }

    private void Update() {
        transform.localScale = new Vector3(data.xyScale, data.xyScale * data.zScale, data.xyScale);
    }

    public void ExpandBox() {
        col.size = new Vector3(3,col.size.y,3);
    }

    public void ResetBox() {
        Vector3 newCenter = ogCenter - new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        col.center = new Vector3(newCenter.x / transform.localScale.x, newCenter.y / transform.localScale.y, newCenter.z / transform.localScale.z);
        col.size = new Vector3(ogSize.x / transform.localScale.x, ogSize.y / transform.localScale.y, ogSize.z / transform.localScale.z);
        overlayUI.SetupContainers();
    }

    public void Touched(XRBaseInteractor interactor) {
        GameObject interactingObject = interactor.gameObject;
        if (currentTouching.Count == 0)
            LeanTween.value(0.05f, 1, 0.3f).setOnUpdate((float alpha) => { mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha); });
        if (!currentTouching.Contains(interactingObject))
            currentTouching.Add(interactingObject);
    }
    public void Untouched(XRBaseInteractor interactor) {
        GameObject interactingObject = interactor.gameObject;
        currentTouching.Remove(interactingObject);
        if (currentTouching.Count == 0)
            LeanTween.value(1, 0.05f, 0.3f).setOnUpdate((float alpha) => { mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha); });
    }
}
