using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;

namespace UI.Dates
{
    [ExecuteInEditMode]
    public class DatePicker : MonoBehaviour
    {
        #region Dates
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

                // This will update the VisibleDate field to match the selected date, ensuring that the currently visible month always matches the selected date
                // (when the date is selected)
                // This is only relevant when selecting dates that don't fall within the current month
                if (Config.Misc.SwitchToSelectedMonthWhenDateSelected)
                {
                    VisibleDate = value;
                }

                UpdateInputFieldText();
                if (Config.Misc.CloseWhenDateSelected) Hide();
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

        [SerializeField]
        private SerializableDate m_VisibleDate;
        public SerializableDate VisibleDate
        {
            get
            {
                if (!m_VisibleDate.HasValue)
                {
                    if (SelectedDate.HasValue)
                    {
                        m_VisibleDate = new SerializableDate(SelectedDate.Date);
                    }
                    else
                    {
                        m_VisibleDate = new SerializableDate(DateTime.Today);
                    }
                }

                return m_VisibleDate;
            }
            set { SetProperty(ref m_VisibleDate, value); }
        }

        [Tooltip("Defines how 'VisibleDate' is calculated, if at all. Only used if Selected Date has no value.")]
        public VisibleDateDefaultBehaviour VisibleDateDefaultBehaviour = VisibleDateDefaultBehaviour.UseTodaysDate;

        public bool IsSharedCalendar { get; set; }
        #endregion

        #region Config
        public DatePickerConfig Config;
        #endregion

        #region References
        [Header("References")]
        public RectTransform Ref_DatePickerTransform;
        public DatePicker_Header Ref_Header;

        public TableLayout Ref_DayTable;
        public DatePicker_Animator Ref_DayTableAnimator;
        public TableCell Ref_DayTableContainer;

        public DatePicker_DayHeader Ref_Template_DayName;
        public DatePicker_DayButton Ref_Template_Day_CurrentMonth;
        public DatePicker_DayButton Ref_Template_Day_OtherMonths;
        public DatePicker_DayButton Ref_Template_Day_Today;
        public DatePicker_DayButton Ref_Template_Day_SelectedDay;

        public Image Ref_Border;
        public DatePicker_ContentLayout Ref_ContentLayout;

        public Image Ref_ScreenOverlay;
        public DatePicker_Animator Ref_ScreenOverlayAnimator;

        public DatePicker_Animator Ref_Animator;

        private GameObject Panel_BlockRaycasts;

        // Optional
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
            set
            {
                m_Ref_InputField = value;
            }
        }


        public TableLayout Ref_InputFieldContainer;
        public TableCell Ref_InputFieldToggleButtonCell;

        public DatePicker_DateRange Ref_DatePicker_DateRange;
        #endregion

        [SerializeField]
        private DatePicker_DayButton_Pool _buttonPool;
        private DatePicker_DayButton_Pool buttonPool
        {
            get
            {
                if (_buttonPool == null) _buttonPool = GetComponent<DatePicker_DayButton_Pool>();
                if (_buttonPool == null) _buttonPool = gameObject.AddComponent<DatePicker_DayButton_Pool>();

                return _buttonPool;
            }
        }

        public RectTransform Ref_Viewport = null;

        public bool IsVisible
        {
            get
            {
                return Ref_DatePickerTransform.gameObject.activeInHierarchy;
            }
        }

        private bool m_initialized = false;
        private bool m_updateScheduled = false;

