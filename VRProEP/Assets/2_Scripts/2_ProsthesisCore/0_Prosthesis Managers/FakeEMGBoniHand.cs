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
        private BoniManager boniManager;

        private bool isConfigured = false;
        private bool isEnabled = false;

        public bool IsEnabled { get => isEnabled; }
        public float RoughnessValue { get => roughnessValue; }

        private float handState = 0.0f;
        private float roughnessValue = 0.0f;
        
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
            emgGains.Add(-0.005f);
            EMGInterfaceReferenceGenerator emgRG = new EMGInterfaceReferenceGenerator(xBar, xMin, xMax, emgGains, EMGInterfaceType.dualSiteProportional);

            inputManager = new SingleInputManager(emgSensor, emgRG);
            
            // Configure the grasp manager
            GameObject graspManagerGO = GameObject.FindGameObjectWithTag("GraspManager");
            if (graspManagerGO == null)
                throw new System.Exception("Grasp Manager not found.");
            GraspManager graspManager = graspManagerGO.GetComponent<GraspManager>();
            graspManager.managerType = GraspManager.GraspManagerType.Assisted;
            graspManager.managerMode = GraspManager.GraspManagerMode.Restriced;

            // Create fake hand and add as user sensor
            handManager = new FakeTactileHand(graspManager);
            AvatarSystem.AddActiveSensor(handManager);

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

                // Get roughness data
                roughnessValue = handManager.GetProcessedData("roughness");

                // Set enable
                isEnabled = inputManager.IsEnabled();
            }
        }
    }
}