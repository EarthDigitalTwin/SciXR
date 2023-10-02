using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxClip : MonoBehaviour {

    public List<Material> m_GridMaterial;
    public BoxCollider box;

    // Use this for initialization
    void OnEnable () {
        //box = GetComponent<BoxCollider>();
        InitMaterial();
    }
	
	// Update is called once per frame
	void LateUpdate () {
        for (int index = 0; index < m_GridMaterial.Count; index++) {
            m_GridMaterial[index].SetMatrix("_ClipMatrix", transform.worldToLocalMatrix);
            m_GridMaterial[index].SetVector("_ClipExtents", box.size / 2);
            m_GridMaterial[index].SetVector("_ClipCenter", box.center);
        }

    }

    public void InitMaterial() {
        for (int index = 0; index < m_GridMaterial.Count; index++) {
            m_GridMaterial[index].SetVector("_ClipExtents", box.size / 2);
            m_GridMaterial[index].SetVector("_ClipCenter", box.center);
        }
    }

    private void OnDisable() {
        for (int index = 0; index < m_GridMaterial.Count; index++) {
            m_GridMaterial[index].SetVector("_ClipExtents", Vector3.positiveInfinity);
            m_GridMaterial[index].SetVector("_ClipCenter", Vector3.zero);
        }

    }

    public void AddMaterial(Material mat) {
        m_GridMaterial.Add(mat);
        InitMaterial();
    }
}
