using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;

namespace UI.Dates
{
    [ExecuteInEditMode]
    public class DatePicker_DateRange : MonoBehaviour
    {
        [SerializeField]
        private SerializableDate m_FromDate;
        public SerializableDate FromDate
        {
            get { return m_FromDate; }
            set
            {
                SetProperty(ref m_FromDate, value);

                Ref_DatePicker_From.SelectedDate = new SerializableDate(value);
            }
        }

        [SerializeField]
        private SerializableDate m_ToDate;
        public SerializableDate ToDate
        {
            get { return m_ToDate; }
            set
            {
                SetProperty(ref m_ToDate, value);

                Ref_DatePicker_To.SelectedDate = new SerializableDate(value);
            }
        }

        public DatePickerConfig Config;

        public DatePicker Ref_DatePicker_From;
        public DatePicker Ref_DatePicker_To;

        public void UpdateDisplay()
        {
            if (!this.gameObject.activeInHierarchy) return;

            m_FromDate = FromDate;
            m_ToDate = ToDate;

            ApplyConfig(Ref_DatePicker_From);
            ApplyConfig(Ref_DatePicker_To);

            if (Ref_DatePicker_From.SelectedDate.HasValue)
            {
                Ref_DatePicker_To.Config.DateRange.FromDate = Ref_DatePicker_From.SelectedDate;
                Ref_DatePicker_To.Config.DateRange.RestrictFromDate = true;
            }

            if (Ref_DatePicker_To.SelectedDate.HasValue)
            {
                Ref_DatePicker_From.Config.DateRange.ToDate = Ref_DatePicker_To.SelectedDate;
                Ref_DatePicker_From.Config.DateRange.RestrictToDate = true;
            }

            Ref_DatePicker_From.UpdateDisplay();
            Ref_DatePicker_To.UpdateDisplay();
        }

        public void InputFieldClicked()
        {
            if (Config.InputField.ToggleDisplayWhenInputFieldClicked) ToggleDisplay();
        }

        public void ToggleDisplay()
        {
            if (Ref_DatePicker_From.IsVisible || Ref_DatePicker_To.IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show()
        {
            Ref_DatePicker_From.Show();
            Ref_DatePicker_To.Show();
        }

        public void Hide()
        {
            Ref_DatePicker_From.Hide();
            Ref_DatePicker_To.Hide();
        }

        void ApplyConfig(DatePicker datePicker)
        {
            datePicker.Config = Config;

            // we manipulate the DateRange configuration object afterwards, so we need to ensure that it is a unique instance or the changes will be applied to both DatePickers
            datePicker.Config.DateRange = datePicker.Config.DateRange.Clone();

            datePicker.Config.Misc = datePicker.Config.Misc.Clone();
            datePicker.Config.Misc.CloseWhenDateSelected = false;
        }

        /// <summary>
        /// Called by the DatePicker instances
        /// </summary>
        /// <param name="date"></param>
        public void DateSelected(DateTime date)
        {
            FromDate = new SerializableDate(Ref_DatePicker_From.SelectedDate);
            ToDate = new SerializableDate(Ref_DatePicker_To.SelectedDate);

            this.UpdateDisplay();

            DatePickerTimer.DelayedCall(0.1f,
                () =>
                {
                    if (Config.Misc.CloseWhenDateSelected)
                    {
                        // Only close if we have both from and to dates selected
                        if (FromDate.HasValue && ToDate.HasValue)
                        {
                            Hide();
                        }
                    }
                },
                this);
        }

        public void ModalOverlayClicked()
        {
            if (Config.Modal.CloseWhenModalOverlayClicked) Hide();
        }

        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;

            UpdateDisplay();
        }

        /*void OnValidate()
        {
            if (!this.gameObject.activeInHierarchy) return;

            DatePickerTimer.DelayedCall(0f, () =>
            {
                FromDate = m_FromDate;
                ToDate = m_ToDate;
            }, this, false);
        }*/
    }
}
