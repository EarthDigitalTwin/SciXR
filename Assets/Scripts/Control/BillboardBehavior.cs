using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardBehavior : MonoBehaviour {

    public bool followCamera = false;

    Vector3 prevAngle = Vector3.zero;
	
	// Update is called once per frame
	void Update () {
        Vector3 current = transform.parent.eulerAngles;
        if (current == prevAngle)
            return;
        prevAngle = current;

        if(!followCamera) {
            float angledAmount = Mathf.Clamp(Mathf.DeltaAngle(current.x, 0f), 0f, 90f);
            float paddedLerp = (angledAmount / 90f) * 1.32f;
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, new Vector3(90f, 0f, 0f), paddedLerp));
        }
        else {

        }
    }
}
