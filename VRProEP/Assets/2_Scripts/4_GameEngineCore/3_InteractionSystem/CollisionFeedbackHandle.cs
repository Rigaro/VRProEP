using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CollisionFeedbackHandle : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float intensity = 0.5f;
    public float duration = 1.0f;
    public float frequency = 100.0f;

    private SteamVR_Action_Vibration hapticFeedback = SteamVR_Input.GetVibrationAction("HapticFeedback");

    // Vibrate when colliding with non-graspable or non-drop-off objects.
    private void OnCollisionEnter(Collision collision)
    {
        if (!(collision.gameObject.CompareTag("Graspable") || collision.gameObject.CompareTag("DropOff") || collision.gameObject.CompareTag("GraspManager") || collision.gameObject.CompareTag("Forearm") || collision.gameObject.CompareTag("Hand")))
        {
            hapticFeedback.Execute(0.0f, duration, frequency, intensity, SteamVR_Input_Sources.Any);
        }
    }
}
