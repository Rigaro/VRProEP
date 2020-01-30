using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using System;
using System.IO;
using VRProEP.ExperimentCore;

using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.AdaptationCore;

public class DynamicExperimentSelectionMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject experimentConfigMenu;
    public LogManager logManager;
    public Dropdown experimentDropdown;
    private List<string> experimentList = new List<string>();
    private int selectedExperiment = 0;


    // Update available sensors when enabled menu.
    private void OnEnable()
    {
        // Get all available sensor names
        string[] availableExperiments = Directory.GetFiles(Application.dataPath + "/Resources/Experiments/", "*.prefab");
        // Clear list
        experimentList.Clear();
        // Add an empty one as default to force selection.
        experimentList.Add(string.Empty);

        // Add them to the sensor list
        foreach (string experiment in availableExperiments)
        {
            experimentList.Add(Path.GetFileNameWithoutExtension(experiment));
        }
        // Add the options to the dropdown
        experimentDropdown.AddOptions(experimentList);
    }

    public void UpdatedSelectedExperiment(int selectedExperiment)
    {
        this.selectedExperiment = selectedExperiment;
    }

    private void InitialiseExperiment()
    {
        SaveSystem.LoadUserData("DB1942174"); // Load the test/demo user (Mr Demo)
                                              //
                                              // Debug using able-bodied configuration
                                              //
        AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);
        if (selectedExperiment == 0)
        {
            logManager.DisplayInformationOnLog(5, "Please select an experiment from the dropdown menu.");
            return;
        }

        ExperimentSystem.SetActiveExperimentID(experimentList[selectedExperiment]);
    }

    public void LoadForestWorld()
    {
        // Initialise experiment data
        InitialiseExperiment();

        // Make sure we keep everything on load
        KeepOnLoad();

        // Load level
        SteamVR_LoadLevel.Begin("ForestWorld");
    }


    public void LoadSpaceWorld()
    {
        // Initialise experiment data
        InitialiseExperiment();

        // Make sure we keep everything on load
        KeepOnLoad();

        // Load level
        SteamVR_LoadLevel.Begin("SpaceWorld");
    }

    public void ReturnToExperimentConfigMenu()
    {
        experimentConfigMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    private void KeepOnLoad()
    {
        // Keep player and avatar objects
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
        if (playerGO == null || avatarGO == null)
        {
            logManager.DisplayInformationOnLog(5.0f, "The user or avatar has not been loaded.");
            throw new System.Exception("The player or avatar has not been loaded.");
        }
        DontDestroyOnLoad(playerGO);
        DontDestroyOnLoad(avatarGO);
    }
}
