using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;
using Dropdown = TMPro.TMP_Dropdown;

public class CitiesControls : MonoBehaviour {

    public Dropdown cityListDropdown;
    public GameObject cityItemPrefab;
    public GameObject sectionTitlePrefab;

    private string prevExtrude, prevColor;
    private DataObject data;
    private GlobeExtras globe;

    // Use this for initialization
    void OnEnable () {
        globe = GetComponentInParent<GlobeExtras>();
        data = GetComponentInParent<DataObject>();
        CreateCities();
	}
	
	// Update is called once per frame
	void Update () {
        if (prevExtrude != data.currentExtrude || prevColor != data.currentColor) {
            UpdateHighlights();
            prevExtrude = data.currentExtrude;
            prevColor = data.currentColor;
        }
    }


    public void CreateCities() {
        cityListDropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        if (globe.cities != null && globe.cities.Count > 0) {
            foreach (string city in globe.cities.Keys) {
                options.Add(new Dropdown.OptionData(city));
            }
        }
        cityListDropdown.AddOptions(options);
        cityListDropdown.value = cityListDropdown.options.IndexOf(cityListDropdown.options.Find(d =>  d.text == globe.currentCity));   
    }

    public void SetCity(int cityIndex) {
        string city = cityListDropdown.options[cityIndex].text;
        globe.currentCity = city;
        globe.ReloadCity();
    }

    public void UpdateHighlights() {
        foreach (CityItem item in GetComponentsInChildren<CityItem>()) {
            item.SetHighlights();
        }
    }

    public void UpdateDisplayMode(int type) {
        switch(type) {
            case 0:
                globe.currentDisplayMode = GlobeExtras.CityDisplayMode.LSL;
                globe.ReloadCity();
                break;
            case 1:
                globe.currentDisplayMode = GlobeExtras.CityDisplayMode.Gradient;
                globe.ReloadCity();
                break;
            case 2:
                globe.currentDisplayMode = GlobeExtras.CityDisplayMode.IceThickness;
                globe.ReloadCity();
                break;
        }
    }
}
