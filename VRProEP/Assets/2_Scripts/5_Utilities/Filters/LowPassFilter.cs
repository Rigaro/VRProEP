using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.Utilities
{
    public class LowPassFilter : FirstOrderFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fc">Cut-off frequency in Hz.</param>
        /// <param name="g"></param>
        /// <param name="ts"></param>
        public LowPassFilter(float fc, float g, float ts) : base(fc, g, ts)
        {
        }

        protected override void ComputeAlpha()
        {
            float RC = 1 / (2 * Mathf.PI * fc);
            alpha = dt / (RC + dt);
        }

        /// <summary>
        /// Updates the filter output for given 'ym' input.
        /// </summary>
        /// <param name="u">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public override float Update(float u)
        {
            float y = (float)Math.Round((alpha * u) + g * ((1 - alpha) * y_prev), 2);
            y_prev = y;
            return y;
        }
    }
}