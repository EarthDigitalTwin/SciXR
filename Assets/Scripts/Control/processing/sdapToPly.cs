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

public class SdapConfiguration
{
    public string SdapUrl { get; set; }
    public List<string> Variables { get; set; }
    public string Units { get; set; }
    public string Dataset { get; set; }
    public string Identifier { get; set; }
    public string Description { get; set; }
    public string Instrument { get; set; }
    public BoundingBox Bbox { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime LabelTime { get; set; }
    public int Interpolation { get; set; }
}

public class BoundingBox
{
    public double MinLon { get; set; }
    public double MaxLon { get; set; }
    public double MinLat { get; set; }
    public double MaxLat { get; set; }
}

public class SdapResponse
{
    public Meta meta { get; set; }

    public class Meta
    {
        public string shortName { get; set; }
        public Bounds bounds { get; set; }
        public Time time { get; set; }

        public class Bounds
        {
            public double east { get; set; }
            public double west { get; set; }
            public double north { get; set; }
            public double south { get; set; }
        }

        public class Time
        {
            public long start { get; set; }
            public long stop { get; set; }
            public string iso_start { get; set; }
            public string iso_stop { get; set; }
        }
    }

    public List<List<SdapData>> data { get; set; }

    public class SdapData
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public double mean { get; set; }
        public int cnt { get; set; }
    }
}


public class SdapToPly : MonoBehaviour
{
    static string dtFormat = "yyyy-MM-ddTHH:mm:ssZ";
    private static readonly string header = @"ply
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
    public static LatLonDataArray TemporalMeanPrep(SdapResponse response)
    {
        HashSet<double> lats = new HashSet<double>();
        HashSet<double> longs = new HashSet<double>();

        List<List<SdapResponse.SdapData>> data = response.data;

        // get the unique latitudes and longitudes
        foreach (List<SdapResponse.SdapData> row in data)
        {
            foreach (SdapResponse.SdapData datum in row)
            {
                // we use a hashset to ensure uniqueness efficiently
                lats.Add(datum.lat);
                longs.Add(datum.lon);
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
        foreach (List<SdapResponse.SdapData> row in data)
        {
            foreach (SdapResponse.SdapData datum in row)
            {
                // replace -9999 (missing data) with 0
                double val = datum.mean == -9999 ? 0 : datum.mean;
                dataArr.SetValueAt(datum.lat, datum.lon, val);
            }
        }

        return dataArr;
    }

    public IEnumerator TemporalMean(string baseUrl, string dataset, BoundingBox bb, DateTime startTime, DateTime endTime, Action<LatLonDataArray> callback)
    {
        string url = $"{baseUrl}/timeAvgMapSpark?ds={dataset}&"+
                     $"b={bb.MinLon},{bb.MinLat},{bb.MaxLon},{bb.MaxLat}&"+
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
                Debug.Log("UnityWebRequest succeeded. Parsing JSON...");
                string responseBody = www.downloadHandler.text;

                // Deserialize the JSON response into objects
                SdapResponse response = JsonConvert.DeserializeObject<SdapResponse>(responseBody);

                Debug.Log("Done parsing JSON.");

                LatLonDataArray dataArr = TemporalMeanPrep(response);
                Debug.Log("Successfully created LatLonDataArray. Calling cb...");
                callback(dataArr);
            }
        }
    }

    public void ProcessSdap(SdapConfiguration config, Action callback)
    {
        Debug.Log("In ProcessSdap, config is " + config);

        // Query SDAP
        StartCoroutine(TemporalMean(
            config.SdapUrl,
            config.Dataset,
            config.Bbox,
            config.StartTime,
            config.EndTime,
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
                int interp = config.Interpolation;

                for (int lat_i = 0; lat_i < dataArr.LatMapping.Count ; lat_i++)
                {
                    double lat = dataArr.Latitudes[lat_i];
                    previous_lat = lat;
                    for (int lon_i = 0; lon_i < dataArr.Longitudes.Count; lon_i++)
                    {
                        double lon = dataArr.Longitudes[lon_i];
                        double data_val = dataArr.GetValueAt(lat, lon);
                        min_val = Math.Min((float)min_val, (float)data_val);
                        max_val = Math.Max((float)max_val, (float)data_val);

                        if (previous_val.HasValue && previous_lat.HasValue && previous_lon.HasValue && interp > 1)
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
                    if (lat_i % 10 == 0)
                    {
                        Debug.Log("Processed latitude no " + lat_i + " of " + dataArr.LatMapping.Count + "; rows now " + rows.Count);
                    }
                }

                Debug.Log("Done converting to PLY. Writing to file...");

                string outputFilePath = Path.Combine(Application.persistentDataPath, config.Identifier + ".ply");
                int count = rows.Count;

                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    writer.Write(string.Format(header, 
                        config.Identifier, 
                        config.Description, 
                        config.Instrument,
                        config.LabelTime,
                        config.Variables[0],
                        max_val, 
                        min_val, 
                        config.Variables,  // z_label (7)
                        config.Variables, config.Units,  // val_label (8, 9)
                        count
                    ));
                    foreach (var row in rows)
                    {
                        writer.Write(row);
                    }
                }

                Debug.Log("*** Successfully created " + outputFilePath + ", writing " + count + " rows. ***");

                callback();
            }
        ));
    }

    public static void Main(string[] args)
    {
        // Testing
        // SdapConfiguration config = new SdapConfiguration
        // {
        //     SdapUrl = "https://ideas-digitaltwin.jpl.nasa.gov/nexus",
        //     Dataset = "TROPOMI_CO_global",
        //     Variables = new List<string> { "CO" },
        //     Units = "",
        //     Identifier = "TROPOMI_CO_NE_USA",
        //     Description = "TROPOMI Carbon Monoxide Northeast USA",
        //     Instrument = "TROPOMI",
        //     Bbox = new BoundingBox
        //     {
        //         MinLon = -81,
        //         MaxLon = -65,
        //         MinLat = -34,
        //         MaxLat = 50
        //     },
        //     StartTime = DateTime.ParseExact("2023-06-07", "yyyy-MM-dd", CultureInfo.InvariantCulture),
        //     EndTime = DateTime.ParseExact("2023-06-08", "yyyy-MM-dd", CultureInfo.InvariantCulture),
        //     LabelTime = DateTime.ParseExact("2023-06-07", "yyyy-MM-dd", CultureInfo.InvariantCulture),
        //     Interpolation = 5
        // };

        // SdapToPly sdapToPly = new SdapToPly();
        // sdapToPly.ProcessSdap(config);
    }
}