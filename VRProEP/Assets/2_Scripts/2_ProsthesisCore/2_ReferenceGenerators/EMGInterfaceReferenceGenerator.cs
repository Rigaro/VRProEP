using System;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.Utilities;

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
        private bool isEnabled = false;
        private bool enableRequested = false;
        private List<LowPassFilter> lowPassFilters = new List<LowPassFilter>();
        private List<MovingAverage> movingAverageFilters = new List<MovingAverage>();

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
        /// Proportional EMG reference generator that converts an EMG signal into a joint angle or velocity reference.
        /// Single site: joint angle reference.
        /// Dual site: joint velocity reference.
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
            //  initialize filters
            for (int i = 0; i < channelSize; i++)
            {
                lowPassFilters.Add(new LowPassFilter(3.0f, 1.0f, Time.fixedDeltaTime));
                movingAverageFilters.Add(new MovingAverage(15));
            }
        }

        /// <summary>
        /// Proportional EMG reference generator that converts an EMG signal into a joint angle or velocity reference.
        /// Single site: joint angle reference.
        /// Dual site: joint velocity reference.
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
            //  initialize filters
            for (int i = 0; i < channelSize; i++)
            {
                lowPassFilters.Add(new LowPassFilter(3.0f, 1.0f, Time.fixedDeltaTime));
                movingAverageFilters.Add(new MovingAverage(15));
            }
        }

        /// <summary>
        /// Proportional EMG reference generator that converts an EMG signal into a joint angle or velocity reference.
        /// Single site: joint angle reference.
        /// Dual site: joint velocity reference.
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
            //  initialize filters
            for (int i = 0; i < channelSize; i++)
            {
                lowPassFilters.Add(new LowPassFilter(3.0f, 1.0f, Time.fixedDeltaTime));
                movingAverageFilters.Add(new MovingAverage(15));
            }
        }

        /// <summary>
        /// Proportional EMG reference generator that converts an EMG signal into a joint angle or velocity reference.
        /// Single site: joint angle reference.
        /// Dual site: joint velocity reference.
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
            //  initialize filters
            for (int i = 0; i < channelSize; i++)
            {
                lowPassFilters.Add(new LowPassFilter(3.0f, 1.0f, Time.fixedDeltaTime));
                movingAverageFilters.Add(new MovingAverage(15));
            }
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

            // Extract input
            bool enable = false;
            if (input[0] >= 1.0f)
                enable = true;

            // Check if requested to enable the synergy
            if (enable && !enableRequested && !isEnabled)
            {
                // Requested to enable, get button down
                //Debug.Log("Enable requested.");
                enableRequested = true;
                isEnabled = true;
            }
            else if (!enable && enableRequested) // Released button
            {
                enableRequested = false;
            }
            else if (enable && !enableRequested && isEnabled)
            {
                //Debug.Log("Disable requested.");
                // Requested to disable, get button down
                enableRequested = true;
                isEnabled = false;
            }

            float tempXBar = xBar[channel];

            // Generate reference.
            if (isEnabled && interfaceType == EMGInterfaceType.singleSiteProportional)
            {
                tempXBar = - Mathf.Deg2Rad * Gains[channel] * input[1]; // Obtained angle from scaled EMG input, since processed EMG input range is 0-100.
            }
            else if (isEnabled && interfaceType == EMGInterfaceType.dualSiteProportional)
            {
                // Threshold diff EMG behaviour
                float diffEmg = input[2] - input[1];
                float filtDiffEmg = lowPassFilters[channel].Update(diffEmg);
                float avfDiffEmg = (float)Math.Round(movingAverageFilters[channel].Update(filtDiffEmg), 1);
                Debug.Log(avfDiffEmg);
                if (Mathf.Abs(avfDiffEmg) > 7.0f)
                {
                    tempXBar = xBar[channel] + (Gains[channel] * avfDiffEmg * Time.fixedDeltaTime); // Differential velocity control.
                }
            }

            // Saturate reference
            if (tempXBar > xMax[channel])
                tempXBar = xMax[channel];
            else if (tempXBar < xMin[channel])
                tempXBar = xMin[channel];

            xBar[channel] = (float)System.Math.Round(tempXBar, 3);
            //Debug.Log(xBar[channel]);
            return xBar[channel];

        }

        /// <summary>
        /// Updates all the references to be tracked by multiple controllers or devices.
        /// Should only be called within Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="input">The input to be integrated.</param>
        /// <returns>The updated set of references.</returns>
        public override float[] UpdateAllReferences(float[] input)
        {
            for (int i = 0; i < channelSize; i++)
            {
                // Update each channel depending on type.
                if (interfaceType == EMGInterfaceType.singleSiteProportional)
                {
                    float[] tempIn = { input[2 * i], input[2 * i + 1] };
                    UpdateReference(i, input);
                }
                else if (interfaceType == EMGInterfaceType.dualSiteProportional)
                {
                    float[] tempIn = { input[2*i], input[2*i + 1], input[2 * i + 2] }; // Select the correct pair.
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
            if (interfaceType == EMGInterfaceType.singleSiteProportional && input.Length != 2)
                return false;
            if (interfaceType == EMGInterfaceType.dualSiteProportional && input.Length != 3)
                return false;
            else
                return true;
        }
    }
}
