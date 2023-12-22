using UnityEngine;
using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Dates
{
    public static class DatePicker_Instantiation
    {
        private static Dictionary<string, UnityEngine.Object> m_CachedResources = new Dictionary<string, UnityEngine.Object>(StringComparer.OrdinalIgnoreCase);

        public static void FixInstanceTransform(RectTransform baseTransform, RectTransform instanceTransform)
        {            
            //instanceTransform.localScale = baseTransform.localScale;
            instanceTransform.localScale = Vector3.one;
            instanceTransform.anchoredPosition = baseTransform.anchoredPosition;
            instanceTransform.sizeDelta = baseTransform.sizeDelta;
            instanceTransform.anchoredPosition3D = new Vector3(baseTransform.anchoredPosition3D.x, baseTransform.anchoredPosition3D.y, 0);
            instanceTransform.localPosition = Vector3.zero;
            instanceTransform.localRotation = new Quaternion();
        }

        public static GameObject InstantiatePrefab(string name, bool playMode = false, bool generateUndo = true, Transform parent = null)
        {
            var prefab = DatePicker_Instantiation.LoadResource<GameObject>("Prefabs/" + name);

            if (prefab == null)
            {
                throw new UnityException(String.Format("Could not find prefab '{0}'!", name));
            }            

#if UNITY_EDITOR
            if(!playMode) parent = UnityEditor.Selection.activeTransform;            
#endif
            var gameObject = GameObject.Instantiate(prefab) as GameObject;
            gameObject.name = name;

            if (parent == null || !(parent is RectTransform))
            {
                parent = GetCanvasTransform();
            }

            gameObject.transform.SetParent(parent);

            var transform = (RectTransform)gameObject.transform;
            var prefabTransform = (RectTransform)prefab.transform;

            FixInstanceTransform(prefabTransform, transform);            

            // Override the prefab's visible date with today's date
            var datePicker = gameObject.GetComponent<DatePicker>();            
            if(datePicker != null) datePicker.VisibleDate = DateTime.Today;

#if UNITY_EDITOR
            if (generateUndo)
            {                
                UnityEditor.Undo.RegisterCreatedObjectUndo(gameObject, "Created " + name);
            }
#endif

            return gameObject;
        }

        public static Transform GetCanvasTransform()
        {
            Canvas canvas = null;
#if UNITY_EDITOR
            // Attempt to locate a canvas object parented to the currently selected object
            if (!Application.isPlaying && UnityEditor.Selection.activeGameObject != null)
            {
                canvas = FindParentOfType<Canvas>(UnityEditor.Selection.activeGameObject);
                //canvas = UnityEditor.Selection.activeTransform.GetComponentInParent<Canvas>();                
            }
#endif

            if (canvas == null)
            {
                // Attempt to find a canvas anywhere
                canvas = UnityEngine.Object.FindObjectOfType<Canvas>();                

                if (canvas != null) return canvas.transform;                
            }

            // if we reach this point, we haven't been able to locate a canvas
            // ...So I guess we'd better create one
            

            GameObject canvasGameObject = new GameObject("Canvas");
            canvasGameObject.layer = LayerMask.NameToLayer("UI");
            canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGameObject.AddComponent<CanvasScaler>();
            canvasGameObject.AddComponent<GraphicRaycaster>();

#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(canvasGameObject, "Create Canvas");
#endif

            var eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();

            if (eventSystem == null)
            {
                GameObject eventSystemGameObject = new GameObject("EventSystem");
                eventSystem = eventSystemGameObject.AddComponent<EventSystem>();
                eventSystemGameObject.AddComponent<StandaloneInputModule>();

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
                eventSystemGameObject.AddComponent<TouchInputModule>();
#endif

#if UNITY_EDITOR
                UnityEditor.Undo.RegisterCreatedObjectUndo(eventSystemGameObject, "Create EventSystem");
#endif
            }

            return canvas.transform;
        }

        public static T FindParentOfType<T>(GameObject childObject)
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
                
        public static T LoadResource<T>(string path, bool ignoreCache = false)
            where T : UnityEngine.Object
        {
            UnityEngine.Object resource = null;

            if (!ignoreCache)
            {
                // include the type name in the cache path, so that if we try to cache two different objects that share the same path, we can still access them both
                var cachePath = String.Format("{0}|{1}", typeof(T).Name, path);                

                if (!m_CachedResources.TryGetValue(cachePath, out resource))
                {
                    resource = Resources.Load<T>(path);

                    if (resource != null)
                    {
                        m_CachedResources.Add(cachePath, resource);                        
                    }                    
                }                
            }
            else
            {
                resource = Resources.Load<T>(path);
            }            

            return resource as T;
        }      
    }  
}
