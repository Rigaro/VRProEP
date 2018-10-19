//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Basic reference generator that integrates the given input. Suitable for velocity control type of interfaces.
    /// Examples of IRGs: velocity control EMG, joystick manual control.
    /// </summary>
    public class IntegratorReferenceGenerator : ReferenceGenerator {

        private float[] gains;

        /// <summary>
        /// Basic reference generator that integrates the given input. Suitable for velocity control type of interfaces.
        /// Examples of IRGs: velocity control EMG, joystick manual control.
        /// Sets all integrator gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        public IntegratorReferenceGenerator(float[] xBar)
        {
            channelSize = xBar.Length;
            this.xBar = xBar;
            SetDefaultGains();
        }

        /// <summary>
        /// Basic reference generator that integrates the given input. Suitable for velocity control type of interfaces.
        /// Examples of IRGs: velocity control EMG, joystick manual control.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="gains">The desired integrator gain for each reference.</param>
        public IntegratorReferenceGenerator(float[] xBar, float[] gains)
        {
            if (xBar.Length != gains.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.gains = gains;
        }

        /// <summary>
        /// Basic reference generator that integrates the given input. Suitable for velocity control type of interfaces.
        /// Examples of IRGs: velocity control EMG, joystick manual control.
        /// Sets all gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        public IntegratorReferenceGenerator(float[] xBar, float[] xMin, float[] xMax)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            SetDefaultGains();
        }

        /// <summary>
        /// Basic reference generator that integrates the given input. Suitable for velocity control type of interfaces.
        /// Examples of IRGs: velocity control EMG, joystick manual control.
        /// Sets all gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        /// <param name="gains">The desired integrator gain for each reference.</param>
        public IntegratorReferenceGenerator(float[] xBar, float[] xMin, float[] xMax, float[] gains)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length || xBar.Length != gains.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            this.gains = gains;
        }

        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">Takes the element "channel number" to be integrated.</param>
        /// <returns>The updated reference.</returns>
        public override float UpdateReference(int channel, float[] input)
        {
            // Check validity of the provided channel
            if (!IsChannelValid(channel))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");

            // Check validity of the provided input
            if (xBar.Length != gains.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            // Integrate
            float tempXBar = xBar[channel - 1] + gains[channel - 1] * input[channel - 1] * Time.fixedDeltaTime;
            // Saturate reference
            if (tempXBar > xMax[channel - 1])
                tempXBar = xMax[channel - 1];
            else if (tempXBar < xMin[channel - 1])
                tempXBar = xMin[channel - 1];

            xBar[channel - 1] = (float)System.Math.Round(tempXBar, 2);
            return xBar[channel - 1];

        }

        /// <summary>
        /// Updates all the references to be tracked by multiple controllers or devices.
        /// Should only be called within Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="input">The input to be integrated.</param>
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
        /// Sets the gains to the default 1.0f.
        /// </summary>
        private void SetDefaultGains()
        {
            for (int i = 0; i < xBar.Length; i++)
            {
                gains[i] = 1.0f;
            }
        }
    }
}