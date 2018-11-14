//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Input manager that includes a single sensor and reference generator.
    /// </summary>
    public abstract class BasicInputManager : IInputManager
    {
        protected ISensor sensorManager;
        protected IReferenceGenerator referenceGenerator;

        /// <summary>
        /// Generates an updated reference for the given device channel.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>The updated reference.</returns>
        public abstract float GenerateReference(int channel);

        /// <summary>
        /// Generates the set of references needed to update all devices.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <returns>The updated set of references.</returns>
        public abstract float[] GenerateAllReferences();
    }

}
