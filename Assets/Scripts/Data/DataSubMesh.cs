using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;
using VRTK;
using System;

public class DataSubMesh : MonoBehaviour,  IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler{
    public class PointerStatus {
        public enum PointerType {
            Camera,
            VR
        }
        public string name;
        public bool enabled;
        public TooltipBehavior tooltip;
        
        public PointerType type;
        public Camera screenCamera;
        public VRTK_Pointer vrPointer;
        public VRTK_ControllerEvents vrEvents;
    }
    
    ///
    /// Properties
    ///
    public GameObject tooltipsContainer;
    public GameObject infoDisplayPrefab;
    [HideInInspector] public bool hasBackface;
    // Original data index lookup
    [HideInInspector]
    public List<int> dataIndex;
    [HideInInspector]
    public List<Vector2[]> blendUV = new List<Vector2[]>();
    [HideInInspector]
    public List<Vector2> flatUV;
    [HideInInspector]
    public int subVertexCount;
    [HideInInspector]
    public int subTriangleCount;

    Renderer rend;
    DataObject data;
    new MeshCollider collider;
    VRTK_InteractableObject io;
    List<VRTK_Pointer> pointers;
    Dictionary<string, PointerStatus> pointerStatus = new Dictionary<string, PointerStatus>();

    void Start() {
        rend = GetComponent<Renderer>();
        collider = GetComponent<MeshCollider>();
        data = GetComponentInParent<DataObject>();
        io = GetComponent<VRTK_InteractableObject>();
        pointers = DataLoader.instance.vrPointers;
        if (pointers != null) {
            foreach (VRTK_Pointer pointer in pointers) {
                pointer.DestinationMarkerEnter += OnVRPointerEnter;
                pointer.DestinationMarkerExit += OnVRPointerExit;
                //pointer.DestinationMarkerHover += OnVRPointerHover;
                pointer.SelectionButtonPressed += OnVRSelectionPressed;
                    
            }
        }
        infoDisplayPrefab.SetActive(false);
    }
    
    private void OnDestroy() {
        if(pointers != null) {
            foreach (VRTK_Pointer pointer in pointers) {
                pointer.DestinationMarkerEnter -= OnVRPointerEnter;
                pointer.DestinationMarkerExit -= OnVRPointerExit;
                //pointer.DestinationMarkerHover -= OnVRPointerHover;
                pointer.SelectionButtonPressed -= OnVRSelectionPressed;
            }
        }
    }

    private void Update() {
        foreach (KeyValuePair<string, PointerStatus> pointer in pointerStatus) {
            if (pointer.Value.enabled) {
                ProcessPointer(pointer.Value);
            }
        }
    }

    public void SetFrame(float frame) {
        if (data.hasExtrudeResults) {
            AnimateToBlendShape(frame);
        }
        if (data.hasColorResults) {
            AnimateToBlendColor(frame);
        }
    }

    public void AnimateToBlendShape(float frame) {
        
        SkinnedMeshRenderer skinRend = (SkinnedMeshRenderer)rend;

        frame = frame - 1;
        int floor = Mathf.FloorToInt(frame);
        float ratioToLow = frame - floor;
        float ratioToHigh = 1 - ratioToLow;

        int stepCount = skinRend.sharedMesh.blendShapeCount;
        for (int count = 0; count < stepCount; count++) {
            if (count == floor) {
                skinRend.SetBlendShapeWeight(count, ratioToHigh * 100);
            }
            else if (count == floor + 1) {
                skinRend.SetBlendShapeWeight(count, ratioToLow * 100);
            }
            else {
                skinRend.SetBlendShapeWeight(count, 0);
            }
        }
        //rend.sharedMesh.RecalculateNormals();
    }

    public void AnimateToBlendColor(float frame) {
        SkinnedMeshRenderer skinRend = (SkinnedMeshRenderer)rend;

        int floor = Mathf.FloorToInt(frame);
        float ratio = 1 - (frame - floor);
        Mesh mesh = skinRend.sharedMesh;
        mesh.uv = blendUV[floor];
        mesh.uv2 = (frame < data.data.results.Count()) ? blendUV[floor + 1] : blendUV[floor];
        rend.sharedMaterial.SetFloat("_AniLerp", ratio);
    }

    // Data Lookup from pointer callbacks


