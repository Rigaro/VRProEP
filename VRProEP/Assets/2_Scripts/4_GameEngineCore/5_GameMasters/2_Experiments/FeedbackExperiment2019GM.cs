using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.ExperimentCore;
using VRProEP.Utilities;

public class FeedbackExperiment2019GM : GameMaster
{

    private float taskTime = 0.0f;
        
    // Experiment configuration
    public enum FeedbackExperiment { Force, Roughness, Mixed }
    public enum VisualFeebackType {None, On }
    [Header("Experiment configuration")]
    public bool skipAll = false;
    public bool skipTraining = false;
    public List<float> forceTargets = new List<float> {0.2f, 0.5f, 0.8f};
    public List<Color> forceColours;
    public List<float> roughnessTargets = new List<float> { 0.0f, (300.0f/5900.0f), (650.0f/5900f)};
    public List<int> iterationsPerSessionPerSetting = new List<int> { 5, 5, 5, 5, 5, 5 };
    public List<int> trainingPerSessionPerSetting = new List<int> { 1, 1, 0, 1, 1, 0 };
    public List<FeedbackExperiment> sessionType = new List<FeedbackExperiment> { FeedbackExperiment.Force, FeedbackExperiment.Roughness, FeedbackExperiment.Mixed, FeedbackExperiment.Force, FeedbackExperiment.Roughness, FeedbackExperiment.Mixed }; //size 6 def.(Force Roughness Mixed Force Roughness Mixed)
    public List<VisualFeebackType> visualFeedbackType = new List<VisualFeebackType> { VisualFeebackType.On, VisualFeebackType.On, VisualFeebackType.On, VisualFeebackType.None, VisualFeebackType.None, VisualFeebackType.None }; // size 6 def.(on on on none none none)
    public int restIterations = 25;
    List<List<int[]>> experimentTargetList = new List<List<int[]>>();
    System.Random r = new System.Random();
    private int randIndex;

    [Header("Experiment objects")]
    public ForceTextureBehaviour experimentObject;
    public Transform dropOffTransform;
    public List<GameObject> offHandObjects;
    public List<GameObject> selectors;
    public bool isLefty = false;

    // Experiment management
    private List<int> iterationsPerSession = new List<int>();
    private List<int> trainingPerSession = new List<int>();
    private int numberOfIterations;
    private int iterationNumberTotal;
    private int iterationNumberCounterTotal;
    private bool hasFeedback = true;
    private bool trainingEnd = false;
    private bool inTraining = false;

    // Active targets: Use these to set what is the current iteration active force and roughness targets.
    private float activeForceTarget = 0.0f;
    private float activeRougnessTarget = 0.0f;
    private Color activeForceColor;

    // Instructions management
    private bool instructionsEnd = false;
    private bool inInstructions = false;
    private bool inSessionInstructions = false;
    private bool inSessionInstructionsEnd = false;
    private string infoText;
    private bool logEnd = false;
    private SteamVR_Action_Boolean buttonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ObjectInteractButton");

    // Balloon selection management
    private bool isSelecting = false;
    private GameObject activeSelector;

    // Prosthesis handling objects
    private GameObject prosthesisManagerGO;
    private FakeEMGBoniHand handManager;
    private GraspManager graspManager;

    // Data logging:
    private DataStreamLogger continuousLogger;
    private DataStreamLogger roughnessLogger;
    private const string continuousDataFormat = "t,emgA,emgB,force,roughness,forceTarget";
    private const string roughnessDataFormat = "i,selRoughness,objRoughness";

    // Performance evaluation
    private float selectedRoughness = 0.0f;

    // Other debug stuff
    private bool emulateEMGOff = false;

