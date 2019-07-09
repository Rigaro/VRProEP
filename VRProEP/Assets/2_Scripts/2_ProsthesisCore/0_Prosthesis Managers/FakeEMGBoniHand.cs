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
        private bool hasFeedback = false;

        public bool IsEnabled { get => isEnabled; }
        public float RoughnessValue { get => roughnessValue; }
        public bool HasFeedback { get => hasFeedback; }

        private float handState = 0.0f;
        private float roughnessValue = 0.0f;
        private float enableFeedback = 0.0f;
        
        /// <summary>
        /// Initializes the hand prosthesis with basic functionality.
        /// Must be called only after the avatar is available.
        /// </summary>
        public void InitializeProsthesis()
        {
            // Make sure the input system has been initialised
            if (inputManager == null)
                throw new System.Exception("The input system has not been set. Do this by calling InitialiseInputSystem.");

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

                if (handState > 0)
                    enableFeedback = 1.0f;
                else
                    enableFeedback = 0.0f;

                // Update feedback when available
                if (hasFeedback)
                {
                    float[] sensorData = { roughnessValue, handState, enableFeedback };
                    //Debug.Log("Force: " + handState + ", Roughness: " + roughnessValue);
                    boniManager.UpdateFeedback(0, sensorData);
                }
            }
        }

        /// <summary>
        /// Reset the Force applied by the hand back to 0.
        /// </summary>
        public void ResetForce()
        {
            inputManager.ResetReference(0);
        }
        
        /// <summary>
        /// Stops connection to Boni when available.
        /// </summary>
        public void StopBoniConnection()
        {
            if (hasFeedback)
                boniManager.StopBoniConnection();
        }

        /// <summary>
        /// Initialised the prosthesis' input system.
        /// </summary>
        /// <param name="emgSensor">The EMG sensor to initialise the input system with.</param>
        public void InitialiseInputSystem(EMGWiFiManager emgSensor)
        {
            // Check that the sensor a valid sensor is provided
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
        }

        /// <summary>
        /// Initialises the Boni feedback system.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void InitialiseFeedbackSystem(string ipAddress, int port)
        {
            // Add Feedback
            float[] xBarFB = new float[] { 100.0f, 0.0f, 0.0f };
            BoneConductionController boniController = new BoneConductionController(ipAddress, port, 1, 1, 1);
            BoneConductionCharacterization userBoniCharac = (BoneConductionCharacterization)SaveSystem.LoadFeedbackCharacterization(SaveSystem.ActiveUser.id, FeedbackType.BoneConduction);
            BoneConductionReferenceGenerator boniRG = new BoneConductionReferenceGenerator(xBarFB, userBoniCharac);
            boniManager = new BoniManager(boniController, boniRG);
            hasFeedback = true;
        }
    }
}