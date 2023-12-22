using UnityEngine;
using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Dates
{
    public static class DatePickerExtensions
    {
        public static void Apply(this ColorBlock colors, ColorBlock newColors)
        {
            colors.colorMultiplier = newColors.colorMultiplier;
            colors.disabledColor = newColors.disabledColor;
            colors.fadeDuration = newColors.fadeDuration;
            colors.highlightedColor = newColors.highlightedColor;
            colors.normalColor = newColors.normalColor;
            colors.pressedColor = newColors.pressedColor;
        }
    }
}
