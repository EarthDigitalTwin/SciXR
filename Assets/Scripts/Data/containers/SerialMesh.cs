using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerialMesh {
    public string name;
    public Vector3[] vertices;
    public Vector3[] normals;
    public int[] triangles;
    public Vector2[] uv;
    public Vector2[] uv2;
    public Vector3 offset;
    public Vector3 scale;
    public List<Vector3[]> blendShapes;

    public Vector3[] overlayVertices;
    public int[] overlayTriangles;
    public Vector2[] overlayUVs;

    public List<int> dataIndex;

    public static Mesh MeshDataToMesh(SerialMesh meshData) {
        return MeshDataToMesh(meshData, true);
    }
    public static Mesh MeshDataToMesh(SerialMesh meshData, bool markDynamic) {
        Mesh mesh = new Mesh();
        if(markDynamic)
            mesh.MarkDynamic();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.name = meshData.name;
        mesh.vertices = meshData.vertices;
        mesh.uv = meshData.uv;
        mesh.triangles = meshData.triangles;
        if (meshData.normals != null && meshData.normals.Length > 0) {
            mesh.normals = meshData.normals;
        }
        else {
            mesh.RecalculateNormals();
        }
        //Add blend shapes
        if (meshData.blendShapes != null && meshData.blendShapes.Count > 0) {
            for (int frame = 0; frame < meshData.blendShapes.Count; frame++) {
                mesh.AddBlendShapeFrame("Frame " + frame, 100, meshData.blendShapes[frame], null, null);
            }
        }

        return mesh;
    }

    public static Mesh MeshOverlayToMesh(SerialMesh meshData) {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.name = meshData.name + "_overlay";
        mesh.vertices = meshData.overlayVertices;
        mesh.uv = meshData.overlayUVs;
        mesh.triangles = meshData.overlayTriangles;
        if(meshData.normals != null && meshData.normals.Length > 0) {
            mesh.normals = meshData.normals;
        }
        else {
            mesh.RecalculateNormals();
        }
        return mesh;
    }

    public static SerialMesh MeshToMeshData(Mesh mesh) {
        return null;
    }

}
