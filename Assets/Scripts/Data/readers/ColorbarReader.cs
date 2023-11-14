using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

public class ColorbarReader {

	private Dictionary<string, Colorbar> colorbars = new Dictionary<string, Colorbar>{};


	public List<Colorbar> ReadColorbarsFromRaw(string rawData) 
	{
		Stopwatch watch = new Stopwatch();
		watch.Start();

		string[] jsOutput = rawData.Split('\n');

		// Get colorbar
		string colorbarName = null;
		foreach (string currentLine in jsOutput) {
			
			int equalSignIndex = currentLine.IndexOf('=');
			if (equalSignIndex != -1) {
				string parameter = currentLine.Substring (0, equalSignIndex);

				if (parameter.IndexOf ("[") != -1 && parameter.IndexOf ("]") != -1) {
					colorbarName = parameter.Substring (parameter.IndexOf ("[") + 1, parameter.IndexOf ("]") - parameter.IndexOf ("[") - 1).Replace ("\"", "");
					Colorbar colorbar = new Colorbar (colorbarName);
					List<string> colorVals = currentLine.Substring (equalSignIndex + 3, currentLine.LastIndexOf ("]") - equalSignIndex - 3).Split (',').ToList<string> ();
					Color color = new Color (float.Parse (colorVals [0]), float.Parse (colorVals [1]), float.Parse (colorVals [2]));
					colorbar.rgbColors.Add (color);
					colorbars.Add (colorbar.name, colorbar);
				}
			} else {
				if (currentLine.IndexOf ("[") != -1 && currentLine.IndexOf ("]") != -1) {
					string colorString = currentLine.Substring (currentLine.IndexOf ("[")+1, currentLine.LastIndexOf ("]")-currentLine.IndexOf ("[")-2);
					List<string> colorVals = colorString.Split (',').ToList<string> ();
					Color color = new Color (float.Parse (colorVals [0]), float.Parse (colorVals [1]), float.Parse (colorVals [2]));
					if (colorbarName != null) {
						colorbars [colorbarName].rgbColors.Add (color);
					}
				}
			}
		}

		List<Colorbar> listColorbars = new List<Colorbar>();
		foreach(KeyValuePair<string, Colorbar> entry in colorbars) {
            Colorbar current = entry.Value;
            current.texture = GenerateColorTexture(current);
			listColorbars.Add (current);
		}

		watch.Stop();
		Debug.Log(watch.Elapsed);
		return listColorbars;
	}

	public List<Colorbar> ReadColorbarsFromPath(string path)
	{
		return ReadColorbarsFromRaw(File.ReadAllText(path));
	}

    public Texture2D GenerateColorTexture(Colorbar colormap) {
        Texture2D texture;
        texture = new Texture2D(colormap.rgbColors.Count, 1, TextureFormat.ARGB32, false);
        // loop through all colors in colorbar
        for (int i = 0; i < colormap.rgbColors.Count; i++) {
            // set the pixel values
            texture.SetPixel(i, 0, colormap.rgbColors[i]);
        }
        // apply all SetPixel calls
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return texture;
    }
}
