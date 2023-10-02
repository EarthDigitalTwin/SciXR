using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Accord.IO;
using Debug = UnityEngine.Debug;

public class MatLabReader {



    public static SerialFile MetadataFromPath(string path) {
        MatReader reader = new MatReader(path);
        

        SerialFile dataFile = new SerialFile();

        dataFile.fileName = Path.GetFileName(path);
        dataFile.path = path;
        dataFile.lastModified = File.GetLastWriteTime(path);
        // Init bools
        dataFile.hasResults = false;
        dataFile.hasOverlay = false;


        
        dataFile.type = SerialData.DataType.flat;
        if (dataFile.identifier == null || dataFile.identifier.Trim() == "")
            dataFile.identifier = Path.GetFileName(path);
        return dataFile;
    }

    public static SerialData ReadModelFromPath(string path, SerialFile dataFile, DataObject dataObject, float loadWeight) {
        SerialData data = new SerialData();
        string[] plyOutput = File.ReadAllLines(path);
        if (dataFile == null)
            dataFile = MetadataFromPath(path);

        //Assign from DataFile
        data.identifier = dataFile.identifier;
        data.vertexCount = dataFile.vertexCount;
        data.triangleCount = dataFile.triangleCount;
        data.notes = dataFile.notes;

        data.fileName = dataFile.fileName;
        data.filePath = dataFile.path;
        data.lastModified = dataFile.lastModified;
        data.runtimeName = dataFile.runtimeName;
        data.type = dataFile.type;

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

            if (line % 1000 == 0 || line == plyOutput.Length - 1) {
                ThreadManager.instance.callbacks.Add(() => {
                    dataObject.UpdateLoadPercent((float)line / plyOutput.Length * loadWeight, "Parsing File");
                });
            }
            line++;

        }

        data.xIndexDefault = "mesh.x";
        data.yIndexDefault = "mesh.y";
        data.zIndexDefault = "mesh.z";
        data.colorIndexDefault = "value";

        data.vars[data.xIndexDefault] = x.ToArray();
        data.vars[data.yIndexDefault] = y.ToArray();
        data.vars[data.zIndexDefault] = z.ToArray();
        data.vars[data.colorIndexDefault] = val.ToArray();

        return data;
    }
    
}