        void Awake()
        {
#if UNITY_EDITOR && UNITY_2019_2_OR_NEWER
            if (UnityEditor.PrefabUtility.IsOutermostPrefabInstanceRoot(this.gameObject))
            {
                UnityEditor.PrefabUtility.UnpackPrefabInstance(this.gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
            }
#endif

            ClearWeekDayHeaders();
            buttonPool.InvalidateAll();

            foreach (var row in Ref_DayTable.Rows)
            {
                DestroyImmediate(row.gameObject);
            }

            // If any buttons already exist (most likely created in edit mode)
            // Then add them to our pool rather than creating new ones unnecessarily
            var existingDayButtons = Ref_DatePickerTransform.GetComponentsInChildren<DatePicker_DayButton>();
            foreach (var button in existingDayButtons)
            {
                buttonPool.AddExistingButton(button);
            }

            if (!SelectedDate.HasValue)
            {
                if (VisibleDateDefaultBehaviour == Dates.VisibleDateDefaultBehaviour.UseTodaysDate)
                {
                    VisibleDate = DateTime.Today;
                }
            }
        }

        void Start()
        {
            //UpdateDisplay();
            //DatePickerTimer.DelayedCall(0, UpdateDisplay, this);
            SetupHoldButtons();
        }

        void SetupHoldButtons()
        {
            if (!Application.isPlaying) return;

            var buttons = new DatePicker_Button[] { Ref_Header.NextMonthButton, Ref_Header.PreviousMonthButton, Ref_Header.NextYearButton, Ref_Header.PreviousYearButton };

            foreach (var button in buttons)
            {
                button.gameObject.AddComponent<DatePicker_HoldButton>();
            }
        }

        /// <summary>
        /// Enable the DatePicker if it has been disabled via Disable()
        /// </summary>
        public void Enable()
        {
            if (Panel_BlockRaycasts != null) Panel_BlockRaycasts.SetActive(false);
        }

        /// <summary>
        /// Disable (but not hide) the DatePicker
        /// </summary>
        public void Disable()
        {
            if (Panel_BlockRaycasts == null)
            {
                Panel_BlockRaycasts = new GameObject("Panel - Block Raycasts", typeof(RectTransform), typeof(Image));
                Panel_BlockRaycasts.transform.SetParent(this.transform);
                Panel_BlockRaycasts.transform.SetAsLastSibling();

                var rt = Panel_BlockRaycasts.GetComponent<RectTransform>();
                rt.anchorMax = Vector2.one;
                rt.anchorMin = Vector2.zero;
                rt.anchoredPosition3D = Vector3.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;

                var image = Panel_BlockRaycasts.GetComponent<Image>();
                image.color = Color.clear;
                image.raycastTarget = true;
            }

            Panel_BlockRaycasts.SetActive(true);
        }

        void OnEnable()
        {
            if (!m_initialized) DatePickerTimer.AtEndOfFrame(UpdateDisplay, this);
        }

        public void ShowPreviousMonth()
        {
            VisibleDate = VisibleDate.Date.AddMonths(-1);
            MonthChangedUpdateDisplay();
        }

        public void ShowNextMonth()
        {
            VisibleDate = VisibleDate.Date.AddMonths(1);
            MonthChangedUpdateDisplay();
        }

        public void ShowPreviousYear()
        {
            VisibleDate = VisibleDate.Date.AddYears(-1);
            MonthChangedUpdateDisplay();
        }

        public void ShowNextYear()
        {
            VisibleDate = VisibleDate.Date.AddYears(1);
            MonthChangedUpdateDisplay();
        }

        void MonthChangedUpdateDisplay()
        {
            if (Config.Animation.MonthChangedAnimation == Animation.None)
            {
                UpdateDisplay();
                return;
            }

            Ref_DayTableAnimator.PlayAnimation(Config.Animation.MonthChangedAnimation,
                                               AnimationType.Hide,
                                               () =>
                                               {
                                                   UpdateDisplay();

                                                   Ref_DayTableAnimator.PlayAnimation(Config.Animation.MonthChangedAnimation, AnimationType.Show);
                                               });
        }

        private void Update()
        {
            if (m_updateScheduled) _UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            m_updateScheduled = true;
        }

        private void _UpdateDisplay()
        {
            // don't do anything if we aren't actually active in the hierarchy
            // (basically, we're either inactive or a prefab)
            if (!this.gameObject.activeInHierarchy) return;
            if (!m_updateScheduled) return;

            m_updateScheduled = false;

            m_initialized = true;

            if (Config.Sizing.OverrideTransformHeight)
            {
                Ref_DatePickerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Config.Sizing.PreferredHeight);
            }

            UpdateBorder();
            UpdateHeader();
            UpdateWeekDayHeaders();
            UpdateDaySection();

            // Free all buttons in the pool
            buttonPool.FreeAll();

            // Clear existing data
            // No longer applicable; re-using existing table structure where possible
            //Ref_DayTable.ClearRows();

            List<TableRow> rows = Ref_DayTable.Rows;

            Ref_DayTable.ColumnWidths = new List<float>() { 0 };

            bool dateRangeValid = Config.DateRange.Validate();

            // Day Names
            var dayNames = DatePickerUtilities.GetAbbreviatedDayNames().ToList();
            var dayNameRow = Ref_DayTable.Rows.Count != 0 ? Ref_DayTable.Rows[0] : Ref_DayTable.AddRow(0);
            dayNameRow.dontUseTableRowBackground = true;

            int columnIndex = 0;

            if (Config.WeekDays.ShowWeekNumbers)
            {
                dayNames.Insert(0, "");
            }

            var dayNameCells = dayNameRow.Cells;
            foreach (var dayName in dayNames)
            {
                DatePicker_DayHeader header = null;

                if (dayNameCells.Count > columnIndex)
                {
                    header = dayNameCells[columnIndex].GetComponent<DatePicker_DayHeader>();
                }

                if (header == null)
                {
                    header = Instantiate(Ref_Template_DayName);
                    header.transform.localScale = Vector3.one;
                    dayNameRow.AddCell(header.Cell);
                }

                header.HeaderText.text = dayName;

                columnIndex++;
            }

            while (dayNameRow.CellCount > dayNames.Count)
            {
                DestroyImmediate(dayNameRow.Cells[dayNameRow.CellCount - 1].gameObject);
            }

            columnIndex = 0;

            var days = DatePickerUtilities.GetDateRangeForDisplay(VisibleDate.Date);
            TableRow row = null;
            int rowIndex = 1;

            foreach (var day in days)
            {
                if (day.DayOfWeek == DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                {
                    row = rows.Count > rowIndex ? rows[rowIndex] : Ref_DayTable.AddRow(0);
                    rowIndex++;
                    columnIndex = 0;
                }

                var dayType = GetDayTypeForDate(day);
                var dayItem = buttonPool.GetButton(dayType);

                row.AddCell(dayItem.Cell);
                dayItem.transform.SetSiblingIndex(columnIndex);
                dayItem.name = day.ToDateString();

                dayItem.Text.text = day.Day.ToString();
                dayItem.DatePicker = this;
                dayItem.Date = day;
                dayItem.name = day.ToDateString();
                dayItem.IsTemplate = false;
                dayItem.Button.interactable = true;
                dayItem.Type = dayType;
                dayItem.transform.localScale = Vector3.one;

                if (dateRangeValid) // if the date range is not valid, then don't attempt to use it
                {
                    if ((Config.DateRange.RestrictFromDate && day.CompareTo(Config.DateRange.FromDate) < 0)
                        || (Config.DateRange.RestrictToDate && day.CompareTo(Config.DateRange.ToDate) > 0))
                    {
                        dayItem.Button.interactable = false;
                    }
                }

                if (Config.DateRange.ProhibitedDates != null && Config.DateRange.ProhibitedDates.Count != 0)
                {
                    if (Config.DateRange.ProhibitedDates.Any(pd => pd.HasValue && pd.Date == day.Date))
                    {
                        dayItem.Button.interactable = false;
                    }
                }

                if ((dayItem.Date.DayOfWeek == DayOfWeek.Monday && !Config.DateRange.PermittedWeekDays.Monday)
                 || (dayItem.Date.DayOfWeek == DayOfWeek.Tuesday && !Config.DateRange.PermittedWeekDays.Tuesday)
                 || (dayItem.Date.DayOfWeek == DayOfWeek.Wednesday && !Config.DateRange.PermittedWeekDays.Wednesday)
                 || (dayItem.Date.DayOfWeek == DayOfWeek.Thursday && !Config.DateRange.PermittedWeekDays.Thursday)
                 || (dayItem.Date.DayOfWeek == DayOfWeek.Friday && !Config.DateRange.PermittedWeekDays.Friday)
                 || (dayItem.Date.DayOfWeek == DayOfWeek.Saturday && !Config.DateRange.PermittedWeekDays.Saturday)
                 || (dayItem.Date.DayOfWeek == DayOfWeek.Sunday && !Config.DateRange.PermittedWeekDays.Sunday)
                    )
                {
                    dayItem.Button.interactable = false;
                }

                // Optionally hide dates from other months
                if (dayType == DatePickerDayButtonType.OtherMonths)
                {
                    if (!Config.Misc.ShowDatesInOtherMonths)
                    {
                        dayItem.Button.gameObject.SetActive(false);
                    }
                    else
                    {
                        dayItem.Button.gameObject.SetActive(true);
                    }
                }

                columnIndex++;
            }

            UpdateWeekNumbers(ref days);
            Ref_DayTable.UpdateLayout();

            DatePickerTimer.AtEndOfFrame(() =>
            {
                var _rows = Ref_DayTable.Rows;

                foreach (var _row in _rows)
                {
                    var cells = _row.Cells;
                    int cIndex = 0;

                    foreach (var cell in cells)
                    {
                        if (cell.transform.childCount == 0 || cIndex >= dayNames.Count)
                        {
                            DestroyImmediate(cell.gameObject);
                        }

                        cIndex++;
                    }
                }

            }, this);


            DatePickerTimer.DelayedCall(0.01f, () => Ref_DayTable.UpdateLayout(), this);

            UpdateInputField();
        }

        void UpdateWeekNumbers(ref List<DateTime> days)
        {
            DateTimeFormatInfo currentDateTimeFormatInfo = DateTimeFormatInfo.CurrentInfo;

            var rows = Ref_DayTable.Rows;
            for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                DatePicker_DayHeader weekNumberElement = rows[rowIndex].GetComponentInChildren<DatePicker_DayHeader>(true);

                if (Config.WeekDays.ShowWeekNumbers)
                {
                    int weekNumber = rowIndex;

                    if (Config.WeekDays.WeekNumberMode == WeekNumberMode.WeekOfYear)
                    {
                        weekNumber = currentDateTimeFormatInfo.Calendar.GetWeekOfYear(days[7 * (rowIndex - 1)], Config.WeekDays.CalendarWeekRule, currentDateTimeFormatInfo.FirstDayOfWeek);
                    }

                    if (weekNumberElement == null)
                    {
                        weekNumberElement = Instantiate(Ref_Template_DayName);
                    }

                    rows[rowIndex].AddCell(weekNumberElement.Cell);
                    weekNumberElement.transform.SetAsFirstSibling();

                    weekNumberElement.HeaderText.text = weekNumber.ToString();
                    weekNumberElement.transform.localScale = Vector3.one;

                    weekNumberElement.gameObject.SetActive(true);
                }
                else
                {
                    if (weekNumberElement != null)
                    {
                        weekNumberElement.gameObject.SetActive(false);
                    }
                }
            }
        }

