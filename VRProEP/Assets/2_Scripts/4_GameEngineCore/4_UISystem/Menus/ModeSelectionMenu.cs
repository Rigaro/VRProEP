using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ModeSelectionMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject experimentSelectionMenu;
    public LogManager logManager;

    public void LoadPlayground()
    {
        KeepOnLoad();
        // Load level.
        SteamVR_LoadLevel.Begin("DemoPlayground");
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
        experimentSelectionMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
