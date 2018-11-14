//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Autonomous reference generator that minimizes the distance to a given point in task space.
    /// </summary>
    public class PointGradientGenerator : ReferenceGenerator
    {

        private List<float> gains;

        /// <summary>
        /// Autonomous reference generator that minimizes the distance to a given point in task space.
        /// Sets all gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the reference.</param>
        public PointGradientGenerator(float[] xBar)
        {
            if (xBar.Length != 1)
                throw new System.ArgumentOutOfRangeException("The reference initial condition should be provided for a signle DOF.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            SetDefaultGains();
            generatorType = ReferenceGeneratorType.PointGradient;
        }

        /// <summary>
        /// Autonomous reference generator that minimizes the distance to a given point in task space.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="gains">The desired integrator gain for each reference.</param>
        public PointGradientGenerator(float[] xBar, List<float> gains)
        {
            if (xBar.Length != gains.Count)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.gains = gains;
            generatorType = ReferenceGeneratorType.PointGradient;
        }

        /// <summary>
        /// Autonomous reference generator that minimizes the distance to a given point in task space.
        /// Sets all gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        public PointGradientGenerator(float[] xBar, float[] xMin, float[] xMax)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            SetDefaultGains();
            generatorType = ReferenceGeneratorType.PointGradient;
        }

        /// <summary>
        /// Autonomous reference generator that minimizes the distance to a given point in task space.
        /// Sets all gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        /// <param name="gains">The desired integrator gain for each reference.</param>
        public PointGradientGenerator(float[] xBar, float[] xMin, float[] xMax, List<float> gains)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length || xBar.Length != gains.Count)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            this.gains = gains;
            generatorType = ReferenceGeneratorType.PointGradient;
        }

        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// The input array length should be = 2*channelSize. Hand tracking error [2*channel - 2] and joint tracking error [2*channel - 1].
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The input to use to update the reference.</param>
        /// <returns>The updated reference.</returns>
        public override float UpdateReference(int channel, float[] input)
        {
            // Check validity of the provided channel
            if (!IsChannelValid(channel))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");

            // Check validity of the provided input
            if (input.Length != 2*channelSize)
                throw new System.ArgumentOutOfRangeException("The input array length should be = 2*channelSize. Hand tracking error [0] and joint tracking error [1].");

            float trackingError = input[2*channel - 2];
            float jointError = input[2*channel - 1];
            float sign = 1.0f;

            // When the joint is further from target than hand, move in negative direction
            if (jointError > trackingError)
                sign = -1.0f;
            else
                sign = 1.0f;

            float tempXBar = xBar[channel - 1] + gains[channel - 1] * sign * trackingError * Time.fixedDeltaTime;
            // Saturate reference
            if (tempXBar > xMax[channel - 1])
                tempXBar = xMax[channel - 1];
            else if (tempXBar < xMin[channel - 1])
                tempXBar = xMin[channel - 1];

            xBar[channel - 1] = (float)System.Math.Round(tempXBar, 2);
            // Return in array
            return xBar[channel - 1];

        }

        /// <summary>
        /// Updates all the references to be tracked by multiple controllers or devices.
        /// Should only be called within Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="input">The input is composed of two terms: hand tracking error [0] and joint tracking error [0].</param>
        /// <returns>The updated set of references.</returns>
        public override float[] UpdateAllReferences(float[] input)
        {
            for (int i = 1; i <= channelSize; i++)
            {
                UpdateReference(i, input);
            }
            return xBar;
        }

        /// <summary>
        /// Sets the gains to the default 1.0f.
        /// </summary>
        private void SetDefaultGains()
        {
            for (int i = 0; i < xBar.Length; i++)
            {
                gains.Add(1.0f);
            }
        }
    }
}
