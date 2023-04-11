using System;

[Serializable]
public class SerialCity {
    public string name;
    public float x, y, z;
    public float[] grad;
    public float[] gradperarea;
    public float[] deltaH;
    public float[] deltaLSL;


    public float[] normalizedGradient;
    public float[] normalizedThickness;
    public float[] normalizedLSL;
}
