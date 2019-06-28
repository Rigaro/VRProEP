//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// Handles avatar following of a physical tracker.
    /// </summary>
    public class LimbFollower : MonoBehaviour
    {

        private Transform trackerTransform;

        public AvatarType avatarType;
        public Vector3 offset;
        public Vector3 angularOffset;

        // Use this for initialization
        void Start()
        {
            if (avatarType == AvatarType.Transhumeral)
            {
                // Get the shoulder location object and initialize
                GameObject limbTracker = GameObject.FindGameObjectWithTag("ShoulderLocation");
                trackerTransform = limbTracker.transform;
            }
            if (avatarType == AvatarType.Transradial)
            {
                // Get the elbow location object and initialize
                GameObject residualLimbTracker = GameObject.FindGameObjectWithTag("ElbowLocation");
                trackerTransform = residualLimbTracker.transform;
            }
            if (avatarType == AvatarType.AbleBodied)
            {
                // Get the shoulder location object and initialize
                GameObject limbTracker = GameObject.FindGameObjectWithTag("ForearmTracker");
                trackerTransform = limbTracker.transform;
                offset = transform.GetChild(0).transform.position;
                angularOffset = transform.GetChild(0).transform.eulerAngles;
            }
            //offset = new Vector3(0.0f, -transform.localScale.y, 0.0f);
        }

        // LateUpdate is called once per frame at the end of everything else
        void LateUpdate()
        {
            // Update the residual limb position to tracker.
            transform.position = trackerTransform.position;
            transform.rotation = trackerTransform.rotation;
            if (offset != null)
                transform.Translate(offset);
            if (angularOffset != null)
                transform.Rotate(angularOffset);

        }
    }
}
