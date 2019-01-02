﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRProEP.ProsthesisCore;

public class AddSensorMenu : MonoBehaviour {


    public GameObject settingsMenu;
    public Dropdown sensorDropdown;
    public GameObject EMGWiFiMenu;
    public TextMeshProUGUI logTMP;

    private List<string> sensorList = new List<string>();
    private int selectedSensor = 0;

    // Update available sensors when enabled menu.
    private void OnEnable()
    {
        // Get all available sensor names
        string[] availableSensors = Enum.GetNames(typeof(SensorType));
        // Clear list
        sensorList.Clear();
        // Add an empty one as default to force selection.
        sensorList.Add(string.Empty);

        // Add them to the sensor list
        foreach (string sensor in availableSensors)
        {
            sensorList.Add(sensor);
        }
        // Add the options to the dropdown
        sensorDropdown.AddOptions(sensorList);
        // And select the last choice.
        UpdatedSelectedSensor(selectedSensor);
    }

    /// <summary>
    /// Updates the selected sensor and enables menu.
    /// </summary>
    /// <param name="selectedSensor"></param>
    public void UpdatedSelectedSensor(int selectedSensor)
    {
        this.selectedSensor = selectedSensor;

        if (selectedSensor == ((int)SensorType.EMGWiFi + 1))
            EMGWiFiMenu.SetActive(true);
        else
        {
            EMGWiFiMenu.SetActive(false);
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
