using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public interface IEstimator
    {
        /// <summary>
        /// Sets the estimator working frequencies 'wo' in rad/s.
        /// </summary>
        /// <param name="wo">The estimator frequencies in rad/s.</param>
        void SetEstimatorFrequencies(float[] wo);
        /// <summary>
        /// Sets the estimator gains 'L'.
        /// </summary>
        /// <param name="L">The estimator gains.</param>
        void SetGains(float[] L);
        /// <summary>
        /// Sets the estimator's sampling time 'ts'.
        /// </summary>
        /// <param name="ts">The estimator sample time.</param>
        void SetSamplingTime(float ts);
        /// <summary>
        /// Updates the state estimation with the given input 'u' and time/iteration 't'.
        /// </summary>
        /// <param name="u">The input signal.</param>
        /// <param name="t">Current system time/iteration.</param>
        /// <returns>True if an estimate is succesfully updated.</returns>
        bool Update(float u, float t);
        /// <summary>
        /// Returns the current value of the state estimate in the given channel.
        /// Call Update beforehand to get an updated state estimate.
        /// </summary>
        /// <param name="channel">The state channel number.</param>
        /// <returns>The current state estimate.</returns>
        float GetEstimate(int channel);
        /// <summary>
        /// Returns the current value of the all state estimates.
        /// Call Update beforehand to get an updated state estimate.
        /// </summary>
        /// <returns>An array with all current state estimates.</returns>
        float[] GetAllEstimates();
        /// <summary>
        /// Returns the number of states available.
        /// </summary>
        /// <returns>The number of states.</returns>
        int GetStatesNumber();
        /// <summary>
        /// Resets the state of the estimator.
        /// </summary>
        void Reset();
    }
}
