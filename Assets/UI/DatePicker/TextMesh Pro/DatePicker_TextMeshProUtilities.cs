using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace UI.Dates
{
    internal static class DatePicker_TextMeshProUtilities
    {
        public static void ApplyDefaultTextMeshProProperties(TextMeshProUGUI tmp)
        {
            tmp.alignment = TextAlignmentOptions.Midline;
        }

        public static TextMeshProUGUI ReplaceTextElementWithTextMeshPro(Text text)
        {
            // cache attributes to copy over
            float fontSize = text.fontSize;
            bool resize = text.resizeTextForBestFit;
            float resizeMin = text.resizeTextMinSize;
            float resizeMax = text.resizeTextMaxSize;

            GameObject go = text.gameObject;

            // replace the old text element with TextMesh Pro
            GameObject.DestroyImmediate(text);

            var tmp = go.AddComponent<TextMeshProUGUI>();

            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = resize;
            tmp.fontSizeMin = resizeMin;
            tmp.fontSizeMax = resizeMax;

            // Some properties cannot be set on TMP if the instance is inactive, which is a possibility here
            if (!go.activeInHierarchy)
            {
                go.AddComponent<DatePicker_TextMeshProDefaults>();
            }
            else
            {
                ApplyDefaultTextMeshProProperties(tmp);
            }

            return tmp;
        }

        public static TMP_InputField ReplaceInputFieldWithTextMeshPro(InputField field)
        {
            var originalTextElement = field.textComponent;
            var originalText = originalTextElement.text;
            var originalFontSize = originalTextElement.fontSize;
            var originalFont = originalTextElement.font;

            var originalPlaceHolder = field.placeholder as Text;
            var originalPlaceHolderText = originalPlaceHolder != null ? originalPlaceHolder.text : "";
            var originalPlaceHolderFontSize = originalPlaceHolder != null ? originalPlaceHolder.fontSize : 14;
            var originalPlaceHolderFont = originalPlaceHolder != null ? originalPlaceHolder.font : null;

            var transform = field.transform as RectTransform;

            var colors = field.colors;

            if (originalPlaceHolder != null) GameObject.DestroyImmediate(originalPlaceHolder.gameObject);

            GameObject.DestroyImmediate(originalTextElement.gameObject);
            GameObject.DestroyImmediate(field);

            var tmpField = CreateTMPInputField(transform);

            tmpField.colors = colors;

            var textArea = CreateTextArea(tmpField.transform as RectTransform);

            var placeHolder = CreatePlaceholder(textArea).GetComponent<TextMeshProUGUI>();
            var tmp = CreateText(textArea).GetComponent<TextMeshProUGUI>();

            placeHolder.GetComponent<TextMeshProUGUI>().text = originalPlaceHolderText;
            tmp.text = originalText;

            tmpField.textComponent = tmp;
            tmpField.placeholder = placeHolder;
            tmpField.textViewport = textArea;

            tmp.fontSize = originalFontSize;
            placeHolder.fontSize = originalPlaceHolderFontSize;

            var newFont = GetTMPFontFromOldFont(originalFont);
            if (newFont != null) tmp.font = newFont;

            if (originalPlaceHolderFont != null)
            {
                var newPlaceHolderfont = GetTMPFontFromOldFont(originalPlaceHolderFont);
                if (newPlaceHolderfont != null) placeHolder.font = newPlaceHolderfont;
            }

            return tmpField;
        }

        /// <summary>
        /// Note: Only works in the editor.
        /// </summary>
        /// <param name="oldFont"></param>
        /// <returns></returns>
        public static TMP_FontAsset GetTMPFontFromOldFont(Font oldFont)
        {
#if UNITY_EDITOR
            var tmpFont = UnityEditor.AssetDatabase.FindAssets(oldFont.name + " t:TMP_FontAsset").FirstOrDefault();
            if (tmpFont != null)
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(tmpFont));
            }
#endif
          return null;
        }

        private static TMP_InputField CreateTMPInputField(RectTransform t)
        {
            var tmpInputField = t.gameObject.AddComponent<TMP_InputField>();
            tmpInputField.interactable = false;

            return tmpInputField;
        }

        private static RectTransform CreateTextArea(RectTransform t)
        {
            var go = new GameObject("Text Area", typeof(RectTransform));

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.SetParent(t);

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            rectTransform.offsetMin = new Vector2(10, 6);
            rectTransform.offsetMax = new Vector2(-10, -7);

            rectTransform.localScale = Vector3.one;

            go.AddComponent<RectMask2D>();

            return rectTransform;
        }

        private static RectTransform CreatePlaceholder(RectTransform textArea)
        {
            var go = new GameObject("Placeholder", typeof(RectTransform));
            var tmp = go.AddComponent<TextMeshProUGUI>();

            var rectTransform = go.GetComponent<RectTransform>();

            rectTransform.SetParent(textArea);

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.one;

            rectTransform.localScale = Vector3.one;

            tmp.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            tmp.text = "Enter text...";
            //tmp.fontStyle = FontStyles.Italic;
            tmp.fontSize = 14f;
            tmp.alignment = TextAlignmentOptions.TopLeft;

            tmp.raycastTarget = false;

            return rectTransform;
        }

        private static RectTransform CreateText(RectTransform textArea)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            var tmp = go.AddComponent<TextMeshProUGUI>();

            var rectTransform = go.GetComponent<RectTransform>();

            rectTransform.SetParent(textArea);

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.one;

            rectTransform.localScale = Vector3.one;

            tmp.color = new Color(0.2f, 0.2f, 0.2f);
            tmp.fontSize = 14f;
            tmp.alignment = TextAlignmentOptions.TopLeft;

            tmp.raycastTarget = false;

            return rectTransform;
        }
    }
}
