﻿// System
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

public class SeparabilityExperiment2020GM : GameMaster
{
    // Here you can place all your Unity (GameObjects or similar)
    #region Unity objects
    [SerializeField]
    private string ablebodiedDataFormat = "loc,t,aDotE,bDotE,gDotE,aE,bE,gE,xE,yE,zE,aDotUA,bDotUA,gDotUA,aUA,bUA,gUA,xUA,yUA,zUA,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,xSH,ySH,zSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xUB,yUB,zUB,xHand,yHand,zHand,aHand,bHand,gHand";
    [SerializeField]
    private TargetGridManager gridManager;

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
    private int iterationsPerTarget = 10;


    [Header("Grid Configurations")]
    [SerializeField]
    [Tooltip("Grid-Close: The percentage of the subject's armlentgh where the grid centre will be placed.")]
    [Range(0, 2)]
    private float gridCloseDistanceFactor = 0.75f;
    [SerializeField]
    [Tooltip("Grid-Mid: The percentage of the subject's armlentgh where the grid centre will be placed.")]
    [Range(0, 2)]
    private float gridMidDistanceFactor = 1.0f;
    [SerializeField]
    [Tooltip("Grid-Far: The percentage of the subject's armlentgh where the grid centre will be placed.")]
    [Range(0, 2)]
    private float gridFarDistanceFactor = 1.5f;
    [SerializeField]
    [Tooltip("The percentage of the subject's height where the grid centre will be placed.")]
    [Range(0, 2)]
    private float gridHeightFactor = 0.5f;

    [SerializeField]
    private bool checkStartPosition;

    [SerializeField]
    private GameObject startPosPhoto;
    #endregion

    // Delsys EMG background data collection
    private DelsysEMG delsysEMG = new DelsysEMG();

    // Target management variables
    private int targetNumber = 9; // The total number of targets
    private List<int> targetOrder = new List<int>(); // A list of target indexes ordered for selection over iterations in a session.

    // Motion tracking for experiment management and adaptation (check for start position)
    private VIVETrackerManager upperArmTracker;
    private VIVETrackerManager lowerArmTracker;
    private VIVETrackerManager shoulderTracker;
    private VIVETrackerManager c7Tracker;
    private VirtualPositionTracker handTracker;

    // Flow control
    private bool hasReached = false;
    private bool taskComplete = false;
    private bool emgIsRecording = false;

    // Lefty subject sign
    private float leftySign = 1.0f;

    //
    private float[] elbowDesiredPos = { 30, 55, 80 };
    //private float[] elbowDesiredPos = { 25, 50, 75 };

    private float[] shoulderDesiredPos = { 40, 60, 80};

    private class SeparabilityGridReachingConfigurator
    {
        public int iterationsPerTarget = 10;
        public int sessionNumber = 2;
        public float gridCloseDistanceFactor = 0.75f;
        public float gridMidDistanceFactor = 1.0f;
        public float gridFarDistanceFactor = 1.5f;
        public float gridHeightFactor = 0.5f;
    }
    private SeparabilityGridReachingConfigurator configurator;


    #region Private methods
    private string ConfigEMGFilePath()
    {
        //string emgDataFilename =  Path.Combine(taskDataLogger.ActiveDataPath, "session_" + sessionNumber , "i" + "_" + iterationNumber + "EMG.csv");
        string emgDataFilename = taskDataLogger.ActiveDataPath + "/session_" + sessionNumber + "/i" + "_" + iterationNumber + "EMG.csv";
        return emgDataFilename;
    }


