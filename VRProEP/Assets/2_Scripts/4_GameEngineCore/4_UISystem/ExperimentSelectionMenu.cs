using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using Valve.VR;

public class ExperimentSelectionMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject modeSelectionMenu;
    public GameObject optionDropdownGO;
    public LogManager logManager;
    public Dropdown optionsDropdown;


    private const int NONE = 0;
    private const int JACOBIAN_SYNERGY = 1;

    private List<string> optionList = new List<string>();

    private int experimentNumber = 0;
    private int optionNumber = 0;

    public void SelectExperimentDropdown(int experimentNumber)
    {
        this.experimentNumber = experimentNumber;
        // Empty all options.
        optionList.Clear();
        optionsDropdown.ClearOptions();

        // Clear when no experiment selected
        if (experimentNumber == NONE)
        {
            optionNumber = 0;
            optionDropdownGO.SetActive(false);
            return;
        }
        else if (experimentNumber == JACOBIAN_SYNERGY)
        {
            // Add the options for the jacobian synergy.
            optionList.Add("");
            optionList.Add("Able-bodied");
            optionList.Add("EMG");
            optionList.Add("Synergy");
            optionsDropdown.AddOptions(optionList);
            // Activate dropdown
            optionDropdownGO.SetActive(true);
        }
    }

    public void SelectExperimentOption(int optionNumber)
    {
        this.optionNumber = optionNumber;
    }
    
    /// <summary>
    /// Checks that all the required components have been loaded and starts experiment.
    /// </summary>
    public void LaunchExperiment()
    {
        // Check that a valid experiment has been selected
        if (experimentNumber == NONE || optionNumber == NONE)
        {
            logManager.DisplayInformationOnLog(3.0f, "Please select a valid experiment.");
            return;
        }
        // Jacobian synergy experiment
        else if (experimentNumber == JACOBIAN_SYNERGY)
        {
            // Able-bodied case
            if (optionNumber == 1 && AvatarSystem.AvatarType == AvatarType.AbleBodied)
            {
                KeepOnLoad();
                // Load experiment.
                SteamVR_LoadLevel.Begin("JacobianSynergyExperiment");
            }
            // EMG case
            else if (optionNumber == 2 && AvatarSystem.AvatarType == AvatarType.Transhumeral)
            {
                // Check that an EMG sensor is available
                bool EMGAvailable = false;
                foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
                {
                    if (sensor.GetSensorType().Equals(SensorType.EMGWiFi))
                        EMGAvailable = true;
                }
                // Load when EMG is available.
                if (EMGAvailable)
                {
                    KeepOnLoad();
                    // Load experiment.
                    SteamVR_LoadLevel.Begin("JacobianSynergyExperiment");
                }
                else
                    logManager.DisplayInformationOnLog(3.0f, "Please add and configure an EMG sensor.");
            }
            // Synergy case
            else if (optionNumber == 3 && AvatarSystem.AvatarType == AvatarType.Transhumeral)
            {
                KeepOnLoad();
                // Load experiment.
                SteamVR_LoadLevel.Begin("JacobianSynergyExperiment");
            }
        }
    }

    public void ReturnToModeSelectionMenu()
    {
        modeSelectionMenu.SetActive(true);
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
            logManager.DisplayInformationOnLog(3.0f, "The user or avatar has not been loaded.");
            throw new System.Exception("The player or avatar has not been loaded.");
        }
        DontDestroyOnLoad(playerGO);
        DontDestroyOnLoad(avatarGO);
    }
}
