//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Basic state-feedback controller a given plant.
    /// </summary>
    public class StateFeedbackController : IController
    {
        private int stateSize;
        private float[] gains;

        public StateFeedbackController(int stateSize, float[] gains)
        {
            if (stateSize <= 0)
                throw new System.ArgumentOutOfRangeException("The number of states should be greater than zero.");
            else if (gains.Length != stateSize)
                throw new System.ArgumentException("The number of gains does not match the number of states.");

            this.stateSize = stateSize;
            this.gains = gains;
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

            if (reference.Length != stateSize || feedback.Length != stateSize)
                throw new System.ArgumentOutOfRangeException("The input arrays size does not match.");

            // Calculate control input
            float u = 0.0f;
            for (int i = 0; i < stateSize; i++)
            {
                float error = reference[i] - feedback[i];
                u += gains[i] * error;
            }

            return u;
        }
    }

}
