using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

[RequireComponent(typeof(Slider))]
public class SliderDataValueDisplay : MonoBehaviour {
    public enum Field {
        Min,
        Max,
        Focus,
        Range
    }

    public Field field;
    public DataObject data {
        get {
            if (_data != null) {
                return _data;
            }
            _data = GetComponentInParent<DataObject>();
            return _data;
        }
    }

    public Text text {
        get {
            if(_text != null) {
                return _text;
            }
            _text = GetComponent<Text>();
            return _text;
        }
    }

    DataObject _data;
    Text _text;

    // Update is called once per frame
    public void DisplayValue(float val) {
        if(field == Field.Min || field == Field.Max || field == Field.Focus) {
            text.text = Mathf.Lerp(data.ColorMin, data.ColorMax, val).ToString();
        }
        else if(field == Field.Range) {
            text.text = Mathf.Lerp(0, data.ColorMax - data.ColorMin, val).ToString();
        }
    }
}
