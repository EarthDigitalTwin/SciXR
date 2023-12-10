/*
 * This script is used to fetch SDAP data using an SDAP config YAML file and save it to a PLY file.
 * It was adapted from sdap-xr.py, because 1. calling out to a Python subprocess isn't good in general; and 2. it doesn't work on Android.
 */

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;

public class LatLonDataArray
{
    public double[,] Data { get; set; }

    // mapping from latitudes/longitudes to indices in the data array
    public Dictionary<double, int> LatMapping { get; set; }
    public Dictionary<double, int> LonMapping { get; set; }

    // list of latitudes/longitudes
    public List<double> Latitudes { get; set; }
    public List<double> Longitudes { get; set; }

    public LatLonDataArray(int latSize, int lonSize)
    {
        Data = new double[latSize, lonSize];
        LatMapping = new Dictionary<double, int>();
        LonMapping = new Dictionary<double, int>();
    }

    public void SetValueAt(double lat, double lon, double value)
    {
        Data[LatMapping[lat], LonMapping[lon]] = value;
    }

    public double GetValueAt(double lat, double lon)
    {
        return Data[LatMapping[lat], LonMapping[lon]];
    }
}

public class SdapToPly : MonoBehaviour
{
    string dtFormat = "yyyy-MM-ddTHH:mm:ssZ";
    private string header = @"ply
format ascii 1.0
comment identifier: {0}
comment description: {1}
comment instrument: {2}
comment time: {3}
comment variable: {4}
comment max_val: {5}
comment min_val: {6}
comment x_label: Longitude
comment y_label: Latitude
comment z_label: {7}
comment val_label: {8} {9}
element vertex {10}
property float x
property float y
property float z
property float val
element face 0
property list uchar int vertex_index
end_header";

    /// <summary>
    /// Format timeavgmap data into a LatLonDataArray.
    /// </summary>
    public static LatLonDataArray TemporalMeanPrep(Dictionary<string, dynamic> varJson)
    {
        HashSet<double> lats = new HashSet<double>();
        HashSet<double> longs = new HashSet<double>();

        // get the unique latitudes and longitudes
        foreach (var row in varJson["data"])
        {
            foreach (var data in row)
            {
                // we use a hashset to ensure uniqueness efficiently
                lats.Add((double)data["lat"]);
                longs.Add((double)data["lon"]);
            }
        }

        // sort the latitudes and longitudes
        List<double> latList = new List<double>(lats);
        List<double> longList = new List<double>(longs);
        latList.Sort();
        longList.Sort();

        // create the data array
        LatLonDataArray dataArr = new LatLonDataArray(latList.Count, longList.Count)
        {
            Latitudes = latList,
            Longitudes = longList
        };
        for (int i = 0; i < latList.Count; i++)
        {
            dataArr.LatMapping[latList[i]] = i;
        }
        for (int j = 0; j < longList.Count; j++)
        {
            dataArr.LonMapping[longList[j]] = j;
        }

        // fill in the data array
        foreach (var row in varJson["data"])
        {
            foreach (var data in row)
            {
                dataArr.SetValueAt((double)data["lat"], (double)data["lon"], (double)data["value"]);
            }
        }

        // replace -9999 (missing data) with 0
        for (int k = 0; k < dataArr.Data.GetLength(0); k++)
        {
            for (int l = 0; l < dataArr.Data.GetLength(1); l++)
            {
                if (dataArr.Data[k, l] == -9999)
                {
                    dataArr.Data[k, l] = 0;
                }
            }
        }

        return dataArr;
    }

    public IEnumerator TemporalMean(string baseUrl, string dataset, Dictionary<string, double> bb, DateTime startTime, DateTime endTime, Action<LatLonDataArray> callback)
    {
        string url = $"{baseUrl}/timeAvgMapSpark?ds={dataset}&"+
                     $"b={bb["min_lon"]},{bb["min_lat"]},{bb["max_lon"]},{bb["max_lat"]}&"+
                     $"startTime={startTime.ToString(dtFormat)}&endTime={endTime.ToString(dtFormat)}";
        Debug.Log("Performing request to " + url);
        DateTime requestStart = DateTime.Now;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            Debug.Log("Request took " + (DateTime.Now - requestStart).TotalSeconds + " seconds");

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("UnityWebRequest failed:");
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("UnityWebRequest succeeded:");
                string responseBody = www.downloadHandler.text;

                // Deserialize the JSON response into objects
                Dictionary<string, dynamic> varJson = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseBody);

                // Print the parsed objects
                // foreach (var row in varJson["data"])
                // {
                //     foreach (var data in row)
                //     {
                //         string lat = $"Lat: {data["lat"]}\n";
                //         string lon = $"Lon: {data["lon"]}\n";
                //         string value = $"Value: {data["value"]}\n";
                //         Debug.Log(lat + lon + value);
                //     }
                // }

                LatLonDataArray dataArr = TemporalMeanPrep(varJson);
                callback(dataArr);
            }
        }
    }

    public void ProcessSdap(string sdapUrl, string dataset, Dictionary<string, double> bb, DateTime startTime, DateTime endTime, int interp, Dictionary<string,string> config)
    {
        // Query SDAP
        StartCoroutine(TemporalMean(
            sdapUrl,
            dataset,
            bb,
            startTime,
            endTime,
            (dataArr) =>
            {
                // Convert to PLY
                Debug.Log("Got data array");
                List<string> rows = new List<string>();
                float min_val = float.MaxValue;
                float max_val = float.MinValue;
                double? previous_val = null;
                double? previous_lat = null;
                double? previous_lon = null;

                for (int lat_i = 0; lat_i < dataArr.LatMapping.Count ; lat_i++)
                {
                    double lat = dataArr.Latitudes[lat_i];
                    for (int lon_i = 0; lon_i < dataArr.Longitudes.Count; lon_i++)
                    {
                        double lon = dataArr.Longitudes[lon_i];
                        double data_val = dataArr.GetValueAt(lat, lon);
                        min_val = Math.Min((float)min_val, (float)data_val);
                        max_val = Math.Max((float)max_val, (float)data_val);

                        if (previous_val.HasValue)
                        {
                            // Interpolation logic
                            double i_val = (data_val - previous_val.Value) / interp;
                            double i_lat = (lat - previous_lat.Value) / interp;
                            double i_lon = (lon - previous_lon.Value) / interp;

                            for (int i = 1; i < interp; i++)
                            {
                                double new_val = previous_val.Value + (i_val * i);
                                double new_lat = previous_lat.Value + (i_lat * i);
                                double new_lon = previous_lon.Value + (i_lon * i);
                                string row = $"\n{new_lon} {new_lat} {new_val} {new_val}";
                                rows.Add(row);
                            }
                        }

                        string currentRow = $"\n{lon} {lat} {data_val} {data_val}";
                        rows.Add(currentRow);
                        previous_val = data_val;
                        previous_lon = lon;
                    }
                    previous_lat = lat;
                }

                string outputFilePath = Path.Combine(Application.persistentDataPath, config["identifier"] + ".ply");
                int count = rows.Count;

                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    writer.Write(string.Format(header, config["identifier"], config["description"], config["instrument"], config["label_time"], config["variables"], max_val, min_val, config["variables"], config["units"], count));
                    foreach (var row in rows)
                    {
                        writer.Write(row);
                    }
                }

                Debug.Log("*** Successfully created " + outputFilePath);
            }
        ));
    }

    public static void Main(string[] args)
    {
        // have to parse config here. notes:
        // - interp should be 0 if missing
    }
}