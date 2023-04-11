using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;

public class ModelJSReader {
    //public static Dictionary<string, string> stepKeyLookup = new Dictionary<string, string> {
    //    { "Vx" , "initialization.vx" },
    //    { "Vy" , "initialization.vy" },
    //    { "Vel" , "initialization.vel" },
    //    { "Pressure" , "initialization.pressure" },
    //    { "Thickness" , "geometry.thickness" },
    //    { "Surface" , "geometry.surface" },
    //    { "Base" , "geometry.base" },
    //    { "SmbMassBalance" , "smb.mass_balance" },
    //};

    static List<string> commonVariables = new List<string> {"Surface","Base","geometry.base","geometry.surface","geometry.bed"};

    public static SerialFile MetadataFromPath(string path)
    {
        SerialFile dataFile = new SerialFile();

        dataFile.fileName = Path.GetFileName(path);
        //if (path.LastIndexOf("\\") > 0)
        //    dataFile.fileName = path.Substring(path.LastIndexOf("\\") + 1);
        //else
        //    dataFile.fileName = path;

        Debug.Log("MetadataFromPath: " + dataFile.fileName);

        dataFile.path = path;
        dataFile.lastModified = File.GetLastWriteTime(path);
        // Init bools
        dataFile.hasResults = false;
        dataFile.hasOverlay = false;

        string[] jsOutput = File.ReadAllLines(path);//ClientObject.GetFile(path);
        
        bool typeSet = false;

        var sr = File.CreateText(dataFile.fileName);

        // Get vertex count
        foreach (string currentLine in jsOutput)
        {
            sr.WriteLine(currentLine);
            int equalSignIndex = currentLine.IndexOf('=');
            int dotIndex = currentLine.IndexOf('.');
            if (dotIndex != -1)
            {
                string label = currentLine.Substring(dotIndex + 1, equalSignIndex - dotIndex - 1);

                string extractedJSONString = currentLine.Substring(equalSignIndex + 1).TrimEnd(';');
                switch (label)
                {
                    case "miscellaneous.name":
                        dataFile.identifier = ToTitleCase(extractedJSONString.Trim(new char[] { '\'', '\"' }));
                        break;
                    case "mesh.numberofvertices":
                        dataFile.vertexCount = int.Parse(extractedJSONString);
                        break;
                    case "mesh.numberofelements":
                        dataFile.triangleCount = int.Parse(extractedJSONString);
                        break;
                    case "mesh.z":
                        typeSet = true;
                        dataFile.type = SerialData.DataType.globe;
                        break;
                    case "priv.runtimename":
                        dataFile.runtimeName = extractedJSONString.Trim(new char[] { '\'', '\"' });
                        break;
                    case "miscellaneous.notes":
                        dataFile.notes = extractedJSONString.Trim(new char[] { '\'', '\"' });
                        break;
                    case "radaroverlay.outerx":
                        dataFile.hasOverlay = true;
                        break;
                    case "results":
                        dataFile.hasResults = true;
                        break;
                }
            }
        }
        sr.Close();
        if (!typeSet)
            dataFile.type = SerialData.DataType.flat;
        if (dataFile.identifier == null || dataFile.identifier.Trim() == "")
            dataFile.identifier = dataFile.fileName;
        return dataFile;
    }

