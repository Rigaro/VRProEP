using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// GameMaster includes
using VRProEP.ExperimentCore;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;

public class JacobianExperimentGM : GameMaster
{
    [Header("Jacobian Synergy Experiment")]
    [Tooltip("The GameObject used for the experiment task.")]
    public GameObject graspTaskObject;
    public List<GameObject> dropOffLocations = new List<GameObject>();
    public int iterationsPerDropOff = 20;

    private GraspTaskManager taskManager;
    private GameObject activeDropOff;
    private int activeDropOffNumber;
    private List<int> remainingDropOffIterations;
    private int iterationLimit;

    // Data logging:
    private DataStreamLogger motionLogger;
    private const string motionDataAbleFormat = "loc,t,aDotS,bDotS,gDotS,aS,bS,gS,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xHand,yHand,zHand,aHand,bHand,gHand";
    private const string motionDataEMGFormat = "loc,t,aDotS,bDotS,gDotS,aS,bS,gS,qE,qDotE,emg,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xHand,yHand,zHand,aHand,bHand,gHand";
    private const string motionDataSynFormat = "loc,t,aDotS,bDotS,gDotS,aS,bS,gS,qE,qDotE,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xHand,yHand,zHand,aHand,bHand,gHand";
    private string motionDataFormat;
    private float taskTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (debug)
        {
            SaveSystem.LoadUserData("RG1988");
            AvatarSystem.LoadPlayer(UserType.AbleBodied, AvatarType.AbleBodied);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);
        }
        // Initialize ExperimentSystem
        InitExperimentSystem();
        
        // Initialize UI.
        InitializeUI();

        // Check that drop-off locations have been set.
        if (dropOffLocations.Count <= 0)
            throw new System.Exception("Drop-off locations not set.");

        // Get the task manager from object.
        taskManager = graspTaskObject.GetComponent<GraspTaskManager>();    
        taskManager.SetObjectEnable(false); // Disable and hide object.

        // Initialize iteration management.
        remainingDropOffIterations = new List<int>(dropOffLocations.Count);
        for (int i = 0; i < dropOffLocations.Count; i++)
            remainingDropOffIterations.Add(iterationsPerDropOff);
        iterationLimit = iterationsPerDropOff * dropOffLocations.Count;

        // Hide all drop-offs.
        foreach (GameObject dropOff in dropOffLocations)
            dropOff.SetActive(false);

        // Configure the grasp manager
        GameObject graspManagerGO = GameObject.FindGameObjectWithTag("GraspManager");
        if (graspManagerGO == null)
            throw new System.Exception("Grasp Manager not found.");
        GraspManager graspManager = graspManagerGO.GetComponent<GraspManager>();
        graspManager.managerType = GraspManager.GraspManagerType.Assisted;
        graspManager.managerMode = GraspManager.GraspManagerMode.Restriced;

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
            case ExperimentState.HelloWorld:
                if (WaitFlag)
                {
                    hudManager.ClearText();
                    experimentState = ExperimentState.InitializingApplication;
                }
                else
                {
                    hudManager.DisplayText("Welcome!");
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
                // Set the initial drop-off.
                SetNextDropOff();
                // Enable colliders
                AvatarSystem.EnableAvatarColliders();

                //
                // Initialize data logs
                //
                motionLogger.AddNewLogFile(1, iterationNumber, motionDataFormat);

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
                    hudManager.DisplayText("Start!", 2.0f);
                    // Enable task object, drop-off and start
                    taskManager.SetObjectEnable(true);
                    activeDropOff.SetActive(true);
                    experimentState = ExperimentState.WaitingForStart;
                    break;
                }

                //
                // Give instructions
                //

                //
                // Go to waiting for start
                //
                hudManager.DisplayText("Start!", 2.0f);
                // Enable task object, drop-off and start
                taskManager.SetObjectEnable(true);
                activeDropOff.SetActive(true);
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
                    // Waiting for subject to grab the object.
                    case WaitState.Waiting:
                        if (taskManager.RunFlag)
                        {
                            hudManager.ClearText();
                            taskTime = 0.0f;
                            experimentState = ExperimentState.PerformingTask;
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
                hudManager.DisplayText("Good job!", 2.0f);
                // Allow 3 seconds after task end to do calculations
                SetWaitFlag(3.0f);
                // Disable drop-off location.
                activeDropOff.SetActive(false);

                //
                // Data analysis and calculations
                //

                //
                // Adaptive system update (when available)
                //

                // 
                // Data logging and log management
                //
                motionLogger.CloseLog();

                //
                // Flow managment
                //
                remainingDropOffIterations[activeDropOffNumber]--;
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

                    //
                    // Update experiment object
                    //
                    // Enable new drop-off location.
                    SetNextDropOff();

                    // 
                    // Update log requirements
                    //
                    motionLogger.AddNewLogFile(1, iterationNumber, motionDataFormat);

                    //
                    // Go to start of next iteration
                    //
                    hudManager.DisplayText("Start!", 2.0f);
                    // Enable task object, drop-off and start
                    taskManager.SetObjectEnable(true);
                    activeDropOff.SetActive(true);
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
                    hudManager.DisplayText("Get ready!", 3.0f);
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
        if (true)
        {
            string debugText = experimentState.ToString() + ".\n";
            if (experimentState == ExperimentState.WaitingForStart)
                debugText += waitState.ToString() + ".\n";
            debugText += "Iteration: " + iterationNumber + "/" + iterationLimit + ".\n";
            debugText += "Active drop-off: " + activeDropOffNumber + ".\n";
            int j = 0;
            foreach (int remainingIterations in remainingDropOffIterations)
            {
                debugText += "Remaining drop-off #" + j + ": " + remainingIterations + ".\n";
                j++;
            }
            instructionManager.DisplayText(debugText);
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
                string logData = activeDropOffNumber.ToString();
                logData += "," + taskTime.ToString();
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
                // Update data and append
                //
                taskTime += Time.fixedDeltaTime;


                //
                // Log current data
                //
                motionLogger.AppendData(logData);

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
        if (AvatarSystem.AvatarType == AvatarType.AbleBodied)
        {
            experimentType = ExperimentType.TypeOne;
            ExperimentSystem.SetActiveExperimentID("Jacobian/Able");
            motionDataFormat = motionDataAbleFormat;
        }
        else if (AvatarSystem.AvatarType == AvatarType.Transhumeral)
        {
            // Check if EMG is available
            bool EMGAvailable = false;
            foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
            {
                if (sensor.GetSensorType().Equals(SensorType.EMGWiFi))
                    EMGAvailable = true;
            }
            // Set whether emg or synergy based
            if (EMGAvailable)
            {
                experimentType = ExperimentType.TypeTwo;
                ExperimentSystem.SetActiveExperimentID("Jacobian/EMG");
                motionDataFormat = motionDataEMGFormat;
            }
            else
            {
                experimentType = ExperimentType.TypeThree;
                ExperimentSystem.SetActiveExperimentID("Jacobian/Syn");
                motionDataFormat = motionDataSynFormat;
            }
        }
        else
            throw new System.NotImplementedException();

        //
        // Create data loggers
        //
        motionLogger = new DataStreamLogger("Motion");
        ExperimentSystem.AddExperimentLogger(motionLogger);

        //
        // Check and add experiment sensors
        //
        //
        // Add VIVE Trackers.
        //
        if (experimentType == ExperimentType.TypeOne) // Additional tracker for upper arm when able-bodied type
        {
            GameObject motionTrackerGO = AvatarSystem.AddMotionTracker();
            VIVETrackerManager upperArmTracker = new VIVETrackerManager(motionTrackerGO.transform);
            ExperimentSystem.AddSensor(upperArmTracker);
        }
        else if (experimentType == ExperimentType.TypeTwo) // EMG case
        {
            // Set EMG sensor and reference generator as active.
            // Get prosthesis
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            ConfigurableElbowManager elbowManager = prosthesisManagerGO.GetComponent<ConfigurableElbowManager>();
            // Set the reference generator to jacobian-based.
            elbowManager.ChangeSensor("VAL_SENSOR_SEMG");
            elbowManager.ChangeReferenceGenerator("VAL_REFGEN_EMGPROP");
        }
        else if (experimentType == ExperimentType.TypeThree) // Jacobian synergy case
        {
            // Set VIVE tracker and Jacobian synergy as active.
            // Get prosthesis
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            ConfigurableElbowManager elbowManager = prosthesisManagerGO.GetComponent<ConfigurableElbowManager>();
            // Set the reference generator to jacobian-based.
            elbowManager.ChangeSensor("VAL_SENSOR_VIVETRACKER");
            elbowManager.ChangeReferenceGenerator("VAL_REFGEN_JACOBIANSYN");
        }
        // Shoulder acromium head tracker
        GameObject motionTrackerGO1 = AvatarSystem.AddMotionTracker();
        VIVETrackerManager shoulderTracker = new VIVETrackerManager(motionTrackerGO1.transform);
        ExperimentSystem.AddSensor(shoulderTracker);
        // C7 tracker
        GameObject motionTrackerGO2 = AvatarSystem.AddMotionTracker();
        VIVETrackerManager c7Tracker = new VIVETrackerManager(motionTrackerGO2.transform);
        ExperimentSystem.AddSensor(c7Tracker);

        //
        // Hand tracking sensor
        //
        GameObject handGO = GameObject.FindGameObjectWithTag("Hand");
        VirtualPositionTracker handTracker = new VirtualPositionTracker(handGO.transform);
        ExperimentSystem.AddSensor(handTracker);
    }

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    public override bool CheckTaskCompletion()
    {
        //
        // Perform some condition testing
        //
        if (taskManager.SuccessFlag)
        {
            return true;
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
        if (iterationNumber >= iterationLimit)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Launches the next session. Performs all the required preparations.
    /// </summary>
    public override void LaunchNextSession()
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

    /// <summary>
    /// Determines the next drop-off location.
    /// </summary>
    private void SetNextDropOff()
    {
        // Get the drop-off numbers that are still available.
        List<int> dropOffNumList = new List<int>();
        for (int i = 0; i < dropOffLocations.Count; i++)
        {
            if (remainingDropOffIterations[i] > 0)
                dropOffNumList.Add(i);
        }
        // Select one randomly.
        activeDropOffNumber = dropOffNumList[Random.Range(0, dropOffNumList.Count)];
        // Set as active.
        activeDropOff = dropOffLocations[activeDropOffNumber];
    }

    #endregion
}
