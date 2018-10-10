//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

/// <summary>
/// Interface for experimental data loggers.
/// </summary>
public interface IExperimentLogger {

    /// <summary>
    /// Initializes the log by creating the directory (if not available) and adding the first file.
    /// </summary>
    /// <param name="subjectID">The subject's identification code.</param>
    /// <param name="sessionNum">The current session number for that subject.</param>
    /// <param name="iteration">The current task iteration.</param>
    /// <param name="format">The format to be used as header for the data in the Log.</param>
    /// <returns>True if the Log was initilized properly.</returns>
    bool InitializeLog(string subjectID, int session, int iteration, string format);

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

    /// <summary>
    /// Checks if the log has been enabled.
    /// </summary>
    /// <returns>True if the log is enabled.</returns>
    bool IsEnabled();
}