        void UpdateInputField()
        {
            if (Ref_InputField != null && Ref_InputFieldContainer != null && Ref_InputFieldToggleButtonCell != null)
            {
                Ref_InputField.text = SelectedDate.HasValue ? SelectedDate.Date.ToString(Config.Format.DateFormat) : "";
                if (Ref_ScreenOverlay != null) Ref_ScreenOverlay.color = Config.Modal.ScreenOverlayColor;

                var valueBefore = Ref_InputFieldContainer.ColumnWidths.ToList();

                if (Config.InputField.ShowToggleButton)
                {
                    Ref_InputFieldContainer.ColumnWidths = new List<float> { 0, Config.InputField.ToggleButtonWidth };
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

        void UpdateBorder()
        {
            // Border size / color
            Ref_ContentLayout.SetBorderSize(Config.Border.Size);
            Ref_Border.color = Config.Border.Color;
        }

        void UpdateHeader()
        {
            // Update month name
            Ref_Header.HeaderText.text = VisibleDate.Date.ToString("MMM yyyy");

            Config.Header.Apply(Ref_Header);

            var dateRangeValid = Config.DateRange.Validate(true);

            if (dateRangeValid && Config.DateRange.RestrictFromDate)
            {
                var lastDayOfPreviousMonth = VisibleDate.Date.AddMonths(-1);
                lastDayOfPreviousMonth = new DateTime(lastDayOfPreviousMonth.Year, lastDayOfPreviousMonth.Month, DateTime.DaysInMonth(lastDayOfPreviousMonth.Year, lastDayOfPreviousMonth.Month)).AddDays(1).AddTicks(-1);

                Ref_Header.PreviousMonthButton.Button.interactable = (lastDayOfPreviousMonth.CompareTo(Config.DateRange.FromDate) >= 0);

                var lastDayOfMonthInPreviousYear = VisibleDate.Date.AddYears(-1);
                lastDayOfMonthInPreviousYear = new DateTime(lastDayOfMonthInPreviousYear.Year, lastDayOfMonthInPreviousYear.Month, DateTime.DaysInMonth(lastDayOfMonthInPreviousYear.Year, lastDayOfMonthInPreviousYear.Month));

                Ref_Header.PreviousYearButton.Button.interactable = (lastDayOfMonthInPreviousYear.CompareTo(Config.DateRange.FromDate) >= 0);
            }
            else
            {
                Ref_Header.PreviousMonthButton.Button.interactable = true;
            }

            if (dateRangeValid && Config.DateRange.RestrictToDate)
            {
                var firstDayOfNextMonth = VisibleDate.Date.AddMonths(1);
                firstDayOfNextMonth = new DateTime(firstDayOfNextMonth.Year, firstDayOfNextMonth.Month, 1);

                Ref_Header.NextMonthButton.Button.interactable = (firstDayOfNextMonth.CompareTo(Config.DateRange.ToDate) <= 0);

                var firstDayOfMonthInNextYear = VisibleDate.Date.AddYears(1);
                firstDayOfMonthInNextYear = new DateTime(firstDayOfMonthInNextYear.Year, firstDayOfMonthInNextYear.Month, 1);

                Ref_Header.NextYearButton.Button.interactable = (firstDayOfMonthInNextYear.CompareTo(Config.DateRange.ToDate) <= 0);
            }
            else
            {
                Ref_Header.NextMonthButton.Button.interactable = true;
            }
        }

        void UpdateWeekDayHeaders()
        {
            Config.WeekDays.ApplyConfig(Ref_Template_DayName);
        }

        void UpdateDaySection()
        {
            var templateList = new List<DatePicker_Button>()
            {
                Ref_Template_Day_Today,
                Ref_Template_Day_SelectedDay,
                Ref_Template_Day_CurrentMonth,
                Ref_Template_Day_OtherMonths
            };

            Config.Days.Validate();

            foreach (var template in templateList)
            {
                template.IsTemplate = true; // just in case
                template.Text.font = Config.Days.Font;

                if (Config.Days.FontSize > 0) template.Text.fontSize = Config.Days.FontSize;

                template.Text.fontSizeMin = Config.Days.FontSizeMin;
                template.Text.fontSizeMax = Config.Days.FontSizeMax;
                template.Text.enableAutoSizing = Config.Days.FontAutoSize;
            }

            Config.Days.Today.ApplyConfig(Ref_Template_Day_Today);
            Config.Days.SelectedDay.ApplyConfig(Ref_Template_Day_SelectedDay);
            Config.Days.OtherMonths.ApplyConfig(Ref_Template_Day_OtherMonths);
            Config.Days.CurrentMonth.ApplyConfig(Ref_Template_Day_CurrentMonth);

            Ref_DayTable.RowBackgroundColor = Config.Days.BackgroundColor;
            Ref_DayTableContainer.image.color = Config.Days.BackgroundColor;

            /*Ref_DayTable.transform.rotation = new Quaternion(0, 0, 0, 0);
            Ref_DayTableContainer.transform.rotation = new Quaternion(0, 0, 0, 0);
            Ref_DayTableContainer.NotifyTableCellPropertiesChanged();*/
        }

        public void InvalidateAllDayButtonTemplates()
        {
            buttonPool.InvalidateAll();
            UpdateDisplay();
        }

        public void InvalidateDayButtonTemplate(DatePickerDayButtonType type)
        {
            buttonPool.InvalidateType(type);
            UpdateDisplay();
        }

        public void ClearWeekDayHeaders()
        {
            var weekDayHeaders = Ref_DayTable.GetComponentsInChildren<DatePicker_DayHeader>();
            foreach (var header in weekDayHeaders)
            {
                DestroyImmediate(header.gameObject);
            }
        }

        private DatePickerDayButtonType GetDayTypeForDate(DateTime date)
        {
            DatePickerDayButtonType type;

            if ((DateSelectionMode == Dates.DateSelectionMode.SingleDate && SelectedDate.HasValue && date.Equals(SelectedDate.Date))
             || (DateSelectionMode == Dates.DateSelectionMode.MultipleDates && SelectedDates.Contains(date)))
            {
                type = DatePickerDayButtonType.SelectedDay;
            }
            else if (date.Equals(DateTime.Today))
            {
                type = DatePickerDayButtonType.Today;
            }
            else if (date.Month == VisibleDate.Date.Month)
            {
                type = DatePickerDayButtonType.CurrentMonth;
            }
            else
            {
                type = DatePickerDayButtonType.OtherMonths;
            }

            return type;
        }

        /// <summary>
        /// Called by DayButton
        /// </summary>
        /// <param name="date"></param>
        public void DayButtonClicked(DateTime date)
        {
            if (DateSelectionMode == Dates.DateSelectionMode.SingleDate)
            {
                SelectedDate = date;
            }
            else
            {
                if (SelectedDates.Any(d => d == date))
                {
                    SelectedDates.Remove(date);
                }
                else
                {
                    SelectedDates.Add(date);
                }
            }

            if (Ref_DatePicker_DateRange != null)
            {
                Ref_DatePicker_DateRange.DateSelected(date);
            }

            if (Config.Events.OnDaySelected != null)
            {
                Config.Events.OnDaySelected.Invoke(date);
            }

            UpdateDisplay();

            // I would have preferred to have this react automatically to changes,
            // but that would mean setting up an observable list, which is an added
            // complication we don't need right now
            UpdateInputFieldText();
        }

        public void UpdateInputFieldText()
        {
            if (Ref_InputField != null)
            {
                switch (DateSelectionMode)
                {
                    case Dates.DateSelectionMode.SingleDate:
                        Ref_InputField.text = (SelectedDate.HasValue) ? SelectedDate.Date.ToString(Config.Format.DateFormat) : "";
                        break;
                    case Dates.DateSelectionMode.MultipleDates:
                        var valueCount = SelectedDates.Count(s => s.HasValue);
                        Ref_InputField.text = ((valueCount == 1) ? SelectedDates.First(s => s.HasValue).Date.ToString(Config.Format.DateFormat)
                                                         : (valueCount > 1 ? "Multiple Dates" : ""));
                        break;
                }
            }
        }

        /// <summary>
        /// Called by DayButton
        /// </summary>
        /// <param name="date"></param>
        public void DayButtonMouseOver(DateTime date)
        {
            if (Config.Events.OnDayMouseOver != null)
            {
                Config.Events.OnDayMouseOver.Invoke(date);
            }
        }

        /// <summary>
        /// Called by the screen overlay when it is clicked
        /// </summary>
        public void ModalOverlayClicked()
        {
            if (Ref_DatePicker_DateRange != null)
            {
                Ref_DatePicker_DateRange.ModalOverlayClicked();
            }
            else
            {
                if (Config.Modal.CloseWhenModalOverlayClicked) Hide();
            }
        }

        public void InputFieldClicked()
        {
            if (Config.InputField.ToggleDisplayWhenInputFieldClicked) ToggleDisplay();
        }

        public void ToggleDisplay()
        {
            if (Ref_DatePickerTransform.gameObject.activeInHierarchy)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show(bool setPositionIfNecessary = true)
        {
            if (Ref_Viewport == null)
            {
                var canvas = FindParentOfType<Canvas>(this.gameObject);
                Ref_Viewport = canvas.transform as RectTransform;
            }


            if (setPositionIfNecessary && Ref_InputField != null)
            {
                // Position tablelayout relative to InputField
                SetPositionAdjacentToInputFieldContainer();
                // Wait till the end of the frame, then complete Show() (this ensures that the DatePicker only becomes visible after being resized)
                DatePickerTimer.DelayedCall(0, () => Show(false), this, true);
                return;
            }

            Ref_DatePickerTransform.gameObject.SetActive(true);

            if (Config.Modal.IsModal && Ref_ScreenOverlay != null)
            {
                if (Ref_Viewport != null)
                {
                    Ref_ScreenOverlay.transform.SetParent(Ref_Viewport);
                    Ref_ScreenOverlay.transform.SetAsLastSibling();
                }

                Ref_ScreenOverlay.gameObject.SetActive(true);

                Ref_ScreenOverlayAnimator.PlayAnimation(Animation.Fade, AnimationType.Show);
            }

            if (Ref_Viewport != null)
            {
                Ref_DatePickerTransform.SetParent(Ref_Viewport);
                Ref_DatePickerTransform.SetAsLastSibling();
            }

            if (Config.Animation.ShowAnimation != Animation.None)
            {
                PlayAnimation(Config.Animation.ShowAnimation, AnimationType.Show);
            }
        }

        private void PlayAnimation(Animation animation, AnimationType animationType, Action onComplete = null)
        {
            Ref_Animator.PlayAnimation(animation, animationType, onComplete);
        }

        public void Hide()
        {
            if (Config.Animation.HideAnimation != Animation.None)
            {
                PlayAnimation(Config.Animation.HideAnimation, AnimationType.Hide, _Hide);
            }
            else
            {
                _Hide();
            }
        }

        private void _Hide()
        {
            if (Config.Modal.IsModal)
            {
                if (Ref_ScreenOverlay != null) Ref_ScreenOverlayAnimator.PlayAnimation(Animation.Fade, AnimationType.Hide, HideScreenOverlay_Complete);
            }

            if (this.transform != Ref_DatePickerTransform)
            {
                Ref_DatePickerTransform.SetParent(this.transform);
            }

            Ref_DatePickerTransform.gameObject.SetActive(false);
        }

        private void HideScreenOverlay_Complete()
        {
            Ref_ScreenOverlay.transform.SetParent(this.transform);
            Ref_ScreenOverlay.gameObject.SetActive(false);
        }

        private void SetPositionAdjacentToInputFieldContainer()
        {
            if (Ref_InputFieldContainer == null) return;

            var rectTransform = Ref_DatePickerTransform;
            var inputFieldRectTransform = Ref_InputFieldContainer.transform as RectTransform;
            var inputFieldWidth = inputFieldRectTransform.rect.width;

            if (IsSharedCalendar)
            {
                rectTransform.SetParent(inputFieldRectTransform.parent);
            }

            // Fix anchors:
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);

            var widthBefore = rectTransform.rect.width;
            if (Config.Sizing.UsePreferredWidthInsteadOfInputFieldWidth)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Config.Sizing.PreferredWidth);
            }
            else
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inputFieldWidth);
            }