    private bool IsAtRightPosition(float qShoudlerRef, float qElbowRef, float tolerance)
    {
  

        // Check that upper and lower arms are within the tolerated start position.
        float qShoulder = leftySign * Mathf.Rad2Deg * (upperArmTracker.GetProcessedData(5) + Mathf.PI); // Offsetting to horizontal position being 0.
        float qElbow = 0;

        HudManager.colour = HUDManager.HUDColour.Orange;
        qElbow = Mathf.Rad2Deg * (lowerArmTracker.GetProcessedData(5)) - qShoulder; // Offsetting to horizontal position being 0.
                                                                                    // The difference to the start position
        float qSDiff = qShoulder - qShoudlerRef;
        float qEDiff = qElbow - qElbowRef;


        //
        // Update information displayed for debugging purposes
        //


        //InstructionManager.DisplayText(qShoulder.ToString() + "\n" + qElbow.ToString() + "\n");

        if (Mathf.Abs(qSDiff) < tolerance && Mathf.Abs(qEDiff) < tolerance)
        {
            HudManager.colour = HUDManager.HUDColour.Orange;
            return true;
        }
        // Provide instructions when not there yet
        else
        {

            string helpText = "";
            if (qSDiff < 0 && Mathf.Abs(qSDiff) > tolerance)
                helpText += "UA: ++ (" + qShoulder.ToString("F0") + "/" + qShoudlerRef.ToString("F0") + ").\n";
            else if (qSDiff > 0 && Mathf.Abs(qSDiff) > tolerance)
                helpText += "UA: -- (" + qShoulder.ToString("F0") + "/" + qShoudlerRef.ToString("F0") + ").\n";
            else
                helpText += "UA: == (" + qShoulder.ToString("F0") + "/" + qShoudlerRef.ToString("F0") + ").\n";


            if (qEDiff < 0 && Mathf.Abs(qEDiff) > tolerance)
                helpText += "LA: ++ (" + qElbow.ToString("F0") + "/" + qElbowRef.ToString("F0") + ").\n";
            else if (qEDiff > 0 && Mathf.Abs(qEDiff) > tolerance)
                helpText += "LA: -- (" + qElbow.ToString("F0") + "/" + qElbowRef.ToString("F0") + ").\n"; 
            else
                helpText += "LA: == (" + qElbow.ToString("F0") + "/" + qElbowRef.ToString("F0") + ").\n"; 
            HudManager.DisplayText(helpText);
            HudManager.colour = HUDManager.HUDColour.Red;


            return false;
        }

        
    }

    #endregion


    #region Dynamic configuration

    //
    // Configuration class:
    // Modify this class to be able to configure your experiment from a configuration file
    //
    private class Configurator
    {
        
    }
    private Configurator config;

    #endregion

    // Here are all the methods you need to write for your experiment.
    #region GameMaster Inherited Methods

    protected override void FixedUpdate()
    {
       
        // Override fixed update to start the emg recording when the start performing the task
        if ( GetCurrentStateName() == State.STATE.PERFORMING_TASK && !emgIsRecording)
        {
            
            delsysEMG.StartRecording(ConfigEMGFilePath());
            emgIsRecording = true;
        }

        base.FixedUpdate();

    }
    // Place debug stuff here, for when you want to test the experiment directly from the world without 
    // having to load it from the menus.
    private void Awake()
    {
        if (debug)
        {
            
            SaveSystem.LoadUserData("TB1995175"); // Load the test/demo user (Mr Demo)
            //
            // Debug using able-bodied configuration
            //
            Debug.Log("Load Avatar to Debug.");
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);
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
            base.HandleHUDColour();
    }

