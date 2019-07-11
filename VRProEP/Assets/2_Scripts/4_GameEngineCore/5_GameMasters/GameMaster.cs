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
    [Header("Control Variables:")]
    public bool enableStart = false;
    public bool demoMode = false;
    public float restTime = 60.0f;
    public bool skipInstructions = false;

    // UI
    protected HUDManager hudManager;
    protected ConsoleManager instructionManager;
    protected ConsoleManager monitorManager;

    [Header("Debug enable:")]
    public bool debug;
    public TextMeshPro debugText;

    [Header("World configuration:")]
    [SerializeField]
    protected float worldOrientation = 180.0f;
    public Transform experimentCenter;

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

    // Flow control
    protected bool startEnable = false;
    protected int sessionNumber = 1;
    protected int iterationNumber = 1;

    // Experiment state management
    protected ExperimentState experimentState = ExperimentState.HelloWorld;
    public enum ExperimentState
    {
        HelloWorld,
        InitializingApplication,
        GivingInstructions,
        Training,
        WaitingForStart,
        PerformingTask,
        AnalizingResults,
        UpdatingApplication,
        InitializingNextSession,
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
    protected bool counting = false;
    protected bool countdownDone = false;
    private bool waitFlag = false;
    protected bool WaitFlag { get => waitFlag; set => waitFlag = value; }

    #endregion

    #region Abstract methods to be implemented

    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// </summary>
    protected abstract void InitExperimentSystem();

    /// <summary>
    /// Checks whether the subject is ready to start performing the task.
    /// </summary>
    /// <returns>True if ready to start.</returns>
    protected abstract bool CheckReadyToStart();

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    protected abstract bool CheckTaskCompletion();

    /// <summary>
    /// Checks if the condition for the rest period has been reached.
    /// </summary>
    /// <returns>True if the rest condition has been reached.</returns>
    protected abstract bool CheckRestCondition();

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    protected abstract bool CheckNextSessionCondition();

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    protected abstract bool CheckEndCondition();

    /// <summary>
    /// Launches the next session. Performs all the required preparations.
    /// </summary>
    protected abstract void LaunchNextSession();

    /// <summary>
    /// Finishes the experiment. Performs all the required procedures.
    /// </summary>
    protected abstract void EndExperiment();

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
        if (monitorManager == null)
            throw new System.Exception("Monitor Manager not found.");
    }

    /// <summary>
    /// Initializes the user's HUD.
    /// </summary>
    private void SetHUD()
    {
        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        if (hudGO == null)
            monitorManager.DisplayError(1, "HUD GameObject not found.");

        hudManager = hudGO.GetComponent<HUDManager>();
        if (hudManager == null)
            monitorManager.DisplayError(2, "HUD Manager not found.");

        // Clear HUD
        hudManager.DisplayText("...", 1.0f);
    }

    /// <summary>
    /// Initializes the guide console.
    /// </summary>
    private void SetConsole()
    {
        GameObject consoleGO = GameObject.FindGameObjectWithTag("Console");
        if (consoleGO == null)
            monitorManager.DisplayError(1, "Console GameObject not found.");

        instructionManager = consoleGO.GetComponent<ConsoleManager>();
        if (instructionManager == null)
            monitorManager.DisplayError(2, "Console Manager not found.");
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
                hudManager.DisplayText("Resuming experiment!", 3.0f);
                instructionManager.DisplayText("Resuming experiment!", 3.0f);
                //monitorManager.DisplayText("Resuming experiment!", 3.0f);
                experimentState = ExperimentState.WaitingForStart;
            }
            else if (experimentState == ExperimentState.WaitingForStart)
            {
                hudManager.DisplayText("Pausing experiment...", 3.0f);
                instructionManager.DisplayText("Pausing experiment!", 3.0f);
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
            hudManager.DisplayText("Ending experiment...", 3.0f);
            instructionManager.DisplayText("Ending experiment...", 3.0f);
            monitorManager.DisplayText("Ending experiment...", 3.0f);
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
            hudManager.DisplayText("Preparing next session...", 3.0f);
            instructionManager.DisplayText("Preparing next session...", 3.0f);
            monitorManager.DisplayText("Preparing next session...", 3.0f);
            experimentState = ExperimentState.InitializingNextSession;
            return true;
        }
        else
            return false;
    }

    #endregion

    #region Support methods

    /// <summary>
    /// Sets the waitFlag after X seconds .
    /// </summary>
    /// <param name="seconds">The time in seconds to wait to set flag.</param>
    protected void SetWaitFlag(float seconds)
    {
        StopCoroutine("SetWaitFlagCoroutine");
        StartCoroutine(SetWaitFlagCoroutine(seconds));
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
    protected void HUDCountDown(int seconds)
    {
        StartCoroutine(CountDownCoroutine(seconds));
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
        hudManager.DisplayText("Ready?", 1.0f);
        yield return new WaitForSecondsRealtime(1.0f);
        for (int i = seconds; i >= 1; i--)
        {
            hudManager.DisplayText(i.ToString(), 1.0f);
            yield return new WaitForSecondsRealtime(1.0f);
        }
        hudManager.DisplayText("Go!", 1.0f);
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

        // Teleport to the start position
        Vector3 playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
        player.trackingOriginTransform.position = player.transform.position + playerFeetOffset;
        player.trackingOriginTransform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), worldOrientation);
        player.transform.position = experimentCenter.position - player.transform.position;

        // Rotate experiment assets.
        this.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), worldOrientation);
    }

    #endregion

}
