using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Basic synergistic prosthesis reference generator.
    /// Provides position reference for prosthesis joints through a simple linear kinematic synergy.
    /// </summary>
    public class LinearKinematicSynergy : AdaptiveGenerator
    {

        /// <summary>
        /// Basic synergistic prosthesis reference generator.
        /// Provides position reference for prosthesis joints through a simple linear kinematic synergy.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        public LinearKinematicSynergy(float[] xBar)
        {
            channelSize = xBar.Length;
            this.xBar = xBar;
            generatorType = ReferenceGeneratorType.LinearKinematicSynergy;
        }

        /// <summary>
        /// Basic synergistic prosthesis reference generator.
        /// Provides position reference for prosthesis joints through a simple linear kinematic synergy.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        public LinearKinematicSynergy(float[] xBar, float[] xMin, float[] xMax)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            generatorType = ReferenceGeneratorType.LinearKinematicSynergy;
        }

        /// <summary>
        /// Computes the reference for a given channel with given input according to a linear kinematic synergy.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The angular velocity input to use to update the reference.</param>
        /// <returns>The updated reference.</returns>
        public override float UpdateReference(int channel, float[] input)
        {
            // Check validity of the provided channel
            if (channel > channelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels.");
            else if (channel <= 0)
                throw new System.ArgumentOutOfRangeException("The channel number should be greater than 0.");

            xBar[channel - 1] = SingleDOFLinearSynergy(channel, input[channel - 1]);
            return xBar[channel - 1];
        }

        /// <summary>
        /// Updates all the references to be tracked by multiple controllers or devices.
        /// Computes the reference for all channels with given input according to a linear kinematic synergy.
        /// Should only be called within Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="input">The angular velocity input to use to update the references.</param>
        /// <returns>The updated set of references.</returns>
        public override float[] UpdateAllReferences(float[] input)
        {
            // Check validity of provided input
            if (!IsInputValid(input))
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match the number of reference channels.");

            for (int i = 1; i <= channelSize; i++)
            {
                UpdateReference(i, input);
            }
            return xBar;
        }

        /// <summary>
        /// Computes the reference for a given channel with given input according to a single degree of freedom linear synergy.
        /// </summary>
        /// <param name="input">The angular velocity input to the synergy in radians/s.</param>
        /// <param name="channel">The reference channel.</param>
        /// <returns>The updated position reference for the given channel in radians.</returns>
        private float SingleDOFLinearSynergy(int channel, float input)
        {
            // Calculate reference from 1D synergy.
            float tempXBar = xBar[channel - 1] + GetParameter(channel) * input * Time.fixedDeltaTime;
            // Saturate reference
            if (tempXBar > xMax[channel - 1])
                tempXBar = xMax[channel - 1];
            else if (tempXBar < xMin[channel - 1])
                tempXBar = xMin[channel - 1];

            return tempXBar;
        }
    }

}
