using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.Utilities;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// Observer-based gradient and curvature estimator. Discretisation of observer presented in:
    /// (Gradient)
    /// Bishop, Brett J., William H. Moase, and Chris Manzie. "Gain Selection in Observer-based Extremum Seeking Schemes." 
    /// IFAC Proceedings Volumes 44.1 (2011): 114-119.
    /// (Gradient-Hessian)
    /// Moase, William H., Chris Manzie, and Michael J. Brear. "Newton-like extremum-seeking part I: Theory." 
    /// Proceedings of the 48h IEEE Conference on Decision and Control (CDC) 
    /// held jointly with 2009 28th Chinese Control Conference. IEEE, 2009.
    /// </summary>
    public class GradientHessianObserver : IEstimator
    {
        public enum ObserverType
        {
            Gradient,
            GradientHessian
        }

        private ObserverType type;
        private float a; // Dither amplitude
        private float[] wo; // Estimator frequencies, well bellow wd, see Garcia-Rosas ACC 2018.
        private float[] L; // Observer gain
        private float ts;
        //private float k = 1.0f; // Integrator gain
        private float[] duHat;
        // Observer states
        private float[] xHat;
        // Observer C gain
        private float[] C;
        // Observer A matrix
        private float[][] A;
        // Dithers
        private List<SinusoidalDither> dithers = new List<SinusoidalDither>(4);

        /// <summary>
        /// Creates an observer-based gradient and curvature estimator with the default discretisation used in VRProEP
        /// (iteration domain: ts = 1), with the given dither frequency and observer gain.
        /// </summary>
        /// <param name="wo">The dither frequency in rad/s.</param>
        /// <param name="L">The observer gain vector (in R^3)</param>
        /// <param name="type">The type of observer.</param>
        /// <param name="a">The amplitude of dither frequency used for excitation.</param>
        public GradientHessianObserver(float[] wo, float[] L, ObserverType type, float a = 1.0f)
        {
            this.type = type;
            this.a = a;
            SetSamplingTime(1.0f);
            // Set parameters and do checks.
            SetEstimatorFrequencies(wo);
            SetGains(L);

            // Add common dithers
            dithers.Add(new SinusoidalDither(1.0f, wo[0], 0.0f));
            dithers.Add(new SinusoidalDither(1.0f, wo[0], Mathf.PI / 2.0f));
            
            // Handle type init
            switch (this.type)
            {
                //
                // Gradient
                //
                case ObserverType.Gradient:
                    // Estimates
                    duHat = new float[1] { 0.0f };
                    // Observer states
                    xHat = new float[3] { 0.0f, 0.0f, 0.0f };
                    // Observer C gain
                    C = new float[3] { 1.0f, 1.0f, 0.0f };
                    // Observer A matrix
                    A = new float[3][];
                    A[0] = new float[3] { 1.0f, 0.0f, 0.0f };
                    A[1] = new float[3] { 0.0f, 0.5403f, 0.8415f };
                    A[2] = new float[3] { 0.0f, -0.8415f, 0.5403f };
                    break;
                //
                // Gradient-Hessian
                //
                case ObserverType.GradientHessian:
                    // Estimates
                    duHat = new float[2] { 0.0f, 0.0f };
                    // Observer states                    
                    xHat = new float[5] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
                    // Observer C gain
                    C = new float[5] { 1.0f, 1.0f, 0.0f, 0.0f, -0.25f };
                    // Observer A matrix
                    A = new float[5][];
                    A[0] = new float[5] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f };
                    A[1] = new float[5] { 0.0f, 0.5403f, 0.8415f, 0.0f, 0.0f };
                    A[2] = new float[5] { 0.0f, -0.8415f, 0.5403f, 0.0f, 0.0f };
                    A[3] = new float[5] { 0.0f, 0.0f, 0.0f, -0.4161f, 0.9093f };
                    A[4] = new float[5] { 0.0f, 0.0f, 0.0f, -0.9093f, -0.4161f };
                    // Add additional dithers.
                    dithers.Add(new SinusoidalDither(1.0f, wo[1], 0.0f));
                    dithers.Add(new SinusoidalDither(1.0f, wo[1], Mathf.PI / 2.0f));
                    break;
                default:
                    throw new System.Exception("Invalid observer type.");
            }
        }

        /// <summary>
        /// Creates an observer-based gradient and curvature estimator with the default discretisation used in VRProEP
        /// (iteration domain: ts = 1), with the given dither frequency and default dead-beat observer gain.
        /// </summary>
        /// <param name="wo">The dither frequency in rad/s.</param>
        /// <param name="type">The type of observer.</param>
        /// <param name="a">The amplitude of dither frequency used for excitation.</param>
        public GradientHessianObserver(float[] wo, ObserverType type, float a = 1.0f)
        {
            this.type = type;
            this.a = a;
            SetSamplingTime(1.0f);
            // Set parameters and do checks.
            SetEstimatorFrequencies(wo);
            float[] L;

            // Add common dithers
            dithers.Add(new SinusoidalDither(1.0f, wo[0], 0.0f)); //Sin
            dithers.Add(new SinusoidalDither(1.0f, wo[0], Mathf.PI / 2.0f)); //Cos

            // Handle type init
            switch (this.type)
            {
                //
                // Gradient
                //
                case ObserverType.Gradient:
                    // Estimates
                    duHat = new float[1] { 0.0f };
                    // Observer states
                    xHat = new float[3] { 0.0f, 0.0f, 0.0f };
                    // Observer C gain
                    C = new float[3] { 1.0f, 1.0f, 0.0f };
                    // Observer A matrix
                    A = new float[3][];
                    A[0] = new float[3] { 1.0f, 0.0f, 0.0f };
                    A[1] = new float[3] { 0.0f, 0.5403f, 0.8415f };
                    A[2] = new float[3] { 0.0f, -0.8415f, 0.5403f };
                    // Observer gain
                    L = new float[3] { 1.0877f, 0.9929f, 0.7417f };
                    SetGains(L);
                    break;
                //
                // Gradient-Hessian
                //
                case ObserverType.GradientHessian:
                    // Estimates
                    duHat = new float[2] { 0.0f, 0.0f };
                    // Observer states                    
                    xHat = new float[5] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
                    // Observer C gain
                    C = new float[5] { 1.0f, 1.0f, 0.0f, 0.0f, -0.25f };
                    // Observer A matrix
                    A = new float[5][];
                    A[0] = new float[5] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f };
                    A[1] = new float[5] { 0.0f, 0.5403f, 0.8415f, 0.0f, 0.0f };
                    A[2] = new float[5] { 0.0f, -0.8415f, 0.5403f, 0.0f, 0.0f };
                    A[3] = new float[5] { 0.0f, 0.0f, 0.0f, -0.4161f, 0.9093f };
                    A[4] = new float[5] { 0.0f, 0.0f, 0.0f, -0.9093f, -0.4161f };
                    // Observer gain
                    L = new float[5] { 0.3840f, 0.6067f, -0.2273f, -0.8977f, -1.0302f };
                    SetGains(L);
                    // Add additional dithers.
                    dithers.Add(new SinusoidalDither(1.0f, wo[1], 0.0f)); //Sin
                    dithers.Add(new SinusoidalDither(1.0f, wo[1], Mathf.PI / 2.0f)); //Cos
                    break;
                default:
                    throw new System.Exception("Invalid observer type.");
            }
        }

        /// <summary>
        /// Sets the estimator working frequencies 'wo' in rad/s.
        /// </summary>
        /// <param name="wo">The estimator frequencies in rad/s.</param>
        public void SetEstimatorFrequencies(float[] wo)
        {
            // Check parameters
            if (wo == null || (type == ObserverType.Gradient && wo.Length != 1) || (type == ObserverType.GradientHessian && wo.Length != 2))
                throw new System.ArgumentException("The following number of frequencies are used per type. Gradient: 1. GradientHessian: 2.");
            if (wo[0] <= 0)
                throw new System.ArgumentOutOfRangeException("The estimator frequency should be greater than 0.");
            if (type == ObserverType.GradientHessian && wo[1] <= 0)
                throw new System.ArgumentOutOfRangeException("The estimator frequency should be greater than 0.");

            this.wo = wo;
        }
        /// <summary>
        /// Sets the estimator gains 'L'.
        /// </summary>
        /// <param name="L">The estimator gains.</param>
        public void SetGains(float[] L)
        {
            if (L == null || (type == ObserverType.Gradient && L.Length != 3) || (type == ObserverType.GradientHessian && L.Length != 5))
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
            // First, multiply the previous states estimate xHat times C.
            // And remove it from the input u to get estimation error 'uTilde'.
            float uTilde = u - Math.VectorMultiplication(C, xHat);
            //Debug.Log(uTilde);
            // Now multiply error by the gain L,
            // multiply the previous states estimate xHat times A.
            // And add them together.
            float[] v = Math.VectorAddition(Math.VectorScalarMultiplication(L, uTilde), Math.MatrixVectorMultiplication(A, xHat));
            // Now multiply by the estimator frequency to get updated states.
            xHat = Math.VectorScalarMultiplication(v, wo[0]);

            // Update estimates
            float[] Cd;
            switch (type)
            {
                //
                // Gradient
                //
                case ObserverType.Gradient:
                    // Update the dither derivative C' and create a vector with it.
                    Cd = new float[3] { 0.0f, dithers[0].Update(t), dithers[1].Update(t) };
                    // And finally multiply it by xHat to get du.
                    duHat[0] = Math.VectorMultiplication(Cd, xHat);
                    break;
                //
                // Gradient-Hessian
                //
                case ObserverType.GradientHessian:
                    // Update the dither derivatives C' and C'' and create vectors with them.
                    Cd = new float[5] { 0, dithers[0].Update(t), dithers[1].Update(t), 0, 0 };
                    float[] Cdd = new float[5] { 0, 0, 0, dithers[2].Update(t), dithers[3].Update(t) };
                    // And finally multiply it by xHat to get du and d2u.
                    duHat[0] = Math.VectorMultiplication(Cd, xHat) / a;
                    duHat[1] = Math.VectorMultiplication(Cdd, xHat) / (a * a);
                    break;
                default:
                    throw new System.Exception("Invalid observer type.");
            }
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
            if (channel >= 0 && (type == ObserverType.Gradient && channel < 1 || type == ObserverType.GradientHessian && channel < 2)) return duHat[channel];
            else throw new System.ArgumentOutOfRangeException("Invalid channel.");
        }
        /// <summary>
        /// Returns the current value of the all state estimates.
        /// Call Update beforehand to get an updated state estimate.
        /// </summary>
        /// <returns>An array with all current state estimates.</returns>
        public float[] GetAllEstimates()
        {
            return duHat;
        }

        /// <summary>
        /// Returns the number of states available.
        /// </summary>
        /// <returns>The number of states.</returns>
        public int GetStatesNumber()
        {
            if (type == ObserverType.Gradient) return 1;
            else if (type == ObserverType.GradientHessian) return 2;
            else throw new System.Exception("Invalid observer type.");
        }
        /// <summary>
        /// Resets the state of the estimator.
        /// </summary>
        public void Reset()
        {
            if (type == ObserverType.Gradient)
            {
                // Estimates
                duHat = new float[1] { 0.0f };
                // Observer states
                xHat = new float[3] { 0.0f, 0.0f, 0.0f };
            }
            else if (type == ObserverType.GradientHessian)
            {
                // Estimates
                duHat = new float[2] { 0.0f, 0.0f };
                // Observer states
                xHat = new float[5] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
            }
            else throw new System.Exception("Invalid observer type.");
        }
    }
}