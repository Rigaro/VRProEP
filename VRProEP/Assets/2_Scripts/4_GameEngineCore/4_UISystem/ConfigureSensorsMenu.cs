using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRProEP.ProsthesisCore;
using VRProEP.GameEngineCore;

public class ConfigureSensorsMenu : MonoBehaviour {

    public MainMenu mainMenu;
    public GameObject settingsMenu;
    public Dropdown sensorDropdown;
    public GameObject EMGWiFiConfig;
    public TextMeshProUGUI logTMP;

    private int selectedSensor = 0;
    private List<string> sensorList = new List<string>();

    public void OnEnable()
    {
        // Clear list
        sensorList.Clear();
        // Add an empty one as default to force selection.
        sensorList.Add(string.Empty);

        // Display available sensors name.
        foreach (ISensor sensor in AvatarSystem.GetAvailableSensors())
        {
            sensorList.Add(sensor.GetSensorType().ToString());
        }
        // Add the options to the dropdown
        sensorDropdown.AddOptions(sensorList);
        // And select the last choice.
        UpdatedSelectedSensor(selectedSensor);
    }

    /// <summary>
    /// Handles when the sensor selection has been updated.
    /// </summary>
    /// <param name="selectedSensor"></param>
    public void UpdatedSelectedSensor(int selectedSensor)
    {
        this.selectedSensor = selectedSensor;
        ISensor sensor;
        // If selected a sensor extract it.
        if (selectedSensor > 0)
            sensor = AvatarSystem.GetAvailableSensors()[selectedSensor - 1];
        else
        {
            // Deactivate all
            EMGWiFiConfig.SetActive(false);
            return;
        }

        // Select the sensor config menu.
        if (sensor.GetSensorType() == SensorType.EMGWiFi)
        {
            EMGWiFiConfig.SetActive(true);
            EMGWiFiConfig.GetComponent<ConfigEMGWiFi>().SetSensorToConfigure((EMGWiFiManager)sensor);
        }
    }

    public void ReturnToSettingsMenu()
    {
        // Clear dropdown
        sensorDropdown.ClearOptions();
        // Clear log
        logTMP.text = "Log: ";
        // Return to main menu
        settingsMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
