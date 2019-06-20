using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface IOptimiser
    {
        /// <summary>
        /// Sets the optimiser gain.
        /// </summary>
        /// <param name="k">The optimiser gain.</param>
        void SetGain(float k);
        /// <summary>
        /// Sets the sample time 'ts' needed for integration.
        /// </summary>
        /// <param name="ts">The optimiser (system) sample time.</param>
        void SetSamplingTime(float ts);
        /// <summary>
        /// Updates the value of the variable being optimized (theta) given a vector of derivatives (duArray).
        /// </summary>
        /// <param name="duArray">The vector of derivatives.</param>
        /// <returns>The updated variable.</returns>
        float Update(float[] duArray);
        /// <summary>
        /// Resets the state of the optimizer.
        /// </summary>
        void Reset();
        /// <summary>
        /// Sets the state of the optimizer.
        /// </summary>
        void Reset(float x_o);
    }
}
