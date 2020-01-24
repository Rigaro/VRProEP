﻿// System
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
using VRProEP.AdaptationCore;
using VRProEP.Utilities;

public class KinematicEnergy2020 : GameMaster
{
    // Here you can place all your Unity (GameObjects or similar)
    #region Unity objects
    [Header("Experiment configuration: Data format")]
    [SerializeField]
    private string ablebodiedDataFormat = "loc,t,aDotUA,bDotUA,gDotUA,aUA,bUA,gUA,xUA,yUA,zUA,aDotE,bDotE,gDotE,aE,bE,gE,xE,yE,zE,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,xSH,ySH,zSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xUB,yUB,zUB,xHand,yHand,zHand,aHand,bHand,gHand";
    [SerializeField]
    private string prostheticDataFormat = "loc,t,aDotUA,bDotUA,gDotUA,aUA,bUA,gUA,xUA,yUA,zUA,qE,qDotE,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,xSH,ySH,zSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xUB,yUB,zUB,xHand,yHand,zHand,aHand,bHand,gHand,enable";
    [SerializeField]
    protected string performanceDataFormat = "i,J,uf,du,d2u,thetaBar,theta";

    [Header("Experiment configuration: Grid")]
    [SerializeField]
    [Tooltip("The number of rows for the reaching grid.")]
    [Range(1, 10)]
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
    [Range(0.0f, 1.5f)]
    private float gridHeightMultiplier = 0.6f;

    [SerializeField]
    [Tooltip("The percentage of the subject's arm length where the grid centre will be placed.")]
    [Range(0.0f, 1.5f)]
    private float gridReachMultiplier = 0.6f;

    [SerializeField]
    private BallGridManager gridManager;
    
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
    
    // Instructions management
    //private string infoText;

    // Motion tracking for experiment management and adaptation (check for start position)
    private VIVETrackerManager upperArmTracker;
    private VIVETrackerManager lowerArmTracker;
    private VIVETrackerManager shoulderTracker;
    private VIVETrackerManager c7Tracker;
    private VirtualPositionTracker handTracker;

    // Target management variables
    private int targetNumber; // The total number of targets
    private List<int> targetOrder = new List<int>(); // A list of target indexes ordered for selection over iterations in a session.

    // Flow control
    private bool hasReached = false;
    private bool taskComplete = false;

    // Prosthesis handling objects
    private GameObject prosthesisManagerGO;
    private ConfigurableElbowManager elbowManager;

    // Prosthesis adaptation stuff
    private List<Vector3> shPosBuffer;
    private List<Vector3> c7PosBuffer;
    private TransCyberHPIPersonalisation personaliser;
    private UpperBodyCompensationMotionPM evaluator;
    [Header("Experiment configuration: Personalisation")]
    [SerializeField]
    [Range(0.1f, 3.0f)]
    private float theta = 0.5f;
    [SerializeField]
    private float ditherAmplitude = 0.05f;
    [SerializeField]
    private float optimiserGain = 0.001f;

    // Additional data logging
    private DataStreamLogger performanceDataLogger;

    // Other helpful stuff
    private float leftySign = 1.0f;
    #endregion

    // Here are all the methods you need to write for your experiment.
    #region GameMaster Inherited Methods

