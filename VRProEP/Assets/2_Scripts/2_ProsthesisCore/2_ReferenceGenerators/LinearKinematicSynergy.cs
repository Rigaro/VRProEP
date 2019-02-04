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

        private bool isEnabled = false;
        private bool enableRequested = false;

        /// <summary>
        /// Basic synergistic prosthesis reference generator.
        /// Provides position reference for prosthesis joints through a simple linear kinematic synergy.
        /// </summary>
        /// <param name="xBar">The initial references.</param>
        /// <param name="xMin">The lower limit for the references.</param>
        /// <param name="xMax">The upper limit for the references.</param>
        /// <param name="theta">The initial parameters.</param>
        /// <param name="thetaMin">The lower limit for the parameters.</param>
        /// <param name="thetaMax">The upper limit for the parameters.</param>
        public LinearKinematicSynergy(float[] xBar, float[] xMin, float[] xMax, float[] theta, float[] thetaMin, float[] thetaMax) : base(xBar, xMin, xMax, theta, thetaMin, thetaMax, ReferenceGeneratorType.LinearKinematicSynergy)
        {

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
            if (channel >= channelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels.");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel number should be greater or equal to 0.");

            // Check validity of the provided input
            if (!IsInputValid(input))
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match the number of reference channels.");

            // Extract input
            float qDotShoulder = input[0];
            bool enable = false;
            if (input[1] >= 1.0f)
                enable = true;


            // Check if requested to enable the synergy
            if (enable && !enableRequested && !isEnabled)
            {
                // Requested to enable, get button down
                enableRequested = true;
                // Get new reference frame
                isEnabled = true;
            }
            else if (!enable && enableRequested) // Released button
            {
                enableRequested = false;
            }
            else if (enable && !enableRequested && isEnabled)
            {
                //Debug.Log("Synergy disabled.");
                // Requested to disable, get button down
                enableRequested = true;
                isEnabled = false;
            }

            // Only update when enabled, otherwise just use the same fixed reference.
            if (isEnabled)
            {
                xBar[channel] = SingleDOFLinearSynergy(channel, qDotShoulder);
                //Debug.Log(Mathf.Rad2Deg * xBar[channel]);
            }


            return xBar[channel];
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

            for (int i = 0; i < channelSize; i++)
            {
                float[] channelInput = { input[i], input[i + 1] };
                UpdateReference(i, channelInput);
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
            float tempXBar = xBar[channel] + GetParameter(channel) * input * Time.fixedDeltaTime;
            // Saturate reference
            if (tempXBar > xMax[channel])
                tempXBar = xMax[channel];
            else if (tempXBar < xMin[channel])
                tempXBar = xMin[channel];

            return tempXBar;
        }


        /// <summary>
        /// Checks the validity of the provided input.
        /// </summary>
        /// <param name="input">The input to be verified.</param>
        /// <returns>True if valid.</returns>
        private new bool IsInputValid(float[] input)
        {
            // Check validity of the provided channel
            if (input.Length != 2*xBar.Length)
                return false;
            else
                return true;
        }
    }

}
