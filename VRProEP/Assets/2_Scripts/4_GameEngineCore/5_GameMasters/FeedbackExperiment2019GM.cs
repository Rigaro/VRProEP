using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.ExperimentCore;
using VRProEP.Utilities;

public class FeedbackExperiment2019GM : GameMaster
{


    public bool skipAll = false;
    public bool skipTraining = false;

    private float taskTime = 0.0f;
        

    public enum FeedbackExperiment { Force, Roughness, Mixed }
    public enum VisualFeebackType {None, On }
    public List<float> forceLevels = new List<float> {0.2f, 0.5f, 0.8f };
    public List<float> roughnessLevels = new List<float> { 0.2f, 0.5f, 0.8f };
    public List<int> iterationsPerSessionPerSetting = new List<int> { 5, 5, 5, 5, 5, 5 };
    public List<int> trainingPerSessionPerSetting = new List<int> { 1, 1, 0, 1, 1, 0 };
    public List<FeedbackExperiment> sessionType = new List<FeedbackExperiment> { FeedbackExperiment.Force, FeedbackExperiment.Roughness, FeedbackExperiment.Mixed, FeedbackExperiment.Force, FeedbackExperiment.Roughness, FeedbackExperiment.Mixed }; //size 6 def.(Force Roughness Mixed Force Roughness Mixed)
    public List<VisualFeebackType> visualFeedbackType = new List<VisualFeebackType> { VisualFeebackType.On, VisualFeebackType.On, VisualFeebackType.On, VisualFeebackType.None, VisualFeebackType.None, VisualFeebackType.None }; // size 6 def.(on on on none none none)
    public List<int> iterationsPerSession;
    public List<int> trainingPerSession; //
    private int numberOfIterations;
    private int iterationNumberTotal;
    private int iterationNumberCounterTotal;
    public int restIterations = 25;

    // Experiment management
    private bool hasFeedback = true;
    private bool trainingEnd = false;
    private bool inTraining = false;
    private bool instructionsEnd = false;
    private bool inInstructions = false;
    private bool inSessionInstructions = false;
    private bool inSessionInstructionsEnd = false;
    private string infoText;
    private bool logEnd = false;

    // Device managment


    // Prosthesis handling objects

    // Data logging:
    // continous
    // time, force, 

