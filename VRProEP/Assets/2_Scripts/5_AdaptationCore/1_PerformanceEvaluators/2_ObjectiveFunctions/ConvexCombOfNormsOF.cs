using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRProEP.Utilities;

namespace VRProEP.AdaptationCore
{
    public class ConvexSumOfNormsOF : SumOfNormsOF
    {
        public ConvexSumOfNormsOF()
        {
            this.w = new List<float>();
        }

        /// <summary>
        /// Creates the convex sum of norms objective function with the given weights.
        /// </summary>
        /// <param name="w">The weights for the objective function.</param>
        public ConvexSumOfNormsOF(List<float> w)
        {
            this.w = new List<float>();
            SetWeights(w);
        }

        /// <summary>
        /// Sets the cost function weights.
        /// </summary>
        /// <param name="w"> The weight vector. </param>
        public new void SetWeights(List<float> w)
        {
            // Check validity
            if (w == null) throw new System.ArgumentNullException("The weight vector is empty");

            float weightSum = 0;
            // Check sign
            foreach(float value in w)
            {
                if (Mathf.Sign(value) == -1) throw new System.ArgumentException("The sign of one of the weights is negative");
                weightSum += value;
            }
            // Check sum
            if (weightSum != 1) throw new System.ArgumentException("The sum of the weights is not equal to 1.");

            this.w.AddRange(w);
        }
    }
}
