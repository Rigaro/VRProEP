using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRProEP.Utilities;

namespace VRProEP.AdaptationCore
{
    public class SumOfNormsOF : IObjectiveFunction
    {
        protected List<float> w;

        public SumOfNormsOF()
        {
            this.w = new List<float>();
        }

        /// <summary>
        /// Creates the convex sum of norms objective function with the given weights.
        /// </summary>
        /// <param name="w">The weights for the objective function.</param>
        public SumOfNormsOF(List<float> w)
        {
            this.w = new List<float>();
            SetWeights(w);
        }

        /// <summary>
        /// Sets the cost function weights.
        /// </summary>
        /// <param name="w"> The weight vector. </param>
        public void SetWeights(List<float> w)
        {
            // Check validity
            if (w == null) throw new System.ArgumentNullException("The weight vector is empty");

            float weightSum = 0;
            // Check sign
            foreach (float value in w)
            {
                if (Mathf.Sign(value) == -1) throw new System.ArgumentException("The sign of one of the weights is negative");
                weightSum += value;
            }

            this.w.AddRange(w);
        }
        /// <summary>
        /// Calculates the cost given the cost/objective function.
        /// </summary>
        /// <param name="u"> The input vector. </param>
        /// <returns> The updated cost. </returns>
        public float Update(List<float> u)
        {
            // Check validity
            if (w == null) throw new System.ArgumentNullException("The weight vector is empty");
            if (u == null) throw new System.ArgumentNullException("The data vector is empty");
            if (u.Count != w.Count) throw new System.ArgumentException("The data and weight vectors do not have the same length.");

            return Math.VectorDotProduct(w.ToArray(), Math.VectorElementMultiplication(u.ToArray(), u.ToArray()));
        }
    }
}
