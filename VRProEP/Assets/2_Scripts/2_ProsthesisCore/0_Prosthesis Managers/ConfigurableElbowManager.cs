//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.GameEngineCore;
using VRProEP.ExperimentCore;

namespace VRProEP.ProsthesisCore
{
    public class ConfigurableElbowManager : MonoBehaviour
    {
        private ConfigurableInputManager inputManager;
        //private ElbowManager elbowManager;
        private IdealElbowManager elbowManager;

        private float elbowState = 0.0f;

        private bool isConfigured = false;
        private bool isEnabled = false;

        public bool IsEnabled { get => isEnabled; }

        private float[] xBar = { Mathf.Deg2Rad * -90.0f };
        private float[] xMin = { Mathf.Deg2Rad * -145.0f };
        private float[] xMax = { Mathf.Deg2Rad * -0.1f };



        /// <summary>
        /// Initializes the Elbow prosthesis with basic functionality.
        /// Must be called only after the avatar is available.
        /// </summary>
        public void InitializeProsthesis(float upperArmLength, float lowerArmLength, float synValue = 1.0f)
        {
            //
            // ConfigurableInputManagar
            //
            // Find ResdiualLimbTracker GameObject and extract its Transform.
            GameObject residualLimbTrackerGO = GameObject.FindGameObjectWithTag("ResidualLimbTracker");
            // Create a VIVETracker with the obtained transform
            VIVETrackerManager trackerManager = new VIVETrackerManager(residualLimbTrackerGO.transform);

            // Find ShoulderTracker GameObject and extract its Transform.
            /*GameObject shoulderTrackerGO = GameObject.FindGameObjectWithTag("shoulderTracker");
            // Create a VIVETracker with the obtained transform
            VIVETrackerManager shoulderTracker = new VIVETrackerManager(residualLimbTrackerGO.transform);

            // Find c7Tracker GameObject and extract its Transform.
            GameObject c7TrackerGO = GameObject.FindGameObjectWithTag("c7Tracker");
            // Create a VIVETracker with the obtained transform
            VIVETrackerManager c7Tracker = new VIVETrackerManager(residualLimbTrackerGO.transform);*/



            // Shoulder acromium head tracker
            GameObject motionTrackerGO1 = AvatarSystem.AddMotionTracker();
            VIVETrackerManager shoulderTracker = new VIVETrackerManager(motionTrackerGO1.transform);
            AvatarSystem.AddActiveSensor(shoulderTracker);
            ExperimentSystem.AddSensor(shoulderTracker);

            // C7 tracker
            GameObject motionTrackerGO2 = AvatarSystem.AddMotionTracker();
            VIVETrackerManager c7Tracker = new VIVETrackerManager(motionTrackerGO2.transform);
            AvatarSystem.AddActiveSensor(c7Tracker);
            ExperimentSystem.AddSensor(c7Tracker);




            // add trackers
            // Create a basic reference generator: Integrator.
            IntegratorReferenceGenerator integratorRG = new IntegratorReferenceGenerator(xBar, xMin, xMax);
            // Create configurable input manager with the created sensor and RG.
            List<ISensor> sensorList = new List<ISensor>();
            sensorList.Add(trackerManager);
            sensorList.Add(shoulderTracker);
            sensorList.Add(c7Tracker);

            List<IReferenceGenerator> RGList = new List<IReferenceGenerator>();

            RGList.Add(integratorRG);
            inputManager = new ConfigurableInputManager(sensorList, RGList);


            //
            // ElbowManager
            //
            // Find Elbow_Lower GameObject and extract its HingeJoint and Rigidbody
            GameObject elbowLowerGO = GameObject.FindGameObjectWithTag("Elbow_Lower");
            if (elbowLowerGO == null)
                throw new System.Exception("Could not find and active elbow prosthesis avatar (GameObject).");
            HingeJoint elbowJoint = elbowLowerGO.GetComponent<HingeJoint>();
            Rigidbody elbowRB = elbowLowerGO.GetComponent<Rigidbody>();
            // Create VirtualEncoder and attach to HingeJoint.
            VirtualEncoderManager virtualEncoder = new VirtualEncoderManager(elbowJoint);
            //*******************************
            // ELBOW DEVICE MANAGER
            //
            // PD controller version
            // Create ElbowManager with the given VirtualEncoder and Rigidbody, and set the axis.
            //elbowManager = new ElbowManager(virtualEncoder, elbowRB);
            //elbowManager.Axis = elbowJoint.axis;
            // Ideal tracking version
            // Create ElbowManager with the given elbowJoint.
            elbowManager = new IdealElbowManager(elbowJoint);
            //*******************************

            //
            // Sensors
            //

            // Add VIVE controller as sensor to enable manual inputs.
            VIVEControllerManager controllerManager = new VIVEControllerManager();
            inputManager.Configure("CMD_ADD_SENSOR", controllerManager);

            // Add joint encoder as sensor for jacobian synergy
            inputManager.Configure("CMD_ADD_SENSOR", virtualEncoder);

            inputManager.Configure("CMD_ADD_SENSOR", shoulderTracker);
            inputManager.Configure("CMD_ADD_SENSOR", c7Tracker);

            // Add the created sensors to the list of available sensors.
            AvatarSystem.AddActiveSensor(trackerManager);
            //AvatarSystem.AddActiveSensor(shoulderTracker);
            //AvatarSystem.AddActiveSensor(c7Tracker);
            AvatarSystem.AddActiveSensor(virtualEncoder);
            //AvatarSystem.AddActiveSensor(controllerManager);


            //
            // Reference generators
            //
            // Add a Linear Kinematic Synergy to the prosthesis
            float[] theta = { -synValue };
            float[] thetaMin = { -3.5f };
            float[] thetaMax = { -0.1f };
            //LinearKinematicSynergy linSyn = new LinearKinematicSynergy(xBar, xMin, xMax, theta, thetaMin, thetaMax);
            //inputManager.Configure("CMD_ADD_REFGEN", linSyn);

            // Add a Jacobian based Kinematic Synergy
            //JacobianSynergy jacSyn = new JacobianSynergy(xBar, xMin, xMax, upperArmLength, lowerArmLength);
            //inputManager.Configure("CMD_ADD_REFGEN", jacSyn);

            // Add an EMG reference generator
            //List<float> emgGains = new List<float>(1);
            // emgGains.Add(1.3f); // single site
            //emgGains.Add(0.015f);
            //EMGInterfaceReferenceGenerator emgRG = new EMGInterfaceReferenceGenerator(xBar, xMin, xMax, emgGains, EMGInterfaceType.dualSiteProportional);
            //inputManager.Configure("CMD_ADD_REFGEN", emgRG);

            // Add ANN reference generator
            // added by Damian
            // Add an ANN reference generator (currently just copys LinSyn reference generator)
            ANNReferenceGenerator aNN = new ANNReferenceGenerator(xBar, xMin, xMax, theta, thetaMin, thetaMax);
       
            inputManager.Configure("CMD_ADD_REFGEN", aNN);
            // Enable
            isConfigured = true;
        }


