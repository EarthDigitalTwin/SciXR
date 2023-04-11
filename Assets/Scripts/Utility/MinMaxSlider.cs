using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.UI;

public class MinMaxSlider : MonoBehaviour {
    public PinchSlider minSlider;
    public PinchSlider maxSlider;
    public RectTransform fill;
    //public RectTransform minDragArea;
    //public RectTransform maxDragArea;

    public MinMaxEvent onMinChange;
    public MinMaxEvent onMaxChange;

    [System.Serializable]
    public class MinMaxEvent: UnityEvent<float> { }
    
    private void Start() {
        minSlider.OnValueUpdated.AddListener(val => { onMinChange.Invoke(minSlider.SliderValue); OnSliderChange(); });
        maxSlider.OnValueUpdated.AddListener(val => { onMaxChange.Invoke(maxSlider.SliderValue); OnSliderChange(); });
        onMinChange.Invoke(0);
        onMaxChange.Invoke(1);
        OnSliderChange();
    }

    public void OnSliderChange() {
        float center = (minSlider.SliderValue + maxSlider.SliderValue) / 2;
        //minDragArea.anchorMax = new Vector2(center, minDragArea.anchorMax.y);
        //maxDragArea.anchorMin = new Vector2(center, maxDragArea.anchorMin.y);


        fill.anchorMin = new Vector2(minSlider.SliderValue, fill.anchorMin.y);
        fill.anchorMax = new Vector2(maxSlider.SliderValue, fill.anchorMax.y);
        
    }
}