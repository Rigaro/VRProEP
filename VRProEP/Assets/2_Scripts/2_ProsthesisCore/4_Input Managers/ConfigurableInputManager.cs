//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Input manager that allows for dynamic customization of sensors and reference generators.
    /// </summary>
    public class ConfigurableInputManager : ComplexInputManagerManager, IConfigurable
    {
        private ISensor activeSensor;
        private IReferenceGenerator activeGenerator;

        public const string CMD_SET_ACTIVE_SENSOR = "CMD_SET_ACTIVE_SENSOR";
        public const string CMD_SET_ACTIVE_REFGEN = "CMD_SET_ACTIVE_REFGEN";
        public const string CMD_ADD_SENSOR = "CMD_ADD_SENSOR";
        public const string CMD_ADD_REFGEN = "CMD_ADD_REFGEN";
        public const string VAL_SENSOR_VIVETRACKER = "VAL_SENSOR_VIVETRACKER";
        public const string VAL_SENSOR_VIVECONTROLLER = "VAL_SENSOR_VIVECONTROLLER";
        public const string VAL_SENSOR_OCULUSTOUCH = "VAL_SENSOR_OCULUSTOUCH";
        public const string VAL_SENSOR_VIRTUALENCODER = "VAL_SENSOR_VIRTUALENCODER";
        public const string VAL_SENSOR_SEMG = "VAL_SENSOR_SEMG";
        public const string VAL_REFGEN_LINKINSYN = "VAL_REFGEN_LINKINSYN";
        public const string VAL_REFGEN_JACOBIANSYN = "VAL_REFGEN_JACOBIANSYN";
        public const string VAL_REFGEN_INTEGRATOR = "VAL_REFGEN_INTEGRATOR";
        public const string VAL_REFGEN_POINTGRAD = "VAL_REFGEN_POINTGRAD";
        public const string VAL_REFGEN_EMGPROP = "VAL_REFGEN_EMGPROP";

        /// <summary>
        /// Input manager that allows for dynamic customization of sensors and reference generators.
        /// </summary>
        /// <param name="sensor">A sensor to initialize the manager with.</param>
        /// <param name="referenceGenerator">A reference generator to initialize the manager with.</param>
        public ConfigurableInputManager(ISensor sensor, IReferenceGenerator referenceGenerator)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor object is empty.");
            if (referenceGenerator == null)
                throw new System.ArgumentNullException("The provided reference generator object is empty.");

            AddSensor(sensor);
            activeSensor = sensor;
            AddReferenceGenerator(referenceGenerator);
            activeGenerator = referenceGenerator;
        }

        /// <summary>
        /// Input manager that allows for dynamic customization of sensors and reference generators.
        /// </summary>
        /// <param name="sensor">A sensor to initialize the manager with.</param>
        /// <param name="referenceGenerator">A reference generator to initialize the manager with.</param>
        public ConfigurableInputManager(List<ISensor> sensorList, List<IReferenceGenerator> referenceGeneratorList)
        {
            if (sensorList == null)
                throw new System.ArgumentNullException("The provided sensor object is empty.");
            if (referenceGeneratorList == null)
                throw new System.ArgumentNullException("The provided reference generator object is empty.");

            // Add all sensors in list.
            foreach (ISensor sensor in sensorList)
            {
                if (sensor == null)
                    throw new System.ArgumentNullException("The provided sensor object is empty.");

                AddSensor(sensor);
            }
            // Set the first sensor as active
            activeSensor = sensorList[0];

            // Add all ref gens in list.
            foreach (IReferenceGenerator refGen in referenceGeneratorList)
            {
                if (refGen == null)
                    throw new System.ArgumentNullException("The provided reference generator object is empty.");

                AddReferenceGenerator(refGen);
            }
            // Set the first ref gen as active
            activeGenerator = referenceGeneratorList[0];
        }

        /// <summary>
        /// Sets the active sensor from the list of available sensors.
        /// </summary>
        /// <param name="sensorType">The sensor type to set as active.</param>
        private void SetActiveSensor(SensorType sensorType)
        {
            ISensor sensor;
            if (GetSensor(sensorType, out sensor))
                activeSensor = sensor;
            else
                throw new System.ArgumentException("Sensor " + sensorType.ToString() + " unavailable.");
        }

        /// <summary>
        /// Sets the active reference generator from the list of available reference generators
        /// </summary>
        /// <param name="refGenType">The reference generator type to set as active.</param>
        private void SetActiveReferenceGenerator(ReferenceGeneratorType refGenType)
        {
            IReferenceGenerator refGen;
            if (GetReferenceGenerator(refGenType, out refGen))
                activeGenerator = refGen;
            else
                throw new System.ArgumentException("Reference generator " + refGenType.ToString() + " unavailable.");
        }

        /// <summary>
        /// Generates an updated reference for the given device channel.
        /// Reads tracking data from a set of sensors.
        /// Generates references with the active reference generator.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>The updated reference.</returns>
        public override float GenerateReference(int channel)
        {
            // Jacobian synergy reference generator requires multiple sensors.
            if (GetActiveReferenceGeneratorType() == ReferenceGeneratorType.JacobianSynergy)
            {
                // Save currently active sensor
                SensorType prevSensorType = activeSensor.GetSensorType();
                // Get shoulder position and velocity
                Configure("CMD_SET_ACTIVE_SENSOR", SensorType.VIVETracker);
                float qShoulder = activeSensor.GetProcessedData(5);
                float qDotShoulder = activeSensor.GetProcessedData(0);
                // Get elbow position
                Configure("CMD_SET_ACTIVE_SENSOR", SensorType.VirtualEncoder);
                float qElbow = activeSensor.GetProcessedData(0);
                // Get enable
                Configure("CMD_SET_ACTIVE_SENSOR", SensorType.VIVEController);
                float enableValue = activeSensor.GetProcessedData(1);

                // Combine input
                float[] input = { qShoulder, -qElbow, 2.0f * qDotShoulder, enableValue };
                //Debug.Log("The input is: qs = " + Mathf.Rad2Deg * input[0] + ", qe = " + Mathf.Rad2Deg * input[1] + ", qDotS = " + input[2] + ", enable = " + input[3]);

                // Go back to previously active sensor
                Configure("CMD_SET_ACTIVE_SENSOR", prevSensorType);

                // Generate reference
                return activeGenerator.UpdateReference(channel, input);
            }
            else if (GetActiveReferenceGeneratorType() == ReferenceGeneratorType.EMGInterface)
            {
                // Save currently active sensor
                SensorType prevSensorType = activeSensor.GetSensorType();
                // Get EMG status
                Configure("CMD_SET_ACTIVE_SENSOR", SensorType.EMGWiFi);
                float[] emgStatus = activeSensor.GetAllProcessedData();
                // Get enable
                Configure("CMD_SET_ACTIVE_SENSOR", SensorType.VIVEController);
                float enableValue = activeSensor.GetProcessedData(1);

                // Combine input
                List<float> input = new List<float>();
                input.Add(enableValue);
                foreach (float emgState in emgStatus)
                {
                    input.Add(emgState);
                    //Debug.Log(emgState);
                }

                // Go back to previously active sensor
                Configure("CMD_SET_ACTIVE_SENSOR", prevSensorType);

                // Generate reference
                return activeGenerator.UpdateReference(channel, input.ToArray());

            }
            else
            {
                // First read the angular velocity from sensor
                float[] input = { activeSensor.GetProcessedData(channel) };
                // Compute reference with that angular velocity
                return activeGenerator.UpdateReference(channel, input);
            }
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
            List<float> references = new List<float>(activeGenerator.ChannelSize());
            // Generate all refereces
            for (int i = 0; i < activeGenerator.ChannelSize(); i++)
            {
                references.Add(GenerateReference(i));
            }
            return references.ToArray();
        }


        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public void Configure(string command, dynamic value)
        {
            switch (command)
            {
                case CMD_ADD_SENSOR:
                    if (value is ISensor)
                        AddSensor(value);
                    else
                        throw new System.ArgumentException("Invalid value provided.");
                    break;
                case CMD_ADD_REFGEN:
                    if (value is IReferenceGenerator)
                        AddReferenceGenerator(value);
                    else
                        throw new System.ArgumentException("Invalid value provided.");
                    break;
                case CMD_SET_ACTIVE_SENSOR:
                    if (value is SensorType)
                        SetActiveSensor(value);
                    else
                        throw new System.ArgumentException("Invalid value provided.");
                    break;
                case CMD_SET_ACTIVE_REFGEN:
                    if (value is ReferenceGeneratorType)
                        SetActiveReferenceGenerator(value);
                    else
                        throw new System.ArgumentException("Invalid value provided.");
                    break;
                /*
                 * 
                 * Add commands for removing sensors and reference generators. 
                 * 
                 */
                default:
                    throw new System.ArgumentException("Invalid command provided.");
            }
        }

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public void Configure(string command, string value)
        {
            switch (command)
            {
                case CMD_SET_ACTIVE_SENSOR:
                    switch (value)
                    {
                        case VAL_SENSOR_OCULUSTOUCH:
                            SetActiveSensor(SensorType.OculusSensor);
                            break;
                        case VAL_SENSOR_VIVETRACKER:
                            SetActiveSensor(SensorType.VIVETracker);
                            break;
                        case VAL_SENSOR_VIVECONTROLLER:
                            SetActiveSensor(SensorType.VIVEController);
                            break;
                        case VAL_SENSOR_VIRTUALENCODER:
                            SetActiveSensor(SensorType.VirtualEncoder);
                            break;
                        case VAL_SENSOR_SEMG:
                            SetActiveSensor(SensorType.EMGWiFi);
                            break;
                        default:
                            throw new System.ArgumentException("Invalid value provided.");
                    }
                    break;
                case CMD_SET_ACTIVE_REFGEN:
                    switch (value)
                    {
                        case VAL_REFGEN_INTEGRATOR:
                            SetActiveReferenceGenerator(ReferenceGeneratorType.Integrator);
                            break;
                        case VAL_REFGEN_POINTGRAD:
                            SetActiveReferenceGenerator(ReferenceGeneratorType.PointGradient);
                            break;
                        case VAL_REFGEN_LINKINSYN:
                            SetActiveReferenceGenerator(ReferenceGeneratorType.LinearKinematicSynergy);
                            break;
                        case VAL_REFGEN_JACOBIANSYN:
                            SetActiveReferenceGenerator(ReferenceGeneratorType.JacobianSynergy);
                            break;
                        case VAL_REFGEN_EMGPROP:
                            SetActiveReferenceGenerator(ReferenceGeneratorType.EMGInterface);
                            break;
                        default:
                            throw new System.ArgumentException("Invalid value provided.");
                    }
                    break;
                default:
                    throw new System.ArgumentException("Invalid command provided.");
            }
        }

        public SensorType GetActiveSensorType()
        {
            return activeSensor.GetSensorType();
        }

        public ReferenceGeneratorType GetActiveReferenceGeneratorType()
        {
            return activeGenerator.GeneratorType();
        }
    }
}
