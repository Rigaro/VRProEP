using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    public class GradientDemodulator : IEstimator
    {
        private float[] wo; // Estimator frequencies, well bellow wd, see Garcia-Rosas ACC 2018.
        private float[] L;
        private float ts;
        private float x = 0.0f;
        private FOHPDFilter hpf;
        private FOLPDFilter lpf;
        private SinusoidalDither dither;

        /// <summary>
        /// Creates a basic first order gradient estimator using a demodulation method with first order high and low pass filters.
        /// Uses a sinusoidal dither signal for demodulation.
        /// </summary>
        /// <param name="a">The dither signal amplitude.</param>
        /// <param name="wd">The dither frequency (rad/s), must satisfy certain conditions, see Garcia-Rosas ACC 2018.</param>
        /// <param name="phi">The dither signal phase shift.</param>
        /// <param name="wo">Estimator frequency, well bellow wd, see Garcia-Rosas ACC 2018.</param>
        /// <param name="L">Estimator gains, one dimensional for this type.</param>
        /// <param name="ts">The estimator (system) sampling time.</param>
        public GradientDemodulator(float a, float wd, float phi, float wo, float[] L, float ts)
        {
            float[] woArr = { wo };
            SetEstimatorFrequencies(woArr);
            SetGains(L);
            SetSamplingTime(ts);
            dither = new SinusoidalDither(a, wd, phi);
            hpf = new FOHPDFilter(wo, L[0], ts);
            lpf = new FOLPDFilter(wo, L[0], ts);
        }

        /// <summary>
        /// Creates a basic first order gradient estimator using a demodulation method with first order high and low pass filters.
        /// Uses a sinusoidal dither signal for demodulation.
        /// Creates the filters with cutt-off frequency of wd/10.
        /// </summary>
        /// <param name="wd">The dither frequency (rad/s), must satisfy certain conditions, see Garcia-Rosas ACC 2018.</param>
        /// <param name="L"></param>
        /// <param name="ts"></param>
        public GradientDemodulator(float wd, float[] L, float ts)
        {
            float[] woArr = { wd };
            SetEstimatorFrequencies(woArr);
            SetGains(L);
            SetSamplingTime(ts);
            dither = new SinusoidalDither(1.0f, wd, 0.0f);
            hpf = new FOHPDFilter(wd / 10.0f, L[0], ts);
            lpf = new FOLPDFilter(wd / 10.0f, L[0], ts);
        }

        /// <summary>
        /// Sets the estimator working frequencies 'wo' in rad/s.
        /// </summary>
        /// <param name="wo">The estimator frequencies in rad/s.</param>
        public void SetEstimatorFrequencies(float[] wo)
        {
            // Check parameters
            if (wo == null || wo.Length != 1)
                throw new System.ArgumentException("A gradient demodulator uses a single operating frequency.");
            if (wo[0] <= 0)
                throw new System.ArgumentOutOfRangeException("The estimator frequency should be greater than 0.");

            this.wo = wo;
        }
        /// <summary>
        /// Sets the estimator gains 'L'.
        /// </summary>
        /// <param name="L">The estimator gains.</param>
        public void SetGains(float[] L)
        {
            if (L == null || L.Length != 1)
                throw new System.ArgumentException("The provided gain vector is empty or invalid.");
            this.L = L;
        }
        /// <summary>
        /// Sets the estimator's sampling time 'ts'.
        /// </summary>
        /// <param name="ts">The estimator sample time.</param>
        public void SetSamplingTime(float ts)
        {
            if (ts <= 0)
                throw new System.ArgumentOutOfRangeException("Sampling time should be greater than 0.");
            this.ts = ts;
        }
        /// <summary>
        /// Updates the state estimation with the given input 'u' and time/iteration 't'.
        /// </summary>
        /// <param name="u">The input signal.</param>
        /// <param name="t">Current system time/iteration.</param>
        /// <returns>True if an estimate is succesfully updated.</returns>
        public bool Update(float u, float t)
        {
            x = lpf.Update(dither.Update(t) * hpf.Update(u));
            return true;

        }

        /// <summary>
        /// Returns the current value of the state estimate in the given channel.
        /// Call Update beforehand to get an updated state estimate.
        /// </summary>
        /// <param name="channel">The state channel number.</param>
        /// <returns>The current state estimate.</returns>
        public float GetEstimate(int channel)
        {
            if (channel == 0) return x;
            else throw new System.ArgumentOutOfRangeException("Only channel 0 available.");
        }
        /// <summary>
        /// Returns the current value of the all state estimates.
        /// Call Update beforehand to get an updated state estimate.
        /// </summary>
        /// <returns>An array with all current state estimates.</returns>
        public float[] GetAllEstimates()
        {
            float[] duArr = { x };
            return duArr;
        }

        /// <summary>
        /// Returns the number of states available.
        /// </summary>
        /// <returns>The number of states.</returns>
        public int GetStatesNumber()
        {
            return 1;
        }
        /// <summary>
        /// Resets the state of the estimator.
        /// </summary>
        public void Reset()
        {
            // This type of estimator does not have states.
            return;
        }
    }
}