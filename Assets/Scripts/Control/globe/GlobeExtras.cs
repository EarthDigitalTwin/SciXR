using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using Text = TMPro.TMP_Text;

public class GlobeExtras : MonoBehaviour {
    public enum CityDisplayMode {
        Gradient,
        LSL,
        IceThickness
    }

    [Header("Global Properties")]
    public float globeRadius = 0.2515f;
    public float lineWidth = 0.001f;

    [Header("File Paths")]
    public string coastlinesPath = "Data/globe/coastlines.js";
    public string citiesPath = "Data/globe/test.sql";
    public string citiesLocationPath = "Data/globe/cities.js";
    public string citiesDataPath = "Assets/StreamingAssets/CityData";

    [Header("Lat/Lon Grid Properties")]
    public float numLatLines = 9;
    public float numLonLines = 7;
    public float lineStep = 10;
    public float labelHeight = 0.001f;

    private DataObject data;

    public string currentCity;
    public CityDisplayMode currentDisplayMode = CityDisplayMode.LSL;
    public SortedDictionary<string, SerialCity> cities;

    public GameObject cityBubblePrefab;
    public Button citiesButton;
    public Button variablesButton;

    public void Init () {
        data = GetComponent<DataObject>();
        data.canSwapExtrude = false;

        // Init components and disabling irrelevant stuff

        GetComponentInChildren<BoxClip>().gameObject.SetActive(false);
        //GetComponentInChildren<OverlayUI>().gameObject.SetActive(false);
        if(GetComponentInChildren<BoxClipControls>() != null)
            GetComponentInChildren<BoxClipControls>().gameObject.SetActive(false);
        data.globeDragSync.SetActive(true);
        data.dataMeshParent.GetComponent<TransformAttach>().attachedTransform = data.globeDragSync.transform;
        data.GetComponentInChildren<MeshVRControls>().meshDragSync = data.globeDragSync;
        data.material.SetFloat("_ClearHalf", 1);

        // Add components
        DrawCoastLines();
        DrawLatLonGrid();
        if(data.name == "Slr")
            LoadCities();
    }

    public void DrawCoastLines() {
        List<Vector3[]> lines = CoastlinesReader.ReadCoastLinesFromPath(coastlinesPath, globeRadius);

        GameObject sphereLinesContainer = new GameObject("CoastLinesSphere");
        sphereLinesContainer.transform.SetParent(data.dataMeshParent.transform, false);

        for (int count = 0; count < lines.Count; count++) {
            GameObject newCoast = new GameObject("Coast" + count.ToString());
            newCoast.transform.SetParent(sphereLinesContainer.transform, false);
            LineRenderer line = newCoast.AddComponent<LineRenderer>();
            line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            line.sharedMaterial.color = Color.black;
            line.sharedMaterial.renderQueue = 2450;
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = lines[count].Length;
            line.widthMultiplier = lineWidth;
            line.SetPositions(lines[count]);
            //Vector3[] newPositions = new Vector3[lines[count].Length] ;
        }
    }

    public void DrawLatLonGrid() {
        GameObject container = new GameObject("LatLonGridContainer");
        container.transform.SetParent(data.dataMeshParent.transform, false);
        container.transform.localEulerAngles = new Vector3(0, 90, 0);
        for (float lon = 0; lon < 360; lon += 360 / numLatLines) {
            List<Vector3> positions = new List<Vector3>();
            GameObject newLatObj = new GameObject("Longitude " + lon);
            newLatObj.transform.SetParent(container.transform, false);
            LineRenderer line = newLatObj.AddComponent<LineRenderer>();
            line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            line.sharedMaterial.color = Color.black;
            line.sharedMaterial.renderQueue = 2450;
            line.useWorldSpace = false;
            line.loop = false;
            line.widthMultiplier = lineWidth;

            for (float lat = -90; lat <= 90; lat += lineStep) {
                Vector3 newPosition = Quaternion.AngleAxis(lon, -Vector3.up) * Quaternion.AngleAxis(lat, -Vector3.right) * new Vector3(0, 0, 1) * globeRadius;
                positions.Add(newPosition);
            }
            line.positionCount = positions.Count;
            line.SetPositions(positions.ToArray());
        }

        for (float lat = -90; lat <= 90; lat += 180 / numLonLines) {
            List<Vector3> positions = new List<Vector3>();
            GameObject newLatObj = new GameObject("Latitude " + lat);
            newLatObj.transform.SetParent(container.transform, false);
            if(lat != -90 && lat != 90) {
                LineRenderer line = newLatObj.AddComponent<LineRenderer>();
                line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                line.sharedMaterial.color = Color.black;
                line.sharedMaterial.renderQueue = 2450;
                line.useWorldSpace = false;
                line.loop = true;
                line.widthMultiplier = lineWidth;

                for (float lon = 0; lon < 360; lon += lineStep) {
                    Vector3 newPosition = LatLonToXYZ(lat, lon, globeRadius);
                    positions.Add(newPosition);
                    if(lon == 0 || lon == 180) {
                        string name = (lon == 0) ? "LatitudeLabel-Front" : "LatitudeLabel-Back";
                        GameObject newLabel = new GameObject(name);
                        newLabel.transform.SetParent(newLatObj.transform, false);
                        newLabel.transform.localPosition = LatLonToXYZ(lat, lon, globeRadius + labelHeight);

                        //GameObject test = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        //test.transform.SetParent(newLabel.transform, false);
                        //test.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                        //test.transform.rotation = Quaternion.LookRotation(-test.transform.parent.localPosition);
                    }
                }
                line.positionCount = positions.Count;
                line.SetPositions(positions.ToArray());
            }
            else {
                string name = (lat == -90) ? "LatitudeLabel-Bottom" : "LatitudeLabel-Top";
                GameObject newLabel = new GameObject(name);
                newLabel.transform.SetParent(newLatObj.transform, false);
                newLabel.transform.localPosition = LatLonToXYZ(lat, 0, globeRadius + labelHeight);

                //GameObject test = GameObject.CreatePrimitive(PrimitiveType.Quad);
                //test.transform.SetParent(newLabel.transform, false);
                //test.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                //test.transform.rotation = Quaternion.LookRotation(-test.transform.parent.localPosition);
            }
        }
    }

