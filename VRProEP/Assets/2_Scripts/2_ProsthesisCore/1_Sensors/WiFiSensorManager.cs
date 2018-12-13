//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

using System.Collections.Generic;

// WiFi UDP includes
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

// Threading includes
using System.Threading;

namespace VRProEP.ProsthesisCore
{
    public enum WiFiSensorType
    {
        UDP_Async,
        UDP_Sync
    }

    public abstract class WiFiSensorManager : ISensor, IConfigurable
    {
        // Generic sensor info
        private int channelSize;
        private SensorType sensorType;

        // WiFi sensor info
        private IPAddress ip;
        private int port;
        private WiFiSensorType wifiType;
        private string command = "1";

        // UDP data
        private struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;
        }
        private UdpState udpState;

        // Threading data
        private Thread thread;

        // Sensor data
        protected List<float> sensorValues;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="sensorType"></param>
        public WiFiSensorManager(string ipAddress, int port, SensorType sensorType)
        {
            if (channelSize <= 0)
                throw new System.ArgumentException("The given channel size is invalid. It should be greater than zero.");
            channelSize = 1;
            this.sensorType = sensorType;

            // Set WiFi data
            wifiType = WiFiSensorType.UDP_Async;
            ip = IPAddress.Parse(ipAddress);
            this.port = port;
            sensorValues = new List<float>(1);

            // Connect
            EstablishConnection();

            // Create and start communication thread
            thread = new Thread(new ThreadStart(GetDataFromSensor));
            thread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="channelSize"></param>
        /// <param name="sensorType"></param>
        /// <param name="wifiType"></param>
        public WiFiSensorManager(string ipAddress, int port, int channelSize, SensorType sensorType, WiFiSensorType wifiType)
        {
            // Set sensor data
            if (channelSize <= 0)
                throw new System.ArgumentException("The given channel size is invalid. It should be greater than zero.");
            this.channelSize = channelSize;
            this.sensorType = sensorType;

            // Set WiFi data
            this.wifiType = wifiType;
            ip = IPAddress.Parse(ipAddress);
            this.port = port;
            sensorValues = new List<float>(channelSize);

            // Connect
            EstablishConnection();

            // Create and start communication thread
            thread = new Thread(new ThreadStart(GetDataFromSensor));
            thread.Start();
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
        }

        /// <summary>
        /// Sends the current command to the sensor to retrieve data.
        /// </summary>
        private void GetDataFromSensor()
        {
            // Send a request for data to sensor.
            Byte[] sendBytes = Encoding.ASCII.GetBytes(command);
            udpState.u.Send(sendBytes, sendBytes.Length);

            // Get data from sensor when available
            if (udpState.u.Available > 0)
            {
                // Asynchronous type
                if (wifiType == WiFiSensorType.UDP_Async)
                {
                    udpState.u.BeginReceive(new AsyncCallback(ReceiveDataCallback), udpState);

                }
                // Synchronous type (can block)
                else
                {
                    Byte[] receivedBytes = udpState.u.Receive(ref udpState.e);
                    ProcessDataReceived(receivedBytes);
                }
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
            ProcessDataReceived(receivedBytes);
        }

        /// <summary>
        /// Function that processes the received data.
        /// </summary>
        /// <param name="result">The received byte.</param>
        private void ProcessDataReceived(Byte[] receivedBytes)
        {
            // Decode data string.
            string valuesString = Encoding.ASCII.GetString(receivedBytes);

            // Split the multiple channels.
            string[] values = valuesString.Split('%');
            if (values.Length != ChannelSize)
                throw new System.Exception("Channel splitting failed. The received data is: " + valuesString);

            // Update the sensor values
            int i = 0;
            foreach (string value in values)
            {
                sensorValues[i] = float.Parse(value);
                i++;
            }
        }

        /// <summary>
        /// Sets the command to be sent to the sensor.
        /// </summary>
        /// <param name="command">The command string to be sent to the sensor.</param>
        protected void SetCommand(string command)
        {
            this.command = command;
        }

        /// <summary>
        /// Gets the current sensor values.
        /// </summary>
        /// <returns>The list with the sensor values.</returns>
        protected List<float> GetCurrentSensorValues()
        {
            List<float> returnValues = new List<float>(sensorValues); // Avoid leakage
            return returnValues;
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

        public SensorType GetSensorType()
        {
            return sensorType;
        }
    }

}

