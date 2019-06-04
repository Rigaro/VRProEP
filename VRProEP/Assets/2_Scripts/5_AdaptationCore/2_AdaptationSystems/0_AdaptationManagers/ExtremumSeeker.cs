using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// Abstract class that defines the structure of a discrete-time Extremum Seeking algorithm.
    /// Implementation based on:
    /// Ariyur, Kartik B., and Miroslav Krstić. Real time optimization by extremum seeking control. 
    /// Hoboken: Wiley Interscience, 2003.
    /// </summary>
    public abstract class ExtremumSeeker : IAdaptationManager
    {
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
            float thetaBar = 0.0f; // Parameter estimate

            // Get gradient estimate
            float[] duArr = estimator.Update(u, t);

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
