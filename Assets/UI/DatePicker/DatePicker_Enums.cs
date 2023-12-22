using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;

namespace UI.Dates
{
    public enum Animation
    {
        None,
        Slide,
        Fade
    }    

    public enum DatePickerPosition
    {
        Below,
        Above
    }

    public enum AnimationType
    {
        Show,
        Hide
    }

    public enum Alignment
    {
        Left,
        Center,
        Right
    }

    public enum VisibleDateDefaultBehaviour
    {
        UseStoredValue,        
        UseTodaysDate
    }

    public enum DateSelectionMode
    {
        SingleDate,
        MultipleDates
    }

    public enum WeekNumberMode
    {
        WeekOfMonth,
        WeekOfYear
    }

    public enum DatePickerDayButtonType
    {
        Today,
        SelectedDay,
        CurrentMonth,
        OtherMonths
    }
}
