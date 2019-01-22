//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;
using System.Collections;

namespace VRProEP.ProsthesisCore
{
    public class VirtualPositionTracker : SensorManager
    {
        private Transform trackingTransform;

        /// <summary>
        /// Virtual position tracker that measures the position of a Unity GameObject.
        /// </summary>
        public VirtualPositionTracker() : base(6, SensorType.VirtualPositionTracker)
        {

        }

        /// <summary>
        /// Virtual position tracker that measures the position of a Unity GameObject.
        /// </summary>
        /// <param name="trackingTransform">The Transform of the object to be tracked.</param>
        public VirtualPositionTracker(Transform trackingTransform) : base(6, SensorType.VirtualPositionTracker)
        {
            SetTrackingTransform(trackingTransform);
        }

        /// <summary>
        /// Assigns the object transform to be tracked.
        /// </summary>
        /// <param name="trackingTransform">The Transform of the object to be tracked.</param>
        public void SetTrackingTransform(Transform trackingTransform)
        {

            this.trackingTransform = trackingTransform ?? throw new System.ArgumentNullException("The provided transform is empty.");
        }

        /// <summary>
        /// Returns requested object position data.
        /// 0-2: Position given in meters.
        /// 3-5: Angular position given locally in Euler Angles in degrees.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw tracking data for the given channel.</returns>
        public override float GetRawData(int channel)
        {
            if (channel >= ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels.");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel number starts from 0.");

            return GetAllRawData()[channel];

        }

        /// <summary>
        /// Not implemented, use index version of method.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Raw tracking data for the given channel.</returns>
        public override float GetRawData(string channel)
        {
            throw new System.NotImplementedException("Use index version of method.");
        }

        /// <summary>
        /// Returns all raw object position data in an array as provided by Unity Transform.
        /// Position given in meters.
        /// Angular position given locally in degrees.
        /// </summary>
        /// <returns>The array with raw position and angular position data.</returns>
        public override float[] GetAllRawData()
        {
            // Get object position
            float[] x = new float[6]
            {
            trackingTransform.position.x,
            trackingTransform.position.y,
            trackingTransform.position.z,
            trackingTransform.localEulerAngles.x,
            trackingTransform.localEulerAngles.y,
            trackingTransform.localEulerAngles.z,
            };
            return x;
        }

        /// <summary>
        /// Returns requested object position data.
        /// 0-2: Position given in meters.
        /// 3-5: Angular position given locally in Euler Angles in radians.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Position data for the given channel.</returns>
        public override float GetProcessedData(int channel)
        {
            if (channel >= ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels.");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel number starts from 0.");

            return GetAllProcessedData()[channel];
        }

        /// <summary>
        /// Not implemented, performs GetRawData.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public override float GetProcessedData(string channel)
        {
            throw new System.NotImplementedException("Use index version of method.");
        }

        /// <summary>
        /// Returns all object position data in an array as provided by Unity Transform.
        /// Position given in meters.
        /// Angular position given locally in radians.
        /// </summary>
        /// <returns>The array with pre-processed angular position and velocity data.</returns>
        public override float[] GetAllProcessedData()
        {
            // Get object position
            float[] x = new float[6]
            {
            trackingTransform.position.x,
            trackingTransform.position.y,
            trackingTransform.position.z,
            trackingTransform.rotation.eulerAngles.x,
            trackingTransform.rotation.eulerAngles.y,
            trackingTransform.rotation.eulerAngles.z,
            };
            return x;
        }

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// Not implemented
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public override void Configure(string command, dynamic value)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public override void Configure(string command, string value)
        {
            throw new System.NotImplementedException();
        }
    }

}
