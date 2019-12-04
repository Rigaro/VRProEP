using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPosReset : MonoBehaviour
{
    public Transform resetLocation;
    public GameObject objectToReset;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.name);
        if (other.gameObject.name == objectToReset.name)
            StartCoroutine(SetPosition());
    }

    private IEnumerator SetPosition()
    {
        yield return new WaitForSecondsRealtime(5.0f);
        //Debug.Log("Reset!");
        objectToReset.transform.position = resetLocation.position;
        objectToReset.transform.localRotation = Quaternion.identity;
    }
}
