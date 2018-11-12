using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// The types of avatar objects supported.
    /// </summary>
    public enum AvatarObjectType
    {
        ResidualLimb,
        Socket,
        ElbowUpper,
        ElbowLower,
        Forearm, 
        Hand
    }

    public class AvatarObjectData
    {
        public string name;
        public Vector2 dimensions;
        public AvatarObjectType objectType;
    }
}