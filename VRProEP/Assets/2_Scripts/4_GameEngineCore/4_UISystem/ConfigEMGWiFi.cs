using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRProEP.ProsthesisCore;
using TMPro;

public class ConfigEMGWiFi : MonoBehaviour
{

    public GameObject sliderPrefab;

    private EMGWiFiManager sensor;
    private List<GameObject> sliders = new List<GameObject>();
    private List<float> maxValues = new List<float>();
    private List<float> minValues = new List<float>();

    private bool raw = true;

    /// <summary>
    /// Sets the sensor that is going to be configured and initializes parameters.
    /// </summary>
    /// <param name="sensor">The EMG WiFi sensor to be configured.</param>
    public void SetSensorToConfigure(EMGWiFiManager sensor)
    {
        for (int i = 0; i < sensor.ChannelSize; i++)
        {
            // Create sliders and initialize configuration parameters
            GameObject slider = Instantiate(sliderPrefab, transform.position + sliderPrefab.transform.position + new Vector3(i * 200.0f, 0.0f, 0.0f), sliderPrefab.transform.rotation, transform);
            sliders.Add(slider);
            maxValues.Add(0.0f);
            minValues.Add(1023.0f);
        }
        this.sensor = sensor;
    }

    public void Update()
    {
        // Check that sensors and sliders are available.
        if (sensor != null && sliders.Count == sensor.ChannelSize)
        {
            // Update all sensors
            for (int i = 0; i < sensor.ChannelSize; i++)
            {
                float sensorData;
                // If raw data, update max and min values.
                if (raw)
                {
                    sensorData = sensor.GetRawData(i + 1);

                    if (sensorData > maxValues[i])
                        maxValues[i] = sensorData;
                    if (sensorData < minValues[i])
                        minValues[i] = sensorData;
                }
                else
                    sensorData = Mathf.Round(sensor.GetProcessedData(i + 1));

                sliders[i].GetComponent<Slider>().value = sensorData;

                // Display data.
                sliders[i].GetComponentInChildren<TextMeshProUGUI>().text = "Val: " + sensorData + "\nMin: " + minValues[i] + "\nMax: " + maxValues[i];
            }
        }
    }

    public void ConfigureSensors()
    {
        // Check that sensors and sliders are available.
        if (sensor != null && sliders.Count == sensor.ChannelSize)
        {
            // Configure all sensor channels
            for (int i = 0; i < sensor.ChannelSize; i++)
            {
                sensor.ConfigureLimits(i + 1, (int)maxValues[i], (int)minValues[i]);
            }
        }
    }

    /// <summary>
    /// Selects the type of sensor data to display: raw or processed.
    /// </summary>
    /// <param name="raw">true if raw data.</param>
    public void SelectValueType(bool raw)
    {
        this.raw = raw;
        // Check sensors and sliders are available.
        if (sensor != null && sliders.Count == sensor.ChannelSize)
        {
            // Change the max value of the slider accordingly.
            if (raw)
            {
                for (int i = 0; i < sensor.ChannelSize; i++)
                {
                    sliders[i].GetComponent<Slider>().maxValue = 1023;
                }

            }
            else
            {
                for (int i = 0; i < sensor.ChannelSize; i++)
                {
                    sliders[i].GetComponent<Slider>().maxValue = 100;
                }
            }

        }
    }
}
