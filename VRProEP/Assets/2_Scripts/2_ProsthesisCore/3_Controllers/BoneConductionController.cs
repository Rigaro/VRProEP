//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections;
using System.Collections.Generic;
using VRProEP.Utilities;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Basic bone conduction controller a given plant.
    /// </summary>
    public class BoneConductionController : UDPWriter, IController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="channelNumber"></param>
        /// <param name="amplitudeMax"></param>
        /// <param name="sampleRate"></param>
        public BoneConductionController(string ipAddress, int port, int channelNumber, float amplitudeMax, int sampleRate) : base(ipAddress, port, "BoniRemote")
        {
            SetUpControlInput(channelNumber);
        }

        /// <summary>
        /// Provides the control input command to an actuator/device. Basic state feedback controller.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="reference">The references for the controller to track.</param>
        /// <param name="feedback">The full state feedback provided by sensors/observers.</param>
        /// <returns>The updated control input.</returns>
        public float UpdateControlInput(float[] reference, float[] feedback)
        {
            // write Data
            SendData(reference);
            return 1;
        }
        private void SetUpControlInput(int channel)
        {

        }

    }
}
