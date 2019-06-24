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
    public Transform objectTransform;
    public Transform objectStart;
    public bool isAble = true;
    public List<GameObject> dropOffLocations = new List<GameObject>();
    public float standOffset = 0.1f;
    public Transform subjectStandLocation;

    private ConfigurableElbowManager elbowManager;


    private void Awake()
    {
        if (isAble)
            LoadAbleBodiedAvatar();
        else
            LoadTHAvatar();
    }

    private void Start()
    {
        float subjectHeight = SaveSystem.ActiveUser.height;
        float subjectArmLength = SaveSystem.ActiveUser.upperArmLength + SaveSystem.ActiveUser.forearmLength + (SaveSystem.ActiveUser.handLength / 2);
        Debug.Log(subjectArmLength);

        List<float> dropOffHeightMultipliers = new List<float>();
        dropOffHeightMultipliers.Add(0.65f);
        dropOffHeightMultipliers.Add(0.65f);
        dropOffHeightMultipliers.Add(0.65f);
        dropOffHeightMultipliers.Add(0.9f);
        List<float> dropOffReachMultipliers = new List<float>();
        dropOffReachMultipliers.Add(0.75f);
        dropOffReachMultipliers.Add(1.0f);
        dropOffReachMultipliers.Add(1.5f);
        dropOffReachMultipliers.Add(1.0f);
        // Set drop-off locations
        int i = 0;
        foreach (GameObject dropOff in dropOffLocations)
        {
            Transform dropOffTransform = dropOff.transform;
            dropOffTransform.localPosition = new Vector3((-standOffset + subjectStandLocation.localPosition.x - (dropOffReachMultipliers[i] * subjectArmLength)), dropOffHeightMultipliers[i] * subjectHeight, dropOffTransform.localPosition.z);
            i++;
        }
    }

    public void LoadAbleBodiedAvatar()
    {
        // Load
        SaveSystem.LoadUserData("RG1988");
        AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied, false);
        // Initialize UI.
        //InitializeUI();

        // Change the number for the forearm tracker being used
        GameObject faTrackerGO = GameObject.FindGameObjectWithTag("ForearmTracker");
        SteamVR_TrackedObject steamvrConfig = faTrackerGO.GetComponent<SteamVR_TrackedObject>();
        steamvrConfig.index = SteamVR_TrackedObject.EIndex.Device5;
    }

    public void LoadTHAvatar()
    {
        SaveSystem.LoadUserData("RG1988");
        AvatarSystem.LoadPlayer(UserType.AbleBodied, AvatarType.Transhumeral);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);

        // Initialize prosthesis
        GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
        elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
        elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + SaveSystem.ActiveUser.handLength / 2.0f), 1.5f);
        // Set the reference generator to jacobian-based.
        elbowManager.ChangeReferenceGenerator("VAL_REFGEN_LINKINSYN");
        //elbowManager.ChangeReferenceGenerator("VAL_REFGEN_JACOBIANSYN");

        // Initialize UI.
        //InitializeUI();
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
