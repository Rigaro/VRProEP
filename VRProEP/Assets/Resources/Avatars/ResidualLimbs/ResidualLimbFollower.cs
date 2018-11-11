using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidualLimbFollower : MonoBehaviour {

    private Transform shoulderTransform;
    private Vector3 offset;

	// Use this for initialization
	void Start ()
    {
        // Get the shoulder location object and initialize
        GameObject residualLimbTracker = GameObject.FindGameObjectWithTag("ShoulderLocation");
        shoulderTransform = residualLimbTracker.transform;
        offset = new Vector3(0.0f, -transform.localScale.y, 0.0f);
    }
	
	// Update is called once per frame
	void LateUpdate () {
        // Update the residual limb position to tracker.
        transform.position = shoulderTransform.position;
        transform.rotation = shoulderTransform.rotation;
        transform.Translate(offset);
    }
}
