using UnityEngine;
using System.Collections;
using System;

namespace UI.Dates
{
    [Serializable]
    public struct SerializableDate
    {
        //private DateTime? m_Date;
        public DateTime Date
        {
            get
            {                
                /*if (!m_Date.HasValue)
                {
                    m_Date = DateTime.ParseExact(m_SerializedDate, DatePickerUtilities.DateFormat, null);
                }

                return m_Date.Value;*/                

                return DateTime.ParseExact(m_SerializedDate, DatePickerUtilities.DateFormat, null);
            }
            set
            {
                m_SerializedDate = value.ToDateString();
                //m_Date = value;
            }
        }

        [SerializeField]
        private string m_SerializedDate;

#if UNITY_EDITOR
#pragma warning disable 414
        [SerializeField]
        private bool m_showEditorCalendar;        
#pragma warning restore 414
#endif

        public bool HasValue
        {
            get
            {                
                return !String.IsNullOrEmpty(m_SerializedDate);
            }
        }        

        public SerializableDate(DateTime date)
        {
#if UNITY_EDITOR
            m_showEditorCalendar = false;
#endif
            m_SerializedDate = "";

            Date = date;
        }

        public SerializableDate(SerializableDate date)
        {
#if UNITY_EDITOR
            m_showEditorCalendar = false;
#endif
            m_SerializedDate = "";

            if (date.HasValue)
            {
                Date = date.Date;
            }
        }

        public override string ToString()
        {
            if (HasValue)
            {
                return Date.ToString();
            }

            return null;
        }

        #region Operators
        public static implicit operator SerializableDate(DateTime date)
        {
            return new SerializableDate(date);
        }

        public static implicit operator DateTime(SerializableDate date)
        {
            return date.Date;
        }        
        #endregion
    }
}
