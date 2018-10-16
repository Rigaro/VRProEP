//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

/// <summary>
/// Interface for managers that handle output to feedback systems.
/// Examples of feedback systems: bone conduction, visual.
/// </summary>
public interface IFeedbackManager
{
    /// <summary>
    /// Updates the feedback provided to the user for the given channel.
    /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
    /// </summary>
    /// <param name="channel">The channel number.</param>
    /// <param name="sensorData">The sensor data from the device.</param>
    void UpdateFeedback(int channel, float[] sensorData);

    /// <summary>
    /// Updates the feedback provided to the user for at all channels.
    /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
    /// </summary>
    /// <param name="sensorData">The sensor data from the device.</param>
    void UpdateAllFeedback(float[] sensorData);
}
