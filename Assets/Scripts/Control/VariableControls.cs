﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public class VariableControls : MonoBehaviour {

    public GameObject variableListContainer;
    public GameObject variableItemPrefab;
    public GameObject sectionTitlePrefab;

    private string prevExtrude, prevColor;
    private DataObject data;

    // Use this for initialization
    void Start () {
        data = GetComponentInParent<DataObject>();
        if(data.isFinishedInit) {
            CreateVariables();
        }
        else {
            data.OnInitComplete.AddListener(CreateVariables);
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (prevExtrude != data.currentExtrude || prevColor != data.currentColor) {
            UpdateHighlights();
            prevExtrude = data.currentExtrude;
            prevColor = data.currentColor;
        }
    }


    public void CreateVariables() {
        List<string> animationVariables = new List<string>();
        Transform parent = variableListContainer.GetComponentInChildren<VerticalLayoutGroup>().transform;

        if(GetComponentInParent<DataObject>().data.results.Count > 0) {
            GameObject newTitle = Instantiate(sectionTitlePrefab, parent);
            newTitle.SetActive(true);
            newTitle.GetComponent<Text>().text = "ANIMATION VARIABLES";
            foreach (string variable in GetComponentInParent<DataObject>().data.results[0].vars.Keys) {
                animationVariables.Add(variable);
                GameObject newVar = Instantiate(variableItemPrefab, parent);
                newVar.SetActive(true);
                newVar.GetComponentInChildren<Text>().text = variable;
                Button colorButton = newVar.transform.Find("ColorButton").GetComponent<Button>();
                Button extrudeButton = newVar.transform.Find("MeshButton").GetComponent<Button>();
                if (!data.canSwapColor)
                    colorButton.gameObject.SetActive(false);
                if (!data.canSwapExtrude)
                    extrudeButton.gameObject.SetActive(false);
                colorButton.onClick.AddListener(() => { data.SwapColor(variable); });
                extrudeButton.onClick.AddListener(() => { data.SwapZ(variable); });
            }
        }

        GameObject newAllTitle = Instantiate(sectionTitlePrefab, parent);
        newAllTitle.SetActive(true);
        newAllTitle.GetComponent<Text>().text = "ALL VARIABLES";
        foreach (string variable in GetComponentInParent<DataObject>().data.vars.Keys) {
            if(!animationVariables.Contains(variable)) {
                GameObject newVar = Instantiate(variableItemPrefab, parent);
                newVar.SetActive(true);
                newVar.GetComponentInChildren<Text>().text = variable;
                Button colorButton = newVar.transform.Find("ColorButton").GetComponent<Button>();
                Button extrudeButton = newVar.transform.Find("MeshButton").GetComponent<Button>();
                if (!data.canSwapColor)
                    colorButton.gameObject.SetActive(false);
                if (!data.canSwapExtrude)
                    extrudeButton.gameObject.SetActive(false);
                colorButton.onClick.AddListener(() => { data.SwapColor(variable); });
                extrudeButton.onClick.AddListener(() => { data.SwapZ(variable); });
            }
        }

        float totalHeight = 0;
        for (int count = 0; count < variableListContainer.transform.childCount; count++) {
            totalHeight += variableListContainer.transform.GetChild(count).GetComponent<RectTransform>().sizeDelta.y;
        }
        variableListContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, totalHeight);

    }

    public void UpdateHighlights() {
        foreach(VariableItem item in GetComponentsInChildren<VariableItem>()) {
            item.SetHighlights();
        }
    }
}
