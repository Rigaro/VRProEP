//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

/// <summary>
/// Interface for components that have adaptable parameters. These are mostly continuously changing.
/// For example: a personalizable prosthesis interface.
/// </summary>
public interface IAdaptable {

    /// <summary>
    /// Updates a given parameter or set of parameters to the given values.
    /// </summary>
    /// <param name="channel">The parameter number or channel to be updated.</param>
    /// <param name="parameter">The new parameter value.</param>
    void UpdateParameter(int channel, float parameter);

    /// <summary>
    /// Updates a given parameter or set of parameters to the given values.
    /// </summary>
    /// <param name="parameters">The new parameters values set to be updated.</param>
    void UpdateAllParameters(float[] parameters);

    /// <summary>
    /// Returns the given parameter value.
    /// </summary>
    /// <param name="channel">The parameter number or channel to be read.</param>
    /// <returns>The desired parameter value</returns>
    float GetParameter(int channel);

    /// <summary>
    /// Returns all the parameter values.
    /// </summary>
    /// <returns>The set of parameter values.</returns>
    float[] GetAllParameters();
}
