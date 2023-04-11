using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointerHandVRControls : MonoBehaviour {
    /*
    public float hapticWaitTime = 0.1f;
    private float timeSinceLastHapticPulse = 0;
    private VRTK_UIPointer UIPointer;
    private VRTK_StraightPointerRenderer straightPointer;
    private Camera vrCamera;

    void Start() {
        vrCamera = GameObject.FindGameObjectWithTag("VRCanvasCamera").GetComponent<Camera>();
        UIPointer = GetComponent<VRTK_UIPointer>();
        straightPointer = GetComponent<VRTK_StraightPointerRenderer>();
        UIPointer.UIPointerElementEnter += UIHaptics;
        UIPointer.SelectionButtonPressed += FreezeCamera; ;
        UIPointer.SelectionButtonReleased += UnFreezeCamera;
        GetComponent<VRTK_InteractGrab>().ControllerGrabInteractableObject += DisablePointer;
        GetComponent<VRTK_InteractGrab>().ControllerUngrabInteractableObject += EnablePointer;

        //Button action assignments
        GetComponent<VRTK_ControllerEvents>().ButtonTwoPressed += TopButtonPressed;
    }
    
    private void FreezeCamera(object sender, ControllerInteractionEventArgs e) {
        vrCamera.transform.parent = null;
    }
    private void UnFreezeCamera(object sender, ControllerInteractionEventArgs e) {
        vrCamera.transform.SetParent(UIPointer.customOrigin, false);
        vrCamera.transform.localPosition = Vector3.zero;
        vrCamera.transform.localEulerAngles = Vector3.zero;
    }

    void Update() {
        CanvasSwitch();
    }

    private void DisablePointer(object sender, ObjectInteractEventArgs e) {
        UIPointer.enabled = false;
        straightPointer.cursorVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff;
        straightPointer.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff;
    }

    private void EnablePointer(object sender, ObjectInteractEventArgs e) {
        UIPointer.enabled = true;
        straightPointer.cursorVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
        straightPointer.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
        UnFreezeCamera(null, new ControllerInteractionEventArgs());
    }

    private void CanvasSwitch() {
        RaycastHit hitInfo;
        if (Physics.Raycast(UIPointer.customOrigin.position, UIPointer.customOrigin.forward, out hitInfo, 5)) {
            Canvas currentCanvas = hitInfo.collider.GetComponentInParent<Canvas>();
            if (currentCanvas != null && currentCanvas.worldCamera != vrCamera) {
                currentCanvas.worldCamera = vrCamera;
            }
        }
    }

    private void UIHaptics(object sender, UIPointerEventArgs e) {
        if(Time.time > timeSinceLastHapticPulse + hapticWaitTime && e.currentTarget.GetComponent<Selectable>() != null) {
            VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(this.gameObject), 500);
            timeSinceLastHapticPulse = Time.time;
        }
    }

    void TopButtonPressed(object sender, ControllerInteractionEventArgs e) {
            ScreenshotControls.instance.TakeScreenshot();
    }
    */
}
