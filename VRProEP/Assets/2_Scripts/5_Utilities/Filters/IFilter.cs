using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.Utilities
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
        /// <param name="G"></param>
        void SetGain(float G);
        /// <summary>
        /// Sets the sample time 'Ts' of the filter.
        /// </summary>
        /// <param name="Ts">The filter (system) sample time.</param>
        void SetSampleTime(float Ts);
        /// <summary>
        /// Updates the filter output for given 'ym' input.
        /// </summary>
        /// <param name="ym">The signal to be filtered</param>
        /// <returns>The filtered signal 'yf'.</returns>
        float Update(float ym);


    }
}
