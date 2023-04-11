using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerHUDPosition : MonoBehaviour
{
    int currentWidth;
    int currentHeight;

    NetworkManagerHUD hud;

    // Start is called before the first frame update
    void Start()
    {
        currentWidth = Screen.width;
        currentHeight = Screen.height;

        hud = gameObject.GetComponent<NetworkManagerHUD>();

        UpdateHUD();
    }

    // Update is called once per frame
    void Update()
    {
        int w = Screen.width;
        int h = Screen.height;

        if (w != currentWidth)
        {
            currentWidth = w;
            UpdateHUD();
        }

        if (h != currentHeight)
        {
            currentHeight = h;
            UpdateHUD();
        }
    }

    void UpdateHUD()
    {
        hud.offsetX = 39 * currentWidth / 48;
        hud.offsetY = currentHeight / 50;
        hud.width = currentWidth / 6;
    }
}
