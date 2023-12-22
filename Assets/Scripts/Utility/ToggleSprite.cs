using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleSprite : MonoBehaviour {
    public Image targetImage;
    public Sprite swapSprite;
    public bool toggleColor = false;
    public Color swapColor = Color.white;

    private Toggle toggle;
    private Sprite originalSprite;
    private Color originalColor;

    void Start() {
        toggle = GetComponent<Toggle>();
        originalSprite = targetImage.sprite;
        originalColor = toggle.colors.normalColor;
        toggle.onValueChanged.AddListener(Toggle);
        Toggle(toggle.isOn);
    }

    public void Toggle(bool status) {
        if (!status) {
            targetImage.sprite = originalSprite;
            if (toggleColor) {
                ColorBlock newColors = toggle.colors;
                newColors.normalColor = originalColor;
                toggle.colors = newColors;
            }
        }
        else {
            targetImage.sprite = swapSprite;
            if (toggleColor) {
                ColorBlock newColors = toggle.colors;
                newColors.normalColor = swapColor;
                toggle.colors = newColors;
            }
        }
    }
}
