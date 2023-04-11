using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public class OverlayUI : MonoBehaviour {

    public enum Orientation {
        Bottom,
        BottomRotated,
        Top,
        TopRotated,
        LeftInside,
        LeftOutside,
        RightInside,
        RightOutside
    }
    public enum Axis {
        x,
        y,
        z,
        none
    }


    [Header("Containers")]
    public RectTransform southContainer;
    public RectTransform northContainer;
    public RectTransform westContainer;
    public RectTransform eastContainer;
    public RectTransform southGridContainer;
    public RectTransform northGridContainer;
    public RectTransform westGridContainer;
    public RectTransform eastGridContainer;
    public GameObject tickPrefab;
    public GameObject tickPrefabSide;
    public BoxCollider box;
    

    [Header("Label Flip")]
    public bool southInvert;
    public bool northInvert;
    public bool westInvert;
    public bool eastInvert;

    [Header("Offset")]
    public bool southOffset;
    public bool northOffset;
    public bool westOffset;
    public bool eastOffset;

    [Header("Container Axis")]
    public Axis southAxis;
    public Axis northAxis;
    public Axis westAxis;
    public Axis eastAxis;

    [Header("LabelOrientation")]
    public Orientation southOrientation;
    public Orientation northOrientation;
    public Orientation westOrientation;
    public Orientation eastOrientation;

    [Header("General Settings")]
    public int numHorizontalTicks = 10;
    public int numVerticalTicks = 5;
    public float gridWidth = 0.001f;


    //private RectTransform rect;
    private DataObject data;
    private Vector3 lastOffset = Vector3.zero;
    private Vector3 lastScale = Vector3.one;

    public void Start() {
        data = GetComponentInParent<DataObject>();
        if(data.isFinishedInit) {
            SetupContainers();
            SetupGrids();
        }
        else {
            data.OnInitComplete.AddListener(SetupContainers);
            data.OnInitComplete.AddListener(SetupGrids);
        }
    }

    private void Update() {
        PositionSyncContainer(southContainer, southAxis);
        PositionSyncContainer(northContainer, northAxis, (northAxis == Axis.x || northAxis == Axis.y));
        PositionSyncContainer(westContainer, westAxis, (westAxis == Axis.x || westAxis == Axis.y));
        PositionSyncContainer(eastContainer, eastAxis);

    }

    public void SetupContainers() {
        lastScale = new Vector3(1 / box.transform.localScale.x, 1 / box.transform.localScale.y, 1 / box.transform.localScale.z);
        lastOffset = box.transform.localPosition;

        SetupSingleContainer(southContainer, southAxis, southOrientation, southOffset, southInvert, false);
        SetupSingleContainer(northContainer, northAxis, northOrientation, northOffset, northInvert, (northAxis == Axis.x || northAxis == Axis.y));
        SetupSingleContainer(westContainer, westAxis, westOrientation, westOffset, westInvert, (westAxis == Axis.x || westAxis == Axis.y));
        SetupSingleContainer(eastContainer, eastAxis, eastOrientation, eastOffset, eastInvert, false);
    }

    public void SetupSingleContainer(RectTransform currentContainer, Axis axis, Orientation orientation, bool offsetDown, bool invertFace, bool invertValue) { 

        foreach (Transform child in currentContainer.transform) {
            GameObject.Destroy(child.gameObject);
        }

        if(axis == Axis.none) {
            return;
        }
        
        if(axis == Axis.x) {
            float boxMin = (box.center.x - (box.size.x / 2));
            float boxMax = (box.center.x + (box.size.x / 2));

            float dataMin = (boxMin / data.scale.x) + data.offset.x;
            float dataMax = (boxMax / data.scale.x) + data.offset.x;
            float dataDelta = (dataMax - dataMin) / numHorizontalTicks;

            float startPosition = -(currentContainer.sizeDelta.x / 2);
            float endPosition = (currentContainer.sizeDelta.x / 2);
            float tickDelta = (currentContainer.sizeDelta.x) / numHorizontalTicks;

            //for (float count = -2 * numHorizontalTicks; count <= 3 * numHorizontalTicks; count++) {
            for (float count = 0; count <= numHorizontalTicks; count++) {
                GameObject newTick;

                if (orientation == Orientation.BottomRotated || orientation == Orientation.TopRotated) {
                    newTick = Instantiate(tickPrefabSide, currentContainer);
                    newTick.transform.localEulerAngles = new Vector3(newTick.transform.localEulerAngles.x, newTick.transform.localEulerAngles.y + 90, newTick.transform.localEulerAngles.z);
                }
                else {
                    newTick = Instantiate(tickPrefab, currentContainer);
                }

                if (offsetDown && orientation == Orientation.Bottom)
                    newTick.transform.localPosition = new Vector3(newTick.transform.localPosition.x, newTick.transform.localPosition.y - 0.018f, newTick.transform.localPosition.z);
                float x = startPosition + count * tickDelta;
                float dataX = (invertValue) ? dataMin + (numHorizontalTicks - count) * dataDelta : dataMin + count * dataDelta;
                newTick.transform.localPosition = new Vector3(x, newTick.transform.localPosition.y, newTick.transform.localPosition.z);
                Text text = newTick.GetComponentInChildren<Text>();
                if (invertFace) {
                    text.transform.localEulerAngles = new Vector3(text.transform.localEulerAngles.x, text.transform.localEulerAngles.y + 180, text.transform.localEulerAngles.z);

                }
                if(orientation == Orientation.BottomRotated) {
                    text.transform.localPosition = new Vector3(60,15, text.transform.localPosition.z);
                }
                newTick.GetComponentInChildren<Text>().text = dataX.ToString();

            }
        }
        else if (axis == Axis.y) {
            float boxMin = (box.center.z - (box.size.z / 2));
            float boxMax = (box.center.z + (box.size.z / 2));

            float dataMin = (boxMin / data.scale.y) + data.offset.y;
            float dataMax = (boxMax / data.scale.y) + data.offset.y;
            float dataDelta = (dataMax - dataMin) / numHorizontalTicks;

            float startPosition = -(currentContainer.sizeDelta.x / 2);
            float endPosition = (currentContainer.sizeDelta.x / 2);
            float tickDelta = (currentContainer.sizeDelta.x) / numHorizontalTicks;

            for (float count = 0; count <= numHorizontalTicks; count++) {
                GameObject newTick;
                if (orientation == Orientation.BottomRotated || orientation == Orientation.TopRotated) {
                    newTick = Instantiate(tickPrefabSide, currentContainer);
                    newTick.transform.localEulerAngles = new Vector3(newTick.transform.localEulerAngles.x, newTick.transform.localEulerAngles.y + 90, newTick.transform.localEulerAngles.z);
                }
                else {
                    newTick = Instantiate(tickPrefab, currentContainer);
                }
                
                if (offsetDown && orientation == Orientation.Bottom)
                    newTick.transform.localPosition = new Vector3(newTick.transform.localPosition.x, newTick.transform.localPosition.y - 0.018f, newTick.transform.localPosition.z);
                float x = startPosition + count * tickDelta;
                float dataY = (invertValue) ? dataMin + (numHorizontalTicks - count) * dataDelta : dataMin + count * dataDelta;
                newTick.transform.localPosition = new Vector3(x, newTick.transform.localPosition.y, newTick.transform.localPosition.z);
                Text text = newTick.GetComponentInChildren<Text>();
                if (invertFace) {
                    text.transform.localEulerAngles = new Vector3(text.transform.localEulerAngles.x, text.transform.localEulerAngles.y + 180, text.transform.localEulerAngles.z);
                }
                if (orientation == Orientation.BottomRotated) {
                    text.transform.localPosition = new Vector3(text.transform.localPosition.x - 6, text.transform.localPosition.y + 6, text.transform.localPosition.z);
                }
                if (dataY < 0.00001f && dataY > -0.00001f) {
                    dataY = 0;
                }
                text.text = dataY.ToString();

            }
        }
        else if (axis == Axis.z){
            // Expand box clip for outside labels
            RectTransform clipRect = currentContainer.parent.GetComponent<RectTransform>();
            clipRect.sizeDelta = new Vector2(clipRect.sizeDelta.x + 0.1f, clipRect.sizeDelta.y);

            Vector3 minPoint = data.subMeshOriginal.transform.InverseTransformPoint(
                box.transform.TransformPoint((box.center - (box.size / 2)))
            );
            Vector3 maxPoint = data.subMeshOriginal.transform.InverseTransformPoint(
                box.transform.TransformPoint((box.center + (box.size / 2)))
            );

            float boxMin = minPoint.z;//(box.center.y - (box.size.y / 2)) - box.transform.localPosition.y;
            float boxMax = maxPoint.z;//(box.center.y + (box.size.y / 2)) - box.transform.localPosition.y;

            float dataMin = (boxMin / data.scale.z) + data.offset.z;
            float dataMax = (boxMax / data.scale.z) + data.offset.z;
            float dataDelta = (dataMax - dataMin) / numVerticalTicks;

            float startPosition = 0;
            float endPosition = currentContainer.sizeDelta.y;
            float tickDelta = currentContainer.sizeDelta.y / numVerticalTicks;

            for (float count = 0; count <= numVerticalTicks; count++) {
                GameObject newTick = Instantiate(tickPrefabSide, currentContainer);
                float y = startPosition + count * tickDelta;
                float x = 0;
                if(orientation == Orientation.RightOutside || orientation == Orientation.RightInside) {
                    x = currentContainer.sizeDelta.x /2;
                }
                else if (orientation == Orientation.LeftOutside || orientation == Orientation.LeftInside) {
                    x = -currentContainer.sizeDelta.x / 2;
                }
                float dataZ = (invertValue) ? dataMin + (numVerticalTicks - count) * dataDelta : dataMin + count * dataDelta;
                newTick.transform.localPosition = new Vector3(x, y, newTick.transform.localPosition.z);

                Text text = newTick.GetComponentInChildren<Text>();
                if(count != 0) {
                    text.transform.localPosition = new Vector3(text.transform.localPosition.x, 0, text.transform.localPosition.z); ;
                }
                if (invertFace) { 
                    text.transform.localEulerAngles = new Vector3(text.transform.localEulerAngles.x, text.transform.localEulerAngles.y + 180, text.transform.localEulerAngles.z);
                    text.alignment = TMPro.TextAlignmentOptions.MidlineRight;
                }
                if(dataZ < 0.00001f && dataZ > -0.00001f)
                {
                    dataZ = 0;
                }
                newTick.GetComponentInChildren<Text>().text = dataZ.ToString();

            }
        }

    }

    private void PositionSyncContainer(RectTransform currentContainer, Axis axis, bool invert = false) {
        invert = (axis == Axis.y) ? !invert : invert;
        Vector3 finalPosition = (invert) ? -box.transform.localPosition + lastOffset : box.transform.localPosition - lastOffset;
        Vector3 currentPosition = currentContainer.transform.localPosition;
        switch (axis) {
            case Axis.x:
                currentPosition.x = finalPosition.x;
                break;
            case Axis.y:
                currentPosition.x = -finalPosition.z;
                break;
            case Axis.z:
                currentPosition.y = finalPosition.y;
                break;

        }   
        currentContainer.transform.localPosition = currentPosition;

        currentContainer.transform.localScale = Vector3.Scale(box.transform.localScale, lastScale);
    }

    public void SetupGrids() {
        SetupGrid(southGridContainer);
        SetupGrid(northGridContainer);
        SetupGrid(eastGridContainer);
        SetupGrid(westGridContainer);
    }

    public void SetupGrid(RectTransform gridContainer) {

        float vertStep = gridContainer.sizeDelta.y / numVerticalTicks;
        for(int vertCount = 0; vertCount <= numVerticalTicks; vertCount++) {
            GameObject newLine = new GameObject("HorizLine"+vertCount);
            newLine.transform.SetParent(gridContainer, false);
            RectTransform newRect = newLine.AddComponent<RectTransform>();
            Image newLineImage = newLine.AddComponent<Image>();
            newRect.anchorMin = new Vector2(0, 0);
            newRect.anchorMax = new Vector2(1, 0);
            newRect.sizeDelta = new Vector2(0, gridWidth);
            newRect.localPosition = new Vector3(0, vertCount * vertStep, 0);
        }
        float horizStep = gridContainer.sizeDelta.x / numHorizontalTicks;
        for (int horizCount = 0; horizCount <= numHorizontalTicks; horizCount++) {
            GameObject newLine = new GameObject("VertLine" + horizCount);
            newLine.transform.SetParent(gridContainer, false);
            RectTransform newRect = newLine.AddComponent<RectTransform>();
            Image newLineImage = newLine.AddComponent<Image>();
            newRect.anchorMin = new Vector2(0, 0);
            newRect.anchorMax = new Vector2(0, 1);
            newRect.sizeDelta = new Vector2(gridWidth, 0);
            newRect.localPosition = new Vector3(horizCount * horizStep - gridContainer.sizeDelta.x / 2, gridContainer.sizeDelta.y/2, 0);
        }
    }
}
