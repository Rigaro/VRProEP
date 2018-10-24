//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public class ElbowManager : BasicDeviceManager
    {
        private Rigidbody elbowRB;

        public ElbowManager(ISensor sensor)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor object is empty.");

            float[] gains = { 100.0f, 5.0f};
            controller = new StateFeedbackController(2, gains);
            this.sensor = sensor;
        }

        public ElbowManager(ISensor sensor, Rigidbody elbowRB)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor object is empty.");

            float[] gains = { 100.0f, 5.0f };
            controller = new StateFeedbackController(2, gains);
            this.sensor = sensor;
            SetElbowDevice(elbowRB);
        }

        public ElbowManager(float[] controllerGains, ISensor sensor)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor object is empty.");
            if (controllerGains.Length != 2)
                throw new System.ArgumentOutOfRangeException("Two gains are required for a single joint (elbow) controller.");

            controller = new StateFeedbackController(2, controllerGains);
            this.sensor = sensor;
        }

        public ElbowManager(float[] controllerGains, ISensor sensor, Rigidbody elbowRB)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor object is empty.");
            if (controllerGains.Length != 2)
                throw new System.ArgumentOutOfRangeException("Two gains are required for a single joint (elbow) controller.");

            controller = new StateFeedbackController(2, controllerGains);
            this.sensor = sensor;
            SetElbowDevice(elbowRB);
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
            if (channel != 1)
                throw new System.ArgumentException("Only channel 1 available since 1DOF.");

            // Get sensor data
            float[] x = sensor.GetAllProcessedData();
            // Create a full reference for controller
            float[] xBar = { reference, 0.0f };
            // Update device joint torque
            float u = controller.UpdateControlInput(xBar, x);
            elbowRB.AddRelativeTorque(Vector3.left * u);
        }

        /// <summary>
        /// Updates all the states of the device. Useful for multi DOF devices.
        /// Since it's 1DOF, only one channel available.
        /// Reference determines the desired joint displacement, only first reference will be used.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="references">The set of references for the device to track.</param>
        public override void UpdateAllStates(float[] references)
        {

        }

        /// <summary>
        /// Assigns a Unity Rigidbody as the elbow prosthetic device.
        /// </summary>
        /// <param name="elbowRB">The Unity Rigidbody to use as device.</param>
        public void SetElbowDevice(Rigidbody elbowRB)
        {
            if (elbowRB == null)
                throw new System.ArgumentNullException("The provided Rigidbody object is empty.");

            this.elbowRB = elbowRB;
        }
    }
}