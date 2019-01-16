using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.Utilities
{
    public class LowPassFilter : FirstOrderFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wo">Cut-off frequency in rad/s.</param>
        /// <param name="g"></param>
        /// <param name="ts"></param>
        public LowPassFilter(float wo, float g, float ts) : base(wo, g, ts)
        {
        }
        /// <summary>
        /// Updates the filter output for given 'ym' input.
        /// </summary>
        /// <param name="ym">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public override float Update(float ym)
        {
            float yf = (Alpha * x_prev) + g * ((1 - Alpha) * ym);
            x_prev = yf;
            return yf;
        }
    }
}