using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRProEP.ProsthesisCore;
using VRProEP.GameEngineCore;
using VRProEP.ExperimentCore;

public class SensorOptionsMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject addUserSensorMenu;
    public GameObject configUserSensorMenu;
    public GameObject addExperimentSensorMenu;

    public TextMeshProUGUI userSensorsTMP;
    public TextMeshProUGUI experimentSensorsTMP;

    //private ISensor sensorDebug;

    public void OnEnable()
    {
        // Display available user sensors name.
        if (AvatarSystem.GetActiveSensors().Count > 0)
        {
            userSensorsTMP.text = "User Sensors: \n";
            foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
            {
                userSensorsTMP.text = userSensorsTMP.text + sensor.GetSensorType().ToString() + "\n";
            }

        }

        // Display available experiment sensors name.
        if (ExperimentSystem.GetActiveSensors().Count > 0)
        {
            experimentSensorsTMP.text = "Experiment Sensors: \n";
            foreach (ISensor sensor in ExperimentSystem.GetActiveSensors())
            {
                experimentSensorsTMP.text = experimentSensorsTMP.text + sensor.GetSensorType().ToString() + "\n";
            }

        }

    }

    public void AddUserSensor()
    {
        addUserSensorMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ConfigureUserSensor()
    {
        configUserSensorMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void AddExperimentSensor()
    {
        addExperimentSensorMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
