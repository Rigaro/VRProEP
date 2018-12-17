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
                logger.SaveLog(); // Save data just in case it was not saved.
                logger.CloseLog();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<IExperimentLogger> GetActiveLoggers()
        {
            return activeLoggers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<ISensor> GetActiveSensors()
        {
            return experimentSensors;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sensor"></param>
        public static void AddSensor(ISensor sensor)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor is empty.");

            experimentSensors.Add(sensor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ISensor GetActiveSensor(int index)
        {
            if (index < 0 || index >= experimentSensors.Count)
                throw new System.IndexOutOfRangeException("The provided index exceeds the number of experiment sensors available.");

            return experimentSensors[index];
        }

    }

}
