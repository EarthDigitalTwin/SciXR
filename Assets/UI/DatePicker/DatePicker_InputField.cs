using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;

namespace UI.Dates
{
    [ExecuteInEditMode]
    public class DatePicker_InputField : MonoBehaviour
    {
        [SerializeField]
        private DateSelectionMode m_DateSelectionMode = DateSelectionMode.SingleDate;
        public DateSelectionMode DateSelectionMode
        {
            get { return m_DateSelectionMode; }
            set
            {
                SetProperty(ref m_DateSelectionMode, value);
            }
        }

        [SerializeField]
        private SerializableDate m_SelectedDate;
        public SerializableDate SelectedDate
        {
            get { return m_SelectedDate; }
            set
            {
                SetProperty(ref m_SelectedDate, value);

                if (Ref_InputField != null)
                {
                    Ref_InputField.text = (SelectedDate.HasValue) ? SelectedDate.Date.ToString(Ref_SharedDatePicker.Config.Format.DateFormat) : "";
                }

                if (Ref_SharedDatePicker.Config.Misc.CloseWhenDateSelected) HideCalendar();
            }
        }

        [SerializeField]
        private List<SerializableDate> m_SelectedDates = new List<SerializableDate>();
        public List<SerializableDate> SelectedDates
        {
            get { return m_SelectedDates; }
            set
            {
                SetProperty(ref m_SelectedDates, value);
            }
        }

        public DatePickerInputFieldConfig InputFieldConfig;
        public DatePickerEventConfig EventConfig;

        [Header("References")]
        public DatePicker Ref_SharedDatePicker;

        #pragma warning disable
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("Ref_InputField"), HideInInspector]
        private InputField Ref_OldInputField;
        #pragma warning restore

        [SerializeField]
        private TMPro.TMP_InputField m_Ref_InputField;
        public TMPro.TMP_InputField Ref_InputField
        {
            get
            {
                if (m_Ref_InputField == null && Ref_OldInputField != null)
                {
                    m_Ref_InputField = DatePicker_TextMeshProUtilities.ReplaceInputFieldWithTextMeshPro(Ref_OldInputField);
                }

                return m_Ref_InputField;
            }
        }

        public TableLayout Ref_InputFieldContainer;
        public TableCell Ref_InputFieldToggleButtonCell;

        public void UpdateDisplay()
        {
            if (Ref_InputField != null && Ref_InputFieldContainer != null && Ref_InputFieldToggleButtonCell != null)
            {
                UpdateInputFieldText();

                var valueBefore = Ref_InputFieldContainer.ColumnWidths.ToList();

                if (InputFieldConfig.ShowToggleButton)
                {
                    Ref_InputFieldContainer.ColumnWidths = new List<float> { 0, InputFieldConfig.ToggleButtonWidth };
                    Ref_InputFieldToggleButtonCell.gameObject.SetActive(true);
                }
                else
                {
                    Ref_InputFieldContainer.ColumnWidths = new List<float> { 0 };
                    Ref_InputFieldToggleButtonCell.gameObject.SetActive(false);
                }

                if (!valueBefore.SequenceEqual(Ref_InputFieldContainer.ColumnWidths)) Ref_InputFieldContainer.UpdateLayout();
            }
        }

        public void InputFieldClicked()
        {
            if (InputFieldConfig.ToggleDisplayWhenInputFieldClicked) ToggleDisplay();
        }

        public void ToggleDisplay()
        {
            if (Ref_SharedDatePicker == null)
            {
                Debug.LogError("[DatePicker_InputField] This Input Field needs 'Ref_SharedDatePicker' populated in order to function correctly. You can create a shared DatePicker using the 'DatePicker - Shared Calendar' menu item.");
                return;
            }

            if (Ref_SharedDatePicker.gameObject.activeInHierarchy)
            {
                HideCalendar();
            }
            else
            {
                ShowCalendar();
            }
        }

        void ShowCalendar()
        {
            Ref_SharedDatePicker.Ref_InputFieldContainer = Ref_InputFieldContainer;
            Ref_SharedDatePicker.Ref_InputField = Ref_InputField;
            Ref_SharedDatePicker.Ref_InputFieldToggleButtonCell = Ref_InputFieldToggleButtonCell;

            Ref_SharedDatePicker.Config.InputField = InputFieldConfig;

            Ref_SharedDatePicker.Config.Events.OnDaySelected.RemoveAllListeners();
            Ref_SharedDatePicker.Config.Events.OnDaySelected.AddListener(OnDaySelected);

            Ref_SharedDatePicker.Config.Events.OnDayMouseOver.RemoveAllListeners();
            Ref_SharedDatePicker.Config.Events.OnDayMouseOver.AddListener(OnDayMouseOver);

            Ref_SharedDatePicker.DateSelectionMode = DateSelectionMode;
            Ref_SharedDatePicker.SelectedDate = SelectedDate;
            Ref_SharedDatePicker.SelectedDates = SelectedDates;

            Ref_SharedDatePicker.IsSharedCalendar = true;

            Ref_SharedDatePicker.Show();

            DatePickerTimer.AtEndOfFrame(() =>
            {
                Ref_SharedDatePicker.UpdateDisplay();
            }, this, true);
        }

        void HideCalendar()
        {
            Ref_SharedDatePicker.Hide();
        }

        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;

            UpdateDisplay();
        }

        private void OnDaySelected(DateTime date)
        {
            if (DateSelectionMode == Dates.DateSelectionMode.SingleDate)
            {
                SelectedDate = date;
            }
            else
            {
                // let the DatePicker handle it
                SelectedDates = Ref_SharedDatePicker.SelectedDates;
            }

            if (EventConfig.OnDaySelected != null) EventConfig.OnDaySelected.Invoke(date);
        }

        private void OnDayMouseOver(DateTime date)
        {
            if (EventConfig.OnDayMouseOver != null) EventConfig.OnDayMouseOver.Invoke(date);
        }

        public void UpdateInputFieldText()
        {
            if (Ref_InputField != null)
            {
                switch (DateSelectionMode)
                {
                    case Dates.DateSelectionMode.SingleDate:
                        Ref_InputField.text = (SelectedDate.HasValue) ? SelectedDate.Date.ToString(Ref_SharedDatePicker.Config.Format.DateFormat) : "";
                        break;
                    case Dates.DateSelectionMode.MultipleDates:
                        var valueCount = SelectedDates.Count(s => s.HasValue);
                        Ref_InputField.text = ((valueCount == 1) ? SelectedDates.First(s => s.HasValue).Date.ToString(Ref_SharedDatePicker.Config.Format.DateFormat)
                                                         : (valueCount > 1 ? "Multiple Dates" : ""));
                        break;
                }
            }
        }

        private void Start()
        {
            UpdateDisplay();
        }
    }
}
