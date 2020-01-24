using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public class UpperBodyCompensationMotionPM : IPerformanceManager
    {
        public readonly string SHOULDER = "Shoulder";
        public readonly string TRUNK = "Trunk";

        private ConvexSumOfNormsOF objectiveFunction;
        private MaxDisplacementPE shEvaluator;
        private MaxDisplacementPE c7Evaluator;

        /// <summary>
        /// Creates a compensation motion performance evaluator with default 0.5 weights.
        /// </summary>
        public UpperBodyCompensationMotionPM()
        {
            // Initialise all, default weights.
            float[] w = { 0.5f, 0.5f };
            List<float> weights = new List<float>(w);
            objectiveFunction = new ConvexSumOfNormsOF(weights);
            shEvaluator = new MaxDisplacementPE();
            c7Evaluator = new MaxDisplacementPE();
        }

        /// <summary>
        /// Creates a compensation motion performance evaluator with the given weights.
        /// </summary>
        /// <param name="w1">The weight given to shoulder compensation.</param>
        /// <param name="w2">The weight given to trunk compensation.</param>
        public UpperBodyCompensationMotionPM(float w1, float w2)
        {
            // Initialise all, default weights.
            float[] w = { w1, w2 };
            List<float> weights = new List<float>(w);
            objectiveFunction = new ConvexSumOfNormsOF(weights);
            shEvaluator = new MaxDisplacementPE();
            c7Evaluator = new MaxDisplacementPE();
        }

        /// <summary>
        /// Adds data to a performance evaluator.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="data">The list containing the data.</param>
        /// <param name="peID">The ID of the performance evaluator to add the data to.</param>
        public void AddData<T>(List<T> data, string peID)
        {
            if (data == null)
                throw new System.ArgumentNullException("The data list is empty");
            if (!(data is List<Vector3>))
                throw new System.ArgumentException("The data should be in a Vector3 list format.");

            // Type-cast the data.
            List<Vector3> convertedData = (List<Vector3>)System.Convert.ChangeType(data, typeof(List<Vector3>));

            // Append the data
            if (peID == SHOULDER)
            {
                shEvaluator.ClearData();
                shEvaluator.AppendData(convertedData);
            }
            else if (peID == TRUNK)
            {
                c7Evaluator.ClearData();
                c7Evaluator.AppendData(convertedData);
            }
                
        }

        /// <summary>
        /// Updates the performance with the current data in the performance evaluators.
        /// </summary>
        /// <returns>The updated performance. </returns>
        public float Update()
        {
            // Calculate compensation with the current data and pass it to the objective function
            // Convert to centimeters for better algorithm response
            float c7Disp = 100*c7Evaluator.EvaluatePerformance();
            float shDisp = 100*shEvaluator.EvaluatePerformance() - c7Disp;
            float[] u = { shDisp, c7Disp };
            //Debug.Log("Trunk: " + c7Disp); // Test
            //Debug.Log("Shoulder: " + shDisp); // Test
            return objectiveFunction.Update(new List<float>(u));
        }
    }
}