    /// <summary>
    /// Configures the experiment from a text file.
    /// The method needs to be extended to extract data from the configuration file that is automatically loaded.
    /// If no configuration is needed, then leave empty.
    /// </summary>
    public override void ConfigureExperiment()
    {
        // First call the base method to load the file
        base.ConfigureExperiment();

        //configAsset = Resources.Load<TextAsset>("Experiments/" + ExperimentSystem.ActiveExperimentID);

        // Convert configuration file to configuration class.
        configurator = JsonUtility.FromJson<SeparabilityGridReachingConfigurator>(configAsset.text);

        // Load from config file
        iterationsPerTarget = configurator.iterationsPerTarget;
        for (int i = 0; i < configurator.sessionNumber-1; i++)
        {
            iterationsPerSession.Add(0);
        }
        
        gridCloseDistanceFactor = configurator.gridCloseDistanceFactor;
        gridMidDistanceFactor = configurator.gridMidDistanceFactor;
        gridFarDistanceFactor = configurator.gridFarDistanceFactor;
        gridHeightFactor = configurator.gridHeightFactor;

    }

    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// This must be done in Start.
    /// Extend this method by doing your own implementation, with base.InitExperimentSystem() being called at the start.
    /// </summary>
    public override void InitialiseExperimentSystems()
    {

        // Set data format
        taskDataFormat = ablebodiedDataFormat;

        // Lefty sign
        /*
        if (SaveSystem.ActiveUser.lefty)
            leftySign = -1.0f;
            */

        #region Modify base
        // Then run the base initialisation which is needed, with a small modification
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
        taskDataLogger = new DataStreamLogger("TaskData/" + AvatarSystem.AvatarType.ToString());
        ExperimentSystem.AddExperimentLogger(taskDataLogger);
        taskDataLogger.AddNewLogFile(sessionNumber, iterationNumber, taskDataFormat); // Add file

        // Send the player to the experiment centre position
        TeleportToStartPosition();
        #endregion

        #region Initialize EMG sensors
        //Initialse Delsys EMG sensor
        delsysEMG.Init();
        delsysEMG.Connect();
        delsysEMG.StartAcquisition();

        #endregion

        #region Initialize world positioning

        // Set the subject physiological data for grid 
        gridManager.ConfigUserData();
       
        gridManager.ConfigGridPositionFactors(gridCloseDistanceFactor, gridMidDistanceFactor, gridFarDistanceFactor, gridHeightFactor);

        #endregion

        #region  Initialize motion sensors
        //
        // Add arm motion trackers for able-bodied case.
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

        #endregion

        

        
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

        HudManager.DisplayText("Look to the top right.");
        InstructionManager.DisplayText("Hi " + SaveSystem.ActiveUser.name + "! Welcome to the virtual world. \n\n (Press the trigger button to continue...)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

        InstructionManager.DisplayText("Make sure you are standing on top of the green circle. \n\n (Press the trigger button to continue...)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.


        //
        // Hud intro
        InstructionManager.DisplayText("Alright " + SaveSystem.ActiveUser.name + ", let's show you the ropes." + "\n\n (Press the trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        InstructionManager.DisplayText("Let me introduce you to your assistant, the Heads Up Display (HUD)." + "\n\n (Press the trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        InstructionManager.DisplayText("Say hi!");
        HudManager.DisplayText("Hi! I'm HUD!" + "\n (Press trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        HudManager.DisplayText("I'm here to help!" + "\n (Press trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        HudManager.DisplayText("Look at the screen.", 3);

        /*
        //
        // Generate the targets
        InstructionManager.DisplayText("First, let's set up the experiment target for you. \n\n (Press the trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

        InstructionManager.DisplayText("Move your arms to the desired position as shown in the HUD. \n\n HUD: UA - Upper arm, FA - Fore arm; in the bracket (current upper and fore arm angle / the desired one)");

        for (int i = 0; i < shoulderDesiredPos.Length; i++)
        {
            for (int j = 0; j < elbowDesiredPos.Length; j++)
            {
                if( i % 2 == 0)
                    yield return new WaitUntil(() => IsAtRightPosition(shoulderDesiredPos[i], elbowDesiredPos[j], 2.0f));
                else
                    yield return new WaitUntil(() => IsAtRightPosition(shoulderDesiredPos[i], elbowDesiredPos[elbowDesiredPos.Length-1-j], 2.0f));
                GameObject[] hand = GameObject.FindGameObjectsWithTag("IndexFingerCollider");
                //Debug.Log(indexFinger[0].transform.position.ToString("F3"));
                Vector3 location = Quaternion.Euler(0,this.worldOrientation, 0) * hand[0].transform.position;
                gridManager.AddTargetLocation(location);
            }
        }

        gridManager.AddTargetRotation(new Vector3(0.0f, 0.0f, 0.0f));
        //gridManager.AddTargetRotation(new Vector3(45.0f, 0.0f, 0.0f));
       // gridManager.AddTargetRotation(new Vector3(-45.0f, 0.0f, 0.0f));

        // Now that you are done, set the flag to indicate we are done.
        */
        welcomeDone = true;

    }

    /// <summary>
    /// Performs initialisation procedures for the experiment. Sets variables to their zero state.
    /// </summary>
    public override void InitialiseExperiment()
    {

        #region Spawn bottle grid
        // Spawn the grid
        gridManager.GenerateTargetLocations();
        gridManager.SpawnTargetGrid();
        gridManager.ResetTargetSelection();
        Debug.Log("Spawn the grid!");
        #endregion

        #region Iteration settings
        // Set iterations variables for flow control.
        targetNumber = gridManager.TargetNumber;
        //Debug.Log(iterationsPerSession.Count);

        for (int i = 0; i < iterationsPerSession.Count; i++)
        {
            iterationsPerSession[i] = targetNumber * iterationsPerTarget;

        }


        // Create the list of target indexes and shuffle it.
        for (int i = 0; i < targetNumber; i++)
        {
            for (int j = 0; j < iterationsPerTarget; j++)
                targetOrder.Add(i);
        }
        targetOrder.Shuffle();

        #endregion
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
        InstructionManager.DisplayText("Alright " + SaveSystem.ActiveUser.name + ", let me explain what we are doing today." + "\n\n (Press the trigger)");
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


        if (trainingPerSession[sessionNumber - 1] == 1)
        {

            InstructionManager.DisplayText("Let's start training then!" + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

            //
            // Start position
            InstructionManager.DisplayText("First, we'll show you the start position." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

            //
            // Start position
            InstructionManager.DisplayText("Your upper arm should be relaxed pointing downards (0 degree) while your elbow should be bent 90 degrees." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Try it. If you are not at the right position , the Hud displays: (current upper and fore arm angle/the desired one)");
            startPosPhoto.SetActive(true);
            startPosPhoto.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f), Space.World);
            yield return new WaitUntil(() => IsReadyToStart());
            startPosPhoto.SetActive(false);
            HudManager.ClearText();
            //
            // HUD Colours
            InstructionManager.DisplayText("The colour of the HUD will tell you what you need to do." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Red for returning back and adjusting your start position." + "\n\n (Press the trigger)");
            HudManager.DisplayText("I'm red!");
            HudManager.colour = HUDManager.HUDColour.Red;
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Orange for waiting for the countdown." + "\n\n (Press the trigger)");
            HudManager.DisplayText("I'm orange!");
            HudManager.colour = HUDManager.HUDColour.Orange;
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Blue for reaching for the target bottle!" + "\n\n (Press the trigger)");
            HudManager.DisplayText("I'm blue!");
            HudManager.colour = HUDManager.HUDColour.Blue;
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Green for complete the task." + "\n\n (Press the trigger)");
            HudManager.DisplayText("I'm green!");
            HudManager.colour = HUDManager.HUDColour.Green;
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("You need to hold on for a while until the HUD says Well Done, like" + "\n\n (Press the trigger)");
            HudManager.DisplayText("Well Done");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

            HudManager.ClearText();
            HudManager.colour = HUDManager.HUDColour.Red;
            //
            // End
            InstructionManager.DisplayText("Make sure you hold your position until the HUD turns green before moving back to the start position." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Otherwise, you look ready to go! Good luck!" + "\n\n (Press the trigger)");
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
        if (checkStartPosition)
        {
            return IsAtRightPosition(startShoulderAngle, startElbowAngle, startTolerance);
            #region Deprecated, use new method instead

            /*
              // Check that upper and lower arms are within the tolerated start position.
              float qShoulder = leftySign * Mathf.Rad2Deg * (upperArmTracker.GetProcessedData(5) + Mathf.PI); // Offsetting to horizontal position being 0.
              float qElbow = 0;


              HudManager.colour = HUDManager.HUDColour.Orange;
              qElbow = Mathf.Rad2Deg * (lowerArmTracker.GetProcessedData(5)) - qShoulder; // Offsetting to horizontal position being 0.
              // The difference to the start position
              float qSDiff = qShoulder - startShoulderAngle;
              float qEDiff = qElbow - startElbowAngle;

              //
              // Update information displayed for debugging purposes
              //


              //InstructionManager.DisplayText(qShoulder.ToString() + "\n" + qElbow.ToString() + "\n");

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
                      helpText += "UA: ++ (" + qShoulder.ToString("F0") + "/" + startShoulderAngle.ToString("F0") + ").\n";
                  else if (qSDiff > 0 && Mathf.Abs(qSDiff) > startTolerance)
                      helpText += "UA: -- (" + qShoulder.ToString("F0") + "/" + startShoulderAngle.ToString("F0") + ").\n";

                  if (qEDiff < 0 && Mathf.Abs(qEDiff) > startTolerance)
                      helpText += "LA: ++ (" + qElbow.ToString("F0") + "/" + startElbowAngle.ToString("F0") + ").\n";
                  else if (qEDiff > 0 && Mathf.Abs(qEDiff) > startTolerance)
                      helpText += "LA: -- (" + qElbow.ToString("F0") + "/" + startElbowAngle.ToString("F0") + ").\n"; ;

                  HudManager.DisplayText(helpText);
                  HudManager.colour = HUDManager.HUDColour.Red;


                  return false;
              }
              */
            #endregion
        }

        else
            return true;

    
    }

    /// <summary>
    /// Prepares variables and performs any procedures needed when the subject has succeded in the preparation
    /// to start performing the task.
    /// </summary>
    public override void PrepareForStart()
    {
        // Here you can do stuff like preparing objects/assets, like setting a different colour to the object

        // Select target
        gridManager.SelectTarget(targetOrder[iterationNumber - 1]);

    }

    /// <summary>
    /// Performs a procedure needed when the subject fails to start the task (goes out of the start condition).
    /// This can be: display some information, reset variables, change something in the experiment.
    /// </summary>
    public override void StartFailureReset()
    {
        // If our subject fails, do some resetting. 
        // Clear bottle selection
        gridManager.ResetTargetSelection();

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
        logData += targetOrder[iterationNumber - 1] + ",";  // Make sure you always end your custom data with a comma! Using CSV for data logging.


        // Continue with data logging.
        base.HandleTaskDataLogging();
    

        //HudManager.DisplayText(GameObject.Find("Bottle").transform.localEulerAngles.ToString());
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
        // You can implement whatever condition you want, maybe touching an object in the virtual world or being in a certain posture.

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
        // Signal the subject that the task is done
        HudManager.DisplayText("Hold on!");
        HudManager.colour = HUDManager.HUDColour.Green;
        HudManager.colour = HUDManager.HUDColour.Green;
       
        yield return new WaitForSecondsRealtime(1.0f);
        taskComplete = true;
    }

    /// <summary>
    /// Handles procedures that occur as soon as the task is completed.
    /// Extend this method by doing your own implementation, with base.HandleTaskCompletion() being called at the start.
    /// </summary>
    public override void HandleTaskCompletion()
    {


        // Stop EMG reading and save data
        delsysEMG.StopRecording();
        emgIsRecording = false;

        base.HandleTaskCompletion();

        // Reset flags
        hasReached = false;
        taskComplete = false;
    }

    /// <summary>
    /// Handles the procedures performed when analysing results.
    /// </summary>
    public override void HandleResultAnalysis()
    {
        // Do some analysis

    }

    /// <summary>
    /// Handles procedures performed when initialising the next iteration.
    /// Updates iteration number, resets the task time, and starts a new data log by default.
    /// Extend this method by doing your own implementation, with base.HandleIterationInitialisation() being called at the start.
    /// </summary>
    public override void HandleIterationInitialisation()
    {
        
        base.HandleIterationInitialisation();

    }

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    public override bool IsEndOfSession()
    {
        // You can do your own implementation of this

        

        return base.IsEndOfSession();
       


    }

    /// <summary>
    /// Handles procedures performed when initialising the next iteration.
    /// Updates iteration number, session number, resets the task time, and starts a new data log by default.
    /// Extend this method by doing your own implementation, with base.HandleSessionInitialisation() being called at the start.
    /// </summary>
    public override void HandleSessionInitialisation()
    {

        HudManager.DisplayText("New session");
        if (gridManager.CurrentTargetType == TargetGridManager.TargetType.Ball)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag("TouchyBall");
            foreach (GameObject target in targets)
            {
                Destroy(target);
            }

            gridManager.CurrentTargetType = TargetGridManager.TargetType.Bottle;
            gridManager.SpawnTargetGrid();
            gridManager.ResetTargetSelection();
            Debug.Log("Spawn the grid!");
        }

        base.HandleSessionInitialisation();

    }

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    public override bool IsEndOfExperiment()
    {
        // You can do your own implementation of this
        return base.IsEndOfExperiment();
    }

    /// <summary>
    /// Performs all the required procedures to end the experiment.
    /// Closes all UPD Sensors and all logs by default.
    /// Extend this method by doing your own implementation, with base.EndExperiment() being called at the start.
    /// </summary>
    public override void EndExperiment()
    {
        base.EndExperiment();
        delsysEMG.StopAcquisition();
        delsysEMG.Close();
        
        // You can do your own end of experiment stuff here
    }

    /// <summary>
    /// Checks if the condition for the rest period has been reached.
    /// </summary>
    /// <returns>True if the rest condition has been reached.</returns>
    public override bool IsRestTime()
    {
        // For example, give rest time after the fifth iteration.
        return iterationNumber % RestIterations == 0;
    }

    #endregion

}