    private void OnVRPointerEnter(object sender, DestinationMarkerEventArgs e) {
        if (e.target == this.transform) {
            PointerStatus currentPointer;
            string name = e.controllerReference.scriptAlias.name;
            if (!pointerStatus.ContainsKey(name)) {
                currentPointer = new PointerStatus();
                currentPointer.name = name;
                currentPointer.type = PointerStatus.PointerType.VR;
                currentPointer.vrPointer = e.controllerReference.scriptAlias.GetComponent<VRTK_Pointer>();
                currentPointer.vrEvents = e.controllerReference.scriptAlias.GetComponent<VRTK_ControllerEvents>();
                currentPointer.tooltip = Instantiate(infoDisplayPrefab, tooltipsContainer.transform).GetComponent<TooltipBehavior>();
                currentPointer.tooltip.gameObject.SetActive(true);
                currentPointer.enabled = true;
                pointerStatus.Add(currentPointer.name, currentPointer);
            }
            else {
                currentPointer = pointerStatus[name];
                currentPointer.tooltip.gameObject.SetActive(true);
                currentPointer.enabled = true;
            }
            ProcessPointer(currentPointer);
        }
    }
    private void OnVRPointerExit(object sender, DestinationMarkerEventArgs e) {
        if (e.target == this.transform) {
            if (pointerStatus.ContainsKey(e.controllerReference.scriptAlias.name)) {
                pointerStatus[e.controllerReference.scriptAlias.name].tooltip.gameObject.SetActive(false);
                pointerStatus[e.controllerReference.scriptAlias.name].enabled = false;
            }
        }
    }
    //private void OnVRPointerHover(object sender, DestinationMarkerEventArgs e) {
    //    if (e.target == this.transform) {
    //        if (pointerStatus.ContainsKey(e.controllerReference.scriptAlias.name)) {
    //            Vector3 lookAtVector = pointerStatus[e.controllerReference.scriptAlias.name].tooltip.canvasGroup.transform.position - pointerStatus[e.controllerReference.scriptAlias.name].vrPointer.customOrigin.position;
    //            pointerStatus[e.controllerReference.scriptAlias.name].tooltip.canvasGroup.transform.rotation = Quaternion.LookRotation(lookAtVector);
    //        }
    //    }
    //}

