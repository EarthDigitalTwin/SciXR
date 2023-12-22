using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class DataObject : MonoBehaviour {
    public enum ColorMode {
        normal,
        focus
    }
    public enum InterpMethod {
        linear,
        log
    }

    [Serializable]
    public class LoadEvent : UnityEvent<float, string> { }

    // Properties
    public BoxCollider parentCollider;
    public GameObject globeDragSync;
    public Material material;
    public Material materialUI;
    public Material materialOverlay;
    public Material wireframe;
    public DataSubMesh subMeshOriginal;
    public GameObject dataOverlay;
    public GameObject dataMeshParent;
    public GameObject overlayUI;

    // Data
    [HideInInspector] public SerialData data;

    [HideInInspector] public int vertexCount;
    [HideInInspector] public int triangleCount;
    public SerialData.DataType type;

    // Metadata properties
    [HideInInspector] public string identifier;
    [HideInInspector] public string runtimeName;
    [HideInInspector] public string fileName;
    [HideInInspector] public string filePath;
    [HideInInspector] public DateTime lastModified;
    [HideInInspector] public string notes;
    [HideInInspector] public DateTime start, end;

    //Instance properties
    [HideInInspector] public Colorbar currentColorbar;
    [HideInInspector] public bool triColorMode = false;
    [HideInInspector] public bool canSwapExtrude = true;
    [HideInInspector] public bool canSwapColor = true;
    [HideInInspector] public string currentExtrude;
    [HideInInspector] public string currentColor;
    [HideInInspector] public int currentFrame = 0;
    [HideInInspector] public InterpMethod method = InterpMethod.linear;
    [HideInInspector] public ColorMode colorMode = ColorMode.normal;

    [HideInInspector] public Vector3 offset, scale;
    [HideInInspector] public float scaleSizeMax = 0.5f;
    [HideInInspector] public float xyScale = 1;
    [HideInInspector] public float zScale = 1;

    public Mesh CurrentMesh {
        get {
            DataSubMesh subMesh = subMeshOriginal.GetComponent<DataSubMesh>();
            if (subMesh.GetComponent<MeshRenderer>() != null) {
                return subMesh.GetComponent<MeshFilter>().sharedMesh;
            }
            else {
                return subMesh.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }
        }

        set {
            DataSubMesh subMesh = subMeshOriginal.GetComponent<DataSubMesh>();
            if (subMesh.GetComponent<MeshRenderer>() != null) {
                subMesh.GetComponent<MeshFilter>().sharedMesh = value;
            }
            else {
                subMesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = value;
            }
        }
    }

    public LoadEvent OnLoadPercentChange;
    public UnityEvent OnInitComplete;
    public UnityEvent OnDestroyEvent;
    [HideInInspector] public bool isFinishedInit = false;

    // Dynamic Properties
    public bool hasExtrudeResults {
        get {
            return data.results.Count > 0 && data.results[0].vars.ContainsKey(currentExtrude);
        }
    }
    public bool hasColorResults {
        get {
            return data.results.Count > 0 && data.results[0].vars.ContainsKey(currentColor);
        }
    }

    //Convenient lookups
    [HideInInspector] public float XMax, XMin, YMax, YMin, ZMax, ZMin, ColorMax, ColorMin;

    private void OnDestroy() {
        OnDestroyEvent.Invoke();
    }


    public void RecalculateBounds() {
        XMax = data.vars[data.xIndexDefault].Max();
        XMin = data.vars[data.xIndexDefault].Min();
        YMax = data.vars[data.yIndexDefault].Max();
        YMin = data.vars[data.yIndexDefault].Min();
        if (data.results.Count > 0 && data.results[0].vars.ContainsKey(currentExtrude)) {
            ZMax = data.results[0].vars[currentExtrude].Max();
            ZMin = data.results[0].vars[currentExtrude].Min();
        }
        else if (data.vars.ContainsKey(currentExtrude)){
            ZMax = data.vars[currentExtrude].Max();
            ZMin = data.vars[currentExtrude].Min();
        }
        else {
            throw new Exception("Invalid Extrude Variable Specified");
        }

        if (data.results.Count > 0 && data.results[0].vars.ContainsKey(currentColor)) {
            ColorMax = data.results[0].vars[currentColor].Max();
            ColorMin = data.results[0].vars[currentColor].Min();
        }
        else if (data.vars.ContainsKey(currentColor)) {
            ColorMax = data.vars[currentColor].Max();
            ColorMin = data.vars[currentColor].Min();
        }
        else {
            throw new Exception("Invalid Color Variable Specified");
        }

    }

    public bool HasBackface {
        get { return type != SerialData.DataType.globe; }
    }

    public void UpdateLoadPercent(float percent, string status) {
        OnLoadPercentChange.Invoke(percent, status);
    }
    public void InitComplete() {
        isFinishedInit = true;
        OnInitComplete.Invoke();
    }

    // Methods
    //int numLoadChunks = 1;
    public SerialMesh GenerateMeshData(SerialData serialData) {
        Stopwatch watch = new Stopwatch();
        watch.Start();

        SerialMesh newSerialMesh = new SerialMesh();
        RecalculateBounds();
        //currentExtrude = newData.zIndexDefault;
        //currentColor = newData.colorIndexDefault;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector2> uv2 = new List<Vector2>();
        List<int> triangles = serialData.triangles;

        float[] x = serialData.vars[serialData.xIndexDefault];
        float[] y = serialData.vars[serialData.yIndexDefault];
        float[] z = (hasExtrudeResults) ? serialData.results[0].vars[serialData.zIndexDefault] : serialData.vars[serialData.zIndexDefault];
        float[] colors = (hasColorResults) ? serialData.results[0].vars[serialData.colorIndexDefault] : serialData.vars[serialData.colorIndexDefault];

        // Validate input array sizes
        if (x.Count() != y.Count() || x.Count() != z.Count() || y.Count() != z.Count()) {
            throw new UnityException("Input arrays do not have matching lengths");
        }

        float xDelta = XMax - XMin;
        float yDelta = YMax - YMin;
        float zDelta = ZMax - ZMin;
        zDelta = (zDelta == 0) ? 1 : zDelta;

        float zOffset = ZMin;
        float zScale = 1;
        switch (type) {
            case SerialData.DataType.flat:
                zScale = (scaleSizeMax / 6) / zDelta;
                break;
            case SerialData.DataType.globe:
                zScale = scaleSizeMax / zDelta;
                break;
            case SerialData.DataType.pointcloud:
                zScale = (scaleSizeMax / 2) / zDelta;
                break;

        }
        if (serialData.overlayX != null && serialData.overlayX.Length > 0) {
            float overlayZMin = serialData.overlayHeight.Min();
            float overlayZMax = serialData.overlayHeight.Max();
            if (overlayZMin < ZMin) {
                zOffset = overlayZMin;
                zScale = (scaleSizeMax / 4) / (overlayZMax - overlayZMin);
            }
        }

        if (type == SerialData.DataType.globe) {
            zOffset = (zDelta / 2) + ZMin;
        }

        offset = new Vector3(
            (xDelta / 2) + XMin,
            (yDelta / 2) + YMin,
            zOffset
        );

        float XYScale = (xDelta >= yDelta) ? scaleSizeMax / xDelta : scaleSizeMax / yDelta;
        scale = new Vector3(XYScale, XYScale, zScale);

        // Construct vertices and texture uv's
        for (int index = 0; index < vertexCount; index++) {
            //if (index % 1000 == 0 || index == vertexCount - 1) {
            //    ThreadManager.instance.callbacks.Add(() => {
            //        UpdateLoadPercent(0.8f + (float)index / vertexCount * 0.2f, "Generating Mesh");
            //    });
            //}
            Vector3 oldPoint = new Vector3(x[index], y[index], z[index]);
            Vector3 newPoint = oldPoint - offset;
            newPoint.Scale(scale);
            vertices.Add(newPoint);

            Vector2 newUV = new Vector2(Mathf.InverseLerp(ColorMin, ColorMax, colors[index]), 0);
            uv.Add(newUV);
            Vector2 newUV2 = new Vector2(Mathf.InverseLerp(YMin, YMax, y[index]), Mathf.InverseLerp(XMin, XMax, x[index]));
            uv2.Add(newUV2);
        }


        if (triColorMode) {
            List<Vector3> triModeVertices = new List<Vector3>();
            List<Vector3> triModeNormals = new List<Vector3>();
            List<Vector2> triModeUV = new List<Vector2>();
            List<Vector2> triModeUV2 = new List<Vector2>();
            List<int> triModeTris = new List<int>(serialData.triangles);
            List<int> newDataIndex = new List<int>();

            for (int triIndex = 0; triIndex < serialData.triangles.Count; triIndex = triIndex + 3) {
                newDataIndex.Add(serialData.triangles[triIndex]);
                newDataIndex.Add(serialData.triangles[triIndex+1]);
                newDataIndex.Add(serialData.triangles[triIndex+2]);
                triModeVertices.Add(vertices[serialData.triangles[triIndex]]);
                triModeVertices.Add(vertices[serialData.triangles[triIndex + 1]]);
                triModeVertices.Add(vertices[serialData.triangles[triIndex + 2]]);
                triModeNormals.Add(vertices[serialData.triangles[triIndex]].normalized);
                triModeNormals.Add(vertices[serialData.triangles[triIndex+1]].normalized);
                triModeNormals.Add(vertices[serialData.triangles[triIndex+2]].normalized);
                triModeUV.Add(uv[serialData.triangles[triIndex]]);
                triModeUV.Add(uv[serialData.triangles[triIndex + 1]]);
                triModeUV.Add(uv[serialData.triangles[triIndex + 2]]);
                triModeUV2.Add(uv2[serialData.triangles[triIndex]]);
                triModeUV2.Add(uv2[serialData.triangles[triIndex + 1]]);
                triModeUV2.Add(uv2[serialData.triangles[triIndex + 2]]);
            }
            vertices = triModeVertices;
            uv = triModeUV;
            uv2 = triModeUV2;
            triangles = Enumerable.Range(0, serialData.triangles.Count).ToList();
            serialData.vertexCount = triModeVertices.Count;
            newSerialMesh.normals = triModeNormals.ToArray();
            newSerialMesh.dataIndex = newDataIndex;
        }
        else {
            newSerialMesh.dataIndex = Enumerable.Range(0, serialData.vertexCount).ToList();
        }

        vertexCount = vertices.Count;
        newSerialMesh.vertices = vertices.ToArray();
        newSerialMesh.uv = uv.ToArray();
        newSerialMesh.uv2 = uv2.ToArray();
        if(triangles != null)
            newSerialMesh.triangles = triangles.ToArray();
        newSerialMesh.offset = offset;
        newSerialMesh.scale = scale;

        watch.Stop();
        Debug.Log("Main Mesh generation: "+watch.Elapsed);
        watch.Reset();
        watch.Start();
        // Adds blendshapes and blendUV's to submesh
        if (data.results != null && data.results.Count != 0) {
            ProcessResults(newSerialMesh);
        }
        watch.Stop();
        Debug.Log("Process results: " + watch.Elapsed);
        watch.Reset();
        watch.Start();
        if (data.overlayX != null && data.overlayX.Length > 0) {
            GenerateOverlay(newSerialMesh);
        }
        watch.Stop();
        Debug.Log("Overlay generation: " + watch.Elapsed);
        watch.Reset();
        watch.Start();
        if (HasBackface) {
            CreateBackFace(newSerialMesh);
        }
        watch.Stop();
        Debug.Log("Backface generation: " + watch.Elapsed);
        return newSerialMesh;
    }

    public void GenerateView(SerialMesh serialMesh) {
        DataSubMesh subMesh = subMeshOriginal.GetComponent<DataSubMesh>();
        Mesh mesh = SerialMesh.MeshDataToMesh(serialMesh, data.type != SerialData.DataType.globe && data.type != SerialData.DataType.pointcloud);

        if (data.type == SerialData.DataType.pointcloud) {
            //GetComponentInChildren<ColliderDisableInteractable>().enabled = false;
            subMesh.GetComponent<MeshCollider>().enabled = false;
            mesh.SetIndices(Enumerable.Range(0, vertexCount).ToArray(), MeshTopology.Points, 0);
            materialOverlay.SetTexture("_MainTex", new Texture2D(0, 0));
            float xTransform = 0.5f;
            float yTransform = 0.2f;
            if ((mesh.bounds.extents.x / mesh.bounds.extents.y) < 2) {
                xTransform = mesh.bounds.extents.x / (mesh.bounds.extents.x + mesh.bounds.extents.y);
                yTransform = mesh.bounds.extents.y / (mesh.bounds.extents.x + mesh.bounds.extents.y);
            }
            // Debug.Log("Data Overlay transform: " + xTransform.ToString() + " " + yTransform.ToString());
            dataOverlay.transform.localScale = new Vector3(xTransform, yTransform, -1f);
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Mesh quadMesh = gameObject.GetComponent<MeshFilter>().mesh;
            dataOverlay.GetComponent<MeshFilter>().sharedMesh = quadMesh;
            GameObject.Destroy(gameObject);
        }

        CurrentMesh = mesh;
        subMesh.dataIndex = serialMesh.dataIndex;
        subMesh.subVertexCount = vertexCount;
        subMesh.subTriangleCount = triangleCount;
        subMesh.blendUV.Add(serialMesh.uv);
        subMesh.flatUV = serialMesh.uv2.ToList();

        subMesh.GetComponent<Renderer>().sharedMaterial = material;
        dataOverlay.GetComponent<Renderer>().sharedMaterial = materialOverlay;
        GetComponentInChildren<BoxClip>().AddMaterial(material);
        GetComponentInChildren<BoxClip>().AddMaterial(materialOverlay);

        if (data.type == SerialData.DataType.globe || data.type == SerialData.DataType.pointcloud) {
            DestroyImmediate(subMeshOriginal.GetComponent<SkinnedMeshRenderer>());
            subMeshOriginal.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            subMeshOriginal.gameObject.GetComponent<MeshRenderer>().receiveShadows = false;
            subMeshOriginal.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        }

        if (data.type == SerialData.DataType.globe) {
            gameObject.GetComponent<GlobeExtras>().Init();
        }


        // Process Collider
        MeshCollider mc = subMesh.GetComponent<MeshCollider>();
        mc.sharedMesh = mesh;

        if (serialMesh.overlayVertices != null && serialMesh.overlayVertices.Length > 0) {
            dataOverlay.GetComponent<MeshFilter>().sharedMesh = SerialMesh.MeshOverlayToMesh(serialMesh);
        }

        if(data.type != SerialData.DataType.globe) {
            overlayUI.gameObject.SetActive(true);
        }
    }

    public void ProcessResults(SerialMesh mesh) {
        // Process dataContainer.results and add animations to appropriate submeshes
        
            mesh.blendShapes = new List<Vector3[]>();
            for (int resultCount = 0; resultCount < data.results.Count; resultCount++) {

                if (hasExtrudeResults || hasColorResults) {
                    List<Vector2> newUVs = new List<Vector2>();
                    List<Vector3> newVertices = new List<Vector3>();
                    float newZ, newColor;
                    Vector3 oldPoint, newPoint;
                    for (int index = 0; index < data.vertexCount; index++) {
                        newZ = (hasExtrudeResults) ? data.results[resultCount].vars[currentExtrude][index] : data.vars[currentExtrude][index];
                        newColor = (hasColorResults) ? data.results[resultCount].vars[currentColor][index] : data.vars[currentColor][index];
                        oldPoint = new Vector3(data.vars[data.xIndexDefault][index], data.vars[data.yIndexDefault][index], newZ);
                        newPoint = oldPoint - offset;
                        newPoint.Scale(scale);
                        newVertices.Add(newPoint - mesh.vertices[index]);
                        newUVs.Add(new Vector2(Mathf.InverseLerp(ColorMin, ColorMax, newColor), 0));
                    }
                    if (HasBackface) {
                        List<Vector3> newVerticesCopy = new List<Vector3>(newVertices);
                        newVertices.AddRange(newVerticesCopy);
                        List<Vector2> newUVCopy = new List<Vector2>(newUVs);
                        newUVs.AddRange(newUVCopy);
                    }
                    subMeshOriginal.blendUV.Add(newUVs.ToArray());
                    mesh.blendShapes.Add(newVertices.ToArray());
                }
                ThreadManager.instance.callbacks.Add(() => {
                    UpdateLoadPercent(0.9f + (float)resultCount / data.results.Count * 0.1f, "Generating Animation");
                });
        }
    }

    public void GenerateOverlay(SerialMesh mesh) {
        List<Vector3> vertices = new List<Vector3>();
        //List<Vector2> xyVertices = new List<Vector2>();
        List<Vector2> uvs = new List<Vector2>();

        float maxX = data.overlayX.Max();
        float minX = data.overlayX.Min();
        float maxY = data.overlayY.Max();
        float minY = data.overlayY.Min();

        // Construct vertices and texture uv's
        for (int index = 0; index < data.overlayX.Length; index++) {
            //ThreadManager.instance.callbacks.Add(() => {
            //    UpdateLoadPercent(0.5f + 2 * (0.5f / numLoadChunks) + (float)index / data.overlayX.Length * 0.5f / numLoadChunks, "Generating Overlay");
            //});
            Vector3 oldPoint = new Vector3(data.overlayX[index], data.overlayY[index], data.overlayHeight[index]);
            Vector3 newPoint = oldPoint - offset;
            newPoint.Scale(scale);
            vertices.Add(newPoint);
            Vector2 newUV = new Vector2(Mathf.InverseLerp(minX, maxX, data.overlayX[index]), Mathf.InverseLerp(minY, maxY, data.overlayY[index]));
            uvs.Add(newUV);
        }
        mesh.overlayVertices = vertices.ToArray();
        mesh.overlayTriangles = data.overlayTriangles.ToArray();//Triangulator.Triangulate(vertices);
        mesh.overlayUVs = uvs.ToArray();
    }


    // Callbacks
    public void SwapZ(string label) {
        Debug.Log("Swapping Extrude to " + label);
        currentExtrude = label;
        RecalculateBounds();

        // Reset offset and scale levels
        float ZDelta = ZMax - ZMin;
        ZDelta = (ZDelta == 0) ? 1 : ZDelta;
        float newZScale = (type != SerialData.DataType.flat) ? scaleSizeMax / (ZDelta) : (scaleSizeMax / 6) / (ZDelta);
        float zOffset = ZMin;
        if (data.overlayX != null && data.overlayX.Length > 0 && (label.ToLower().Contains("surface") || label.ToLower().Contains("base") || label.ToLower().Contains("bed"))) {
            float overlayZMin = data.overlayHeight.Min();
            float overlayZMax = data.overlayHeight.Max();
            if (overlayZMin < ZMin) {
                zOffset = overlayZMin;
                newZScale = (scaleSizeMax / 4) / (overlayZMax - overlayZMin);
            }
            dataOverlay.transform.localScale = new Vector3(1, 1, -1);
        }
        else {
            dataOverlay.transform.localScale = new Vector3(1, 1, -0.001f);
        }

        offset = new Vector3(offset.x, offset.y, zOffset);
        scale = new Vector3(scale.x, scale.y, newZScale);

        DataSubMesh subMesh = subMeshOriginal.GetComponent<DataSubMesh>();
        Mesh mesh = CurrentMesh;
        Vector3[] vertices = mesh.vertices;

        // Update mesh start
        float newZ;
        for (int index = 0; index < subMesh.subVertexCount; index++) {
            if(hasExtrudeResults && data.results.Count > 0 && data.results[0].vars.ContainsKey(label)) {
                newZ = (data.results[0].vars[currentExtrude][subMesh.dataIndex[index]] - offset.z) * scale.z;
            }
            else{
                newZ = (data.vars[currentExtrude][subMesh.dataIndex[index]] - offset.z) * scale.z;
            }

            vertices[index] = new Vector3(vertices[index].x, vertices[index].y, newZ);
            if (HasBackface)
                vertices[index + subMesh.subVertexCount] = vertices[index];
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        //mesh.RecalculateNormalsBetter(60);

        // Update blendshapes
        if (hasExtrudeResults) {
            // Copy current blend shapes before clearing
            List<Vector3[]> blendShapeVerts = new List<Vector3[]>();
            for (int frame = 0; frame < mesh.blendShapeCount; frame++) {
                Vector3[] frameVertices = (HasBackface) ? new Vector3[subMesh.subVertexCount * 2] : new Vector3[subMesh.subVertexCount];
                mesh.GetBlendShapeFrameVertices(frame, 0, frameVertices, null, null);
                blendShapeVerts.Add(frameVertices);
            }
            mesh.ClearBlendShapes();
            for (int frame = 0; frame < blendShapeVerts.Count(); frame++) {
                for (int index = 0; index < subMesh.subVertexCount; index++) {
                    //float outZ = ((dataContainer.results[frame].dataContainer.vars[currentExtrude][subMesh.dataIndex[index]] - offset.z) * scale.z) - vertices[index].z;
                    blendShapeVerts[frame][index] = new Vector3(blendShapeVerts[frame][index].x, blendShapeVerts[frame][index].y, ((data.results[frame].vars[currentExtrude][subMesh.dataIndex[index]] - offset.z) * scale.z) - vertices[index].z);
                    if (HasBackface)
                        blendShapeVerts[frame][index + subMesh.subVertexCount] = blendShapeVerts[frame][index];
                }
                mesh.AddBlendShapeFrame("Step" + frame, 100, blendShapeVerts[frame].ToArray(), null, null);
            }

        }
        CurrentMesh = mesh;
        subMesh.GetComponent<MeshCollider>().sharedMesh = mesh;

        // Updating guide bounds
        if(GetComponentInChildren<OverlayUI>() != null)
            GetComponentInChildren<OverlayUI>().SetupContainers();
        // RefreshUI
        GetComponentInChildren<MeshVRControls>().RefreshUI();
    }

    public void SwapColor(string label) {
        Debug.Log("Swapping Color to " + label);
        currentColor = label;
        RecalculateBounds();

        // Reset offset and scale levels
        DataSubMesh subMesh = subMeshOriginal.GetComponent<DataSubMesh>();
        Mesh mesh = CurrentMesh;
        Vector2[] uvs = mesh.uv;

        // Update mesh start
        float newColor;
        for (int index = 0; index < subMesh.subVertexCount; index++) {
            if (hasColorResults && data.results.Count > 0 && data.results[0].vars.ContainsKey(label)) {
                newColor = data.results[0].vars[currentColor][subMesh.dataIndex[index]];
            }
            else {
                newColor = data.vars[currentColor][subMesh.dataIndex[index]];
            }
            uvs[index] = new Vector2(Mathf.InverseLerp(ColorMin, ColorMax, newColor), 0);
            if (HasBackface)
                uvs[index + subMesh.subVertexCount] = uvs[index];
        }
        mesh.uv = uvs;
        //mesh.RecalculateNormals();
        //mesh.RecalculateNormalsBetter(60);

        subMesh.blendUV.Clear();
        subMesh.blendUV.Add(uvs);

        // Update blend colors
        if (hasColorResults) {
            for (int frame = 0; frame < data.results.Count(); frame++) {
                Vector2[] newUVs = (HasBackface) ? new Vector2[subMesh.subVertexCount * 2] : new Vector2[subMesh.subVertexCount];
                for (int index = 0; index < subMesh.subVertexCount; index++) {
                    newColor = data.results[frame].vars[currentColor][subMesh.dataIndex[index]];
                    newUVs[index] = new Vector2(Mathf.InverseLerp(ColorMin, ColorMax, newColor), 0);
                    if (HasBackface)
                        newUVs[index + subMesh.subVertexCount] = newUVs[index];
                }
                subMesh.blendUV.Add(newUVs);
            }
        }
        CurrentMesh = mesh;
        
        // RefreshUI
        GetComponentInChildren<MeshVRControls>().RefreshUI();
    }

    public void ManualSwapUV(Vector2[] newUV) {
        DataSubMesh subMesh = subMeshOriginal.GetComponent<DataSubMesh>();
        Mesh mesh = CurrentMesh;
        mesh.uv = newUV;
        CurrentMesh = mesh;
    }

    public void SetFrame(float frame) {
        DataSubMesh subMesh = subMeshOriginal.GetComponent<DataSubMesh>();
        subMesh.SetFrame(frame);
        currentFrame = (int)frame;
    }
    public void SetInterpMethod(int newMethod) {
        InterpMethod newMethodType = InterpMethod.linear;
        switch (newMethod) {
            case 0:
                newMethodType = InterpMethod.linear;
                break;
            case 1:
                newMethodType = InterpMethod.log;
                break;
        }
        materialUI.SetFloat("_EnableLog", (newMethodType == InterpMethod.log) ? 1 : 0);
        material.SetFloat("_EnableLog", (newMethodType == InterpMethod.log) ? 1 : 0);
        method = newMethodType;
    }
    public void SetColorMode(bool status) {
        ColorMode newMode = (status) ? ColorMode.normal : ColorMode.focus;
        SetColorMode(newMode);
    }
    public void SetColorMode(ColorMode newMode) {
        //ColorMode newMode = (ColorMode)System.Enum.Parse(typeof(ColorMode), newModeString);
        if(newMode == ColorMode.normal) {
            materialUI.SetFloat("_ColorMode", 1);
            material.SetFloat("_ColorMode", 1);
        }
        if(newMode == ColorMode.focus) {
            materialUI.SetFloat("_ColorMode", 0);
            material.SetFloat("_ColorMode", 0);
        }
        colorMode = newMode;
    }
    public void SetColorMin(float normalizedVal) {
        material.SetFloat("_Min", normalizedVal);
        materialUI.SetFloat("_Min", normalizedVal);
    }
    public void SetColorMax(float normalizedVal) {
        material.SetFloat("_Max", normalizedVal);
        materialUI.SetFloat("_Max", normalizedVal);
    }
    public void SetInvert(bool status) {
        material.SetFloat("_Invert", (status) ? 1 : 0);
        materialUI.SetFloat("_Invert", (status) ? 1 : 0);
    }
    public void SetDataClip(bool status) {
        material.SetFloat("_MinMaxClip", (status) ? 1 : 0);
        materialUI.SetFloat("_MinMaxClip", (status) ? 1 : 0);
    }
    public void SetColorbar(int colorbarIndex) {
        Colorbar newColorbar = DataLoader.instance.presetColorbars[colorbarIndex];
        if (newColorbar != null) {
            currentColorbar = newColorbar;
            material.SetTexture("_MainTex", newColorbar.texture);
            materialUI.SetTexture("_MainTex", newColorbar.texture);
            GetComponentInChildren<MeshVRControls>().RefreshUI();
        }
    }

    //Private methods
    private void CreateBackFace(SerialMesh meshData) {
        Vector3[] vertices = meshData.vertices;
        Vector2[] uv = meshData.uv;
        Vector2[] uv2 = meshData.uv2;
        Vector3[] normals = meshData.normals;
        //Color[] colors = mesh.colors;
        int numVerts = vertices.Length;
        Vector3[] newVertices = new Vector3[numVerts * 2];
        Vector2[] newUV = new Vector2[numVerts * 2];
        Vector2[] newUV2 = new Vector2[numVerts * 2];
        Vector3[] newNorms = new Vector3[numVerts * 2];
        //Color[] newColors = new Color[numVerts * 2];


        for (int j = 0; j < numVerts; j++) {
            // duplicate vertices colors and uvs:
            newVertices[j] = newVertices[j + numVerts] = vertices[j];
            //newColors[j] = newColors[j + numVerts] = colors[j];
            newUV[j] = newUV[j + numVerts] = uv[j];
            newUV2[j] = newUV2[j + numVerts] = uv2[j];
            if(normals != null) {
                newNorms[j] = normals[j];
                newNorms[j + numVerts] = -normals[j];
            }
        }

        int[] newTris = null;
        if (meshData.triangles != null) {
            // Double the triangles
            int[] triangles = meshData.triangles;
            int numTris = triangles.Length;
            newTris = new int[numTris * 2];

            for (int i = 0; i < numTris; i += 3) {
                // copy the original triangle
                newTris[i] = triangles[i];
                newTris[i + 1] = triangles[i + 1];
                newTris[i + 2] = triangles[i + 2];
                int j = i + numTris;
                newTris[j] = triangles[i] + numVerts;
                newTris[j + 2] = triangles[i + 1] + numVerts;
                newTris[j + 1] = triangles[i + 2] + numVerts;
            }
        }

        //Mesh newMesh = new Mesh();

        meshData.vertices = newVertices;
        meshData.uv = newUV;
        meshData.uv2 = newUV2;
        if (normals != null) 
            meshData.normals = newNorms;
        //newMesh.colors = newColors;
        meshData.triangles = newTris; // assign triangles last!


        // Do overlay backface
        if(meshData.overlayVertices != null && meshData.overlayVertices.Length > 0) {
            Vector3[] overlayVertices = meshData.overlayVertices;
            Vector2[] overlayUV = meshData.overlayUVs;
            int numOverlayVerts = overlayVertices.Length;
            Vector3[] newOverlayVertices = new Vector3[numOverlayVerts * 2];
            Vector2[] newOverlayUV = new Vector2[numOverlayVerts * 2];

            for (int j = 0; j < numOverlayVerts; j++) {
                newOverlayVertices[j] = newOverlayVertices[j + numOverlayVerts] = overlayVertices[j];
                newOverlayUV[j] = newOverlayUV[j + numOverlayVerts] = overlayUV[j];
            }
            int[] overlayTriangles = meshData.overlayTriangles;
            int numOverlayTris = overlayTriangles.Length;
            int[] newOverlayTris = new int[numOverlayTris * 2];

            for (int i = 0; i < numOverlayTris; i += 3) {
                // copy the original triangle
                newOverlayTris[i] = overlayTriangles[i];
                newOverlayTris[i + 1] = overlayTriangles[i + 1];
                newOverlayTris[i + 2] = overlayTriangles[i + 2];
                int j = i + numOverlayTris;
                newOverlayTris[j] = overlayTriangles[i] + numOverlayVerts;
                newOverlayTris[j + 2] = overlayTriangles[i + 1] + numOverlayVerts;
                newOverlayTris[j + 1] = overlayTriangles[i + 2] + numOverlayVerts;
            }
            meshData.overlayVertices = newOverlayVertices;
            meshData.overlayUVs = newOverlayUV;
            meshData.overlayTriangles = newOverlayTris;
        }
    }
}