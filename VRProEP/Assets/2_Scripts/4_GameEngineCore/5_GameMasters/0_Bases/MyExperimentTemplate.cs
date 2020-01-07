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

public class MyExperimentTemplate : GameMaster
{
    // Here you can place all your Unity (GameObjects or similar)
    #region Unity objects
    [Header("My Experiment Objects:")]
    // Serialised
    [SerializeField]
    private GameObject cubeGO; // e.g. some object you want to manipulate or get info from.
    [SerializeField]
    private GameObject sphereGO;

    // Hidden
    private Transform cubeTransform; // e.g. the position of the object.
    private Transform sphereTransform;
    #endregion

    // Here are all the variables you need for your experiment
    #region Experiment variables
    [Header("My Experiment Variables:")]
    // Serialised
    [SerializeField]
    private bool isCubeAvailable = false;
    [SerializeField]
    private bool isSphereAvailable = false;

    // Hidden
    private float someHiddenNumber = 0.0f;
    #endregion

    // Here are all the methods you need to write for your experiment.
    #region GameMaster Inherited Methods

    // Place debug stuff here, for when you want to test the experiment directly from the world without 
    // having to load it from the menus.
    private void Awake()
    {
        //instance = (MyExperimentTemplate)GetComponent<GameMaster>();

        if (debug)
        {
            SaveSystem.LoadUserData("MD1942"); // Load the test/demo user (Mr Demo)
            //
            // Debug using able-bodied configuration
            //
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

        // e.g. only display the hidden number when performing the task
        if (GetCurrentStateName() == State.STATE.PERFORMING_TASK)
            text += "The magic number is: " + someHiddenNumber + ".\n";
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
        // You can choose to use the base initialisation or get rid of it completely.
        base.HandleHUDColour();

        // No changes needed to HUD.
    }

    /// <summary>
    /// Initializes the ExperimentSystem and its components.
    /// Verifies that all components needed for the experiment are available.
    /// This must be done in Start.
    /// Extend this method by doing your own implementation, with base.InitExperimentSystem() being called at the start.
    /// </summary>
    public override void InitialiseExperimentSystems()
    {
        // First run the base initialisation which is needed.
        base.InitialiseExperimentSystems();

        // e.g. make sure that the needed objects have been set.
        if (cubeGO == null || sphereGO == null)
            throw new System.Exception("You need to add the objects in the Unity inspector!");

        // e.g. the update their transform handle and hide the objects
        cubeTransform = cubeGO.transform;
        sphereTransform = sphereGO.transform;
        cubeGO.SetActive(false);
        sphereGO.SetActive(false);
                
        //
        // Hand tracking sensor for demo
        //
        GameObject handGO = GameObject.FindGameObjectWithTag("Hand");
        VirtualPositionTracker handTracker = new VirtualPositionTracker(handGO.transform);
        ExperimentSystem.AddSensor(handTracker);
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
        InstructionManager.DisplayText("This is the welcome. Here you can write a welcome for your subject! \n Press the trigger button to continue...");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
        //
        HudManager.ClearText();
        InstructionManager.DisplayText("It's awesome! \n Press the trigger button to continue...");
        yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

        // Now that you are done, set the flag to indicate we are done.
        welcomeDone = true;
    }

    /// <summary>
    /// Performs initialisation procedures for the experiment. Sets variables to their zero state.
    /// </summary>
    public override void InitialiseExperiment()
    {
        // e.g. activate the sphere and set the hidden number to 42.
        sphereGO.SetActive(true);
        someHiddenNumber = 42.0f;

        // e.g. then set the object positions relative to the player position
        // Get player object
        GameObject headGO = GameObject.FindGameObjectWithTag("Head");
        if (headGO == null)
            throw new System.NullReferenceException("Head GameObject not found.");
        // Set the position
        cubeTransform.position = headGO.transform.position + new Vector3(-0.3f, -0.6f, 0.3f); // Front right of the subject (30cm)
        sphereTransform.position = headGO.transform.position + new Vector3(-0.3f, -0.6f, -0.3f); // Front left of the subject (30cm)
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
        InstructionManager.DisplayText("These are the isntructions. You can use other conditions to wait, not only the trigger press! e.g. 10 seconds.");
        HudManager.DisplayText("You can use the HUD too!");
        yield return new WaitForSecondsRealtime(10.0f); // Wait for some time.
        //
        HudManager.DisplayText("And get their attention!", 5.0f);
        InstructionManager.DisplayText("It's exciting! \n Press the trigger button to continue...");
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
        if(trainingPerSession[sessionNumber-1] == 1)
        {

            //
            InstructionManager.DisplayText("This is training. You'll probably want to guide your subject through the task here! \n Press the trigger button to continue...");
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.
                                                          //
            InstructionManager.DisplayText("Like... Punch the Cube!");
            yield return new WaitForSecondsRealtime(5.0f);
            HudManager.DisplayText("What Cube???", 2.0f);
            yield return WaitForSubjectAcknowledgement(); // And wait for the subject to cycle through them.

            //
            HudManager.DisplayText("On your left, quick!!", 5.0f);
            if (isCubeAvailable != true)
            {
                isCubeAvailable = true;
                cubeGO.SetActive(true);
            }
            yield return new WaitForSecondsRealtime(5.0f);
            HudManager.DisplayText("And it's gone...", 3.0f);
            isCubeAvailable = false;
            cubeGO.SetActive(false);
            yield return new WaitForSecondsRealtime(5.0f);

            //
            HudManager.DisplayText("Look to the screen.");
            InstructionManager.DisplayText("It's revolutionary ain't it! \n Press the trigger button to continue...");
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
        // You can implement whatever condition you want, maybe touching an object in the virtual world or being in a certain posture.
        // Here we are just going to use a button: Space!. So, the subject needs to hold the space bar.
        return Input.GetKey(KeyCode.Space);
    }

    /// <summary>
    /// Prepares variables and performs any procedures needed when the subject has succeded in the preparation
    /// to start performing the task.
    /// </summary>
    public override void PrepareForStart()
    {
        // Here you can do stuff like preparing objects/assets, like setting a different colour to the object
        // we want the subject to reach.
        // For demo purposes, we'll just randomly pick between cube and sphere!
        int randomNumber = Random.Range(0, 1); // The one is inclusive.
        if (randomNumber == 0) // Select cube
        {
            isCubeAvailable = true;
            cubeGO.SetActive(true);
            isSphereAvailable = false;
            sphereGO.SetActive(false);
        }
        else // Select sphere
        {
            isCubeAvailable = false;
            cubeGO.SetActive(false);
            isSphereAvailable = true;
            sphereGO.SetActive(true);
        }
    }

    /// <summary>
    /// Performs a procedure needed when the subject fails to start the task (goes out of the start condition).
    /// This can be: display some information, reset variables, change something in the experiment.
    /// </summary>
    public override void StartFailureReset()
    {
        // If our subject fails, do some resetting. 
        // In this example it'll happen if the subject stops holding the space bar.
        // So we enable both objects
        isCubeAvailable = true;
        cubeGO.SetActive(true);
        isSphereAvailable = true;
        sphereGO.SetActive(true);

        someHiddenNumber++;
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
        logData += someHiddenNumber + ",";  // Make sure you always end your custom data with a comma! Using CSV for data logging.

        // Continue with data logging.
        base.HandleTaskDataLogging();
    }

    /// <summary>
    /// Handles procedures that occurs while the task is being executed (and not related to data logging).
    /// </summary>
    /// <returns></returns>
    public override void HandleInTaskBehaviour()
    {
        // For demo purposes, we'll hide whatever object is active after 10 seconds
        if (taskTime >= 10 && isCubeAvailable)
        {
            cubeGO.SetActive(false);
            isCubeAvailable = false;
        }
        else if (taskTime >= 10 && isSphereAvailable)
        {
            sphereGO.SetActive(false);
            isSphereAvailable = false;
        }

    }

    /// <summary>
    /// Checks whether the task has be successfully completed or not.
    /// </summary>
    /// <returns>True if the task has been successfully completed.</returns>
    public override bool IsTaskDone()
    {
        // You can implement whatever condition you want, maybe touching an object in the virtual world or being in a certain posture.
        // Here we are just going to use a button: End!. So, the subject needs to hit End once.
        return Input.GetKeyDown(KeyCode.End);
    }

    /// <summary>
    /// Handles procedures that occur as soon as the task is completed.
    /// </summary>
    public override void HandleTaskCompletion()
    {
        // Let's just reactivate the objects
        isCubeAvailable = true;
        cubeGO.SetActive(true);
        isSphereAvailable = true;
        sphereGO.SetActive(true);
    }

    /// <summary>
    /// Handles the procedures performed when analysing results.
    /// </summary>
    public override void HandleResultAnalysis()
    {
        // Do some analysis
        if (someHiddenNumber != 42.0f)
            HudManager.DisplayText("Slap!");
    }

    /// <summary>
    /// Handles procedures performed when initialising the next iteration.
    /// Updates iteration number, resets the task time, and starts a new data log by default.
    /// Extend this method by doing your own implementation, with base.HandleIterationInitialisation() being called at the start.
    /// </summary>
    public override void HandleIterationInitialisation()
    {
        base.HandleIterationInitialisation();

        // Return variables to their initial condition
        someHiddenNumber = 42.0f;
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
        base.HandleSessionInitialisation();

        // Return variables to their initial condition
        someHiddenNumber = 42.0f;
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
