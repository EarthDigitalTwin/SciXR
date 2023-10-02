using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI.Tables;

namespace UI.Dates
{
    /// <summary>
    /// A special button which responds when held down
    /// Note: These are intended to supplement the existing Next/Previous buttons,
    /// and as such, they require that the existing buttons already have an event handler defined
    /// (which this component will then extract and replace at runtime)
    /// </summary>
    public class DatePicker_HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Button Button;
        public float Delay = 0.5f;

        private bool pointerDown = false;
        private Action action;
        private float lastInvokeTime = 0;
        private int executionCount = 0;

        void Start()
        {
            Button = this.GetComponent<Button>();

            // The default buttons have click event handlers
            int eventCount = Button.onClick.GetPersistentEventCount();
            MonoBehaviour[] target = new MonoBehaviour[eventCount];
            string[] method = new string[eventCount];
            for (int i = 0; i < eventCount; ++i)
            {
                target[i] = (MonoBehaviour)Button.onClick.GetPersistentTarget(i);
                method[i] = Button.onClick.GetPersistentMethodName(i);
            }
            action = () =>
            {
                for (int i = 0; i < eventCount; ++i)
                {
                    target[i].Invoke(method[i], 0);
                }
            };

            /*
            // The default buttons have click event handlers
            var target = Button.onClick.GetPersistentTarget(0);
            var method = Button.onClick.GetPersistentMethodName(0);

            action = () => { ((MonoBehaviour)target).Invoke(method, 0); };*/

            // This appears to be the only way to remove persistent listeners in code (at runtime)
            Button.onClick = new Button.ButtonClickedEvent();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDown = true;

            Execute();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerDown = false;
            executionCount = 0;
        }

        void Update()
        {
            if (!Button.interactable) return;

            var _delay = Delay / executionCount;

            if (pointerDown && lastInvokeTime < Time.realtimeSinceStartup - _delay)
            {
                Execute();
            }
        }

        void Execute()
        {
            if (!Button.interactable) return;

            lastInvokeTime = Time.realtimeSinceStartup;
            executionCount++;

            action.Invoke();
        }
    }
}
