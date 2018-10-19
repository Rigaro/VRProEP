//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Abstract reference generator to add basic variables used across all generators.
    /// </summary>
    public abstract class ReferenceGenerator : IReferenceGenerator
    {
        protected int channelSize;
        protected float[] xBar;
        protected float[] xMin;
        protected float[] xMax;

        public int ChannelSize()
        {
                return channelSize;
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
            if (!IsChannelValid(channel))
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
            if (!IsChannelValid(channel))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");            

            if (value > xMax[channel - 1] || value < xMin[channel - 1])
                throw new System.ArgumentOutOfRangeException("The provided parameter exceeds the limits.");

            xBar[channel - 1] = value;
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
            int channel = 1;
            foreach (float value in refs)
            {
                if (value > xMax[channel - 1] || value < xMin[channel - 1])
                    throw new System.ArgumentOutOfRangeException("The provided parameter for channel #" + channel  + " exceeds the limits.");

                channel++;
            }

            xBar = refs;
        }

        /// <summary>
        /// Checks the validity of the provedided output.
        /// </summary>
        /// <param name="input">The input to be verified.</param>
        /// <returns>True if valid.</returns>
        protected bool IsChannelValid(int channel)
        {
            // Check validity of the provided channel
            if (channel > channelSize)
                return false;
            else if (channel <= 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Checks the validity of the provedided input.
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
    }
}
