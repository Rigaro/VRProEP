using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Input manager that includes a single VIVE Tracker as sensor and a linear kinematic synergy as reference generator.
    /// </summary>
    public class KinematicVIVEInputManager : BasicInputManager
    {
        /// <summary>
        /// Input manager that includes a single VIVE Tracker as sensor and a linear kinematic synergy as reference generator.
        /// </summary>
        /// <param name="sensorManager">The VIVE Tracker object to be used as sensor.</param>
        public KinematicVIVEInputManager(VIVETrackerManager sensorManager)
        {
            if (sensorManager == null)
                throw new System.ArgumentNullException("The provided sensor manager is empty.");

            this.sensorManager = sensorManager;
            // Create a single DOF KSRG
            float[] xBar = { 0.0f };
            float[] xMin = { -5.0f };
            float[] xMax = { 145.0f };
            float[] theta = { 1.0f };
            float[] thetaMin = { 0.8f };
            float[] thetaMax = { 2.4f };
            referenceGenerator = new LinearKinematicSynergy(xBar, xMin, xMax, theta, thetaMin, thetaMax);
        }

        /// <summary>
        /// Input manager that includes a single VIVE Tracker as sensor and a linear kinematic synergy as reference generator.
        /// </summary>
        /// <param name="sensorManager">The VIVE Tracker object to be used as sensor.</param>
        /// <param name="xBar">The initial condition for the references.</param>
        /// <param name="xMin">The lower limits for the references.</param>
        /// <param name="xMax">The upper limits for the references.</param>
        public KinematicVIVEInputManager(VIVETrackerManager sensorManager, float[] xBar, float[] xMin, float[] xMax, float[] theta, float[] thetaMin, float[] thetaMax)
        {
            if (sensorManager == null)
                throw new System.ArgumentNullException("The provided sensor manager is empty.");

            this.sensorManager = sensorManager;
            // Create a custom KSRG
            referenceGenerator = new LinearKinematicSynergy(xBar, xMin, xMax, theta, thetaMin, thetaMax);
        }

        /// <summary>
        /// Generates an updated reference for the given device channel.
        /// Reads tracking data from a VIVE Tracker.
        /// Generates references using a linear kinematic synergy.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>The updated reference.</returns>
        public override float GenerateReference(int channel)
        {
            // First read the angular velocity from sensor
            float[] input = { sensorManager.GetRawData(channel) };
            // Compute reference with that angular velocity
            return referenceGenerator.UpdateReference(channel, input);
        }

        /// <summary>
        /// Generates the set of references needed to update all devices. 
        /// Reads tracking data from a VIVE Tracker.
        /// Generates references using a linear kinematic synergy.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <returns>The updated set of references.</returns>
        public override float[] GenerateAllReferences()
        {
            List<float> references = new List<float>(referenceGenerator.ChannelSize());
            // Generate all refereces
            for (int i = 0; i < referenceGenerator.ChannelSize(); i++)
            {
                references.Add(GenerateReference(i));
            }
            return references.ToArray();
        }
    }

}
