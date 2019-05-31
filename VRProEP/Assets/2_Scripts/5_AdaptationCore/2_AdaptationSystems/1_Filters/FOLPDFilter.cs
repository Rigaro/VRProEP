using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public class FOLPDFilter : FODFilter
    {
        /// <summary>
        /// Creates a first order low-pass filter.
        /// </summary>
        /// <param name="wo">Cut-off frequency in rad/s.</param>
        /// <param name="g">The filter gain.</param>
        /// <param name="ts">The sampling time (seconds).</param>
        public FOLPDFilter(float wo, float g, float ts) : base(wo, g, ts)
        {
        }

        protected override void ComputeAlpha()
        {
            float RC = 1 / wo;
            alpha = ts / (RC + ts);
        }

        /// <summary>
        /// Updates the filter output for given 'ym' input.
        /// </summary>
        /// <param name="u">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public override float Update(float u)
        {
            float y = (float)Math.Round((alpha * u) + g * ((1 - alpha) * x_prev), 2);
            x_prev = y;
            return y;
        }
    }
}