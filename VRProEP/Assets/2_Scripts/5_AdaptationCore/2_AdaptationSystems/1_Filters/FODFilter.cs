using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VRProEP.AdaptationCore
{
    public abstract class FODFilter : IFilter
    {

        protected float x_prev = 0;
        protected float u_prev = 0;
        protected float wo; // Cut-off frequency in rad/s
        protected float g; // Filter gain
        protected float ts; // Sampling time
        protected float alpha; // Discretization alpha.

        /// <summary>
        /// Initialises the data for a first order digital filter.
        /// </summary>
        /// <param name="wo">Cut-off frequency in rad/s.</param>
        /// <param name="g">The filter gain.</param>
        /// <param name="ts">The sampling time (seconds).</param>
        public FODFilter(float wo, float g, float ts)
        {
            SetCutOffFrequency(wo);
            SetGain(g);
            SetSamplingTime(ts);
            ComputeAlpha();
            //Debug.Log(alpha);
        }

        public FODFilter() : this(1, 1, 1)
        {
        }

        /// <summary>
        /// Computes the alpha value of discrete filter according to ZOH discretization.
        /// </summary>
        protected abstract void ComputeAlpha();

        /// <summary>
        /// Sets the cut-off frequency 'wo' in rad/s of the filter.
        /// </summary>
        /// <param name="wo">The filter cut-off frequency 'wo' in rad/s.</param>
        public void SetCutOffFrequency(float wo)
        {
            this.wo = wo;
        }
        /// <summary>
        /// Sets the gain 'G' of the filter.
        /// </summary>
        /// <param name="G"></param>
        public void SetGain(float g)
        {
            this.g = g;
        }
        /// <summary>
        /// Sets the sampling time 'ts' of the filter.
        /// </summary>
        /// <param name="ts">The filter (system) sampling time.</param>
        public void SetSamplingTime(float ts)
        {
            if (ts <= 0)
                throw new System.ArgumentOutOfRangeException("Sample time should be greater than 0.");
            this.ts = ts;
        }
        /// <summary>
        /// Updates the filter output for given 'u' input.
        /// </summary>
        /// <param name="u">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public abstract float Update(float u);

        /// <summary>
        /// Resets the filter states.
        /// </summary>
        public void Reset()
        {
            x_prev = 0;
            u_prev = 0;
        }
    }
}