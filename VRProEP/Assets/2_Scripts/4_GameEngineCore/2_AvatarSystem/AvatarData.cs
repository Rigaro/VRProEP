//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;


namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// The types of avatars supported.
    /// </summary>
    public enum AvatarType
    {
        Transhumeral,
        Transradial,
        AbleBodied
    }

    /// <summary>
    /// Structure to save user avatar customization data for persistency.
    /// </summary>
    public class AvatarData
    {
        public string residualLimbType;
        public string socketType;
        public string elbowType;
        public string forearmType;
        public string handType;
    }

}
