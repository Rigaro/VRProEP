//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections;
using System.Collections.Generic;

namespace VRProEP.ProsthesisCore
{

    public class SingleInputManager : BasicInputManager
    {
        VIVEControllerManager enableControllerManager;

        /// <summary>
        /// Creates an input manager with the given sensor and reference generator.
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="refGen"></param>
        public SingleInputManager(ISensor sensor, IReferenceGenerator refGen)
        {
            this.sensorManager = sensor;
            this.referenceGenerator = refGen;
            enableControllerManager = new VIVEControllerManager();
        }

        /// <summary>
        /// Generates an updated reference for the given device channel.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>The updated reference.</returns>
        public override float GenerateReference(int channel)
        {
            // First read data from sensor
            float[] sensorData = sensorManager.GetAllProcessedData();
            // Get enable
            float enableValue = enableControllerManager.GetProcessedData(1);

            // Combine input
            List<float> input = new List<float>();
            input.Add(enableValue);
            foreach (float value in sensorData)
            {
                input.Add(value);
                //Debug.Log(emgState);
            }

            // Compute reference with that data
            return referenceGenerator.UpdateReference(channel, input.ToArray());
        }

        /// <summary>
        /// Generates the set of references needed to update all devices.
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
