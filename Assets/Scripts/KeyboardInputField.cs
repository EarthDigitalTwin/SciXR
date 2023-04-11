using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class KeyboardInputField : MonoBehaviour
{
    public TextMeshPro text;
    public string input;
    public string defaultText = "";
    public UnityEvent OnSubmit = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshPro>();
        text.text = defaultText;
        input = defaultText;
    }

    public void OnKeyPressed(GameObject key)
    {
        if (key.name == "Submit")
        {
            OnSubmit.Invoke();
        }
        else if (key.name == "Delete")
        {
            input = input.Substring(0, input.Length - 1);
            text.text = input;
        }
        else
        {
            input += key.name;
            text.text = input;
        }
    }
}
