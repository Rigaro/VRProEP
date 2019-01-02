using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.ExperimentCore;
using Valve.VR;

public class MainMenu : MonoBehaviour {

    public GameObject userOptionsMenu;
    public GameObject avatarOptionsButton;
    public GameObject avatarOptionsMenu;
    public GameObject sensorOptionsButton;
    public GameObject sensorOptionsMenu;
    public GameObject modeSelectionButton;
    public GameObject modeSelectionMenu;
    public TextMeshProUGUI logTMP;
    public TextMeshProUGUI activeUserTMP;
    public TextMeshProUGUI sensorTMP;
    public TextMeshProUGUI experimentSensorsTMP;
    public bool createdUser = false;
    public bool loadedUser = false;

    /*
    public GameObject playerAblePrefab;
    public GameObject playerAbleTHPrefab;
    public GameObject playerAbleTRPrefab;
    public GameObject playerTHPrefab;
    public GameObject playerTRPrefab;
    public GameObject avatarObject;
    */
    private GameObject playerGO;
    
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
        if (SaveSystem.IsUserAvailable)
        {
            activeUserTMP.text = "Active User: \n" + SaveSystem.ActiveUser.name + " " + SaveSystem.ActiveUser.familyName;
        }

        // Display available user sensors name.
        if (AvatarSystem.GetActiveSensors().Count > 0)
        {
            sensorTMP.text = "Sensors: \n";
            foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
            {
                sensorTMP.text = sensorTMP.text + sensor.GetSensorType().ToString() + "\n";
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

        // Conditional menus
        // Show avatar menu when there is an available user.
        if (SaveSystem.IsUserAvailable)
            avatarOptionsButton.SetActive(true);

        // Show sensors menu when there is an available avatar.
        if (AvatarSystem.IsPlayerAvailable && AvatarSystem.IsAvatarAvaiable)
        {
            sensorOptionsButton.SetActive(true);
            modeSelectionButton.SetActive(true);
        }

    }

    public void OnDestroy()
    {
        foreach (ISensor sensor in AvatarSystem.GetActiveSensors())
        {
            // Stop all wifi sensors
            if (sensor.GetSensorType() == SensorType.EMGWiFi)
            {
                WiFiSensorManager wifiSensor = (WiFiSensorManager)sensor;
                wifiSensor.StopSensorReading();
            }
        }
    }

    public void LoadUserOptionsMenu()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: \n";
        // Switch
        userOptionsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadAvatarOptionsMenu()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: \n";
        // Switch
        avatarOptionsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadSensorOptionsMenu()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: \n";
        // Switch
        sensorOptionsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadModeSelectionMenu()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: \n";
        // Switch
        modeSelectionMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public IEnumerator DisplayInformationOnLog(float time, string info)
    {
        string defaultText = logTMP.text;
        logTMP.text += info;
        yield return new WaitForSecondsRealtime(time);
        logTMP.text = defaultText;
    }
}
