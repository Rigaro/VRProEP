using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// Performance Evaluator: Maximum Displacement.
    /// Takes a list of positions represented as a Vector3.
    /// Calculates the total displacement from the first position in the list.
    /// Performance is given by the max displacement from the start position.
    /// </summary>
    public class MaxDisplacementPE : IPerformanceEvaluator<Vector3>
    {
        private List<Vector3> positionVectors;
        private List<float> displacements;

        /// <summary>
        /// Creates a performance evaluator that calculates the maximum differential displacement
        /// in a list of vectors.
        /// </summary>
        public MaxDisplacementPE()
        {
            positionVectors = new List<Vector3>();
            displacements = new List<float>();
        }

        /// <summary>
        /// Appends displacement data for the given marker.
        /// </summary>
        /// <param name="data"> The x, y, z data displacement vector. </param>
        public void AppendData(List<Vector3> data)
        {
            // make sure the lists are initialised
            if (displacements == null)
            {
                positionVectors = new List<Vector3>();
                displacements = new List<float>();
            }
            if (data == null)
                throw new System.ArgumentNullException("The provided data list is empty");

            // Append data when the data is correct.
            positionVectors.AddRange(data);
        }

        /// <summary>
        /// Clears the performance evaluator data memory.
        /// </summary>
        public void ClearData()
        {
            // Do nothing if not yet initialised
            if (displacements == null) return;

            positionVectors.Clear();
            displacements.Clear();
        }
        /// <summary>
        /// Sets the number of data-sets to be used for performance evaluation.
        /// </summary>
        /// <param name="size"></param>
        public void SetBufferSize(int size)
        {
            // No buffer limit as this performance evaluator calculates the maximum displacement from the given vector.
            throw new System.Exception("No buffer limit as this performance evaluator calculates the maximum displacement from the given vector.");
        }
        /// <summary>
        /// Calculates the performance with the current data-sets available.
        /// </summary>
        /// <returns> The updated performance. </returns>
        public float EvaluatePerformance()
        {
            // make sure the array is initialised
            if (positionVectors == null)
                throw new System.NullReferenceException("Data has not been added for evaluation");

            // Obtain the maximum value for displacement in the dataset.
            return GetMaxDisplacement();
        }

        /// <summary>
        /// Calculates the maximum differential displacement for the active dataset.
        /// </summary>
        /// <returns></returns>
        private float GetMaxDisplacement()
        {
            positionVectors.ForEach(CalculateDisplacement);
            return displacements.Max();
        }

        private void CalculateDisplacement(Vector3 vector)
        {
            // Calculate the displacement from the start position
            Vector3 diffPosition = vector - positionVectors[0];
            displacements.Add(diffPosition.magnitude);
        }
    }
}
