
using UnityEngine;
using VRTK;
using VRTK.SecondaryControllerGrabActions;

public class ScaleGrab : VRTK_BaseGrabAction {
    [Tooltip("The distance the secondary grabbing object must move away from the original grab position before the secondary grabbing object auto ungrabs the Interactable Object.")]
    public float ungrabDistance = 1f;

    protected Vector3 initialScale;
    protected float initalLength;
    protected float initialScaleFactor;

    protected virtual void Awake() {
        //isActionable = false;
        //isSwappable = true;
    }

    public override void Initialise(VRTK_InteractableObject currentGrabbdObject, VRTK_InteractGrab currentPrimaryGrabbingObject, VRTK_InteractGrab currentSecondaryGrabbingObject, Transform primaryGrabPoint, Transform secondaryGrabPoint) {
        base.Initialise(currentGrabbdObject, currentPrimaryGrabbingObject, currentSecondaryGrabbingObject, primaryGrabPoint, secondaryGrabPoint);
        initialScale = currentGrabbdObject.transform.localScale;
        initalLength = (primaryGrabbingObject.transform.position - secondaryGrabbingObject.transform.position).magnitude;
        initialScaleFactor = currentGrabbdObject.transform.localScale.x / initalLength;

        //currentPrimaryGrabbingObject.
        //Destroy(currentGrabbdObject.GetComponent<FixedJoint>());
    }

    public override void ResetAction() {
        base.ResetAction();

    }

    /// <summary>
    /// The ProcessUpdate method runs in every Update on the Interactable Object whilst it is being grabbed by a secondary Interact Grab.
    /// </summary>
    public override void ProcessUpdate() {
        CheckForceStopDistance(ungrabDistance);
    }

    /// <summary>
    /// The ProcessFixedUpdate method runs in every FixedUpdate on the Interactable Object whilst it is being grabbed by a secondary Interact Grab and performs the scaling action.
    /// </summary>
    public override void ProcessFixedUpdate() {
        if (initialised) {
            UniformScale();
        }
    }

    protected virtual void ApplyScale(Vector3 newScale) {
        Vector3 existingScale = grabbedObject.transform.localScale;

        if (newScale.x > 0 && newScale.y > 0 && newScale.z > 0) {
            grabbedObject.transform.localScale = newScale;
            //grabbedObject.transform.localEulerAngles = new Vector3(90,180,0);
        }
    }

    protected virtual void UniformScale() {
        float adjustedLength = (primaryGrabbingObject.transform.position - secondaryGrabbingObject.transform.position).magnitude;
        float adjustedScale = initialScaleFactor * adjustedLength;

        Vector3 newScale = new Vector3(adjustedScale, adjustedScale, adjustedScale);
        ApplyScale(newScale);
    }
}
