using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UI.Dates
{
    [CustomEditor(typeof(DatePicker_DateRange)), CanEditMultipleObjects]
    public class DatePicker_DateRangeEditor : Editor
    {
        public void OnEnable()
        {            
        }

        public override void OnInspectorGUI()
        {
            var datePicker = ((DatePicker_DateRange)target);
            if (GUILayout.Button("Force Update"))
            {
                datePicker.UpdateDisplay();
            }           

            if (DrawDefaultInspector())
            {
                datePicker.UpdateDisplay();
                DatePickerTimer.DelayedCall(0.005f, datePicker.UpdateDisplay, datePicker);
            }
        }        
    }
}