    public static void MetadataFromSerialFile(SerialFile dataFile) {
        // Init bools
        dataFile.hasResults = false;
        dataFile.hasOverlay = false;

        string[] jsOutput = File.ReadAllLines(dataFile.path);//ClientObject.GetFile(dataFile.path);
        bool typeSet = false;

        // Get vertex count
        foreach (string currentLine in jsOutput) {
            int equalSignIndex = currentLine.IndexOf('=');
            int dotIndex = currentLine.IndexOf('.');
            if (dotIndex != -1) {
                string label = currentLine.Substring(dotIndex + 1, equalSignIndex - dotIndex - 1);

                string extractedJSONString = currentLine.Substring(equalSignIndex + 1).TrimEnd(';');
                switch (label) {
                    case "miscellaneous.name":
                        dataFile.identifier =  ToTitleCase(extractedJSONString.Trim(new char[] { '\'', '\"' }));
                        break;
                    case "mesh.numberofvertices":
                        dataFile.vertexCount = int.Parse(extractedJSONString);
                        break;
                    case "mesh.numberofelements":
                        dataFile.triangleCount = int.Parse(extractedJSONString);
                        break;
                    case "mesh.z":
                        typeSet = true;
                        dataFile.type = SerialData.DataType.globe; 
                        break;
                    case "priv.runtimename":
                        dataFile.runtimeName = extractedJSONString.Trim(new char[] { '\'', '\"' });
                        break;
                    case "miscellaneous.notes":
                        dataFile.notes = extractedJSONString.Trim(new char[] { '\'', '\"' });
                        break;
                    case "radaroverlay.outerx":
                        dataFile.hasOverlay = true;
                        break;
                    case "results":
                        dataFile.hasResults = true;
                        break;
                }
            }
        }
        if (!typeSet)
            dataFile.type = SerialData.DataType.flat;
    }

    public static SerialData ReadModelFromPath(string path, SerialFile dataFile, DataObject dataObject, float loadWeight) {
        SerialData data = new SerialData();
        //Debug.Log("Starting file read...");
        string[] jsOutput = File.ReadAllLines(dataFile.path);//ClientObject.GetFile(dataFile.path);
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

        int line = 0;
        foreach (string currentLine in jsOutput) {
            int equalSignIndex = currentLine.IndexOf('=');
            int dotIndex = currentLine.IndexOf('.');

            if(dotIndex != -1) {
                string label = currentLine.Substring(dotIndex+1, equalSignIndex - dotIndex - 1);
                string extractedJSONString = currentLine.Substring(equalSignIndex + 1).TrimEnd(';');
                if (extractedJSONString[0] == '[' || label == "mesh.epsg") {
                    SimpleJSON.JSONNode currentData = SimpleJSON.JSON.Parse(extractedJSONString);
                    switch (label) {
                        case "mesh.epsg":
                            data.projection = currentLine.Split('=')[1].Replace(";","");
                            break;
                        case "mesh.elements":
                            data.triangles = ConvertTriArray(currentData.AsArray);
                            break;
                        case "radaroverlay.outerx":
                            data.overlayX = ConvertJSONArray(currentData.AsArray);
                            break;
                        case "radaroverlay.outery":
                            data.overlayY = ConvertJSONArray(currentData.AsArray);
                            break;
                        case "radaroverlay.outerheight":
                            data.overlayHeight = ConvertJSONArray(currentData.AsArray);
                            break;
                        case "radaroverlay.outerindex":
                            data.overlayTriangles = ConvertTriArray(currentData.AsArray);
                            break;
                        case "results":
                            data.results = ParseResults(currentData);
                            break;
                        default:
                            if (currentData.IsArray && currentData.Count == data.vertexCount && currentData[0].IsNumber) {
                                data.vars[label] = ConvertJSONArray(currentData.AsArray);
                            }
                            else if(currentData.IsArray && currentData.Count == data.triangleCount && currentData[0].IsNumber) {
                                data.triVars[label] = ConvertJSONArray(currentData.AsArray);
                            }
                            break;
                    }
                }
                else {
                    //TODO add prop handling
                    data.props[label] = currentLine;
                }
                if (dataObject != null)
                {
                    ThreadManager.instance.callbacks.Add(() => {
                        dataObject.UpdateLoadPercent((float)line / jsOutput.Length * loadWeight, "Parsing File");
                    });
                }

                line++;
            }
        }

        //TODO add default string index lookups
        data.xIndexDefault = "mesh.x";
        data.yIndexDefault = "mesh.y";

        if (data.vars.ContainsKey("mesh.z")) {
            data.type = SerialData.DataType.globe;
            //data.xIndexDefault = "mesh.lat";
            //data.yIndexDefault = "mesh.long";
            data.zIndexDefault = "mesh.z";
            data.colorIndexDefault = "geometry.surface";
        }
        else {
            data.type = SerialData.DataType.flat;

            if (data.results.Count > 0 && data.results[0].vars.ContainsKey("Surface")) {
                data.zIndexDefault = "Surface";
            }
            else {
                data.zIndexDefault = "geometry.surface";
            }

            if (data.results.Count > 0 && data.results[0].vars.ContainsKey("Thickness")) {
                data.colorIndexDefault = "Thickness";
            }
            else {
                data.colorIndexDefault = "geometry.thickness";
            }
        }

        // Calculate common min maxes
        bool firstFlag = true;
        foreach (string var in commonVariables) {
            float[] varArray = null;
            if(data.vars.ContainsKey(var)) {
                varArray = data.vars[var];
            }
            if(data.results != null && data.results.Count > 0 && data.results[0].vars.ContainsKey(var)) {
                varArray = data.results[0].vars[var];
            }

            if(varArray != null) {
                float min = varArray.Min();
                float max = varArray.Max();
                if (firstFlag) {
                    data.commonMin = min;
                    data.commonMax = max;
                    firstFlag = false;
                } else {
                    if (min < data.commonMin) {
                        data.commonMin = min;
                    }
                    if (max > data.commonMax) {
                        data.commonMax = max;
                    }
                }
            }
        }
        data.commonMin = 0;
        data.commonMin = data.commonMin - (data.commonMax - data.commonMin) * 0.5f;
        //data.commonMax = data.commonMax + (data.commonMax - data.commonMin) * 0.33f;
        data.commonVariables = commonVariables;

        //foreach(String result in newData.vars.Keys) {
        //    Debug.Log(result);
        //}

        return data;
    }

