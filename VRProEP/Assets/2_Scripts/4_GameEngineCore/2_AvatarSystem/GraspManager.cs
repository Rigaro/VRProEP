using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GraspManager : MonoBehaviour {

    public Rigidbody handRB;
    public Transform attachmentPoint;

    private GameObject objectInHand = null;
    // private FixedJoint objectInHandFJ = null;
    private ObjectHandTracking oIHHandTracker = null;

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

    /* Debug 
    private void FixedUpdate()
    {
        // Release when button pressed.
        if (SteamVR_Input.vrproep.inActions.ObjectInteractButton.GetStateDown(SteamVR_Input_Sources.Any) && objectInHand != null)
        {
            Destroy(oIHHandTracker);
            oIHHandTracker = null;
            StartCoroutine(EnableObjectGraspability());
        }
    }*/

    private IEnumerator EnableObjectGraspability()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        objectInHand.tag = "Graspable";
        objectInHand = null;
    }

    /* Debug */
    private void OnApplicationQuit()
    {
        Destroy(oIHHandTracker);
        oIHHandTracker = null;
        StartCoroutine(EnableObjectGraspability());
    }
}
