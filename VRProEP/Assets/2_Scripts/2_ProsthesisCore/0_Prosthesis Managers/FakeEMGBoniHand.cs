using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRProEP.GameEngineCore;

namespace VRProEP.ProsthesisCore
{
    public class FakeEMGBoniHand : MonoBehaviour
    {
        private SingleInputManager inputManager;
        private FakeTactileHand handManager;

        private bool isConfigured = false;

        private float handState = 0.0f;

        /// <summary>
        /// Initializes the hand prosthesis with basic functionality.
        /// Must be called only after the avatar is available.
        /// </summary>
        public void InitializeProsthesis()
        {
            // Get available sensors from avatar system
            List<ISensor> activeSensors = AvatarSystem.GetActiveSensors();
            if (activeSensors == null || activeSensors.Count == 0)
                throw new System.Exception("There are no user sensors available.");

            // Extract EMG sensor
            EMGWiFiManager emgSensor = null;
            foreach (ISensor sensor in activeSensors)
            {
                //Debug.Log(sensor.GetSensorType());
                if (sensor.GetSensorType().Equals(SensorType.EMGWiFi))
                    emgSensor = (EMGWiFiManager)sensor;
            }
            if (emgSensor == null)
                throw new System.Exception("No EMG sensor available.");

            // Add an EMG reference generator
            float[] xBar = { 0.0f };
            float[] xMin = { 0.0f };
            float[] xMax = { 1.0f };
            List<float> emgGains = new List<float>(1);
            emgGains.Add(0.015f);
            EMGInterfaceReferenceGenerator emgRG = new EMGInterfaceReferenceGenerator(xBar, xMin, xMax, emgGains, EMGInterfaceType.dualSiteProportional);

            inputManager = new SingleInputManager(emgSensor, emgRG);

            // Create fake hand
            handManager = new FakeTactileHand();

            isConfigured = true;
        }

        // Update the prosthesis state deterministically
        public void FixedUpdate()
        {
            if (isConfigured)
            {
                // Update references
                handState = inputManager.GenerateReference(0);
                // Update device state
                handManager.UpdateState(0, handState);
            }
        }
    }
}