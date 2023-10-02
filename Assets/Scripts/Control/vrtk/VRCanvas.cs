using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class VRCanvas : VRTK_UICanvas {

    public VRTK_SnapDropZone defaultDropzone;
    
    private void Start() {
        //Subscribe to data destroy event (in case it's parented elsewhere)
        if(GetComponentInParent<DataObject>() != null)
            GetComponentInParent<DataObject>().OnDestroyEvent.AddListener(DestroySelf);
    }

    private new void OnEnable() {
        base.OnEnable();
        if (defaultDropzone != null) {
            transform.position = defaultDropzone.transform.position;
            GameObject currentSnapped = defaultDropzone.GetCurrentSnappedObject();
            if(currentSnapped != null) {
                currentSnapped.SetActive(false);
            }

            defaultDropzone.ForceSnap(this.gameObject);
        }
    }
    
    void DestroySelf() {
        GameObject.Destroy(this.gameObject);
    }

    protected override void SetupCanvas() {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null || canvas.renderMode != RenderMode.WorldSpace) {
            VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTK_UICanvas", "Canvas", "the same", " that is set to `Render Mode = World Space`"));
            return;
        }

        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRectTransform.sizeDelta;
        //copy public params then disable existing graphic raycaster
        GraphicRaycaster defaultRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
        VRTK_UIGraphicRaycaster customRaycaster = canvas.gameObject.GetComponent<VRTK_UIGraphicRaycaster>();

        //if it doesn't already exist, add the custom raycaster
        if (customRaycaster == null) {
            customRaycaster = canvas.gameObject.AddComponent<VRTK_UIGraphicRaycaster>();
        }
        customRaycaster.enabled = true;

        MainScreenGraphicRaycaster screenRayCaster = canvas.gameObject.GetComponent<MainScreenGraphicRaycaster>();
        if (screenRayCaster == null) {
            screenRayCaster = canvas.gameObject.AddComponent<MainScreenGraphicRaycaster>();
            screenRayCaster.blockingObjects = MainScreenGraphicRaycaster.BlockingObjects.All;
        }

        if (defaultRaycaster != null && defaultRaycaster.enabled) {
            customRaycaster.ignoreReversedGraphics = defaultRaycaster.ignoreReversedGraphics;
            customRaycaster.blockingObjects = defaultRaycaster.blockingObjects;
            defaultRaycaster.enabled = false;
        }

        //add a box collider and background image to ensure the rays always hit
        //if (canvas.gameObject.GetComponent<BoxCollider>() == null) {
        //    Vector2 pivot = canvasRectTransform.pivot;
        //    float zSize = 0.1f;
        //    float zScale = zSize / canvasRectTransform.localScale.z;

        //    canvasBoxCollider = canvas.gameObject.AddComponent<BoxCollider>();
        //    canvasBoxCollider.size = new Vector3(canvasSize.x, canvasSize.y, zScale);
        //    canvasBoxCollider.center = new Vector3(canvasSize.x / 2 - canvasSize.x * pivot.x, canvasSize.y / 2 - canvasSize.y * pivot.y, zScale / 2f);
        //    canvasBoxCollider.isTrigger = true;
        //}

        draggablePanelCreation = StartCoroutine(CreateDraggablePanel(canvas, canvasSize));
        CreateActivator(canvas, canvasSize);
    }

}
