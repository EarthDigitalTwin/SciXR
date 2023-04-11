using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ThreadManager : MonoBehaviour 
{
    public static ThreadManager instance;
    public List<Action> callbacks;

    void Start() {
        instance = this;
        callbacks = new List<Action>();
    }

    void Update() {
        if(callbacks != null) {
            while (callbacks.Count > 0) {
                Action function = callbacks[0];
                callbacks.RemoveAt(0);
                if (function != null)
                    function();
            }
        }
    }
}
