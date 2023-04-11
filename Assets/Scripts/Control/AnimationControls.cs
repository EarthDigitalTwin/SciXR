using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;


public class AnimationControls : MonoBehaviour {
    public PinchSlider slider;
    public Text animationText;

    //public ToggleGroup speedToggleGroup;
    public float secondsToAnimate = 2f; 

    private LTDescr tween;
    public DataObject data;

    private bool playToggleOn = false;
    private bool loopToggleOn = false;

    void Start() {
        data = GetComponentInParent<DataObject>();
    }

    public bool IsAnimating() {
        return playToggleOn;
    }

    public void OnPlayToggle(bool status) {
        // status is set up backwards on the toggle so lets flip
        status = !status;
        playToggleOn = status;

        if(status) {
            // Start animating

            // First loop to end
            int numLoops = loopToggleOn ? -1 : 0;
            tween = LeanTween.value(gameObject, 0, 1, secondsToAnimate).setOnUpdate((float val) => { slider.SliderValue = val; }).setRepeat(numLoops);
            tween.setOnComplete(() => { playToggleOn = false; }).setOnCompleteOnRepeat(false);
            if (slider.SliderValue < 1)
                tween.setPassed(secondsToAnimate * slider.SliderValue);
;        }
        else {
            // Stop animating
            if(tween != null) {
                LeanTween.cancel(tween.id);
                tween = null;
            }
        }
    }

    public void OnLoopToggle(bool status) {
        loopToggleOn = status;

        if (status && tween != null) {
            tween.setLoopClamp();
            tween.pause();
            tween.resume();
        }
        else if(tween != null) {
            tween.setLoopOnce();
            tween.pause();
            tween.resume();
        }
    }

    public void UpdateAnimationInfo(SliderEventData info) {
        if( data != null && data.data.results != null && data.data.results.Count > 0) {
            float frame = info.NewValue * data.data.results.Count;
            int frameInt = (int)frame;
            if (frameInt == data.data.results.Count)
                frameInt--;

            float time = data.data.results[frameInt].time;
            int step = data.data.results[frameInt].step;
            string iceVolume = "";

            if (data.data.results[frameInt].props.ContainsKey("IceVolume")) {
                iceVolume = data.data.results[frameInt].props["IceVolume"];
            }

            animationText.text =
                "<b>Time: </b> " + time + "\n" +
                "<b>Step: </b> " + step;
            if (iceVolume != "") {
                animationText.text += "\n" +
                    "<b>IceVolume: </b> " + iceVolume;
            }

            data.SetFrame(frame);
        }
        else {
            animationText.text = "No Animation Available";
        }
    }
}
