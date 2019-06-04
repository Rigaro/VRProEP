using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface IPerformanceManager
    {
        /// <summary>
        /// Adds the given measure of performance for data analysis.
        /// </summary>
        /// <param name="pe"> The performance evaluator to add. </param>
        void AddPerformanceEvaluator(IPerformanceEvaluator pe);

        /// <summary>
        /// Removes the given measure of performance for data analysis.
        /// </summary>
        /// <param name="pe"> The performance evaluator to remove. </param>
        void RemovePerformanceEvaluator(IPerformanceEvaluator pe);

        /// <summary>
        /// Sets the cost function to be used for cost calculation.
        /// </summary>
        /// <param name="cf"> The desired cost function. </param>
        void SetCostFunction(ICostFunction cf);

        /// <summary>
        /// Updates the cost with the given data.
        /// </summary>
        /// <param name="dataBuffers"> The data-sets to be used to update the cost. </param>
        /// <returns> The updated cost. </returns>
        float UpdateCost(List<List<float>> dataBuffers);
    }
}
