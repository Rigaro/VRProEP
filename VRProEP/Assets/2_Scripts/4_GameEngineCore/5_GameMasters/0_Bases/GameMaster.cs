// System
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Unity
using UnityEngine;
using TMPro;

// SteamVR
using Valve.VR;
using Valve.VR.InteractionSystem;

// GameMaster includes
using VRProEP.ExperimentCore;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.Utilities;

public abstract class GameMaster : MonoBehaviour
{
    //protected GameMaster instance;

    [Header("Flow Control:")]
    // Serialised
    [SerializeField]
    protected bool startEnable = false;
    [SerializeField]
    protected bool demoMode = false;
    [SerializeField]
    protected bool skipInstructions = false;
    [SerializeField]
    protected bool skipTraining = false;
    // Accessors
    public bool StartEnable { get => startEnable; set => startEnable = value; }
    public bool DemoMode { get => demoMode; }
    public bool SkipInstructions { get => skipInstructions; }
    public bool SkipTraining { get => skipTraining; }

    // UI Management
    private HUDManager hudManager;
    private ConsoleManager instructionManager;
    private ConsoleManager monitorManager;
    protected string infoText;
    // Accessors
    public HUDManager HudManager { get => hudManager; }
    public ConsoleManager InstructionManager { get => instructionManager; }
    public ConsoleManager MonitorManager { get => monitorManager; }

    [Header("Debug:")]
    [Tooltip("The debug enable variable.")]
    [SerializeField]
    protected bool debug;
    [SerializeField]
    protected TextMeshPro debugText;

    [Header("World Configuration:")]
    [SerializeField]
    protected float worldOrientation = 180.0f;
    [SerializeField]
    protected Transform experimentCenter;
    [SerializeField]
    protected float playerOrientation = 0.0f;

    [Header("Experiment Configuration:")]
    [SerializeField]
    private float restTime = 60.0f;
    [SerializeField]
    private int restIterations = 100;
    [SerializeField]
    protected List<int> iterationsPerSession = new List<int>();
    [SerializeField]
    protected List<int> trainingPerSession = new List<int>();
    // Accessors
    public float RestTime { get => restTime; }
    public int RestIterations { get => restIterations; }


    #region Data Logging
    // Task Data logging
    protected DataStreamLogger taskDataLogger;
    [Header("Experiment configuration: Data format")]
    [SerializeField]
    protected string taskDataFormat = "";
    protected string logData = "";
    #endregion

    // Subject group management
    public enum SubjectGroup
    {
        Control,
        TypeOne,
        TypeTwo,
        TypeThree,
        TypeFour
    }
    protected SubjectGroup subjectGroup;

    // Experiment type management
    public enum ExperimentType
    {
        TypeOne,
        TypeTwo,
        TypeThree,
        TypeFour
    }
    protected ExperimentType experimentType;

    #region Dynamic configuration

    // Experiment configuration
    protected TextAsset configAsset;

    #endregion

    #region Flow control variables

    //
    // Flow control
    //
    private State currentState;
    protected float taskTime = 0.0f;
    protected int sessionNumber = 1;
    protected int iterationNumber = 1;
    protected bool inWelcome = false;
    protected bool welcomeDone = false;
    protected bool inInstructions = false;
    protected bool instructionsDone = false;
    protected bool inTraining = false;
    protected bool trainingDone = false;
    protected bool countdownDone = false;
    private bool waitFlag = false;
    private Coroutine countdownCoroutine;
    protected SteamVR_Action_Boolean buttonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ObjectInteractButton");
    // Accessors
    public bool InWelcome { get => inWelcome; set => inWelcome = value; }
    public bool WelcomeDone { get => welcomeDone; }
    public bool InInstructions { get => inInstructions; set => inInstructions = value; }
    public bool InstructionsDone { get => instructionsDone; }
    public bool InTraining { get => inTraining; set => inTraining = value; }
    public bool TrainingDone { get => trainingDone; }
    public bool CountdownDone { get => countdownDone; }
    public bool WaitFlag { get => waitFlag; }

    //
    // THESE VARIABLES ARE NOW OBSOLETE, THEY WILL BE REMOVED IN A FUTURE VERSION
    //
    // Experiment state management
    protected ExperimentState experimentState = ExperimentState.Welcome;
    public enum ExperimentState
    {
        Welcome,
        Initialising,
        Instructions,
        Training,
        WaitingForStart,
        PerformingTask,
        AnalizingResults,
        UpdatingApplication,
        InitializingNext,
        Resting,
        Paused,
        End
    }

