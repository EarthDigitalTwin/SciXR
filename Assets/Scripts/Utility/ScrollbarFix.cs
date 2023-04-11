using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollbarFix : MonoBehaviour {
    RectTransform rect;


	// Use this for initialization
	void Start () {
        rect = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
        if(rect.anchoredPosition.x != 0)
            rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y);
	}
}
