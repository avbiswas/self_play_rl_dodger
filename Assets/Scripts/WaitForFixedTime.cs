using System;
using UnityEngine;

public class WaitForFixedTime: MonoBehaviour
{
    float waitTime;
    float currentTime;
    Action callbackFunction;
    public string taskname;

    void Awake(){
        Stop();
    }

    public void StartWait(float wait, Action callback=null, string taskname=""){
        callbackFunction = callback;
        waitTime = wait;
        currentTime = 0;
        this.enabled = true;
        this.taskname = taskname;
    }

    public void Stop(bool triggerCallback=false){
        if (triggerCallback){
            callbackFunction?.Invoke();
        }
        callbackFunction = null;
        currentTime = 0;
        this.enabled = false;
    }

    void FixedUpdate(){
        if (!this.enabled){
            return;
        }
        currentTime += Time.fixedDeltaTime;
        if (currentTime > waitTime){
            callbackFunction?.Invoke();
            Stop();
        }
    }


}

