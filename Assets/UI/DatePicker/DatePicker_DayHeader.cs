using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;
using UnityEngine.Serialization;
using TMPro;

namespace UI.Dates
{
    public class DatePicker_DayHeader : MonoBehaviour
    {
#pragma warning disable
        [SerializeField, FormerlySerializedAs("HeaderText"), HideInInspector]
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
                        Debug.LogError("[DatePicker] An error ocurred while upgrading to TextMesh Pro - unable to locate original text element for a day header.");
                        return null;
                    }

                    m_HeaderText = DatePicker_TextMeshProUtilities.ReplaceTextElementWithTextMeshPro(oldHeaderText);

                    m_HeaderText.enableAutoSizing = true;
                    m_HeaderText.fontSizeMin = 2;
                    m_HeaderText.fontSizeMax = 16;
                }

                return m_HeaderText;
            }
        }


        public TableCell Cell;
    }
}
