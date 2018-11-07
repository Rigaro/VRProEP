//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ExperimentCore
{
    /// <summary>
    /// Interface for experimental data loggers.
    /// </summary>
    public interface IExperimentLogger
    {
        /// <summary>
        /// Sets the configuration parameters for the experiment data logger.
        /// </summary>
        /// <param name="experimentID">The identifier for the experiment that is being run.</param>
        /// <param name="dataLogTypeID">The identifier for the data log type that is being used.</param>
        void ConfigureLogger(string experimentID, string dataLogTypeID);

        /// <summary>
        /// Initializes the log by setting the active user directory for data logging or creating it if not available.
        /// Creates the folders for the active experiment identifier and data log type if not available.
        /// </summary>
        /// <param name="activeUserPath">The path of the active user to initialize the log to.</param>
        void InitializeLog(string activeUserPath);

        /// <summary>
        /// Adds a new log for the given session Number and Iteration.
        /// </summary>
        /// <param name="sessionNum">The current session number for that subject.</param>
        /// <param name="iteration">The current task iteration.</param>
        /// <param name="format">The format to be used as header for the data in the Log.</param>
        void AddNewLogFile(int sessionNum, int iteration, string format);

        /// <summary>
        /// Appends new data to the current Log file.
        /// </summary>
        /// <param name="data">The string data to be appended.</param>
        void AppendData(string data);

        /// <summary>
        /// Saves and closes the Log. Must be called on OnApplicationQuit.
        /// </summary>
        void CloseLog();

        /// <summary>
        /// Saves the current log file.
        /// </summary>
        void SaveLog();
    }
}
