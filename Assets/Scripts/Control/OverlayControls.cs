using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayControls : MonoBehaviour
{
    public PinchSlider slider;

    public DataObject data;

    public float minHeight = 0;
    public float maxHeight = 1;

    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        data = GetComponentInParent<DataObject>();
    }
    #endregion

    public void UpdateOverlayHeight()
    {
        float loc = slider.SliderValue;
        float scaledLoc = loc * maxHeight + minHeight;
        data.SetOverlayZ(scaledLoc);
    }
}
