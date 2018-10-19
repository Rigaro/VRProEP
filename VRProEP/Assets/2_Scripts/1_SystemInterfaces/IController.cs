//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Interface for controllers that provide low level control input to actuators or devices.
    /// Examples of controllers: PD control of a prosthetic elbow.
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Provides the control input command to an actuator/device.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// For example: a PD controller for a prosthesis joint.
        /// </summary>
        /// <param name="reference">The references for the controller to track.</param>
        /// <param name="feedback">The feedback provided by sensors.</param>
        /// <returns>The updated control input.</returns>
        float UpdateControlInput(float[] reference, float[] feedback);
    }
}
