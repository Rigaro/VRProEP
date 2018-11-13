//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public class ConfigurableElbowManager : MonoBehaviour
    {
        private ConfigurableInputManager inputManager;
        private ElbowManager elbowManager;

        /// <summary>
        /// Initializes the Elbow prosthesis with basic functionality.
        /// Must be called only after the avatar is available.
        /// </summary>
        public void InitializeProsthesis()
        {
            // ConfigurableInputManagar
            // Create a VIVETracker and basic reference generator.
            // Create configurable input manager.
            
            // ElbowManager
            // Find Elbow_Lower GameObject and extract its HingeJoint and Rigidbody
            // Create VirtualEncoder and attach to HingeJoint.
            // Create ElbowManager with the given VirtualEncoder and Rigidbody.
        }

    }
}
