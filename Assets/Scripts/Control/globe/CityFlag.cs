using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TMP_Text;

public class CityFlag : MonoBehaviour {
    public GameObject flag;
    public Text label;

    public void DisableFlag() {
        if(flag.activeSelf)
            flag.SetActive(false);
    }
    public void EnableFlag() {
        if(!flag.activeSelf)
            flag.SetActive(true);
    }
}
