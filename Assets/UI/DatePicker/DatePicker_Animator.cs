using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Dates
{    
    // Was having some positioning issues with the default Animator component, so I wrote a simple animator here
    // It turned out that the Animator component wasn't responsible, but I've decided to leave the simpler implementation in place for now
    [RequireComponent(typeof(RectTransform)), RequireComponent(typeof(CanvasGroup))]
    public class DatePicker_Animator : MonoBehaviour
    {        
        protected List<DatePicker_Animation> animations = new List<DatePicker_Animation>();

        private CanvasGroup m_canvasGroup = null;
        protected CanvasGroup canvasGroup
        {
            get
            {
                if (m_canvasGroup == null) m_canvasGroup = GetComponent<CanvasGroup>();
                return m_canvasGroup;
            }
        }

        private RectTransform m_rectTransform = null;
        protected RectTransform rectTransform
        {
            get
            {
                if (m_rectTransform == null) m_rectTransform = GetComponent<RectTransform>();
                return m_rectTransform;
            }
        }

        public bool ResetWhenAnimationComplete = true;
        public float AnimationDuration = 0.25f;

        public void PlayAnimation(Animation animation, AnimationType animationType, Action onComplete = null)
        {            
            if (animationType == AnimationType.Show)
            {
                switch (animation)
                {
                    case Animation.Slide:
                        {
                            // Start off by setting our Y scale to zero;
                            SetPropertyValue(DatePicker_Animation_Property.ScaleY, 0);
                            // Then animate to full size
                            Animate(DatePicker_Animation_Property.ScaleY, 1, AnimationDuration, onComplete);
                        }
                        break;
                    case Animation.Fade:
                        {
                            SetPropertyValue(DatePicker_Animation_Property.Alpha, 0);
                            Animate(DatePicker_Animation_Property.Alpha, 1, AnimationDuration, onComplete);
                        }
                        break;
                }                
            }
            else
            {
                switch (animation)
                {
                    case Animation.Slide:
                        {
                            Animate(DatePicker_Animation_Property.ScaleY, 0f, AnimationDuration, onComplete);
                        }
                        break;

                    case Animation.Fade:
                        {
                            Animate(DatePicker_Animation_Property.Alpha, 0f, AnimationDuration, onComplete);
                        }
                        break;
                }
            }
        }

        public void Animate(DatePicker_Animation_Property property, float desiredValue, float duration, Action onComplete = null)
        {
            if (!Application.isPlaying) 
            {
                SetPropertyValue(property, desiredValue);
                if (onComplete != null) onComplete.Invoke();
                return;
            }

            var animation = animations.FirstOrDefault(a => a.property == property);

            if (animation == null)
            {
                animation = new DatePicker_Animation
                {
                    property = property             
                };

                animations.Add(animation);
            }

            animation.initialValue = GetPropertyValue(animation.property);
            animation.desiredValue = desiredValue;
            animation.percentageComplete = 0;
            animation.startTime = Time.time;
            animation.duration = duration;
            animation.onComplete = onComplete;            
        }        

        void Update()
        {
            if (!animations.Any()) return;            

            foreach (var animation in animations)
            {                
                animation.percentageComplete = (Time.time - animation.startTime) / animation.duration;                
                SetPropertyValue(animation.property, animation.currentValue);

                if (animation.percentageComplete >= 1)
                {
                    if (animation.onComplete != null) animation.onComplete.Invoke();
                }
            }

            animations.RemoveAll(a => a.percentageComplete >= 1);

            if (ResetWhenAnimationComplete && !animations.Any())
            {
                SetPropertyValue(DatePicker_Animation_Property.Alpha, 1);
                SetPropertyValue(DatePicker_Animation_Property.ScaleX, 1);
                SetPropertyValue(DatePicker_Animation_Property.ScaleY, 1);
            }
        }

        float GetPropertyValue(DatePicker_Animation_Property property)
        {
            switch (property)
            {
                case DatePicker_Animation_Property.Alpha:
                    return canvasGroup.alpha;
                case DatePicker_Animation_Property.ScaleX:
                    return rectTransform.localScale.x;
                case DatePicker_Animation_Property.ScaleY:
                    return rectTransform.localScale.y;
            }

            return 0f;
        }

        void SetPropertyValue(DatePicker_Animation_Property property, float newValue)
        {
            switch (property)
            {
                case DatePicker_Animation_Property.Alpha:
                    canvasGroup.alpha = newValue;
                    break;
                case DatePicker_Animation_Property.ScaleX:
                    rectTransform.localScale = new Vector3(newValue, rectTransform.localScale.y, rectTransform.localScale.z);
                    break;
                case DatePicker_Animation_Property.ScaleY:
                    rectTransform.localScale = new Vector3(rectTransform.localScale.x, newValue, rectTransform.localScale.z);
                    break;
            }
        }

        protected class DatePicker_Animation
        {
            public DatePicker_Animation_Property property;
            public float initialValue;
            public float desiredValue;
            public float startTime;
            public float percentageComplete;
            public float duration;

            public Action onComplete;

            public float currentValue
            {
                get
                {
                    return Mathf.Lerp(initialValue, desiredValue, percentageComplete);
                }
            }
        }

        public enum DatePicker_Animation_Property
        {
            Alpha,
            ScaleX,
            ScaleY
        }
    }    
}
