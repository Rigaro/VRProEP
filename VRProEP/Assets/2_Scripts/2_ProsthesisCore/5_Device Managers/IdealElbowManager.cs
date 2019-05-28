//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public class IdealElbowManager : BasicDeviceManager
    {
        // sensor and controllers unnecessary for this elbow device.
        private HingeJoint elbowJoint;
        private JointSpring eJointSpring;
        /// <summary>
        /// Manager for a virtual elbow prosthetic device with ideal tracking.
        /// </summary>
        /// <param name="sensor">The Unity HingeJoint for the elbow.</param>
        public IdealElbowManager(HingeJoint elbowJoint)
        {
            if (elbowJoint == null)
                throw new System.ArgumentNullException("The provided HingeJoint object is empty.");

            this.elbowJoint = elbowJoint;

            // Configure spring
            eJointSpring = this.elbowJoint.spring;
            eJointSpring.spring = 1000.0f;
            eJointSpring.damper = 30.0f;
            this.elbowJoint.useSpring = true;
            this.elbowJoint.spring = eJointSpring;
        }

        /// <summary>
        /// Updates the state of the device for the given channel.
        /// Since it's 1DOF, only one channel available.
        /// Reference determines the desired joint displacement.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="reference">The reference for the device to track.</param>
        public override void UpdateState(int channel, float reference)
        {
            if (channel != 0)
                throw new System.ArgumentException("Only channel 0 available since 1DOF.");

            eJointSpring.targetPosition = (float)System.Math.Round(Mathf.Rad2Deg*reference, 1);
            elbowJoint.spring = eJointSpring;
        }

        /// <summary>
        /// Updates all the states of the device. Since it's 1DOF, only one channel available.
        /// Reference determines the desired joint displacement, only length 1 allowed.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="references">The set of references for the device to track.</param>
        public override void UpdateAllStates(float[] references)
        {
            if (references.Length != 1)
                throw new System.ArgumentException("Only 2 references (position and velocity) required since 1DOF.");

            UpdateState(1, references[0]);
        }
    }
}