using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// A switching gradient/newton based optimiser optimiser. Use negative gain for minimisation
    /// and positive gain for maximisation.
    /// For more details see:
    /// Garcia-Rosas R., Tan Y., Oetomo D., Manzie C., Choong P. "Personalized On-line Adaptation of 
    /// Kinematic Synergies for Human-Prosthesis Interfaces". Transactions on Cybernetics.
    /// </summary>
    public class SGNOptimiser : IOptimiser
    {
        private float k;
        private float wo;
        private float e = 0.1f;
        private float x = 0.0f;
        /// <summary>
        /// A switching gradient/newton based optimiser optimiser. Use negative gain for minimisation
        /// and positive gain for maximisation.
        /// </summary>
        /// <param name="k">The optimiser gain.</param>
        /// <param name="wo">The main dither frequency.</param>
        /// <param name="e">The switchin threshold.</param>
        /// <param name="x_o">The initial condition.</param>
        public SGNOptimiser(float k, float wo, float e, float x_o)
        {
            SetGain(k);
            SetSamplingTime(wo);
            SetEpsilon(e);
            x = x_o;
        }

        /// <summary>
        /// Sets the switching threshold 'epsilon'.
        /// </summary>
        /// <param name="e">The switching threshold.</param>
        public void SetEpsilon(float e)
        {
            if (e <= 0)
                throw new System.ArgumentOutOfRangeException("Switching threshold should be greater than 0.");
            this.e = e;
        }

        /// <summary>
        /// Sets the optimiser gain.
        /// </summary>
        /// <param name="k">The optimiser gain.</param>
        public void SetGain(float k)
        {
            this.k = k;
        }
        /// <summary>
        /// OVERRIDEN TO SET DITHER FREQUENCY NEEDED IN OPTMISER.
        /// Sets the main dither frequency.
        /// </summary>
        /// <param name="wo">The main dither frequency.</param>
        public void SetSamplingTime(float wo)
        {
            if (wo <= 0)
                throw new System.ArgumentOutOfRangeException("Dither frequency should be greater than 0.");
            this.wo = wo;
        }
        /// <summary>
        /// Updates the value of the variable being optimized (theta) given a vector of derivatives (duArray).
        /// </summary>
        /// <param name="duArray">The vector of derivatives.</param>
        /// <returns>The updated variable.</returns>
        public float Update(float[] duArray)
        {
            // Check input
            if (duArray == null)
                throw new System.ArgumentNullException();
            else if (duArray.Length != 2)
                throw new System.ArgumentException("The vector of derivative should only contain the gradient and hessian.");

            // Extract gradient and hessian
            float du = duArray[0];
            float d2u = duArray[1];
            // Determine method used
            if (Mathf.Abs(du) < -e * d2u) // Newton
                x -= k * wo * du / d2u;
            else // Gradient
                x += k * wo * du;
            return x;
        }
        /// <summary>
        /// Resets the state of the optimizer.
        /// </summary>
        public void Reset()
        {
            x = 0;
        }
        /// <summary>
        /// Sets the state of the optimizer.
        /// </summary>
        public void Reset(float x_o)
        {
            x = x_o;
        }
    }
}