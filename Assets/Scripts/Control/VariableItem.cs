using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TMP_Text;

public class VariableItem : MonoBehaviour {
    public Text label;
    public GameObject colorHighlight;
    public GameObject meshHighlight;

    private DataObject data;

    void Start() {
        data = GetComponentInParent<DataObject>();
    }

    public void SetHighlights() {
        colorHighlight.SetActive(data.currentColor.Equals(label.text));
        meshHighlight.SetActive(data.currentExtrude.Equals(label.text));
    }
}