    // Place debug stuff here, for when you want to test the experiment directly from the world without 
    // having to load it from the menus.
    private void Awake()
    {
        if (debug)
        {
            //
            // Debug able
            //
            SaveSystem.LoadUserData("DB1942174"); // Load the test/demo user (Mr Demo)
            //
            // Debug using able-bodied configuration
            //
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);

            //
            // Debug prosthetic
            //
            //AvatarSystem.LoadPlayer(UserType.Ablebodied, AvatarType.Transhumeral);
            //AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);
            //// Initialize prosthesis
            //GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            //ConfigurableElbowManager elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
            //elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + SaveSystem.ActiveUser.handLength / 2.0f));
            //// Set the reference generator to jacobian-based.
            ////elbowManager.ChangeReferenceGenerator("VAL_REFGEN_JACOBIANSYN");
            //// Set the reference generator to linear synergy.
            //elbowManager.ChangeReferenceGenerator("VAL_REFGEN_LINKINSYN");
        }

    }

    /// <summary>
    /// Gets the progress text to be displayed to the subject.
    /// Displays status, current time, experiment progress (session), and session progress (iteration) by default.
    /// </summary>
    /// <returns>The text to be displayed as a string.</returns>
    public override string GetDisplayInfoText()
    {
        // You can choose to use the base initialisation or get rid of it completely.
        string text = base.GetDisplayInfoText();

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
    public override void HandleHUDColour(bool setActive = false)
    {
        // Overwrite HUD behaviour except for default HUD behaviour
        if (GetCurrentStateName() == State.STATE.RESTING || GetCurrentStateName() == State.STATE.PAUSED || GetCurrentStateName() == State.STATE.END)
            base.HandleHUDColour();

        // Custom HUD handled in waiting for start and performing task relevant methods.
    }

    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// This must be done in Start.
    /// Extend this method by doing your own implementation, with base.InitExperimentSystem() being called at the start.
    /// </summary>
    public override void InitialiseExperimentSystems()
    {
        //
        // Set the experiment type configuration
        //
        // Type one if able-bodied subject
        if (AvatarSystem.AvatarType == AvatarType.AbleBodied)
        {
            experimentType = ExperimentType.TypeOne; // Able-bodied experiment type
            taskDataFormat = ablebodiedDataFormat;
        }
        // Type two if prosthetic (Adaptive Synergy)
        else if (AvatarSystem.AvatarType == AvatarType.Transhumeral)
        {
            experimentType = ExperimentType.TypeTwo; // Able-bodied experiment type
            taskDataFormat = prostheticDataFormat;
        }
        // Then run the base initialisation which is needed.
        base.InitialiseExperimentSystems();

        //
        // Create the performance data loggers
        //
        performanceDataLogger = new DataStreamLogger("TaskData");
        ExperimentSystem.AddExperimentLogger(performanceDataLogger);
        performanceDataLogger.AddNewLogFile(AvatarSystem.AvatarType.ToString(), sessionNumber, performanceDataFormat); // Add file

        if (SaveSystem.ActiveUser.lefty)
            leftySign = -1.0f;

        //
        // Iterations configuration
        //
        // Set iterations variables for flow control.
        targetNumber = gridRows * gridColumns;
        for (int i = 0; i < iterationsPerSession.Count; i++)
            iterationsPerSession[i] = targetNumber * iterationsPerTarget;

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
        gridManager.transform.position = new Vector3((-gridReachMultiplier * subjectArmLength) - 0.1f, gridHeightMultiplier * subjectHeight, 0.0f);

        //
        // Add arm motion trackers for able-bodied case.
        //
        if (experimentType == ExperimentType.TypeOne)
        { 
            // Lower limb motion tracker
            GameObject llMotionTrackerGO = GameObject.FindGameObjectWithTag("ForearmTracker");
            lowerArmTracker = new VIVETrackerManager(llMotionTrackerGO.transform);
            ExperimentSystem.AddSensor(lowerArmTracker);

            // Upper limb motion tracker
            GameObject ulMotionTrackerGO = AvatarSystem.AddMotionTracker();
            upperArmTracker = new VIVETrackerManager(ulMotionTrackerGO.transform);
            ExperimentSystem.AddSensor(upperArmTracker);
        }
        else if (experimentType == ExperimentType.TypeTwo)
        {
            // Get active sensors from avatar system and get the vive tracker being used for the UA
            foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
            {
                if (sensor is VIVETrackerManager)
                    upperArmTracker = (VIVETrackerManager)sensor;
            }
            if (upperArmTracker == null)
                throw new System.NullReferenceException("The residual limb tracker was not found.");

            // Set VIVE tracker and Linear synergy as active.
            // Get prosthesis
            prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            elbowManager = prosthesisManagerGO.GetComponent<ConfigurableElbowManager>();
            // Set the reference generator to linear synergy.
            elbowManager.ChangeSensor("VAL_SENSOR_VIVETRACKER");
            elbowManager.ChangeReferenceGenerator("VAL_REFGEN_LINKINSYN");

            // TEST
            elbowManager.SetSynergy(theta);
            // Create the personalisation algorithm object
            float[] ditherFrequency = { Mathf.PI / 4, 2*Mathf.PI / 4 };
            float[] observerGain = { 0.3840f, 0.6067f, -0.2273f, -0.8977f, -1.0302f };
            float[][] A = new float[2][];
            A[0] = new float[2] { 1.3130f, -0.8546f };
            A[1] = new float[2] { 1.0f, 0.0f };
            float[] B = { 1.0f, 0.0f };
            float[] C = { 0.0131f, -0.0131f };
            float D = 0.0f;
            personaliser = new TransCyberHPIPersonalisation(ditherAmplitude, 0, ditherFrequency, observerGain, 1, optimiserGain, 0.1f, A, B, C, D, theta, 0.1f, 3.0f);
        }

        // Performance evaluation objects
        evaluator = new UpperBodyCompensationMotionPM(0.5f, 0.5f);
        shPosBuffer = new List<Vector3>();
        c7PosBuffer = new List<Vector3>();

        // Debug?
        if (!debug)
        {
            // Shoulder acromium head tracker
            GameObject motionTrackerGO1 = AvatarSystem.AddMotionTracker();
            shoulderTracker = new VIVETrackerManager(motionTrackerGO1.transform);
            ExperimentSystem.AddSensor(shoulderTracker);
            // C7 tracker
            GameObject motionTrackerGO2 = AvatarSystem.AddMotionTracker();
            c7Tracker = new VIVETrackerManager(motionTrackerGO2.transform);
            ExperimentSystem.AddSensor(c7Tracker);
        }

        //
        // Hand tracking sensor
        //
        GameObject handGO = GameObject.FindGameObjectWithTag("Hand");
        handTracker = new VirtualPositionTracker(handGO.transform);
        ExperimentSystem.AddSensor(handTracker);

        // Spawn grid
        gridManager.SpawnGrid(gridRows, gridColumns, gridSpacing);

    }

    /// <summary>
    /// Coroutine for the welcome text.
    /// Implement your welcome loop here.
    /// </summary>
    /// <returns>Yield instruction</returns>
    public override IEnumerator WelcomeLoop()
    {
        // First flag that we are in the welcome routine
        welcomeDone = false;
        inWelcome = true;

        //
        HudManager.DisplayText("Look to the top right.");
        InstructionManager.DisplayText("Hi " + SaveSystem.ActiveUser.name + "! Welcome to the virtual world. \n\n Press the trigger button to continue...");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

        // Now that you are done, set the flag to indicate we are done.
        welcomeDone = true;
    }

    /// <summary>
    /// Performs initialisation procedures for the experiment. Sets variables to their zero state.
    /// </summary>
    public override void InitialiseExperiment()
    {
    }

    /// <summary>
    /// Coroutine for the experiment instructions.
    /// Implement your instructions loop here.
    /// </summary>
    /// <returns>Yield instruction</returns>
    public override IEnumerator InstructionsLoop()
    {
        // First flag that we are in the instructions routine
        instructionsDone = false;
        inInstructions = true;

        //
        InstructionManager.DisplayText("Here I'll add some instructions! \n\n Press the trigger button to continue...");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

        // Now that you are done, set the flag to indicate we are done.
        instructionsDone = true;
    }


    /// <summary>
    /// Coroutine for the experiment training.
    /// Implement your training loop here.
    /// </summary>
    /// <returns>Yield instruction</returns>
    public override IEnumerator TrainingLoop()
    {
        // First flag that we are in the training routine
        trainingDone = false;
        inTraining = true;

        // Only run the training loop when requested, for instance some session may require different session.
        if (trainingPerSession[sessionNumber - 1] == 1)
        {
            //
            InstructionManager.DisplayText("Alright, let's start training! \n\n Press the trigger button to continue...");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        }

        // Now that you are done, set the flag to indicate we are done.
        trainingDone = true;
    }

    /// <summary>
    /// Checks whether the subject is ready to start performing the task.
    /// </summary>
    /// <returns>True if ready to start.</returns>
    public override bool IsReadyToStart()
    {
        // Check that upper and lower arms are within the tolerated start position.
        float qShoulder = leftySign * Mathf.Rad2Deg * (upperArmTracker.GetProcessedData(5) + Mathf.PI); // Offsetting to horizontal position being 0.
        float qElbow = 0;

        if (experimentType == ExperimentType.TypeOne)
            qElbow = Mathf.Rad2Deg * (lowerArmTracker.GetProcessedData(5)) - qShoulder; // Offsetting to horizontal position being 0.
        else if (experimentType == ExperimentType.TypeTwo)
            qElbow = -Mathf.Rad2Deg * elbowManager.GetElbowAngle();
        
        // The difference to the start position
        float qSDiff = qShoulder - startShoulderAngle;
        float qEDiff = qElbow - startElbowAngle;
        
        //
        // Update information displayed for debugging purposes
        //
        if (debug)
        {
            debugText.text = experimentState.ToString() + "\n";
            if (experimentState == ExperimentState.WaitingForStart)
                debugText.text += waitState.ToString() + "\n";
            debugText.text += qShoulder.ToString() + "\n";
            debugText.text += qElbow.ToString() + "\n";
        }

        if (Mathf.Abs(qSDiff) < startTolerance && Mathf.Abs(qEDiff) < startTolerance)
        {
            HudManager.colour = HUDManager.HUDColour.Orange;
            return true;
        }
        // Provide instructions when not there yet
        else
        {
            string helpText = "";
            if (qSDiff < 0 && Mathf.Abs(qSDiff) > startTolerance)
                helpText += "UA: ++.\n";
            else if (qSDiff > 0 && Mathf.Abs(qSDiff) > startTolerance)
                helpText += "UA: --.\n";

            if (qEDiff < 0 && Mathf.Abs(qEDiff) > startTolerance)
                helpText += "LA: ++.\n";
            else if (qEDiff > 0 && Mathf.Abs(qEDiff) > startTolerance)
                helpText += "LA: --.\n";

            HudManager.DisplayText(helpText);
            HudManager.colour = HUDManager.HUDColour.Red;
            return false;
        }
    }

    /// <summary>
    /// Prepares variables and performs any procedures needed when the subject has succeded in the preparation
    /// to start performing the task.
    /// </summary>
    public override void PrepareForStart()
    {
        // Clear buffers
        shPosBuffer.Clear();
        c7PosBuffer.Clear();
        // Select target
        gridManager.SelectBall(targetOrder[iterationNumber - 1]);
        // Return prosthetic elbow to -90deg
        if (experimentType == ExperimentType.TypeTwo)
            elbowManager.SetElbowAngle(Mathf.Deg2Rad * -90.0f);
    }

    /// <summary>
    /// Performs a procedure needed when the subject fails to start the task (goes out of the start condition).
    /// This can be: display some information, reset variables, change something in the experiment.
    /// </summary>
    public override void StartFailureReset()
    {
        // Clear ball
        gridManager.ResetBallSelection();
    }

    /// <summary>
    /// Handles task data logging which runs on FixedUpdate.
    /// Logs data from sensors registered in the AvatarSystem and ExperimentSystem by default.
    /// Can be exteded to add more data by implementing an override method in the derived class which first adds data
    /// to the logData string (e.g. logData +=  myDataString + ","), and then calls base.HandleTaskDataLogging().
    /// </summary>
    public override void HandleTaskDataLogging()
    {
        // Add your custom data logging here
        // e.g. the magic number!
        logData += targetOrder[iterationNumber - 1] + ",";  // Make sure you always end your custom data with a comma! Using CSV for data logging.

        // Continue with data logging.
        base.HandleTaskDataLogging();

        // Performance evaluation data buffering
        if (!debug)
        {
            Vector3 shPos = new Vector3(shoulderTracker.GetProcessedData("X_Pos"), shoulderTracker.GetProcessedData("Y_Pos"), shoulderTracker.GetProcessedData("Z_Pos"));
            Vector3 c7Pos = new Vector3(c7Tracker.GetProcessedData("X_Pos"), c7Tracker.GetProcessedData("Y_Pos"), c7Tracker.GetProcessedData("Z_Pos"));
            shPosBuffer.Add(shPos);
            c7PosBuffer.Add(c7Pos);
        }
    }

    /// <summary>
    /// Handles procedures that occurs while the task is being executed (and not related to data logging).
    /// </summary>
    /// <returns></returns>
    public override void HandleInTaskBehaviour()
    {
        HudManager.colour = HUDManager.HUDColour.Blue;
    }

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    public override bool IsTaskDone()
    {
        //
        // Check that the selected ball has been touched.
        //
        if (gridManager.SelectedTouched && !hasReached)
            StartCoroutine(EndTaskCoroutine());

        return taskComplete;
    }

    /// <summary>
    /// Raises the complete task flag after 1 second to allow for additional data to be gathered
    /// after the subject touches the selected sphere.
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndTaskCoroutine()
    {
        hasReached = true;
        yield return new WaitForSecondsRealtime(1.0f);
        taskComplete = true;
    }


    public override void HandleTaskCompletion()
    {
        base.HandleTaskCompletion();

        // Signal the subject that the task is done
        HudManager.colour = HUDManager.HUDColour.Green; 
        // Reset flags
        hasReached = false;
        taskComplete = false;
    }

    /// <summary>
    /// Handles the procedures performed when analysing results.
    /// </summary>
    public override void HandleResultAnalysis()
    {
        if(!debug)
        {
            // Performance evaluation
            evaluator.AddData<Vector3>(shPosBuffer, evaluator.SHOULDER);
            evaluator.AddData<Vector3>(c7PosBuffer, evaluator.TRUNK);
            float J = evaluator.Update();

            //Debug.Log("J = " + J);
            string iterationResults = iterationNumber + "," +
                                     J;

            // If it's able-bodied, no need to do anything
            // If it's KE-Adaptive-Synergy then perform synergy update
            if (experimentType == ExperimentType.TypeTwo)
            {
                // Perform update
                theta = personaliser.UpdateParameter(J, iterationNumber);
                elbowManager.SetSynergy(theta);
                // Add algorithm states data to log
                foreach (float value in personaliser.GetStates())
                    iterationResults += "," + value;
            }
            iterationResults += "," + theta;
            //Debug.Log("Theta: " + theta);

            // Log results
            performanceDataLogger.AppendData(iterationResults);
            performanceDataLogger.SaveLog();
        }
    }

    public override void HandleIterationInitialisation()
    {
        base.HandleIterationInitialisation();

        // Clear ball
        gridManager.ResetBallSelection();
    }

    public override void HandleSessionInitialisation()
    {
        base.HandleSessionInitialisation();

        // New file for the performance data logger
        performanceDataLogger.CloseLog();
        performanceDataLogger.AddNewLogFile(AvatarSystem.AvatarType.ToString(), sessionNumber, performanceDataFormat); // Add file

        // Clear ball
        gridManager.ResetBallSelection();
    }

    #endregion

}
