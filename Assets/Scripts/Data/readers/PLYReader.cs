using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Debug = UnityEngine.Debug;

public class PLYReader {

    public static SerialFile MetadataFromPath(string path)
    {
        SerialFile dataFile = new SerialFile();

        dataFile.fileName = Path.GetFileName(path);
        //if (path.LastIndexOf("\\") > 0)
        //    dataFile.fileName = path.Substring(0, path.LastIndexOf("\\"));
        //else
        //    dataFile.fileName = path;

        dataFile.path = path;
        dataFile.lastModified = File.GetLastWriteTime(path);
        // Init bools
        dataFile.hasResults = false;
        dataFile.hasOverlay = false;

        string[] plyOutput = File.ReadAllLines(path);//ClientObject.GetFile(path);
        // Get vertex count
        foreach (string currentLine in plyOutput)
        {
            string[] items = currentLine.Split(null, 3);
            if (items != null && items.Length > 0)
            {
                switch (items[0])
                {
                    case "comment":
                        if (items[1] == "identifier:")
                            dataFile.identifier = items[2];
                        if (items[1] == "description:")
                            dataFile.description = items[2];
                        if (items[1] == "instrument:")
                            dataFile.instrument = items[2];
                        if (items[1] == "time:")
                            dataFile.time = items[2];
                        if (items[1] == "variable:")
                            dataFile.variable = items[2];
                        if (items[1] == "x_label:")
                            dataFile.x_label = items[2];
                        if (items[1] == "y_label:")
                            dataFile.y_label = items[2];
                        if (items[1] == "z_label:")
                            dataFile.z_label = items[2];
                        if (items[1] == "val_label:")
                            dataFile.val_label = items[2];
                        if (items[1] == "min_val:")
                            dataFile.min = float.Parse(items[2]);
                        if (items[1] == "max_val:")
                            dataFile.max = float.Parse(items[2]);
                        break;
                    case "element":
                        if (items[1] == "vertex")
                            dataFile.vertexCount = int.Parse(items[2]);
                        if (items[1] == "face")
                            dataFile.triangleCount = int.Parse(items[2]);
                        break;
                }
            }


            if (items[0] == "end_header")
                break;
        }
        dataFile.type = SerialData.DataType.pointcloud;
        if (dataFile.identifier == null || dataFile.identifier.Trim() == "")
        {
            dataFile.identifier = dataFile.fileName;
        }
        Debug.Log(dataFile.identifier);
        return dataFile;
    }

    public static void MetadataFromSerialFile(SerialFile dataFile) {
        // Init bools
        dataFile.hasResults = false;
        dataFile.hasOverlay = false;

        string[] plyOutput = File.ReadAllLines(dataFile.path);//ClientObject.GetFile(dataFile.path);
        // Get vertex count
        foreach (string currentLine in plyOutput) {
            string[] items = currentLine.Split(null, 3);
            if (items != null && items.Length > 0) {
                switch (items[0]) {
                    case "comment":
                        if (items[1] == "identifier:")
                            dataFile.identifier = items[2];
                        if (items[1] == "description:")
                            dataFile.description = items[2];
                        if (items[1] == "instrument:")
                            dataFile.instrument = items[2];
                        if (items[1] == "time:")
                            dataFile.time = items[2];
                        if (items[1] == "variable:")
                            dataFile.variable = items[2];
                        if (items[1] == "x_label:")
                            dataFile.x_label = items[2];
                        if (items[1] == "y_label:")
                            dataFile.y_label = items[2];
                        if (items[1] == "z_label:")
                            dataFile.z_label = items[2];
                        if (items[1] == "val_label:")
                            dataFile.val_label = items[2];
                        if (items[1] == "min_val:")
                            dataFile.min = float.Parse(items[2]);
                        if (items[1] == "max_val:")
                            dataFile.max = float.Parse(items[2]);
                        break;
                    case "element":
                        if(items[1] == "vertex")
                            dataFile.vertexCount = int.Parse(items[2]);
                        if (items[1] == "face")
                            dataFile.triangleCount = int.Parse(items[2]);
                        break;
                }
            }


            if (items[0] == "end_header")
                break;
        }
        dataFile.type = SerialData.DataType.pointcloud;
        Debug.Log(dataFile.identifier);
    }

    public static SerialData ReadModelFromPath(string path, SerialFile dataFile, DataObject dataObject, float loadWeight) {
        SerialData data = new SerialData();
        string[] plyOutput = File.ReadAllLines(path);//ClientObject.GetFile(path);
        if (dataFile == null)
            dataFile = MetadataFromPath(path);

        //Assign from DataFile
        string variable = dataFile.identifier.Substring(dataFile.identifier.IndexOf('_') + 1);

        data.identifier = dataFile.identifier;
        data.vertexCount = dataFile.vertexCount;
        data.triangleCount = dataFile.triangleCount;
        data.notes = dataFile.notes;
            
        data.fileName = dataFile.fileName;
        data.filePath = dataFile.path;
        data.lastModified = dataFile.lastModified;
        data.runtimeName = dataFile.runtimeName;
        data.type = dataFile.type;

        data.description = dataFile.description;
        data.instrument = dataFile.instrument;
		data.time = dataFile.time;
        data.variable = dataFile.variable;
        data.x_label = dataFile.x_label;
        data.y_label = dataFile.y_label;
        data.z_label = dataFile.z_label;
        data.val_label = dataFile.val_label;

    int line = 0;
        bool headerFlag = false;
        List<float> x = new List<float>();
        List<float> y = new List<float>();
        List<float> z = new List<float>();
        List<float> val = new List<float>();
        foreach (string currentLine in plyOutput) {
            string[] items = currentLine.Split(' ');

            // Skip header
            if (!headerFlag) {
                if (items[0] == "end_header") {
                    headerFlag = true;
                }
                continue;
            }

            x.Add(float.Parse(items[0]));
            y.Add(float.Parse(items[1]));
            z.Add(float.Parse(items[2]));
            val.Add(float.Parse(items[3]));

            if (dataObject != null)
            {
                if (line % 1000 == 0 || line == plyOutput.Length - 1)
                {
                    ThreadManager.instance.callbacks.Add(() => {
                        dataObject.UpdateLoadPercent((float)line / plyOutput.Length * loadWeight, "Parsing File");
                    });
                }
            }
            
            line++;

        }

        data.xIndexDefault = "mesh.x";
        data.yIndexDefault = "mesh.y";
        data.zIndexDefault = "mesh.z";
        data.colorIndexDefault = variable;

        data.vars[data.xIndexDefault] = x.ToArray();
        data.vars[data.yIndexDefault] = y.ToArray();
        data.vars[data.zIndexDefault] = z.ToArray();
        data.vars[data.colorIndexDefault] = val.ToArray();

        return data;
    }

    private static string ToTitleCase(string str) {
        if(str != "")
            return char.ToUpper(str[0]) + ((str.Length > 1) ? str.Substring(1) : string.Empty);
        return string.Empty;
    }
}