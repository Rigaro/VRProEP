using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public class BoniManager : BasicFeedbackManager
    {
        /// <summary>
        /// Creates the manager for BONI, a bone conduction feedback system.
        /// </summary>
        /// <param name="bcController">The boni controller.</param>
        /// <param name="bcRefGen">The boni reference generator.</param>
        public BoniManager(IController bcController, IReferenceGenerator bcRefGen)
        {
            this.controller = bcController;
            this.referenceGenerator = bcRefGen;
        }

        /// <summary>
        /// Updates the feedback provided to the user for the given channel.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="sensorData">The sensor data from the device.</param>
        public override void UpdateFeedback(int channel, float[] sensorData)
        {
            if (!CheckInput(sensorData))
                throw new System.ArgumentOutOfRangeException("Two sensor data is required: force and roughness.");

            float[] reference = referenceGenerator.UpdateAllReferences(sensorData);
            controller.UpdateControlInput(reference, sensorData);
        }

        /// <summary>
        /// Updates the feedback provided to the user for at all channels.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="sensorData">The sensor data from the device.</param>
        public override void UpdateAllFeedback(float[] sensorData)
        {
            if (!CheckInput(sensorData))
                throw new System.ArgumentOutOfRangeException("Two sensor data is required: force and roughness. So the passed data should be a multiple of 2.");

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Checks whether the provided sensor input is of the right dimensions.
        /// </summary>
        /// <param name="input">The sensor data.</param>
        /// <returns>True if the input passes the check.</returns>
        private bool CheckInput(float[] input)
        {
            if (input.Length % 2 != 0)
                return false;
            else return true;
        }
    }
}
