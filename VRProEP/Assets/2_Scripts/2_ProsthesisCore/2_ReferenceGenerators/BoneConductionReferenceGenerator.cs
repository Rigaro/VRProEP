//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System;
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
        private float[] frequency;

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
        public BoneConductionReferenceGenerator(float[] xBar, float[] gains, float[] offset, float[] frequency)
        {
            if (xBar.Length != gains.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.gains = gains;
            this.offset = offset;
            this.frequency = frequency;
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
        public BoneConductionReferenceGenerator(float[] xBar, float[] xMin, float[] xMax, float[] gains, float[] offset, float[] frequency)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length || xBar.Length != gains.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            this.gains = gains;
            this.offset = offset;
            this.frequency = frequency;
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
            this.frequency = bcCharac.frequency;
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
            float[] Int = CalculateFrequencyDependendParameters(xBar[0],1);
            xBar[1] = Int[0] + input[1] * Int[1];

            return xBar;
        }

        /// <summary>
        /// Calculates the frequency dependend gains and offset for the given frequency
        /// </summary>
        /// <param name="freq">Gives the frequency at which the gain and offset has to be calculated</param>
        /// <param name="startindex">Gives the index at which the gain and offset are to be interpolated</param>
        /// <returns>returns the obtained gain [0] and offset [1] </returns>
        private float[] CalculateFrequencyDependendParameters(float freq, int startindex)
        {
            float[] interpolate = new float[2];
         
           //interpolate gain
            interpolate[0] = LinearInterpolate(frequency,gains,freq, startindex);
            //interpolate offset
            interpolate[1] = LinearInterpolate(frequency, offset, freq, startindex);

            return interpolate;
        }

        /// <summary>
        /// Linear Interpolation to obtain the y Value within the given x and y Array
        /// </summary>
        /// <param name="xVal">Gives the x parameters</param>
        /// <param name="yVal">Gives the y parameters</param>
        /// <param name="t">Gives the x Value to be interpolated</param>
        /// <param name="startindex">Gives the index at which the gain and offset are to be interpolated</param>
        /// <returns>the y(t) is returned</returns>
        private float LinearInterpolate(float[] xVal, float[] yVal, float t, int startindex)
        {
            if(xVal.Length != yVal.Length)
                throw new System.ArgumentOutOfRangeException("The input array length of x and y should be equal");

            if (t < xVal[0] || t > xVal[xVal.Length-1])
                throw new System.ArgumentOutOfRangeException("No Extrapolation Method implemented in LinearInterpolation.");

            //find i for gap x[i-1]<= t <=x[i]
            int index = startindex;
            for(int i = startindex+1; i < (xVal.Length-1); i++)
            {
                if(xVal[i-1] <= t && t <= xVal[i])
                {
                    index = i-1;
                    break;
                }
            }
            //interpolate
            float linInt = yVal[index] + (t - xVal[index]) * ((yVal[index + 1] - yVal[index]) / (xVal[index + 1] - xVal[index]));

            return linInt;
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
