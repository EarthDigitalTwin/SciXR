using System;
using System.Collections.Generic;

[Serializable]
public class SerialData {
    public struct DataStep {
        public string name;
        public Dictionary<string, float[]> vars;
        public Dictionary<string, string> props;
        public int step;
        public float time;
    }

    public enum DataType {
        flat,
        globe,
        pointcloud
    };

    // Data fields
    public DataType type;
    public Dictionary<string, float[]> vars = new Dictionary<string, float[]>();
    public Dictionary<string, float[]> triVars = new Dictionary<string, float[]>();
    public Dictionary<string, string> props = new Dictionary<string, string>();
    public List<int> triangles;
    public string xIndexDefault, yIndexDefault, zIndexDefault, colorIndexDefault; // Set by data reader
    public int vertexCount;
    public int triangleCount;
    public string projection; // EPSG code

    // Animation properties
    public List<DataStep> results = new List<DataStep>();
    //[HideInInspector] public List<string> resultFields; 

    // Overlay Properties
    public float[] overlayX;
    public float[] overlayY;
    public float[] overlayHeight;
    public List<int> overlayTriangles;

    // Metadata properties
    public string identifier;
    public string runtimeName;
    public string fileName;
    public string filePath;
    public DateTime lastModified;
    public string notes;
    public DateTime start, end;
    public string description;
    public string instrument;
	public string time;
    public string variable;
    public string x_label;
    public string y_label;
    public string z_label;
    public string val_label;

    // Common shared scales properties
    public List<string> commonVariables;
    public float commonMin, commonMax;

}
