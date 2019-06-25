using System.Collections;
using System.Collections.Generic;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using UnityEngine;

public class UDPTester : MonoBehaviour {

    public string IP = "192.168.137.88";
    public int port = 2390;
    public bool request = false;
    public Transform guiTransform;

    private float[] sensorValues = { 0.0f, 0.0f, 0.0f };

    private struct UdpState
    {
        public UdpClient u;
        public IPEndPoint e;
    }

    private UdpState udpState;

    private string receivedString;

    private Thread thread;
    private bool runThread = true;
    private string command = "config";

    // Commands and acknowledges
    private const string CONFIGURE = "config";
    private const string READ = "read";
    private const string ACKNOWLEDGE_CONFIGURE = "ack_config";
    private const string ACKNOWLEDGE_CHANNEL = "ack_chan";

    // Turn into constructor
    void Start ()
    {
        // Create a receive UDP end point with sensor configuration.
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        // Create a UDP client with sensor configuration.
        UdpClient udpClient = new UdpClient(port);

        // IPEndPoint object will allow us to read datagrams sent from any source.

        udpState = new UdpState();
        udpState.e = remoteIpEndPoint;
        udpState.u = udpClient;

        // Connect to sensor
        udpState.u.Connect(IPAddress.Parse(IP), port);
        // Send a request for initialize to sensor.

        command = CONFIGURE;

        thread = new Thread(new ThreadStart(GetDataFromSensor));
        thread.Start();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
            //Debug.Log("Sensor data: " + sensorValues[0] + ", " + sensorValues[1] + ", " + sensorValues[2]);
            // guiTransform.position = new Vector3(0.01f * sensorValues[1], 0.01f * sensorValues[0], guiTransform.position.z);
    }

    private void GetDataFromSensor()
    {
        while(runThread)
        {
            try
            {
                //Debug.Log("Command: " + command);
                // Send a request for data to sensor.
                Byte[] sendBytes = Encoding.ASCII.GetBytes(command);
                udpState.u.Send(sendBytes, sendBytes.Length);

                // Get data from sensor when available
                if (udpState.u.Available > 0)
                {
                    udpState.u.BeginReceive(new AsyncCallback(ProcessDataReceived), udpState);
                    //Byte[] receivedBytes = udpState.u.Receive(ref udpState.e);
                    //ProcessDataReceived(receivedBytes);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            Thread.Sleep(50);
        }
    }

    
    private void ProcessDataReceived(IAsyncResult result)
    {
        Byte[] receivedBytes = udpState.u.EndReceive(result, ref udpState.e);
        receivedString = Encoding.ASCII.GetString(receivedBytes);
        //Debug.Log("I received: " + receivedString);

        if (command == CONFIGURE && receivedString == ACKNOWLEDGE_CONFIGURE)
        {
            command = "3"; // Set channel size;
        }
        else if(receivedString.Equals(ACKNOWLEDGE_CHANNEL))
        {
            command = READ; // Start reading
        }
        else if (command.Equals(READ))
        {
            string[] values = receivedString.Split('%');

            int i = 0;
            foreach (string value in values)
            {
                sensorValues[i] = float.Parse(value);
                i++;
            }
            Debug.Log("Sensor data: " + sensorValues[0] + ", " + sensorValues[1] + ", " + sensorValues[2]);
        }
        //Debug.Log("rs" + sensorValue);
    }
    
    
    private void ProcessDataReceived(Byte[] receiveBytes)
    {
        //Byte[] receiveBytes = udpState.u.Receive(ref udpState.e);
        receivedString = Encoding.ASCII.GetString(receiveBytes);
        Debug.Log("I received: " + receivedString);

        //Debug.Log(receiveString);
    }
    
    void OnApplicationQuit()
    {
        runThread = false;
        try
        {
            udpState.u.Close();
        }
        catch
        {
            // Do nothing because no need to close it.
        }
    }

}
