using UnityEngine;
using System.Collections;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Sensor manager for Oculus sensors. Uses OculusVR SDK to obtain data from object. Only tested with Oculus Touch so far.
    /// </summary>
    public class OculusSensorManager : SensorManager
    {
        private OVRInput.Controller controller;

        public enum OculusTouchChannels
        {
            X_Vel,
            Y_Vel,
            Z_Vel
        }

        public OculusSensorManager(OVRInput.Controller controller, SensorType sensorType) : base(3, sensorType)
        {

            // Check that the given controller corresponds to a valid OTouch controller type.
            if (sensorType == SensorType.OculusSensor)
            {
                if (!(controller.Equals(OVRInput.Controller.LTouch) || controller.Equals(OVRInput.Controller.RTouch)))
                    throw new System.PlatformNotSupportedException("Only LTouch and RTouch supported.");
            }
            // Other Oculus controllers are currently unsupported.
            else
            {
                throw new System.PlatformNotSupportedException("Only Oculus Touch controllers supported.");
            }

            this.controller = controller;
        }

        /// <summary>
        /// Returns raw angular velocity for the selected channel, where:
        /// Channel 1: x.
        /// Channel 2: y.
        /// Channel 3: z.
        /// See Oculus documentation to determine orientation of x, y, z.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        public override float GetRawData(int channel)
        {
            if (channel > ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channles.");
            else if (channel <= 0)
                throw new System.ArgumentOutOfRangeException("The channel range is 1-3.");

            // Get the 3-axis angular velocity
            Vector3 angVel = OVRInput.GetLocalControllerAngularVelocity(controller);

            // Select channel to return.
            return angVel[channel-1];
        }

        /// <summary>
        /// Returns raw angular velocity for the selected channel identifier, according to OculusTouchChannels enum.
        /// See Oculus documentation to determine orientation of x, y, z.
        /// </summary>
        /// <param name="channel">The channel identifier.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        public override float GetRawData(string channel)
        {
            // Get the 3-axis angular velocity
            Vector3 angVel = OVRInput.GetLocalControllerAngularVelocity(controller);

            // Select channel to return.
            if (channel.Equals(OculusTouchChannels.X_Vel.ToString()))
                return angVel.x;
            else if (channel.Equals(OculusTouchChannels.Y_Vel.ToString()))
                return angVel.y;
            else if (channel.Equals(OculusTouchChannels.Z_Vel.ToString()))
                return angVel.z;
            else
                throw new System.ArgumentOutOfRangeException("The provided channel identifier is invalid, see OCULUS_TOUCH_CHANNELS enum.");
        }

        /// <summary>
        /// Returns all raw sensor data in an array.
        /// </summary>
        /// <returns>The array with all raw sensor data.</returns>
        public override float[] GetAllRawData()
        {
            // Get the 3-axis angular velocity and return it in an array.
            Vector3 angVel = OVRInput.GetLocalControllerAngularVelocity(controller);
            float[] data = { angVel.x, angVel.y, angVel.z };
            return data;
        }

        /// <summary>
        /// Not implemented, performs GetRawData.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public override float GetProcessedData(int channel)
        {
            return GetRawData(channel);
        }

        /// <summary>
        /// Not implemented, performs GetRawData.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public override float GetProcessedData(string channel)
        {
            return GetRawData(channel);
        }

        /// <summary>
        /// Not implemented, performs GetAllRawData.
        /// </summary>
        /// <returns>The array with all pre-processed sensor data.</returns>
        public override float[] GetAllProcessedData()
        {
            return GetAllRawData();
        }

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// Not implemented
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public override void Configure(string command, float value)
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