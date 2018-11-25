using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GraspManager : MonoBehaviour {

    public Rigidbody handRB;
    public Transform attachmentPoint;

    private GameObject objectInHand = null;
    private FixedJoint objectInHandFJ = null;

    // When another object comes in contact with the hand grasp
    private void OnTriggerEnter (Collider other) {
        // Make sure the hand is not grasping anything else.
        if (objectInHand != null)
            return;

        // Check that it's a graspable object.
        if (!other.CompareTag("Graspable"))
            return;

        // Attach object to hand with a fixed joint
        objectInHand = other.gameObject;
        objectInHandFJ = objectInHand.AddComponent<FixedJoint>();
        objectInHandFJ.connectedBody = handRB;
        //objectInHandFJ.anchor = attachmentPoint.position;
	}

    private void FixedUpdate()
    {
        // Release when button pressed.
        if (SteamVR_Input.vrproep.inActions.Button.GetStateDown(SteamVR_Input_Sources.Any) && objectInHand != null)
        {
            Destroy(objectInHandFJ);
            objectInHand = null;
        }
    }
}
