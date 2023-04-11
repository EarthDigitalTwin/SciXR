using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataInstanceRatio : MonoBehaviour
{
    public GameObject basePlate;
    public GameObject infoBackground;
    public GameObject frontFaceBackground;
    public OverlayUI overlayUI;
    public BoxCollider filterBox;
    public GameObject basePlatePrefab;
    public GameObject baseContainer;
    public GameObject grabCollider;

    public void SetAspectRatio(float xRatio, float yRatio)
    {
        // Determine a whole number ratio to use
        xRatio = Mathf.Round(xRatio / yRatio);
        yRatio = 1 / xRatio;
        xRatio = 1;

        //gameObject.GetComponent<DataObject>().scale = new Vector3(1, 1, 1 / yRatio);

        // Scale the base plate
        Vector3 center = basePlate.transform.position;
        basePlate.transform.localScale *= yRatio;

        Vector3 infoOffset = infoBackground.transform.position - center;
        Vector3 frontOffset = frontFaceBackground.transform.position - center;

        infoOffset *= yRatio;
        frontOffset *= yRatio;

        Vector3 infoPosition = new Vector3(
            center.x + infoOffset.x, 
            infoBackground.transform.position.y, 
            center.z + infoOffset.z
        );
        Vector3 frontPosition = new Vector3(
            center.x + frontOffset.x,
            frontFaceBackground.transform.position.y,
            center.z + frontOffset.z
        );

        infoBackground.transform.parent.position = infoPosition;
        frontFaceBackground.transform.parent.position = frontPosition;

        // Scale the bounding box
        Vector3 boxScale = filterBox.size;
        boxScale.z *= yRatio;
        filterBox.size = boxScale;

        Vector3 grabScale = grabCollider.gameObject.transform.localScale;
        grabScale.z *= yRatio;
        grabCollider.gameObject.transform.localScale = grabScale;

        Vector3 overlayScale = overlayUI.gameObject.transform.localScale;
        overlayScale.z *= yRatio;
        overlayUI.gameObject.transform.localScale = overlayScale;

        Vector3 filterScale = filterBox.gameObject.transform.localScale;
        filterScale.z *= yRatio;
        filterBox.gameObject.transform.localScale = filterScale;

        // Scale the base plate
        // Assume the x ratio is 1 and y ratio is inverse of a whole number
        int yRatioInverse = (int)(1 / yRatio);
        basePlate.SetActive(false);

        // Instantiate one of the base plate squares for each part of the y ratio
        //bool oddRatio = yRatioInverse % 2 == 0;
        float basePlateLength = basePlate.GetComponent<MeshRenderer>().bounds.size.x * 0.7f;
        float halfIdx = (float)(yRatioInverse - 1) / 2.0f;
        for (int i = 0; i < yRatioInverse; i++)
        {
            GameObject baseTile = Instantiate(basePlatePrefab, baseContainer.transform);
            Vector3 tileLoc = baseTile.transform.localPosition;
            tileLoc.x += basePlateLength * (i - halfIdx);
            baseTile.transform.localPosition = tileLoc;
            baseTile.transform.localScale *= yRatio;
            //baseTile.transform.localPosition = new Vector3(baseTile.transform.localPosition.x, baseTile.transform.localPosition.y, 0);
        }
    }
}
