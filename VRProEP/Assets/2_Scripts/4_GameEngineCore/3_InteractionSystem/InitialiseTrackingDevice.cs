using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class InitialiseTrackingDevice : MonoBehaviour
{
    public SteamVR_Behaviour_Pose behaviourPose;
    public SteamVR_TrackedObject trackedObject;

    // Start is called before the first frame update
    void Start()
    {
        if (behaviourPose == null)
            behaviourPose = GetComponent<SteamVR_Behaviour_Pose>();
        if (trackedObject == null)
            trackedObject = GetComponent<SteamVR_TrackedObject>();

        int deviceIndex = behaviourPose.GetDeviceIndex();
        Debug.Log(deviceIndex);
        trackedObject.SetDeviceIndex(deviceIndex);
    }
}
