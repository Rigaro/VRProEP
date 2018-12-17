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
            EMGWiFiManager emgSensor = new EMGWiFiManager(ip, port, channelSize);
            AvatarSystem.AddActiveSensor(emgSensor);

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
    }


    public IEnumerator DisplayInformationOnLog(float time, string info)
    {
        string defaultText = logTMP.text;
        logTMP.text += info;
        yield return new WaitForSecondsRealtime(time);
        logTMP.text = defaultText;
    }
}
