using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ARDropdown : MonoBehaviour
{
    public List<GameObject> options = new List<GameObject>();
    public List<GameObject> checks = new List<GameObject>();
    public int selected = 0;
    public TextMeshProUGUI label;
    public UnityEvent OnChangeSelected;

    public void Start()
    {
        if (options.Count > 0)
            ChangeSelected(options[selected]);
    }

    public void ChangeSelected(GameObject obj)
    {
        label.text = obj.name;

        selected = options.IndexOf(obj);

        for (int i = 0; i < checks.Count; i++)
        {
            if (i == selected)
            {
                checks[i].SetActive(true);
            }
            else
            {
                checks[i].SetActive(false);
            }
        }

        OnChangeSelected.Invoke();
    }


    // For use over network or when we don't want to invoke OnChangeSelected
    public void ChangeSelected(int index)
    {
        GameObject selectedObj = options[index];

        label.text = selectedObj.name;

        for (int i = 0; i < checks.Count; i++)
        {
            if (i == index)
            {
                checks[i].SetActive(true);
            }
            else
            {
                checks[i].SetActive(false);
            }
        }
    }
}
