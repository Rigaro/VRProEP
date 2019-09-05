using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;

public class ModeSelectionMenu : MonoBehaviour {
    
    public GameObject experimentMenu;
    public LogManager logManager;

    public void LoadPlayground()
    {
        // Load player and able bodied avatar (without adaptive tracker).
        SaveSystem.LoadUserData("MD1942");
        AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied, false);
        // Change the number for the forearm tracker being used
        GameObject faTrackerGO = GameObject.FindGameObjectWithTag("ForearmTracker");
        SteamVR_TrackedObject steamvrConfig = faTrackerGO.GetComponent<SteamVR_TrackedObject>();
        steamvrConfig.index = SteamVR_TrackedObject.EIndex.Device5;
        //
        KeepOnLoad();
        // Load level.
        SteamVR_LoadLevel.Begin("DemoPlayground");

        /*
        KeepOnLoad();
        // Load level.
        if (AvatarSystem.AvatarType == AvatarType.Transradial)
        {
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            FakeEMGBoniHand prosthesisManager = prosthesisManagerGO.GetComponent<FakeEMGBoniHand>();
            prosthesisManager.InitializeProsthesis();
        }
        SteamVR_LoadLevel.Begin("FantasyTemplate");
        */
        //SteamVR_LoadLevel.Begin("PhotoStage");
    }

    private void KeepOnLoad()
    {
        // Keep player and avatar objects
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
        if (playerGO == null || avatarGO == null)
        {
            logManager.DisplayInformationOnLog(3.0f, "The user or avatar has not been loaded.");
            throw new System.Exception("The player or avatar has not been loaded.");
        }
        DontDestroyOnLoad(playerGO);
        DontDestroyOnLoad(avatarGO);
    }

    public void LoadExperimentSelectionMenu()
    {
        experimentMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ExitGame()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }
}
