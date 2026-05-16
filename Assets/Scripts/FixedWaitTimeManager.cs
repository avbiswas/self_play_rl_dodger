using System.Collections.Generic;

using System;
using UnityEngine;

public class FixedWaitTimeManager: MonoBehaviour
{
    Dictionary<string, WaitForFixedTime> coroutines;

    void Start(){
        coroutines = new Dictionary<string, WaitForFixedTime>();

    }
    public void StartFixedCoroutine(string coroutine_name, float waitTime, Action callback=null, bool override_=false, object[] input_params=null){
        if (coroutines == null){
            coroutines = new Dictionary<string, WaitForFixedTime>();
        }
        if (!coroutines.ContainsKey(coroutine_name)){
            WaitForFixedTime wait = gameObject.AddComponent<WaitForFixedTime>();
            coroutines.Add(coroutine_name, wait);
        }
        if (override_){
            coroutines[coroutine_name].StartWait(waitTime, callback, coroutine_name);
        }
        else{
            if (!coroutines[coroutine_name].enabled){
                coroutines[coroutine_name].StartWait(waitTime, callback, coroutine_name);
            }
        }
    }

    public void FinishAllCoroutines(bool invoke=true){
        if (coroutines == null)
            return;
        bool all_stopped = true;
        do {
            foreach (KeyValuePair<string, WaitForFixedTime> c in coroutines){
                c.Value.Stop(invoke);
            }
            foreach (KeyValuePair<string, WaitForFixedTime> c in coroutines){
                if (c.Value.enabled){
                    c.Value.Stop(false);
                }
            }

        }
        while (!all_stopped);

    }

}

