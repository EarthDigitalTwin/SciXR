using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class ParentAxisSecondary : VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction {

    private DataObject _data;
    private DataObject data {
        get {
            if (_data == null)
                _data = GetComponentInParent<DataObject>();
            return _data;
        }
    }
    private BoxCollider _box;
    private BoxCollider box {
        get {
            if (_box == null)
                _box = GetComponent<BoxCollider>();
            return _box;
        }
    }
    private BoxClipControls _boxClip;
    private BoxClipControls boxClip {
        get {
            if (_boxClip == null)
                _boxClip = GetComponent<BoxClipControls>();
            return _boxClip;
        }
    }

    private bool zScaling = false;
    private float paddingOnTop = 6f;

    public override void Initialise(VRTK_InteractableObject currentGrabbdObject, VRTK_InteractGrab currentPrimaryGrabbingObject, VRTK_InteractGrab currentSecondaryGrabbingObject, Transform primaryGrabPoint, Transform secondaryGrabPoint) {
        Debug.Log("init");
        base.Initialise(currentGrabbdObject, currentPrimaryGrabbingObject, currentSecondaryGrabbingObject, primaryGrabPoint, secondaryGrabPoint);
        zScaling = false; //(secondaryInitialGrabPoint.localPosition.y > box.size.y - (box.size.y / paddingOnTop));

        initialScale = new Vector3(data.xyScale, data.xyScale * data.zScale, data.xyScale);
        if (zScaling) {
            initalLength = Mathf.Abs(secondaryGrabbingObject.transform.position.y - primaryGrabbingObject.transform.position.y);
            initialScaleFactor = data.zScale / initalLength;
        }
        else {
            Vector3 diff = secondaryGrabbingObject.transform.position - primaryGrabbingObject.transform.position;
            diff.y = 0;
            initalLength = diff.magnitude;
            initialScaleFactor = data.xyScale / initalLength;
        }
        
    }

    protected override void UniformScale() {
        float adjustedLength = 0;
        if (zScaling) {
            adjustedLength = Mathf.Abs(secondaryGrabbingObject.transform.position.y - primaryGrabbingObject.transform.position.y);
        }
        else {
            Vector3 diff = secondaryGrabbingObject.transform.position - primaryGrabbingObject.transform.position;
            diff.y = 0;
            adjustedLength = diff.magnitude;
        }
        float adjustedScale = initialScaleFactor * adjustedLength;

        Vector3 newScale = new Vector3(adjustedScale, adjustedScale, adjustedScale);
        ApplyScale(newScale);
    }

    protected override void ApplyScale(Vector3 newScale) {
        if(zScaling) {
            Transform sync = transform.Find("MeshDragSync");
            sync.localScale = new Vector3(sync.localScale.x, newScale.y, sync.localScale.z);
            //data.zScale = newScale.y;
        }
        else {
            transform.parent.localScale = new Vector3(newScale.x, newScale.y, newScale.x);
            //data.xyScale = newScale.x;
        }
    }
}
