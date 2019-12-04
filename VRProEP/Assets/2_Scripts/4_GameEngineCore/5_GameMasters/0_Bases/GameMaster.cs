// System
using System.Collections;
using System.Collections.Generic;

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
    [Header("Flow Control:")]
    // Serialised
    [SerializeField]
    protected bool enableStart = false;
    [SerializeField]
    protected bool demoMode = false;
    [SerializeField]
    protected bool skipInstructions = false;
    [SerializeField]
    protected bool skipTraining = false;
    protected State currentState;
    // Accessors
    public bool EnableStart { get => enableStart; set => enableStart = value; }
    public bool DemoMode { get => demoMode; }
    public bool SkipInstructions { get => skipInstructions; }

    // UI Management
    private HUDManager hudManager;
    private ConsoleManager instructionManager;
    private ConsoleManager monitorManager;
    protected string infoText;
    // Accessors
    public HUDManager HudManager { get => hudManager; }
    protected ConsoleManager InstructionManager { get => instructionManager; }
    protected ConsoleManager MonitorManager { get => monitorManager; }

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

    #region Flow control variables

    //
    // Flow control
    //
    protected float taskTime = 0.0f;
    protected bool startEnable = false;
    protected int sessionNumber = 1;
    protected int iterationNumber = 1;
    protected bool inWelcome = false;
    protected bool welcomeDone = false;
    protected bool inInstructions = false;
    protected bool instructionsDone = false;
    protected bool inTraining = false;
    protected bool trainingDone = false;
    protected SteamVR_Action_Boolean buttonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ObjectInteractButton");
    // Accessors
    public bool InWelcome { get => inWelcome; set => inWelcome = value; }
    public bool WelcomeDone { get => welcomeDone; set => welcomeDone = value; }
    public bool InInstructions { get => inInstructions; set => inInstructions = value; }
    public bool InstructionsDone { get => instructionsDone; }
    public bool InTraining { get => inTraining; }
    public bool TrainingDone { get => trainingDone; }

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
    protected bool countdownDone = false;
    private Coroutine countdownCoroutine;

    // Waiting
    private bool waitFlag = false;
    protected bool WaitFlag { get => waitFlag; set => waitFlag = value; }

    private Coroutine waitCoroutine;

    #endregion

    #region Abstract methods to be implemented

    /// <summary>
    /// Gets the progress text to be displayed to the subject.
    /// </summary>
    /// <returns>The text to be displayed as a string.</returns>
    public abstract string GetDisplayInfoText();

    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// This must be done in Start.
    /// </summary>
    public abstract void InitExperimentSystem();

    /// <summary>
    /// Performs initialisation procedures for the experiment. Sets variables to their zero state.
    /// </summary>
    public abstract void InitialiseExperiment();

    /// <summary>
    /// Checks whether the subject is ready to start performing the task.
    /// </summary>
    /// <returns>True if ready to start.</returns>
    public abstract bool CheckReadyToStart();

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    public abstract bool CheckTaskCompletion();

    /// <summary>
    /// Checks if the condition for the rest period has been reached.
    /// </summary>
    /// <returns>True if the rest condition has been reached.</returns>
    public abstract bool CheckRestCondition();

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    public abstract bool CheckNextSessionCondition();

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    public abstract bool CheckEndCondition();

    /// <summary>
    /// Configures the next session. Performs all the configurations required when starting a new experiment session.
    /// </summary>
    public abstract void ConfigureNextSession();

    /// <summary>
    /// Finishes the experiment. Performs all the required procedures.
    /// </summary>
    public abstract void EndExperiment();

    /// <summary>
    /// Coroutine for the welcome text.
    /// Implement your welcome loop here.
    /// </summary>
    /// <returns>Yield instruction</returns>
    public abstract IEnumerator WelcomeLoop();

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
    protected void UpdatePause()
    {
        // Check if pause input has been provided and toggle between paused and waiting for start.
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (experimentState == ExperimentState.Paused)
            {
                HudManager.DisplayText("Resuming experiment!", 3.0f);
                InstructionManager.DisplayText("Resuming experiment!", 3.0f);
                //monitorManager.DisplayText("Resuming experiment!", 3.0f);
                experimentState = ExperimentState.WaitingForStart;
            }
            else if (experimentState == ExperimentState.WaitingForStart)
            {
                HudManager.DisplayText("Pausing experiment...", 3.0f);
                InstructionManager.DisplayText("Pausing experiment!", 3.0f);
                //monitorManager.DisplayText("Pausing experiment!", 3.0f);
                experimentState = ExperimentState.Paused;
            }
        }
    }

    /// <summary>
    /// Transfers to experiment End state when directly requested by experimenter by pression 'E' key.
    /// </summary>
    protected bool UpdateEnd()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            HudManager.DisplayText("Ending experiment...", 3.0f);
            InstructionManager.DisplayText("Ending experiment...", 3.0f);
            MonitorManager.DisplayText("Ending experiment...", 3.0f);
            experimentState = ExperimentState.End;
            return true;
        }
        else
            return false;
    }

    protected void UpdateCloseApplication()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    protected bool UpdateNext()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            HudManager.DisplayText("Preparing next session...", 3.0f);
            InstructionManager.DisplayText("Preparing next session...", 3.0f);
            MonitorManager.DisplayText("Preparing next session...", 3.0f);
            experimentState = ExperimentState.InitializingNext;
            return true;
        }
        else
            return false;
    }

    #endregion

    #region Support methods

    /// <summary>
    /// Resets the iteration counter back to one.
    /// </summary>
    public void ResetIterationCounter()
    {
        iterationNumber = 1;
    }

    /// <summary>
    /// Increases the iteration counter by one.
    /// </summary>
    public void IncreaseIterationCounter()
    {
        iterationNumber++;
    }

    /// <summary>
    /// Resets the session counter back to one.
    /// </summary>
    public void ResetSessionNumber()
    {
        sessionNumber = 1;
    }

    /// <summary>
    /// Increases the session counter by one.
    /// </summary>
    public void IncreaseSessionNumber()
    {
        sessionNumber++;
    }

    /// <summary>
    /// Resets the task time back to zero.
    /// </summary>
    public void ResetTaskTime()
    {
        taskTime = 0;
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
        WaitFlag = false;
        yield return new WaitForSecondsRealtime(seconds);
        WaitFlag = true;
    }

    /// <summary>
    /// Displays a count-down on the HUD
    /// </summary>
    /// <param name="seconds"></param>
    public void HUDCountDown(int seconds)
    {
        countdownCoroutine = StartCoroutine(CountDownCoroutine(seconds));
    }

    /// <summary>
    /// Stops the current countdown.
    /// </summary>
    public void StopHUDCountDown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
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
    public void TeleportToStartPosition()
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
