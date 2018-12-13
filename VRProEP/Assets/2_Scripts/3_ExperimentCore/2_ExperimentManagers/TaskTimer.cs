using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Times the performance of a task deterministically using Unity's physics engine.
/// </summary>
public class TaskTimer : MonoBehaviour {

    private bool enable = false;
    private float timer = 0.0f;

    public float Timer
    {
        get
        {
            return timer;
        }
    }

    // Deterministic update
    void FixedUpdate()
    {
        if (enable)
            timer += Time.fixedDeltaTime;
    }

    /// <summary>
    /// Resets the timer to 0.
    /// </summary>
    public void ResetTimer () {
        timer = 0.0f;
    }
	
    /// <summary>
    /// Resets and starts timer.
    /// </summary>
    public void StartTimer()
    {
        ResetTimer();
        enable = true;
    }

    /// <summary>
    /// Stops the timer and returns its value.
    /// </summary>
    /// <returns>The time.</returns>
    public float StopTimer()
    {
        enable = false;
        return timer;
    }

    public bool IsEnabled()
    {
        return enable;
    }
}
