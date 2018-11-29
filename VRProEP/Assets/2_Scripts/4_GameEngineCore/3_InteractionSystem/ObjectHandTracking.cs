using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHandTracking : MonoBehaviour {

    public Transform handTransform;
    private Transform attachOffset;

    private bool isEnabled = false;

	// Use this for initialization
	void Start () {
        attachOffset = transform.GetChild(0).transform;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
        tag = "Untagged";
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (handTransform != null)
        {
            UpdateTransform();
        }
    }

    private void OnDestroy()
    {
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    private void UpdateTransform()
    {
        transform.position = handTransform.position;
        Vector3 positionOffset = handTransform.position - attachOffset.position;
        transform.position += positionOffset;
        transform.rotation = handTransform.rotation;
        transform.Rotate(attachOffset.localRotation.eulerAngles);
    }

}
