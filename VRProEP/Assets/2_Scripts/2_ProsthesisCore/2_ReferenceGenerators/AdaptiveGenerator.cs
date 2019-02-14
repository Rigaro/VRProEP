//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Abstract adaptive reference generator to add basic variables used across all generators.
    /// </summary>
    public abstract class AdaptiveGenerator : IReferenceGenerator, IAdaptable
    {
        private int parameterChannelSize;
        private float[] theta; // The interface parameters.
        private float[] thetaMin;
        private float[] thetaMax;
        protected int channelSize;
        protected float[] xBar; // The reference state
        protected float[] xMin;
        protected float[] xMax;
        protected ReferenceGeneratorType generatorType;
        protected bool isEnabled = false;

        public int ParameterChannelSize()
        {
                return parameterChannelSize;
        }
        public int ChannelSize()
        {
                return channelSize;
        }

        public enum ChannelType
        {
            Parameter,
            Reference
        }

        /// <summary>
        /// Abstract adaptive reference generator to add basic variables used across all generators.
        /// </summary>
        /// <param name="xBar">The initial references.</param>
        /// <param name="xMin">The lower limit for the references.</param>
        /// <param name="xMax">The upper limit for the references.</param>
        /// <param name="theta">The initial parameters.</param>
        /// <param name="thetaMin">The lower limit for the parameters.</param>
        /// <param name="thetaMax">The upper limit for the parameters.</param>
        public AdaptiveGenerator(float[] xBar, float[] xMin, float[] xMax, float[] theta, float[] thetaMin, float[] thetaMax, ReferenceGeneratorType generatorType)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length || theta.Length != xBar.Length || theta.Length != thetaMin.Length || theta.Length != thetaMax.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            parameterChannelSize = theta.Length;
            this.theta = theta;
            this.thetaMin = thetaMin;
            this.thetaMax = thetaMax;
            this.generatorType = generatorType;
        }

        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The input to use to update the reference.</param>
        /// <returns>The updated reference.</returns>
        public abstract float UpdateReference(int channel, float[] input);

        /// <summary>
        /// Updates all the references to be tracked by multiple controllers or devices.
        /// Should only be called within Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="input">The input to use to update the references.</param>
        /// <returns>The updated set of references.</returns>
        public abstract float[] UpdateAllReferences(float[] input);

        /// <summary>
        /// Returns the current reference value for the provided channel.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>The current reference value.</returns>
        public float GetReference(int channel)
        {
            // Check validity of the provided channel
            if (!IsChannelValid(channel, ChannelType.Reference))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");

            return xBar[channel];
        }
        
        /// <summary>
        /// Returns the current references.
        /// </summary>
        /// <returns>The current set of references.</returns>
        public float[] GetAllReferences()
        {
            return xBar;
        }
        /// <summary>
        /// Forces a value into the reference. It is recommended that it is used only for re-setting/intializing a reference.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="value">The value to set the reference to.</param>
        public void SetReference(int channel, float value)
        {
            // Check validity of the provided channel
            if (!IsChannelValid(channel,ChannelType.Reference))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");

            if (value > xMax[channel] || value < xMin[channel])
                throw new System.ArgumentOutOfRangeException("The provided parameter exceeds the limits.");

            xBar[channel] = value;
        }

        /// <summary>
        /// Forces a set of values into the references. It is recommended that it is used only for re-setting/intializing a reference.
        /// </summary>
        /// <param name="refs">The set of values to set the references to.</param>
        public void SetAllReferences(float[] refs)
        {
            // Check validity of provided references
            if (!IsInputValid(refs))
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match the number of reference channels.");

            // Check limits
            int channel = 0;
            foreach (float value in refs)
            {
                if (value > xMax[channel] || value < xMin[channel])
                    throw new System.ArgumentOutOfRangeException("A provided parameter exceeds the limits.");

                channel++;
            }

            xBar = refs;
        }

        /// <summary>
        /// Updates a given parameter or set of parameters to the given values.
        /// </summary>
        /// <param name="channel">The parameter number or channel to be updated.</param>
        /// <param name="parameter">The new parameter value.</param>
        public void UpdateParameter(int channel, float parameter)
        {
            // Check validity of the provided channel
            if (!IsChannelValid(channel,ChannelType.Parameter))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");

            if (parameter > thetaMax[channel] || parameter < thetaMin[channel])
                throw new System.ArgumentOutOfRangeException("The provided parameter exceeds the limits.");

            theta[channel] = parameter;
        }

        /// <summary>
        /// Updates a given parameter or set of parameters to the given values.
        /// </summary>
        /// <param name="parameters">The new parameters values set to be updated.</param>
        public void UpdateAllParameters(float[] parameters)
        {
            if (theta.Length != parameters.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            // Check limits
            int channel = 0;
            foreach (float value in parameters)
            {
                if (value > thetaMax[channel] || value < thetaMin[channel])
                    throw new System.ArgumentOutOfRangeException("A provided parameter exceeds the limits.");

                channel++;
            }

            theta = parameters;
        }

        /// <summary>
        /// Returns the given parameter value.
        /// </summary>
        /// <param name="channel">The parameter number or channel to be read.</param>
        /// <returns>The desired parameter value</returns>
        public float GetParameter(int channel)
        {
            // Check validity of the provided channel
            if (!IsChannelValid(channel,ChannelType.Parameter))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");

            return theta[channel];
        }

        /// <summary>
        /// Returns all the parameter values.
        /// </summary>
        /// <returns>The set of parameter values.</returns>
        public float[] GetAllParameters()
        {
            return theta;
        }

        /// <summary>
        /// Checks the validity of the provided output.
        /// </summary>
        /// <param name="input">The input to be verified.</param>
        /// <returns>True if valid.</returns>
        protected bool IsChannelValid(int channel, ChannelType type)
        {
            if (channel < 0)
                return false;
            // Check validity of the provided channel
            if (type == ChannelType.Reference)
            {
                if (channel >= channelSize)
                    return false;
            }
            else
            {
                if (channel >= parameterChannelSize)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks the validity of the provided input.
        /// </summary>
        /// <param name="input">The input to be verified.</param>
        /// <returns>True if valid.</returns>
        protected bool IsInputValid(float[] input)
        {
            // Check validity of the provided channel
            if (input.Length != xBar.Length)
                return false;
            else
                return true;
        }

        public ReferenceGeneratorType GeneratorType()
        {
            return generatorType;
        }


        public bool IsEnabled()
        {
            return isEnabled;
        }
    }

}