    private static List<SerialData.DataStep> ParseResults (SimpleJSON.JSONNode resultsData) {
        List<SerialData.DataStep> results = null;
        if (resultsData.IsArray) {
            results = new List<SerialData.DataStep>();
            int resultCount = 0;
            foreach (SimpleJSON.JSONNode result in resultsData.AsArray) {
                SerialData.DataStep newStep = new SerialData.DataStep();
                newStep.vars = new Dictionary<string, float[]>();
                newStep.props = new Dictionary<string, string>();
                foreach(string key in result.Keys) {
                    newStep.name = "Step" + resultCount;
                    if(result[key].IsArray) {
                        newStep.vars.Add(key, ConvertJSONArray(result[key].AsArray));
                    }
                    else if(key == "step") {
                        newStep.step = result[key].AsInt;
                    }
                    else if(key == "time") {
                        newStep.time = result[key].AsFloat;
                    }
                    else if(result[key].IsNumber) {
                        newStep.props.Add(key, result[key].ToString());
                    }
                    else {
                        newStep.props.Add(key, result[key].ToString());
                    }
                    resultCount++;
                }
                results.Add(newStep);
            }
        }

        return results;
    }

    private static float[] ConvertJSONArray(SimpleJSON.JSONArray input) {
        List<float> final = new List<float>();
        foreach (SimpleJSON.JSONNode item in input) {
            final.Add(item.AsFloat);
        }
        return final.ToArray();
    }

    private static List<int> ConvertTriArray(SimpleJSON.JSONArray input) {
        List<int> final = new List<int>();
        foreach (SimpleJSON.JSONNode item in input) {
            final.Add(item[0].AsInt - 1);
            final.Add(item[1].AsInt - 1);
            final.Add(item[2].AsInt - 1);
        }
        return final;
    }

    private static string ToTitleCase(string str) {
        if(str != "")
            return char.ToUpper(str[0]) + ((str.Length > 1) ? str.Substring(1) : string.Empty);
        return string.Empty;
    }
}
