using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRProEP.Utilities;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// An 'N' degree digital filter through its state-space representation.
    /// SISO.
    /// </summary>
    public class NDegreeSSDFilter : IFilter
    {
        private float[] states;
        private float[][] A;
        private float[] B;
        private float[] C;
        private float D;

        public NDegreeSSDFilter(float[][] A, float[] B, float[] C, float D)
        {
            // Check nulls
            if (A == null || B == null || C == null)
                throw new System.ArgumentNullException();
            // Check the size of A and vectors
            if (A.Length != A[0].Length)
                throw new System.ArgumentException("A matrix should be square.");
            if (A.Length != B.Length || A.Length != C.Length)
                throw new System.ArgumentException("A, B, and C should have the same 'Length'.");

            // Initialise states
            states = new float[A.Length];
            // Set matrices
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        /// <summary>
        /// Updates the filter output for given 'u' input.
        /// </summary>
        /// <param name="u">The signal to be filtered</param>
        /// <returns>The filtered signal 'y'.</returns>
        public float Update(float u)
        {
            // Compute next states x_1 = A*x + B*u
            float[] states_1 = Math.VectorAddition(Math.MatrixVectorMultiplication(A, states), Math.VectorScalarMultiplication(B, u));
            // Compute output y = C*x + D*u
            float y = Math.VectorDotProduct(C, states) + D*u;
            // Update states
            states = states_1;
            // Return filteres signal
            return y;
        }

        /// <summary>
        /// Clear's the filter's buffer.
        /// </summary>
        public void Reset()
        {
            states = new float[A.Length];
        }


        /// <summary>
        /// NOT NEEDED FOR MOVING AVERAGE FILTER.
        /// </summary>
        /// <param name="wo">The filter cut-off frequency 'wo' in rad/s.</param>
        public void SetCutOffFrequency(float wo)
        {
        }
        /// <summary>
        /// NOT NEEDED FOR MOVING AVERAGE FILTER.
        /// </summary>
        /// <param name="G"></param>
        public void SetGain(float g)
        {
        }
        /// <summary>
        /// NOT NEEDED FOR MOVING AVERAGE FILTER.
        /// </summary>
        /// <param name="ts">The filter (system) sampling time.</param>
        public void SetSamplingTime(float ts)
        {
        }

    }
}

