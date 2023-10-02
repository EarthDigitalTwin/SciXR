using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


public class ColliderDisableInteractable : MonoBehaviour{

    public bool disableOnTouch = false, disableOnGrab = true, disableOnSecondaryGrab = false;
    public Collider[] disabledColliders;
    
    private VRTK_InteractableObject inter;

    private void OnEnable() {
        inter = GetComponent<VRTK_InteractableObject>();
        if (disableOnTouch) {
            inter.InteractableObjectTouched += DisableColliders;
            inter.InteractableObjectUntouched += EnableCollidersOnUntouch;
        }
        if (disableOnGrab || disableOnSecondaryGrab) {
            inter.InteractableObjectGrabbed += DisableColliders;
            inter.InteractableObjectUngrabbed += EnableCollidersOnUngrab;
        }
    }

    private void OnDisable() {
        if (disableOnTouch) {
            inter.InteractableObjectTouched -= DisableColliders;
            inter.InteractableObjectUntouched -= EnableCollidersOnUntouch;
        }
        if (disableOnGrab || disableOnSecondaryGrab) {
            inter.InteractableObjectGrabbed -= DisableColliders;
            inter.InteractableObjectUngrabbed -= EnableCollidersOnUngrab;
        }
    }

    public void DisableColliders(object sender, InteractableObjectEventArgs e) {
        if(disableOnSecondaryGrab && inter.GetSecondaryGrabbingObject() != null) {
            foreach (Collider collider in disabledColliders) {
                collider.enabled = false;
            }
        }
    }
    public void EnableCollidersOnUngrab(object sender, InteractableObjectEventArgs e) {
        if(!inter.IsGrabbed()) {
            foreach (Collider collider in disabledColliders) {
                collider.enabled = true;
            }
        }
    }
    public void EnableCollidersOnUntouch(object sender, InteractableObjectEventArgs e) {
        if (!inter.IsTouched()) {
            foreach (Collider collider in disabledColliders) {
                collider.enabled = true;
            }
        }
    }
}
