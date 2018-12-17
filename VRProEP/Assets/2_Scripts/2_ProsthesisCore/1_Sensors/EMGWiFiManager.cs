﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{

    public class EMGWiFiManager : WiFiSensorManager
    {
        private List<float> gains = new List<float>();
        private List<float> lowerLimits = new List<float>();

        public EMGWiFiManager(string ipAddress, int port, int channelSize) : base(ipAddress, port, channelSize, SensorType.EMGWiFi, UDPType.UDP_Async)
        {
            // Set the default gains
            for (int i = 0; i < channelSize; i++)
            {
                gains.Add(100.0f / 1023.0f);
                lowerLimits.Add(100.0f);
            }
        }

        /// <summary>
        /// Sets the gain for the given channel number and maximum sensor value achieved by the subject.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="upperLimit">The maximum raw sensor value achieved by the subject.</param>
        /// <param name="lowerLimit">The minimum raw sensor value achieved by the subject.</param>
        public void ConfigureLimits(int channel, int upperLimit, int lowerLimit)
        {
            if (channel > ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels. The number of available channels is: " + ChannelSize + ".");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel number starts from 0.");            


            if (upperLimit <= 0 || upperLimit > 1023 || lowerLimit < 0 || lowerLimit >= 1023 || lowerLimit == upperLimit)
                throw new System.ArgumentOutOfRangeException("The limits should be between 0-1023 and cannot be equal.");

            gains[channel] = 100.0f/(upperLimit - lowerLimit);
            lowerLimits[channel] = lowerLimit;
        }

        /// <summary>
        /// Returns EMG amplitude data for the selected channel in a 0-1023 range.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>EMG amplitude data for the given channel.</returns>
        public override float GetRawData(int channel)
        {
            if (channel > ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels. The number of available channels is: " + ChannelSize + ".");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel number starts from 0.");

            return GetAllRawData()[channel];
        }

        /// <summary>
        /// Returns EMG amplitude data for the selected channel in a 0-1023 range.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>EMG amplitude data for the given channel.</returns>
        public override float GetRawData(string channel)
        {
            throw new System.NotImplementedException("Not implemented, use int version.");
        }

        /// <summary>
        /// Returns EMG amplitude data for all channels in a 0-1023 range.
        /// </summary>
        /// <returns>The array with all EMG amplitude data.</returns>
        public override float[] GetAllRawData()
        {
            // Get current sensor data from memory.
            return GetCurrentSensorValues();
        }

        /// <summary>
        /// Returns EMG amplitude data for the selected channel in a 0-100 range.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>EMG amplitude data for the given channel.</returns>
        public override float GetProcessedData(int channel)
        {
            if (channel > ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels. The number of available channels is: " + ChannelSize + ".");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel number starts from 0.");

            return GetAllProcessedData()[channel];
        }

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public override float GetProcessedData(string channel)
        {
            throw new System.NotImplementedException("Not implemented, use int version.");
        }

        /// <summary>
        /// Returns EMG amplitude data for all channels in a 0-100 range.
        /// </summary>
        /// <returns>The array with all EMG amplitude data.</returns>
        public override float[] GetAllProcessedData()
        {
            // Get current sensor data from memory.
            float[] sensorValues = GetCurrentSensorValues();
            // Transform to 0-100 value.
            for (int i = 0; i < sensorValues.Length; i++)
            {
                sensorValues[i] = gains[i] * (sensorValues[i] - lowerLimits[i]);
            }
            return sensorValues;
        }

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public override void Configure(string command, dynamic value)
        {

        }

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public override void Configure(string command, string value)
        {

        }
    }

}