    private void OnVRSelectionPressed(object sender, ControllerInteractionEventArgs e) {
        PointerStatus pointer = pointerStatus[e.controllerReference.scriptAlias.name];
        Ray newRay = new Ray(pointer.vrPointer.customOrigin.position, pointer.vrPointer.customOrigin.forward);
        RaycastHit newHit;
        if (collider.Raycast(newRay, out newHit, 1000)) {
            GameObject copiedTooltip = Instantiate(pointer.tooltip.gameObject, pointer.tooltip.transform.parent);
            copiedTooltip.GetComponentInChildren<Collider>().enabled = true;
            copiedTooltip.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        PointerStatus currentPointer;
        string name = eventData.pointerCurrentRaycast.module.name;
        if (!pointerStatus.ContainsKey(name)) {
            currentPointer = new PointerStatus();
            currentPointer.name = name;
            currentPointer.type = PointerStatus.PointerType.Camera;
            currentPointer.screenCamera = eventData.enterEventCamera;
            currentPointer.tooltip = Instantiate(infoDisplayPrefab, tooltipsContainer.transform).GetComponent<TooltipBehavior>();
            currentPointer.tooltip.gameObject.SetActive(true);
            currentPointer.enabled = true;
            pointerStatus.Add(currentPointer.name, currentPointer);
        }
        else {
            currentPointer = pointerStatus[name];
            currentPointer.tooltip.gameObject.SetActive(true);
            currentPointer.enabled = true;
        }
        ProcessPointer(currentPointer);
    }

    public void OnPointerExit(PointerEventData eventData) {

        if (pointerStatus.ContainsKey(eventData.pointerCurrentRaycast.module.name)) {
            pointerStatus[eventData.pointerCurrentRaycast.module.name].tooltip.gameObject.SetActive(false);
            pointerStatus[eventData.pointerCurrentRaycast.module.name].enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left && pointerStatus.ContainsKey(eventData.pointerCurrentRaycast.module.name)) {
            GameObject copiedTooltip = Instantiate(pointerStatus[eventData.pointerCurrentRaycast.module.name].tooltip.gameObject, pointerStatus[eventData.pointerCurrentRaycast.module.name].tooltip.transform.parent);
            copiedTooltip.GetComponentInChildren<Collider>().enabled = true;
        }
    }

    RaycastHit hitInfo;
    Ray ray;
    public void ProcessPointer(PointerStatus pointer) {
        if(collider == null) {
            collider = GetComponent<MeshCollider>();
        }
        if(pointer.type == PointerStatus.PointerType.Camera) {
            ray = pointer.screenCamera.ScreenPointToRay(Input.mousePosition);
        }
        else if (pointer.type == PointerStatus.PointerType.VR) {
            ray = new Ray(pointer.vrPointer.customOrigin.position, pointer.vrPointer.customOrigin.forward);
        }
        if (collider.Raycast(ray, out hitInfo, 10)) {
            pointer.tooltip.transform.position = hitInfo.point;
            SetInfo(hitInfo.triangleIndex, hitInfo.point, pointer.tooltip); 

            if(data.type == SerialData.DataType.globe) {
                pointer.tooltip.transform.rotation = Quaternion.LookRotation(hitInfo.point - transform.position);
                pointer.tooltip.transform.localEulerAngles = new Vector3(pointer.tooltip.transform.localEulerAngles.x + 90, pointer.tooltip.transform.localEulerAngles.y, pointer.tooltip.transform.localEulerAngles.z);
            }

            Vector3 lookAtVector = Vector3.zero;
            if (pointer.type == PointerStatus.PointerType.Camera) {
                lookAtVector = pointer.tooltip.canvasGroup.transform.position - pointer.screenCamera.transform.position;
            }
            else if(pointer.type == PointerStatus.PointerType.VR) {
                lookAtVector = pointer.tooltip.canvasGroup.transform.position - pointer.vrPointer.customOrigin.position;
            }
            pointer.tooltip.canvasGroup.transform.rotation = Quaternion.LookRotation(lookAtVector);
        }
        else {
            //Missed an onpointerexit event
            pointer.tooltip.gameObject.SetActive(false);
            pointer.enabled = false;
        }
    }

    // SetInfo optimization params
    private Vector3[] vertices;
    private int[] triangles;
    private float[] p_dist = new float[3];
    private int offset;
    private int indexFinalPoint;
    private int indexSubMeshPoint;
    private float finalX, finalY, finalZ, finalColorValue;
    private Vector3 translatedHitPosition;

    public void SetInfo(int triangleIndex, Vector3 hitPosition, TooltipBehavior tooltip) {
        if (vertices == null || triangles == null) {
            vertices = collider.sharedMesh.vertices;
            triangles = collider.sharedMesh.triangles;
        }
        translatedHitPosition = transform.InverseTransformPoint(hitPosition);

        p_dist[0] = (translatedHitPosition - vertices[triangles[triangleIndex * 3]]).sqrMagnitude;
        p_dist[1] = (translatedHitPosition - vertices[triangles[triangleIndex * 3 + 1]]).sqrMagnitude;
        p_dist[2] = (translatedHitPosition - vertices[triangles[triangleIndex * 3 + 2]]).sqrMagnitude;
        offset = 0;
        if (p_dist[1] > p_dist[0] && p_dist[1] > p_dist[2])
            offset = 1;
        if (p_dist[2] > p_dist[0] && p_dist[2] > p_dist[1])
            offset = 2;


        indexSubMeshPoint = triangles[triangleIndex * 3 + offset];
        if (indexSubMeshPoint >= data.vertexCount) {
            indexSubMeshPoint = indexSubMeshPoint - data.vertexCount;
        }
        indexFinalPoint = dataIndex[indexSubMeshPoint];
        finalX = data.data.vars[data.data.xIndexDefault][indexFinalPoint];
        finalY = data.data.vars[data.data.yIndexDefault][indexFinalPoint];
        finalZ = (data.hasExtrudeResults) ? data.data.results[data.currentFrame].vars[data.currentExtrude][indexFinalPoint] : data.data.vars[data.currentExtrude][indexFinalPoint];
        finalColorValue = (data.hasColorResults) ? data.data.results[data.currentFrame].vars[data.currentColor][indexFinalPoint] : data.data.vars[data.currentColor][indexFinalPoint];
        
        if(data.type == SerialData.DataType.flat) {
            tooltip.labelText.text = data.currentColor+":\n" +
                data.currentExtrude + ":\n\n" +
                "X:\n" +
                "Y:\n";

            tooltip.valueText.text = finalColorValue.ToString() + "\n"
                + finalZ.ToString() + "\n\n"
                + finalX.ToString() + "\n"
                + finalY.ToString() + "\n";
        }
        else if (data.type == SerialData.DataType.globe) {
            finalX = data.data.vars["mesh.lat"][indexFinalPoint];
            finalY = data.data.vars["mesh.long"][indexFinalPoint];
            tooltip.labelText.text = "Color:\n\n" +
                "Latitude:\n" +
                "Longitude:\n";

            tooltip.valueText.text = finalColorValue.ToString() + "\n\n"
                + finalX.ToString() + "\n"
                + finalY.ToString() + "\n";
        }
        else {
            tooltip.labelText.text = "Color:\n\n" +
                "X:\n" +
                "Y:\n" +
                "Z:\n";

            tooltip.valueText.text = finalColorValue.ToString() + "\n\n"
                + finalX.ToString() + "\n"
                + finalY.ToString() + "\n"
                + finalZ.ToString() + "\n";
        }

    }

}
