using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface ICostFunction
    {
        /// <summary>
        /// Sets the cost function weights.
        /// </summary>
        /// <param name="W"> The weight vector. </param>
        void SetWeights(float[] W);
        /// <summary>
        /// Calculates the cost given the cost/objective function.
        /// </summary>
        /// <param name="u"> The input vector. </param>
        /// <returns> The updated cost. </returns>
        float UpdateCost(float[] u);
    }
}