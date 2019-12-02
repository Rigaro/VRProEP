using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.ProsthesisCore;

public class ThalmicMyobandManager : SensorManager
{
    private GameObject thalmicMyoGO;
    private ThalmicMyo thalmicMyoController;

    /// <summary>
    /// Creates the manager for a Thalmic-Labs Myoband.
    /// The Player must have been loaded already in order to instantiate the band controller.
    /// </summary>
    public ThalmicMyobandManager() : base(2, SensorType.ThalmicMyo)
    {
        // Add the prefab to the avatar object
        // Load Avatar object to set as parent.
        GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
        // Instantiate the Single myo Hub.
        GameObject thalmicMyoSinglePrefab = Resources.Load<GameObject>("Sensors/ThalmicMyoSingle");
        thalmicMyoGO = GameObject.Instantiate(thalmicMyoSinglePrefab, avatarGO.transform);
        // Get its controller
        thalmicMyoController = thalmicMyoGO.GetComponentInChildren<ThalmicMyo>();
    }

    ~ThalmicMyobandManager()
    {
        GameObject.Destroy(thalmicMyoGO);
    }

    /// <summary>
    /// Returns raw sensor data for the selected channel.
    /// NOTE: ONLY POSE DATA IMPLEMENTED, SINGLE CHANNEL
    /// Pose:
    /// 0 - None
    /// 1 - Wrist flexion
    /// 2 - Wrist extension
    /// 3 - Fist
    /// 4 - Finger spread
    /// 5 - Double-tap
    /// </summary>
    /// <param name="channel">The channel number.</param>
    /// <returns>Raw sensor data for the given channel.</returns>
    public override float GetRawData(int channel)
    {
        if (channel >= ChannelSize)
            throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels. The number of available channels is: " + ChannelSize + ".");
        else if (channel < 0)
            throw new System.ArgumentOutOfRangeException("The channel number starts from 0.");

        if (thalmicMyoController.pose == Thalmic.Myo.Pose.WaveIn)
            return 1.0f;
        else if (thalmicMyoController.pose == Thalmic.Myo.Pose.WaveOut)
            return 2.0f;
        else if (thalmicMyoController.pose == Thalmic.Myo.Pose.Fist)
            return 3.0f;
        else if (thalmicMyoController.pose == Thalmic.Myo.Pose.FingersSpread)
            return 4.0f;
        else if (thalmicMyoController.pose == Thalmic.Myo.Pose.DoubleTap)
            return 5.0f;
        else
            return 0.0f;
    }

    /// <summary>
    /// Returns raw sensor data for the selected channel.
    /// </summary>
    /// <param name="channel">The channel/data identifier.</param>
    /// <returns>Raw sensor data for the given channel.</returns>
    public override float GetRawData(string channel)
    {
        throw new System.NotImplementedException("Not implemented, use int version.");
    }

    /// <summary>
    /// Returns all raw sensor data in an array.
    /// </summary>
    /// <returns>The array with all raw sensor data.</returns>
    public override float[] GetAllRawData()
    {
        float[] data = { GetRawData(0) };
        return data;
    }

    /// <summary>
    /// Returns pre-processed sensor data for the selected channel.
    /// Fist and wave in are mapped to EMG site full activation -> 100
    /// Finger spread and wave out are mapped to EMG opposing site full activation -> -100
    /// </summary>
    /// <param name="channel">The channel number.</param>
    /// <returns>Pre-processed sensor data for the given channel.</returns>
    public override float GetProcessedData(int channel)
    {
        if (channel >= ChannelSize)
            throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels. The number of available channels is: " + ChannelSize + ".");
        else if (channel < 0)
            throw new System.ArgumentOutOfRangeException("The channel number starts from 0.");

        if (channel == 0 && (thalmicMyoController.pose == Thalmic.Myo.Pose.WaveIn || thalmicMyoController.pose == Thalmic.Myo.Pose.Fist))
            return 60.0f;
        else if (channel == 1 && (thalmicMyoController.pose == Thalmic.Myo.Pose.WaveOut || thalmicMyoController.pose == Thalmic.Myo.Pose.FingersSpread))
            return 60.0f;
        else
            return 0.0f;
    }

    /// <summary>
    /// Returns pre-processed sensor data for the selected channel.
    /// </summary>
    /// <param name="channel">The channel/data identifier.</param>
    /// <returns>Pre-processed sensor data for the given channel.</returns>
    public override float GetProcessedData(string channel)
    {
        throw new System.NotImplementedException("Not implemented, single channel.");
    }

    /// <summary>
    /// Returns all pre-processed sensor data in an array.
    /// </summary>
    /// <returns>The array with all pre-processed sensor data.</returns>
    public override float[] GetAllProcessedData()
    {
        float[] data = { GetProcessedData(0), GetProcessedData(1) };
        return data;
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
