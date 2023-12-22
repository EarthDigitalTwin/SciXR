using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinimumSlider : Slider
{
	public MaximumSlider other;

	protected override void Set(float input, bool sendCallback)
	{
		float newValue = input;

		if (wholeNumbers)
		{
			newValue = Mathf.Round(newValue);
		}
        float compareVal = other.value - (maxValue - minValue) * .03f;

        if (newValue >= compareVal) {
            input = compareVal;
        }

        base.Set(input, sendCallback);
	}
}
