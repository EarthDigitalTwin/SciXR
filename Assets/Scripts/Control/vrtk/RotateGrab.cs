using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/// <summary>
/// Event Payload
/// </summary>
/// <param name="interactingObject">The GameObject that is performing the interaction (e.g. a controller).</param>
/// <param name="currentAngle">The current angle the Interactable Object is rotated to.</param>
/// <param name="normalizedAngle">The normalized angle (between `0f` and `1f`) the Interactable Object is rotated to.</param>
/// <param name="rotationSpeed">The speed in which the rotation is occuring.</param>
public struct RotateTransformGrabAttachEventArgs {
    public GameObject interactingObject;
    public Vector3 currentAngle;
    public Vector3 normalizedAngle;
    public Vector3 rotationSpeed;
}

/// <summary>
/// Event Payload
/// </summary>
/// <param name="sender">this object</param>
/// <param name="e"><see cref="RotateTransformGrabAttachEventArgs"/></param>
public delegate void RotateTransformGrabAttachEventHandler(object sender, RotateTransformGrabAttachEventArgs e);

public class RotateGrab : VRTK.GrabAttachMechanics.VRTK_BaseGrabAttach {

    public LineRenderer line;

    GameObject currentGrabbing;

    protected override void Initialise() {
    }

    public override bool StartGrab(GameObject grabbingObject, GameObject givenGrabbedObject, Rigidbody givenControllerAttachPoint) {
        currentGrabbing = grabbingObject;
        if(line != null) {
            line.gameObject.SetActive(true);
        }
        return base.StartGrab(grabbingObject, givenGrabbedObject, givenControllerAttachPoint);
    }

    public override void StopGrab(bool applyGrabbingObjectVelocity) {
        currentGrabbing = null;
        if (line != null) {
            line.gameObject.SetActive(false);
        }
        base.StopGrab(applyGrabbingObjectVelocity);
    }
    public override void ProcessUpdate() {
        if(currentGrabbing != null) {
            if (line != null) {
                line.SetPosition(1, line.transform.InverseTransformPoint(currentGrabbing.transform.position));
            }
            transform.rotation = Quaternion.LookRotation(transform.position - currentGrabbing.transform.position);
        }
    }
}
