using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;
using Dropdown = TMPro.TMP_Dropdown;

public class MeshVRControls : MonoBehaviour {
    [Header("Containers")]
    public GameObject animationContainer;
    public GameObject earthLocator;
    public GameObject frontUI;
    public GameObject meshDragSync;
    public BoxCollider meshDragFill;
    public GameObject[] emissionObjects;

    [Header("Main Window References")]
    public Button animationButton;
    public Slider animationSlider;
    public Text nameLabel;
    public Text metadataText;
    public Text animationText;
    public Text currentExtrude;
    public Text currentColor;
    public Image mapBounds;
    //public Text xyScaleText;
    //public Text zScaleText;
    //public Text zOffsetText;
    //public Slider xyScaleSlider;
    //public Slider zScaleSlider;

    [Header("Color References")]
    public Dropdown colorbarDropdown;
    public RawImage colorbarPreview;
    public RawImage colorbarScalePreview;
    public Slider colorModeMinSlider;
    public Slider colorModeMaxSlider;

    [Header("Other Windows")]
    public CanvasGroup variablesCanvas;
    public CanvasGroup colormapCanvas;
    public CanvasGroup vizOptionsCanvas;
    public CanvasGroup legendCanvas;


    [HideInInspector]
    public DataObject data;
    [HideInInspector]
    public List<Material> emissionMaterials = new List<Material>();

    //public float zScaleOffset {
    //    get {
    //        return zScaleSlider.value;
    //    }
    //}
    //public float xyScaleOffset {
    //    get {
    //        return xyScaleSlider.value;
    //    }
    //}


    // Use this for initialization
    void Start () {
		data = GetComponentInParent<DataObject>();
        foreach (GameObject obj in emissionObjects) {
            Material mat = obj.GetComponent<Renderer>().material;
            if (mat != null)
                emissionMaterials.Add(mat);
        }
        // Refresh UI with minimum info then again after init complete
        RefreshUI();
        data.OnInitComplete.AddListener(UpdateColorbarDropdown);
        data.OnInitComplete.AddListener(RefreshUI);
	}

