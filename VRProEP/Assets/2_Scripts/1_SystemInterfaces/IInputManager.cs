//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Interface for managers that handle input from users and provide references to devices.
    /// Examples of input systems: sEMG interface, kinematic synergy interface.
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Generates an updated reference for the given device channel.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>The updated reference.</returns>
        float GenerateReference(int channel);

        /// <summary>
        /// Generates the set of references needed to update all devices.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <returns>The updated set of references.</returns>
        float[] GenerateAllReferences();
    }
}
