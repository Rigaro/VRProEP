//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
namespace VRProEP.ProsthesisCore
{
    /// <summary>
    /// Interface for components that have configurable parameters. These configurations are mostly constant.
    /// For example: sensor with gain parameters.
    /// </summary>
    public interface IConfigurable
    {

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        void Configure(string command, dynamic value);

        /// <summary>
        /// Updates the configuration of a parameter defined by the "command" parameter to the provided "value".
        /// </summary>
        /// <remarks>Commands are defined by the implementing class.</remarks>
        /// <param name="command">The configuration command as established by the implementing class.</param>
        /// <param name="value">The value to update the configuration parameter determined by "command".</param>
        void Configure(string command, string value);
    }
}
