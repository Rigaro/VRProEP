using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.AdaptationCore
{
    /// <summary>
    /// Extremum Seeking-based synergistic human-prosthetic interface personalisation algorithm. Based on:
    /// Garcia-Rosas, Riccardo, et al. "On-line synergy identification for personalized active arm prosthesis: a feasibility study." 
    /// 2018 Annual American Control Conference (ACC). IEEE, 2018.
    /// </summary>
    public class ACCHPIPersonalisation : ExtremumSeeker
    {
        /// <summary>
        /// Extremum Seeking-based synergistic human-prosthetic interface personalisation algorithm. Based on:
        /// Garcia-Rosas, Riccardo, et al. "On-line synergy identification for personalized active arm prosthesis: a feasibility study." 
        /// 2018 Annual American Control Conference (ACC). IEEE, 2018.
        /// </summary>
        /// <param name="a">The dither signal amplitude.</param>
        /// <param name="wd">The dither signal frequency (rad/s).</param>
        /// <param name="phi">The dither signal phase shift (rad).</param>
        /// <param name="wo"> The estimator frequency, well bellow wd, see Garcia-Rosas ACC 2018.</param>
        /// <param name="L">The gradient estimator gain (one dimensional in this case).</param>
        /// <param name="ts">The system sampling time (sec).</param>
        /// <param name="k"> The optimiser gain. </param>
        /// <param name="theta_0"> The HPI parameter initial condition. </param>
        /// <param name="thetaMin"> The lower bound of the HPI parameter. </param>
        /// <param name="thetaMax"> The upper bound of the HPI parameter. </param>
        public ACCHPIPersonalisation(float a, float wd, float phi, float wo, float[] L, float ts, float k, float theta_0, float thetaMin, float thetaMax)
        {
            estimator = new GradientDemodulator(a, wd, phi, wo, L, ts);
            optimiser = new GradientOptimiser(k, ts, theta_0);
            dither = new SinusoidalDither(a, wd, phi);
            SetParameterLimits(thetaMin, thetaMax);
        }
    }
}