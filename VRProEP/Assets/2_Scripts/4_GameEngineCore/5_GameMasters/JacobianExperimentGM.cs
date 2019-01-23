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
    public int trainingIterations = 20;
    public int restIterations = 25;
    public bool skipTraining = false;

    // Experiment management
    private GraspTaskManager taskManager;
    private GameObject activeDropOff;
    private int activeDropOffNumber;
    private List<int> remainingDropOffIterations;
    private int iterationLimit;
    private bool trainingEnd = false;
    private bool inTraining = false;
    private bool instructionsEnd = false;
    private bool inInstructions = false;
    private string infoText;
    private bool logEnd = false;

    // Data logging:
    private DataStreamLogger motionLogger;
    private const string motionDataAbleFormat = "loc,t,aDotS,bDotS,gDotS,aS,bS,gS,aDotE,bDotE,gDotE,aE,bE,gE,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xHand,yHand,zHand,aHand,bHand,gHand";
    private const string motionDataEMGFormat = "loc,t,aDotS,bDotS,gDotS,aS,bS,gS,qE,qDotE,tpad,enable,emg,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xHand,yHand,zHand,aHand,bHand,gHand";
    private const string motionDataSynFormat = "loc,t,aDotS,bDotS,gDotS,aS,bS,gS,qE,qDotE,tpad,enable,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xHand,yHand,zHand,aHand,bHand,gHand";
    private string motionDataFormat;
    private float taskTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (debug)
        {
            SaveSystem.LoadUserData("MD1942");
            //AvatarSystem.LoadPlayer(UserType.AbleBodied, AvatarType.AbleBodied);
            //AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);
            AvatarSystem.LoadPlayer(UserType.AbleBodied, AvatarType.Transhumeral);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);
            // Initialize prosthesis
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            ConfigurableElbowManager elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
            elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + SaveSystem.ActiveUser.handLength / 2.0f));
            // Set the reference generator to jacobian-based.
            elbowManager.ChangeReferenceGenerator("VAL_REFGEN_JACOBIANSYN");
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
                    instructionManager.DisplayText("Hello world!");
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
                if (!inTraining)
                    StartCoroutine(TrainingLoop());

                //
                // Go to instructions
                //
                if (trainingEnd)
                {
                    inTraining = false;
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
                // Guide subject through training
                //
                if (!inInstructions)
                    StartCoroutine(InstructionsLoop());

                //
                // Go to waiting for start
                //
                if (instructionsEnd)
                {
                    inInstructions = false;
                    hudManager.DisplayText("Start!", 2.0f);
                    // Enable task object, drop-off and start
                    taskManager.SetObjectEnable(true);
                    activeDropOff.SetActive(true);
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
                infoText = "Status: Waiting to start.\n";
                infoText += "Progress: " + iterationNumber + "/" + iterationLimit + ".\n";
                infoText += "Time: " + System.DateTime.Now.ToString("H:mm tt") + ".\n";
                instructionManager.DisplayText(infoText);
                // Enable task object, drop-off if not done
                if (!taskManager.enableFlag)
                {
                    taskManager.SetObjectEnable(true);
                    activeDropOff.SetActive(true);
                }
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
                //
                // Task performance is handled deterministically in FixedUpdate.
                //
                // Display experiment information to subject.
                //
                infoText = "Status: Performing task.\n";
                infoText += "Progress: " + iterationNumber + "/" + iterationLimit + ".\n";
                infoText += "Time: " + System.DateTime.Now.ToString("H:mm tt") + ".\n";
                instructionManager.DisplayText(infoText);

                break;
            /*
             *************************************************
             *  AnalizingResults
             *************************************************
             */
            case ExperimentState.AnalizingResults:
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
                if (CheckEndCondition())
                {
                    hudManager.DisplayText("Experiment end. Thank you!", 6.0f);
                    experimentState = ExperimentState.End;
                }
                // Check whether the new session condition is met
                else if (CheckNextSessionCondition())
                {
                    experimentState = ExperimentState.InitializingNextSession;
                }
                // Rest for some time when required
                else if (CheckRestCondition())
                {
                    hudManager.DisplayText("Take a " + restTime + " seconds rest.", 6.0f);
                    SetWaitFlag(restTime);
                    experimentState = ExperimentState.Resting;
                }
                else
                {
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
                infoText = "Status: Resting.\n";
                infoText += "Progress: " + iterationNumber + "/" + iterationLimit + ".\n";
                infoText += "Time: " + System.DateTime.Now.ToString("H:mm tt") + ".\n";
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
                infoText = "Status: Experiment paused.\n";
                infoText += "Progress: " + iterationNumber + "/" + iterationLimit + ".\n";
                infoText += "Time: " + System.DateTime.Now.ToString("H:mm tt") + ".\n";
                instructionManager.DisplayText(infoText);
                // Disable task object, drop-off
                if (taskManager.enableFlag)
                {
                    taskManager.SetObjectEnable(false);
                    activeDropOff.SetActive(false);
                }
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
                EndExperiment();
                UpdateCloseApplication();
                break;
            default:
                break;
        }
        //
        // Update information displayed on instructions monitor
        //

        //
        // Update information displayed on monitor
        //


        //
        // Update information displayed for debugging purposes
        //
        if (false)
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
        // Handle application quit procedures, make sure everything is stopped properly.
        //
        // Check if WiFi sensors are available
        foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
        {
            if (sensor != null && sensor.GetSensorType().Equals(SensorType.EMGWiFi))
            {
                WiFiSensorManager wifiSensor = (WiFiSensorManager)sensor;
                wifiSensor.StopSensorReading();
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
                {
                    EMGAvailable = true;
                    WiFiSensorManager wifiSensor = (WiFiSensorManager)sensor;
                    //Debug.Log(wifiSensor.RunThread);
                    wifiSensor.StartSensorReading();
                    //Debug.Log(wifiSensor.RunThread);
                }
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
        if (experimentType == ExperimentType.TypeOne && !debug) // Additional trackers for upper and lower arm when able-bodied type
        {
            GameObject ulMotionTrackerGO = AvatarSystem.AddMotionTracker();
            VIVETrackerManager upperArmTracker = new VIVETrackerManager(ulMotionTrackerGO.transform);
            ExperimentSystem.AddSensor(upperArmTracker);

            GameObject llMotionTrackerGO = AvatarSystem.AddMotionTracker();
            VIVETrackerManager lowerArmTracker = new VIVETrackerManager(llMotionTrackerGO.transform);
            ExperimentSystem.AddSensor(lowerArmTracker);
        }
        else if (experimentType == ExperimentType.TypeTwo) // EMG case
        {
            // Set EMG sensor and reference generator as active.
            // Get prosthesis
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            ConfigurableElbowManager elbowManager = prosthesisManagerGO.GetComponent<ConfigurableElbowManager>();
            // Set active sensor and reference generator to EMG.
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
        if (iterationNumber % restIterations == 0)
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
        //
        // Update log data and close logs.
        //
        if (!logEnd)
        {
            // Check if WiFi sensors are available
            foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
            {
                if (sensor != null && sensor.GetSensorType().Equals(SensorType.EMGWiFi))
                {
                    WiFiSensorManager wifiSensor = (WiFiSensorManager)sensor;
                    wifiSensor.StopSensorReading();
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

    #region Instruction Coroutines

    /// <summary>
    /// Training coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator TrainingLoop()
    {
        // Look in the direction of the monitor.
        inTraining = true;
        trainingEnd = false;
        hudManager.DisplayText("Please look at the monitor.");
        yield return new WaitForSeconds(3.0f);

        // Introduce experiment modality.
        instructionManager.DisplayText("Welcome to the space prosthesis training facility.");
        yield return new WaitForSeconds(8.0f);
        instructionManager.DisplayText("Make sure you are standing on top of the green circle.");
        yield return new WaitForSeconds(10.0f);
        instructionManager.DisplayText("Do not step outside of the circle for the duration of the experiment.");
        yield return new WaitForSeconds(10.0f);

        if (experimentType == ExperimentType.TypeOne)
        {
            instructionManager.DisplayText("Today we will be testing your forward reaching capabilities using your actual arm.");
            yield return new WaitForSeconds(7.0f);
            instructionManager.DisplayText("You should already be familiar with it. ;)");
            yield return new WaitForSeconds(7.0f);

        }
        else if (experimentType == ExperimentType.TypeTwo)
        {
            instructionManager.DisplayText("Today we will be testing your forward reaching capabilities using a traditional prosthesis.");
            yield return new WaitForSeconds(7.0f);
            instructionManager.DisplayText("You should be familiar with how to make the prosthesis move using your muscles.");
            yield return new WaitForSeconds(7.0f);
        }
        else if (experimentType == ExperimentType.TypeThree)
        {
            instructionManager.DisplayText("Today we will be testing your forward reaching capabilities using an experimental prosthesis.");
            yield return new WaitForSeconds(7.0f);
            instructionManager.DisplayText("You should be familiar with how the prosthesis moves accoring to the motion of your arm.");
            yield return new WaitForSeconds(7.0f);
        }

        if (!skipTraining)
        {
            instructionManager.DisplayText("You will be required to grasp an object in front of you and place it on a shelf.");
            yield return new WaitForSeconds(7.0f);
            instructionManager.DisplayText("You should not step when reaching for the object or dropping it off.");
            yield return new WaitForSeconds(7.0f);

            // Look in the direction of the task.
            instructionManager.DisplayText("Please look forward.");
            hudManager.DisplayText("Look forward.", 3.0f);
            yield return new WaitForSeconds(4.0f);

            // Show object
            hudManager.DisplayText("The object will appear on the +.");
            yield return new WaitForSeconds(4.0f);
            taskManager.SetObjectEnable(true);
            hudManager.DisplayText("You can see it now.");
            yield return new WaitForSeconds(3.0f);
            taskManager.SetObjectEnable(false);

            // Show drop-off points
            hudManager.DisplayText("You'll need to drop it off...");
            yield return new WaitForSeconds(4.0f);
            hudManager.DisplayText("on the shelf in front of you.");
            yield return new WaitForSeconds(4.0f);
            hudManager.DisplayText("The drop-off location will be...");
            yield return new WaitForSeconds(4.0f);
            hudManager.DisplayText("shown to you like this...");
            yield return new WaitForSeconds(4.0f);
            int dropNum = 1;
            foreach (GameObject dropOff in dropOffLocations)
            {
                dropOff.SetActive(true);
                hudManager.DisplayText("Drop-off #" + dropNum + ".");
                dropNum++;
                yield return new WaitForSeconds(3.0f);
                dropOff.SetActive(false);
            }
            // Handling
            hudManager.DisplayText("Grasping is done automatically...");
            yield return new WaitForSeconds(5.0f);
            hudManager.DisplayText("by touching the object with your fingers.");
            yield return new WaitForSeconds(5.0f);
            hudManager.DisplayText("Try it now.");
            taskManager.SetObjectEnable(true);
            activeDropOff.SetActive(true);
            yield return new WaitUntil(() => taskManager.SuccessFlag);
            hudManager.DisplayText("Good job!");
            activeDropOff.SetActive(false);
            yield return new WaitForSeconds(5.0f);
        }


        // Practice
        hudManager.DisplayText("Let's do some practice.");
        yield return new WaitForSeconds(5.0f);
        hudManager.DisplayText("Do the task " + trainingIterations + " times.");
        yield return new WaitForSeconds(5.0f);
        while (trainingIterations > 0)
        {
            int dONum = Random.Range(0, dropOffLocations.Count);
            hudManager.DisplayText("Start!", 3.0f);
            taskManager.SetObjectEnable(true);
            dropOffLocations[dONum].SetActive(true);
            yield return new WaitUntil(() => taskManager.RunFlag);
            yield return new WaitUntil(() => taskManager.SuccessFlag);
            dropOffLocations[dONum].SetActive(false);
            trainingIterations--;
            hudManager.DisplayText("Good job!", 3.0f);
            yield return new WaitForSeconds(5.0f);
        }
        
        hudManager.DisplayText("Well done!");
        yield return new WaitForSeconds(5.0f);

        // Finish training
        hudManager.DisplayText("Please look at the monitor.");
        instructionManager.DisplayText("Hi again. :)");
        yield return new WaitForSeconds(4.0f);
        instructionManager.DisplayText("I think you are ready to get started!");
        yield return new WaitForSeconds(5.0f);
        instructionManager.DisplayText("The experiment will start after some final instructions.");
        yield return new WaitForSeconds(6.0f);

        trainingEnd = true;
    }

    private IEnumerator InstructionsLoop()
    {
        inInstructions = true;
        instructionsEnd = false;

        string defaultText = "Instructions:\n";

        instructionManager.DisplayText(defaultText + "The experiment requires you to repeat the grasp-and-drop-off task for " + iterationLimit + " iterations.");
        yield return new WaitForSeconds(8.0f);
        instructionManager.DisplayText(defaultText + "The drop-off location will be chosen randomly.");
        yield return new WaitForSeconds(5.0f);
        instructionManager.DisplayText(defaultText + "You will get a " + restTime + " second rest every " + restIterations + " iterations.");
        yield return new WaitForSeconds(8.0f);
        instructionManager.DisplayText(defaultText + "Your progress will be displayed here along with the status of the experiment.");
        yield return new WaitForSeconds(8.0f);
        instructionManager.DisplayText(defaultText + "If you need any rest please request it to the experimenter.");
        yield return new WaitForSeconds(8.0f);
        instructionManager.DisplayText(defaultText + "If you feel dizzy or want to stop the experiment please let the experimenter know as soon as possible.");
        yield return new WaitForSeconds(8.0f);
        instructionManager.DisplayText(defaultText + "Remember to not step during the experiment to perform the grasp-and-drop-off task.");
        yield return new WaitForSeconds(8.0f);
        instructionManager.DisplayText(defaultText + "Your HUD will instruct you when you can start by saying 'Start!'.");
        yield return new WaitForSeconds(8.0f);
        instructionManager.DisplayText("Get ready to start!");
        hudManager.DisplayText("Look forward.", 3.0f);
        yield return new WaitForSeconds(5.0f);
        HUDCountDown(3);
        yield return new WaitForSeconds(5.0f);

        instructionsEnd = true;
    }

    #endregion
}