    // Waiting for start management variables
    protected WaitState waitState = WaitState.Waiting;
    protected enum WaitState
    {
        Waiting,
        Countdown
    }
    // Countdown management
    protected bool counting = false;

    private Coroutine waitCoroutine;
    //
    // THESE VARIABLES ARE NOW OBSOLETE, THEY WILL BE REMOVED IN A FUTURE VERSION
    //

    #endregion

    /// <summary>
    /// Methods in this region should not be overriden in the implemented class.
    /// If for some reason you need to override them, make sure you call base.Method() first.
    /// </summary>
    #region Unity MonoBehavior methods

    // Runs once when the experiment is launched
    protected virtual void Start()
    {
        // Configure experiment
        ConfigureExperiment();

        // Initialize UI.
        InitializeUI();

        // Initialize ExperimentSystem
        InitialiseExperimentSystems();

        // Initialise state machine
        currentState = new Welcome(this);
    }

    // Runs continuously at the current frame-rate.
    protected virtual void Update()
    {
        // Run everything in Update except Performing Task
        if (!(currentState is PerformingTask))
            currentState = currentState.Process();
    }

    // Run deterministically at the physics engine sample time.
    protected virtual void FixedUpdate()
    {
        // Only run Performing Task in Fixed Update
        if (currentState is PerformingTask)
            currentState = currentState.Process();
    }

    private void OnApplicationQuit()
    {
        // Make sure things are closed properly.
        EndExperiment();
    }

    #endregion

    #region Abstract methods to be implemented

    /// <summary>
    /// Gets the progress text to be displayed to the subject.
    /// Displays status, current time, experiment progress (session), and session progress (iteration) by default.
    /// </summary>
    /// <returns>The text to be displayed as a string.</returns>
    public virtual string GetDisplayInfoText()
    {
        string text;
        text = "Status: " + currentState.StateName.ToString() + ".\n";
        text += "Time: " + System.DateTime.Now.ToString("H:mm tt") + ".\n";
        text += "Experiment Progress: " + sessionNumber + "/" + iterationsPerSession.Count + ".\n";
        text += "Session Progress: " + iterationNumber + "/" + iterationsPerSession[sessionNumber - 1] + ".\n";
        return text;
    }

    /// <summary>
    /// Default implementation of HUD colour behaviour.
    /// Green: When state is "Paused", "Resting" or "End".
    /// Red: Any other state and !setActive.
    /// Blue: Any other state and setActive.
    /// Can be overriden to specify a different colour behaviour.
    /// </summary>
    /// <param name="setActive">Sets the HUD colour as active (Blue).</param>
    public virtual void HandleHUDColour(bool setActive = false)
    {
        if (currentState is Resting || currentState is End || currentState is Paused)
        {
            HudManager.colour = HUDManager.HUDColour.Purple;
        }
        else
        {
            if (setActive)
                HudManager.colour = HUDManager.HUDColour.Blue;
            else
                HudManager.colour = HUDManager.HUDColour.Red;
        }
    }

    /// <summary>
    /// Configures the experiment from a text file.
    /// The method needs to be extended to extract data from the configuration file that is automatically loaded.
    /// </summary>
    public virtual void ConfigureExperiment()
    {
        if (debug)
            configAsset = Resources.Load<TextAsset>("Experiments/" + this.gameObject.name);
        else
            configAsset = Resources.Load<TextAsset>("Experiments/" + ExperimentSystem.ActiveExperimentID);
    }

    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// This must be done in Start.
    /// Extend this method by doing your own implementation, with base.InitExperimentSystem() being called at the start.
    /// </summary>
    public virtual void InitialiseExperimentSystems()
    {
        //
        // Set the experiment name only when debugging. Take  the name from the gameobject + Debug
        //
        if (debug)
            ExperimentSystem.SetActiveExperimentID(this.gameObject.name + "_Debug");
        
        // Make sure flow control is initialised
        sessionNumber = 1;
        iterationNumber = 1;

        //
        // Create the default data loggers
        //
        taskDataLogger = new DataStreamLogger("TaskData");
        ExperimentSystem.AddExperimentLogger(taskDataLogger);
        taskDataLogger.AddNewLogFile(sessionNumber, iterationNumber, taskDataFormat); // Add file

        // Restart UDP threads
        foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
        {
            if (sensor is UDPSensorManager udpSensor )
            {
                //Debug.Log(wifiSensor.RunThread);
                udpSensor.StartSensorReading();
                //Debug.Log(wifiSensor.RunThread);
            }
        }

        // Send the player to the experiment centre position
        TeleportToStartPosition();
    }

