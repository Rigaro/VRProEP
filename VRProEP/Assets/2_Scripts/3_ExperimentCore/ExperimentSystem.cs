using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.ProsthesisCore;
using VRProEP.GameEngineCore;

namespace VRProEP.ExperimentCore
{
    public static class ExperimentSystem
    {
        private static string activeExperimentID;
        private static List<IExperimentLogger> activeLoggers = new List<IExperimentLogger>();
        private static List<ISensor> experimentSensors = new List<ISensor>();

        public static string ActiveExperimentID
        {
            get
            {
                return activeExperimentID;
            }
        }

        public static void SetActiveExperimentID(string id)
        {
            if (id == null)
                throw new System.ArgumentNullException("The provided experiment ID is empty.");

            // Check resources for available experiments.

            activeExperimentID = id;
        }

        /// <summary>
        /// Initializes and adds an experiment logger to the save system.
        /// </summary>
        /// <param name="logger">The logger to be initialized and added.</param>
        public static void AddExperimentLogger(IExperimentLogger logger)
        {
            if (logger == null)
                throw new System.ArgumentNullException("The provided logger is empty.");

            logger.InitializeLog(SaveSystem.ActiveSaveFolder);
            activeLoggers.Add(logger);
        }

        /// <summary>
        /// Gets an experiment logger by index from the list of active loggers.
        /// </summary>
        /// <param name="index">The logger index.</param>
        /// <returns>The requested logger.</returns>
        public static IExperimentLogger GetActiveLogger(int index)
        {
            if (index < 0 || index >= activeLoggers.Count)
                throw new System.IndexOutOfRangeException("The provided index exceeds the number of experiment loggers available.");

            return activeLoggers[index];
        }

        /// <summary>
        /// Closes all active loggers that have been added to the save system.
        /// </summary>
        public static void CloseAllExperimentLoggers()
        {
            foreach (IExperimentLogger logger in activeLoggers)
            {
                if (logger != null)
                    logger.CloseLog();
            }
        }

        /// <summary>
        /// Returns all the available loggers.
        /// </summary>
        /// <returns>The list of loggers.</returns>
        public static List<IExperimentLogger> GetActiveLoggers()
        {
            return activeLoggers;
        }

        /// <summary>
        /// Returns all the available sensors.
        /// </summary>
        /// <returns>The list of sensors.</returns>
        public static List<ISensor> GetActiveSensors()
        {
            return experimentSensors;
        }

        /// <summary>
        /// Adds a given sensor to the list of available experiment sensors.
        /// </summary>
        /// <param name="sensor">The sensor to be added.</param>
        public static void AddSensor(ISensor sensor)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor is empty.");
            
            experimentSensors.Add(sensor);
        }

        /// <summary>
        /// Returns the sensor given by the index.
        /// </summary>
        /// <param name="index">The index of the sensor to be retreived.</param>
        /// <returns>The requested sensor.</returns>
        public static ISensor GetActiveSensor(int index)
        {
            if (index < 0 || index >= experimentSensors.Count)
                throw new System.IndexOutOfRangeException("The provided index exceeds the number of experiment sensors available.");

            return experimentSensors[index];
        }

    }

}
