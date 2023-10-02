using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Dates
{
    /// <summary>
    /// This component will call 'ApplyDefaultTextMeshProProperties' on the TextMesh Pro instance it is attached to
    /// when the instance becomes active, and then it will remove itself (so this is a once-off process)
    /// </summary>
    [ExecuteInEditMode]
    public class DatePicker_TextMeshProDefaults : MonoBehaviour
    {
        public void OnEnable()
        {
            var tmp = GetComponent<TextMeshProUGUI>();

            if (tmp != null)
            {
                DatePicker_TextMeshProUtilities.ApplyDefaultTextMeshProProperties(tmp);
            }

            DestroyImmediate(this);
        }
    }
}
