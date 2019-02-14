using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPositionManager : MonoBehaviour
{
    private bool isHandInPosition;

    public bool IsHandInPosition { get => isHandInPosition; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GraspManager"))
            isHandInPosition = true;
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.CompareTag("GraspManager"))
            isHandInPosition = false;
    }
}
