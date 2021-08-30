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
using VRProEP.AdaptationCore;
using VRProEP.Utilities;

public class GridReaching2020GM : GameMaster
{ 
    // added by Damian
    private PyTCPRequester pyTCPRequester;
    private float[] data = { 1.0f, 1.0f, 1.0f };
    private float[] terminateData = { 0.0f };



    // Here you can place all your Unity (GameObjects or similar)
    #region Unity objects
    //[Header("Experiment configuration: Data format")]
    [SerializeField]
    private string ablebodiedDataFormat = "loc,t,aDotE,bDotE,gDotE,aE,bE,gE,xE,yE,zE,aDotUA,bDotUA,gDotUA,aUA,bUA,gUA,xUA,yUA,zUA,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,xSH,ySH,zSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xUB,yUB,zUB,xHand,yHand,zHand,aHand,bHand,gHand";
    [SerializeField]
    private string prostheticDataFormat = "loc,t,aDotUA,bDotUA,gDotUA,aUA,bUA,gUA,xUA,yUA,zUA,qE,qDotE,aDotSH,bDotSH,gDotSH,aSH,bSH,gSH,xSH,ySH,zSH,aDotUB,bDotUB,gDotUB,aUB,bUB,gUB,xUB,yUB,zUB,xHand,yHand,zHand,aHand,bHand,gHand,enable";
    [SerializeField]
    private string ablebodiedPerformanceDataFormat = "i,J,loc,targetX,targetY,targetZ";
    [SerializeField]
    private string prostheticPerformanceDataFormat = "i,J,uf,du,d2u,thetaBar,theta,loc,targetX,targetY,targetZ";
    private string performanceDataFormat;

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

    [SerializeField]
    private GameObject startPosPhoto;

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
    private List<Vector3> laDataBuffer;
    private List<Vector3> uaDataBuffer;
    private List<Vector3> shDataBuffer;
    private List<Vector3> c7DataBuffer;
    private TransCyberHPIPersonalisation personaliser;
    //private UpperBodyCompensationMotionPM evaluator;
    private IPerformanceManager evaluator;
    [Header("Experiment configuration: Personalisation")]
    [SerializeField]
    [Range(0.1f, 3.0f)]
    private float theta = 0.5f;
    [SerializeField]
    private float ditherAmplitude = 0.05f;
    [SerializeField]
    private float optimiserGain = 0.001f;
    [SerializeField]
    private bool adaptiveSynergy = true;

    // Additional data logging
    private DataStreamLogger performanceDataLogger;

    // Other helpful stuff
    private float leftySign = 1.0f;

    // Experiment configuration file objects
    public enum EvaluatorType
    {
        Compensation,
        KinematicEnergy
    }
    [SerializeField]
    private EvaluatorType evaluatorType = EvaluatorType.Compensation;
    private class GridReachingConfigurator
    {
        public int gridRows = 3;
        public int gridColumns = 3;
        public float gridSpacing = 0.2f;
        public float gridHeightMultiplier = 0.8f;
        public float gridReachMultiplier = 0.9f;
        public int iterationsPerTarget = 10;
        public EvaluatorType evaluatorType = EvaluatorType.Compensation;
        public bool adaptiveSynergy = true;
    }
    private GridReachingConfigurator configurator;

    #endregion

    // Here are all the methods you need to write for your experiment.
    #region GameMaster Inherited Methods

    // Added by Damian
    //private void Start()
    //{

        //pyTCPRequester = new PyTCPRequester(data);
        //pyTCPRequester.Start();

       // base.Start();
    // }

    // Added by Damian
    // Fixed update method to test socket connection with matlab
    protected override void FixedUpdate()
    {


        /*if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("Terminate client thread");
            pyTCPRequester.newData(terminateData);
            pyTCPRequester.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("Send new data");


            pyTCPRequester.newData(data);
        }
        else
        {

            Debug.Log("Send new data");


            pyTCPRequester.newData(data);

            data[1] += 0.01f;
            data[2] += 0.01f;
            data[3] += 0.01f;
        } */




        base.FixedUpdate();

    }