    /// <summary>
    /// Coroutine for the welcome text.
    /// Implement your welcome loop here.
    /// </summary>
    /// <returns>Yield instruction</returns>
    public abstract IEnumerator WelcomeLoop();

    /// <summary>
    /// Performs initialisation procedures for the experiment. Sets variables to their zero state.
    /// </summary>
    public abstract void InitialiseExperiment();

    /// <summary>
    /// Coroutine for the experiment instructions.
    /// Implement your instructions loop here.
    /// </summary>
    /// <returns>Yield instruction</returns>
    public abstract IEnumerator InstructionsLoop();

    /// <summary>
    /// Coroutine for the experiment training.
    /// Implement your training loop here.
    /// </summary>
    /// <returns>Yield instruction</returns>
    public abstract IEnumerator TrainingLoop();

    /// <summary>
    /// Checks whether the subject is ready to start performing the task.
    /// </summary>
    /// <returns>True if ready to start.</returns>
    public abstract bool IsReadyToStart();

    /// <summary>
    /// Prepares variables and performs any procedures needed when the subject has succeded in the preparation
    /// to start performing the task.
    /// </summary>
    public abstract void PrepareForStart();

    /// <summary>
    /// Performs a procedure needed when the subject fails to start the task (goes out of the start condition).
    /// This can be: display some information, reset variables, change something in the experiment.
    /// </summary>
    public abstract void StartFailureReset();

    /// <summary>
    /// Handles task data logging which runs on FixedUpdate.
    /// Logs data from sensors registered in the AvatarSystem and ExperimentSystem by default.
    /// Can be exteded to add more data by implementing an override method in the derived class which first adds data
    /// to the logData string (e.g. logData += myDataString), and then calls base.HandleTaskDataLogging().
    /// </summary>
    public virtual void HandleTaskDataLogging()
    {
        //
        // Gather data while experiment is in progress
        //
        logData += taskTime.ToString();
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
        // Log current data and clear before next run.
        //
        taskDataLogger.AppendData(logData);
        logData = "";

        // Update run time
        taskTime += Time.fixedDeltaTime;
    }

    /// <summary>
    /// Handles procedures that occurs while the task is being executed (and not related to data logging).
    /// </summary>
    /// <returns></returns>
    public abstract void HandleInTaskBehaviour();

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    public abstract bool IsTaskDone();

    /// <summary>
    /// Handles procedures that occur as soon as the task is completed.
    /// Extend this method by doing your own implementation, with base.HandleTaskCompletion() being called at the start.
    /// </summary>
    public virtual void HandleTaskCompletion()
    {
        // Make sure the log is closed properly
        taskDataLogger.CloseLog();
    }

    /// <summary>
    /// Handles the procedures performed when analysing results.
    /// </summary>
    public abstract void HandleResultAnalysis();

    /// <summary>
    /// Handles procedures performed when initialising the next iteration.
    /// Updates iteration number, resets the task time, and starts a new data log by default.
    /// Extend this method by doing your own implementation, with base.HandleIterationInitialisation() being called at the start.
    /// </summary>
    public virtual void HandleIterationInitialisation()
    {
        //
        // Update iteration number and flow control
        //
        iterationNumber++;
        taskTime = 0.0f;

        // 
        // Update log
        //
        taskDataLogger.AddNewLogFile(sessionNumber, iterationNumber, taskDataFormat);
    }

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    public virtual bool IsEndOfSession()
    {
        // Check that the iteration counter has reached the max for that session
        return iterationNumber >= iterationsPerSession[sessionNumber - 1];
    }

    /// <summary>
    /// Handles procedures performed when initialising the next iteration.
    /// Updates iteration number, session number, resets the task time, and starts a new data log by default.
    /// Extend this method by doing your own implementation, with base.HandleSessionInitialisation() being called at the start.
    /// </summary>
    public virtual void HandleSessionInitialisation()
    {
        //
        // Initialize new session variables and flow control
        //
        iterationNumber = 1;
        sessionNumber++;
        taskTime = 0.0f;

        // 
        // Update log
        //
        taskDataLogger.AddNewLogFile(sessionNumber, iterationNumber, taskDataFormat);
    }

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    public virtual bool IsEndOfExperiment()
    {
        // Check that the last session has been reached and the last iteration of that session has passed.
        return (sessionNumber == iterationsPerSession.Count && iterationNumber >= iterationsPerSession[sessionNumber - 1]);
    }

