using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SerialFile {
    public string path;
    public DateTime lastModified;

    // Generic Metadata properties
    public string identifier;
    public int vertexCount;
    public int triangleCount;
    public bool hasResults;
    public bool hasOverlay;
    public string runtimeName;
    public string fileName;
    public string notes;
    public string description;
    public string instrument;
    public string time;
    public SerialData.DataType type;

    //PLY properties
    public float min;
    public float max;
    public string variable;
    public string x_label;
    public string y_label;
    public string z_label;
    public string val_label;
}
