using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public class BoniManager : BasicFeedbackManager
    {
        /// <summary>
        /// Updates the feedback provided to the user for the given channel.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="sensorData">The sensor data from the device.</param>
        public override void UpdateFeedback(int channel, float[] sensorData)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the feedback provided to the user for at all channels.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="sensorData">The sensor data from the device.</param>
        public override void UpdateAllFeedback(float[] sensorData)
        {
            throw new System.NotImplementedException();
        }
    }
}
