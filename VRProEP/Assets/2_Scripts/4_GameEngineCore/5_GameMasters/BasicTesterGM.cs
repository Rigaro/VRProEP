using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.Utilities;

public class BasicTesterGM : MonoBehaviour
{
    public string ipAddress = "192.168.137.23";
    public int port = 2390;
    public bool sendData = false;
    public float frequency = 1000.0f;
    public float amplitude = 0.5f;
    public Transform cube;

    private UDPWriter writer;
    // Start is called before the first frame update
    void Start()
    {
        writer = new UDPWriter(ipAddress, port, "Boni");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (sendData)
        {
            float amp = amplitude*Mathf.Sin(3 * 2 * Mathf.PI * Time.time) + 0.5f;
            cube.position = new Vector3(cube.position.x, amp, cube.position.z);
            List<float> data = new List<float>();
            data.Add(frequency);
            data.Add(amp);
            writer.SendData(data);
        }
    }
}
