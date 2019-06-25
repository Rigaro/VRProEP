//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using VRProEP.Utilities;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Basic bone conduction controller a given plant.
    /// </summary>
    public class BoneConductionController : UDPWriter
    {
        public BoneConductionController(string ipAddress, int port, int channel, float Amplitude_max, int SampleRate) : base(ipAddress, port, "BoniRemote")
        {
            SetUpControlInput(channel);
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
