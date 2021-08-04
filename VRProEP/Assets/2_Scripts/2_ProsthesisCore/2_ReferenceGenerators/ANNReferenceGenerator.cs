using System;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.AdaptationCore;
namespace VRProEP.ProsthesisCore
{
    public class ANNReferenceGenerator : ReferenceGenerator
    {
        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The input to use to update the reference.</param>
        /// <returns>The updated reference.</returns>
        public override float UpdateReference(int channel, float[] input)
        {
            return input[0];
        }

        /// <summary>
        /// Updates all the references to be tracked by multiple controllers or devices.
        /// Should only be called within Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="input">The input to use to update the references.</param>
        /// <returns>The updated set of references.</returns>
        public override float[] UpdateAllReferences(float[] input)
        {
            return input;
        }
    }
}
