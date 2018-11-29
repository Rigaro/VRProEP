using UnityEngine;
using Valve.VR;

namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Tracks a body part using a VIVE Tracker. Uses Unity's XR API to obtain tracking data.
    /// Requires reference to a the Tracker's object Transform.
    /// </summary>
    public class VIVEControllerManager : SensorManager
    {
        // Channel size given by the number of actions implemented.
        // Curently implemented actions:

        private SteamVR_Action_Vector2 trackpadAction = SteamVR_Input.vrproep.inActions.Trackpad;
        private SteamVR_Action_Boolean buttonAction = SteamVR_Input.vrproep.inActions.InterfaceEnableButton;

        public VIVEControllerManager() : base(2, SensorType.VIVEController)
        {

        }

        /// <summary>
        /// Returns raw sensor data for the selected channel.
        /// Currently available data:
        /// 0: value from 0-1 given by the wheel action set.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        public override float GetRawData(int channel)
        {
            if (channel > ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels. The number of available channels is: " + ChannelSize + ".");
            else if (channel <= 0)
                throw new System.ArgumentOutOfRangeException("The channel number starts from 1.");
            
            // Check that the action set is active.
            if (!SteamVR_Input.vrproep.IsActive())
            {
                SteamVR_Input._default.Deactivate();
                SteamVR_Input.vrproep.ActivatePrimary();
            }
            if (channel == 1)
            {
                Vector2 trackpadValue = trackpadAction.GetAxis(SteamVR_Input_Sources.Any);
                return trackpadValue.x;
            }
            else if (channel == 2)
            {
                bool buttonState = buttonAction.GetState(SteamVR_Input_Sources.Any);
                if (buttonState)
                    return 1.0f;
                else
                    return 0.0f;
            }
            else
                throw new System.Exception("Something went wrong, the provided channel is unavailable.");

        }

        /// <summary>
        /// Returns raw sensor data for the selected channel.
        /// Currently available data:
        /// WHEEL: value from 0-1 given by the wheel action set.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Raw sensor data for the given channel.</returns>
        public override float GetRawData(string channel)
        {
            if (channel.Equals("TRACKPAD"))
            {
                // Check that the action set is active.
                if (!SteamVR_Input.vrproep.IsActive())
                {
                    SteamVR_Input._default.Deactivate();
                    SteamVR_Input.vrproep.ActivatePrimary();
                }

                Vector2 trackpadValue = trackpadAction.GetAxis(SteamVR_Input_Sources.Any);
                return trackpadValue.x;
            }
            else if (channel.Equals("BUTTON"))
            {
                bool buttonValue = buttonAction.GetState(SteamVR_Input_Sources.Any);
                if (buttonValue)
                    return 1.0f;
                else
                    return 0.0f;
            }
            else
                throw new System.Exception("The provided channel does not exist. See documentation for available channels.");

        }

        /// <summary>
        /// Returns all raw sensor data for the selected channel.
        /// Currently available data:
        /// 0: value from 0-1 given by the wheel action set.
        /// </summary>
        /// <returns>The array with all raw sensor data.</returns>
        public override float[] GetAllRawData()
        {
            // Check that the action set is active.
            if (!SteamVR_Input.vrproep.IsActive())
            {
                SteamVR_Input._default.Deactivate();
                SteamVR_Input.vrproep.ActivatePrimary();
            }

            Vector2 trackpadValue = trackpadAction.GetAxis(SteamVR_Input_Sources.Any);

            bool buttonState = buttonAction.GetState(SteamVR_Input_Sources.Any);
            float buttonValue;
            if (buttonState)
                buttonValue = 1.0f;
            else
                buttonValue = 0.0f;

            float[] data = { trackpadValue.x, buttonValue };
            return data;
        }

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// Currently available data:
        /// 0: the wheel action delta.
        /// </summary>
        /// <param name="channel">The channel number.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public override float GetProcessedData(int channel)
        {
            if (channel > ChannelSize)
                throw new System.ArgumentOutOfRangeException("The requested channel number is greater than the available number of channels. The number of available channels is: " + ChannelSize + ".");
            else if (channel <= 0)
                throw new System.ArgumentOutOfRangeException("The channel number starts from 1.");

            // Check that the action set is active.
            if (!SteamVR_Input.vrproep.IsActive())
            {
                SteamVR_Input._default.Deactivate();
                SteamVR_Input.vrproep.ActivatePrimary();
            }

            if (channel == 1)
            {
                Vector2 trackpadValue = trackpadAction.GetAxis(SteamVR_Input_Sources.Any);
                return trackpadValue.x;
            }
            else if (channel == 2)
            {
                bool buttonState = buttonAction.GetState(SteamVR_Input_Sources.Any);
                if (buttonState)
                    return 1.0f;
                else
                    return 0.0f;
            }
            else
                throw new System.Exception("Something went wrong, the provided channel is unavailable.");
        }

        /// <summary>
        /// Returns pre-processed sensor data for the selected channel.
        /// Currently available data:
        /// WHEEL: the wheel action delta.
        /// </summary>
        /// <param name="channel">The channel/data identifier.</param>
        /// <returns>Pre-processed sensor data for the given channel.</returns>
        public override float GetProcessedData(string channel)
        {
            if (channel.Equals("TRACKPAD"))
            {
                // Check that the action set is active.
                if (!SteamVR_Input.vrproep.IsActive())
                {
                    SteamVR_Input._default.Deactivate();
                    SteamVR_Input.vrproep.ActivatePrimary();
                }

                Vector2 trackpadValue = trackpadAction.GetAxis(SteamVR_Input_Sources.Any);
                return trackpadValue.x;
            }
            else if (channel.Equals("BUTTON"))
            {
                bool buttonValue = buttonAction.GetState(SteamVR_Input_Sources.Any);
                if (buttonValue)
                    return 1.0f;
                else
                    return 0.0f;
            }
            else
                throw new System.Exception("The provided channel does not exist. See documentation for available channels.");
        }

        /// <summary>
        /// Returns all pre-processed sensor data in an array.
        /// Currently available data:
        /// 0: the wheel action delta.
        /// </summary>
        /// <returns>The array with all pre-processed sensor data.</returns>
        public override float[] GetAllProcessedData()
        {
            // Check that the action set is active.
            if (!SteamVR_Input.vrproep.IsActive())
            {
                SteamVR_Input._default.Deactivate();
                SteamVR_Input.vrproep.ActivatePrimary();
            }

            Vector2 trackpadValue = trackpadAction.GetAxis(SteamVR_Input_Sources.Any);

            bool buttonState = buttonAction.GetState(SteamVR_Input_Sources.Any);
            float buttonValue;
            if (buttonState)
                buttonValue = 1.0f;
            else
                buttonValue = 0.0f;

            float[] data = { trackpadValue.x, buttonValue };

            return data;
        }

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public override void Configure(string command, dynamic value)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        public override void Configure(string command, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}
