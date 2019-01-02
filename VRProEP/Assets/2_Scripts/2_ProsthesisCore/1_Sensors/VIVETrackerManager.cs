using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Tracks a body part using a VIVE Tracker. Uses Unity's XR API to obtain tracking data.
    /// Requires reference to a the Tracker's object Transform.
    /// </summary>
    public class VIVETrackerManager : SensorManager
    {    
        // Local enumeration for channels
        public enum VIVETrackerChannels
        {
            X_AngVel,
            Y_AngVel,
            Z_AngVel,
            X_AngPos,
            Y_AngPos,
            Z_AngPos
        }

        // Configuration variables.
        private static int trackerNumber = 0; // Sets tracker number for when there are multiple ones.

        // Transform to get position information
        private Transform trackerTransform;

        // Unity XR nodes for accessing VIVETracker data.
        private List<XRNodeState> xrNodes = new List<XRNodeState>();
        private XRNodeState trackerState;

        /// <summary>
        /// Tracks a body part using a VIVE Tracker. Uses Unity's XR API to obtain tracking data.
        /// Requires reference to a the Tracker's object Transform.
        /// </summary>
        public VIVETrackerManager() : base (6, SensorType.VIVETracker)
        {
            trackerNumber++;
        }

        /// <summary>
        /// Tracks a body part using a VIVE Tracker. Uses Unity's XR API to obtain tracking data.
        /// Requires reference to a the Tracker's object Transform.
        /// </summary>
        /// <param name="trackerTransform">The Tracker's Transform to refer to for position data.</param>
        public VIVETrackerManager(Transform trackerTransform) : base(6, SensorType.VIVETracker)
        {
            SetTrackerTransform(trackerTransform);
            trackerNumber++;
        }

        public void SetTrackerTransform(Transform trackerTransform)
        {
            if (trackerTransform == null)
                throw new System.ArgumentNullException();
            this.trackerTransform = trackerTransform;
        }
        
        /// <summary>
        /// Uses Unity's XR API to extract angular velocity information from the tracker.
        /// </summary>
        /// <param name="localAngVel">The Vector3 refernece to store data.</param>
        /// <returns>True if sucessful.</returns>
        private bool TryGetTrackerAngularVelocity(out Vector3 localAngVel)
        {
            // Get node information
            InputTracking.GetNodeStates(xrNodes);
            // Look for Hardware trackers
            int currentTracker = 1;
            foreach (XRNodeState ns in xrNodes)
            {
                if (ns.nodeType == XRNode.HardwareTracker && currentTracker == trackerNumber)
                {
                    if (ns.TryGetAngularVelocity(out localAngVel))
                        return true;
                    else
                        return false;
                }
                else if (ns.nodeType == XRNode.HardwareTracker)
                    currentTracker++;
            }
            // If no tracker was found return error
            throw new System.Exception("No VIVE Tracker was found");
        }

        /// <summary>
        /// Uses Unity's XR API to extract angular position information from the tracker.
        /// </summary>
        /// <param name="localAngPos">The Vector3 refernece to store data.</param>
        /// <returns>True if sucessful.</returns>
        private bool TryGetTrackerPosition(out Quaternion localAngPos)
        {
            // Get node information
            InputTracking.GetNodeStates(xrNodes);
            // Look for Hardware trackers
            int currentTracker = 1;
            foreach (XRNodeState ns in xrNodes)
            {
                if (ns.nodeType == XRNode.HardwareTracker && currentTracker == trackerNumber)
                {
                    if (ns.TryGetRotation(out localAngPos))
                        return true;
                }
                else if (ns.nodeType == XRNode.HardwareTracker)
                    currentTracker++;
            }
            // If no tracker was found return error
            throw new System.Exception("No VIVE Tracker was found");
        }

        /// <summary>
        /// Returns raw tracking information for the selected channel.
        /// See VIVETrackerChannels for channel information. 
        /// Angular velocity given radians per second, world coordinates.
        /// Angular displacement given in Euler angles, world coordinates.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw tracking data for the given channel.</returns>
        public override float GetRawData(int channel)
        {
            if (channel >= ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels.");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel range is 0-5.");
            else if (trackerTransform == null)
                throw new System.Exception("The tracker transform has not been set.");

            // Angular velocity requested
            if (channel <=2)
            {
                Vector3 angVel;
                TryGetTrackerAngularVelocity(out angVel);
                return angVel[channel];
            }
            // Angular position requested
            else if(channel > 2)
            {
                int chan = channel - 3;
                Vector3 angPos = trackerTransform.eulerAngles;
                return angPos[chan];
            }
            else
                throw new System.ArgumentOutOfRangeException("The channel range is 1-6.");

        }

        /// <summary>
        /// Returns raw tracking information for the selected channel identifier.
        /// See VIVETrackerChannels for channel information.
        /// Angular velocity given radians per second, world coordinates.
        /// Angular displacement given in Euler angles, world coordinates.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Raw tracking data for the given channel.</returns>
        public override float GetRawData(string channel)
        {
            int channelNum = (int)System.Enum.Parse(typeof(VIVETrackerChannels), channel);

            return GetRawData(channelNum);
        }

        /// <summary>
        /// Returns all raw tracking data in an array.
        /// Angular velocity given radians per second, world coordinates.
        /// Angular displacement given in Euler angles, world coordinates.
        /// </summary>
        /// <returns>The array with all raw tracking data.</returns>
        public override float[] GetAllRawData()
        {
            if (trackerTransform == null)
                throw new System.Exception("The tracker transform has not been set.");

            Vector3 angVel;
            TryGetTrackerAngularVelocity(out angVel);
            Vector3 angPos = trackerTransform.eulerAngles;
            float[] data = { angVel.x, angVel.y, angVel.z, angPos.x, angPos.y, angPos.z };
            return data;
        }

        /// <summary>
        /// Returns processed tracking data in .
        /// Converts from world coordinates to local residual limb coordinates.
        /// Angular velocity given radians per second, world coordinates.
        /// Angular displacement given in Euler angles, world coordinates.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public override float GetProcessedData(int channel)
        {
            if (channel >= ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels.");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel range is 0-5.");
            else if (trackerTransform == null)
                throw new System.Exception("The tracker transform has not been set.");

            Vector3 angVel;
            TryGetTrackerAngularVelocity(out angVel);
            Vector3 localAngVel = trackerTransform.InverseTransformVector(angVel);
            if (channel == 0)
                return localAngVel.z;
            else if (channel == 1)
                return localAngVel.y;
            else if (channel == 2)
                return localAngVel.x;
            else if (channel == 3)
            {
                float offsetAngle = (-trackerTransform.localRotation.eulerAngles.x + 270.0f);
                if (offsetAngle > 180.0f)
                {
                    offsetAngle -= 360;
                }
                return Mathf.Deg2Rad * offsetAngle;
            }
            else if (channel == 4)
            {
                float offsetAngle = (-trackerTransform.localRotation.eulerAngles.y + 270.0f);
                if (offsetAngle > 180.0f)
                {
                    offsetAngle -= 360;
                }
                return Mathf.Deg2Rad * offsetAngle;
            }
            else
            {
                float offsetAngle = (-trackerTransform.localRotation.eulerAngles.z + 270.0f);
                if (offsetAngle > 180.0f)
                {
                    offsetAngle -= 360;
                }
                return Mathf.Deg2Rad * offsetAngle;
            }
        }

        /// <summary>
        /// Not implemented, performs GetRawData.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public override float GetProcessedData(string channel)
        {
            int channelNum = (int)System.Enum.Parse(typeof(VIVETrackerChannels), channel);

            return GetProcessedData(channelNum);
        }

        /// <summary>
        /// Not implemented, performs GetAllRawData.
        /// </summary>
        /// <returns>The array with all pre-processed sensor data.</returns>
        public override float[] GetAllProcessedData()
        {
            if (trackerTransform == null)
                throw new System.Exception("The tracker transform has not been set.");

            Vector3 angVel;
            TryGetTrackerAngularVelocity(out angVel);
            Vector3 localAngVel = trackerTransform.InverseTransformVector(angVel);
            float[] data = { localAngVel.z, localAngVel.y, localAngVel.x, GetProcessedData(3), GetProcessedData(4), GetProcessedData(5) };
            return data;
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