    private void Awake()
    {
        if (debug)
        {
            SaveSystem.LoadUserData("MD1942");

            //
            // Debug Able
            //
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);

            //sessionNumber++;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitExperimentSystem();
        InitializeUI();

        // Initialize iteration management.
        iterationNumberTotal = 0;
        for (int i = 0; i < sessionType.Count; i++)
        {
            switch (sessionType[i])
            {
                case FeedbackExperiment.Force:
                    iterationsPerSession.Add(iterationsPerSessionPerSetting[i] * forceTargets.Count);
                    trainingPerSession.Add(trainingPerSessionPerSetting[i] * forceTargets.Count);
                    //add targets for each iteration
                    List<int[]> forceTargetList = new List<int[]>();
                    for (int u = 0; u < iterationsPerSessionPerSetting[i]; u++)
                    {
                        for (int r = 0; r < forceTargets.Count; r++)
                        {
                            int[] parameterTarget = new int[2] { r, 0 };
                            forceTargetList.Add(parameterTarget);
                        }
                    }
                    experimentTargetList.Add(forceTargetList);
                    break;
                case FeedbackExperiment.Roughness:
                    iterationsPerSession.Add(iterationsPerSessionPerSetting[i] * roughnessTargets.Count);
                    trainingPerSession.Add(trainingPerSessionPerSetting[i] * roughnessTargets.Count);
                    //add targets for each iteration
                    List<int[]> roughnessTargetList = new List<int[]>();
                    for (int u = 0; u < iterationsPerSessionPerSetting[i]; u++)
                    {
                        for (int z = 0; z < roughnessTargets.Count; z++)
                        {
                            int[] parameterTarget = new int[2] { 0, z };
                            roughnessTargetList.Add(parameterTarget);
                        }
                    }
                    experimentTargetList.Add(roughnessTargetList);

                    break;
                case FeedbackExperiment.Mixed:
                    iterationsPerSession.Add(iterationsPerSessionPerSetting[i] * forceTargets.Count * roughnessTargets.Count);
                    trainingPerSession.Add(trainingPerSessionPerSetting[i] * forceTargets.Count * roughnessTargets.Count);
                    //add targets for each iteration
                    List<int[]> mixedTargetList = new List<int[]>();
                    for (int u = 0; u < iterationsPerSessionPerSetting[i]; u++)
                    {
                        for (int z = 0; z < roughnessTargets.Count; z++)
                        {
                            for (int r = 0; r < forceTargets.Count; r++)
                            {
                                int[] parameterTarget = new int[2] { r, z };
                                mixedTargetList.Add(parameterTarget);
                            }
                        }
                    }
                    experimentTargetList.Add(mixedTargetList);

                    break;
                default:
                    break;
            }
            iterationNumberTotal += iterationsPerSession[i];
        }
        iterationNumberCounterTotal = 1;

        //
        // Configure the grasp manager
        //
        GameObject graspManagerGO = GameObject.FindGameObjectWithTag("GraspManager");
        if (graspManagerGO == null)
            throw new System.Exception("Grasp Manager not found.");
        graspManager = graspManagerGO.GetComponent<GraspManager>();
        graspManager.managerType = GraspManager.GraspManagerType.Assisted;
        graspManager.managerMode = GraspManager.GraspManagerMode.Restriced;

        // Spawn off-hand used for selecting balloons
        SpawnOffHand();

        // Move subject to the centre of the experiment space
        TeleportToStartPosition();
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
                //
                // Give instructions
                //
                if (!inInstructions)
                    StartCoroutine(InstructionsLoop());

                //
                // DEBUG
                //
                //experimentState = ExperimentState.InitializingApplication;
                //
                // DEBUG
                //

                //
                // Go to Initializing Application
                //
                if (instructionsEnd)
                {
                    inInstructions = false;
                    hudManager.DisplayText("Ready to start!", 2.0f);
                    experimentState = ExperimentState.InitializingApplication;
                }
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
                LaunchNextSession();
                //StartCoroutine(ClearObjectFromHandCoroutine());
                UpdateForceAndRoughnessTargets();
                StartCoroutine(SpawnExperimentObject());

                //
                // Initialize data logs
                //
                if (sessionType[sessionNumber - 1] == FeedbackExperiment.Force || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                    continuousLogger.AddNewLogFile(sessionNumber, iterationNumber, continuousDataFormat);
                else if (sessionType[sessionNumber - 1] == FeedbackExperiment.Roughness || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                    roughnessLogger.AddNewLogFile(sessionNumber, iterationNumber, roughnessDataFormat);

                //
                // Go to training
                //
                if (skipAll)
                {
                    skipInstructions = true;
                }
                experimentState = ExperimentState.Training;
                break;
            /*
             *************************************************
             *  Practice
             *************************************************
             */
            // Perform initialization functions before starting experiment.
            case ExperimentState.Training:
                if (skipTraining)
                {
                    // Make sure everything is re-set
                    LaunchNextSession();
                    StartCoroutine(ClearObjectFromHandCoroutine());
                    UpdateForceAndRoughnessTargets();
                    // Go to instructions
                    experimentState = ExperimentState.GivingInstructions;
                    break;
                }
                //
                // Guide subject through training
                //
                if (!inTraining)
                    StartCoroutine(TrainingLoop());

                //
                // DEBUG
                //
                //experimentState = ExperimentState.GivingInstructions;
                //skipInstructions = true;
                //
                // DEBUG
                //

                //
                // Go to instructions
                //
                if (trainingEnd)
                {
                    // Make sure everything is re-set
                    LaunchNextSession();
                    StartCoroutine(ClearObjectFromHandCoroutine());
                    UpdateForceAndRoughnessTargets();
                    StartCoroutine(SpawnExperimentObject());

                    inTraining = false;
                    trainingEnd = false;
                    experimentState = ExperimentState.GivingInstructions;
                }
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
                    hudManager.DisplayText("Ready to start", 2.0f);
                    // Turn targets clear
                    experimentState = ExperimentState.WaitingForStart;
                    break;
                }

                //
                // Give instructions
                //
                if (!inSessionInstructions)
                    StartCoroutine(SessionInstructionLoop());

                //
                // Go to waiting for start
                //
                if (inSessionInstructionsEnd)
                {
                    startEnable = true;
                    inSessionInstructions = false;
                    hudManager.DisplayText("Ready to start!", 2.0f);
                    waitState = WaitState.Waiting;
                    experimentState = ExperimentState.WaitingForStart;
                }
                break;
            /*
             *************************************************
             *  WaitingForStart
             *************************************************
             */
            case ExperimentState.WaitingForStart:
                // Print status
                infoText = GetInfoText();
                instructionManager.DisplayText(infoText);

                // DEBUG
                // experimentState = ExperimentState.PerformingTask;
                // DEBUG


                // Check if pause requested
                UpdatePause();
                switch (waitState)
                {
                    // Waiting for subject to get to start position.
                    case WaitState.Waiting:
                        StartCoroutine(SpawnExperimentObject()); // Make sure object is in-hand
                        waitState = WaitState.Countdown;
                        break;
                    // HUD countdown for reaching action.
                    case WaitState.Countdown:
                        // If hand goes out of target reset countdown and wait for position
                        if (!startEnable && !countdownDone)
                        {
                            counting = false;
                            countdownDone = false;
                            // Indicate to move back
                            hudManager.DisplayText("Move to start", 2.0f);
                            waitState = WaitState.Waiting;
                            break;
                        }
                        // If all is good and haven't started counting, start.
                        if (!counting && !countdownDone)
                        {
                            counting = true;
                            HUDCountDown(0);
                        }
                        // If all is good and the countdownDone flag is raised, switch to reaching.
                        if (countdownDone)
                        {
                            handManager.ResetForce();
                            // Reset flags
                            counting = false;
                            countdownDone = false;
                            // Continue
                            experimentState = ExperimentState.PerformingTask;
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
                //
                // Task performance is handled deterministically in FixedUpdate.
                //
                // Display experiment information to subject.
                //
                infoText = GetInfoText();
                instructionManager.DisplayText(infoText);
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
                StartCoroutine(ClearObjectFromHandCoroutine());
                experimentObject.SetForce(0.0f);

                // 
                // Data logging
                //

                //
                // Flow managment
                //
                // Check whether the experiment end condition is met
                if (CheckEndCondition())
                {
                    hudManager.DisplayText("Experiment end. Thank you!", 6.0f);
                    experimentState = ExperimentState.End;
                }
                // Rest for some time when required
                else if (CheckRestCondition())
                {
                    hudManager.DisplayText("Take a " + restTime + " seconds rest.", 6.0f);
                    SetWaitFlag(restTime);
                    experimentState = ExperimentState.Resting;
                }
                // Check whether the new session condition is met
                else if (CheckNextSessionCondition())
                {
                    //iterations
                    hudManager.DisplayText("Good job!", 2.0f);
                    // Allow 3 seconds after task end to do calculations
                    SetWaitFlag(3.0f);
                    experimentState = ExperimentState.InitializingNextSession;
                }
                else
                {
                    //iterations
                    hudManager.DisplayText("Good job!", 2.0f);
                    // Allow 3 seconds after task end to do calculations
                    SetWaitFlag(3.0f);
                    experimentState = ExperimentState.UpdatingApplication;
                }
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
                    iterationNumberCounterTotal++;
                    taskTime = 0.0f;

                    //
                    // Initialize data logging
                    //
                    if (sessionType[sessionNumber - 1] == FeedbackExperiment.Force || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                        continuousLogger.AddNewLogFile(sessionNumber, iterationNumber, continuousDataFormat);

                    if (sessionType[sessionNumber - 1] == FeedbackExperiment.Roughness || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                        roughnessLogger.AddNewLogFile(sessionNumber, iterationNumber, roughnessDataFormat);


                    //
                    // Update objects
                    //
                    UpdateForceAndRoughnessTargets();
                    StartCoroutine(SpawnExperimentObject());
                    handManager.ResetForce();

                    //
                    //
                    //
                    // Go to start of next iteration
                    hudManager.DisplayText("Ready to start!", 2.0f);
                    //set object drop off
                    //
                    experimentState = ExperimentState.WaitingForStart;
                }
                break;
            /*
             *************************************************
             *  InitializingNext
             *************************************************
             */
            case ExperimentState.InitializingNextSession:
                if (WaitFlag)
                {
                    //
                    // Initialize new session variables and flow control
                    //
                    iterationNumber = 1;
                    sessionNumber++;
                    iterationNumberCounterTotal++;
                    taskTime = 0.0f;

                    //
                    // Initialize data logging
                    //
                    if (sessionType[sessionNumber - 1] == FeedbackExperiment.Force || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                        continuousLogger.AddNewLogFile(sessionNumber, iterationNumber, continuousDataFormat);

                    if (sessionType[sessionNumber - 1] == FeedbackExperiment.Roughness || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                        roughnessLogger.AddNewLogFile(sessionNumber, iterationNumber, roughnessDataFormat);

                    // Start next session immediately
                    LaunchNextSession();

                    //
                    // Update objects
                    //
                    LaunchNextSession();
                    UpdateForceAndRoughnessTargets();
                    StartCoroutine(SpawnExperimentObject());
                    handManager.ResetForce();

                    experimentState = ExperimentState.Training; // Go to training to check if needed
                }
                break;
            /*
             *************************************************
             *  Resting
             *************************************************
             */
            case ExperimentState.Resting:
                infoText = GetInfoText();
                instructionManager.DisplayText(infoText);
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
                    if (CheckEndCondition())
                    {
                        hudManager.DisplayText("Experiment end. Thank you!", 6.0f);
                        experimentState = ExperimentState.End;
                    }
                    else if (CheckNextSessionCondition())
                    {
                        //iterations
                        hudManager.DisplayText("Good job!", 2.0f);
                        // Allow 3 seconds after task end to do calculations
                        SetWaitFlag(3.0f);
                        experimentState = ExperimentState.InitializingNextSession;
                    }
                    else
                    {
                        hudManager.DisplayText("Get ready to restart!", 3.0f);
                        SetWaitFlag(5.0f);
                        experimentState = ExperimentState.UpdatingApplication;
                    }
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
                infoText = GetInfoText();
                instructionManager.DisplayText(infoText);

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
                EndExperiment();
                UpdateCloseApplication();
                break;
            default:
                break;
        }

        //
        // Update information displayed on monitor
        //
        //
        // Update HUD state
        //
        if (experimentState == ExperimentState.Resting || experimentState == ExperimentState.End)
        {
            hudManager.colour = HUDManager.HUDColour.Green;
        }
        else
        {
            if (!debug)
            {
                if (handManager.IsEnabled)
                    hudManager.colour = HUDManager.HUDColour.Red;
                else
                    hudManager.colour = HUDManager.HUDColour.Blue;
            }
        }
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
                logData += "," + activeForceTarget.ToString();

                //
                // Append data to lists
                //
                taskTime += Time.fixedDeltaTime;

                //
                // Log current data for continous type
                //
                if (sessionType[sessionNumber - 1] == FeedbackExperiment.Force || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                    continuousLogger.AppendData(logData);

                //
                // Save log and reset flags when successfully compeleted task
                //
                if (CheckTaskCompletion())
                {
                    //
                    // Log current data for roughness type
                    //
                    if (sessionType[sessionNumber - 1] == FeedbackExperiment.Roughness || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                    {
                        logData = iterationNumber.ToString();
                        logData += "," + selectedRoughness.ToString();
                        logData += "," + activeRougnessTarget.ToString();
                        //Debug.Log("Logging roughness: " + logData);
                        roughnessLogger.AppendData(logData);
                    }

                    //
                    // Save logger for current experiment and change to data analysis
                    //
                    if (sessionType[sessionNumber - 1] == FeedbackExperiment.Force || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                        continuousLogger.CloseLog();

                    if (sessionType[sessionNumber - 1] == FeedbackExperiment.Roughness || sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
                        roughnessLogger.CloseLog();

                    //
                    // Clear data management buffers
                    //

                    // Change state
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

        if (handManager != null) handManager.StopBoniConnection();

        //
        // Save and close all logs
        //
        ExperimentSystem.CloseAllExperimentLoggers();
    }

    /// <summary>
    /// Returns the progress update String
    /// </summary>
    /// <returns></returns>
    private string GetInfoText()
    {
        string Text;
        Text = "Status: " + experimentState.ToString() + ".\n";
        Text += "Session type: " + sessionType[sessionNumber - 1].ToString() + ".\n";
        Text += "Progress: " + (iterationNumberCounterTotal) + "/" + iterationNumberTotal + ".\n";
        Text += "Time: " + System.DateTime.Now.ToString("H:mm tt") + ".\n";
        return Text;
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
        experimentType = ExperimentType.TypeOne;
        if (debug)
            ExperimentSystem.SetActiveExperimentID("Feedback2019");

        //
        // Create data loggers
        //
        continuousLogger = new DataStreamLogger("Continous");
        ExperimentSystem.AddExperimentLogger(continuousLogger);
        roughnessLogger = new DataStreamLogger("Roughness");
        ExperimentSystem.AddExperimentLogger(roughnessLogger);

        // Restart EMG readings
        foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
        {
            if (sensor.GetSensorType().Equals(SensorType.EMGWiFi))
            {
                UDPSensorManager udpSensor = (UDPSensorManager)sensor;
                //Debug.Log(wifiSensor.RunThread);
                udpSensor.StartSensorReading();
                //Debug.Log(wifiSensor.RunThread);
            }
        }

        // Get hand object
        if(!debug)
        {
            prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            handManager = prosthesisManagerGO.GetComponent<FakeEMGBoniHand>() ?? throw new System.NullReferenceException("Prosthesis manager not found.");
            // Restart feedback connection
            //handManager.StartBoniConnection();
        }

        // Clear object
        experimentObject.gameObject.SetActive(false);
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
        // Task is completed when EMG is disabled and the subject has selected a balloon
        //
        if ((!debug && !handManager.IsEnabled) || (debug && (buttonAction.GetStateDown(SteamVR_Input_Sources.Any) || emulateEMGOff))) // Prosthesis not enabled (EMG)
        {
            if (!isSelecting) // If we haven't enabled the balloons, do so.
            {
                isSelecting = true;
                // Enable all balloons in active selector
                for( int i = 0; i < activeSelector.transform.childCount; i++)
                {
                    activeSelector.transform.GetChild(i).GetComponent<TouchyBallManager>().SetSelected();
                }

                // Debug stuff
                if (debug)
                    emulateEMGOff = true;

                return false;
            }
            else // Otherwise just check if any has been selected.
            {
                // Check if any selector has been touched
                for (int i = 0; i < activeSelector.transform.childCount; i++)
                {
                    if (activeSelector.transform.GetChild(i).GetComponent<TouchyBallManager>().BallState == TouchyBallManager.TouchyBallState.Correct)
                    {
                        selectedRoughness = roughnessTargets[i]; // Set the selected roughness by the ball number
                        //Debug.Log("Selected roughness: " + selectedRoughness);
                        // Reset the selectors
                        for (int j = 0; j < activeSelector.transform.childCount; j++)
                        {
                            if(j != i)
                                activeSelector.transform.GetChild(j).GetComponent<TouchyBallManager>().ClearSelection();
                        }

                        // Debug stuff
                        if (debug)
                            emulateEMGOff = false;

                        isSelecting = false; // clear variable and return
                        return true;
                    }
                }
                // If we got here we failed
                return false;
            }
        }
        else if(!debug && handManager.IsEnabled)
        {
            if (isSelecting) // If we havent disabled the balloons then do so.
            {
                isSelecting = false;
                // Enable all balloons in active selector
                for (int i = 0; i < activeSelector.transform.childCount; i++)
                {
                    activeSelector.transform.GetChild(i).GetComponent<TouchyBallManager>().ClearSelection();
                }
            }

            return false;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the condition for the rest period has been reached.
    /// </summary>
    /// <returns>True if the rest condition has been reached.</returns>
    protected override bool CheckRestCondition()
    {
        if (iterationNumberCounterTotal % restIterations == 0)
        {
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    protected override bool CheckNextSessionCondition()
    {
        if (iterationNumber >= iterationsPerSession[sessionNumber - 1])
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    protected override bool CheckEndCondition()
    {
        if (sessionNumber >= iterationsPerSession.Count && iterationNumber >= iterationsPerSession[sessionNumber - 1])
            return true;
        else
            return false;
    }

    /// <summary>
    /// Launches the next session. Performs all the required preparations.
    /// </summary>
    protected override void LaunchNextSession()
    {
        //No training in Session: Mixed or no Visual Feedback
        if (sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed || visualFeedbackType[sessionNumber - 1] == VisualFeebackType.None)
        {
            skipTraining = true;
        }
        else
        {
            skipTraining = false;
        }

        //check visual feedback on
        if(visualFeedbackType[sessionNumber - 1] == VisualFeebackType.On)
        {
            experimentObject.enableColourFeedback = true;
        }
        else if(visualFeedbackType[sessionNumber - 1] == VisualFeebackType.None)
        {
            experimentObject.enableColourFeedback = false;
        }

        // Clear visual feedback for Roughness type
        if (sessionType[sessionNumber - 1] == FeedbackExperiment.Roughness)
            experimentObject.enableColourFeedback = false;

        //check which feedback
        switch (sessionType[sessionNumber - 1])
        {
            //load force feedback assets
            case FeedbackExperiment.Force:

                // Set the active balloon set
                selectors[0].SetActive(true);
                activeSelector = selectors[0];
                selectors[1].SetActive(false);

                break;
            //load Roughness feedback assets
            case FeedbackExperiment.Roughness:

                // Set the active balloon set
                selectors[1].SetActive(true);
                activeSelector = selectors[1];
                selectors[0].SetActive(false);

                break;
            //load mixed feedback assets
            case FeedbackExperiment.Mixed:

                // Set the active balloon set
                selectors[1].SetActive(true);
                activeSelector = selectors[1];
                selectors[0].SetActive(false);

                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Finishes the experiment. Performs all the required procedures.
    /// </summary>
    protected override void EndExperiment()
    {
        //
        // Update log data and close logs.
        //
        if (!logEnd)
        {
            // Check if UDP sensors are available
            foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
            {
                if (sensor != null && sensor.GetSensorType().Equals(SensorType.Tactile) && sensor.GetSensorType().Equals(SensorType.EMGWiFi))
                {
                    UDPSensorManager udpSensor = (UDPSensorManager)sensor;
                    udpSensor.StopSensorReading();
                }
            }

            //
            // Save and close all logs
            //
            ExperimentSystem.CloseAllExperimentLoggers();

            logEnd = true;
        }

        //
        // Display information
        //
        instructionManager.DisplayText("End of experiment.\nThanks for your participation!\nYou can take the headset off.");
        hudManager.DisplayText("Experiment end.");

        //
        // Return to main menu ?
        //
    }

    #endregion

    #region Instruction Coroutines

    /// <summary>
    /// Training coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator TrainingLoop()
    {
        inTraining = true;
        trainingEnd = false;

        string defaultText = "Training:\n";
        string continueText = "\n\n...Press the Trigger to continue...";

        if (!skipTraining && trainingPerSession[sessionNumber - 1] >= 1)
        {
            hudManager.DisplayText("Please look at the monitor. Top-right.");
            yield return new WaitForSeconds(3.0f);
               
            // Introduce experiment modality.
            instructionManager.DisplayText("Welcome to the diamond factory training." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);

            switch (sessionType[sessionNumber - 1])
            {
                case FeedbackExperiment.Force://explain force limits X different once
                    // Set the active balloon set
                    selectors[0].SetActive(true);
                    activeSelector = selectors[0];
                    selectors[1].SetActive(false);

                    instructionManager.DisplayText(defaultText + "In this sessions' training you will use flexion and extension of your wrist to control the grasp force of a prosthetic hand to produce diamonds." + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText(defaultText + "The stones will be attached to the prosthetic hand automatically." + continueText);
                    SendObjectToHand();
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    //
                    // Colour instruction:
                    //
                    instructionManager.DisplayText(defaultText + "The stones colour will indicate the level of grasp force required to produce diamonds out of them." + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    // Low force: blue
                    instructionManager.DisplayText(defaultText + "Stones with blue colour need little grip force" + continueText);
                    hudManager.DisplayText("See, it's blue!");
                    // Set colour
                    experimentObject.SetRestColour(forceColours[0]);
                    // Wait for acknowledge
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);

                    // Mid force: yellow
                    instructionManager.DisplayText(defaultText + "Stones with yellow colour need medium grip force" + continueText);
                    hudManager.DisplayText("See, it's yellow!");
                    // Set colour
                    experimentObject.SetRestColour(forceColours[1]);
                    // Wait for acknowledge
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);

                    // High force: pink
                    instructionManager.DisplayText(defaultText + "Stones with pink colour need strong grip force" + continueText);
                    hudManager.DisplayText("See, it's pink!");
                    // Set colour
                    experimentObject.SetRestColour(forceColours[2]);
                    // Wait for acknowledge
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    hudManager.ClearText();


                    if (visualFeedbackType[sessionNumber - 1] == VisualFeebackType.On) //visual feedback
                    {
                        instructionManager.DisplayText(defaultText + "The stones colour will change according to the level of grasp force applied." + continueText);
                        yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                        yield return new WaitForSeconds(0.5f);
                        instructionManager.DisplayText(defaultText + "You should aim to change the stones colour into green." + continueText);
                        yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                        yield return new WaitForSeconds(0.5f);

                        // Target force: green
                        instructionManager.DisplayText(defaultText + "This is the colour you should aim for!" + continueText);
                        hudManager.DisplayText("See, it's green!");
                        // Set colour
                        experimentObject.SetRestColour(forceColours[3]);
                        // Wait for acknowledge
                        yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                        yield return new WaitForSeconds(0.5f);

                        instructionManager.DisplayText(defaultText + "If you squeeze too hard the diamond might break and the stone colour will turn red." + continueText);
                        yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                        yield return new WaitForSeconds(0.5f);
                    }
                    instructionManager.DisplayText(defaultText + "To start the experiment press the button." + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText(defaultText + "Then adjust the grip force via extension/flexion of your hand." + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText(defaultText + "After adjusting the force confirm via button. " + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText(defaultText + "This will turn the sphere blue, which means you can now touch it. " + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText(defaultText + "Touch the sphere with your hand to continue.");
                    yield return new WaitUntil(() => CheckTaskCompletion());
                    yield return new WaitForSeconds(0.5f);
                    if (visualFeedbackType[sessionNumber - 1] == VisualFeebackType.On) //visual feedback
                    {
                        //
                        //let them squeze with targetforclevel[0] + visual feedback
                        //
                        hudManager.DisplayText("Look at the screen.", 3.0f);
                        instructionManager.DisplayText(defaultText + "Lets train with little grasp force. Squeeze until it is green, you should feel some vibrations.");
                        // Set force and colour
                        experimentObject.SetTargetForce(forceTargets[0]);
                        experimentObject.SetRestColour(forceColours[0]);
                        yield return new WaitUntil(() => CheckTaskCompletion());
                        yield return new WaitForSeconds(0.5f);
                        experimentObject.SetForce(0.0f);
                        handManager.ResetForce();
                        hudManager.DisplayText("Good job!", 2.0f);
                        yield return new WaitForSeconds(3.0f);

                        //
                        //let them squeze with targetforclevel[1] + visual feedback
                        //
                        hudManager.DisplayText("Look at the screen.", 3.0f);
                        instructionManager.DisplayText(defaultText + "Lets train with medium grasp force. Squeeze until it is green, you should feel some vibrations.");
                        // Set force and colour
                        experimentObject.SetTargetForce(forceTargets[1]);
                        experimentObject.SetRestColour(forceColours[1]);
                        yield return new WaitUntil(() => CheckTaskCompletion());
                        yield return new WaitForSeconds(0.5f);
                        experimentObject.SetForce(0.0f);
                        handManager.ResetForce();
                        hudManager.DisplayText("Good job!", 2.0f);
                        yield return new WaitForSeconds(3.0f);
                        //
                        //let them squeze with targetforclevel[2] + visual feedback
                        //
                        hudManager.DisplayText("Look at the screen.", 3.0f);
                        instructionManager.DisplayText(defaultText + "Lets train with strong grasp force. Squeeze until it is green, you should feel some vibrations.");
                        // Set force and colour
                        experimentObject.SetTargetForce(forceTargets[2]);
                        experimentObject.SetRestColour(forceColours[2]);
                        yield return new WaitUntil(() => CheckTaskCompletion());
                        yield return new WaitForSeconds(0.5f);
                        experimentObject.SetForce(0.0f);
                        handManager.ResetForce();
                        hudManager.DisplayText("Good job!", 2.0f);
                        yield return new WaitForSeconds(3.0f);
                    }

                    instructionManager.DisplayText("Well done! With this your training ends." + continueText);
                    StartCoroutine(ClearObjectFromHandCoroutine());
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    break;
                case FeedbackExperiment.Roughness://roughness

                    // Set the active balloon set
                    selectors[1].SetActive(true);
                    activeSelector = selectors[1];
                    selectors[0].SetActive(false);

                    instructionManager.DisplayText("It is time to determine the quality of our diamonds by classifying them based on ther surface roughness." + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("The stones will be attached to the prosthetic hand automatically." + continueText);
                    experimentObject.SetRestColour(forceColours[0]);
                    SendObjectToHand();
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("The tactile feedback will vibrate with different frequencies according to the surface roughness of the stones." + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("Low frequencies will indicate a smooth surface while high frequencies will indicate a rough surface" + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("You will be asked to classify if the surface is smooth, medium or rough" + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("Grasp a stone by flexing your hand, grip harder and softer to feel the surface roughness." + continueText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);


                    //
                    //present targertroughness[0] and let them classify
                    // Set force and colour
                    experimentObject.SetTargetForce(forceTargets[1]);
                    experimentObject.SetRoughness(roughnessTargets[0]);
                    //
                    instructionManager.DisplayText("A smooth stone feels like this and is classified as Smooth. If you do not feel it, squeeze harder (flex wrist).");
                    hudManager.DisplayText("Squeeze it!", 3.0f);
                    yield return new WaitUntil(() => CheckTaskCompletion());
                    yield return new WaitForSeconds(0.5f);
                    experimentObject.SetForce(0.0f);
                    handManager.ResetForce();
                    hudManager.DisplayText("Good job!", 2.0f);
                    yield return new WaitForSeconds(3.0f);
                    //
                    //present targertroughness[1] and let them classify
                    experimentObject.SetRoughness(roughnessTargets[1]);
                    //
                    instructionManager.DisplayText("A medium rough stone feels like this and is classified as Mid. If you do not feel it, squeeze harder (flex wrist).");
                    hudManager.DisplayText("Squeeze it!", 3.0f);
                    yield return new WaitUntil(() => CheckTaskCompletion());
                    yield return new WaitForSeconds(0.5f);
                    experimentObject.SetForce(0.0f);
                    handManager.ResetForce();
                    hudManager.DisplayText("Good job!", 2.0f);
                    yield return new WaitForSeconds(3.0f);
                    //
                    //present targertroughness[2] and let them classify
                    experimentObject.SetRoughness(roughnessTargets[2]);
                    //
                    instructionManager.DisplayText("A rough stone feels like this and is classified as sphere Rough. If you do not feel it, squeeze harder (flex wrist).");
                    hudManager.DisplayText("Squeeze it!", 3.0f);
                    yield return new WaitUntil(() => CheckTaskCompletion());
                    yield return new WaitForSeconds(0.5f);
                    experimentObject.SetForce(0.0f);
                    handManager.ResetForce();
                    hudManager.DisplayText("Good job!", 2.0f);
                    yield return new WaitForSeconds(3.0f);

                    instructionManager.DisplayText("Well done! With this your training ends." + continueText);
                    StartCoroutine(ClearObjectFromHandCoroutine());
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    break;
                case FeedbackExperiment.Mixed://mixed
                    throw new System.NotImplementedException();
                default:
                    break;
            }
        }

        trainingEnd = true;
    }

    /// <summary>
    /// Instruction coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator InstructionsLoop()
    {
            inInstructions = true;
            instructionsEnd = false;

            string defaultText = "Instructions:\n";
            string continueText = "\n\n...Press the Trigger to continue...";

            instructionManager.DisplayText(defaultText + "Welcome on diamond range. Today we are going to produce some diamonds out of stones. Come on, we have to start." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Different coloured stones will be presented to you and you will have to squeeze them with different strength." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Furthermore the quality of the diamonds is determined by the surface roughness of them and you therefore will have to put them in different categories." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "To earn our dinner today we need to produce " + iterationNumberTotal + " diamonds." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "The grasping will be controlled by your EMG activity controlling the grasping force via flexing/extending your hand." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Different tactile feedback will be given to you, as explained before the experiment." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "You will get " + restTime + " seconds rest every " + restIterations + " iterations." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Your HUD will indicate when it is time to rest by turning green." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Your progress will be displayed here along with the status of the experiment." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "If you need any rest please request it to the experimenter." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "If you feel dizzy or want to stop the experiment please let the experimenter know immediately." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Remember that objects in VR are not physical so do not try to lean or support on them, particularly on the virtual desk in front of you while performing the task." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "All the information regarding the task will be displayed on your HUD." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Your progress will be displayed here along with the current time." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Get ready to start training!" + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);

        instructionsEnd = true;
    }

    /// <summary>
    /// Session Instruction coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator SessionInstructionLoop()
    {
        inSessionInstructions = true;
        inSessionInstructionsEnd = false;

        string defaultText = "Session instructions:\n";
        string continueText = "\n\n...Press the Trigger to continue...";

        switch (sessionType[sessionNumber - 1])
        {
            case FeedbackExperiment.Force:
                instructionManager.DisplayText(defaultText + "In this session you will use flexion and extension of your wrist..." + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText(defaultText + "...to control the grasp force of the prosthetic hand as shown in the training and produce diamonds." + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText(defaultText + "Start the experiment by pressing the button" + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText(defaultText + "After adjusting the force press the button again to enable the selection sphere." + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);

                break;
            case FeedbackExperiment.Roughness://roughness
                instructionManager.DisplayText(defaultText + "In this session you will get feedback about the stones surface roughness." + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText(defaultText + "Classify the stones according to ther surface roughness as shown in the training." + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);

                break;
            case FeedbackExperiment.Mixed://mixed
                instructionManager.DisplayText(defaultText + "In this session you will use flexion and extension of your hand to control..." + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText(defaultText + "...the grasp force of the prosthetic hand and will get feedback about the stones surface roughness as shown in the training." + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText(defaultText + "Start the experiment by pressing the button" + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText(defaultText + "After adjusting the force press the button again to enable the selection balloons." + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText(defaultText + "Then you can touch the suitable balloon: Smooth-Mid-Rough" + continueText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                break;
            default:
                break;
        }

        instructionManager.DisplayText("Get ready to start! Look forward towards the prosthetic hand.");
        hudManager.DisplayText("Look forward.", 3.0f);
        yield return new WaitForSeconds(5.0f);
        HUDCountDown(3);
        yield return new WaitForSeconds(5.0f);

        inSessionInstructionsEnd = true;
    }


    #endregion

    
    private void SpawnOffHand()
    {
        // Get hand
        SteamVR_Behaviour_Pose offHandPose;
        if (isLefty)
        {
            offHandObjects[0].SetActive(false);
            offHandObjects[1].SetActive(true);
            offHandPose = offHandObjects[1].GetComponent<SteamVR_Behaviour_Pose>();
        }
        else
        {
            offHandObjects[0].SetActive(true);
            offHandObjects[1].SetActive(false);
            offHandPose = offHandObjects[0].GetComponent<SteamVR_Behaviour_Pose>();
        }
        // Get player
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
            throw new System.Exception("Player not found.");
        // Set skeleton behaviour origin to player transform
        offHandPose.origin = playerGO.transform;
    }

    /// <summary>
    /// Updates the force and targets targets with the current iteration and session.
    /// </summary>
    private void UpdateForceAndRoughnessTargets()
    {
        if (experimentTargetList[sessionNumber - 1].Count - 1 >= 0)
        {
            //random value in Range of List at certain session
            randIndex = r.Next(experimentTargetList[sessionNumber - 1].Count - 1);
            //select targets via indeces stored in TargetList
            activeForceTarget = forceTargets[experimentTargetList[sessionNumber - 1][randIndex][0]];
            activeForceColor = forceColours[experimentTargetList[sessionNumber - 1][randIndex][0]];
            activeRougnessTarget = roughnessTargets[experimentTargetList[sessionNumber - 1][randIndex][1]];

            //delete Target from List
            experimentTargetList[sessionNumber - 1].Remove(experimentTargetList[sessionNumber - 1][randIndex]);
        }
    }

    /// <summary>
    /// Spawns the experiment object
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnExperimentObject()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        if (sessionType[sessionNumber - 1] == FeedbackExperiment.Force)
        {
            // Set the target force and rest colour
            experimentObject.SetTargetForce(activeForceTarget);
            experimentObject.SetRestColour(activeForceColor);
        }
        else if (sessionType[sessionNumber - 1] == FeedbackExperiment.Roughness)
        {
            // Set the object roughness
            experimentObject.SetRoughness(activeRougnessTarget);
        }
        else if (sessionType[sessionNumber - 1] == FeedbackExperiment.Mixed)
        {
            // Set the target force and rest colour
            experimentObject.SetTargetForce(activeForceTarget);
            experimentObject.SetRestColour(activeForceColor);
            // Set the object roughness
            experimentObject.SetRoughness(activeRougnessTarget);
        }
        else
            throw new System.Exception("The session type " + sessionType[sessionNumber - 1] + " is unavailable.");

        SendObjectToHand();
    }

    private void SendObjectToHand()
    {
        // Send the object to the hand to automatically grab it.
        experimentObject.gameObject.SetActive(true);
        experimentObject.transform.position = graspManager.transform.position;
    }

    /// <summary>
    /// Clears the experiment object from hand, making it drop.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ClearObjectFromHandCoroutine()
    {
        // Move the drop-off to hand to trigger release
        dropOffTransform.position = graspManager.transform.position;
        yield return new WaitForSecondsRealtime(0.5f);
        dropOffTransform.position = Vector3.zero; // Reset it to avoid issues
        // Hide object
        experimentObject.SetForce(0.0f);
        experimentObject.transform.position = Vector3.zero;
        experimentObject.gameObject.SetActive(false);
    }


}
