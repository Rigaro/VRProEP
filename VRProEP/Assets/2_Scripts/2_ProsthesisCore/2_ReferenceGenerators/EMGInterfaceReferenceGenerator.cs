﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public enum EMGInterfaceType
    {
        singleSiteProportional,
        dualSiteProportional
    }

    public class EMGInterfaceReferenceGenerator : ReferenceGenerator
    {

        private List<float> gains = new List<float>();
        private EMGInterfaceType interfaceType;

        public List<float> Gains
        {
            get
            {
                return gains;
            }

            set
            {
                gains = value;
            }
        }


        /// <summary>
        /// Proportional EMG reference generator that converts an EMG signal (either single or dual site) into a joint velocity reference.
        /// Sets all integrator gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        public EMGInterfaceReferenceGenerator(float[] xBar, EMGInterfaceType interfaceType)
        {
            channelSize = xBar.Length;
            this.xBar = xBar;
            SetDefaultGains();
            generatorType = ReferenceGeneratorType.EMGInterface;
            this.interfaceType = interfaceType;
        }

        /// <summary>
        /// Proportional EMG reference generator that converts an EMG signal (either single or dual site) into a joint velocity reference.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="gains">The desired integrator gain for each reference.</param>
        public EMGInterfaceReferenceGenerator(float[] xBar, List<float> gains, EMGInterfaceType interfaceType)
        {
            if (xBar.Length != gains.Count)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.Gains = gains;
            generatorType = ReferenceGeneratorType.EMGInterface;
            this.interfaceType = interfaceType;
        }

        /// <summary>
        /// Proportional EMG reference generator that converts an EMG signal (either single or dual site) into a joint velocity reference.
        /// Sets all gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        public EMGInterfaceReferenceGenerator(float[] xBar, float[] xMin, float[] xMax, EMGInterfaceType interfaceType)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            SetDefaultGains();
            generatorType = ReferenceGeneratorType.EMGInterface;
            this.interfaceType = interfaceType;
        }

        /// <summary>
        /// Proportional EMG reference generator that converts an EMG signal (either single or dual site) into a joint velocity reference.
        /// Sets all gains to the default: 1.0.
        /// </summary>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        /// <param name="gains">The desired integrator gain for each reference.</param>
        public EMGInterfaceReferenceGenerator(float[] xBar, float[] xMin, float[] xMax, List<float> gains, EMGInterfaceType interfaceType)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length || xBar.Length != gains.Count)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            this.Gains = gains;
            generatorType = ReferenceGeneratorType.EMGInterface;
            this.interfaceType = interfaceType;
        }

        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The input of size 1 for single site and size 2 for dual site.</param>
        /// <returns>The updated reference.</returns>
        public override float UpdateReference(int channel, float[] input)
        {
            // Check validity of the provided channel
            if (!IsChannelValid(channel))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");

            // Check validity of the provided input
            if (!IsInputValid(input))
                throw new System.ArgumentOutOfRangeException("The provided input does not match the interface type.");

            float tempXBar = 0.0f;

            // Generate and integrate reference.
            if (interfaceType == EMGInterfaceType.singleSiteProportional)
            {
                tempXBar = xBar[channel - 1] + (Gains[channel - 1] * input[0] * Time.fixedDeltaTime);
            }
            else if (interfaceType == EMGInterfaceType.dualSiteProportional)
            {
                tempXBar = xBar[channel - 1] + (Gains[channel - 1] * (input[0] - input[1]) * Time.fixedDeltaTime);
            }

            // Saturate reference
            if (tempXBar > xMax[channel - 1])
                tempXBar = xMax[channel - 1];
            else if (tempXBar < xMin[channel - 1])
                tempXBar = xMin[channel - 1];

            xBar[channel - 1] = (float)System.Math.Round(tempXBar, 3);
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
            if (!(input.Length == channelSize || input.Length/2 == channelSize))
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match the number of reference channels.");

            for (int i = 1; i <= channelSize; i++)
            {
                // Update each channel depending on type.
                if (interfaceType == EMGInterfaceType.singleSiteProportional)
                {
                    UpdateReference(i, input);
                }
                else if (interfaceType == EMGInterfaceType.dualSiteProportional)
                {
                    float[] tempIn = { input[2*i - 1], input[2*i] }; // Select the correct pair.
                    UpdateReference(i, tempIn);
                }
            }
            return xBar;
        }

        /// <summary>
        /// Sets the gains to the default 1.0f.
        /// </summary>
        private void SetDefaultGains()
        {
            ;
            for (int i = 0; i < xBar.Length; i++)
            {
                gains.Add(1.0f);
            }
        }

        /// <summary>
        /// Checks the validity of the provided input.
        /// </summary>
        /// <param name="input">The input to be verified.</param>
        /// <returns>True if valid.</returns>
        protected new bool IsInputValid(float[] input)
        {
            // Check validity of the provided channel
            if (interfaceType == EMGInterfaceType.singleSiteProportional && input.Length != 1)
                return false;
            if (interfaceType == EMGInterfaceType.dualSiteProportional && input.Length != 2)
                return false;
            else
                return true;
        }
    }
}
