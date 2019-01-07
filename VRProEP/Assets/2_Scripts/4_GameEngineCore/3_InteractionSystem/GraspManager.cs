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

    /// <summary>
    /// Determines the type of manager being used.
    /// - Controller: uses SteamVR Actions to determine object interaction.
    /// - Assisted: Grasping and dropping off is handled automatically. Only works with restricted locations.
    /// </summary>
    public enum GraspManagerType
    {
        Controller,
        Assisted
    }

    /// <summary>
    /// Determines the mode the manager operates in.
    /// - Open: no restrictions for grasping and dropping off.
    /// - Restricted: Dropping off only available in specific locations.
    /// </summary>
    public enum GraspManagerMode
    {
        Open,
        Restriced
    }

    [Tooltip("Enable when the hand is a SteamVR tracked object.")]
    public bool isTrackedObject = false;
    [Tooltip("The hand's object attachment point transform.")]
    public Transform attachmentPoint;

    [Header("Grasp behaviour settings")]
    public GraspManagerType managerType = GraspManagerType.Controller;
    public GraspManagerMode managerMode = GraspManagerMode.Open;
    public float dropOffVelocityThreshold = 0.02f;

    [Header("Throw physics settings")]
    [Tooltip("Enables throw physics. CAUTION, BUGGY.")]
    public bool throwEnable = false;
    public float throwMultiplier = 1.0f;

    private GameObject handGO;
    private Rigidbody handRB;
    private GameObject objectInHand = null;
    private ObjectHandTracking oIHHandTracker = null;
    private GameObject objectGraspable = null;
    private bool inGrasp = false;
    private bool inDropOff = false;
    private bool releasing = false;
    private float handVelocity = 0.0f;
    private Vector3 prevHandPosition;

    private void Start()
    {
        // Check that the action set is active.
        if (!SteamVR_Input.vrproep.IsActive())
        {
            SteamVR_Input._default.Deactivate();
            SteamVR_Input.vrproep.ActivatePrimary();
        }
        
        handGO = GameObject.FindGameObjectWithTag("Hand");
                
        // Get tracking handle when able-bodied
        if (!isTrackedObject)
        {
            // Get hand rigid body when prosthesis
            handRB = handGO.GetComponent<Rigidbody>();

            if (handRB == null)
                throw new System.Exception("Hand Rigidbody not found.");
        }
        else
        {
            prevHandPosition = handGO.transform.position;
        }
    }

    // When another object comes in contact with the hand grasp
    private void OnTriggerEnter (Collider other) {
        // Make sure the hand is not grasping anything else and check that it's a graspable object.
        if (objectInHand == null && other.CompareTag("Graspable"))
        {
            inGrasp = true;
            objectGraspable = other.gameObject;
        }

        // If object in hand and in a DropOff point enable drop-off.
        else if (objectInHand != null && objectGraspable == null && other.tag == "DropOff")
        {
            inDropOff = true;
        }

    }

    // When leaving a drop-off point, disable drop-off.
    private void OnTriggerExit(Collider other)
    {
        // If the object was not holding anything and left a graspable object, clear flag and handle.
        if (objectInHand == null && other.CompareTag("Graspable"))
        {
            inGrasp = false;
            objectGraspable = null;
        }
        // If left a drop-off point, clear flag.
        if (other.tag == "DropOff")
        {
            inDropOff = false;
        }
    }

    private void FixedUpdate()
    {
        if (inGrasp && objectInHand == null)
            HandleGrasp();
        else if ((inDropOff || managerMode == GraspManagerMode.Open) && objectInHand != null)
            HandleDropOff();

        if (isTrackedObject)
        {
            EstimateHandVelocityMagnitude();
        }
    }

    /// <summary>
    /// Handles grasping behaviour.
    /// </summary>
    private void HandleGrasp()
    {
        // Check manager type and if requested to interact.
        if (managerType == GraspManagerType.Assisted || (managerType == GraspManagerType.Controller && SteamVR_Input.vrproep.inActions.ObjectInteractButton.GetStateDown(SteamVR_Input_Sources.Any)))
        {
            if (objectGraspable == null)
                throw new System.NullReferenceException("There was no object found within grasp.");

            // Attach object to hand with hand Tracking script
            objectInHand = objectGraspable;
            oIHHandTracker = objectInHand.AddComponent<ObjectHandTracking>();
            oIHHandTracker.handTransform = attachmentPoint;
            // clear flag and handle
            inGrasp = false;
            objectGraspable = null;
        }
    }

    /// <summary>
    /// Handles drop-off behaviour.
    /// </summary>
    private void HandleDropOff()
    {

        // Determine behaviour depending on mode.
        if (managerType == GraspManagerType.Controller)
        {
            // Releases object when the object interaction button is pressed and there is an object in hand.
            if (SteamVR_Input.vrproep.inActions.ObjectInteractButton.GetStateDown(SteamVR_Input_Sources.Any) && objectInHand != null && (managerMode == GraspManagerMode.Open || (managerMode == GraspManagerMode.Restriced && inDropOff)))
            {
                ReleaseObjectInHand();
            }
        }
        else if (managerType == GraspManagerType.Assisted)
        {
            // Get hand velocity from physics engine when not tracked object
            if (!isTrackedObject)
            {
                handVelocity = handRB.velocity.magnitude;
            }
            // Get hand velocity and compare to threshold.
            if (handVelocity < dropOffVelocityThreshold && objectInHand != null && !releasing)
            {
                ReleaseObjectInHand();
            }
        }
    }

    /// <summary>
    /// Releases the object in hand.
    /// </summary>
    private void ReleaseObjectInHand()
    {
        // Only release when the object is in hand.
        if (objectInHand != null)
        {
            // Stop tracking
            Destroy(oIHHandTracker);
            oIHHandTracker = null;

            // Throw if enabled
            if (throwEnable)
            {
                // Get hand velocity from physics engine when not tracked object
                if (!isTrackedObject)
                {
                    // Set object in hand velocity to that of the hand.
                    objectInHand.GetComponent<Rigidbody>().velocity = throwMultiplier * handRB.velocity;
                    objectInHand.GetComponent<Rigidbody>().angularVelocity = throwMultiplier * handRB.angularVelocity;
                }
                else
                {
                    objectInHand.GetComponent<Rigidbody>().velocity = throwMultiplier * EstiamteHandVelocity();
                }
            }
            // Start release and cool-off period.
            releasing = true;
            StartCoroutine(EnableObjectGraspability(2.0f));
        }
    }

    /// <summary>
    /// Estimates hand velocity magnitude differentially. Uses Unity physics engine delta time.
    /// </summary>
    private void EstimateHandVelocityMagnitude()
    {
        Vector3 poseDifference = handGO.transform.position - prevHandPosition;
        handVelocity = poseDifference.magnitude / Time.fixedDeltaTime;
        prevHandPosition = handGO.transform.position;
    }

    private Vector3 EstiamteHandVelocity()
    {
        Vector3 poseDifference = handGO.transform.position - prevHandPosition;
        return new Vector3 ( poseDifference.x / Time.fixedDeltaTime, poseDifference.y / Time.fixedDeltaTime, poseDifference.z / Time.fixedDeltaTime);
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
        releasing = false;
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
