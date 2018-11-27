//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;

namespace VRProEP.ProsthesisCore
{
    public class JacobianSynergy : ReferenceGenerator
    {
        private float upperArmLength;
        private float lowerArmLength;
        private float alpha;
        private bool isEnabled = false;
        private bool enableRequested = false;
        
        public JacobianSynergy(float[] xBar, float[] xMin, float[] xMax, float upperArmLength, float lowerArmLength)
        {
            if (xBar.Length != xMin.Length || xBar.Length != xMax.Length)
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match.");

            channelSize = xBar.Length;
            this.xBar = xBar;
            this.xMin = xMin;
            this.xMax = xMax;
            this.upperArmLength = upperArmLength;
            this.lowerArmLength = lowerArmLength;
            generatorType = ReferenceGeneratorType.JacobianSynergy;
        }

        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// Input organized as following:
        /// - 0: Shoulder flexion/extension angle in radians.
        /// - 1: Elbow flexion/extension angle in radians.
        /// - 2: Shoulder flexion/extension angular velocity in rad/s.
        /// - 3: Enable (1.0f), Disable (0.0f).
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <remarks>
        /// Uses the simplified frame of reference formed by the shoulder-elbow-hand plane.
        /// Therefore, local shoulder and elbow flexion/extension angles shoulder be used.
        /// </remarks>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The orgnized input as given in summary.</param>
        /// <returns>The updated elbow joint angle reference.</returns>
        public override float UpdateReference(int channel, float[] input)
        {
            // Check validity of the provided channel
            if (!IsChannelValid(channel))
                throw new System.ArgumentOutOfRangeException("The requested channel number is invalid.");

            // Check validity of the provided input
            if (!IsInputValid(1, input))
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match the number of reference channels.");

            // Extract input
            float qShoulder = input[0];
            float qElbow = input[1];
            float qDotShoulder = input[2];
            bool enable = false;
            if (input[3] >= 1.0f)
                enable = true;

            // Check if requested to enable the synergy
            if (enable && !enableRequested && !isEnabled)
            {
                // Requested to enable, get button down
                enableRequested = true;
                // Get new reference frame
                SetDirectionOfMotionFrameOffset(qShoulder, qElbow);
            }
            else if (!enable && enableRequested) // Released button
            {
                enableRequested = false;
            }
            else if (enable && !enableRequested && isEnabled)
            {
                // Requested to disable, get button down
                enableRequested = true;
                isEnabled = false;
            }

            xBar[channel - 1] = SingleDOFJacobianSynergy(channel, qShoulder, qElbow, qDotShoulder);

            return xBar[channel - 1];

        }

        /// <summary>
        /// Updates the reference for all channels to be tracked by a controller or device.
        /// Input, per channel, organized as following:
        /// - 0: Shoulder flexion/extension angle in radians.
        /// - 1: Elbow flexion/extension angle in radians.
        /// - 2: Shoulder flexion/extension angular velocity in rad/s.
        /// - 3: Enable (1.0f), Disable (0.0f).
        /// Should only be called within Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="input">The orgnized input as given in summary.</param>
        /// <returns>The updated elbow joint angle reference.</returns>
        public override float[] UpdateAllReferences(float[] input)
        {
            // Check validity of provided input
            if (!IsInputValid(input))
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match the number of reference channels.");

            for (int i = 1; i <= channelSize; i++)
            {
                float[] newInput = { input[4 * i - 4], input[4 * i - 3], input[4 * i - 2], input[4 * i - 1] };
                UpdateReference(i, newInput);
            }
            return xBar;
        }

        /// <summary>
        /// Sets the rotation (alpha) to transform from the reference to the direction of motion plane.
        /// </summary>
        /// <param name="qShoulder">The starting position shoulder angle in radians.</param>
        /// <param name="qElbow">The starting position elbow angle in radians.</param>
        private void SetDirectionOfMotionFrameOffset(float qShoulder, float qElbow)
        {
            // Compute the unity vector from the shoulder joint to the hand grasp point.
            Vector2 dShoulderToHand = new Vector2(upperArmLength * Mathf.Cos(qShoulder) + lowerArmLength * Mathf.Cos(qShoulder + qElbow), upperArmLength * Mathf.Sin(qShoulder) + lowerArmLength * Mathf.Sin(qShoulder + qElbow));
            Vector2 uS2H = dShoulderToHand / (dShoulderToHand.magnitude);
            // Compute the raw rotation
            alpha = Mathf.Acos(uS2H.x);
        }

        /// <summary>
        /// Rotates the given shoulder angle to the current direction of motion frame.
        /// </summary>
        /// <param name="qShoulder">The given shoulder angle in radians.</param>
        /// <returns>The rotated shoulder angle in radians.</returns>
        private float RotateShoulderAngleToDOMFrame(float qShoulder)
        {
            if (qShoulder >= 0)
                return alpha - qShoulder;
            else
                return qShoulder - alpha;
        }

        /// <summary>
        /// Computes de desired elbow joint angle using an arm jacobian-based synergy.
        /// </summary>
        /// <param name="channel">The desired channel to update.</param>
        /// <param name="qShoulder">The shoulder angle in radians w.r.t. reference frame.</param>
        /// <param name="qElbow">The elbow angle in radians w.r.t. reference frame.</param>
        /// <param name="qDotShoulder">The shoulder angular velocity in rad/s w.r.t. reference frame.</param>
        /// <returns></returns>
        private float SingleDOFJacobianSynergy(int channel, float qShoulder, float qElbow, float qDotShoulder)
        {
            float qShoulder_DOM = RotateShoulderAngleToDOMFrame(qShoulder);
            // Compute the desired elbow velocity
            float qDotElbow = (upperArmLength * Mathf.Cos(qShoulder_DOM) + lowerArmLength * Mathf.Cos(qShoulder_DOM + qElbow)) * qDotShoulder / (lowerArmLength * Mathf.Cos(qShoulder_DOM + qElbow));
            // Integrate.
            float tempXBar = xBar[channel - 1] + ( qDotElbow * Time.fixedDeltaTime );
            // Saturate reference
            if (tempXBar > xMax[channel - 1])
                tempXBar = xMax[channel - 1];
            else if (tempXBar < xMin[channel - 1])
                tempXBar = xMin[channel - 1];

            return tempXBar;
        }

        /// <summary>
        /// Checks the validity of the provided input.
        /// </summary>
        /// <param name="input">The input to be verified.</param>
        /// <returns>True if valid.</returns>
        private new bool IsInputValid(float[] input)
        {
            // Check validity of the provided channel
            if (input.Length/channelSize != 4)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Checks the validity of the provided input for a given channel size.
        /// </summary>
        /// <param name="channelSize">The given channel size.</param>
        /// <param name="input">The input to be verified.</param>
        /// <returns>True if valid.</returns>
        private bool IsInputValid(int channelSize, float[] input)
        {
            // Check validity of the provided channel
            if (input.Length / channelSize != 4)
                return false;
            else
                return true;
        }

        // Encapsulation
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
        }
    }

}

