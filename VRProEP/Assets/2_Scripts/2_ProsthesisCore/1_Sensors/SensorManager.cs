//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Abstract class for the implementation of sensor managers such as Oculus controllers, Vive controllers, IMUs, etc.
    /// Includes an enumerator with all the sensor types currently implemented.
    /// </summary>
    public abstract class SensorManager : ISensor, IConfigurable
    {
        private int channelSize;
        private SensorType sensorType;

        /// <summary>
        /// Abstract class for the implementation of sensor managers such as Oculus controllers, Vive controllers, IMUs, etc.
        /// </summary>
        /// <param name="channelSize">The number of channels in the sensor.</param>
        /// <param name="sensorType">The type of sensor.</param>
        public SensorManager(int channelSize, SensorType sensorType)
        {
            if (channelSize <= 0)
                throw new System.ArgumentException("The given channel size is invalid. It should be greater than zero.");
            this.channelSize = channelSize;
            this.sensorType = sensorType;
        }

        /// <summary>
        /// Returns raw sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        public abstract float GetRawData(int channel);

        /// <summary>
        /// Returns raw sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        public abstract float GetRawData(string channel);

        /// <summary>
        /// Returns all raw sensor data in an array.
        /// </summary>
        /// <returns>The array with all raw sensor data.</returns>
        public abstract float[] GetAllRawData();

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public abstract float GetProcessedData(int channel);

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public abstract float GetProcessedData(string channel);

        /// <summary>
        /// Returns all pre-processed sensor data in an array.
        /// </summary>
        /// <returns>The array with all pre-processed sensor data.</returns>
        public abstract float[] GetAllProcessedData();

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public abstract void Configure(string command, dynamic value);

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public abstract void Configure(string command, string value);


        // Encapsulation
        public int ChannelSize
        {
            get
            {
                return channelSize;
            }
        }

        public SensorType GetSensorType()
        {
                return sensorType;
        }
    }

}