    /// <summary>
    /// Performs all the required procedures to end the experiment.
    /// Closes all UPD Sensors and all logs by default.
    /// Extend this method by doing your own implementation, with base.EndExperiment() being called at the start.
    /// </summary>
    public virtual void EndExperiment()
    {
        //
        // Stop UDP threads
        //
        foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
        {
            if (sensor is UDPSensorManager udpSensor)
            {
                udpSensor.StopSensorReading();
            }
        }

        //
        // Save and close all logs
        //
        ExperimentSystem.CloseAllExperimentLoggers();
    }

    /// <summary>
    /// Checks if the condition for the rest period has been reached.
    /// </summary>
    /// <returns>True if the rest condition has been reached.</returns>
    public virtual bool IsRestTime()
    {
        // Give rest after a restIterations number of task iterations.
        return iterationNumber % restIterations == 0;
    }

    #endregion

    #region UI Set-up methods

    /// <summary>
    /// Initializes all UI components.
    /// </summary>
    protected void InitializeUI()
    {
        SetMonitor();
        SetHUD();
        SetConsole();
    }

    /// <summary>
    /// Initializes the experiment monitor UI.
    /// </summary>
    private void SetMonitor()
    {
        GameObject monitorGO = GameObject.FindGameObjectWithTag("Monitor");
        if (monitorGO == null)
            throw new System.Exception("Monitor GameObject not found.");

        monitorManager = monitorGO.GetComponent<ConsoleManager>();
        if (MonitorManager == null)
            throw new System.Exception("Monitor Manager not found.");
    }

    /// <summary>
    /// Initializes the user's HUD.
    /// </summary>
    private void SetHUD()
    {
        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        if (hudGO == null)
            MonitorManager.DisplayError(1, "HUD GameObject not found.");

        hudManager = hudGO.GetComponent<HUDManager>();
        if (hudManager == null)
            MonitorManager.DisplayError(2, "HUD Manager not found.");

        // Clear HUD
        HudManager.DisplayText("...", 1.0f);
    }

    /// <summary>
    /// Initializes the guide console.
    /// </summary>
    private void SetConsole()
    {
        GameObject consoleGO = GameObject.FindGameObjectWithTag("Console");
        if (consoleGO == null)
            MonitorManager.DisplayError(1, "Console GameObject not found.");

        instructionManager = consoleGO.GetComponent<ConsoleManager>();
        if (InstructionManager == null)
            MonitorManager.DisplayError(2, "Console Manager not found.");
    }

    #endregion

    #region Experiment control input management

    /// <summary>
    /// Handle pause state transfer between WaitingForStart and Paused. Transition to pause should only happen in waiting state.
    /// </summary>
    public bool UpdatePause()
    {
        // Check if pause input has been provided and toggle between paused and waiting for start.
        if (Input.GetKeyDown(KeyCode.P))
        {
            //if (experimentState == ExperimentState.Paused)
            //{
            //    HudManager.DisplayText("Resuming experiment!", 3.0f);
            //    InstructionManager.DisplayText("Resuming experiment!", 3.0f);
            //    //monitorManager.DisplayText("Resuming experiment!", 3.0f);
            //    experimentState = ExperimentState.WaitingForStart;
            //}
            //else if (experimentState == ExperimentState.WaitingForStart)
            //{
            //    HudManager.DisplayText("Pausing experiment...", 3.0f);
            //    InstructionManager.DisplayText("Pausing experiment!", 3.0f);
            //    //monitorManager.DisplayText("Pausing experiment!", 3.0f);
            //    experimentState = ExperimentState.Paused;
            //}
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Transfers to experiment End state when directly requested by experimenter by pressing 'E' key.
    /// </summary>
    public bool UpdateEnd()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //HudManager.DisplayText("Ending experiment...", 3.0f);
            //InstructionManager.DisplayText("Ending experiment...", 3.0f);
            //MonitorManager.DisplayText("Ending experiment...", 3.0f);
            experimentState = ExperimentState.End;
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Handles closing the application at the end of the experiment
    /// </summary>
    public void UpdateCloseApplication()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            // Destroy player and avatar objects
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
            if (playerGO == null || avatarGO == null)
                throw new System.Exception("The player or avatar has not been loaded.");

            Destroy(playerGO);
            Destroy(avatarGO);

            // Load main menu
            SteamVR_LoadLevel.Begin("MainMenu");
        }
    }

