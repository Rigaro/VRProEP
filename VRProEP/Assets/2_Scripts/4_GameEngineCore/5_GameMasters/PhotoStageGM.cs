using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// GameMaster includes
using VRProEP.ExperimentCore;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;

public class PhotoStageGM : GameMaster
{
    [Header("Configuration")]
    public float synergyValue = 1.0f;
    public Transform objectTransform;
    public Vector3 objectStart;

    private ConfigurableElbowManager elbowManager;

    void Awake()
    {
        SaveSystem.LoadUserData("RG1988");
        AvatarSystem.LoadPlayer(UserType.AbleBodied, AvatarType.Transhumeral);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);
        // Initialize prosthesis
        GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
        elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
        elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + SaveSystem.ActiveUser.handLength / 2.0f), synergyValue);
        // Set the reference generator to linear synergy.
        elbowManager.ChangeReferenceGenerator("VAL_REFGEN_LINKINSYN");

    }

    private void Start()
    {
        // Initialize UI.
        //InitializeUI();

        // Configure the grasp manager
        GameObject graspManagerGO = GameObject.FindGameObjectWithTag("GraspManager");
        if (graspManagerGO == null)
            throw new System.Exception("Grasp Manager not found.");
        GraspManager graspManager = graspManagerGO.GetComponent<GraspManager>();
        graspManager.managerType = GraspManager.GraspManagerType.Controller;
        graspManager.managerMode = GraspManager.GraspManagerMode.Open;

        AvatarSystem.EnableAvatarColliders();
    }

    // Update is called once per frame
    void Update()
    {
        // Move object back to start and set synergy
        if (Input.GetKeyDown(KeyCode.Space))
        {
            objectTransform.localPosition = objectStart;
            objectTransform.rotation = Quaternion.identity;
            elbowManager.SetSynergy(synergyValue);
        }
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
        ExperimentSystem.SetActiveExperimentID("PhotoStage");

        //
        // Create data loggers
        //

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
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks if the condition for changing experiment session has been reached.
    /// </summary>
    /// <returns>True if the condition for changing sessions has been reached.</returns>
    public override bool CheckNextSessionCondition()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks if the condition for ending the experiment has been reached.
    /// </summary>
    /// <returns>True if the condition for ending the experiment has been reached.</returns>
    public override bool CheckEndCondition()
    {
        throw new System.NotImplementedException();
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

    #endregion
}
