using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Discrete implementation of a Q filter, from continuous transfer function:
/// Ho = H/Q, where H is the filter gain.
///               Ho(wo/Q)s
///  H(s) = ----------------------
///          s^2 + (wo/Q)s + wo^2
/// With discrete implementation:
///               G(z-1)
///  H(z) = ----------------------
///          1 + b1 z^-1 + b2 z^-2
/// where G, a, b are given by: See http://lpsa.swarthmore.edu/LaplaceZTable/LaplaceZFuncTable.html
/// Easiest way to compute G, a, b for a given filter is through MATLAB.
/// Eventually will implement automatic computation from H, wo, Q.
/// For more filter references see: http://www.analog.com/en/analog-dialogue/articles/band-pass-response-in-active-filters.html

/// </summary>
namespace VRProEP.Utilities
{
    public class BandPassQFilter : IFilter
    {

        private float x_1 = 0;
        private float x_2 = 0;
        private float u_1 = 0;
        private float wo; // Cut-off frequency
        private float G; // Filter gain
        private float Ts; // Sample time
        private float b1;
        private float b2;

        public BandPassQFilter(float G, float b1, float b2)
        {
            SetGain(G);
            this.b1 = b1;
            this.b2 = b2;
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
        public void SetGain(float G)
        {
            this.G = G;
        }
        /// <summary>
        /// Sets the sample time 'Ts' of the filter.
        /// </summary>
        /// <param name="Ts">The filter (system) sample time.</param>
        public void SetSampleTime(float Ts)
        {
            if (Ts <= 0)
                throw new System.ArgumentOutOfRangeException("Sample time should be greater than 0.");
            this.Ts = Ts;
        }
        /// <summary>
        /// Updates the filter output for given 'ym' input.
        /// </summary>
        /// <param name="ym">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        public float Update(float ym)
        {
            float yf = -b1 * x_1 - b2 * x_2 + G * (ym - u_1);
            x_2 = x_1;
            x_1 = yf;
            u_1 = ym;
            return yf;
        }
    }
}