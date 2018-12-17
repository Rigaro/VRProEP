//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// Handles avatar following of a physical tracker.
    /// </summary>
    public class ResidualLimbFollower : MonoBehaviour
    {

        private Transform trackerTransform;

        public AvatarType avatarType;
        public Vector3 offset;

        // Use this for initialization
        void Start()
        {
            if (avatarType == AvatarType.Transhumeral)
            {
                // Get the shoulder location object and initialize
                GameObject residualLimbTracker = GameObject.FindGameObjectWithTag("ShoulderLocation");
                trackerTransform = residualLimbTracker.transform;
            }
            if (avatarType == AvatarType.Transradial)
            {
                throw new System.NotImplementedException("Transradial avatars not yet implemented");
                // Get the shoulder location object and initialize
                //GameObject residualLimbTracker = GameObject.FindGameObjectWithTag("ElbowLocation");
                //trackerTransform = residualLimbTracker.transform;
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

        }
    }
}