        // Update the prosthesis state deterministically
        public void FixedUpdate()
        {
            if (isConfigured)
            {
                // Update references
                elbowState = inputManager.GenerateReference(0);
                // Update device state
                elbowManager.UpdateState(0, elbowState);
                isEnabled = inputManager.IsEnabled();
            }

        }

        /// <summary>
        /// Changes the active sensor for reference generation.
        /// Available sensors:
        /// - "VAL_SENSOR_VIVETRACKER";
        /// - "VAL_SENSOR_VIVECONTROLLER";
        /// - "VAL_SENSOR_VIRTUALENCODER";
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
        /// - Jacobian-based synergy: "VAL_REFGEN_JACOBIANSYN";
        /// - Integrator: "VAL_REFGEN_INTEGRATOR";
        /// - Gradient-to-point: "VAL_REFGEN_POINTGRAD";
        /// </summary>
        /// <param name="rgName"></param>
        public void ChangeReferenceGenerator(string rgName)
        {
            inputManager.Configure("CMD_SET_ACTIVE_REFGEN", rgName);
        }

        /// <summary>
        /// Adds the given sensor to the elbow.
        /// </summary>
        /// <param name="sensors">The sensor.</param>
        public void AddSensor(ISensor sensor)
        {
            inputManager.Configure("CMD_ADD_SENSOR", sensor);
        }

        /// <summary>
        /// Adds the given reference generator to the elbow.
        /// </summary>
        /// <param name="refGens">The reference generator.</param>
        public void AddRefGen(IReferenceGenerator refGen)
        {
            inputManager.Configure("CMD_ADD_REFGEN", refGen);
        }

        public ReferenceGeneratorType GetInterfaceType()
        {
            return inputManager.GetActiveReferenceGeneratorType();
        }

        /// <summary>
        /// Sets the synergy value for a synergistic elbow.
        /// </summary>
        /// <param name="theta">The synergy value.</param>
        public void SetSynergy(float theta)
        {
            inputManager.Configure("CMD_SET_SYNERGY", -theta);
        }

        /// <summary>
        /// Returns the current elbow joint angle.
        /// </summary>
        /// <returns></returns>
        public float GetElbowAngle()
        {
            return elbowState;
        }

        /// <summary>
        /// Sets the elbow to a given value.
        /// </summary>
        /// <param name="elbowAngle">The desired elbow angle in radians.</param>
        public void SetElbowAngle(float elbowAngle)
        {
            if (elbowAngle > xMax[0] || elbowAngle < xMin[0])
                throw new System.ArgumentOutOfRangeException("The provided elbow angle is out of the allowed range.");

            inputManager.Configure("CMD_SET_REFERENCE", elbowAngle);
            elbowState = elbowAngle;
        }
    }
}
