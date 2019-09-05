using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GameMaster includes
using VRProEP.ExperimentCore;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.Utilities;

public class TestGM : GameMaster
{
    [Header("Test config. variables.")]
    public AvatarType avatarType = AvatarType.AbleBodied;

    [Header("EMG config")]
    public bool emgEnable = false;
    public string ip = "192.168.137.2";
    public int port = 2390;
    public int channelSize = 1;

    private float taskTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Load player
        SaveSystem.LoadUserData("MD1942");
        AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, avatarType);
        InitExperimentSystem();
        InitializeUI();
    }

    // Update is called once per frame
    void Update()
    {
        switch (experimentState)
        {
            /*
             *************************************************
             *  HelloWorld
             *************************************************
             */
            // Welcome subject to the virtual world.
            case ExperimentState.HelloWorld:
                // Load avatar
                if (avatarType == AvatarType.AbleBodied)
                {
                    AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);

                }
                else if (avatarType == AvatarType.Transhumeral)
                {
                    AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);

                    // Initialize prosthesis
                    GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
                    ConfigurableElbowManager elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
                    elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + SaveSystem.ActiveUser.handLength / 2.0f));
                    // Set the reference generator to jacobian-based.
                    elbowManager.ChangeReferenceGenerator("VAL_REFGEN_JACOBIANSYN");

                    // Enable & configure EMG
                    if (emgEnable)
                    {
                        // Create and add sensor
                        EMGWiFiManager emgSensor = new EMGWiFiManager(ip, port, channelSize);
                        AvatarSystem.AddActiveSensor(emgSensor);
                        elbowManager.AddSensor(emgSensor);
                        emgSensor.StartSensorReading();
                        
                        // Set active sensor and reference generator to EMG.
                        elbowManager.ChangeSensor("VAL_SENSOR_SEMG");
                        elbowManager.ChangeReferenceGenerator("VAL_REFGEN_EMGPROP");
                    }
                }

                experimentState = ExperimentState.InitializingApplication;
                break;
            /*
             *************************************************
             *  InitializingApplication
             *************************************************
             */
            // Perform initialization functions before starting experiment.
            case ExperimentState.InitializingApplication:
                //
                // Perform experiment initialization procedures
                //

                //
                // Initialize data logs
                //

                //
                // Go to training
                //
                experimentState = ExperimentState.Training;
                break;
            /*
             *************************************************
             *  Practice
             *************************************************
             */
            // Perform initialization functions before starting experiment.
            case ExperimentState.Training:
                //
                // Guide subject through training
                //

                //
                // Go to instructions
                //
                experimentState = ExperimentState.GivingInstructions;
                break;
            /*
             *************************************************
             *  GivingInstructions
             *************************************************
             */
            case ExperimentState.GivingInstructions:
                // Skip instructions when repeating sessions
                if (skipInstructions)
                {
                    hudManager.DisplayText("Move to start", 2.0f);
                    // Turn targets clear
                    experimentState = ExperimentState.WaitingForStart;
                    break;
                }

                //
                // Give instructions
                //

                //
                // Go to waiting for start
                //
                hudManager.DisplayText("Move to start", 2.0f);
                // Turn targets clear
                experimentState = ExperimentState.WaitingForStart;

                break;
            /*
             *************************************************
             *  WaitingForStart
             *************************************************
             */
            case ExperimentState.WaitingForStart:

                // Check if pause requested
                UpdatePause();
                switch (waitState)
                {
                    // Waiting for subject to get to start position.
                    case WaitState.Waiting:
                        SetWaitFlag(3.0f);
                        waitState = WaitState.Countdown;
                        break;
                    case WaitState.Countdown:
                        if (WaitFlag)
                            experimentState = ExperimentState.PerformingTask;
                        break;
                    default:
                        break;
                }
                break;
            /*
             *************************************************
             *  PerformingTask
             *************************************************
             */
            case ExperimentState.PerformingTask:
                // Task performance is handled deterministically in FixedUpdate.
                break;
            /*
             *************************************************
             *  AnalizingResults
             *************************************************
             */
            case ExperimentState.AnalizingResults:
                // Allow 3 seconds after task end to do calculations
                SetWaitFlag(3.0f);

                //
                // Data analysis and calculations
                //

                //
                // System update
                //

                // 
                // Data logging
                //

                //
                // Flow managment
                //
                // Rest for some time when required
                if (CheckRestCondition())
                {
                    SetWaitFlag(restTime);
                    experimentState = ExperimentState.Resting;
                }
                // Check whether the new session condition is met
                else if (CheckNextSessionCondition())
                {
                    experimentState = ExperimentState.InitializingNextSession;
                }
                // Check whether the experiment end condition is met
                else if (CheckEndCondition())
                {
                    experimentState = ExperimentState.End;
                }
                else
                    experimentState = ExperimentState.UpdatingApplication;
                break;
            /*
             *************************************************
             *  UpdatingApplication
             *************************************************
             */
            case ExperimentState.UpdatingApplication:
                if (WaitFlag)
                {
                    //
                    // Update iterations and flow control
                    //

                    // 
                    // Update log requirements
                    //

                    //
                    //
                    // Go to start of next iteration
                    experimentState = ExperimentState.WaitingForStart;
                }
                break;
            /*
             *************************************************
             *  InitializingNext
             *************************************************
             */
            case ExperimentState.InitializingNextSession:
                //
                // Perform session closure procedures
                //

                //
                // Initialize new session variables and flow control
                //
                iterationNumber = 1;
                sessionNumber++;
                skipInstructions = true;

                //
                // Initialize data logging
                //
                //ExperimentSystem.GetActiveLogger(1).AddNewLogFile(sessionNumber, iterationNumber, "Data format");

                experimentState = ExperimentState.InitializingApplication; // Initialize next session
                break;
            /*
             *************************************************
             *  Resting
             *************************************************
             */
            case ExperimentState.Resting:
                //
                // Check for session change or end request from experimenter
                //
                if (UpdateNext())
                {
                    LaunchNextSession();
                    break;
                }
                else if (UpdateEnd())
                {
                    EndExperiment();
                    break;
                }
                //
                // Restart after flag is set by wait coroutine
                //
                if (WaitFlag)
                {
                    hudManager.DisplayText("Get ready to restart!", 3.0f);
                    SetWaitFlag(5.0f);
                    experimentState = ExperimentState.UpdatingApplication;
                    break;
                }
                break;
            /*
             *************************************************
             *  Paused
             *************************************************
             */
            case ExperimentState.Paused:
                //
                // Check for session change or end request from experimenter
                //
                UpdatePause();
                if (UpdateNext())
                {
                    LaunchNextSession();
                    break;
                }
                else if (UpdateEnd())
                {
                    EndExperiment();
                    break;
                }
                break;
            /*
             *************************************************
             *  End
             *************************************************
             */
            case ExperimentState.End:
            //
            // Update log data and close logs.
            //

            //
            // Return to main menu
            //
            default:
                break;
        }

        //
        // Update information displayed on monitor
        //

        //
        // Update information displayed for debugging purposes
        //
        if (debug)
        {
            debugText.text = experimentState.ToString() + "\n";
            if (experimentState == ExperimentState.WaitingForStart)
                debugText.text += waitState.ToString() + "\n";
        }
    }

    private void FixedUpdate()
    {
        //
        // Tasks performed determinalistically throughout the experiment
        // E.g. data gathering.
        //
        switch (experimentState)
        {
            case ExperimentState.PerformingTask:
                //
                // Gather data while experiment is in progress
                //
                string logData = taskTime.ToString();
                // Read from all user sensors
                foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
                {
                    logData += "\n" + sensor.GetSensorType().ToString();
                    float[] sensorData = sensor.GetAllProcessedData();
                    foreach (float element in sensorData)
                        logData += "\n" + element.ToString();
                }
                // Read from all experiment sensors
                foreach (ISensor sensor in ExperimentSystem.GetActiveSensors())
                {
                    logData += "\n" + sensor.GetSensorType().ToString();
                    float[] sensorData = sensor.GetAllProcessedData();
                    foreach (float element in sensorData)
                        logData += "\n" + element.ToString();
                }

                hudManager.DisplayText(logData);

                //
                // Append data to lists
                //
                taskTime += Time.fixedDeltaTime;

                //
                // Log current data
                //
                //ExperimentSystem.GetActiveLogger(1).AppendData(logData);

                //
                // Save log and reset flags when successfully compeleted task
                //
                if (CheckTaskCompletion())
                {
                    //
                    // Perform data management, such as appending data to lists for analysis
                    //

                    //
                    // Save logger for current experiment and change to data analysis
                    //
                    //ExperimentSystem.GetActiveLogger(1).CloseLog();

                    //
                    // Clear data management buffers
                    //
                    experimentState = ExperimentState.AnalizingResults;
                    break;
                }

                break;
            default:
                break;
        }
    }

    private void OnApplicationQuit()
    {
        //
        // Handle application quit procedures.
        //
        // Check if UDP sensors are available
        foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
        {
            if (sensor.GetSensorType().Equals(SensorType.EMGWiFi))
            {
                UDPSensorManager udpSensor = (UDPSensorManager)sensor;
                udpSensor.StopSensorReading();
            }
        }

        //
        // Save and close all logs
        //
        ExperimentSystem.CloseAllExperimentLoggers();
    }

    #region Inherited methods overrides

    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// </summary>
    protected override void InitExperimentSystem()
    {
        //
        // Set the experiment type and ID
        //

        //
        // Create data loggers
        //

    }


    /// <summary>
    /// Checks whether the subject is ready to start performing the task.
    /// </summary>
    /// <returns>True if ready to start.</returns>
    protected override bool CheckReadyToStart()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    protected override bool CheckTaskCompletion()
    {
        //
        // Perform some condition testing
        //
        return false;
    }

    /// <summary>
    /// Checks if the condition for the rest period has been reached.
    /// </summary>
    /// <returns>True if the rest condition has been reached.</returns>
    protected override bool CheckRestCondition()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    protected override bool CheckNextSessionCondition()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    protected override bool CheckEndCondition()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Launches the next session. Performs all the required preparations.
    /// </summary>
    protected override void LaunchNextSession()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Finishes the experiment. Performs all the required procedures.
    /// </summary>
    protected override void EndExperiment()
    {
        throw new System.NotImplementedException();
    }

    #endregion
}
