using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraspTimeManager : MonoBehaviour {

    public TaskTimer timer;
    public TextMeshPro topConsole;
    private float lastTime;
    private bool inDropOff;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DropOff")
        {
            inDropOff = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "DropOff")
        {
            inDropOff = false;
        }            
    }

    // Update is called once per frame
    void FixedUpdate () {
        // When the object is grabbed and the timer hasn't been enabled, start timer.
        if (GetComponent<ObjectHandTracking>() != null && !timer.IsEnabled())
        {
            timer.StartTimer();
            // Successfull start flag.
        }
        // When the object has been dropped off wrongly while timer is running, stop and reset.
        else if (GetComponent<ObjectHandTracking>() == null && timer.IsEnabled() && !inDropOff)
        {
            timer.StopTimer();
            timer.ResetTimer();
            // Unsuccessfull end flag.
        }
        // When the object is dropped off successfully, stop, save and return.
        else if (GetComponent<ObjectHandTracking>() == null && timer.IsEnabled() && inDropOff)
        {
            lastTime = timer.StopTimer();
            topConsole.text = "Task time: " + lastTime.ToString();
            // Successfull end flag.
        }
    }
}
