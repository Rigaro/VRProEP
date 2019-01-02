using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes an object track the given hand transform.
/// </summary>
public class ObjectHandTracking : MonoBehaviour {

    public Transform handTransform;
    private Transform attachOffset;

    //private bool isEnabled = false;

	// Use this for initialization
	void Start () {
        attachOffset = transform.GetChild(0).transform; // Set the offset from the object's child attachment position object.
        GetComponent<Collider>().isTrigger = true; // Disable collisions, only act as trigger.
        GetComponent<Rigidbody>().useGravity = false; // Disable gravity and physics.
        GetComponent<Rigidbody>().isKinematic = true;
        tag = "Untagged"; // Make the object un-graspable.
    }

    // Update is called deterministically with physics system
    void FixedUpdate ()
    {
        // If the transform has been defined
        if (handTransform != null)
        {
            UpdateTransform();
        }
    }

    private void OnDestroy()
    {
        // Re-enable physics and collisions
        GetComponent<Collider>().isTrigger = false;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    /// <summary>
    /// Update the position and orientation of the object.
    /// </summary>
    private void UpdateTransform()
    {
        transform.position = handTransform.position; // Set position same as hand
        Vector3 positionOffset = handTransform.position - attachOffset.position; // Offset to attachment point.
        transform.position += positionOffset;
        transform.rotation = handTransform.rotation; // Set rotation to same as hand
        transform.Rotate(attachOffset.localRotation.eulerAngles); // Offset rotation.
    }

}
