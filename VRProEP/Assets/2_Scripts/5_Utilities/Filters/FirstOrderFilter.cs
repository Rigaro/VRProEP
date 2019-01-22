using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VRProEP.Utilities
{
    public abstract class FirstOrderFilter : IFilter
    {

        protected float x_prev = 0;
        protected float u_prev = 0;
        private float wo; // Cut-off frequency
        protected float g; // Filter gain
        private float ts; // Sample time
        private float alpha; // Discretization alpha.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wo">Cut-off frequency in rad/s.</param>
        /// <param name="g"></param>
        /// <param name="ts"></param>
        public FirstOrderFilter(float wo, float g, float ts)
        {
            SetCutOffFrequency(wo);
            SetGain(g);
            SetSampleTime(ts);
            ComputeAlpha();
            Debug.Log(alpha);
        }

        public FirstOrderFilter() : this(1, 1, 1)
        {
        }

        protected float Alpha
        {
            set { alpha = value; }
            get { return alpha; }
        }

        /// <summary>
        /// Computes the alpha value of discrete filter according to ZOH discretization.
        /// </summary>
        private void ComputeAlpha()
        {
            Alpha = (float)Math.Exp(-wo * ts);
        }

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
        /// Sets the sample time 'Ts' of the filter.
        /// </summary>
        /// <param name="Ts">The filter (system) sample time.</param>
        public void SetSampleTime(float ts)
        {
            if (ts <= 0)
                throw new System.ArgumentOutOfRangeException("Sample time should be greater than 0.");
            this.ts = ts;
        }
        /// <summary>
        /// Updates the filter output for given 'ym' input.
        /// </summary>
        /// <param name="ym">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public abstract float Update(float ym);
    }
}