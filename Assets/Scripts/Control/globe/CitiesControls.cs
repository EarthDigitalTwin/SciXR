using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CitiesControls : MonoBehaviour {

    public ARDropdown cityListDropdown;
    public GameObject cityItemPrefab;
    public GameObject sectionTitlePrefab;
    public GameObject dropdownOptionPrefab;
    public GameObject cityOptionContainer;

    public ARDropdown typeDropdown;

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
        float yCurr = 20;
        float yDelta = 20;
        int selected = 0;

        if (globe.cities != null && globe.cities.Count > 0) {
            int counter = 0;
            foreach (string city in globe.cities.Keys) {
                if (globe.currentCity == city)
                {
                    selected = counter;
                }
                counter++;

                GameObject option = Instantiate(dropdownOptionPrefab, cityOptionContainer.transform);
                option.name = city;
                option.transform.GetChild(2).GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = city;
                option.transform.localPosition = new Vector3(0, yCurr, 0);
                yCurr -= yDelta;

                cityListDropdown.options.Add(option);
                cityListDropdown.checks.Add(option.transform.GetChild(2).GetChild(1).gameObject);
                option.SetActive(true);
            }

            cityListDropdown.Start();
        }
        
    }

    public void SetCity() {
        int cityIndex = cityListDropdown.selected;
        string city = cityListDropdown.options[cityIndex].name;
        globe.currentCity = city;
        globe.ReloadCity();
    }

    public void UpdateHighlights() {
        foreach (CityItem item in GetComponentsInChildren<CityItem>()) {
            item.SetHighlights();
        }
    }

    public void UpdateDisplayMode() {
        int type = typeDropdown.selected;
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
