using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace UI.Dates
{
    [CustomPropertyDrawer(typeof(SerializableDate))]
    public class DatePickerDateDrawer : PropertyDrawer
    {
        private GUIContent buttonImage = null;
        private GUIStyle textStyle = null;


        //private bool showDatePicker = false;

        private GUIContent m_leftArrowImage;
        private GUIContent leftArrowImage
        {
            get
            {
                if (m_leftArrowImage == null) m_leftArrowImage = new GUIContent(Resources.Load("Sprites/Editor/Editor_Arrow_Left") as Texture2D);

                return m_leftArrowImage;
            }
        }

        private GUIContent m_doubleLeftArrowImage;
        private GUIContent doubleLeftArrowImage
        {
            get
            {
                if (m_doubleLeftArrowImage == null) m_doubleLeftArrowImage = new GUIContent(Resources.Load("Sprites/Editor/Editor_Arrow_Double_Left") as Texture2D);

                return m_doubleLeftArrowImage;
            }
        }

        private GUIContent m_rightArrowImage;
        private GUIContent rightArrowImage
        {
            get
            {
                if (m_rightArrowImage == null) m_rightArrowImage = new GUIContent(Resources.Load("Sprites/Editor/Editor_Arrow_Right") as Texture2D);

                return m_rightArrowImage;
            }
        }

        private GUIContent m_doubleRightArrowImage;
        private GUIContent doubleRightArrowImage
        {
            get
            {
                if (m_doubleRightArrowImage == null) m_doubleRightArrowImage = new GUIContent(Resources.Load("Sprites/Editor/Editor_Arrow_Double_Right") as Texture2D);

                return m_doubleRightArrowImage;
            }
        }

        private GUIStyle m_monthNameStyle;
        private GUIStyle monthNameStyle        
        {
            get
            {
                if (m_monthNameStyle == null)
                {
                    m_monthNameStyle = new GUIStyle(EditorStyles.label);
                    m_monthNameStyle.fontStyle = FontStyle.Bold;
                    m_monthNameStyle.fontSize = 14;
                    m_monthNameStyle.alignment = TextAnchor.MiddleCenter;
                }

                return m_monthNameStyle;
            }
        }

        private GUIStyle m_dayNameStyle;
        private GUIStyle dayNameStyle
        {
            get
            {
                if (m_dayNameStyle == null)
                {
                    m_dayNameStyle = new GUIStyle(EditorStyles.label);
                    m_dayNameStyle.alignment = TextAnchor.MiddleCenter;
                    m_dayNameStyle.fontStyle = FontStyle.Bold;
                }

                return m_dayNameStyle;
            }
        }

        private GUIStyle m_dayButtonStyle;
        private GUIStyle dayButtonStyle
        {
            get
            {
                if (m_dayButtonStyle == null)
                {
                    m_dayButtonStyle = new GUIStyle(EditorStyles.miniButton);
                    m_dayButtonStyle.fontSize = 12;
                    m_dayButtonStyle.richText = true;
                    m_dayButtonStyle.alignment = TextAnchor.MiddleCenter;
                    m_dayButtonStyle.normal.textColor = Color.black;                    
                }

                return m_dayButtonStyle;
            }
        }

        private GUIStyle m_dayButtonStyle_OtherMonth;
        private GUIStyle dayButtonStyle_OtherMonth
        {
            get
            {
                if (m_dayButtonStyle_OtherMonth == null)
                {
                    m_dayButtonStyle_OtherMonth = new GUIStyle(dayButtonStyle);
                    m_dayButtonStyle_OtherMonth.normal.textColor = new Color(0, 0, 0, 0.5f);                    
                }

                return m_dayButtonStyle_OtherMonth;
            }
        }

        private GUIStyle m_dayButtonStyle_CurrentDay;
        private GUIStyle dayButtonStyle_CurrentDay          
        {
            get
            {
                if(m_dayButtonStyle_CurrentDay == null)
                {
                    m_dayButtonStyle_CurrentDay = new GUIStyle(dayButtonStyle);
                    m_dayButtonStyle_CurrentDay.fontStyle = FontStyle.Bold;
                }

                return m_dayButtonStyle_CurrentDay;
            }
        }        

        private DateTime SelectedDate;

        private DateTime? VisibleDate;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.FindPropertyRelative("m_showEditorCalendar").boolValue)
                return 24f;
            else
                return 292f;
        }        

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var showDatePickerProperty = property.FindPropertyRelative("m_showEditorCalendar");
            var showDatePicker = showDatePickerProperty.boolValue;

            var serializedProperty = property.FindPropertyRelative("m_SerializedDate");

            SelectedDate = !String.IsNullOrEmpty(serializedProperty.stringValue) ? DateTime.Parse(serializedProperty.stringValue) : DateTime.Today;

            if (!String.IsNullOrEmpty(serializedProperty.stringValue))
            {
                if (!DateTime.TryParse(serializedProperty.stringValue, out SelectedDate))
                {
                    SelectedDate = DateTime.Today;
                }
            }
            else
            {
                SelectedDate = DateTime.Today;
            }
     
            if (buttonImage == null)
            {
                buttonImage = new GUIContent(Resources.Load("Sprites/Editor/Calendar_Editor") as Texture2D);
            }

            if (textStyle == null)
            {
                textStyle = new GUIStyle(EditorStyles.textField);
                textStyle.fontSize = 18;
                textStyle.alignment = TextAnchor.MiddleCenter;
            }

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;            

            var textFieldPosition = new Rect(position.x, position.y, 224, 24);
            var buttonPosition = new Rect(position.x + 224, position.y, 40, 24);

            EditorGUI.BeginDisabledGroup(true);
            serializedProperty.stringValue = EditorGUI.TextField(textFieldPosition, serializedProperty.stringValue, textStyle);
            EditorGUI.EndDisabledGroup();
            if (GUI.Button(buttonPosition, buttonImage))
            {
                showDatePicker = !showDatePicker;
            }

            buttonPosition.x += 48;
            buttonPosition.width = 24;

            if (GUI.Button(buttonPosition, "X"))
            {
                serializedProperty.stringValue = null;
                showDatePicker = false;
            }

            position.y += 24;

            if (showDatePicker)
            {
                showDatePicker = DrawDatePicker(position, serializedProperty, showDatePickerProperty);                
            }

            EditorGUI.indentLevel = indent;

            showDatePickerProperty.boolValue = showDatePicker;

            EditorGUI.EndProperty();            
        }

        bool DrawDatePicker(Rect position, SerializedProperty property, SerializedProperty showDatePickerProperty)
        {
            if (!VisibleDate.HasValue) VisibleDate = SelectedDate;

            float startX = position.x;            

            var calendarPosition = new Rect(position.x, position.y, 224, 256);
            EditorGUI.DrawRect(calendarPosition, new Color(0, 0, 0, 0.1f));

            var rowSize = 224f / 7f;
            var columnSize = rowSize;

            // previous year button
            if(GUI.Button(new Rect(position.x, position.y, rowSize, columnSize), doubleLeftArrowImage))
            {
                VisibleDate = VisibleDate.Value.AddYears(-1);
            }

            position.x += columnSize;

            // previous month button
            if (GUI.Button(new Rect(position.x, position.y, rowSize, columnSize), leftArrowImage))
            {
                VisibleDate = VisibleDate.Value.AddMonths(-1);
            }

            position.x += columnSize;

            // month name
            GUI.Label(new Rect(position.x, position.y, columnSize * 3, rowSize), VisibleDate.Value.ToString("MMM yyyy"), monthNameStyle);

            position.x += columnSize * 3;

            // next month button
            if(GUI.Button(new Rect(position.x, position.y, columnSize, rowSize), rightArrowImage))
            {
                VisibleDate = VisibleDate.Value.AddMonths(+1);                
            }

            position.x += columnSize;

            // next year button
            if (GUI.Button(new Rect(position.x, position.y, columnSize, rowSize), doubleRightArrowImage))
            {
                VisibleDate = VisibleDate.Value.AddYears(+1);
            }

            // next row
            position.x = startX;
            position.y += rowSize;

            // Day of week headers            
            var dayNames = DatePickerUtilities.GetAbbreviatedDayNames().Select(d => d.Substring(0,1)).ToArray();
            foreach (var dayName in dayNames)
            {
                GUI.Label(new Rect(position.x, position.y, columnSize, rowSize), dayName, dayNameStyle);
                position.x += columnSize;
            }
            
            // Start outputing day buttons
            var days = DatePickerUtilities.GetDateRangeForDisplay(VisibleDate.Value);

            var selectedDateString = SelectedDate.ToDateString();
            var returnValue = true;

            foreach (var day in days)
            {
                if (day.DayOfWeek == DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                {
                    // Next row
                    position.x = startX;
                    position.y += rowSize;
                }

                var style = dayButtonStyle;                

                if (day.Month == VisibleDate.Value.Month)                                
                {

                    var backColor = new Color(0, 0, 0, 0.5f);
                    if (day.ToDateString() == selectedDateString)
                    {
                        style = dayButtonStyle_CurrentDay;
                        backColor = Color.green;
                    }                    

                    EditorGUI.DrawRect(new Rect(position.x, position.y, rowSize, columnSize), backColor);
                }
                else
                {
                    style = dayButtonStyle_OtherMonth;
                }                

                if (GUI.Button(new Rect(position.x + 1, position.y + 1, columnSize - 2, rowSize - 2), day.Day.ToString(), style))
                {
                    property.stringValue = day.ToDateString();
                    VisibleDate = day;
                    returnValue = false; // hide the datepicker
                }

                position.x += columnSize;                
            }

            return returnValue;
        }        
    }
}
