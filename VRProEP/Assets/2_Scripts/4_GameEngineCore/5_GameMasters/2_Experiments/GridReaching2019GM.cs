// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

// SteamVR
using Valve.VR;

// GameMaster includes
using VRProEP.ExperimentCore;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.Utilities;

public class GridReaching2019GM : GameMaster
{
    [Header("Experiment configuration: Grid")]
    [SerializeField]
    [Tooltip("The number of rows for the reaching grid.")]
    [Range(1,10)]
    private int gridRows = 5;

    [SerializeField]
    [Tooltip("The number of columns for the reaching grid.")]
    [Range(1, 10)]
    private int gridColumns = 5;

    [SerializeField]
    [Tooltip("The spacing between the balls in the grid.")]
    [Range(0.05f, 1.0f)]
    private float gridSpacing = 0.15f;

    [SerializeField]
    [Tooltip("The percentage of the subject's height where the grid centre will be placed.")]
    [Range(0.0f, 1.0f)]
    private float gridHeightMultiplier = 0.6f;

    [SerializeField]
    [Tooltip("The percentage of the subject's arm length where the grid centre will be placed.")]
    [Range(0.0f, 1.0f)]
    private float gridReachMultiplier = 0.6f;

    [SerializeField]
    private BallGridManager gridManager;
    
    // Target management variables
    private int targetNumber; // The total number of targets
    private List<int> targetOrder = new List<int>(); // A list of target indexes ordered for selection over iterations in a session.

    [Header("Experiment configuration: Start position")]
    [SerializeField]
    [Tooltip("The subject's shoulder start angle in degrees.")]
    [Range(-180.0f, 180.0f)]
    private float startShoulderAngle = -90.0f;

    [SerializeField]
    [Tooltip("The subject's elbow start angle in degrees.")]
    [Range(-180.0f, 180.0f)]
    private float startElbowAngle = 90.0f;

    [SerializeField]
    [Tooltip("The start angle tolerance in degrees.")]
    [Range(0.0f, 10.0f)]
    private float startTolerance = 2.0f;
    
    [Header("Experiment configuration: Reps and Sets")]
    [SerializeField]
    [Tooltip("The number of iterations per target.")]
    [Range(1, 100)]
    private int iterationsPerTarget = 1;

    [SerializeField]
    [Tooltip("The number of sessions for the experiment.")]
    [Range(1, 100)]
    private int sessions = 1;

    // iteration management variables
    private int iterationsPerSession;
    private int maxIterations;
    private int completedIterations = 0;

    // Instructions management
    private string infoText;

    // Motion tracking for experiment management (check for start position)
    private VIVETrackerManager upperArmTracker;
    private VIVETrackerManager lowerArmTracker;

    //
    // Data logging
    //
    private float taskTime = 0.0f;
    private DataStreamLogger motionLogger;
    private const string motionDataAbleFormat = "loc,t,aDotUA,bDotUA,gDotUA,aUA,bUA,gUA,xUA,yUA,zUA,aDotE,bDotE,gDotE,aE,bE,gE,xE,yE,zE,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,xSH,ySH,zSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xUB,yUB,zUB,xHand,yHand,zHand,aHand,bHand,gHand";