            if (widthBefore != rectTransform.rect.width)
            {
                ((RectTransform)Ref_DayTable.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width);
            }

            var pivotX = 0.5f;
            switch (Config.InputField.DatePickerAlignmentRelativeToInputField)
            {
                case Alignment.Left:
                    {
                        pivotX = 0f;
                    }
                    break;
                case Alignment.Right:
                    {
                        pivotX = 1f;
                    }
                    break;
            }

            if (Ref_Viewport == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                Ref_Viewport = ((RectTransform)canvas.transform);
            }

            rectTransform.pivot = new Vector2(pivotX, 0.5f);
            rectTransform.anchoredPosition = inputFieldRectTransform.anchoredPosition;
            rectTransform.SetParent(Ref_Viewport);

            rectTransform.pivot = new Vector2(pivotX, 1);
            rectTransform.anchoredPosition -= new Vector2(0, inputFieldRectTransform.rect.height);

            var spaceBelow = Ref_Viewport.rect.height + rectTransform.anchoredPosition.y;

            if (spaceBelow < rectTransform.rect.height)
            {
                rectTransform.pivot = new Vector2(pivotX, 0);
                rectTransform.anchoredPosition += new Vector2(0, inputFieldRectTransform.rect.height);

                var spaceAbove = -(rectTransform.anchoredPosition.y + rectTransform.rect.height);
                if (spaceAbove < 0)
                {
                    rectTransform.anchoredPosition += new Vector2(0, spaceAbove);
                }
            }

