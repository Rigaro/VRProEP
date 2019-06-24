using System;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.AdaptationCore;
using VRProEP.Utilities;

namespace VRProEP.ProsthesisCore
{

    public class EMGWiFiManager : UDPSensorManager, ISensor
    {
        private List<float> gains = new List<float>();
        private List<float> lowerLimits = new List<float>();
        private List<FOLPDFilter> lowPassFilters = new List<FOLPDFilter>();
        private List<MovingAverageFilter> movingAverageFilters = new List<MovingAverageFilter>();
        private bool isRaw = false;
        private SensorType sensorType;

        public EMGWiFiManager(string ipAddress, int port, int channelSize, bool isRaw = false) : base(ipAddress, port, channelSize, "EMGWiFi", UDPType.UDP_Async)
        {
            // Set the default gains and initialize filters
            for (int i = 0; i < channelSize; i++)
            {
                gains.Add(100.0f / 1023.0f);
                lowerLimits.Add(100.0f);
                lowPassFilters.Add( new FOLPDFilter(2*Mathf.PI*3.0f, 1.17f, Time.fixedDeltaTime) );
                movingAverageFilters.Add(new MovingAverageFilter(15));
            }

            this.isRaw = isRaw;
        }

        /// <summary>
        /// Sets the gain for the given channel number and maximum sensor value achieved by the subject.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="upperLimit">The maximum raw sensor value achieved by the subject.</param>
        /// <param name="lowerLimit">The minimum raw sensor value achieved by the subject.</param>
        public void ConfigureLimits(int channel, float upperLimit, float lowerLimit)
        {
            if (channel >= ChannelSize)
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
        public float GetRawData(int channel)
        {
            if (channel >= ChannelSize)
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
        public float GetRawData(string channel)
        {
            throw new System.NotImplementedException("Not implemented, use int version.");
        }

        /// <summary>
        /// Returns EMG amplitude data for all channels in a 0-1023 range.
        /// </summary>
        /// <returns>The array with all EMG amplitude data.</returns>
        public float[] GetAllRawData()
        {
            // Get current sensor data from memory.
            float[] sensorValues = GetCurrentSensorValues();
            if (isRaw)
            {
                float[] filteredValues = sensorValues;
                float[] averagedValues = sensorValues;
                // Transform to 0-100 value.
                for (int i = 0; i < sensorValues.Length; i++)
                {
                    // Shift and rectify
                    //float shiftRect = sensorValues[i] - 512.0f;
                    float shiftRect = Mathf.Abs(sensorValues[i] - 512.0f);
                    filteredValues[i] = lowPassFilters[i].Update(shiftRect);
                    averagedValues[i] = (float)System.Math.Round(movingAverageFilters[i].Update(filteredValues[i]), 1);
                }
                return averagedValues;
            }
            else
            {
                // Get current sensor data from memory.
                return sensorValues;
            }
        }

        /// <summary>
        /// Returns EMG amplitude data for the selected channel in a 0-100 range.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>EMG amplitude data for the given channel.</returns>
        public float GetProcessedData(int channel)
        {
            if (channel >= ChannelSize)
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
        public float GetProcessedData(string channel)
        {
            throw new System.NotImplementedException("Not implemented, use int version.");
        }

        /// <summary>
        /// Returns EMG amplitude data for all channels in a 0-100 range.
        /// </summary>
        /// <returns>The array with all EMG amplitude data.</returns>
        public float[] GetAllProcessedData()
        {
            // Get current sensor data from memory.
            float[] sensorValues = GetCurrentSensorValues();

            if (isRaw)
            {
                float[] filteredValues = sensorValues;
                float[] averagedValues = sensorValues;
                float[] normalizedValues = sensorValues;
                // Transform to 0-100 value.
                for (int i = 0; i < sensorValues.Length; i++)
                {
                    // Shift and rectify
                    float shiftRect = Mathf.Abs(sensorValues[i] - 512.0f);
                    filteredValues[i] = lowPassFilters[i].Update(shiftRect);
                    averagedValues[i] = (float)System.Math.Round(movingAverageFilters[i].Update(filteredValues[i]), 1);
                    normalizedValues[i] = gains[i] * (averagedValues[i] - lowerLimits[i]);
                    if (normalizedValues[i] > 100.0f)
                        normalizedValues[i] = 100.0f;
                    else if (normalizedValues[i] < 0.0f)
                        normalizedValues[i] = 0.0f;
                }
                return normalizedValues;
            }
            else
            {
                float[] normalizedValues = sensorValues;
                // Transform to 0-100 value.
                for (int i = 0; i < sensorValues.Length; i++)
                {
                    // Shift and rectify
                    normalizedValues[i] = (float)System.Math.Round(gains[i] * (sensorValues[i] - lowerLimits[i]), 1);
                    if (normalizedValues[i] > 100.0f)
                        normalizedValues[i] = 100.0f;
                    if (normalizedValues[i] < 0.0f)
                        normalizedValues[i] = 0.0f;
                }
                return sensorValues;
            }
        }


        public SensorType GetSensorType()
        {
            return sensorType;
        }
    }

}
