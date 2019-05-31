using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface IFilter
    {

        /// <summary>
        /// Sets the cut-off frequency 'wo' in rad/s of the filter.
        /// </summary>
        /// <param name="wo">The filter cut-off frequency 'wo' in rad/s.</param>
        void SetCutOffFrequency(float wo);
        /// <summary>
        /// Sets the gain 'G' of the filter.
        /// </summary>
        /// <param name="g"></param>
        void SetGain(float g);
        /// <summary>
        /// Sets the sample time 'Ts' of the filter.
        /// </summary>
        /// <param name="ts">The filter (system) sample time.</param>
        void SetSamplingTime(float ts);
        /// <summary>
        /// Updates the filter output for given 'u' input.
        /// </summary>
        /// <param name="u">The signal to be filtered</param>
        /// <returns>The filtered signal 'y'.</returns>
        float Update(float u);
        /// <summary>
        /// Resets the filter states.
        /// </summary>
        void Reset();


    }
}
