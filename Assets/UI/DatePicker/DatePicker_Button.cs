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
    public class DatePicker_Button : MonoBehaviour
    {
        public Button Button;

        #pragma warning disable
        [SerializeField, FormerlySerializedAs("Text"), HideInInspector]
        private Text oldText;
        #pragma warning restore

        [SerializeField]
        private TextMeshProUGUI m_Text;

        public TextMeshProUGUI Text
        {
            get
            {
                if (m_Text == null)
                {
                    if (oldText == null)
                    {
                        Debug.LogError("[DatePicker] An error ocurred while upgrading to TextMesh Pro - unable to locate original text element for a button.");
                        return null;
                    }

                    m_Text = DatePicker_TextMeshProUtilities.ReplaceTextElementWithTextMeshPro(oldText);
                }

                return m_Text;
            }
        }


        public bool IsTemplate = true;

        [SerializeField]
        private TableCell m_Cell = null;
        public TableCell Cell
        {
            get
            {
                if (m_Cell == null) m_Cell = GetComponent<TableCell>(); // as used by DayButton
                if (m_Cell == null) m_Cell = GetComponentInParent<TableCell>(); // as used by header buttons

                return m_Cell;
            }
        }
    }
}
