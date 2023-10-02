using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UI.Dates
{
    public class DatePicker_DayButton_Pool : MonoBehaviour
    {
        private DatePicker _datePicker;
        private DatePicker datePicker
        {
            get
            {
                if (_datePicker == null) _datePicker = this.GetComponent<DatePicker>();

                return _datePicker;
            }
        }

        [SerializeField]
        private RectTransform _poolRect;
        private RectTransform poolRect
        {
            get
            {
                if (_poolRect == null)
                {
                    _poolRect = (RectTransform)(new GameObject("Pool Rect", typeof(RectTransform)).transform);
                    _poolRect.SetParent(this.transform);
                    _poolRect.gameObject.SetActive(false);
                }

                return _poolRect;
            }
        }

        private Dictionary<DatePickerDayButtonType, DatePicker_DayButton_Pool_List> pool = new Dictionary<DatePickerDayButtonType, DatePicker_DayButton_Pool_List>();

        public void AddExistingButton(DatePicker_DayButton button)
        {
            GetPoolList(button.Type).AddExistingButtonToPool(button);
        }

        public DatePicker_DayButton GetButton(DatePickerDayButtonType type)
        {
            return GetPoolList(type).GetButton();
        }

        private DatePicker_DayButton_Pool_List GetPoolList(DatePickerDayButtonType type)
        {
            DatePicker_DayButton_Pool_List poolList = null;
            if (!pool.ContainsKey(type))
            {
                poolList = new DatePicker_DayButton_Pool_List(type, poolRect);
                pool.Add(type, poolList);

                var template = GetTemplate(type);
                poolList.SetTemplate(template);
            }
            else
            {
                poolList = pool[type];
            }

            return poolList;
        }

        public void FreeAll()
        {
            foreach (var kvp in pool)
            {
                kvp.Value.FreeAll();
            }
        }

        public void InvalidateAll()
        {
            foreach (var kvp in pool)
            {
                kvp.Value.Invalidate();
            }


        }

        public void InvalidateType(DatePickerDayButtonType type)
        {
            if (!pool.ContainsKey(type)) return;

            pool[type].Invalidate();
        }

        private DatePicker_DayButton GetTemplate(DatePickerDayButtonType type)
        {
            switch (type)
            {
                case DatePickerDayButtonType.Today:
                    return datePicker.Ref_Template_Day_Today;

                case DatePickerDayButtonType.SelectedDay:
                    return datePicker.Ref_Template_Day_SelectedDay;

                case DatePickerDayButtonType.CurrentMonth:
                    return datePicker.Ref_Template_Day_CurrentMonth;

                case DatePickerDayButtonType.OtherMonths:
                    return datePicker.Ref_Template_Day_OtherMonths;
            }

            return null;
        }
    }

    public class DatePicker_DayButton_Pool_List
    {
        //private DatePickerDayButtonType type;
        private DatePicker_DayButton template;
        private RectTransform poolRect;

        private List<DatePicker_DayButton_Pool_List_Item> pool = new List<DatePicker_DayButton_Pool_List_Item>();

        public DatePicker_DayButton_Pool_List(DatePickerDayButtonType type, RectTransform poolRect)
        {
            //this.type = type;
            this.poolRect = poolRect;
        }

        public void SetTemplate(DatePicker_DayButton template)
        {
            this.template = template;
        }

        public void AddExistingButtonToPool(DatePicker_DayButton button)
        {
            if (pool.Any(p => p.button == button)) return;

            var buttonItem = new DatePicker_DayButton_Pool_List_Item() { button = button };

            pool.Add(buttonItem);
        }

        public DatePicker_DayButton GetButton()
        {
            var buttonItem = pool.FirstOrDefault(i => !i.inUse && i.button != null);

            if (buttonItem == null)
            {
                buttonItem = new DatePicker_DayButton_Pool_List_Item(template);
                pool.Add(buttonItem);
            }

            buttonItem.inUse = true;

            return buttonItem.button;
        }

        public void FreeAll()
        {
            if (pool.Count == 0) return;

            pool.ForEach(p =>
            {
                if (p == null) return;

                p.inUse = false;
                p.button.transform.SetParent(poolRect);
            });
        }

        public void Invalidate()
        {
            var items = pool.Select(p => p.button).ToList();

            foreach (var i in items)
            {
                if (i == null) continue;

                if (Application.isPlaying) GameObject.Destroy(i.gameObject);
                else GameObject.DestroyImmediate(i.gameObject);
            }

            // if, for example, our template has changed, all of our pool items are invalid, so we should remove them
            pool.Clear();
        }
    }

    class DatePicker_DayButton_Pool_List_Item
    {
        public bool inUse = false;
        public DatePicker_DayButton button = null;

        public DatePicker_DayButton_Pool_List_Item()
        {
        }

        public DatePicker_DayButton_Pool_List_Item(DatePicker_DayButton template)
        {
            button = GameObject.Instantiate(template);
            button.IsTemplate = false;
        }
    }
}
