using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UI.Dates
{
    [ExecuteInEditMode]
    public class ExampleController : MonoBehaviour
    {        

        public DatePicker InlineDatePicker;
        
        public void ToggleNextPreviousMonthButtons(bool on)
        {            
            InlineDatePicker.Config.Header.ShowNextAndPreviousMonthButtons = on;
            InlineDatePicker.UpdateDisplay();
        }

        public void ToggleNextPreviousYearButtons(bool on)
        {
            InlineDatePicker.Config.Header.ShowNextAndPreviousYearButtons = on;
            InlineDatePicker.UpdateDisplay();
        }

        public void ToggleWeekNumberDisplay(bool on)
        {
            InlineDatePicker.Config.WeekDays.ShowWeekNumbers = on;
            InlineDatePicker.UpdateDisplay();
        }

        public void ToggleShowDatesInOtherMonths(bool on)
        {
            InlineDatePicker.Config.Misc.ShowDatesInOtherMonths = on;
            InlineDatePicker.UpdateDisplay();
        }

        public void ToggleAllowMultipleDateSelection(bool on)
        {
            InlineDatePicker.DateSelectionMode = on ? DateSelectionMode.MultipleDates : DateSelectionMode.SingleDate;
            InlineDatePicker.UpdateDisplay();
        }        
    }
}
