//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using UnityEngine;


namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// Structure to save user avatar customization data for persistency.
    /// </summary>
    public class AvatarData
    {
        public Vector3 residualLimbDimensions;
        public Vector3 socketDimensions;
        public Vector3 elbowUpperDimensions;
        public Vector3 elbowLowerDimensions;
        public Vector3 forearmDimensions;
        public float handLength;

        public string residualLimbType;
        public string socketType;
        public string elbowUpperType;
        public string elbowLowerType;
        public string forearmType;
        public string handType;
    }

}
