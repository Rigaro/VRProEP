
// WiFi UDP includes
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

// Threading includes
using System.Threading;

// Debug
using UnityEngine;

namespace VRProEP.Utilities
{
    public enum UDPDeviceType { Writer, Listener, Dual };

    public abstract class UDPDeviceManager : ICommunicationsManager
    {

        // Generic sensor info
        private string deviceName;
        private UDPDeviceType deviceType;

        // WiFi sensor info
        private IPAddress ip;
        private int port;
        private UDPType udpType;

        // Threading data
        private Thread thread;
        private bool runThread = true;

        // UDP data
        private struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;
        }
        private UdpState udpState;


        public UDPDeviceManager(string ipAddress, int port, string deviceName, UDPDeviceType deviceType)
        {
            this.deviceName = deviceName;
            this.deviceType = deviceType;

            // Set UDP data
            udpType = UDPType.UDP_Async;
            ip = IPAddress.Parse(ipAddress);
            this.port = port;

            // Connect
            EstablishConnection();

            // Start listening when listener type
            if (deviceType == UDPDeviceType.Listener || deviceType == UDPDeviceType.Dual)
            {
                // Create and start communication thread
                thread = new Thread(new ThreadStart(ListenForData));
                thread.Start();
            }
        }


        public UDPDeviceManager(string ipAddress, int port, string deviceName, UDPDeviceType deviceType, UDPType udpType)
        {
            this.deviceName = deviceName;
            this.deviceType = deviceType;

            // Set UDP data
            this.udpType = udpType;
            ip = IPAddress.Parse(ipAddress);
            this.port = port;

            // Connect
            EstablishConnection();

            // Start listening when listener type
            if (deviceType == UDPDeviceType.Listener || deviceType == UDPDeviceType.Dual)
            {
                // Create and start communication thread
                thread = new Thread(new ThreadStart(ListenForData));
                thread.Start();
            }
        }


        /// <summary>
        /// Stop the thread when destroying and close the UDP port.
        /// </summary>
        ~UDPDeviceManager()
        {
            StopListening();
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
            Debug.Log(deviceName + " device connection established.");
        }

        #region Listener methods
        /// <summary>
        /// Sends the current command to the sensor to retrieve data.
        /// </summary>
        private void ListenForData()
        {
            // Continuously read from sensor while active.
            while (runThread)
            {

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

                // Sleep for 1ms.
                Thread.Sleep(1);
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
        /// Re-starts sensor readings if previously stopped.
        /// </summary>
        public void StartListening()
        {
            if (deviceType == UDPDeviceType.Listener || deviceType == UDPDeviceType.Dual)
            {
                if (runThread == false)
                {
                    Debug.Log(deviceName + " device data gathering started.");
                    //Debug.Log(ip + " " + port);
                    runThread = true;
                    // Create and start communication thread
                    thread = new Thread(new ThreadStart(ListenForData));
                    thread.Start();
                }
            }
            else
                throw new System.InvalidOperationException("Cannot start listening, the device is of Writer type.");
        }

        /// <summary>
        /// Terminates the device reading thread. Should be called when not using sensor anymore.
        /// </summary>
        public void StopListening()
        {
            runThread = false;
        }

        protected abstract void ProcessReceivedData(Byte[] receivedBytes);

        #endregion

        #region Writer methods

        /// <summary>
        /// Sends a data string through UDP
        /// </summary>
        /// <param name="data"></param>
        protected void WriteData(string data)
        {
            if (deviceType == UDPDeviceType.Writer || deviceType == UDPDeviceType.Dual)
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                udpState.u.Send(sendBytes, sendBytes.Length);
            }
            else
                throw new System.InvalidOperationException("Cannot write to device, this device is of Listener type.");
        }

        #endregion

        /// <summary>
        /// Processes a set of data and sends it to the remote device.
        /// Should call the WriteData method to send the data through UDP.
        /// </summary>
        /// <param name="data">The data to be transmitted: string, float.</param>
        /// <returns>True if successful.</returns>
        public abstract void SendData(dynamic data);

        /// <summary>
        /// Receives requested data (dynamic type) for the given command.
        /// </summary>
        /// <param name="command">The command to request data from device.</param>
        /// <returns>The requested data.</returns>
        public abstract dynamic GetData(string command);
    }
}