    private void Awake()
    {
        if (debug)
        {
            SaveSystem.LoadUserData("MD1942");
            //SaveSystem.LoadUserData("RG1988");
            //
            // Debug Able
            //
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);

        }
    }
            // Start is called before the first frame update
    void Start()
    {
        // Initialize ExperimentSystem
        InitExperimentSystem();

        // Initialize UI.
        InitializeUI();

        // Spawn grid
        gridManager.SpawnGrid(gridRows, gridColumns, gridSpacing);

        // Move subject to the centre of the experiment space
        if (!debug)
            TeleportToStartPosition();

        // Wait for 5 seconds at HelloWorld
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
                if (WaitFlag)
                {
                    if (debug)
                        TeleportToStartPosition();
                    HudManager.ClearText();
                    experimentState = ExperimentState.Initialising;
                }
                else
                {
                    HudManager.DisplayText("Welcome!");
                    InstructionManager.DisplayText("Hello world!");
                }
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
                    HudManager.DisplayText("Move to start", 2.0f);
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
                HudManager.DisplayText("Move to start", 2.0f);
                // Turn targets clear
                experimentState = ExperimentState.WaitingForStart;

                break;
            /*
             *************************************************
             *  WaitingForStart
             *************************************************
             */
            case ExperimentState.WaitingForStart:
                // Show status to subject
                infoText = GetInfoText();
                InstructionManager.DisplayText(infoText);

                // Check if pause requested
                UpdatePause();
                switch (waitState)
                {
                    // Waiting for subject to get to start position.
                    case WaitState.Waiting:
                        if (CheckReadyToStart())
                        {
                            startEnable = true;
                            waitState = WaitState.Countdown;
                        }
                        break;
                    // HUD countdown for reaching action.
                    case WaitState.Countdown:
                        // If all is good and haven't started counting, start.
                        if (startEnable && !counting && !countdownDone)
                        {
                            // Manage countdown
                            HudManager.ClearText();
                            StopHUDCountDown();
                            counting = true;
                            HUDCountDown(3);
                            // Select target
                            gridManager.SelectBall(iterationNumber - 1);
                        }
                        // If all is good and the countdownDone flag is raised, switch to reaching.
                        else if (countdownDone)
                        {
                            // Reset flags
                            startEnable = false;
                            counting = false;
                            countdownDone = false;
                            // Continue
                            experimentState = ExperimentState.PerformingTask;
                            waitState = WaitState.Waiting;
                            break;
                        }
                        // If hand goes out of target reset countdown and wait for position
                        else if (!CheckReadyToStart() && !countdownDone)
                        {
                            StopHUDCountDown();
                            startEnable = false;
                            counting = false;
                            countdownDone = false;
                            // Clear ball
                            gridManager.ResetBallSelection();
                            // Indicate to move back
                            HudManager.DisplayText("Move to start", 2.0f);
                            waitState = WaitState.Waiting;
                            break;
                        }
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
                    SetWaitFlag(RestTime);
                    experimentState = ExperimentState.Resting;
                }
                // Check whether the new session condition is met
                else if (CheckNextSessionCondition())
                {
                    experimentState = ExperimentState.InitializingNext;
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
                    iterationNumber++;
                    completedIterations++;

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
                ExperimentSystem.GetActiveLogger(1).AddNewLogFile(sessionNumber, iterationNumber, "Data format");

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
            float qShoulder = Mathf.Rad2Deg*(upperArmTracker.GetProcessedData(5) + Mathf.PI); // Offsetting to horizontal position being 0.
            float qElbow = Mathf.Rad2Deg * (lowerArmTracker.GetProcessedData(5)) - qShoulder; // Offsetting to horizontal position being 0.
            debugText.text += qShoulder.ToString() + "\n";
            debugText.text += qElbow.ToString() + "\n";
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
                    float[] sensorData = sensor.GetAllProcessedData();
                    foreach (float element in sensorData)
                        logData += "," + element.ToString();
                }
                // Read from all experiment sensors
                foreach (ISensor sensor in ExperimentSystem.GetActiveSensors())
                {
                    float[] sensorData = sensor.GetAllProcessedData();
                    foreach (float element in sensorData)
                        logData += "," + element.ToString();
                }

                //
                // Append data to lists
                //
                taskTime += Time.fixedDeltaTime;

                //
                // Log current data
                //
                //motionLogger.AppendData(logData);

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
                    //motionLogger.CloseLog();

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

    public override void InitialiseExperiment()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Gets the progress text to be displayed to the subject.
    /// </summary>
    /// <returns>The text to be displayed as a string.</returns>
    public override string GetDisplayInfoText()
    {
        string Text;
        Text = "Status: " + experimentState.ToString() + ".\n";
        Text += "Time: " + System.DateTime.Now.ToString("H:mm tt") + ".\n";
        return Text;
    }
    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// </summary>
    public override void InitExperimentSystem()
    {
        //
        // Set the experiment type configuration
        //
        experimentType = ExperimentType.TypeOne; // Able-bodied experiment type
        if (debug)
            ExperimentSystem.SetActiveExperimentID("GridReaching2019");

        //
        // Iterations configuration
        //
        // Make sure flow control is initialised
        sessionNumber = 1;
        iterationNumber = 1;
        // Set iterations variables for flow control.
        targetNumber = gridRows * gridColumns;
        iterationsPerSession = targetNumber * iterationsPerTarget;
        maxIterations = iterationsPerSession * sessions;
        // Create the list of target indexes and shuffle it.
        for (int i = 0; i < targetNumber; i++)
        {
            for (int j = 0; j < iterationsPerTarget; j++)
                targetOrder.Add(i);
        }
        targetOrder.Shuffle();

        //
        // Initialize world positioning
        //
        // Get user physiological data.
        float subjectHeight = SaveSystem.ActiveUser.height;
        float subjectArmLength = SaveSystem.ActiveUser.upperArmLength + SaveSystem.ActiveUser.forearmLength + (SaveSystem.ActiveUser.handLength / 2);
        // Set the grid distance from subject
        gridManager.transform.position = new Vector3(0.0f, gridHeightMultiplier * subjectHeight, gridReachMultiplier * subjectArmLength);

        //
        // Create data loggers
        //
        motionLogger = new DataStreamLogger("Motion");
        ExperimentSystem.AddExperimentLogger(motionLogger);


        //
        // Add arm and body motion trackers.
        //
        // Lower limb motion tracker
        GameObject llMotionTrackerGO = GameObject.FindGameObjectWithTag("ForearmTracker");
        lowerArmTracker = new VIVETrackerManager(llMotionTrackerGO.transform);
        ExperimentSystem.AddSensor(lowerArmTracker);

        // Upper limb motion tracker
        GameObject ulMotionTrackerGO = AvatarSystem.AddMotionTracker();
        upperArmTracker = new VIVETrackerManager(ulMotionTrackerGO.transform);
        ExperimentSystem.AddSensor(upperArmTracker);

        if (!debug)
        {
            // Shoulder acromium head tracker
            GameObject motionTrackerGO1 = AvatarSystem.AddMotionTracker();
            VIVETrackerManager shoulderTracker = new VIVETrackerManager(motionTrackerGO1.transform);
            ExperimentSystem.AddSensor(shoulderTracker);
            // C7 tracker
            GameObject motionTrackerGO2 = AvatarSystem.AddMotionTracker();
            VIVETrackerManager c7Tracker = new VIVETrackerManager(motionTrackerGO2.transform);
            ExperimentSystem.AddSensor(c7Tracker);
        }

        //
        // Hand tracking sensor
        //
        GameObject handGO = GameObject.FindGameObjectWithTag("Hand");
        VirtualPositionTracker handTracker = new VirtualPositionTracker(handGO.transform);
        ExperimentSystem.AddSensor(handTracker);

    }


    /// <summary>
    /// Checks whether the subject is ready to start performing the task.
    /// </summary>
    /// <returns>True if ready to start.</returns>
    public override bool CheckReadyToStart()
    {
        // Check that upper and lower arms are within the tolerated start position.
        float qShoulder = Mathf.Rad2Deg * (upperArmTracker.GetProcessedData(5) + Mathf.PI); // Offsetting to horizontal position being 0.
        float qElbow = Mathf.Rad2Deg * (lowerArmTracker.GetProcessedData(5)) - qShoulder; // Offsetting to horizontal position being 0.
        // The difference to the start position
        float qSDiff = qShoulder - startShoulderAngle;
        float qEDiff = qElbow - startElbowAngle;
        
        if (Mathf.Abs(qSDiff) < startTolerance && Mathf.Abs(qEDiff) < startTolerance)
            return true;
        // Provide instructions when not there yet
        else
        {
            string helpText = "";
            if (qSDiff < 0 && Mathf.Abs(qSDiff) > startTolerance)
                helpText += "Raise your upper-arm.\n";
            else if (qSDiff > 0 && Mathf.Abs(qSDiff) > startTolerance)
                helpText += "Lower your upper-arm.\n";

            if (qEDiff < 0 && Mathf.Abs(qEDiff) > startTolerance)
                helpText += "Raise your lower-arm.\n";
            else if (qEDiff > 0 && Mathf.Abs(qEDiff) > startTolerance)
                helpText += "Lower your lower-arm.\n";

            HudManager.DisplayText(helpText);
            return false;
        }
    }

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    public override bool CheckTaskCompletion()
    {
        //
        // Check that the selected ball has been touched.
        //
        return gridManager.SelectedTouched;
    }

    /// <summary>
    /// Checks if the condition for the rest period has been reached.
    /// </summary>
    /// <returns>True if the rest condition has been reached.</returns>
    public override bool CheckRestCondition()
    {
        return false;
    }

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    public override bool CheckNextSessionCondition()
    {
        return false;
    }

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    public override bool CheckEndCondition()
    {
        return completedIterations >= maxIterations;
    }

    /// <summary>
    /// Launches the next session. Performs all the required preparations.
    /// </summary>
    public override void ConfigureNextSession()
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

    #endregion


    /// <summary>
    /// Returns the progress update String
    /// </summary>
    /// <returns></returns>
    public string GetInfoText()
    {
        string Text;
        Text = "Status: " + experimentState.ToString() + ".\n";
        Text = "Progress: " + completedIterations + "/" + maxIterations + ".\n";
        Text += "Time: " + System.DateTime.Now.ToString("H:mm tt") + ".\n";
        return Text;
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
}
