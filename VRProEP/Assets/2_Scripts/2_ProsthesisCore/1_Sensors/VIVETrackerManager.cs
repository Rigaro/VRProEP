using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Virtual tracker that uses Unity's physics engine to obtain tracking data.
    /// Requires reference to a Rigidbody object.
    /// </summary>
    public class VIVETrackerManager : MonoBehaviour, ISensor
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
        private int channelSize = 6;
        private SensorType sensorType = SensorType.VIVETracker;
        public int trackerNumber = 1; // Sets tracker number for when there are multiple ones.

        // Transform to get position information
        private Transform trackerTransform;

        // Unity XR nodes for accessing VIVETracker data.
        private List<XRNodeState> xrNodes = new List<XRNodeState>();
        private XRNodeState trackerState;
        
        void Start()
        {
            // Get transform to obtain positional data.
            trackerTransform = GetComponent<Transform>();
        }

        /*
         * Debug
         */

        void Update()
        {
            Debug.Log(GetRawData("Z_AngVel"));
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
        /// Returns raw tracking information for the selected channel.
        /// See VirtualTrackerChannels for channel information. 
        /// Angular velocity given radians per second.
        /// Angular displacement given in Euler angles.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw tracking data for the given channel.</returns>
        public float GetRawData(int channel)
        {
            if (channel > ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channles.");
            else if (channel <= 0)
                throw new System.ArgumentOutOfRangeException("The channel range is 1-6.");

            // Angular velocity requested
            if (channel <=3)
            {
                Vector3 angVel;
                TryGetTrackerAngularVelocity(out angVel);
                return angVel[channel - 1];
            }
            // Angular position requested
            else if(channel > 3)
            {
                int chan = channel - 4;
                Vector3 angPos = trackerTransform.localEulerAngles;
                return angPos[chan];
            }
            else
                throw new System.ArgumentOutOfRangeException("The channel range is 1-6.");

        }

        /// <summary>
        /// Returns raw tracking information for the selected channel identifier.
        /// See VIVETrackerChannels for channel information.
        /// Angular velocity given radians per second.
        /// Angular displacement given in Euler angles.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Raw tracking data for the given channel.</returns>
        public float GetRawData(string channel)
        {
            int channelNum = (int)System.Enum.Parse(typeof(VIVETrackerChannels), channel);

            return GetRawData(channelNum);
        }

        /// <summary>
        /// Returns all raw tracking data in an array.
        /// Angular velocity given radians per second.
        /// Angular displacement given in Euler angles.
        /// </summary>
        /// <returns>The array with all raw tracking data.</returns>
        public float[] GetAllRawData()
        {
            Vector3 angVel;
            TryGetTrackerAngularVelocity(out angVel);
            Vector3 angPos = trackerTransform.localEulerAngles;
            float[] data = { angVel.x, angVel.y, angVel.z, angPos.x, angPos.y, angPos.z };
            return data;
        }

        /// <summary>
        /// Not implemented, performs GetRawData.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public float GetProcessedData(int channel)
        {
            return GetRawData(channel);
        }

        /// <summary>
        /// Not implemented, performs GetRawData.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public float GetProcessedData(string channel)
        {
            return GetRawData(channel);
        }

        /// <summary>
        /// Not implemented, performs GetAllRawData.
        /// </summary>
        /// <returns>The array with all pre-processed sensor data.</returns>
        public float[] GetAllProcessedData()
        {
            return GetAllRawData();
        }

        // Encapsulation
        public int ChannelSize
        {
            get
            {
                return channelSize;
            }
        }
        public SensorType SensorType
        {
            get
            {
                return sensorType;
            }
        }
    }
}