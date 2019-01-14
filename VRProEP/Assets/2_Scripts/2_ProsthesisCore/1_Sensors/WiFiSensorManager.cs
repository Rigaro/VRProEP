//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;

// WiFi UDP includes
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

// Threading includes
using System.Threading;

// Debug
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public enum UDPType
    {
        UDP_Async,
        UDP_Sync
    }

    /// <summary>
    /// Manager for WiFi sensors implemented with UDP protocol.
    /// </summary>
    public abstract class WiFiSensorManager : ISensor, IConfigurable
    {
        // Generic sensor info
        private int channelSize;
        private SensorType sensorType;

        // WiFi sensor info
        private IPAddress ip;
        private int port;
        private UDPType udpType;

        // UDP data
        private struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;
        }
        private UdpState udpState;

        // Threading data
        private Thread thread;
        private bool runThread = true;

        // Sensor data
        private List<float> sensorValues;

        // Commands and acknowledges
        private string command = CONFIGURE;
        private const string CONFIGURE = "config";
        private const string READ = "read";
        private const string ACKNOWLEDGE_CONFIGURE = "ack_config";
        private const string ACKNOWLEDGE_CHANNEL = "ack_chan";

        /// <summary>
        /// Manager for WiFi sensors implemented with UDP protocol.
        /// </summary>
        /// <param name="ipAddress">The IP address of the sensor to connect to.</param>
        /// <param name="port">The UDP port to use for data transfer.</param>
        /// <param name="sensorType">The type of sensor.</param>
        public WiFiSensorManager(string ipAddress, int port, SensorType sensorType)
        {
            channelSize = 1;
            this.sensorType = sensorType;

            // Set WiFi data
            udpType = UDPType.UDP_Async;
            ip = IPAddress.Parse(ipAddress);
            this.port = port;
            sensorValues = new List<float>(channelSize);
            sensorValues.Add(0.0f);

            // Connect
            EstablishConnection();

            // Create and start communication thread
            thread = new Thread(new ThreadStart(GetDataFromSensor));
            thread.Start();            
        }

        /// <summary>
        /// Manager for WiFi sensors implemented with UDP protocol.
        /// </summary>
        /// <param name="ipAddress">The IP address of the sensor to connect to.</param>
        /// <param name="port">The UDP port to use for data transfer.</param>
        /// <param name="channelSize">The number of sensor channels available.</param>
        /// <param name="sensorType">The type of sensor.</param>
        /// <param name="udpType">The type of UDP connection to use (Asynchronous or Synchronous).</param>
        public WiFiSensorManager(string ipAddress, int port, int channelSize, SensorType sensorType, UDPType udpType)
        {
            // Set sensor data
            if (channelSize <= 0)
                throw new System.ArgumentException("The given channel size is invalid. It should be greater than zero.");
            this.channelSize = channelSize;
            this.sensorType = sensorType;

            // Set WiFi data
            this.udpType = udpType;
            ip = IPAddress.Parse(ipAddress);
            this.port = port;
            sensorValues = new List<float>(channelSize);

            // init sensor values
            for (int i = 0; i< channelSize; i++)
            {
                sensorValues.Add(0.0f);
            }

            // Connect
            EstablishConnection();

            // Create and start communication thread
            thread = new Thread(new ThreadStart(GetDataFromSensor));
            thread.Start();
        }

        /// <summary>
        /// Stop the thread when destroying and close the UDP port.
        /// </summary>
        ~WiFiSensorManager()
        {
            runThread = false;
            udpState.u.Close();
        }

        /// <summary>
        /// Establishes a connection with the remote sensor.
        /// </summary>
        private void EstablishConnection()
        {
            // Create a receive UDP end point with sensor configuration.
            IPEndPoint remoteIpEndPoint = new IPEndPoint(ip, port);
            // Create a UDP client with sensor configuration.
            UdpClient udpClient = new UdpClient(port);
            
            // IPEndPoint object will allow us to read datagrams sent from any source.

            udpState = new UdpState();
            udpState.e = remoteIpEndPoint;
            udpState.u = udpClient;

            // Connect to sensor
            udpState.u.Connect(ip, port);
            Debug.Log( sensorType + " sensor connection established.");
        }

        /// <summary>
        /// Sends the current command to the sensor to retrieve data.
        /// </summary>
        private void GetDataFromSensor()
        {
            // Continuously read from sensor while active.
            while(runThread)
            {
                //Debug.Log(runThread.ToString());
                // Send a request for data to sensor.
                Byte[] sendBytes = Encoding.ASCII.GetBytes(command);
                udpState.u.Send(sendBytes, sendBytes.Length);

                // Get data from sensor when available
                if (udpState.u.Available > 0)
                {
                    // Asynchronous type
                    if (udpType == UDPType.UDP_Async)
                    {
                        udpState.u.BeginReceive(new AsyncCallback(ReceiveDataCallback), udpState);

                    }
                    // Synchronous type (can block)
                    else
                    {
                        Byte[] receivedBytes = udpState.u.Receive(ref udpState.e);
                        ProcessReceivedData(receivedBytes);
                    }
                }
                // Sleep for 50ms.
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Callback that processes the recived data from an asynchronous data request through the UDP client.
        /// </summary>
        /// <param name="result">The asynchronous result.</param>
        private void ReceiveDataCallback(IAsyncResult result)
        {
            // Receive and process data string.
            Byte[] receivedBytes = udpState.u.EndReceive(result, ref udpState.e);
            ProcessReceivedData(receivedBytes);
        }

        /// <summary>
        /// Function that processes the received data.
        /// </summary>
        /// <param name="result">The received byte.</param>
        private void ProcessReceivedData(Byte[] receivedBytes)
        {
            // Decode data string.
            string receivedString = Encoding.ASCII.GetString(receivedBytes);
            
            // Wait for configure acknowledgement
            if (command == CONFIGURE && receivedString == ACKNOWLEDGE_CONFIGURE)
            {
                command = channelSize.ToString(); // Set channel size;
            }
            // When channel has been set, start reading
            else if (receivedString.Equals(ACKNOWLEDGE_CHANNEL))
            {
                command = READ; // Start reading
            }
            else if (command.Equals(READ))
            {
                // Split the multiple channels.
                string[] values = receivedString.Split('%');
                if (values.Length != channelSize)
                    throw new Exception("Channel splitting failed. The received data is: " + receivedString);

                // Update the sensor values and parse as float.
                int i = 0;
                foreach (string value in values)
                {
                    sensorValues[i] = float.Parse(value);
                    i++;
                }
                // Debug.Log(sensorValues[1]);
            }
        }
        
        /// <summary>
        /// Returns the current sensor values.
        /// </summary>
        /// <returns>The list with the sensor values.</returns>
        protected float[] GetCurrentSensorValues()
        {
            List<float> returnValues = new List<float>(sensorValues);
            return returnValues.ToArray();
        }

        /// <summary>
        /// Re-starts sensor readings if previously stopped.
        /// </summary>
        public void StartSensorReading()
        {
            if (runThread == false)
            {
                Debug.Log( sensorType + " data gathering started.");
                Debug.Log(ip + " " + port);
                runThread = true;
            }
            else
                throw new Exception("The sensor is already running.");
        }

        /// <summary>
        /// Terminates the sensor reading thread. Should be called when not using sensor anymore.
        /// </summary>
        public void StopSensorReading()
        {
            runThread = false;
        }
               
        /// <summary>
        /// Returns raw sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        public abstract float GetRawData(int channel);

        /// <summary>
        /// Returns raw sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        public abstract float GetRawData(string channel);

        /// <summary>
        /// Returns all raw sensor data in an array.
        /// </summary>
        /// <returns>The array with all raw sensor data.</returns>
        public abstract float[] GetAllRawData();

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public abstract float GetProcessedData(int channel);

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public abstract float GetProcessedData(string channel);

        /// <summary>
        /// Returns all pre-processed sensor data in an array.
        /// </summary>
        /// <returns>The array with all pre-processed sensor data.</returns>
        public abstract float[] GetAllProcessedData();

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public abstract void Configure(string command, dynamic value);

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public abstract void Configure(string command, string value);


        // Encapsulation
        public int ChannelSize
        {
            get
            {
                return channelSize;
            }
        }

        public bool RunThread { get => runThread; }

        public SensorType GetSensorType()
        {
            return sensorType;
        }
    }

}