    // after each iteration
    // roughnessTarget , roughnessReported


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
                    iterationsPerSession.Add(iterationsPerSessionPerSetting[i] * forceLevels.Count);
                    trainingPerSession.Add(trainingPerSessionPerSetting[i] * forceLevels.Count);
                    break;
                case FeedbackExperiment.Roughness:
                    iterationsPerSession.Add(iterationsPerSessionPerSetting[i] * roughnessLevels.Count);
                    trainingPerSession.Add(trainingPerSessionPerSetting[i] * roughnessLevels.Count);
                    break;
                case FeedbackExperiment.Mixed:
                    iterationsPerSession.Add(iterationsPerSessionPerSetting[i] * forceLevels.Count * roughnessLevels.Count);
                    trainingPerSession.Add(trainingPerSessionPerSetting[i] * forceLevels.Count * roughnessLevels.Count);
                    break;
                default:
                    break;
            }
            iterationNumberTotal += iterationsPerSession[i]; 
        }
        iterationNumberCounterTotal = 1;

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

                //
                // Initialize data logs
                //

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
                    inSessionInstructions = false;
                    hudManager.DisplayText("Ready to start!", 2.0f);
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

                // Check if pause requested
                UpdatePause();
                switch (waitState)
                {
                    // Waiting for subject to get to start position.
                    case WaitState.Waiting:
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
                            StopAllCoroutines();
                            counting = true;
                            HUDCountDown(3);
                        }
                        // If all is good and the countdownDone flag is raised, switch to reaching.
                        if (countdownDone)
                        {
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

                // 
                // Data logging
                //

                //
                // Flow managment
                //
                // Rest for some time when required
                if (CheckRestCondition())
                {
                    hudManager.DisplayText("Take a " + restTime + " seconds rest.", 6.0f);
                    SetWaitFlag(restTime);
                    experimentState = ExperimentState.Resting;
                }
                // Check whether the experiment end condition is met
                else if (CheckEndCondition())
                {
                    hudManager.DisplayText("Experiment end. Thank you!", 6.0f);
                    experimentState = ExperimentState.End;
                }
                // Check whether the new session condition is met
                else if (CheckNextSessionCondition())
                {
                    experimentState = ExperimentState.InitializingNextSession;
                }
                else//iterations
                    hudManager.DisplayText("Good job!", 2.0f);
                    // Allow 3 seconds after task end to do calculations
                    SetWaitFlag(3.0f);
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
                    iterationNumberCounterTotal++;

                    // 
                    // Update log requirements
                    //
                    //log session save....!!!!!!!!!!!!



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
                //
                // Perform session closure procedures
                //

                //
                // Initialize new session variables and flow control
                //
                iterationNumber = 1;
                sessionNumber++;                
                //
                // Initialize data logging
                //
                ExperimentSystem.GetActiveLogger(1).AddNewLogFile(sessionNumber, iterationNumber, "Data format");

                hudManager.DisplayText("Take a " + restTime + " seconds rest.", 6.0f);
                SetWaitFlag(restTime);

                experimentState = ExperimentState.Resting; // Initialize next session
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
                    hudManager.DisplayText("Get ready to restart!", 3.0f);
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

                //
                // Append data to lists
                //
                taskTime += Time.fixedDeltaTime;

                //
                // Log current data
                //
                ExperimentSystem.GetActiveLogger(1).AppendData(logData);

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
                    ExperimentSystem.GetActiveLogger(1).CloseLog();

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

    /// <summary>
    /// Returns the progress update String
    /// </summary>
    /// <returns></returns>
    private string GetInfoText()
    {
        string Text;
        Text = "Status: Waiting to start.\n";
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
        ExperimentSystem.SetActiveExperimentID("template");

        //
        // Create data loggers
        //

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
        if (true)
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
    public override bool CheckNextSessionCondition()
    {
        if (iterationNumber >= iterationsPerSession[sessionNumber])
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
    public override bool CheckEndCondition()
    {
        if (sessionNumber >= iterationsPerSession.Count && iterationNumber >= iterationsPerSession[sessionNumber])
            return true;
        else
            return false;
    }

    /// <summary>
    /// Launches the next session. Performs all the required preparations.
    /// </summary>
    public override void LaunchNextSession()
    {
        //No training in Session: Mixed or no Visual Feedback
        if (sessionType[sessionNumber] == FeedbackExperiment.Mixed || visualFeedbackType[sessionNumber] == VisualFeebackType.None)
        {
            skipTraining = true;
        }
        else
        {
            skipTraining = false;
        }

        //check visual feedback on
        if(visualFeedbackType[sessionNumber] == VisualFeebackType.On)
        {
            //load visual feedback on assets
        }
        else if(visualFeedbackType[sessionNumber] == VisualFeebackType.None)
        {
            //load visual feedback off assets
        }

        //check which feedback
        switch(sessionType[sessionNumber])
        {
            case FeedbackExperiment.Force:
                //load force feedback assets

                break;
            case FeedbackExperiment.Roughness:
                //load Roughness feedback assets

                break;
            case FeedbackExperiment.Mixed:
                //load mixed feedback assets

                break;
            default:
                break;
        }
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

        string defaultText = "Instructions:\n";
        string continueText = "\n\n...Press the Trigger to continue...";

        if (!skipTraining && trainingPerSession[sessionNumber] >= 1)
        {
            hudManager.DisplayText("Please look at the monitor. Top-right.");
            yield return new WaitForSeconds(3.0f);
               
            // Introduce experiment modality.
            instructionManager.DisplayText("Welcome to prosthesis training." + defaultText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);

            switch (sessionType[sessionNumber])
            {
                case FeedbackExperiment.Force://explain force limits X different once
                    instructionManager.DisplayText("In this sessions' training you will use flexion and extension of your hand to control the grasp force of the hand." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("The object will be attached to your hand as soon as you touch it." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("The objects' colour will indicate the level of grasp force required." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("Grasp force for the objects' required is red:light yellow:middle  purple:hard." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    if (visualFeedbackType[sessionNumber] == VisualFeebackType.On) //visual feedback
                    {
                        instructionManager.DisplayText("The colour of the object will change with the grasp force giving you visual feedback how close to the target force you are." + defaultText);
                        yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                        yield return new WaitForSeconds(0.5f);
                    }
                    instructionManager.DisplayText("In addition the tactile feedback will indicate the grasp force." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("The harder you grip the object the more the tactile feedback will vibrate." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("Get ready to start! Look forward towards the desk.");
                    hudManager.DisplayText("Look forward.", 3.0f);
                    yield return new WaitForSeconds(5.0f);
                    HUDCountDown(3);
                    yield return new WaitForSeconds(5.0f);

                    //present force level 1,2,3
                    
                    break;
                case FeedbackExperiment.Roughness://roughness
                    instructionManager.DisplayText("In this sessions' training you will get feedback about the objects surface roughness." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("The object will be attached to your hand as soon as you touch it." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("The tactile feedback will vibrate with different frequencies according to the surface roughness." + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("Low frequencies will indicate a smooth surface while high frequencies will indicate a rough surface" + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("You will be asked to classify if the surface is smooth, medium or rough" + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("Therefore after grasping the object, touch the suitable ballon: red:smooth yellow:middle  purple:rough " + defaultText);
                    yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                    yield return new WaitForSeconds(0.5f);
                    instructionManager.DisplayText("Get ready to start! Look forward towards the desk.");
                    hudManager.DisplayText("Look forward.", 3.0f);
                    yield return new WaitForSeconds(5.0f);
                    HUDCountDown(3);
                    yield return new WaitForSeconds(5.0f);

                    //present roughness level 1,2,3

                    break;
                case FeedbackExperiment.Mixed://mixed
                    throw new System.NotImplementedException();
                    break;
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

            instructionManager.DisplayText("Today we will be testing your grasping force capabilities when using different tactile feedback." + defaultText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "The experiment requires you to repeat the grasping task for " + iterationNumberTotal + " iterations." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "The grasping will be countrolled by your EMG activity controlling the grasping force via flexing/extending your hand." + continueText);
            yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
            yield return new WaitForSeconds(0.5f);
            instructionManager.DisplayText(defaultText + "Different tactile feedback will be given to you, explained before the experiment." + continueText);
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
            instructionManager.DisplayText("Get ready to start! Look forward towards the desk.");
            hudManager.DisplayText("Look forward.", 3.0f);
            yield return new WaitForSeconds(5.0f);
            HUDCountDown(3);
            yield return new WaitForSeconds(5.0f);

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

        switch (sessionType[sessionNumber])
        {
            case FeedbackExperiment.Force:
                instructionManager.DisplayText("In this session you will use flexion and extension of your hand to control the grasp force of the hand as shown in the training." + defaultText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText("Start the experiment by pressing the button" + defaultText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText("After adjusting the force you can stop the experiment by pressing the button" + defaultText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);

                break;
            case FeedbackExperiment.Roughness://roughness
                instructionManager.DisplayText("In this session you will get feedback about the objects surface roughness as shown in the training." + defaultText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);

                break;
            case FeedbackExperiment.Mixed://mixed
                instructionManager.DisplayText("In this session you will use flexion and extension of your hand to control the grasp force of the hand and will get feedback about the objects surface roughness as shown in the training." + defaultText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText("Start the experiment by pressing the button" + defaultText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText("After adjusting the force you can stop the experiment by pressing the button" + defaultText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                instructionManager.DisplayText("Then you can touch the suitable ballon: red:smooth yellow:middle  purple:rough" + defaultText);
                yield return new WaitUntil(() => buttonAction.GetStateDown(SteamVR_Input_Sources.Any));
                yield return new WaitForSeconds(0.5f);
                break;
            default:
                break;
        }
        instructionManager.DisplayText("Get ready to start! Look forward towards the desk.");
        hudManager.DisplayText("Look forward.", 3.0f);
        yield return new WaitForSeconds(5.0f);
        HUDCountDown(3);
        yield return new WaitForSeconds(5.0f);

        inSessionInstructionsEnd = true;
    }


    #endregion
}
