using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UI.Tables;
using TMPro;

namespace UI.Dates
{
    public class DatePicker_Header : MonoBehaviour
    {
        #pragma warning disable
        [SerializeField, FormerlySerializedAs("HeaderText")]
        private Text oldHeaderText;
        #pragma warning restore

        [SerializeField]
        private TextMeshProUGUI m_HeaderText;

        public TextMeshProUGUI HeaderText
        {
            get
            {
                if (m_HeaderText == null)
                {
                    if (oldHeaderText == null)
                    {
                        Debug.LogError("[DatePicker] An error ocurred while upgrading to TextMesh Pro - unable to locate original header text element.");
                        return null;
                    }

                    m_HeaderText = DatePicker_TextMeshProUtilities.ReplaceTextElementWithTextMeshPro(oldHeaderText);
                }

                return m_HeaderText;
            }
        }

        public DatePicker_Button PreviousMonthButton;
        public DatePicker_Button NextMonthButton;
        public DatePicker_Button PreviousYearButton;
        public DatePicker_Button NextYearButton;
        public Image Background;
        public TableRow Row;
        public TableLayout TableLayout;
    }
}
