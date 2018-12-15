using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRProEP.ProsthesisCore;

public class ConfigureSensorsMenu : MonoBehaviour {

    public MainMenu mainMenu;
    public GameObject settingsMenu;
    public Dropdown sensorDropdown;
    public GameObject EMGWiFiConfig;

    private int selectedSensor = 0;
    private List<string> sensorList = new List<string>();

    public void OnEnable()
    {
        // Clear list
        sensorList.Clear();
        // Add an empty one as default to force selection.
        sensorList.Add(string.Empty);

        // Display available sensors name.
        foreach (ISensor sensor in mainMenu.sensorList)
        {
            sensorList.Add(sensor.GetSensorType().ToString());
        }
        // Add the options to the dropdown
        sensorDropdown.AddOptions(sensorList);
    }

    public void UpdatedSelectedSensor(int selectedSensor)
    {
        this.selectedSensor = selectedSensor;

        if (mainMenu.sensorList[selectedSensor - 1].GetSensorType() == SensorType.EMGWiFi)
        {
            EMGWiFiConfig.SetActive(true);
            EMGWiFiConfig.GetComponent<ConfigEMGWiFi>().SetSensorToConfigure((EMGWiFiManager)mainMenu.sensorList[selectedSensor - 1]);
        }
        else
        {
            EMGWiFiConfig.SetActive(false);
        }
    }

    public void ReturnToSettingsMenu()
    {
        // Clear dropdown
        sensorDropdown.ClearOptions();
        // Return to main menu
        settingsMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
