//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VRProEP.ExperimentCore
{
    /// <summary>
    /// Experiment logger that handles data streams in text format.
    /// </summary>
    public class DataStreamLogger : IExperimentLogger
    {
        protected StreamWriter fileWriter;
        private string activeUserPath;
        private string activeDataPath;
        private string activeExperimentID;
        private string activeDataLogTypeID;

        private bool isInitialized = false;
        private bool isConfigured = false;

        public bool IsInitialized
        {
            get
            {
                return isInitialized;
            }
        }
        public bool IsConfigured
        {
            get
            {
                return isConfigured;
            }
        }

        /// <summary>
        /// Experiment logger that handles data streams in text format.
        /// Uses the active experiment ID from ExperimentSystem. This must be set beforehand.
        /// </summary>
        /// <param name="experimentID">The experiment identifier.</param>
        /// <param name="dataLogTypeID">The log type identifier.</param>
        public DataStreamLogger(string dataLogTypeID)
        {
            activeExperimentID = ExperimentSystem.ActiveExperimentID;
            activeDataLogTypeID = dataLogTypeID;
        }

        /// <summary>
        /// Returns the experiment type identified that is currently active.
        /// </summary>
        /// <returns>The active experiment identifier.</returns>
        public string GetActiveExperiment()
        {
            return activeExperimentID;
        }

        /// <summary>
        /// Returns the data log type identified that is currently active.
        /// </summary>
        /// <returns>The active log type identifier.</returns>
        public string GetActiveLogType()
        {
            return activeDataLogTypeID;
        }


        /// <summary>
        /// Sets the configuration parameters for the experiment data logger.
        /// Creates the directories when not available.
        /// </summary>
        /// <param name="experimentID">The identifier for the experiment that is being run.</param>
        /// <param name="dataLogTypeID">The identifier for the data log type that is being used.</param>
        public void ConfigureLogger(string experimentID, string dataLogTypeID)
        {
            if (experimentID == null || dataLogTypeID == null)
                throw new System.ArgumentNullException("The provided experiment or data log type identifier is empty.");

            if (activeUserPath == null)
                throw new System.ArgumentNullException("The logger has not been initialized with the active user data path.");

            // Create the file path
            string experimentPath = activeUserPath + "/" + experimentID;
            if (!Directory.Exists(experimentPath))
                Directory.CreateDirectory(experimentPath);

            string dataPath = experimentPath + "/" + dataLogTypeID;
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            activeExperimentID = experimentID;
            activeDataLogTypeID = dataLogTypeID;
            activeDataPath = dataPath;
            isConfigured = true;
        }

        /// <summary>
        /// Initializes the log by setting the active user directory for data logging or creating it if not available.
        /// Creates the folders for the active experiment identifier and data log type if not available.
        /// </summary>
        /// <param name="activeUserPath">The path of the active user to initialize the log to.</param>
        public void InitializeLog(string activeUserPath)
        {
            this.activeUserPath = activeUserPath;
            // Re-configure the logger with the active configuration for the change in path.
            ConfigureLogger(activeExperimentID, activeDataLogTypeID);
            isInitialized = true;
        }

        /// <summary>
        /// Adds a new log for the given session number and iteration.
        /// </summary>
        /// <param name="sessionNum">The current experiment session number.</param>
        /// <param name="iteration">The current task iteration.</param>
        /// <param name="format">The format to be used as header for the data in the Log.</param>
        public void AddNewLogFile(int sessionNum, int iteration, string format)
        {
            if (!isInitialized || !isConfigured)
                throw new System.Exception("The logger has not been initialized or configured.");
            // Create directory for the given session number if not available.
            string newFilePath = Path.Combine(activeDataPath, "session" + "_" + sessionNum.ToString());
            if (!Directory.Exists(newFilePath))
                Directory.CreateDirectory(newFilePath);
            // Create comma separated file to hold data stream for the given iteration
            newFilePath = Path.Combine(newFilePath, "i" + "_" + iteration + ".csv");
            FileStream sb = new FileStream(newFilePath, FileMode.Create);
            // Initialize file StreamWriter with the given file.
            fileWriter = new StreamWriter(sb);
            // Add log format as header
            fileWriter.WriteLine(format);
            SaveLog();
        }

        /// <summary>
        /// Appends new data to the current Log file.
        /// </summary>
        /// <param name="data">The string data to be appended.</param>
        public void AppendData(string data)
        {
            try
            {
                fileWriter.WriteLine(data);
            }
            catch (System.Exception e)
            {
                Debug.Log("Couldn't append data to the Log! :'(. Error:" + e.ToString());
            }
        }

        /// <summary>
        /// Saves and closes the Log. Must be called on OnApplicationQuit.
        /// </summary>
        public void CloseLog()
        {
            if (fileWriter != null)
            {
                try
                {
                    fileWriter.Close();
                }
                catch (System.Exception e)
                {
                    Debug.Log("Something went wrong when saving the Log! D:. Error:" + e.ToString());
                }
            }
        }

        /// <summary>
        /// Saves the current log file.
        /// </summary>
        public void SaveLog()
        {
            if (fileWriter != null)
            {
                try
                {
                    fileWriter.Flush();
                }
                catch (System.Exception e)
                {
                    Debug.Log("Something went wrong when saving the Log! D:. Error:" + e.ToString());
                }
            }
        }
    }

}
