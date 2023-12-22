using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UI.Dates
{
    [CustomEditor(typeof(DatePicker)), CanEditMultipleObjects]
    public class DatePickerEditor : Editor
    {
        public void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            var datePicker = ((DatePicker)target);
            if (GUILayout.Button("Force Update"))
            {
                datePicker.InvalidateAllDayButtonTemplates();

                var rows = datePicker.Ref_DayTable.Rows;
                foreach (var row in rows)
                {
                    DestroyImmediate(row.gameObject);
                }

                datePicker.UpdateDisplay();
            }

            if (GUILayout.Button("Invalidate Button Templates"))
            {
                datePicker.InvalidateAllDayButtonTemplates();
            }

            // create a copy of the configuration object
            DatePickerConfig configBefore = JsonUtility.FromJson<DatePickerConfig>(datePicker.GetSerializedConfiguration());

            if (DrawDefaultInspector())
            {
                var weekDayConfigChanged = (JsonUtility.ToJson(configBefore.WeekDays) != JsonUtility.ToJson(datePicker.Config.WeekDays));

                if (weekDayConfigChanged) datePicker.ClearWeekDayHeaders();

                var buttonConfigChanged = (JsonUtility.ToJson(configBefore.Days) != JsonUtility.ToJson(datePicker.Config.Days));

                if (buttonConfigChanged)
                {
                    datePicker.InvalidateAllDayButtonTemplates();
                }
                else
                {
                    datePicker.UpdateDisplay();
                }
            }
        }
    }
}
