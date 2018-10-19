//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Interface for managers that handle interaction with output devices.
    /// Examples of devices: prosthetic elbow, hand.
    /// </summary>
    public interface IDeviceManager
    {
        /// <summary>
        /// Updates the state of the device for the given channel.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="reference">The reference for the device to track.</param>
        void UpdateState(int channel, float reference);

        /// <summary>
        /// Updates all the states of the device. Useful for multi DOF devices.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="references">The set of references for the device to track.</param>
        void UpdateAllStates(float[] references);
    }
}
