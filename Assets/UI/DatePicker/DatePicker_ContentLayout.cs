using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;

namespace UI.Dates
{    
    public class DatePicker_ContentLayout : MonoBehaviour
    {
        public DatePicker DatePicker;

        private RectTransform m_rectTransform = null;
        protected RectTransform rectTransform
        {
            get
            {
                if (m_rectTransform == null) m_rectTransform = this.GetComponent<RectTransform>();

                return m_rectTransform;
            }
        }

        public void SetBorderSize(RectOffset borderSize)
        {
            rectTransform.offsetMin = new Vector2(borderSize.right, borderSize.bottom);
            rectTransform.offsetMax = new Vector2(-borderSize.left, -borderSize.top);
        }

        public void SetBorderSize(int borderSize)
        {
            SetBorderSize(new RectOffset(borderSize, borderSize, borderSize, borderSize));
        }        

        void OnRectTransformDimensionsChanged()
        {
            DatePicker.UpdateDisplay();
        }
    }    
}
