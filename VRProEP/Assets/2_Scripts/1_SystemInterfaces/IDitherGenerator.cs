using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface IDitherGenerator
    {
        /// <summary>
        /// Sets the dither signal amplitude 'a'.
        /// </summary>
        /// <param name="a">Dither signal amplitude.</param>
        void SetAmplitude(float a);
        /// <summary>
        /// Sets the dither signal frequency 'w' in rad/s (w > 0).
        /// </summary>
        /// <param name="w">Dither signal frequency (rad/s).</param>
        void SetFrequency(float w);
        /// <summary>
        /// Sets the dither signal phase shift 'phi' in radians.
        /// </summary>
        /// <param name="phi">Dither signal phase shift (rad).</param>
        void SetPhase(float phi);
        /// <summary>
        /// Updates the output value of the dither signal according to a given time or iteration 't'.
        /// </summary>
        /// <param name="t">The current time or iteration.</param>
        /// <returns>The current dither value.</returns>
        float Update(float t);
    }
}
