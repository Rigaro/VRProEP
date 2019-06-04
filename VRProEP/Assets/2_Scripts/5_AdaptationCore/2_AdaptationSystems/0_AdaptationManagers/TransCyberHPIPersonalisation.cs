using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// Fast Extremum Seeking-based synergistic human-prosthetic interface personalisation algorithm. Based on:
    /// Garcia-Rosas R., Tan Y., Oetomo D., Manzie C., Choong P. "Personalized On-line Adaptation of 
    /// Kinematic Synergies for Human-Prosthesis Interfaces". Transactions on Cybernetics. 2019.
    /// </summary>
    public class TransCyberHPIPersonalisation : FastExtremumSeeker
    {
        /// <summary>
        /// Fast Extremum Seeking-based synergistic human-prosthetic interface personalisation algorithm. Based on:
        /// Garcia-Rosas R., Tan Y., Oetomo D., Manzie C., Choong P. "Personalized On-line Adaptation of 
        /// Kinematic Synergies for Human-Prosthesis Interfaces". Transactions on Cybernetics. 2019.
        /// </summary>
        /// <param name="a"> The dither signal amplitude. </param>
        /// <param name="phi"> The dither signal phase shift (rad) .</param>
        /// <param name="wo"> The excitation and estimation dither vector frequencies. [0]: excitation dither and gradient estimation, 
        /// [1]: hessian estimation. From paper: [0]: wd, [1]: 2*wd. </param>
        /// <param name="L"> The observer gain. </param>
        /// <param name="ts"> The system sampling time (sec). </param>
        /// <param name="k"> The optimiser gain. </param>
        /// <param name="epsilon"> The optimiser switching threshold. </param>
        /// <param name="A"> The filter A matrix (state-space representation). </param>
        /// <param name="B"> The filter B matrix (state-space representation). </param>
        /// <param name="C"> The filter C matrix (state-space representation). </param>
        /// <param name="D"> The filter D matrix (state-space representation). </param>
        /// <param name="theta_0"> The HPI parameter initial condition. </param>
        /// <param name="thetaMin"> The lower bound of the HPI parameter. </param>
        /// <param name="thetaMax"> The upper bound of the HPI parameter. </param>
        public TransCyberHPIPersonalisation(float a, float phi, float[] wo, float[] L, float ts, float k, float epsilon, float[][] A, float[] B, float[] C, float D, float theta_0, float thetaMin, float thetaMax)
        {
            filter = new NDegreeSSDFilter(A, B, C, D);
            estimator = new GradientHessianObserver(wo, L, GradientHessianObserver.ObserverType.GradientHessian, a);
            optimiser = new SGNOptimiser(k, wo[0], epsilon, theta_0);
            dither = new SinusoidalDither(a, wo[0], phi);
            SetParameterLimits(thetaMin, thetaMax);
        }
    }
}