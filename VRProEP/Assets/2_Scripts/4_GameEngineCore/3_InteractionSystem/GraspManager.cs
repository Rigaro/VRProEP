//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
//#define DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Manages object grasping behavior for VRProEP platform.
/// </summary>
public class GraspManager : MonoBehaviour {

    public Transform attachmentPoint;
    public float throwMultiplier = 1.0f;

    private Rigidbody handRB;
    private GameObject objectInHand = null;
    private ObjectHandTracking oIHHandTracker = null;

    private void Start()
    {
        // Check that the action set is active.
        if (!SteamVR_Input.vrproep.IsActive())
        {
            SteamVR_Input._default.Deactivate();
            SteamVR_Input.vrproep.ActivatePrimary();
        }
        // Get hand rigid body
        GameObject handGO = GameObject.FindGameObjectWithTag("Hand");
        handRB = handGO.GetComponent<Rigidbody>();

        if (handRB == null)
            throw new System.Exception("Hand Rigidbody not found.");
    }

    // When another object comes in contact with the hand grasp
    private void OnTriggerEnter (Collider other) {
        // Make sure the hand is not grasping anything else.
        if (objectInHand != null)
            return;

        // Check that it's a graspable object.
        if (!other.CompareTag("Graspable"))
            return;

        // Attach object to hand with hand Tracking script
        objectInHand = other.gameObject;
        oIHHandTracker = objectInHand.AddComponent<ObjectHandTracking>();
        oIHHandTracker.handTransform = attachmentPoint;
    }

    private void FixedUpdate()
    {
        // Releases object when the object interaction button is pressed and there is an object in hand.
        if (SteamVR_Input.vrproep.inActions.ObjectInteractButton.GetStateDown(SteamVR_Input_Sources.Any) && objectInHand != null)
        {
            Destroy(oIHHandTracker);
            oIHHandTracker = null;
            StartCoroutine(EnableObjectGraspability(1.0f));

            // Set object in hand velocity to that of the hand.
            //objectInHand.GetComponent<Rigidbody>().velocity = throwMultiplier * handRB.velocity;
            //objectInHand.GetComponent<Rigidbody>().angularVelocity = throwMultiplier * handRB.angularVelocity;
        }
    }

    /// <summary>
    /// Re-enables the object's graspability after the given time.
    /// </summary>
    /// <param name="time">The time in seconds.</param>
    /// <returns>Coroutine IEnumerator</returns>
    private IEnumerator EnableObjectGraspability(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        objectInHand.tag = "Graspable"; // Make the object graspable again.
        objectInHand = null; // Allow grasping again.
    }

    // Clear object from hand and return to normal state if quitting application
    private void OnApplicationQuit()
    {
        if(objectInHand != null)
        {
            Destroy(oIHHandTracker);
            oIHHandTracker = null;
            StartCoroutine(EnableObjectGraspability(1.0f));
        }
    }
}
