//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
namespace VRProEP.Utilities
{
    /// <summary>
    /// Interface for managing extrenal communications.
    /// Examples of external communication: bluetooth, serial/Arduino.
    /// </summary>
    /// <typeparam name="T">The type of the data to be communicated.</typeparam>
    public interface ICommunicationsManager
    {
        /// <summary>
        /// Processes a set of data and sends it to the remote device.
        /// </summary>
        /// <param name="data">The data to be transmitted.</param>
        /// <returns>True if successful.</returns>
        void SendData(dynamic data);

        /// <summary>
        /// Receives requested data (dynamic type) for the given command.
        /// </summary>
        /// <param name="command">The command to request data from device.</param>
        /// <returns>The requested data</returns>
        dynamic GetData(string command);
    }
}
