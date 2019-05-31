using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// Gradient-based optimiser. Use negative gain for gradient descent
    /// and positive gain for gradient ascent.
    /// </summary>
    public class GradientOptimiser : IOptimiser
    {
        private float k;
        private float ts;
        private float x = 0;
        /// <summary>
        /// Gradient-based optimiser. Use negative gain for gradient descent
        /// and positive gain for gradient ascent.
        /// </summary>
        /// <param name="k">The optimiser gain.</param>
        /// <param name="ts">The optimiser sampling time, used for integration.</param>
        /// <param name="x_o">The initial condition.</param>
        public GradientOptimiser(float k, float ts, float x_o)
        {
            SetGain(k);
            SetSamplingTime(ts);
            x = x_o;
        }
        /// <summary>
        /// Gradient-based optimiser. Creates a default gradient ascent optimiser with
        /// gain 1, sampling time 1s, and 0 initial condition.
        /// </summary>
        public GradientOptimiser() : this(1, 1, 0)
        {

        }

        /// <summary>
        /// Sets the optimiser gain.
        /// </summary>
        /// <param name="k">The optimiser gain.</param>
        public void SetGain(float k)
        {
            this.k = k;
        }
        /// <summary>
        /// Sets the sample time 'ts' needed for integration.
        /// </summary>
        /// <param name="ts">The optimiser (system) sample time.</param>
        public void SetSamplingTime(float ts)
        {
            if (ts <= 0)
                throw new System.ArgumentOutOfRangeException("Sampling time should be greater than 0.");
            this.ts = ts;

        }
        /// <summary>
        /// Updates the value of the variable being optimized (theta) given a vector of derivatives (duArray).
        /// </summary>
        /// <param name="duArray">The vector of derivatives.</param>
        /// <returns>The updated variable.</returns>
        public float Update(float[] duArray)
        {
            if (duArray == null)
                throw new System.ArgumentNullException();
            else if (duArray.Length != 1)
                throw new System.ArgumentException("The vector of derivative should only contain the gradient.");
            // Integrate gradient
            x += k * ts * duArray[0];
            return x;
        }
        /// <summary>
        /// Resets the state of the optimizer.
        /// </summary>
        public void Reset()
        {
            x = 0;
        }
        /// <summary>
        /// Sets the state of the optimizer.
        /// </summary>
        public void Reset(float x_o)
        {
            x = x_o;
        }
    }
}