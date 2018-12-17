using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Valve.VR;

using VRProEP.ProsthesisCore;
using VRProEP.ExperimentCore;

public class AddVIVETrackerSensor : MonoBehaviour {

    public Dropdown deviceDropdown;
    public TextMeshProUGUI logTMP;
    public AddExperimentSensorMenu addExperimentSensorMenu;

    private List<string> deviceList = new List<string>();
    private SteamVR_TrackedObject.EIndex selectedDevice = 0;

    // Update available devices when enabled menu.
    private void OnEnable()
    {
        // Get all available sensor names
        string[] availableDevices = Enum.GetNames(typeof(SteamVR_TrackedObject.EIndex));
        // Clear list
        deviceList.Clear();

        // Add them to the sensor list
        foreach (string device in availableDevices)
        {
            deviceList.Add(device);
        }
        // Add the options to the dropdown
        deviceDropdown.AddOptions(deviceList);
        // And select the last choice.
        UpdatedSelectedDevice((int)selectedDevice);
    }


    /// <summary>
    /// Updates the selected device.
    /// </summary>
    /// <param name="selectedDevice"></param>
    public void UpdatedSelectedDevice(int selectedDevice)
    {
        this.selectedDevice = (SteamVR_TrackedObject.EIndex)selectedDevice;
    }

    public void AddVIVETracker()
    {
        if ((int)selectedDevice > 5)
        {
            // Instantiate a new VIVETracker with Player as parent.
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null)
                throw new Exception("The player GameObject was not found.");

            // Load VIVE Tracker prefab.
            GameObject viveTrackerPrefab = Resources.Load<GameObject>("Trackers/VIVETracker");
            if (viveTrackerPrefab == null)
                throw new Exception("The requested tracker prefab was not found.");

            // Instantiate and with the other SteamVRObjects and set device number
            GameObject viveTrackerGO = Instantiate(viveTrackerPrefab, viveTrackerPrefab.transform.position, viveTrackerPrefab.transform.rotation, playerGO.transform.GetChild(0).transform);
            viveTrackerGO.GetComponentInChildren<SteamVR_TrackedObject>().SetDeviceIndex((int)selectedDevice);

            // Create tracker sensor and add to logger
            VIVETrackerManager viveTrackerSensor = new VIVETrackerManager(viveTrackerGO.transform);
            ExperimentSystem.AddSensor(viveTrackerSensor);

            addExperimentSensorMenu.ReturnToSettingsMenu();
        }
        else
        {
            StartCoroutine(DisplayInformationOnLog(3.0f, "Select a valid VIVE Tracker device."));
        }
    }

    public IEnumerator DisplayInformationOnLog(float time, string info)
    {
        string defaultText = logTMP.text;
        logTMP.text += info;
        yield return new WaitForSecondsRealtime(time);
        logTMP.text = defaultText;
    }
}
