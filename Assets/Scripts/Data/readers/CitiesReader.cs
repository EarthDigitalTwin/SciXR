using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

public class CitiesReader {

    [Serializable]
    public struct CityLocation {
        public float x;
        public float y;
        public float z;
    }

    public static void SaveCities(SortedDictionary<string, SerialCity> cities, string citiesDataPath) {
        if (!File.Exists(citiesDataPath)) {
            Directory.CreateDirectory(citiesDataPath);
        }
        foreach (SerialCity city in cities.Values) {
            string fullPath = Path.Combine(citiesDataPath, city.name + ".bin");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(fullPath, FileMode.Create);
            bf.Serialize(stream, city);
            stream.Close();
        }
    }

    public static SortedDictionary<string, SerialCity> ReadEmptyCitiesFromJS(string path) {
        SortedDictionary<string, SerialCity> newCities = new SortedDictionary<string, SerialCity>();
        string[] jsOutput = File.ReadAllLines(path);
        for (int lineCount = 0; lineCount < jsOutput.Length; lineCount++) {
            if (jsOutput[lineCount].StartsWith("xcity")) {
                SerialCity newData = new SerialCity();

                string[] itemsX = jsOutput[lineCount].Split('\"');
                string[] itemsY = jsOutput[lineCount + 1].Split('\"');
                string[] itemsZ = jsOutput[lineCount + 2].Split('\"');
                newData.name = itemsX[1];
                newData.x = float.Parse(itemsX[2].Trim(new char[] { ']', '=', ';' }));
                newData.y = float.Parse(itemsZ[2].Trim(new char[] { ']', '=', ';' }));
                newData.z = float.Parse(itemsY[2].Trim(new char[] { ']', '=', ';' }));
                newCities.Add(newData.name, newData);
            }
        }

        return newCities;
    }

    public static SerialCity ReadCityFromBinary(string path) {
        string data = File.ReadAllText(path);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        SerialCity cityData = bf.Deserialize(stream) as SerialCity;
        stream.Close();
        return cityData;
    }

    public static SortedDictionary<string, SerialCity> ReadCitiesFromPath(SerialData data, string path, string dataPath) {
        return ReadCitiesFromPath(data, path, dataPath, 1);
    }

    public static SortedDictionary<string, SerialCity> ReadCitiesFromPath(SerialData data, string path, string dataPath, float radius) {

        SortedDictionary<string, SerialCity> newCities = new SortedDictionary<string, SerialCity>();

        float[] deltaH = data.triVars["slr.deltathickness"];
        if (deltaH !=  null && deltaH.Length > 0) {
            string[] sqlOutput = File.ReadAllLines(path);
            foreach (string currentLine in sqlOutput) {
                if (currentLine.StartsWith("INSERT INTO cities VALUES")) {
                    SerialCity newCity = new SerialCity();
                    string[] elements = currentLine.Split('\'');
                    newCity.name = elements[1];

                    SimpleJSON.JSONNode dataNode = SimpleJSON.JSON.Parse(elements[3]);
                    newCity.x = dataNode["xcity"];
                    newCity.y = dataNode["ycity"];
                    newCity.z = dataNode["zcity"];

                    Vector3 normalizedLocation = new Vector3(newCity.x, newCity.y, newCity.z).normalized;

                    newCity.grad = ConvertJSONArray(dataNode["grad"].AsArray);
                    newCity.gradperarea = ConvertJSONArray(dataNode["gradperarea"].AsArray);
                    newCity.deltaH = deltaH;
                    newCity.deltaLSL = newCity.gradperarea.Zip(newCity.deltaH, (x, y) => x * y * 1e15f).ToArray();
                    newCity.gradperarea = newCity.gradperarea.Select(r => r * 1e15f).ToArray();

                    Debug.Log("Process gradient");
                    newCity.normalizedGradient = GenerateNormalizedFromTriData(newCity.gradperarea, -3, 3);
                    Debug.Log("Process lsl");
                    newCity.normalizedLSL = GenerateNormalizedFromTriData(newCity.deltaLSL, -0.5f, 0.5f);
                    Debug.Log("Process thickness");
                    newCity.normalizedThickness = GenerateNormalizedFromTriData(newCity.deltaH, -0.2f, 0.2f);

                    newCities.Add(newCity.name, newCity);
                }

            }
        }
        return newCities;
    }


    private static float[] ConvertJSONArray(SimpleJSON.JSONArray input) {
        List<float> final = new List<float>();
        foreach (SimpleJSON.JSONNode item in input) {
            final.Add(item.AsFloat);
        }
        return final.ToArray();
    }


    public static float[]  GenerateNormalizedFromTriData(float[] triangleData) {
        //triangleData = triangleData.Select(r => r * 1e15f).ToArray();
        float min = triangleData.Min();
        float max = triangleData.Max();
        return GenerateNormalizedFromTriData(triangleData, min , max);
    }

    public static float[] GenerateNormalizedFromTriData(float[] triangleData, float min, float max) {
        List<float> newNorms = new List<float>();


        for (int triCount = 0; triCount < triangleData.Length; triCount ++) {
            int triIndex = triCount * 3;
            float normalizedVal = Mathf.InverseLerp(min, max, triangleData[triCount]);
            newNorms.Add(normalizedVal);
            newNorms.Add(normalizedVal);
            newNorms.Add(normalizedVal);
        }
        return newNorms.ToArray();
    }
}
