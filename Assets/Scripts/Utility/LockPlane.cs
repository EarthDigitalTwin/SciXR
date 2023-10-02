using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using VRTK;

public class LockPlane : MonoBehaviour {

    [Header("Freeze Local Position")]
    public bool x;
    public bool y;
    public bool z;

    public float zOffset = 0;

    Vector3 ogPos; 

    private void Start() {
        ogPos = transform.localPosition;
    }

    private void Update() {
        float x, y, z;
        
        x = (this.x) ? ogPos.x : transform.localPosition.x;
        y = (this.y) ? ogPos.y + zOffset : transform.localPosition.y;
        z = (this.z) ? ogPos.z : transform.localPosition.z;
        transform.localPosition = new Vector3(x, y, z);
    }
}
