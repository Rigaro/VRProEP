//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Autonomous reference generator that minimizes the distance to a given point in task space.
    /// </summary>
    public class BoneConductionReferenceGenerator : ReferenceGenerator
    {

        private List<float> gains;
        private List<float> offset;

        /// <summary>
        /// Autonomous reference generator that gives a linear relationship of input to output
        /// Sets all gains to the default: 0.0.
        /// Sets all offsets to the default: 0.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the reference.</param>
        public BoneConductionReferenceGenerator(float[] xBar)
        {
            if (xBar.Length != 1)
                throw new System.ArgumentOutOfRangeException("The reference initial condition should be provided for a signle DOF.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            SetDefaultGains();
            SetDefaultOffset();
            generatorType = ReferenceGeneratorType.BoneConduction;
        }

        /// <summary>
        /// Autonomous reference generator that gives a linear relationship of input to output
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="gains">The desired gain for each reference.</param>
        /// <param name="ofsset">The desired offset for each reference.</param>
        public BoneConductionReferenceGenerator(float[] xBar, List<float> gains, List<float> offset)
        {
            if (xBar.Length != gains.Count)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.gains = gains;
            this.offset = offset;
            generatorType = ReferenceGeneratorType.BoneConduction;
        }

        /// <summary>
        /// Autonomous reference generator that gives a linear relationship of input to output
        /// Sets all gains to the default: 0.0.
        /// Sets all offsets to the default: 0.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        public BoneConductionReferenceGenerator(float[] xBar, float[] xMin, float[] xMax)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            SetDefaultGains();
            SetDefaultOffset();
            generatorType = ReferenceGeneratorType.BoneConduction;
        }

        /// <summary>
        /// Autonomous reference generator that gives a linear relationship of input to output
        /// Sets all gains to the default: 0.0.
        /// Sets all offsets to the default: 0.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        /// <param name="gains">The desired integrator gain for each reference.</param>
        public BoneConductionReferenceGenerator(float[] xBar, float[] xMin, float[] xMax, List<float> gains, List<float> offset)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length || xBar.Length != gains.Count)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            this.gains = gains;
            this.offset = offset;
            generatorType = ReferenceGeneratorType.BoneConduction;
        }

        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The input to use to update the reference.</param>
        /// <returns>The updated reference.</returns>
        public override float UpdateReference(int channel, float[] input)
        {
            throw new System.NotImplementedException("The UpdateReference() methode has not been implemented for BoneConductionReferenceGn.");
        }

        /// <summary>
        /// Updates all the references of Boneconduction Feedback (Amplitude and Frequency)
        /// </summary>
        /// <param name="input">The input is composed of two terms: force input[0] and surfaceroughness input[1].</param>
        /// <returns>The updated set of references.</returns>
        public override float[] UpdateAllReferences(float[] input)
        {
            // Check validity of the provided input
            if (input.Length != 2)
                throw new System.ArgumentOutOfRangeException("The input array length should be = 2.");

            //force to stimulation amplitude
            xBar[0] = offset[0] + input[0] * gains[0];
            //surfaceroughness to frequency 
            xBar[1] = offset[1] + input[1] * gains[1];

            return xBar;
        }

        /// <summary>
        /// Sets the gains to the default 0.0f.
        /// </summary>
        private void SetDefaultGains()
        {
            for (int i = 0; i < xBar.Length; i++)
            {
                gains.Add(0.0f);
            }
        }

        /// <summary>
        /// Sets the offset to the default 0.0f.
        /// </summary>
        private void SetDefaultOffset()
        {
            for (int i = 0; i < xBar.Length; i++)
            {
                offset.Add(0.0f);
            }
        }
    }
}
