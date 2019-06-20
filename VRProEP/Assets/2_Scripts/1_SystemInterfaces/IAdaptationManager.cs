using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface IAdaptationManager
    {
        /// <summary>
        /// Sets the limits for the parameter that is being adapted.
        /// </summary>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        void SetParameterLimits(float min, float max);
        /// <summary>
        /// Updates the parameter to be adapted acording to the given input and time/iteration.
        /// Follows the implemented adaptation law.
        /// </summary>
        /// <param name="u">The input signal.</param>
        /// <param name="t">The current time/iteration.</param>
        /// <returns>The updated parameter value.</returns>
        float UpdateParameter(float u, float t);
        /// <summary>
        /// Returns the current parameter value.
        /// </summary>
        /// <returns>The current parameter value.</returns>
        float GetParameter();
        /// <summary>
        /// Resets the adaptation algorithm.
        /// </summary>
        void Reset();
    }
}
