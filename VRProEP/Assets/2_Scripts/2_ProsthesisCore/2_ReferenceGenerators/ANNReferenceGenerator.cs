using System;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.AdaptationCore;
namespace VRProEP.ProsthesisCore
{
    public class ANNReferenceGenerator : ReferenceGenerator
    {
        /// <summary>
        /// Updates the reference for the given channel to be tracked by a controller or device.
        /// Should only be called during Physics updates, Monobehaviour : FixedUpdate.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <param name="input">The input to use to update the reference.</param>
        /// <returns>The updated reference.</returns>
        /// 

        private PyTCPRequester pyTCPRequester;
        private float[] data = { 1.0f, 1.0f, 1.0f };
        private float[] terminateData = { 0.0f };


        public ANNReferenceGenerator()
        {

            pyTCPRequester = new PyTCPRequester(data);
            pyTCPRequester.Start();

        }

        public override float UpdateReference(int channel, float[] input)
        {

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Debug.Log("Terminate client thread");
                pyTCPRequester.newData(terminateData);
                pyTCPRequester.Stop();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("Send new data");


                pyTCPRequester.newData(data);
            }
            else
            {

                Debug.Log("Send new data");


                pyTCPRequester.newData(data);

                data[1] += 0.01f;
                data[2] += 0.01f;
                data[3] += 0.01f;
            }
            return input[0];
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
    }
}
