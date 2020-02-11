using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

// GameMaster includes
using VRProEP.ExperimentCore;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.Utilities;

public class PlaygroundGM : GameMaster
{
    [Header("Playground config. variables.")]
    public AvatarType avatarType = AvatarType.AbleBodied;
    public Transform startTransform;


    //private string defaultText = "\n\no o";
    private ConfigurableElbowManager elbowManager;

    // Start is called before the first frame update
    void Awake()
    {
        // Load player
        if (debug)
        {
            SaveSystem.LoadUserData("MD1942");
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // Initialize ExperimentSystem
        InitialiseExperimentSystems();

        // Initialize UI.
        InitializeUI();

        // Configure the grasp manager
        GameObject graspManagerGO = GameObject.FindGameObjectWithTag("GraspManager");
        if (graspManagerGO == null)
            throw new System.Exception("Grasp Manager not found.");
        GraspManager graspManager = graspManagerGO.GetComponent<GraspManager>();
        graspManager.managerType = GraspManager.GraspManagerType.Controller;
        graspManager.managerMode = GraspManager.GraspManagerMode.Open;

        //
        SetWaitFlag(5.0f);
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
            case ExperimentState.Welcome:
                // Load avatar
                if (avatarType == AvatarType.AbleBodied)
                {
                    //AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);

                }
                else if (avatarType == AvatarType.Transhumeral)
                {
                    AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);

                    // Initialize prosthesis
                    GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
                    ConfigurableElbowManager elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
                    elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + SaveSystem.ActiveUser.handLength / 2.0f));
                    // Set the reference generator to jacobian-based.
                    elbowManager.ChangeReferenceGenerator("VAL_REFGEN_LINKINSYN");
                }

                // Teleport to the start position
                TeleportToStartPosition();

                experimentState = ExperimentState.Initialising;
                break;
            /*
             *************************************************
             *  InitializingApplication
             *************************************************
             */
            // Perform initialization functions before starting experiment.
            case ExperimentState.Initialising:
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
                experimentState = ExperimentState.Instructions;
                break;
            /*
             *************************************************
             *  GivingInstructions
             *************************************************
             */
            case ExperimentState.Instructions:
                // Skip instructions when repeating sessions
                if (SkipInstructions)
                {
                    HudManager.DisplayText("Welcome...", 2.0f);
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
                HudManager.DisplayText("Welcome...", 2.0f);
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
                if (avatarType == AvatarType.AbleBodied)
                {
                    HudManager.colour = HUDManager.HUDColour.Green;
                }
                else
                {
                    if (elbowManager.IsEnabled)
                        HudManager.colour = HUDManager.HUDColour.Blue;
                    else
                        HudManager.colour = HUDManager.HUDColour.Red;
                }
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
                if (IsRestTime())
                {
                    SetWaitFlag(RestTime);
                    experimentState = ExperimentState.Resting;
                }
                // Check whether the new session condition is met
                else if (IsEndOfSession())
                {
                    experimentState = ExperimentState.InitializingNext;
                }
                // Check whether the experiment end condition is met
                else if (IsEndOfExperiment())
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
            case ExperimentState.InitializingNext:
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

                experimentState = ExperimentState.Initialising; // Initialize next session
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
                    ConfigureNextSession();
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
                    HudManager.DisplayText("Get ready to restart!", 3.0f);
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
                    ConfigureNextSession();
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
                //
                // Append data to lists
                //

                //
                // Log current data
                //
                //ExperimentSystem.GetActiveLogger(1).AppendData(logData);

                //
                // Save log and reset flags when successfully compeleted task
                //
                if (IsTaskDone())
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
        // Check if WiFi sensors are available
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

    public override void HandleResultAnalysis()
    {
        throw new System.NotImplementedException();
    }
    public override void HandleInTaskBehaviour()
    {
        throw new System.NotImplementedException();
    }
    public override void HandleTaskCompletion()
    {
        throw new System.NotImplementedException();
    }
    public override void PrepareForStart()
    {
        throw new System.NotImplementedException();
    }
    public override void StartFailureReset()
    {
        throw new System.NotImplementedException();
    }
    public override void InitialiseExperiment()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator WelcomeLoop()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator InstructionsLoop()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator TrainingLoop()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// </summary>
    public override void InitialiseExperimentSystems()
    {
        //
        // Set the experiment type and ID
        //

        //
        // Create data loggers
        //

        AvatarSystem.EnableAvatarColliders();
    }

    /// <summary>
    /// Checks whether the subject is ready to start performing the task.
    /// </summary>
    /// <returns>True if ready to start.</returns>
    public override bool IsReadyToStart()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    public override bool IsTaskDone()
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
    public override bool IsRestTime()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    public override bool IsEndOfSession()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    public override bool IsEndOfExperiment()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Launches the next session. Performs all the required preparations.
    /// </summary>
    public void ConfigureNextSession()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Finishes the experiment. Performs all the required procedures.
    /// </summary>
    public override void EndExperiment()
    {
        throw new System.NotImplementedException();
    }

    public override string GetDisplayInfoText()
    {
        throw new System.NotImplementedException();
    }

    #endregion

    public void LoadAbleBodiedAvatar()
    {
        // Fade
        SteamVR_Fade.Start(Color.black, 0.0f);

        // Load
        avatarType = AvatarType.AbleBodied;
        AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied, false);
        // Enable colliders
        StartCoroutine(EnableColliders());
        // Initialize UI.
        InitializeUI();
        // Fade
        SteamVR_Fade.Start(Color.black, 0.0f);

        // Change the number for the forearm tracker being used
        GameObject faTrackerGO = GameObject.FindGameObjectWithTag("ForearmTracker");
        SteamVR_TrackedObject steamvrConfig = faTrackerGO.GetComponent<SteamVR_TrackedObject>();
        steamvrConfig.index = SteamVR_TrackedObject.EIndex.Device5;

        // Teleport to the start position
        StartCoroutine(TeleportCoroutine());
    }

    public void LoadTHAvatar()
    {
        // Fade
        SteamVR_Fade.Start(Color.black, 0.0f);
        // Load
        avatarType = AvatarType.Transhumeral;

        AvatarSystem.LoadPlayer(UserType.Ablebodied, AvatarType.Transhumeral);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);
        // Fade
        SteamVR_Fade.Start(Color.black, 0.0f);

        // Initialize prosthesis
        GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
        elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
        elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + SaveSystem.ActiveUser.handLength / 2.0f), 1.5f);
        // Set the reference generator to jacobian-based.
        elbowManager.ChangeReferenceGenerator("VAL_REFGEN_LINKINSYN");

        StartCoroutine(EnableColliders());
        // Initialize UI.
        InitializeUI();

        // Teleport to the start position
        StartCoroutine(TeleportCoroutine());
    }

    public IEnumerator EnableColliders()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        // Enable colliders
        AvatarSystem.EnableAvatarColliders();
    }

    public void SendExternalMessage(string message)
    {
        HudManager.DisplayText(message, 5.0f);
    }

    public void ReturnToMainMenu()
    {
        // Load level.
        SteamVR_LoadLevel.Begin("MainMenu");
    }

    private IEnumerator TeleportCoroutine()
    {
        yield return new WaitForSeconds(1.0f);
        TeleportToStartPosition();
        SteamVR_Fade.Start(Color.clear, 1.0f);
    }
}
