using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TMP_Text;

public class CityItem : MonoBehaviour {
    public Text label;
    public GameObject cityHighlight;
    
    private GlobeExtras globe;

    void Start() {
        globe = GetComponentInParent<GlobeExtras>();
    }

    public void SetHighlights() {
        cityHighlight.SetActive(globe.currentCity.Equals(label.text));
    }
}
