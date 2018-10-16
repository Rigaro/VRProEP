using UnityEngine;

namespace VRProEP
{
    namespace ProsthesisCore
    {
        /// <summary>
        /// Virtual tracker that uses Unity's physics engine to obtain tracking data.
        /// Requires reference to a Rigidbody object.
        /// </summary>
        public class VirtualTrackerManager : SensorManager
        {

            public enum VirtualTrackerChannels
            {
                X_Vel,
                Y_Vel,
                Z_Vel,
                X_Pos,
                Y_Pos,
                Z_Pos
            }

            [SerializeField]
            private Rigidbody virtualTracker; // Virtual tracker uses a Unity physics object to obtain tracking data.
            public Rigidbody VirtualTracker
            {
                get
                {
                    return virtualTracker;
                }

                set
                {
                    virtualTracker = value;
                }
            }

            /// <summary>
            /// Virtual tracker that uses Unity's physics engine to obtain tracking data.
            /// Reference to a Rigidbody object provided by serialized data.
            /// </summary>
            public VirtualTrackerManager() : base(6, SensorType.VirtualTracker)
            {

            }

            /// <summary>
            /// Virtual tracker that uses Unity's physics engine to obtain tracking data.
            /// Reference to Rigidbody object defined in constructor.
            /// </summary>
            /// <param name="virtualTracker">The Rigidbody used for tracking.</param>
            public VirtualTrackerManager(Rigidbody virtualTracker) : base(6, SensorType.VirtualTracker)
            {
                this.VirtualTracker = virtualTracker;
            }

            /// <summary>
            /// Returns raw tracking information for the selected channel.
            /// See VirtualTrackerChannels for channel information. 
            /// Angular velocity given radians per second.
            /// Angular displacement given in Euler angles.
            /// </summary>
            /// <param name="channel">The channel number.</param>
            /// <returns>Raw tracking data for the given channel.</returns>
            public override float GetRawData(int channel)
            {
                if (channel > ChannelSize)
                    throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channles.");
                else if (channel <= 0)
                    throw new System.ArgumentOutOfRangeException("The channel range is 1-6.");

                // Angular velocity requested
                if (channel <=3)
                {
                    Vector3 angVel = virtualTracker.angularVelocity;
                    return angVel[channel];
                }
                // Angular position requested
                else if(channel > 3)
                {
                    int chan = channel - 3;
                    Vector3 angPos = virtualTracker.transform.localEulerAngles;
                    return angPos[chan];
                }
                else
                    throw new System.ArgumentOutOfRangeException("The channel range is 1-6.");

            }

            /// <summary>
            /// Returns raw tracking information for the selected channel identifier.
            /// See VirtualTrackerChannels for channel information.
            /// Angular velocity given radians per second.
            /// Angular displacement given in Euler angles.
            /// </summary>
            /// <param name="channel">The channel/data identifier.</param>
            /// <returns>Raw tracking data for the given channel.</returns>
            public override float GetRawData(string channel)
            {
                int channelNum = (int)System.Enum.Parse(typeof(VirtualTrackerChannels), channel);
                
                // Angular velocity requested
                if (channelNum <= 3)
                {
                    Vector3 angVel = virtualTracker.angularVelocity;
                    return angVel[channelNum];
                }
                // Angular position requested
                else if (channelNum > 3)
                {
                    int chan = channelNum - 3;
                    Vector3 angPos = virtualTracker.transform.localEulerAngles;
                    return angPos[chan];
                }
                else
                    throw new System.ArgumentOutOfRangeException("The provided channel identifier was not found.");
            }

            /// <summary>
            /// Returns all raw tracking data in an array.
            /// Angular velocity given radians per second.
            /// Angular displacement given in Euler angles.
            /// </summary>
            /// <returns>The array with all raw tracking data.</returns>
            public override float[] GetAllRawData()
            {
                Vector3 angVel = virtualTracker.angularVelocity;
                Vector3 angPos = virtualTracker.transform.localEulerAngles;
                float[] data = { angVel.x, angVel.y, angVel.z, angPos.x, angPos.y, angPos.z };
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
}