using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorbarHelper : MonoBehaviour
{
    public DataObject obj;

    public void ChangeColorbar()
    {
        int newIndex = gameObject.GetComponent<ARDropdown>().selected;
        obj.SetColorbar(newIndex);
    }
}
