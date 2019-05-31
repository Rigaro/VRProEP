using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public class SinusoidalDither : IDitherGenerator
    {
        private float a;
        private float w;
        private float phi;

        public SinusoidalDither(float a, float w, float phi)
        {
            SetAmplitude(a);
            SetFrequency(w);
            SetPhase(phi);
        }

        public SinusoidalDither() : this(1, 1, 0)
        {

        }

        /// <summary>
        /// Sets the dither signal amplitude 'a'.
        /// </summary>
        /// <param name="a">Dither signal amplitude.</param>
        public void SetAmplitude(float a)
        {
            this.a = a;
        }
        /// <summary>
        /// Sets the dither signal frequency 'w' in rad/s (w > 0).
        /// </summary>
        /// <param name="w">Dither signal frequency (rad/s).</param>
        public void SetFrequency(float w)
        {
            if (w <= 0)
                throw new System.ArgumentException("The dither frequency should be > 0.");
            this.w = w;
        }
        /// <summary>
        /// Sets the dither signal phase shift 'phi' in radians.
        /// </summary>
        /// <param name="phi">Dither signal phase shift (rad).</param>
        public void SetPhase(float phi)
        {
            this.phi = phi;
        }
        /// <summary>
        /// Updates the output value of the dither signal according to a given time or iteration 't'.
        /// </summary>
        /// <param name="t">The current time or iteration.</param>
        /// <returns>The current dither value.</returns>
        public float Update(float t)
        {
            return a * Mathf.Sin(w * t + phi);
        }
    }
}
