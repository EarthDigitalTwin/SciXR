using UnityEngine;
using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace UI.Dates
{
    public static class DatePickerCache
    {
        internal static Dictionary<string, bool> _DateFallsWithinMonthResults = new Dictionary<string, bool>();
    }  
}
