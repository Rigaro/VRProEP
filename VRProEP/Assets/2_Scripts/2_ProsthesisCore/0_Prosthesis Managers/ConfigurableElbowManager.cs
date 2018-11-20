//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public class ConfigurableElbowManager : MonoBehaviour
    {
        private ConfigurableInputManager inputManager;
        private ElbowManager elbowManager;

        private float elbowState = 0.0f;

        private bool isConfigured = false;

        /// <summary>
        /// Initializes the Elbow prosthesis with basic functionality.
        /// Must be called only after the avatar is available.
        /// </summary>
        public void InitializeProsthesis()
        {
            // ConfigurableInputManagar
            // Find ResdiualLimbTracker GameObject and extract its Transform.
            GameObject residualLimbTrackerGO = GameObject.FindGameObjectWithTag("ResidualLimbTracker");
            // Create a VIVETracker with the obtained transform
            VIVETrackerManager trackerManager = new VIVETrackerManager(residualLimbTrackerGO.transform);
            // Create a basic reference generator: Integrator.
            float[] xBar = { Mathf.Deg2Rad * -90.0f };
            float[] xMin = { Mathf.Deg2Rad * -145.0f };
            float[] xMax = { Mathf.Deg2Rad * 0.0f };
            IntegratorReferenceGenerator integratorRG = new IntegratorReferenceGenerator(xBar, xMin, xMax);
            // Create configurable input manager with the created sensor and RG.
            inputManager = new ConfigurableInputManager(trackerManager, integratorRG);

            // ElbowManager
            // Find Elbow_Lower GameObject and extract its HingeJoint and Rigidbody
            GameObject elbowLowerGO = GameObject.FindGameObjectWithTag("Elbow_Lower");
            if (elbowLowerGO == null)
                throw new System.Exception("Could not find and active elbow prosthesis avatar (GameObject).");
            HingeJoint elbowJoint = elbowLowerGO.GetComponent<HingeJoint>();
            Rigidbody elbowRB = elbowLowerGO.GetComponent<Rigidbody>();
            // Create VirtualEncoder and attach to HingeJoint.
            VirtualEncoderManager virtualEncoder = new VirtualEncoderManager(elbowJoint);
            // Create ElbowManager with the given VirtualEncoder and Rigidbody, and set the axis.
            elbowManager = new ElbowManager(virtualEncoder, elbowRB);
            elbowManager.Axis = elbowJoint.axis;

            // Add a LKS to the prosthesis
            float[] theta = { -2.5f };
            float[] thetaMin = { -1.0f };
            float[] thetaMax = { -3.5f };
            LinearKinematicSynergy linSyn = new LinearKinematicSynergy(xBar, xMin, xMax, theta, thetaMin, thetaMax);
            inputManager.Configure("CMD_ADD_REFGEN", linSyn);

            // Add VIVE controller as sensor to manually move.
            VIVEControllerManager controllerManager = new VIVEControllerManager();
            inputManager.Configure("CMD_ADD_SENSOR", controllerManager);

            // Enable
            isConfigured = true;
        }

        // Update the prosthesis state deterministically
        public void FixedUpdate()
        {
            if (isConfigured)
            {
                // Update references
                elbowState = inputManager.GenerateReference(1);
                Debug.Log(elbowState);
                // Update device state
                elbowManager.UpdateState(1, elbowState);
            }
        }

        /// <summary>
        /// Changes the active sensor for reference generation.
        /// Available sensors:
        /// - "VAL_SENSOR_VIVETRACKER";
        /// - "VAL_SENSOR_VIVECONTROLLER";
        /// </summary>
        /// <param name="sensorName"></param>
        public void ChangeSensor(string sensorName)
        {
            inputManager.Configure("CMD_SET_ACTIVE_SENSOR", sensorName);
        }

        /// <summary>
        /// Changes the active reference generator.
        /// Available reference generatprs:
        /// - Linear kinematic synergy: "VAL_REFGEN_LINKINSYN";
        /// - Integrator: "VAL_REFGEN_INTEGRATOR";
        /// - Gradient-to-point: "VAL_REFGEN_POINTGRAD";
        /// </summary>
        /// <param name="rgName"></param>
        public void ChangeReferenceGenerator(string rgName)
        {
            inputManager.Configure("CMD_SET_ACTIVE_REFGEN", rgName);
        }
    }
}
