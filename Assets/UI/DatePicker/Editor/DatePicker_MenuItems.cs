using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace UI.Dates
{    
    public class DatePickerMenuItems
    {
        [UnityEditor.MenuItem("GameObject/UI/DatePicker/DatePicker - Inline")]
        public static void MenuItem_NewDatePickerInline()
        {
            var datePicker = DatePicker_Instantiation.InstantiatePrefab("DatePicker - Inline").GetComponent<DatePicker>();

            DatePickerTimer.DelayedCall(0.1f, datePicker.UpdateDisplay, datePicker);
        }

        [UnityEditor.MenuItem("GameObject/UI/DatePicker/DatePicker - Popup")]
        public static void MenuItem_NewDatePickerPopup()
        {
            var datePicker = DatePicker_Instantiation.InstantiatePrefab("DatePicker - Popup").GetComponent<DatePicker>();

            DatePickerTimer.DelayedCall(0.1f, datePicker.UpdateDisplay, datePicker);
        }

        [UnityEditor.MenuItem("GameObject/UI/DatePicker/DatePicker - Date Range")]
        public static void MenuItem_NewDatePickerDateRange()
        {
            var datePicker = DatePicker_Instantiation.InstantiatePrefab("DatePicker - Date Range").GetComponent<DatePicker_DateRange>();

            DatePickerTimer.DelayedCall(0.1f, datePicker.UpdateDisplay, datePicker);
        }

        [UnityEditor.MenuItem("GameObject/UI/DatePicker/Shared Calendar/DatePicker - Popup (Shared Calendar)")]
        public static void MenuItem_NewDatePickerPopupSharedCalendar()
        {
            var datePicker = DatePicker_Instantiation.InstantiatePrefab("DatePicker - Popup (Shared Calendar)").GetComponent<DatePicker_InputField>();

            DatePickerTimer.DelayedCall(0.1f, datePicker.UpdateDisplay, datePicker);
        }

        [UnityEditor.MenuItem("GameObject/UI/DatePicker/Shared Calendar/Shared Calendar")]
        public static void MenuItem_NewDatePickerSharedCalendar()
        {
            var datePicker = DatePicker_Instantiation.InstantiatePrefab("DatePicker - Shared Calendar").GetComponent<DatePicker>();

            DatePickerTimer.DelayedCall(0.1f, datePicker.UpdateDisplay, datePicker);
        }
    }
}