            DatePickerTimer.DelayedCall(0.05f, () => { Ref_DayTableContainer.GetRow().NotifyTableRowPropertiesChanged(); }, this);
        }

        private static T FindParentOfType<T>(GameObject childObject)
        where T : UnityEngine.Object
        {
            Transform t = childObject.transform;
            while (t.parent != null)
            {
                var component = t.parent.GetComponent<T>();

                if (component != null) return component;

                t = t.parent.transform;
            }

            // We didn't find anything
            return null;
        }

        #region SetProperty
        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;

            currentValue = newValue;

            UpdateDisplay();
        }

        void OnRectTransformDimensionsChange()
        {
            //UpdateDisplay();
            //DatePickerTimer.DelayedCall(0f, UpdateDisplay, this);
        }
        #endregion

        /// <summary>
        /// Get all Day buttons of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<DatePicker_DayButton> GetDayButtons(DatePickerDayButtonType type)
        {
            var dayButtons = GetComponentsInChildren<DatePicker_DayButton>();

            return dayButtons.Where(d => d.Type == type).ToList();
        }

        /// <summary>
        /// Get all day buttons regardless of type
        /// </summary>
        /// <returns></returns>
        private List<DatePicker_DayButton> GetDayButtons()
        {
            return GetComponentsInChildren<DatePicker_DayButton>().ToList();
        }

        /// <summary>
        /// Get the day button for the specified date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public DatePicker_DayButton GetDayButton(DateTime date)
        {
            var dateString = date.ToDateString();
            var dayButtons = GetComponentsInChildren<DatePicker_DayButton>();

            return dayButtons.FirstOrDefault(d => d.name == dateString);
        }

        /// <summary>
        /// Get a serialized string representing the configuration of this DatePicker
        /// </summary>
        /// <returns></returns>
        public string GetSerializedConfiguration()
        {
            return JsonUtility.ToJson(Config);
        }

        /// <summary>
        /// Set the configuration of this DatePicker based on a Json string
        /// (output from GetSerializedConfiguration)
        /// </summary>
        /// <param name="json"></param>
        public void SetConfigFromJsonString(string json)
        {
            Config = JsonUtility.FromJson<DatePickerConfig>(json);
            UpdateDisplay();
        }
    }
}
