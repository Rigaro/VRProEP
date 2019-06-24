using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace VRProEP.Utilities
{
    public class UDPWriter : UDPDeviceManager
    {
        /// <summary>
        /// Writes data through a UDP connection
        /// </summary>
        /// <param name="ipAddress">The remote IP address.</param>
        /// <param name="port">The remote port.</param>
        /// <param name="deviceName">The remote device name.</param>
        /// <param name="udpType">The type of UDP connection (sync/async).</param>
        public UDPWriter(string ipAddress, int port, string deviceName, UDPType udpType = UDPType.UDP_Async) : base(ipAddress, port, deviceName, UDPDeviceType.Writer, udpType)
        {
        }

        /// <summary>
        /// Sends the provided data to the remote device. Data types supported:
        /// string, List<float>
        /// </summary>
        /// <param name="data">The data to be transmitted.</param>
        public override void SendData(dynamic data)
        {
            string dataString = "";
            // Check the data type and process it
            // String
            if (data.GetType() == typeof(string))
            {
                dataString = data;
            }
            // List<float>
            else if (data.GetType() == typeof(List<float>))
            {
                // Concatenate all values in a string separated by %
                foreach(float value in data)
                {
                    dataString += value + "%";
                }
            }
            else
            {
                throw new System.NotImplementedException("Provided data type (" + data.GetType() + ") not yet implemented.");
            }
            // Send data
            TransferData(dataString);
        }


        protected override void ProcessReceivedData(byte[] receivedBytes)
        {
            throw new System.NotImplementedException("Writer type, do not use.");
        }

        public override dynamic GetData(string command)
        {
            throw new System.NotImplementedException("Writer type, do not use.");
        }

    }
}