    /// <summary>
    /// Handles experimenter input to request the skipping to the next session.
    /// </summary>
    /// <returns></returns>
    public bool UpdateNext()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            //HudManager.DisplayText("Preparing next session...", 3.0f);
            //InstructionManager.DisplayText("Preparing next session...", 3.0f);
            //MonitorManager.DisplayText("Preparing next session...", 3.0f);
            experimentState = ExperimentState.InitializingNext;
            return true;
        }
        else
            return false;
    }

    #endregion

    #region Support methods

    /// <summary>
    /// Starts the welcome loop.
    /// </summary>
    public void StartWelcomeLoop()
    {
        StartCoroutine(WelcomeLoop());
    }

    /// <summary>
    /// Starts the instructions loop.
    /// </summary>
    public void StartInstructionsLoop()
    {
        StartCoroutine(InstructionsLoop());
    }

    /// <summary>
    /// Starts the training loop.
    /// </summary>
    public void StartTrainingLoop()
    {
        StartCoroutine(TrainingLoop());
    }


    /// <summary>
    /// Returns the name of the current state.
    /// </summary>
    /// <returns>The current state name.</returns>
    protected State.STATE GetCurrentStateName()
    {
        return currentState.StateName;
    }

    /// <summary>
    ///  Waits for the subject to press the "object interact button" which is mapped to the trigger.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitForSubjectAcknowledgement()
    {
        yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// Sets the waitFlag after X seconds .
    /// </summary>
    /// <param name="seconds">The time in seconds to wait to set flag.</param>
    public void SetWaitFlag(float seconds)
    {
        if (waitCoroutine != null)
            StopCoroutine(waitCoroutine);
        waitCoroutine = StartCoroutine(SetWaitFlagCoroutine(seconds));
    }

    /// <summary>
    /// Set waitFlag coroutine.
    /// </summary>
    /// <param name="seconds">The time in seconds to wait to set flag.</param>
    /// <returns></returns>
    private IEnumerator SetWaitFlagCoroutine(float seconds)
    {
        waitFlag = false;
        yield return new WaitForSecondsRealtime(seconds);
        waitFlag = true;
    }

    /// <summary>
    /// Displays a count-down on the HUD
    /// </summary>
    /// <param name="seconds"></param>
    public void HUDCountDown(int seconds)
    {
        countdownDone = false;
        countdownCoroutine = StartCoroutine(CountDownCoroutine(seconds));
    }

    /// <summary>
    /// Stops the current countdown.
    /// </summary>
    public void StopHUDCountDown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownDone = false;
    }

    /// <summary>
    /// Countdown coroutine
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private IEnumerator CountDownCoroutine(int seconds)
    {
        if (seconds < 0)
            throw new System.Exception("Countdown time must be greater or equal to 0 seconds.");
        HudManager.DisplayText("Ready?", 1.0f);
        yield return new WaitForSecondsRealtime(1.0f);
        for (int i = seconds; i >= 1; i--)
        {
            HudManager.DisplayText(i.ToString(), 1.0f);
            yield return new WaitForSecondsRealtime(1.0f);
        }
        HudManager.DisplayText("Go!", 1.0f);
        countdownDone = true;
    }

    /// <summary>
    /// Teleports the player to the experiment center position and orients experiment assets to fit the world.
    /// </summary>
    protected void TeleportToStartPosition()
    {
        // Get player object
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
            throw new System.NullReferenceException("Player GameObject not found.");

        Player player = playerGO.GetComponent<Player>();
        if (player == null)
            throw new System.NullReferenceException("Player Component not found.");
        
        // Rotate experiment assets.
        this.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), worldOrientation);

        // Get the player position on floor
        Vector3 playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
        // Set the new tracking origin as that place
        player.trackingOriginTransform.position = player.transform.position + playerFeetOffset;
        player.trackingOriginTransform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), playerOrientation);
        // Teleport to the start position
        if (playerOrientation == 180.0f)
            player.transform.position = experimentCenter.position - player.trackingOriginTransform.position;
        else
            player.transform.position = experimentCenter.position + player.trackingOriginTransform.position;
    }

    #endregion

}
