using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using VRProEP.ProsthesisCore;
using VRProEP.GameEngineCore;

public class AddThalmicMyoSensor : MonoBehaviour
{
    public TextMeshProUGUI logTMP;
    public AddSensorMenu addSensorMenu;

    /// <summary>
    /// Add a new EMG sensor
    /// </summary>
    public void AddSensor()
    {
        // Create and add sensor
        ThalmicMyobandManager emgSensor = new ThalmicMyobandManager();
        AvatarSystem.AddActiveSensor(emgSensor);

        // Get prosthesis and add sensor when tH
        if (AvatarSystem.AvatarType == AvatarType.Transhumeral)
        {
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            ConfigurableElbowManager elbowManager = prosthesisManagerGO.GetComponent<ConfigurableElbowManager>();
            elbowManager.AddSensor(emgSensor);
        }

        else if (AvatarSystem.AvatarType == AvatarType.Transradial)
        {
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            FakeEMGBoniHand prosthesisManager = prosthesisManagerGO.GetComponent<FakeEMGBoniHand>();
            prosthesisManager.InitialiseInputSystem(emgSensor);
        }

        // Return to settings menu.
        addSensorMenu.ReturnToSettingsMenu();
    }


    public IEnumerator DisplayInformationOnLog(float time, string info)
    {
        string defaultText = logTMP.text;
        logTMP.text += info;
        yield return new WaitForSecondsRealtime(time);
        logTMP.text = defaultText;
    }
}
