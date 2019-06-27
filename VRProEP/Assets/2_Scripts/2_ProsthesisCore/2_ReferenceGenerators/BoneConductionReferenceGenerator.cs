//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;
using UnityEngine;
using VRProEP.GameEngineCore;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Autonomous reference generator that minimizes the distance to a given point in task space.
    /// </summary>
    public class BoneConductionReferenceGenerator : ReferenceGenerator
    {

        private float[] gains;
        private float[] offset;

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
        public BoneConductionReferenceGenerator(float[] xBar, float[] gains, float[] offset)
        {
            if (xBar.Length != gains.Length)
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
        /// <param name="gains">The desired integrator gain for each reference.</param>
        public BoneConductionReferenceGenerator(float[] xBar, float[] xMin, float[] xMax, float[] gains, float[] offset)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length || xBar.Length != gains.Length)
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
        /// 
        /// </summary>
        /// <param name="xBar"></param>
        /// <param name="bcCharac"></param>
        public BoneConductionReferenceGenerator(float[] xBar, BoneConductionCharacterization bcCharac)
        {
            this.xBar = xBar;
            channelSize = xBar.Length;
            ExtractParameters(bcCharac);
            generatorType = ReferenceGeneratorType.BoneConduction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcCharac"></param>
        private void ExtractParameters(BoneConductionCharacterization bcCharac)
        {
            // Extract all parameters from characterisation file and load into class.
            this.xMin = bcCharac.xMin;
            this.xMax = bcCharac.xMax;
            this.gains = bcCharac.gains;
            this.offset = bcCharac.offset;
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

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

            //roughness to stimulation frequency
            xBar[0] = offset[0] + input[0] * gains[0];
            //force to amplitude 
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
                gains[i] = 0.0f;
            }
        }

        /// <summary>
        /// Sets the offset to the default 0.0f.
        /// </summary>
        private void SetDefaultOffset()
        {
            for (int i = 0; i < xBar.Length; i++)
            {
                offset[i] = 0.0f;
            }
        }
    }
}
