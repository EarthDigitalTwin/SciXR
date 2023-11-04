﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandVRControls : MonoBehaviour {
    public GameObject loadMenu;
    public CanvasGroup screenshotMenu;
    public GameObject slicer;
	public GameObject screenshot;
    public GameObject[] rooms;

    public CanvasGroup mainMenuGroup;


	// Use this for initialization
	void Start () {
    }

    //Main Menu Button Callbacks

    public void LoadModelClick() {
        ScreenshotCloseClick();
        if(!loadMenu.gameObject.activeSelf) {
            loadMenu.gameObject.SetActive(true);
            //LeanTween.alphaCanvas(loadMenu, 1, 0.2f);
            //LeanTween.moveLocalX(loadMenu, 434, 0.2f).setEaseOutCubic();
        }
        else {
            loadMenu.GetComponent<FileLoadMenu>()?.BeginDisable();
            loadMenu.GetComponent<FileLoad2DMenu>()?.BeginDisable();
            //LeanTween.alphaCanvas(loadMenu, 0, 0.2f);
            //LeanTween.moveLocalX(loadMenu, 400, 0.2f).setEaseInCubic().setOnComplete(() => { loadMenu.SetActive(false); });
        }
    }

    public void SlicerToggleClick() {
        if(!slicer.activeSelf) {
            slicer.SetActive(true);
            slicer.transform.position = GetFrontOfCamera();
        }
        else {
            slicer.SetActive(false);
        }

    }
    public void ScreenshotClick() {
        

        LoadMenuCloseClick();
        if (!screenshotMenu.gameObject.activeSelf) {
            screenshot.SetActive(true);
            screenshot.transform.position = GetFrontOfCamera(1);
            screenshotMenu.gameObject.SetActive(true);
            screenshotMenu.alpha = 0;
            LeanTween.alphaCanvas(screenshotMenu, 1, 0.2f);
            //LeanTween.moveLocalX(screenshotMenu.gameObject, 280, 0.2f).setEaseOutCubic();
        }
        else {
            ScreenshotCloseClick();
        }
    }
    public void SceneryClick() {
        //for (int count = 0; count < rooms.Length; count++) {
        //    if (rooms[count].activeSelf) {
        //        int nextCount = count + 1;
        //        if (nextCount == rooms.Length) {
        //            nextCount = 0;
        //        }
        //        rooms[count].SetActive(false);
        //        rooms[nextCount].SetActive(true);
        //        return;
        //    }
        //}
    }
    public void CloseClick() {
        if (mainMenuGroup.gameObject.activeSelf) {
            LeanTween.alphaCanvas(mainMenuGroup, 0, 0.2f).setOnComplete(() => { mainMenuGroup.gameObject.SetActive(false); });
        }
        else {
            mainMenuGroup.gameObject.SetActive(true);
            LeanTween.alphaCanvas(mainMenuGroup, 1, 0.2f);
        }
    }

    public void LoadMenuCloseClick() {
        //LeanTween.alphaCanvas(loadMenu, 0, 0.2f);
        //LeanTween.moveLocalX(loadMenu.gameObject, 400, 0.2f).setEaseInCubic().setOnComplete(() => { loadMenu.gameObject.SetActive(false); });
        //loadMenu.GetComponent<FileLoadMenu>()?.BeginDisable();
        if(loadMenu.activeSelf)
            loadMenu.GetComponent<FileLoad2DMenu>()?.BeginDisable();
    }

    public void ScreenshotCloseClick() {
		screenshot.SetActive (false);
        LeanTween.alphaCanvas(screenshotMenu, 0, 0.2f).setOnComplete(() => { screenshotMenu.gameObject.SetActive(false); });
        //LeanTween.moveLocalX(screenshotMenu.gameObject, 214, 0.2f).setEaseInCubic().setOnComplete(() => { screenshotMenu.gameObject.SetActive(false); });
    }

    //Helpers
	private Vector3 GetFrontOfCamera(float height = 1.2f) {
        return new Vector3(0, 0, 0);
    }
}
