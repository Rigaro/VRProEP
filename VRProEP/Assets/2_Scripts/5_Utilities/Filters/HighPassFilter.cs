using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.Utilities
{
    public class HighPassFilter : FirstOrderFilter
    {

        public HighPassFilter(float wo, float g, float ts) : base(wo, g, ts)
        {
        }

        /// <summary>
        /// Updates the filter output for given 'ym' input.
        /// </summary>
        /// <param name="ym">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public override float Update(float ym)
        {
            float yf = (Alpha * x_prev) + g * (ym - u_prev);
            u_prev = ym;
            x_prev = yf;
            return yf;
        }
    }
}