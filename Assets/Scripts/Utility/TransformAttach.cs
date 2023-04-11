using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAttach : MonoBehaviour {

    public enum OnUpdate {
        Update,
        LateUpdate,
        FixedUpdate
    }

    public Transform attachedTransform;
    public Transform scaleMirror;

    public OnUpdate useUpdate = OnUpdate.Update;
    public bool syncPosition = true;
    public bool syncRotation = true;
    public bool syncScale = true;
    public bool attach = true;

    Vector3 originalOffset;
    //Vector3 originalScale;
    //Quaternion originalRotation;

    public void Attach() {
        attach = true;
        InitAttach();
    }

    public void Detach() {
        attach = false;
    }

    void Start() {
        if(attachedTransform != null)
            Attach();
    }

    void Update() {
        if(attach && useUpdate == OnUpdate.Update) {
            SetTransform();
        }
    }

    void LateUpdate() {
        if (attach && useUpdate == OnUpdate.LateUpdate) {
            SetTransform();
        }
    }

    void FixedUpdate() {
        if (attach && useUpdate == OnUpdate.FixedUpdate) {
            SetTransform();
        }
    }

    void InitAttach() {
        originalOffset = attachedTransform.position - transform.position;
        //originalRotation = transform.rotation;
        //originalScale = transform.localScale;
    }

    void SetTransform () {
        if(syncPosition) {
            transform.position = attachedTransform.position;// + originalOffset;
        }
        if(syncRotation) {
            transform.rotation = attachedTransform.rotation;// * originalRotation;
        }
        if (syncScale ) {
            if(scaleMirror == null)
                transform.localScale = attachedTransform.localScale;
            else
                transform.localScale = scaleMirror.localScale;
        }
	}
}
