using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface IObjectiveFunction
    {
        /// <summary>
        /// Sets the cost function weights.
        /// </summary>
        /// <param name="w"> The weight vector. </param>
        void SetWeights(List<float> w);
        /// <summary>
        /// Calculates the cost given the cost/objective function.
        /// </summary>
        /// <param name="u"> The input vector. </param>
        /// <returns> The updated cost. </returns>
        float Update(List<float> u);
    }
}