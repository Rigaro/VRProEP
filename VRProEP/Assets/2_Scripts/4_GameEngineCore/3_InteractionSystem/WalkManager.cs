using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class WalkManager : MonoBehaviour {

    public Transform player;
    private Transform headTransform;

    public SteamVR_Action_Boolean walkEnable;
    public SteamVR_Input_Sources inputSource;

    // Use this for initialization
    void Start ()
    {
        // Check that the action set is active.
        if (!SteamVR_Input.GetActionSet("vrproep").IsActive())
        {
            SteamVR_Input.GetActionSet("default").Deactivate();
            SteamVR_Input.GetActionSet("vrproep").Activate();
        }

        // Get head transform
        headTransform = GameObject.FindGameObjectWithTag("Head").transform;

        if (headTransform == null)
            throw new System.Exception("Head tracking object not found.");
    }
	
	// Update walking with physics
	void FixedUpdate ()
    {
        if (walkEnable.GetState(inputSource))
        {            
            Vector2 trackpad = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("Trackpad").GetAxis(SteamVR_Input_Sources.Any);
            Vector3 walkingMotion = new Vector3(trackpad.x * Time.fixedDeltaTime, 0.0f, trackpad.y * Time.fixedDeltaTime);
            player.position += new Vector3(headTransform.TransformVector(walkingMotion).x, 0.0f, headTransform.TransformVector(walkingMotion).z);
        }

    }
}