    public void LoadCities() {
        variablesButton.gameObject.SetActive(false);
        citiesButton.gameObject.SetActive(true);

        //cities = CitiesReader.ReadCitiesFromPath(data.data, citiesPath, citiesDataPath, globeRadius);
        //CitiesReader.SaveCities(cities, citiesDataPath);

        cities = CitiesReader.ReadEmptyCitiesFromJS(citiesLocationPath);
        GenerateCities(cities);

        currentCity = "NewOrleans";
        ReloadCity();
    }

    public void GenerateCities(SortedDictionary<string, SerialCity> cities) {
        GameObject container = new GameObject("CityMarkerContainer");
        container.transform.SetParent(data.dataMeshParent.transform, false);
        
        foreach (SerialCity city in cities.Values) {
            Vector3 location = new Vector3(city.x, city.y, city.z).normalized / 4 * 1.0001f;
            //Vector3 newScale = new Vector3(sphereScale, sphereScale, sphereScale);

            GameObject newMarker = Instantiate(cityBubblePrefab);
            newMarker.name = city.name + "Marker";
            CityFlag cityFlag = newMarker.GetComponent<CityFlag>();
            cityFlag.label.text = city.name;
            newMarker.transform.SetParent(container.transform, false);
            newMarker.transform.localPosition = location;
            newMarker.transform.localRotation = Quaternion.LookRotation(location);
        }
    }


    // Callbacks
    public void ReloadCity() {
        GetComponentInChildren<MeshVRControls>().RefreshUI();
        if(cities.Keys.Contains(currentCity)) {
            CityFlag[] allFlags = FindObjectsOfType<CityFlag>();
            foreach(CityFlag flag in allFlags) {
                if(flag.name == currentCity + "Marker")
                    flag.EnableFlag();
                else
                    flag.DisableFlag();
            }

            if(cities[currentCity].normalizedLSL == null) {
                Task.Run(() => { AsyncLoadFile(currentCity); });
            }
            else {
                Vector2[] newUVs = null;
                switch (currentDisplayMode) {
                    case CityDisplayMode.Gradient:
                        newUVs = NormalizedValueToVector2(cities[currentCity].normalizedGradient);
                        break;
                    case CityDisplayMode.IceThickness:
                        newUVs = NormalizedValueToVector2(cities[currentCity].normalizedThickness);
                        break;
                    case CityDisplayMode.LSL:
                        newUVs = NormalizedValueToVector2(cities[currentCity].normalizedLSL);
                        break;
                }
                data.ManualSwapUV(newUVs.ToArray());
            }
        }
        else {
            Debug.LogError("City not found: " + currentCity);
        }
    }

    private void AsyncLoadFile(string city) {
        cities[city] = CitiesReader.ReadCityFromBinary(citiesDataPath + "/" + city + ".bin");
        Vector2[] newUVs = null;
        switch (currentDisplayMode) {
            case CityDisplayMode.Gradient:
                newUVs = NormalizedValueToVector2(cities[currentCity].normalizedGradient);
                break;
            case CityDisplayMode.IceThickness:
                newUVs = NormalizedValueToVector2(cities[currentCity].normalizedThickness);
                break;
            case CityDisplayMode.LSL:
                newUVs = NormalizedValueToVector2(cities[currentCity].normalizedLSL);
                break;
        }
        ThreadManager.instance.callbacks.Add(() => data.ManualSwapUV(newUVs.ToArray()));
    }

    // Helpers

    public Vector2[] NormalizedValueToVector2(float[] values) {
        return values.Select(f => new Vector2(f, 1)).ToArray();
    }

    public Vector3 LatLonToXYZ(float lat, float lon, float radius) {
        return Quaternion.AngleAxis(lon, -Vector3.up) * Quaternion.AngleAxis(lat, -Vector3.right) * new Vector3(0, 0, 1) * (radius);
    }

    public Vector3 XYZToLatLon(Vector3 position) {
        float lat = (float)Mathf.Acos(position.y / position.magnitude); //theta
        float lon = (float)Mathf.Atan2(position.x, position.z); //phi
        lat *= Mathf.Rad2Deg;
        lon *= Mathf.Rad2Deg;
        return new Vector3(lat, lon, 0);
    }
}
