using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VRProEP.Utilities
{
    public abstract class FirstOrderFilter : IFilter
    {

        protected float y_prev = 0;
        protected float u_prev = 0;
        protected float fc; // Cut-off frequency
        protected float g; // Filter gain
        protected float dt; // Sample time
        protected float alpha; // Discretization alpha.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fc">Cut-off frequency in Hz.</param>
        /// <param name="g"></param>
        /// <param name="dt"></param>
        public FirstOrderFilter(float fc, float g, float dt)
        {
            SetCutOffFrequency(fc);
            SetGain(g);
            SetSampleTime(dt);
            ComputeAlpha();
            //Debug.Log(alpha);
        }

        public FirstOrderFilter() : this(1, 1, 1)
        {
        }

        /// <summary>
        /// Computes the alpha value of discrete filter according to ZOH discretization.
        /// </summary>
        protected abstract void ComputeAlpha();

        /// <summary>
        /// Sets the cut-off frequency 'wo' in rad/s of the filter.
        /// </summary>
        /// <param name="fc">The filter cut-off frequency 'wo' in rad/s.</param>
        public void SetCutOffFrequency(float fc)
        {
            this.fc = fc;
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
        /// Sets the sample time 'Ts' of the filter.
        /// </summary>
        /// <param name="Ts">The filter (system) sample time.</param>
        public void SetSampleTime(float dt)
        {
            if (dt <= 0)
                throw new System.ArgumentOutOfRangeException("Sample time should be greater than 0.");
            this.dt = dt;
        }
        /// <summary>
        /// Updates the filter output for given 'u' input.
        /// </summary>
        /// <param name="u">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public abstract float Update(float u);
    }
}