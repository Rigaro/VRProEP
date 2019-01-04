using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraspTaskManager : MonoBehaviour {

    [Tooltip("Timer object to time task.")]
    public TaskTimer timer;
    [Tooltip("The object's start transform. Used for respawning.")]
    public Transform startTransform;
    [Tooltip("Enables respawning of the object after task completion.")]
    public bool respawnEnabled = true;

    [Header("Debug")]
    public TextMeshPro topConsole; // Debug

    private bool runFlag = false;
    private bool successFlag = false;
    private float lastTime;
    private bool inDropOff;

    public bool SuccessFlag { get => successFlag; }
    public bool RunFlag { get => runFlag; }

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
        if (GetComponent<ObjectHandTracking>() != null && !timer.IsEnabled() && respawnEnabled)
        {
            timer.StartTimer();
            // Successfull start flag.
            runFlag = true;
            successFlag = false;
        }
        // When the object has been dropped off wrongly while timer is running, stop and reset.
        else if (GetComponent<ObjectHandTracking>() == null && timer.IsEnabled() && !inDropOff)
        {
            timer.StopTimer();
            timer.ResetTimer();
            // Unsuccessfull end flag.
            runFlag = false;
            successFlag = false;
        }
        // When the object is dropped off successfully, stop, save and return.
        else if (GetComponent<ObjectHandTracking>() == null && timer.IsEnabled() && inDropOff)
        {
            lastTime = timer.StopTimer();
            topConsole.text = "Task time: " + lastTime.ToString();
            // Successfull end flag.
            runFlag = false;
            successFlag = true;
            respawnEnabled = false;
            // Return to start
            StartCoroutine(ReturnToStart());
        }
    }

    private IEnumerator ReturnToStart()
    {
        // Wait
        yield return new WaitForSecondsRealtime(2.0f);
        // Hide before teleporting.
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        // Set transform.
        gameObject.transform.position = startTransform.position;
        gameObject.transform.localRotation = startTransform.localRotation;
        // Wait
        yield return new WaitForSecondsRealtime(1.0f);
        yield return new WaitUntil(() => respawnEnabled);
        // Show
        gameObject.GetComponent<MeshRenderer>().enabled = true;
    }

    public float GetLastTime()
    {
        return lastTime;
    }
}
