using UnityEngine;
using UnityEngine.UI;
//using OSGeo.GDAL;
//using OSGeo.OGR;
//using OSGeo.OSR;
using System;
using System.Linq;
using System.Runtime.InteropServices;

public class EarthLocator : MonoBehaviour
{
	//public GameObject globe;
 //   public string basemapPath = "/Planet Earth/Planets/Earth/Textures/earth_color_8K_LC.png";
 //   public RawImage map;
 //   public DataObject currentMesh;
 //   public Camera cam;

 //   private float[] longvals;
 //   private float[] latvals;
 //   private float[] xvals;
 //   private float[] yvals;
 //   private float[] zvals;

 //   private float xmax;
 //   private float xmin;
 //   private float ymax;
 //   private float ymin;

 //   private int iWidth;
 //   private int iHeight;

 //   private enum ImageFilterMode : int
 //   {
 //       Nearest = 0,
 //       Bilinear = 1,
 //       Average = 2
 //   }

 //   // Use this for initialization
 //   void Start()
 //   {

	//	// Register GDAL and OGR
	//	Gdal.AllRegister();
 //       Ogr.RegisterAll();
	//	OSGeo.OSR.SpatialReference sr = new OSGeo.OSR.SpatialReference("");

 //       // Get basemap image 
	//	basemapPath = Application.dataPath + basemapPath;

	//	// Set image parameters
 //       iWidth = 1024;
	//	iHeight = 512;

 //       try
 //       {
 //           // Check for lat/long
 //           longvals = currentMesh.data.vars["mesh.long"];
 //           latvals = currentMesh.data.vars["mesh.lat"];
 //           // sr.ImportFromEPSG(4326); // not working on Windows
 //           sr.ImportFromProj4("+proj=longlat +datum=WGS84 +no_defs");
 //       }
 //       catch {
 //           // Assume 3413 otherwise
 //           longvals = new float[] { };
	//		latvals = new float[] { };
	//		//sr.ImportFromEPSG(3413); // not working on Windows
	//		sr.ImportFromProj4("+proj=stere +lat_0=90 +lat_ts=70 +lon_0=-45 +k=1 +x_0=0 +y_0=0 +datum=WGS84 +units=m +no_defs");
 //       }

	//	int i = 0;
	//	int j = 0;

	//	// Use in memory driver
	//	OSGeo.GDAL.Driver drv = Gdal.GetDriverByName("MEM");

	//	// Load orthographic mesh image
	//	Texture2D meshTexture = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGBA32, false);
	//	RenderTexture.active = cam.activeTexture;
	//	meshTexture.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
	//	meshTexture.Apply();
	//	meshTexture = FlipTexture(meshTexture);

	//	// read the mesh image
	//	int meshWidth = meshTexture.width;
	//	int meshHeight = meshTexture.height;
	//	byte[] textureBytes = meshTexture.GetRawTextureData();
	//	byte[] textbuff = new byte[(meshWidth * meshHeight) * 4];
	//	byte[] r = new byte[meshWidth * meshHeight];
	//	byte[] g = new byte[meshWidth * meshHeight];
	//	byte[] b = new byte[meshWidth * meshHeight];
	//	byte[] a = new byte[meshWidth * meshHeight];

	//	// RGBA texture data is ordered this way: R, G, B, A bytes one after another. Do reverse.
	//	j = 0;
	//	for (i = 0; i < textureBytes.Length; i++)
	//	{
	//		if (i % 4 == 0 && j < r.Length)
	//		{
	//			r[j] = textureBytes[i];
	//		}
	//		if (i % 4 == 1 && j < g.Length)
	//		{
	//			g[j] = textureBytes[i];
	//		}
	//		if (i % 4 == 2 && j < b.Length)
	//		{
	//			b[j] = textureBytes[i];
	//		}
	//		if (i % 4 == 3 && j < a.Length)
	//		{
	//			a[j] = textureBytes[i];
	//			j++;
	//		}
	//	}
	//	r.CopyTo(textbuff, 0);
	//	g.CopyTo(textbuff, r.Length);
	//	b.CopyTo(textbuff, r.Length + g.Length);
	//	a.CopyTo(textbuff, r.Length + g.Length + b.Length);

 //       // Assume EPSG:3413
	//	Dataset dsI = drv.Create("", meshWidth, meshHeight, 4, DataType.GDT_Byte, null);
	//	//sr.ImportFromEPSG(3413);
 //       sr.ImportFromProj4("+proj=stere +lat_0=90 +lat_ts=70 +lon_0=-45 +k=1 +x_0=0 +y_0=0 +datum=WGS84 +units=m +no_defs");
	//	string epsg = "";
	//	sr.ExportToWkt(out epsg);
	//	dsI.SetProjection(epsg);

