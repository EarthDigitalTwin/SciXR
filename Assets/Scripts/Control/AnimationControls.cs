using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;


public class AnimationControls : MonoBehaviour {
    public Slider slider;
    public Toggle playPauseToggle;
    public Toggle loopToggle;
    public Text animationText;

    //public ToggleGroup speedToggleGroup;
    public float secondsToAnimate = 2f; 

    private LTDescr tween;
    public DataObject data;

    void Start() {
        data = GetComponentInParent<DataObject>();
    }

    public bool IsAnimating() {
        return !playPauseToggle.isOn;
    }

    public void OnPlayToggle(bool status) {
        // status is set up backwards on the toggle so lets flip
        status = !status;

        if(status) {
            // Start animating

            // First loop to end
            int numLoops = loopToggle.isOn ? -1 : 0;
            tween = LeanTween.value(gameObject, 0, slider.maxValue, secondsToAnimate).setOnUpdate((float val) => { slider.value = val; }).setRepeat(numLoops);
            tween.setOnComplete(() => { playPauseToggle.isOn = true; }).setOnCompleteOnRepeat(false);
            if (slider.normalizedValue < 1)
                tween.setPassed(secondsToAnimate * slider.normalizedValue);
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

    public void UpdateAnimationInfo(float frame) {
        if( data != null && data.data.results != null && data.data.results.Count > 0) {
            int frameInt = (int)frame;

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
        }
        else {
            animationText.text = "No Animation Available";
        }
    }
}
