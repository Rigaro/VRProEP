using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRProEP.ProsthesisCore;

public class SettingsMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject addSensorMenu;
    public GameObject configSensorMenu;

    public TextMeshProUGUI sensorTMP;

    private ISensor sensorDebug;

    public void OnEnable()
    {
        MainMenu mainMenuBehavior = mainMenu.GetComponent<MainMenu>();
        // Display available sensors name.
        if (mainMenuBehavior.sensorList.Count > 0)
        {
            sensorTMP.text = "Sensors: \n";
            foreach (ISensor sensor in mainMenuBehavior.sensorList)
            {
                sensorDebug = sensor;
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