 //       // Get x/y/z values
	//	xvals = currentMesh.data.vars["mesh.x"];
	//	yvals = currentMesh.data.vars["mesh.y"];
 //       zvals = currentMesh.data.vars["geometry.surface"];
 //       // Get min and max for bounds
	//	xmin = xvals.Min();
	//	xmax = xvals.Max();
	//	ymin = yvals.Min();
	//	ymax = yvals.Max();

 //       // Calculate extents
	//	float ydelta = ymax - ymin;
 //       float xdelta = xmax - xmin;
 //       if (ydelta >= xdelta)
 //       {
 //           float centerx = (xmax + xmin) / 2;
 //           xmin = centerx - (ydelta / 2);
 //           xmax = centerx + (ydelta / 2);
 //       } else {
	//		float centery = (ymax + ymin) / 2;
	//		ymin = centery - (xdelta / 2);
	//		ymax = centery + (xdelta / 2);            
 //       }

 //       // Set image transformation
	//	double[] geotransformImage = new double[6] { xmin, (xmax - xmin) / meshWidth, 0, ymax, 0, (ymax - ymin) / (meshHeight * -1) };
	//	dsI.SetGeoTransform(geotransformImage);

 //       // Generate GDAL raster
	//	int[] iBandMap = { 1, 2, 3, 4 };
	//	dsI.WriteRaster(0, 0, meshWidth, meshHeight, textbuff, meshWidth, meshHeight, 4, iBandMap, 0, 0, 0);

	//	//Reproject to 4326
	//	Dataset dsR = Gdal.AutoCreateWarpedVRT(dsI, epsg, OSGeo.OSR.Osr.SRS_WKT_WGS84, ResampleAlg.GRA_NearestNeighbour, 0);
	//	string[] warpOptionsString = new string[] { "-of", "MEM", "-te", "-180", "-90", "180", "90", "-ts", iWidth.ToString(), iHeight.ToString() };
	//	GDALWarpAppOptions warpOptions = new GDALWarpAppOptions(warpOptionsString);
	//	var ptr = new[] { Dataset.getCPtr(dsR).Handle };
	//	var gcHandle = GCHandle.Alloc(ptr, GCHandleType.Pinned);
	//	var dss = new SWIGTYPE_p_p_GDALDatasetShadow(gcHandle.AddrOfPinnedObject(), false, null);
	//	Dataset dsWarp = Gdal.wrapper_GDALWarpDestName("", 1, dss, warpOptions, null, null);
	//	Texture2D warpedTexture = new Texture2D(iWidth, iHeight, TextureFormat.RGBA32, false);

 //       // Apply warped image to texture
	//	byte[] warpbuff = new byte[(iWidth * iHeight) * 4];
	//	byte[] r3 = new byte[iWidth * iHeight];
	//	byte[] g3 = new byte[iWidth * iHeight];
	//	byte[] b3 = new byte[iWidth * iHeight];
	//	byte[] a3 = new byte[iWidth * iHeight];

	//	Band meshband1 = dsWarp.GetRasterBand(1);
	//	Band meshband2 = dsWarp.GetRasterBand(2);
	//	Band meshband3 = dsWarp.GetRasterBand(3);
	//	Band meshband4 = dsWarp.GetRasterBand(4);
	//	meshband1.ReadRaster(0, 0, iWidth, iHeight, r3, iWidth, iHeight, 0, 0);
	//	meshband2.ReadRaster(0, 0, iWidth, iHeight, g3, iWidth, iHeight, 0, 0);
	//	meshband3.ReadRaster(0, 0, iWidth, iHeight, b3, iWidth, iHeight, 0, 0);
	//	meshband4.ReadRaster(0, 0, iWidth, iHeight, a3, iWidth, iHeight, 0, 0);

	//	j = 0;
	//	for (i = 0; i < (iWidth * iHeight); i++)
	//	{
	//		warpbuff[j] = r3[i];
	//		warpbuff[j + 1] = g3[i];
	//		warpbuff[j + 2] = b3[i];
	//		warpbuff[j + 3] = a3[i];
	//		j += 4;
	//	}

	//	warpedTexture.LoadRawTextureData(warpbuff);
	//	warpedTexture.Apply();


