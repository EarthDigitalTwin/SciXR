using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CloseButton : MonoBehaviour {

    public Button initCloseButton;
    public Button confirmButton;
    public Button cancelButton;

    public float delayUntilReset = 4.0f;

    public UnityEvent OnClick;

    private float lastTimeChecked;

    private void Start() {
        initCloseButton.onClick.AddListener(() => {
            initCloseButton.gameObject.SetActive(false);
            confirmButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
            lastTimeChecked = Time.time;
        });
        cancelButton.onClick.AddListener(()=> {
            initCloseButton.gameObject.SetActive(true);
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
        });
        confirmButton.onClick.AddListener(() => { OnClick.Invoke(); });

        confirmButton.GetComponent<EventTrigger>().triggers
            .Find(i => { return i.eventID == EventTriggerType.PointerEnter; })
            .callback.AddListener(eventData => { lastTimeChecked = Time.time; });

        confirmButton.GetComponent<EventTrigger>().triggers
            .Find(i => { return i.eventID == EventTriggerType.PointerExit; })
            .callback.AddListener(eventData => { lastTimeChecked = Time.time; });

        cancelButton.GetComponent<EventTrigger>().triggers
            .Find(i => { return i.eventID == EventTriggerType.PointerEnter; })
            .callback.AddListener(eventData => { lastTimeChecked = Time.time; });

        cancelButton.GetComponent<EventTrigger>().triggers
            .Find(i => { return i.eventID == EventTriggerType.PointerExit; })
            .callback.AddListener(eventData => { lastTimeChecked = Time.time; });
    }

    // Use this for initialization
    void OnEnable () {
        initCloseButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (initCloseButton.gameObject.activeSelf == false && lastTimeChecked + delayUntilReset < Time.time) {
            initCloseButton.gameObject.SetActive(true);
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
        }
    }
}
