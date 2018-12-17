using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using Valve.VR;

public class MainMenu : MonoBehaviour {

    public GameObject createNewUserMenu;
    public GameObject loadUserMenu;
    public GameObject settingsMenu;
    public TextMeshProUGUI logTMP;
    public TextMeshProUGUI activeUserTMP;
    public TextMeshProUGUI sensorTMP;
    public bool createdUser = false;
    public bool loadedUser = false;
       
    public void OnEnable()
    {
        if (createdUser)
        {
            StartCoroutine(DisplayInformationOnLog(3.0f, "Created new user with ID " + SaveSystem.ActiveUser.id));
            createdUser = false;
        }

        if (loadedUser)
        {
            StartCoroutine(DisplayInformationOnLog(3.0f, "Loaded user with ID " + SaveSystem.ActiveUser.id));
            createdUser = false;
        }

        // Display active user name.
        if (SaveSystem.ActiveUser != null)
            activeUserTMP.text = "Active User: " + SaveSystem.ActiveUser.name + " " + SaveSystem.ActiveUser.familyName;

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

    public void OnDestroy()
    {
        foreach (ISensor sensor in AvatarSystem.GetAvailableSensors())
        {
            // Stop all wifi sensors
            if (sensor.GetSensorType() == SensorType.EMGWiFi)
            {
                WiFiSensorManager wifiSensor = (WiFiSensorManager)sensor;
                wifiSensor.StopSensorReading();
            }
        }
    }

    public void CreateNewUser()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: ";
        // Switch
        createNewUserMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadUser()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: ";
        // Switch
        loadUserMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    public void SettingsMenu()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: ";
        // Switch
        settingsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadPlayground()
    {
        SteamVR_LoadLevel.Begin("DemoPlayground");
    }
    
    public IEnumerator DisplayInformationOnLog(float time, string info)
    {
        string defaultText = logTMP.text;
        logTMP.text += info;
        yield return new WaitForSecondsRealtime(time);
        logTMP.text = defaultText;
    }
}
