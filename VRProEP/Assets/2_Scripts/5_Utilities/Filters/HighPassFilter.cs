using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.Utilities
{
    public class HighPassFilter : FirstOrderFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fc">Cut-off frequency in Hz.</param>
        /// <param name="g"></param>
        /// <param name="ts"></param>
        public HighPassFilter(float fc, float g, float ts) : base(fc, g, ts)
        {
        }


        protected override void ComputeAlpha()
        {
            float RC = 1 / (2 * Mathf.PI * fc);
            alpha = RC / (RC + dt);
        }

        /// <summary>
        /// Updates the filter output for given 'ym' input.
        /// </summary>
        /// <param name="u">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public override float Update(float u)
        {
            float y = (float)Math.Round((alpha * y_prev) + g * (u - u_prev), 2);
            u_prev = u;
            y_prev = y;
            return y;
        }
    }
}