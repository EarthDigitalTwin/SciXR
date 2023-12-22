using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class CoastlinesReader {

    public static List<Vector3[]> ReadCoastLinesFromPath(string path) {
        return ReadCoastLinesFromPath(path, 1);
	}

    public static List<Vector3[]> ReadCoastLinesFromPath(string path, float radius) {
        List<Vector3[]> coastlines = new List<Vector3[]>();

        string[] xcoast = null, ycoast = null, zcoast = null;
		string[] jsOutput = File.ReadAllLines(path);

        foreach (string currentLine in jsOutput) {
			int equalSignIndex = currentLine.IndexOf('=');
            string label = currentLine.Substring(0, equalSignIndex);
            string extractedJSONString = currentLine.Substring(equalSignIndex + 1).TrimEnd(';').TrimStart('[').TrimEnd(']');
            if(label.Contains("xcoast")) {
                xcoast = extractedJSONString.Split(',');
            }
			if (label.Contains("ycoast")) {
				ycoast = extractedJSONString.Split(',');
			}
			if (label.Contains("zcoast")) {
				zcoast = extractedJSONString.Split(',');
			}
        }

        List<Vector3> line = new List<Vector3>();
        float currentRadius = new Vector3(float.Parse(xcoast[0]), float.Parse(ycoast[0]), float.Parse(zcoast[0])).magnitude;
        float scale = radius / currentRadius;

        //Parse lines and separate at 
        for (int count = 0; count < xcoast.Length; count++) {
            if(xcoast[count] == "NaN") {
                if(line.Count > 20) {
					coastlines.Add(line.ToArray());
                }
                line = new List<Vector3>();
            }
            else {
                Vector3 currentPoint = new Vector3(float.Parse(xcoast[count]) * scale ,  float.Parse(zcoast[count]) * scale, float.Parse(ycoast[count]) * scale);
                line.Add(currentPoint);
            }
            if (xcoast[count + 1] != "NaN")
                count++;
        }
        return coastlines;
    }

}
