using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TMP_Text;

public class LoadingBar : MonoBehaviour {

    public GameObject loadbar;
    public Text loadPercentText;
    public Text statusText;

    public GameObject overlayUI; 

    public void UpdateLoadBar(float newPercent, string status) {
        loadbar.transform.localScale = new Vector3(newPercent, transform.localScale.y, transform.localScale.z);
        loadPercentText.text = Mathf.Round(newPercent * 100).ToString() + "%";
        statusText.text = status;
        if(newPercent >= 0.99) {

            gameObject.SetActive(false);
            //overlayUI.SetActive(true);
        }
    }

}
