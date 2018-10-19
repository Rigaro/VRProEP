//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============


namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Interface for sensor hardware the will provide input to the prosthesis system.
    /// Whether it is a sensor for human interface or prosthetic device sensor (e.g. touch)
    /// all input sensors ought to implement ISensor.
    /// Examples of sensors: tactile sensor on prosthetic hand, IMU, sEMG, Vive tracker.
    /// </summary>
    public interface ISensor
    {
        /// <summary>
        /// Returns raw sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        float GetRawData(int channel);

        /// <summary>
        /// Returns raw sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        float GetRawData(string channel);

        /// <summary>
        /// Returns all raw sensor data in an array.
        /// </summary>
        /// <returns>The array with all raw sensor data.</returns>
        float[] GetAllRawData();

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        float GetProcessedData(int channel);

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        float GetProcessedData(string channel);

        /// <summary>
        /// Returns all pre-processed sensor data in an array.
        /// </summary>
        /// <returns>The array with all pre=processed sensor data.</returns>
        float[] GetAllProcessedData();
    }
}
