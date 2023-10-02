using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Highlighters;


public class HoverHighlight : MonoBehaviour {
    public enum ColorType {
        color,
        emission
    }

    public Renderer highlightRenderer;
    public Color highlightColor;
    public ColorType applyTo = ColorType.color;
    Color prevEmissionColor;
    VRTK_InteractableObject inter;
    List<GameObject> currentTouching = new List<GameObject>();
    string materialIndex {
        get {
            string typeLabel = null;
            switch (applyTo) {
                case ColorType.color:
                    typeLabel = "_Color";
                    break;
                case ColorType.emission:
                    typeLabel = "_EmissionColor";
                    break;
                default:
                    typeLabel = "_Color";
                    break;
            }
            return typeLabel;
        }
    }

    void Start() {
        prevEmissionColor = highlightRenderer.material.GetColor(materialIndex);
        inter = GetComponent<VRTK_InteractableObject>();
        inter.InteractableObjectTouched += Touched;
        inter.InteractableObjectUntouched += Untouched;
    }

    public void Touched(object sender, InteractableObjectEventArgs e) {
        if (inter.isGrabbable) {
            if (!currentTouching.Contains(e.interactingObject)) {
                currentTouching.Add(e.interactingObject);
            }

            if (currentTouching.Count <= 1) {
                LeanTween.cancel(gameObject);
                LeanTween.value(gameObject, prevEmissionColor, highlightColor, 0.2f).setOnUpdate((Color color) => { highlightRenderer.material.SetColor(materialIndex, color); });
            }
        }
    }
    public void Untouched(object sender, InteractableObjectEventArgs e) {
        if (inter.isGrabbable) {
            currentTouching.Remove(e.interactingObject);
            if (currentTouching.Count == 0) {
                LeanTween.cancel(gameObject);
                LeanTween.value(gameObject, highlightColor, prevEmissionColor, 0.2f).setOnUpdate((Color color) => { highlightRenderer.material.SetColor(materialIndex, color); });
            }
        }
    }

}