    // Place debug stuff here, for when you want to test the experiment directly from the world without 
    // having to load it from the menus.
    private void Awake()
    {
        if (debug)
        {
            //// Save some test config data
            //string configFilePath = Application.dataPath + "/Resources/Experiments/GridReaching2020.json";
            //Debug.Log(configFilePath);
            //configurator = new GridReachingConfigurator();
            //string configuratorAsJson = JsonUtility.ToJson(configurator);
            //File.WriteAllText(configFilePath, configuratorAsJson);

            //
            // Debug able
            //
            SaveSystem.LoadUserData("TB1995175"); // Load the test/demo user (Mr Demo)
            //
            // Debug using able-bodied configuration
            //
            /*
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);
            */

            //
            // Debug prosthetic
            //
            
            AvatarSystem.LoadPlayer(UserType.Ablebodied, AvatarType.Transhumeral);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);
            // Initialize prosthesis
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            ConfigurableElbowManager elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
            elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + SaveSystem.ActiveUser.handLength / 2.0f));
            // Set the reference generator to jacobian-based.
            //elbowManager.ChangeReferenceGenerator("VAL_REFGEN_JACOBIANSYN");
            // Set the reference generator to linear synergy.

            // Set the reference generator to neural network (added by Damian
            elbowManager.ChangeReferenceGenerator("VAL_REFGEN_NN");
            
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

        // Make sure the user knows the elbow is not enabled.
        if (experimentType == ExperimentType.TypeTwo)
        {
            if (!elbowManager.IsEnabled)
                HudManager.centreColour = HUDManager.HUDCentreColour.Yellow;
            else
                HudManager.centreColour = HUDManager.HUDCentreColour.None;
        }
    }

    /// <summary>
    /// Configures the experiment from a text file.
    /// The method needs to be extended to extract data from the configuration file that is automatically loaded.
    /// </summary>
    public override void ConfigureExperiment()
    {
        
        string expCode = "";
        if (AvatarSystem.AvatarType == AvatarType.AbleBodied)
            expCode = "_A";
        else if (AvatarSystem.AvatarType == AvatarType.Transhumeral)
            expCode = "_P";

        if (debug)
            configAsset = Resources.Load<TextAsset>("Experiments/" + this.gameObject.name + expCode);
        else
            configAsset = Resources.Load<TextAsset>("Experiments/" + ExperimentSystem.ActiveExperimentID + expCode);


       

        

        // Convert configuration file to configuration class.
        
        configurator = JsonUtility.FromJson<GridReachingConfigurator>(configAsset.text);
        // Load data
        gridRows = configurator.gridRows;
        gridColumns = configurator.gridColumns;
        gridSpacing = configurator.gridSpacing;
        gridHeightMultiplier = configurator.gridHeightMultiplier;
        gridReachMultiplier = configurator.gridReachMultiplier;
        iterationsPerTarget = configurator.iterationsPerTarget;
        evaluatorType = configurator.evaluatorType;
        adaptiveSynergy = configurator.adaptiveSynergy;
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
            performanceDataFormat = ablebodiedPerformanceDataFormat;
        }
        // Type two if prosthetic (Adaptive Synergy)
        else if (AvatarSystem.AvatarType == AvatarType.Transhumeral)
        {
            experimentType = ExperimentType.TypeTwo; // Able-bodied experiment type
            taskDataFormat = prostheticDataFormat;
            performanceDataFormat = prostheticPerformanceDataFormat;
        }
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

        // Restart UDP threads
        foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
        {
            if (sensor is UDPSensorManager udpSensor)
            {
                //Debug.Log(wifiSensor.RunThread);
                udpSensor.StartSensorReading();
                //Debug.Log(wifiSensor.RunThread);
            }
        }

        // Send the player to the experiment centre position
        TeleportToStartPosition();

        startPosPhoto.SetActive(false);

        //
        // Create the performance data loggers
        //
        performanceDataLogger = new DataStreamLogger("PerformanceData");
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
            elbowManager.ChangeReferenceGenerator("VAL_REFGEN_NN");

            // Create the personalisation algorithm object
            elbowManager.SetSynergy(theta);
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

        if (evaluatorType == EvaluatorType.Compensation)
        {
            // Performance evaluation objects
            evaluator = new UpperBodyCompensationMotionPM(0.5f, 0.5f);
            shDataBuffer = new List<Vector3>();
            c7DataBuffer = new List<Vector3>();
        }
        else if (evaluatorType == EvaluatorType.KinematicEnergy)
        {
            throw new System.NotImplementedException("KE method not yet implemented");
        }

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
        InstructionManager.DisplayText("Hi " + SaveSystem.ActiveUser.name + "! Welcome to the virtual world. \n\n (Press the trigger button to continue...)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        InstructionManager.DisplayText("Make sure you are standing on top of the green circle. \n\n (Press the trigger button to continue...)");
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
        InstructionManager.DisplayText("Alright " + SaveSystem.ActiveUser.name + ", let me explain what we are doing today." + "\n\n (Press the trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        InstructionManager.DisplayText("Today you are doing the " + AvatarSystem.AvatarType + " version of the experiment, which means..." + "\n\n (Press the trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

        // Type instructions
        if (experimentType == ExperimentType.TypeOne)
        {
            InstructionManager.DisplayText("You'll be reaching for a bunch of floating colorful spheres!" + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("You should see 9 spheres arranged in a 3x3 matrix. They should all be reachable, if not, please let me know." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("You are not allowed to step! So if they are too far away, I'll adjust them." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("You'll have to touch each sphere a total of " + iterationsPerTarget + " times." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("The order will be random; however, you'll be shown which one to touch before each reach." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("You don't need to do anything special, just reach as you normally would, comfortably." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        }
        else if (experimentType == ExperimentType.TypeTwo)
        {
            InstructionManager.DisplayText("You'll be reaching for a single floating colorful sphere while using a smart prosthesis!" + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("You'll have to reach for the sphere a total of " + iterationsPerTarget + " times." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("At the beginning it might be a bit difficult to reach and you'll need to compensate with your upper body." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("You should not step though!" + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("However, after each reach, the prosthesis will learn from your movement and adjust itself." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("After a few reachs it should be a piece of cake." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        }
        else
            throw new System.Exception("The experiment type is not valid. Place: Instructions");

        InstructionManager.DisplayText("There is no break in this experiment, but if you feel tired or any discomfort let me know immediately and we'll take a break or stop." + "\n\n (Press the trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        InstructionManager.DisplayText("Remember you are in VR, so the objects you see are not actually there. So... Don't lean on them!" + "\n\n (Press the trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        InstructionManager.DisplayText("Also, objects you can't see may be around you, so don't go running around. You'll have a chance to explore at the end. :)" + "\n\n (Press the trigger)");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        InstructionManager.DisplayText("If you are ready, let's start training!" + "\n\n (Press the trigger)");
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
            InstructionManager.DisplayText("Let's start training then!" + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

            // Type instructions
            if (experimentType == ExperimentType.TypeOne)
            {
                InstructionManager.DisplayText("First, we'll show you the start position." + "\n\n (Press the trigger)");
                yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            }
            else if (experimentType == ExperimentType.TypeTwo)
            {
                InstructionManager.DisplayText("First, we'll show how to use the prosthesis." + "\n\n (Press the trigger)");
                yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
                InstructionManager.DisplayText("When the prosthesis is not enabled, the HUD centre will turn yellow." + "\n\n (Press the trigger)");
                HudManager.centreColour = HUDManager.HUDCentreColour.Yellow;
                HudManager.DisplayText("\n <- Like so.");
                yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
                HudManager.ClearText();
                InstructionManager.DisplayText("To activate the prosthesis, press the circle button.");
                yield return new WaitUntil(() => elbowManager.IsEnabled); // And wait for the subject to enable it.
                HudManager.DisplayText("\n <- I'll be transparent!");
                InstructionManager.DisplayText("Try moving it around." + "\n\n (Press the trigger)");
                yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
                InstructionManager.DisplayText("Nice! Let's deactivate the prosthesis for now." + "\n\n (Press the trigger)");
                yield return new WaitUntil(() => !elbowManager.IsEnabled); // And wait for the subject to disable it.
                InstructionManager.DisplayText("Now, we'll show you the start position." + "\n\n (Press the trigger)");
                yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            }

            //
            // Start position
            InstructionManager.DisplayText("Your upper arm should be relaxed pointing downards while your elbow should be bent 90 degrees." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Like so:" + "\n Try it.");
            startPosPhoto.SetActive(true);
            yield return new WaitUntil(() => IsReadyToStart());
            startPosPhoto.SetActive(false);
            HudManager.ClearText();
            //
            // HUD Colours
            InstructionManager.DisplayText("The colour of the HUD will tell you what you need to do." + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Red for adjusting your start position." + "\n\n (Press the trigger)");
            HudManager.DisplayText("I'm red!");
            HudManager.colour = HUDManager.HUDColour.Red;
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Orange for waiting for the countdown." + "\n\n (Press the trigger)");
            HudManager.DisplayText("I'm orange!");
            HudManager.colour = HUDManager.HUDColour.Orange;
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Blue for reaching for the sphere!" + "\n\n (Press the trigger)");
            HudManager.DisplayText("I'm blue!");
            HudManager.colour = HUDManager.HUDColour.Blue;
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Green for returning to the start position." + "\n\n (Press the trigger)");
            HudManager.DisplayText("I'm green!");
            HudManager.colour = HUDManager.HUDColour.Green;
            //
            // Practice
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("Now, let's practice reaching for a target!" + "\n\n (Press the trigger)");
            HudManager.ClearText();
            HudManager.colour = HUDManager.HUDColour.Red;
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("The sphere that you need to reach will turn blue." + "\n\n (Press the trigger)");
            gridManager.SelectBall(0);
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            InstructionManager.DisplayText("You'll have to wait for a three second countdown. Look at the sphere and get ready!" + "\n\n (Press the trigger)");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
            HudManager.colour = HUDManager.HUDColour.Orange;
            HUDCountDown(3);
            yield return new WaitUntil(() => CountdownDone); // And wait 
            InstructionManager.DisplayText("Reach for it!!");
            HudManager.DisplayText("Reach for it!!");
            HudManager.colour = HUDManager.HUDColour.Blue;
            yield return new WaitUntil(() => IsTaskDone());
            // Signal the subject that the task is done
            HudManager.colour = HUDManager.HUDColour.Green;
            // Reset flags
            hasReached = false;
            taskComplete = false;
            HudManager.DisplayText("You can relax now.", 3);
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

        // Make sure the user knows the elbow is not enabled.
        if (experimentType == ExperimentType.TypeTwo)
        {
            if (!elbowManager.IsEnabled)
                HudManager.centreColour = HUDManager.HUDCentreColour.Yellow;
            else
                HudManager.centreColour = HUDManager.HUDCentreColour.None;
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
        shDataBuffer.Clear();
        c7DataBuffer.Clear();
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
    /// Handles task data logging which runs on Update.
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
            if (evaluatorType == EvaluatorType.Compensation)
            {
                Vector3 shPos = new Vector3(shoulderTracker.GetProcessedData("X_Pos"), shoulderTracker.GetProcessedData("Y_Pos"), shoulderTracker.GetProcessedData("Z_Pos"));
                Vector3 c7Pos = new Vector3(c7Tracker.GetProcessedData("X_Pos"), c7Tracker.GetProcessedData("Y_Pos"), c7Tracker.GetProcessedData("Z_Pos"));
                shDataBuffer.Add(shPos);
                c7DataBuffer.Add(c7Pos);
            }
            else if (evaluatorType == EvaluatorType.KinematicEnergy)
            {
                throw new System.NotImplementedException("KE method not yet implemented");
            }
        }
    }

    /// <summary>
    /// Handles procedures that occurs while the task is being executed (and not related to data logging).
    /// </summary>
    /// <returns></returns>
    public override void HandleInTaskBehaviour()
    {
        // Make sure the user knows the elbow is not enabled.
        if (experimentType == ExperimentType.TypeTwo)
        {
            if (!elbowManager.IsEnabled)
                HudManager.centreColour = HUDManager.HUDCentreColour.Yellow;
            else
                HudManager.centreColour = HUDManager.HUDCentreColour.None;
        }

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
            if (evaluatorType == EvaluatorType.Compensation)
            {
                evaluator.AddData<Vector3>(shDataBuffer, UpperBodyCompensationMotionPM.SHOULDER);
                evaluator.AddData<Vector3>(c7DataBuffer, UpperBodyCompensationMotionPM.TRUNK);
            }
            else if (evaluatorType == EvaluatorType.KinematicEnergy)
            {
                throw new System.NotImplementedException("KE method not yet implemented.");
            }
            float J = evaluator.Update();

            //Debug.Log("J = " + J);
            string iterationResults = iterationNumber + "," +
                                     J;

            // If it's able-bodied, no need to do anything
            // If it's KE-Adaptive-Synergy then perform synergy update
            if (experimentType == ExperimentType.TypeTwo && adaptiveSynergy)
            {
                // Perform update
                theta = personaliser.UpdateParameter(J, iterationNumber);
                elbowManager.SetSynergy(theta);
                // Add algorithm states data to log
                foreach (float value in personaliser.GetStates())
                    iterationResults += "," + value;

                iterationResults += "," + theta;
            }
            Vector3 ballPos = gridManager.GetSelectedBallPosition();
            iterationResults += "," + targetOrder[iterationNumber - 1] + "," + ballPos.x + "," + ballPos.y + "," + ballPos.z;
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
        performanceDataLogger.AddNewLogFile(AvatarSystem.AvatarType.ToString(), sessionNumber, ablebodiedPerformanceDataFormat); // Add file

        // Clear ball
        gridManager.ResetBallSelection();
    }

    #endregion

}