	//	// Generate bounding box texture if we can figure out the projected bounds
	//	Texture2D geoTexture = new Texture2D(iWidth, iHeight, TextureFormat.RGBA32, false);
 //       if (longvals.Length > 0)
 //       {

 //           // Create vector dataset in memory
 //           OSGeo.OGR.Driver ogrDriver = Ogr.GetDriverByName("Memory");
 //           DataSource shape = ogrDriver.CreateDataSource("out", null);

 //           // Create vector layer
 //           // sr.ImportFromEPSG(4326);
 //           sr.ImportFromProj4("+proj=longlat +datum=WGS84 +no_defs");
 //           Layer layer = shape.CreateLayer("ISSM Vertices", sr, wkbGeometryType.wkbPoint, null);
 //           Layer layerBox = shape.CreateLayer("ISSM Bounding Box", sr, wkbGeometryType.wkbLinearRing, null);
 //           FeatureDefn layerDefinition = layer.GetLayerDefn();
 //           FeatureDefn layerDefinition2 = layer.GetLayerDefn();

 //           // Generate points for layer
 //           Geometry points = new Geometry(wkbGeometryType.wkbMultiPoint);
 //           for (int k = 0; k < longvals.Length; k++)
 //           {
 //               Geometry geom = new Geometry(wkbGeometryType.wkbPoint);
 //               geom.AddPoint(longvals[k], latvals[k], zvals[k]);
 //               points.AddGeometry(geom);
 //           }
 //           Feature pointFeature = new Feature(layerDefinition);
 //           pointFeature.SetGeometry(points);
 //           layer.CreateFeature(pointFeature);

 //           // Generate bounding box for layer
 //           Geometry bounds = points.ConvexHull();
 //           Envelope envelope = new Envelope();
 //           bounds.GetEnvelope(envelope);
 //           Geometry outline = new Geometry(wkbGeometryType.wkbLinearRing);
 //           outline.AddPoint_2D(envelope.MinX, envelope.MaxY);
 //           outline.AddPoint_2D(envelope.MaxX, envelope.MaxY);
 //           outline.AddPoint_2D(envelope.MaxX, envelope.MinY);
 //           outline.AddPoint_2D(envelope.MinX, envelope.MinY);
 //           outline.AddPoint_2D(envelope.MinX, envelope.MaxY);
 //           Feature boundary = new Feature(layerDefinition2);
 //           boundary.SetGeometry(outline);
 //           layerBox.CreateFeature(boundary);

 //           // Create raster dataset from vector layer
 //           Dataset dsRaster = drv.Create("", iWidth, iHeight, 4, DataType.GDT_Byte, null);
 //           // Use WGS84 values
 //           dsRaster.SetProjection(OSGeo.OSR.Osr.SRS_WKT_WGS84);
 //           xmin = -180;
 //           xmax = 180;
 //           ymin = -90;
 //           ymax = 90;
 //           double[] geotransform = new double[6] { xmin, (xmax - xmin) / iWidth, 0, ymax, 0, (ymax - ymin) / (iHeight * -1) };
 //           dsRaster.SetGeoTransform(geotransform);
 //           Band rasterBand1 = dsRaster.GetRasterBand(1);
 //           Band rasterBand2 = dsRaster.GetRasterBand(2);
 //           Band rasterBand3 = dsRaster.GetRasterBand(3);
 //           Band rasterBand4 = dsRaster.GetRasterBand(4);
 //           rasterBand1.SetNoDataValue(0f);
 //           rasterBand2.SetNoDataValue(0f);
 //           rasterBand3.SetNoDataValue(0f);
 //           rasterBand4.SetNoDataValue(0f);
 //           int[] bandlist = { 1, 2, 3, 4 };
 //           //double[] burnvalueslist = { 0, 0, 0, 255 };
 //           //string[] options = { "BURN_VALUE_FROM=Z" };
 //           IntPtr intptr = new IntPtr();
 //           //Gdal.RasterizeLayer(dsRaster, 4, bandlist, layer, intptr, intptr, 4, burnvalueslist, options, null, null);

 //           // Rasterize layer bounding box
 //           double[] burnvalueslist2 = { 255, 255, 0, 255 };
 //           Gdal.RasterizeLayer(dsRaster, 4, bandlist, layerBox, intptr, intptr, 4, burnvalueslist2, null, null, null);

