using System;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.AdaptationCore;
namespace VRProEP.ProsthesisCore
{
    public class ANNReferenceGenerator : AdaptiveGenerator
    {
        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The input to use to update the reference.</param>
        /// <returns>The updated reference.</returns>
        /// 

        /// Provides position reference for prosthesis joints through a simple linear kinematic synergy.
        /// </summary>
        /// <param name="xBar">The initial references.</param>
        /// <param name="xMin">The lower limit for the references.</param>
        /// <param name="xMax">The upper limit for the references.</param>
        /// <param name="theta">The initial parameters.</param>
        /// <param name="thetaMin">The lower limit for the parameters.</param>
        /// <param name="thetaMax">The upper limit for the parameters.</param>

        private bool enableRequested = false;
        private float leftySign = 1.0f;

        private PyTCPRequester pyTCPRequester;
        private float[] data = { 1.0f, 1.0f, 1.0f };
        private float[] terminateData = { 0.0f };


        public ANNReferenceGenerator(float[] xBar, float[] xMin, float[] xMax, float[] theta, float[] thetaMin, float[] thetaMax) : base(xBar, xMin, xMax, theta, thetaMin, thetaMax, ReferenceGeneratorType.ANNReferenceGenerator)
        {

            pyTCPRequester = new PyTCPRequester(data);
            pyTCPRequester.Start();






        }

        public override float UpdateReference(int channel, float[] input) {

            //float[] matlab_data = pyTCPRequester.getMatlabData();


            //Debug.Log("MatlabRequester<- Received: " + matlab_data[0]);
            //Debug.Log("The input is: qs = " + Mathf.Rad2Deg * input[0] + ", qe = " + Mathf.Rad2Deg * input[1] + ", qDotS = " + input[2] + ", enable = " + input[3]);


            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Debug.Log("Terminate client thread");
                pyTCPRequester.newData(terminateData);
                pyTCPRequester.Stop();
            }

            

            //Debug.Log(input.Length);
            //Debug.Log(xBar.Length);

            // Check validity of the provided channel
            /*if (channel >= channelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels.");
            else if (channel < 0)
                throw new System.ArgumentOutOfRangeException("The channel number should be greater or equal to 0.");

            // Check validity of the provided input
            if (!IsInputValid(input))
                throw new System.ArgumentOutOfRangeException("The length of the parameters does not match the number of reference channels.");
                */ 

            // Extract input
            float qDotShoulder = leftySign * input[0];
            bool enable = false;
            if (input[3] >= 1.0f)
                enable = true;


            // Check if requested to enable the synergy
            if (enable && !enableRequested && !isEnabled)
            {
                // Requested to enable, get button down
                enableRequested = true;
                // Get new reference frame
                isEnabled = true;
            }
            else if (!enable && enableRequested) // Released button
            {
                enableRequested = false;
            }
            else if (enable && !enableRequested && isEnabled)
            {
                //Debug.Log("Synergy disabled.");
                // Requested to disable, get button down
                enableRequested = true;
                isEnabled = false;
            }

            // Only update when enabled, otherwise just use the same fixed reference.
            if (isEnabled)
            {
            

                Debug.Log("Send new data");


                pyTCPRequester.newData(input);


                
                xBar[channel] = TestNN(channel);
                //Debug.Log(Mathf.Rad2Deg * xBar[channel]);
            }



            //Debug.Log(xBar[channel]);
            return xBar[channel];


            
        }

        private float TestNN(int channel)
        {
            // Calculate reference from 1D synergy.
            float[] tempXBarArray = pyTCPRequester.getMatlabData();

            float tempXBar = tempXBarArray[0];
            Debug.Log(tempXBar);

            //float tempXBar = xBar[channel] + GetParameter(channel) * input * Time.fixedDeltaTime; ;
            // Saturate reference
            if (tempXBar > xMax[channel])
                tempXBar = xMax[channel];
            else if (tempXBar < xMin[channel])
                tempXBar = xMin[channel];

            return tempXBar;
        }

        /// <summary>
        /// Updates all the references to be tracked by multiple controllers or devices.
        /// Should only be called within Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="input">The input to use to update the references.</param>
        /// <returns>The updated set of references.</returns>
        public override float[] UpdateAllReferences(float[] input)
        {

            
            return input;
        }

        /// <summary>
        /// Checks the validity of the provided input.
        /// </summary>
        /// <param name="input">The input to be verified.</param>
        /// <returns>True if valid.</returns>
        private new bool IsInputValid(float[] input)
        {
            // Check validity of the provided channel
            if (input.Length != 2 * xBar.Length)
                return false;
            else
                return true;
        }
    }
}
