using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MinMaxSlider : MonoBehaviour {
    public Slider minSlider;
    public Slider maxSlider;
    public RectTransform fill;
    public RectTransform minDragArea;
    public RectTransform maxDragArea;

    public Slider.SliderEvent onMinChange;
    public Slider.SliderEvent onMaxChange;
    
    private void Start() {
        minSlider.onValueChanged.AddListener(val => { onMinChange.Invoke(val); OnSliderChange(); });
        maxSlider.onValueChanged.AddListener(val => { onMaxChange.Invoke(val); OnSliderChange(); });
        onMinChange.Invoke(0);
        onMaxChange.Invoke(1);
        OnSliderChange();
    }

    public void OnSliderChange() {
        float center = (minSlider.normalizedValue + maxSlider.normalizedValue) / 2;
        minDragArea.anchorMax = new Vector2(center, minDragArea.anchorMax.y);
        maxDragArea.anchorMin = new Vector2(center, maxDragArea.anchorMin.y);


        fill.anchorMin = new Vector2(minSlider.normalizedValue, fill.anchorMin.y);
        fill.anchorMax = new Vector2(maxSlider.normalizedValue, fill.anchorMax.y);
        
    }
}