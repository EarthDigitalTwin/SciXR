using System.Collections;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;
using Proj4Net;
using UnityEngine.Networking;

public class OverlayLoader
{

    public class BBox
    {
        public double minX;
        public double minY;
        public double maxX;
        public double maxY;

        public BBox (double minX, double minY, double maxX, double maxY) {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }
    }

    public static BBox GetObjectBounds (float[] xVals, float[] yVals, string projection) {
        float xMin = xVals.Min();
        float xMax = xVals.Max();
        float yMin = yVals.Min();
        float yMax = yVals.Max();

        Debug.Log("Projection: EPSG:" + projection);

        CoordinateReferenceSystemFactory CRSFactory = new CoordinateReferenceSystemFactory();
        // Initiate target values
        CoordinateReferenceSystem targetEPSG = CRSFactory.CreateFromParameters("EPSG:4326", "+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs");
		GeoAPI.Geometries.Coordinate tgtUL = new GeoAPI.Geometries.Coordinate(xMin, yMax, 0);
		GeoAPI.Geometries.Coordinate tgtUR = new GeoAPI.Geometries.Coordinate(xMax, yMax, 0);
		GeoAPI.Geometries.Coordinate tgtLL = new GeoAPI.Geometries.Coordinate(xMin, yMin, 0);
		GeoAPI.Geometries.Coordinate tgtLR = new GeoAPI.Geometries.Coordinate(xMax, yMin, 0);

		if (projection != "4326") {
			CoordinateReferenceSystem sourceEPSG = null;
			if (projection == "32406") {
				sourceEPSG = CRSFactory.CreateFromParameters ("EPSG:" + projection, "+proj=utm +zone=6 +ellps=WGS72 +towgs84=0,0,1.9,0,0,0.814,-0.38 +units=m +no_defs");
			} else if (projection == "3413") {
				sourceEPSG = CRSFactory.CreateFromParameters ("EPSG:" + projection, "+proj=stere +lat_0=90 +lat_ts=70 +lon_0=-45 +k=1 +x_0=0 +y_0=0 +datum=WGS84 +units=m +no_defs");
			} else {
				Debug.Log ("Projection: EPSG:" + projection + " is not supported.");
				return null; 
			}
			//CoordinateReferenceSystem sourceEPSG = CRSFactory.CreateFromName("EPSG:" + projection);
			BasicCoordinateTransform coordinateTransform = new BasicCoordinateTransform (sourceEPSG, targetEPSG);

			GeoAPI.Geometries.Coordinate srcUL = new GeoAPI.Geometries.Coordinate (xMin, yMax, 0);
			GeoAPI.Geometries.Coordinate srcUR = new GeoAPI.Geometries.Coordinate (xMax, yMax, 0);
			GeoAPI.Geometries.Coordinate srcLL = new GeoAPI.Geometries.Coordinate (xMin, yMin, 0);
			GeoAPI.Geometries.Coordinate srcLR = new GeoAPI.Geometries.Coordinate (xMax, yMin, 0);

			coordinateTransform.Transform (srcUL, tgtUL);
			coordinateTransform.Transform (srcUR, tgtUR);
			coordinateTransform.Transform (srcLL, tgtLL);
			coordinateTransform.Transform (srcLR, tgtLR);
		}

        return new OverlayLoader.BBox(tgtUL.X, tgtLL.Y, tgtLR.X, tgtUR.Y);
    } 

	public static IEnumerator LoadImageryFromBounds (Material material, string layerName, string projection, BBox bbox, string time) {
        double[] bBox = { bbox.minX, bbox.minY, bbox.maxX, bbox.maxY };
        int scale = 1;
        if ((bbox.maxX / bbox.maxY) >= 2) {
            scale = 2;
        }
            Debug.Log(bbox.minX.ToString() + "," + bbox.minY + "," + bbox.maxX + "," + bbox.maxY);
        WMSRequest request = new WMSRequest(layerName, 2048, 2048/scale, bBox, time, projection);
        Debug.Log(request.Url);

        Texture2D texture;
        texture = new Texture2D(2048, 2048/scale);
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(request.Url))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            }
            material.SetTexture("_MainTex", texture);
        }
    }
}