 //           // Create Unity Texture2D from GDAL raster
 //           byte[] meshbuff = new byte[(iWidth * iHeight) * 4];
 //           byte[] r1 = new byte[iWidth * iHeight];
 //           byte[] g1 = new byte[iWidth * iHeight];
 //           byte[] b1 = new byte[iWidth * iHeight];
 //           byte[] a1 = new byte[iWidth * iHeight];
 //           rasterBand1.ReadRaster(0, 0, iWidth, iHeight, r1, iWidth, iHeight, 0, 0);
 //           rasterBand2.ReadRaster(0, 0, iWidth, iHeight, g1, iWidth, iHeight, 0, 0);
 //           rasterBand3.ReadRaster(0, 0, iWidth, iHeight, b1, iWidth, iHeight, 0, 0);
 //           rasterBand4.ReadRaster(0, 0, iWidth, iHeight, a1, iWidth, iHeight, 0, 0);
 //           // RGBA texture data is ordered this way: R, G, B, A bytes one after another.
 //           j = 0;
 //           for (i = 0; i < (iWidth * iHeight); i++)
 //           {
 //               meshbuff[j] = r1[i];
 //               meshbuff[j + 1] = g1[i];
 //               meshbuff[j + 2] = b1[i];
 //               meshbuff[j + 3] = a1[i];
 //               j += 4;
 //           }
 //           geoTexture.LoadRawTextureData(meshbuff);
 //           geoTexture.Apply();
 //       }

 //       // Get texture for basemap
	//	iWidth = 8192;
	//	iHeight = 4096;
	//	Texture2D mapTexture = new Texture2D(iWidth, iHeight, TextureFormat.RGB24, false);
	//	Dataset dsSource = Gdal.Open(basemapPath, Access.GA_ReadOnly);
	//	Dataset dsBasemap = drv.CreateCopy("", dsSource, 0, null, null, null);
	//	dsBasemap.SetProjection(OSGeo.OSR.Osr.SRS_WKT_WGS84);
	//	byte[] mapbuff = new byte[(iWidth * iHeight) * 3];
	//	byte[] r2 = new byte[iWidth * iHeight];
	//	byte[] g2 = new byte[iWidth * iHeight];
	//	byte[] b2 = new byte[iWidth * iHeight];
	//	Band band1 = dsBasemap.GetRasterBand(1);
	//	Band band2 = dsBasemap.GetRasterBand(2);
	//	Band band3 = dsBasemap.GetRasterBand(3);
	//	band1.ReadRaster(0, 0, iWidth, iHeight, r2, iWidth, iHeight, 0, 0);
	//	band2.ReadRaster(0, 0, iWidth, iHeight, g2, iWidth, iHeight, 0, 0);
	//	band3.ReadRaster(0, 0, iWidth, iHeight, b2, iWidth, iHeight, 0, 0);
	//	// RGB texture data is ordered this way: R, G, B bytes one after another.
	//	j = 0;
	//	for (i = 0; i < (iWidth * iHeight); i++)
	//	{
	//		mapbuff[j] = r2[i];
	//		mapbuff[j + 1] = g2[i];
	//		mapbuff[j + 2] = b2[i];
	//		j += 3;
	//	}
	//	mapTexture.LoadRawTextureData(mapbuff);
	//	mapTexture.Apply();
 //       Texture2D mapTextureResized = ResizeTexture(mapTexture, ImageFilterMode.Nearest, 0.125f);

	//	// Load image on map and globe
	//	MeshRenderer mesh = globe.GetComponent<MeshRenderer>();
 //       Texture2D mergedTexture = AlphaBlend(mapTextureResized, warpedTexture);
 //       if (longvals.Length > 0) { // nothing to overlay
 //           Texture2D mergedTexture2 = AlphaBlend(mergedTexture, geoTexture);
	//		map.texture = mergedTexture2;
	//		mesh.materials[0].SetTexture("_MainTex", mergedTexture2);
 //       } else {
	//		map.texture = mergedTexture;
	//		mesh.materials[0].SetTexture("_MainTex", mergedTexture);
 //       }
        		
	//}
	
	//// Update is called once per frame
	//void Update () {

	//}

 //   // Flips a texture vertically
	//private Texture2D FlipTexture(Texture2D original)
	//{
	//	Texture2D flipped = new Texture2D(original.width, original.height);

	//	int xN = original.width;
	//	int yN = original.height;

	//	for (int i = 0; i < xN; i++)
	//	{
	//		for (int j = 0; j < yN; j++)
	//		{
	//			flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
	//		}
	//	}
	//	flipped.Apply();
	//	return flipped;
	//}