    public void UpdateColorbarDropdown() {
        colorbarDropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        
        foreach (Colorbar colorbar in DataLoader.instance.presetColorbars) {
            Sprite newSprite = Sprite.Create(colorbar.texture as Texture2D, new Rect(0.0f, 0.0f, colorbar.texture.width, colorbar.texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            options.Add(new Dropdown.OptionData(colorbar.name, newSprite));
        }
        colorbarDropdown.AddOptions(options);
    }

    public void RefreshUI() {
        // Set top label
        nameLabel.text = data.identifier;

        // Load bbox to map preview
        if (data.data.projection == null)
        {
            data.data.projection = "0";
        }
        if (data.identifier == "Columbia07") // for demo
        {
            data.data.projection = "32406";
        }
        if (data.data.vars.Count > 0 && data.data.projection != "0")
        {
            float[] xVals = data.data.vars[data.data.xIndexDefault];
            float[] yVals = data.data.vars[data.data.yIndexDefault];
            OverlayLoader.BBox bbox = OverlayLoader.GetObjectBounds(xVals, yVals, data.data.projection);
            double height = bbox.maxY - bbox.minY;
            double width = bbox.maxX - bbox.minX;
            mapBounds.rectTransform.sizeDelta = new Vector2((float)(width * (217f / 360f)), (float)(height * (108f / 180f)));
            mapBounds.transform.localPosition = new Vector3((float)((bbox.maxX + bbox.minX) / 2) * (217f / 360f), (float)((bbox.maxY + bbox.minY) / 2) * (108f / 180f), 0);
        }


        // Set info texts
        int numVerts = data.vertexCount;
        int numTriangles = data.triangleCount;
        int numSteps = 0;
        string hasBackface = (data.type != SerialData.DataType.globe) ? "Yes" : "No";

        bool animateExtrude = false, animateColor = false;
        if (data.data.results != null && data.data.results.Count > 0) {
            animateExtrude = data.hasExtrudeResults;
            animateColor = data.hasColorResults;
            numSteps = data.data.results.Count;
        }

        string hasAnimation = "No";
        if (animateExtrude && animateColor) {
            hasAnimation = "Extrude and Color";
        }
        else if (animateColor) {
            hasAnimation = "Color";
        }
        else if (animateExtrude) {
            hasAnimation = "Extrude";
        }

        //metadataText.text =
        //    "<b>Number of Vertices:</b> " + numVerts + "\n" +
        //    "<b>Number of Triangles:</b> " + numTriangles + "\n" +
        //    "<b>Has Backface:</b> " + hasBackface + "\n" +
        //    "<b>Has Animation:</b> " + hasAnimation + "\n" +
        //    "<b>Number of Result Steps:</b> " + numSteps + "\n";

        // Set main window variable labels
        if (data.data.type == SerialData.DataType.pointcloud) {
            currentExtrude.text = (data.data.z_label == "") ? "Loading..." : data.data.z_label;
            currentColor.text = (data.data.variable == "") ? "Loading..." : data.data.variable;
        }
        else {
            currentExtrude.text = (data.currentExtrude == "") ? "Loading..." : data.currentExtrude;
            currentColor.text = (data.currentColor == "") ? "Loading..." : data.currentColor;
        }


        // Set slider
        if (numSteps > 0 && (animateExtrude || animateColor)) {
            animationButton.gameObject.SetActive(true);
            //animationSlider.transform.parent.gameObject.SetActive(true);
            animationSlider.maxValue = numSteps - 1;
            animationSlider.onValueChanged.Invoke(0);
        } else {
            animationButton.gameObject.SetActive(false);
            //animationSlider.transform.parent.gameObject.SetActive(false);
        }

        //Set colorbar preview
        colorbarPreview.material = data.materialUI;
        colorbarPreview.texture = data.currentColorbar.texture;
        colorbarScalePreview.material = data.materialUI;
        colorbarScalePreview.texture = data.currentColorbar.texture;

        //Finalize legend
        UpdateLegend();
    }

    private void UpdateLegend() {
        RawImage colorbarImage = legendCanvas.GetComponentInChildren<RawImage>();
        colorbarImage.texture = data.currentColorbar.texture;
        colorbarImage.material = data.materialUI;

        Text[] tickvals = legendCanvas.transform.Find("Ticks").GetComponentsInChildren<Text>();
        Text title = legendCanvas.transform.Find("Title").GetComponent<Text>();
        Text uom = legendCanvas.transform.Find("UOM").GetComponent<Text>();
        title.SetText(data.identifier);
        uom.SetText("No Units Specified");

        //InputField units = GameObject.Find("Units")?.GetComponent<InputField>();
        //InputField units1 = InputField.FindWithTag("Units");
        // GameObject units2 = GameObject.FindWithTag("Units");
        // Text units1 = units2.GetComponentInChildren<Text>();

        // Debug.Log("units2: " + units2);
        // Debug.Log("units1: " + units1 + "| " + units1.text);
        // uom.SetText(units1.text);

        float min = data.ColorMin;
        float max = data.ColorMax;
        GlobeExtras ge = GetComponentInParent<GlobeExtras>();
        if(ge != null && ge.cities != null && ge.cities.Count > 0) {
            if(ge.currentDisplayMode == GlobeExtras.CityDisplayMode.Gradient) {
                min = -3;
                max = 3;
                uom.SetText("dS/dH");
            }
            if (ge.currentDisplayMode == GlobeExtras.CityDisplayMode.LSL) {
                min = -0.5f;
                max = 0.5f;
                uom.SetText("dS/dH*ΔH (10<sup>-3</sup>μm)");
            }
            if (ge.currentDisplayMode == GlobeExtras.CityDisplayMode.IceThickness) {
                min = -0.2f;
                max = 0.2f;
                uom.SetText("ΔH (m/yr)");
            }
        }

        if (data.method == DataObject.InterpMethod.linear) {
            float dist = (max - min) / (tickvals.Length - 1);
            for (int i = 0; i < tickvals.Length; i++) {
                float val = (dist * i) + min;
                tickvals[tickvals.Length - 1 - i].SetText(val.ToString("0.00"));
            }
        }
        else if (data.method == DataObject.InterpMethod.log) {
            float dist = (Mathf.Log10(max) - Mathf.Log10(min)) / (tickvals.Length - 1);
            for (int i = 0; i < tickvals.Length; i++) {
                float val = Mathf.Pow(10, (dist * i) + Mathf.Log10(min));
                tickvals[tickvals.Length - 1 - i].SetText(val.ToString("0.00"));
            }
        }

        
    }

    //
    // Callbacks
    //

    // Effects Callbacks

    public void LockBaseToggle(bool status) {
        if(status) {
            foreach (Material mat in emissionMaterials) {
                mat.SetColor("_EmissionColor", Color.red);
            }
        }
        else {
            foreach (Material mat in emissionMaterials) {
                mat.SetColor("_EmissionColor", Color.white);
            }
        }
    }

    // Viz Options

    public void ToggleWireframe(bool status) {
        if(status) {
            foreach(SkinnedMeshRenderer rend in data.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                rend.sharedMaterial = data.wireframe;
            }
        }
        else {
            foreach (SkinnedMeshRenderer rend in data.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                rend.sharedMaterial = data.material;
            }
        }
    }

    //Main Menu Options

    public void DeleteMesh() {
        DataLoader loader = FindObjectOfType<DataLoader>();
        loader.RemoveMesh(transform.parent.gameObject);
    }

    public void ToggleCanvas(CanvasGroup canvas) {
        if (!canvas.gameObject.activeSelf) {
            canvas.gameObject.SetActive(true);
            if(canvas.CompareTag("DockableVRMenu")) {
                canvas.transform.position = frontUI.transform.position;
                canvas.transform.rotation = frontUI.transform.rotation;
                canvas.transform.Translate(new Vector3(0, 0.1f, -0.05f));
                canvas.transform.eulerAngles = new Vector3(0, canvas.transform.eulerAngles.y, canvas.transform.eulerAngles.z);
            }
        }
        else {
            if (!canvas.CompareTag("DockableVRMenu")) {
                canvas.gameObject.SetActive(false);
            }
        }
    }

    //public void SetMeshDragSyncXYScale(float scale) {
    //    data.xyScale = scale;
    //    xyScaleText.text = "XY-Scale <b><color=#A2A500FF>" + scale.ToString("0.00") + "</color></b>";
    //}
    //public void SetMeshDragSyncZScale(float scale) {
    //    data.zScale = scale;
    //    zScaleText.text = "Z-Scale <b><color=#A2A500FF>" + scale.ToString("0.00") + "</color></b>";
    //}
    //public void SetMeshDragSyncZOffset(float offset) {
    //    meshDragSync.GetComponent<LockPlane>().zOffset = offset;
    //    zOffsetText.text = "Z-Offset <b><color=#A2A500FF>" + offset.ToString("0.00") + "</color></b>";
    //}

    public void SetDataInstanceScale(float scale) {
        data.transform.localScale = new Vector3(scale, scale, scale);
    }

    // Colormap Callbacks
    //public void ToggleColormode(bool mode) {
    //    data.SetColorMode(mode);
    //    if(mode) {
    //        colorModeMinSlider.gameObject.SetActive(true);
    //        colorModeMaxSlider.gameObject.SetActive(true);
    //        colorModeFocusSlider.gameObject.SetActive(false);
    //        colorModeRangeSlider.gameObject.SetActive(false);

    //        colorModeMinSlider.onValueChanged.Invoke(colorModeMinSlider.value);
    //        colorModeMaxSlider.onValueChanged.Invoke(colorModeMaxSlider.value);
    //    }
    //    else {
    //        colorModeMinSlider.gameObject.SetActive(false);
    //        colorModeMaxSlider.gameObject.SetActive(false);
    //        colorModeFocusSlider.gameObject.SetActive(true);
    //        colorModeRangeSlider.gameObject.SetActive(true);

    //        colorModeFocusSlider.onValueChanged.Invoke(colorModeFocusSlider.value);
    //        colorModeRangeSlider.onValueChanged.Invoke(colorModeRangeSlider.value);
    //    }
    //}
}
