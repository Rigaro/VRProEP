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
        /// <typeparam name="T">The type of data the performance evaluator takes.</typeparam>
        /// <param name="pe"> The performance evaluator to add. </param>
        //void AddPerformanceEvaluator<T>(IPerformanceEvaluator<T> pe);

        /// <summary>
        /// Removes the given measure of performance for data analysis.
        /// </summary>
        /// <typeparam name="T">The type of data the performance evaluator takes.</typeparam>
        /// <param name="pe"> The performance evaluator to remove. </param>
        //void RemovePerformanceEvaluator<T>(IPerformanceEvaluator<T> pe);

        /// <summary>
        /// Sets the cost function to be used for cost calculation.
        /// </summary>
        /// <param name="cf"> The desired cost function. </param>
        //void SetObjectiveFunction(IObjectiveFunction cf);

        /// <summary>
        /// Adds data to a performance evaluator.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="data">The list containing the data.</param>
        /// <param name="peID">The ID of the performance evaluator to add the data to.</param>
        void AddData<T>(List<T> data, string peID);

        /// <summary>
        /// Updates the performance with the current data in the performance evaluators.
        /// </summary>
        /// <returns>The updated performance. </returns>
        float Update();
    }
}
