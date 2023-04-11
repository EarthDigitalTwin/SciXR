using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Colorbar {

    public string name;
    public List<Color> rgbColors = new List<Color>();
    public Texture texture;

    public Colorbar(string name) {
        this.name = name;
    }

}
