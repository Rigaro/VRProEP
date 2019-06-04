using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// Abstract class that defines the structure of a discrete Fast Extremum Seeking algorithm.
    /// Implementation based on:
    /// Garcia-Rosas, Ricardo, et al. "Personalized On-line Adaptation of Kinematic Synergies for Human-Prosthesis Interfaces." 
    /// IEEE Transactions on Cybernetics (2019).
    /// Moase, William H., Chris Manzie, and Michael J. Brear. "Newton-like extremum-seeking part I: Theory." 
    /// Proceedings of the 48h IEEE Conference on Decision and Control (CDC) held jointly with 2009 28th Chinese Control Conference. IEEE, 2009.
    /// </summary>
    public abstract class FastExtremumSeeker : IAdaptationManager
    {
        protected IFilter filter;
        protected IEstimator estimator;
        protected IOptimiser optimiser;
        protected IDitherGenerator dither;
        protected float theta;
        protected float thetaMin;
        protected float thetaMax;

        /// <summary>
        /// Sets the limits for the parameter that is being adapted.
        /// </summary>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        public void SetParameterLimits(float min, float max)
        {
            thetaMin = min;
            thetaMax = max;
        }
        /// <summary>
        /// Updates the parameter to be adapted acording to the given input and time/iteration.
        /// Follows a discrete-time extremum seeking adaptation law.
        /// </summary>
        /// <param name="u">The input signal.</param>
        /// <param name="t">The current time/iteration.</param>
        /// <returns>The updated parameter value.</returns>
        public float UpdateParameter(float u, float t)
        {
            // Prepare variables
            float uf; // filtered input
            float thetaBar = 0.0f; // Parameter estimate

            // Filter input signal
            uf = filter.Update(u);

            // Get derivatives estimates
            float[] duArr = estimator.Update(uf, t);

            // Update parameter estimate
            thetaBar = optimiser.Update(duArr);

            // Add excitation dither
            theta = thetaBar + dither.Update(t);

            // Clamping
            if (theta > thetaMax) theta = thetaMax;
            else if (theta < thetaMin) theta = thetaMin;

            return theta;
        }
        /// <summary>
        /// Returns the current parameter value.
        /// </summary>
        /// <returns>The current parameter value.</returns>
        public float GetParameter()
        {
            return theta;
        }
        /// <summary>
        /// Resets the adaptation algorithm.
        /// </summary>
        public void Reset()
        {
            theta = 0.0f;
            estimator.Reset();
            optimiser.Reset();
        }
    }
}
