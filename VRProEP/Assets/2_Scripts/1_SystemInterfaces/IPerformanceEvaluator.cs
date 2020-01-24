using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface IPerformanceEvaluator<T>
    {
        /// <summary>
        /// Adds a data-set for performance evaluation.
        /// </summary>
        /// <param name="data"> The data vectors. </param>
        void AppendData(List<T> data);
        /// <summary>
        /// Clears the performance evaluator data memory.
        /// </summary>
        void ClearData();
        /// <summary>
        /// Sets the number of data-sets to be used for performance evaluation.
        /// </summary>
        /// <param name="size"></param>
        void SetBufferSize(int size);
        /// <summary>
        /// Calculates the performance with the current data-sets available.
        /// </summary>
        /// <returns> The updated performance. </returns>
        float EvaluatePerformance();
    }
}
