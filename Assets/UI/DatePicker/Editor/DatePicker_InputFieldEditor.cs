using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UI.Dates
{
    [CustomEditor(typeof(DatePicker_InputField)), CanEditMultipleObjects]
    public class DatePicker_InputFieldEditor : Editor
    {
        public void OnEnable()
        {            
        }

        public override void OnInspectorGUI()
        {
            var datePickerInputField = ((DatePicker_InputField)target);
            if (GUILayout.Button("Force Update"))
            {
                datePickerInputField.UpdateDisplay();
            }           

            if (DrawDefaultInspector())
            {
                datePickerInputField.UpdateDisplay();
                DatePickerTimer.DelayedCall(0.005f, datePickerInputField.UpdateDisplay, datePickerInputField);
            }
        }        
    }
}