 //   // Blends two textures together
	//private Texture2D AlphaBlend(Texture2D aBottom, Texture2D aTop) {
 //       if (aBottom.width != aTop.width || aBottom.height != aTop.height) {
 //           throw new System.InvalidOperationException("AlphaBlend only works with two equal sized images");
 //       }
	//	var bData = aBottom.GetPixels();
	//	var tData = aTop.GetPixels();
	//	int count = bData.Length;
	//	var rData = new Color[count];
	//	for (int i = 0; i < count; i++) {
	//		Color B = bData[i];
	//		Color T = tData[i];
	//		float srcF = T.a;
	//		float destF = 1f - T.a;
	//		float alpha = srcF + destF * B.a;
	//		Color R = (T * srcF + B * B.a * destF) / alpha;
	//		R.a = alpha;
	//		rData[i] = R;
	//	}
	//	var res = new Texture2D(aTop.width, aTop.height);
	//	res.SetPixels(rData);
	//	res.Apply();
	//	return res;
	//}

 //   // Resizes a texture
	//private static Texture2D ResizeTexture(Texture2D pSource, ImageFilterMode pFilterMode, float pScale) {

	//	//*** Variables
	//	int i;

	//	//*** Get All the source pixels
	//	Color[] aSourceColor = pSource.GetPixels(0);
	//	Vector2 vSourceSize = new Vector2(pSource.width, pSource.height);

	//	//*** Calculate New Size
	//	float xWidth = Mathf.RoundToInt((float)pSource.width * pScale);
	//	float xHeight = Mathf.RoundToInt((float)pSource.height * pScale);

	//	//*** Make New
	//	Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);

	//	//*** Make destination array
	//	int xLength = (int)xWidth * (int)xHeight;
	//	Color[] aColor = new Color[xLength];

	//	Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

	//	//*** Loop through destination pixels and process
	//	Vector2 vCenter = new Vector2();
	//	for (i = 0; i < xLength; i++)
	//	{

	//		//*** Figure out x&y
	//		float xX = (float)i % xWidth;
	//		float xY = Mathf.Floor((float)i / xWidth);

	//		//*** Calculate Center
	//		vCenter.x = (xX / xWidth) * vSourceSize.x;
	//		vCenter.y = (xY / xHeight) * vSourceSize.y;

	//		//*** Do Based on mode
	//		//*** Nearest neighbour (testing)
	//		if (pFilterMode == ImageFilterMode.Nearest)
	//		{

	//			//*** Nearest neighbour (testing)
	//			vCenter.x = Mathf.Round(vCenter.x);
	//			vCenter.y = Mathf.Round(vCenter.y);

	//			//*** Calculate source index
	//			int xSourceIndex = (int)((vCenter.y * vSourceSize.x) + vCenter.x);

	//			//*** Copy Pixel
	//			aColor[i] = aSourceColor[xSourceIndex];
	//		}

	//		//*** Bilinear
	//		else if (pFilterMode == ImageFilterMode.Bilinear)
	//		{

	//			//*** Get Ratios
	//			float xRatioX = vCenter.x - Mathf.Floor(vCenter.x);
	//			float xRatioY = vCenter.y - Mathf.Floor(vCenter.y);

	//			//*** Get Pixel index's
	//			int xIndexTL = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
	//			int xIndexTR = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));
	//			int xIndexBL = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
	//			int xIndexBR = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));

	//			//*** Calculate Color
	//			aColor[i] = Color.Lerp(
	//				Color.Lerp(aSourceColor[xIndexTL], aSourceColor[xIndexTR], xRatioX),
	//				Color.Lerp(aSourceColor[xIndexBL], aSourceColor[xIndexBR], xRatioX),
	//				xRatioY
	//			);
	//		}

	//		//*** Average
	//		else if (pFilterMode == ImageFilterMode.Average)
	//		{

	//			//*** Calculate grid around point
	//			int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
	//			int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
	//			int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
	//			int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);

	//			//*** Loop and accumulate
	//			//Vector4 oColorTotal = new Vector4();
	//			Color oColorTemp = new Color();
	//			float xGridCount = 0;
	//			for (int iy = xYFrom; iy < xYTo; iy++)
	//			{
	//				for (int ix = xXFrom; ix < xXTo; ix++)
	//				{

	//					//*** Get Color
	//					oColorTemp += aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];

	//					//*** Sum
	//					xGridCount++;
	//				}
	//			}

	//			//*** Average Color
	//			aColor[i] = oColorTemp / (float)xGridCount;
	//		}
	//	}

	//	//*** Set Pixels
	//	oNewTex.SetPixels(aColor);
	//	oNewTex.Apply();

	//	//*** Return
	//	return oNewTex;
	//}

}
