//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Abstract input manager that can append and delete sensors and reference managers.
    /// Gives the functionality necessary for dynamic customization.
    /// </summary>
    public abstract class ComplexInputManagerManager : IInputManager
    {
        protected List<ISensor> sensorManagers = new List<ISensor>(1);
        private int sensorNum;
        protected List<IReferenceGenerator> referenceGenerators = new List<IReferenceGenerator>(1);
        private int refGenNum;

        public int SensorNum
        {
            get
            {
                return sensorNum;
            }
        }

        public int RefGenNum
        {
            get
            {
                return refGenNum;
            }

        }

        /// <summary>
        /// Generates an updated reference for the given device channel.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>The updated reference.</returns>
        public abstract float GenerateReference(int channel);

        /// <summary>
        /// Generates the set of references needed to update all devices.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <returns>The updated set of references.</returns>
        public abstract float[] GenerateAllReferences();

        /// <summary>
        /// Adds a given sensor to the list of sensors held by the input manager.
        /// </summary>
        /// <param name="sensor">The sensor to be attached to the input manager.</param>
        protected void AddSensor(ISensor sensor)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor object is empty.");

            sensorManagers.Add(sensor);
            sensorNum++;
        }

        /// <summary>
        /// Adds a given reference generator to the list of sensors held by the input manager.
        /// </summary>
        /// <param name="referenceGenerator">The reference generator to be attached to the input manager.</param>
        protected void AddReferenceGenerator(IReferenceGenerator referenceGenerator)
        {

            if (referenceGenerator == null)
                throw new System.ArgumentNullException("The provided reference generator object is empty.");

            referenceGenerators.Add(referenceGenerator);
            refGenNum++;
        }

        /// <summary>
        /// Removes a given sensor from the list of sensors held by the input manager.
        /// </summary>
        /// <param name="sensor">The sensor to be removed from the input manager.</param>
        /// <returns>True if the sensor was removed successfully.</returns>
        protected bool RemoveSensor(ISensor sensor)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor object is empty.");

            sensorNum--;
            return sensorManagers.Remove(sensor);
        }

        /// <summary>
        /// Removes the sensor with the given index from the list of sensors held by the input manager.
        /// </summary>
        /// <param name="index">The sensor index to be removed from the input manager.</param>
        /// <returns>True if the sensor was removed successfully.</returns>
        protected bool RemoveSensor(int index)
        {
            try
            {
                sensorManagers.RemoveAt(index);
                sensorNum--;
                return true;
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Debug.Log("Error: " + e.ToString() + ". The provided sensor index to remove is invalid.");
                return false;
            }
        }

        /// <summary>
        /// Removes a given reference generator from the list of reference generators held by the input manager.
        /// </summary>
        /// <param name="sensor">The reference generator to be removed from the input manager.</param>
        /// <returns>True if the reference generator was removed successfully.</returns>
        protected bool RemoveReferenceGenerator(IReferenceGenerator referenceGenerator)
        {
            if (referenceGenerator == null)
                throw new System.ArgumentNullException("The provided reference generator object is empty.");

            refGenNum--;
            return referenceGenerators.Remove(referenceGenerator);
        }

        /// <summary>
        /// Removes the reference generator with the given index from the list of reference generators held by the input manager.
        /// </summary>
        /// <param name="index">The reference generator index to be removed from the input manager.</param>
        /// <returns>True if the reference generator was removed successfully.</returns>
        protected bool RemoveReferenceGenerator(int index)
        {
            try
            {
                referenceGenerators.RemoveAt(index);
                refGenNum--;
                return true;
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Debug.Log("Error: " + e.ToString() + ". The provided reference generator index to remove is invalid.");
                return false;
            }
        }

        /// <summary>
        /// Looks for a sensor of the given type within the list of sensors in the input manager and is returned if found.
        /// </summary>
        /// <param name="sensorType">The type of sensor to look for.</param>
        /// <param name="outSensor">The sensor object to put the found sensor in.</param>
        /// <returns>True if a sensor was found.</returns>
        protected bool GetSensor(SensorType sensorType, out ISensor outSensor)
        {
            // Look for a sensor with the given type.
            foreach (ISensor sensor in sensorManagers)
            {
                // If found set it to the referenced sensor and return true.
                if (sensor.GetSensorType().Equals(sensorType))
                {
                    outSensor = sensor;
                    return true;
                }
            }
            // Failed to find a sensor with the provided type.
            outSensor = null;
            return false;
        }

        /// <summary>
        /// Looks for a reference generator of the given type within the list of reference generators in the input manager and is returned if found.
        /// </summary>
        /// <param name="refGenType">The type of reference generator to look for.</param>
        /// <param name="outRefGen">The reference generator object to put the found reference generator in.</param>
        /// <returns>True if a reference generator was found.</returns>
        protected bool GetReferenceGenerator(ReferenceGeneratorType refGenType, out IReferenceGenerator outRefGen)
        {
            // Look for a reference generator with the given type.
            foreach (IReferenceGenerator refGen in referenceGenerators)
            {
                // If found set it to the referenced reference generator and return true.
                if (refGen.GeneratorType().Equals(refGenType))
                {
                    outRefGen = refGen;
                    return true;
                }
            }
            // Failed to find a reference generator with the provided type.
            outRefGen = null;
            return false;
        }
    }
}