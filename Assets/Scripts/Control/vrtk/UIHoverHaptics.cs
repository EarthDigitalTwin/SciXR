using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VRTK;

[RequireComponent(typeof(Selectable))]
public class UIHoverHaptics : MonoBehaviour, IPointerEnterHandler  {

    //VRTK_VRInputModule inputModule;

	// Use this for initialization
	void Start () {
        //inputModule = VRTK_EventSystem.current.GetComponent<VRTK_VRInputModule>();
	}

    public void OnPointerEnter(PointerEventData eventData) {
        GameObject controller = null;
        if(eventData.pointerDrag != null) {
            //Dragging so do nothing and return;
            return;
        }

        // Identify controller that shot the event
        if(eventData.pointerId == 100) {
            controller = VRTK_DeviceFinder.GetControllerLeftHand();
        }
        else if(eventData.pointerId == 200) {
            controller = VRTK_DeviceFinder.GetControllerRightHand();
        }

        // Do rumble if controller found
        if(controller != null) {
            VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(controller), 50);
        }
    }
}
