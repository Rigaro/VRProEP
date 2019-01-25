using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Sockets;

using VRProEP.ProsthesisCore;
using VRProEP.GameEngineCore;

public class AddEMGWiFiSensor : MonoBehaviour {

    private string ip;
    private int port = 0;
    private int channelSize = 0;
    private bool isRaw = false;

    public TextMeshProUGUI logTMP;
    public AddSensorMenu addSensorMenu;

    public void UpdateIP(string ip)
    {
        this.ip = ip;
    }

    public void UpdatePort(string port)
    {
        this.port = int.Parse(port);
    }

    public void UpdateChannelSize(string channelSize)
    {
        this.channelSize = int.Parse(channelSize);
    }

    public void UpdateIsRaw(bool isRaw)
    {
        this.isRaw = isRaw;
    }

    /// <summary>
    /// Add a new EMG sensor
    /// </summary>
    public void AddSensor()
    {
        if (ip == null || ip == string.Empty || port == 0 || channelSize <= 0)
            StartCoroutine(DisplayInformationOnLog(3.0f, "The provided sensor info is invalid."));

        try
        {
            // Create and add sensor
            EMGWiFiManager emgSensor = new EMGWiFiManager(ip, port, channelSize, isRaw);
            AvatarSystem.AddActiveSensor(emgSensor);

            // Get prosthesis and add sensor
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            ConfigurableElbowManager elbowManager = prosthesisManagerGO.GetComponent<ConfigurableElbowManager>();
            elbowManager.AddSensor(emgSensor);
            
            // Return to settings menu.
            addSensorMenu.ReturnToSettingsMenu();
        }
        // 10048 address duplicate
        catch (SocketException e) when (e.ErrorCode == 10048)
        {
            StartCoroutine(DisplayInformationOnLog(3.0f, "IP address already in use."));

        }
        // 10045 port duplicate
        catch (SocketException e) when (e.ErrorCode == 10045)
        {
            StartCoroutine(DisplayInformationOnLog(3.0f, "Port already in use."));

        }
        catch (System.Exception e)
        {
            StartCoroutine(DisplayInformationOnLog(3.0f, "An error occured while adding the sensor.\nError message: " + e.Message));
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
