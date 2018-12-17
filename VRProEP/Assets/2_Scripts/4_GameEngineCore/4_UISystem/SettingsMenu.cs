using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRProEP.ProsthesisCore;
using VRProEP.GameEngineCore;

public class SettingsMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject addSensorMenu;
    public GameObject configSensorMenu;

    public TextMeshProUGUI sensorTMP;

    //private ISensor sensorDebug;

    public void OnEnable()
    {
        // Display available sensors name.
        if (AvatarSystem.GetAvailableSensors().Count > 0)
        {
            sensorTMP.text = "Sensors: \n";
            foreach (ISensor sensor in AvatarSystem.GetAvailableSensors())
            {
                sensorTMP.text = sensorTMP.text + sensor.GetSensorType().ToString() + "\n";
            }

        }

    }

    public void AddSensor()
    {
        addSensorMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ConfigureSensor()
    {
        configSensorMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
