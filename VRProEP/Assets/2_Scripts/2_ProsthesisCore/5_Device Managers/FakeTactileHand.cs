//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public class FakeTactileHand : BasicDeviceManager, ISensor
    {
        private float force = 0.0f;
        private float roughness = 0.0f;
        /// <summary>
        /// Updates the state of the device for the given channel.
        /// Since it's 1DOF, only one channel available.
        /// Reference determines the desired force applied.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="reference">The reference for the device to track.</param>
        public override void UpdateState(int channel, float reference)
        {
            if (reference > 1.0f)
                force = 1.0f;
            else
                force = reference;
        }

        /// <summary>
        /// Updates all the states of the device. Since it's 1DOF, only one channel available.
        /// Reference determines the desired force applied.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="references">The set of references for the device to track.</param>
        public override void UpdateAllStates(float[] references)
        {
            if (references.Length != 1)
                throw new System.ArgumentException("Only 2 references (position and velocity) required since 1DOF.");

            UpdateState(1, references[0]);
        }

        /// <summary>
        /// Updates the sensed roughness value.
        /// </summary>
        /// <param name="roughness">The roughness value [0,1].</param>
        public void SenseRoughness(float roughness)
        {
            if (roughness > 1.0f)
                roughness = 1.0f;
            else
                this.roughness = roughness;
        }

        /// <summary>
        /// Applies a force on the object
        /// </summary>
        /// <returns></returns>
        public float ApplyForce()
        {
            return force;
        }


        /// <summary>
        /// Returns raw force and roughness.
        /// 0: force.
        /// 1: roughness.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw force or roughness data.</returns>
        public float GetRawData(int channel)
        {
            if (channel >= 2)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels.");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel number starts from 0.");

            return GetAllRawData()[channel];
        }

        /// <summary> 
        /// Returns raw force and roughness, channel names:
        /// force.
        /// roughness.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Raw tracking data for the given channel.</returns>
        public float GetRawData(string channel)
        {
            int channelNum = 0;
            if (channel.ToLower() == "force")
                channelNum = 0;
            else if (channel.ToLower() == "roughness")
                channelNum = 0;

            return GetRawData(channelNum);
        }

        /// <summary>
        /// Returns all raw joint data in an array.
        /// 0 : Force.
        /// 1 : Roughness.
        /// </summary>
        /// <returns>The array with force and roughness data.</returns>
        public float[] GetAllRawData()
        {
            // Get current prosthesis angle
            float[] x = new float[2]
            {
            force,
            roughness
            };
            return x;
        }

        /// <summary>
        /// Does the same as raw.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public float GetProcessedData(int channel)
        {

            return GetRawData(channel);
        }

        /// <summary>
        /// Does the same as raw.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public float GetProcessedData(string channel)
        {
            return GetRawData(channel);
        }


        /// <summary>
        /// Returns all raw joint data in an array.
        /// 0 : Force.
        /// 1 : Roughness.
        /// </summary>
        /// <returns>The array with force and roughness data.</returns>
        public float[] GetAllProcessedData()
        {
            // Get current prosthesis angle
            float[] x = new float[2]
            {
            force,
            roughness
            };
            return x;
        }

        public SensorType GetSensorType()
        {
            return SensorType.Tactile;
        }
    }
}
