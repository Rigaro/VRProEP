using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRProEP.ProsthesisCore
{
    public abstract class BasicFeedbackManager : IFeedbackManager
    {
        protected IController controller;
        protected IReferenceGenerator referenceGenerator;

        /// <summary>
        /// Updates the feedback provided to the user for the given channel.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="sensorData">The sensor data from the device.</param>
        public abstract void UpdateFeedback(int channel, float[] sensorData);

        /// <summary>
        /// Updates the feedback provided to the user for at all channels.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="sensorData">The sensor data from the device.</param>
        public abstract void UpdateAllFeedback(float[] sensorData);
    }

}

