using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[AddComponentMenu("UI/VR Dropdown")]
public class VRDropdown : TMP_Dropdown {

    protected override GameObject CreateBlocker(Canvas rootCanvas) {
        GameObject newBlocker = base.CreateBlocker(rootCanvas);
        //newBlocker.AddComponent<VRCanvas>();
        return newBlocker;
    }
}
