using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class ScreenshotControls : MonoBehaviour {

    public static ScreenshotControls instance;

    public enum ScreenshotMode {
        Normal,
        Hybrid,
        Minimal,
        Publication
    }

    [Header("View Options")]
    public LayerMask publicationModeLayerMask;
    public ScreenshotMode currentMode = ScreenshotMode.Normal;

    [Header("References")]
    public Camera screenshotCamera;
    public GameObject previewContainer;
    public GameObject previewButtonPrefab;
    public GameObject previewWindowPrefab;

    // Use this for initialization
    void Start () {
        instance = this;
	}

    void OnValidate() {
        ProcessModeChange();
    }

    public void TakeScreenshot() {
        RenderTexture rt = screenshotCamera.targetTexture;
        Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        screenShot.Apply();
        RenderTexture.active = null;
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(rt.width, rt.height);
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));

        // Create Preview
        GameObject newPreviewButton = Instantiate(previewButtonPrefab, previewContainer.transform);
        newPreviewButton.GetComponentInChildren<Button>().onClick.AddListener(() => { CreateLargePreview(screenShot); });
        newPreviewButton.GetComponentInChildren<RawImage>().texture = screenShot;
    }

    public void CreateLargePreview(Texture2D tex) {

    }

    public ScreenshotMode CycleMode() {
        int numModes = System.Enum.GetValues(typeof(ScreenshotMode)).Length;
        if((int) currentMode == numModes - 1) {
            currentMode = 0;
        }
        else {
            currentMode++;
        }

        return currentMode;
    }

    private string ScreenShotName(int width, int height) {
        string basePath = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
        if(!Directory.Exists(basePath)) {
            Directory.CreateDirectory(basePath);
        }
        return string.Format(Path.Combine(basePath, "screen_{0}x{1}_{2}.png"),
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    private void ProcessModeChange (){
        switch (currentMode) {
            case ScreenshotMode.Normal:
                screenshotCamera.cullingMask = ~0;
                break;
            case ScreenshotMode.Publication:
                screenshotCamera.cullingMask = publicationModeLayerMask;
                break;
            default:
                break;
        }
    }

}
