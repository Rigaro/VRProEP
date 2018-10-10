//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

/// <summary>
/// Interface for Reference Generators that will be used to provide a reference to a controller or device.
/// This includes both human interface based (HMI) based or autonomous.
/// Examples of Reference Generators: kinematic synergy, differential proportional, feedback generator.
/// </summary>
public interface IReferenceGenerator
{
    /// <summary>
    /// Updates the reference for the given channel to be tracked by a controller or device.
    /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
    /// </summary>
    /// <param name="channel">The channel number.</param>
    /// <param name="input">The input to use to update the reference.</param>
    /// <returns>The updated reference.</returns>
    float UpdateReference(int channel, float input);

    /// <summary>
    /// Updates all the references to be tracked by multiple controllers or devices.
    /// Should only be called within Monobehaviour : FixedUpdate.
    /// </summary>
    /// <param name="input">The input to use to update the references.</param>
    /// <returns>The updated set of references.</returns>
    float[] UpdateReference(float[] input);

    /// <summary>
    /// Returns the current reference value for the provided channel.
    /// </summary>
    /// <param name="channel">The channel number.</param>
    /// <returns>The current reference value.</returns>
    float GetReference(int channel);

    /// <summary>
    /// Returns the current references.
    /// </summary>
    /// <returns>The current set of references.</returns>
    float[] GetReference();

    /// <summary>
    /// Forces a value into the reference. It is recommended that it is used only for re-setting/intializing a reference.
    /// </summary>
    /// <param name="channel">The channel number.</param>
    /// <param name="value">The value to set the reference to.</param>
    void SetReference(int channel, float value);

    /// <summary>
    /// Forces a set of values into the references. It is recommended that it is used only for re-setting/intializing a reference.
    /// </summary>
    /// <param name="refs">The set of values to set the references to.</param>
    void SetReference(float[] refs);
}
