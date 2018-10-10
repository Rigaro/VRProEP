//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

/// <summary>
/// Interface for managing extrenal communications.
/// Examples of external communication: bluetooth, serial/Arduino.
/// </summary>
/// <typeparam name="T">The type of the data to be communicated.</typeparam>
public interface ICommunicationsManager
{
    /// <summary>
    /// Transfers data of type "T" through the selected device.
    /// </summary>
    /// <param name="device">The device to use for data transmission.</param>
    /// <param name="data">The data to be transmitted.</param>
    /// <returns></returns>
    bool TransferData(string device, dynamic data);

    /// <summary>
    /// Receives requested data of type "T" through the selected device.
    /// </summary>
    /// <param name="device">The device to use for data transmission.</param>
    /// <param name="command">The command to request data from device.</param>
    /// <returns>The requested data</returns>
    dynamic ReceiveData(string device, string command);
}
