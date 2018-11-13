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

    /// <summary>
    /// Structure to save avatar object data which contains:
    /// - Object name.
    /// - The object general dimensions: length and width.
    /// - The object type as defined by the enum AvatarObjectType.
    /// </summary>
    public class AvatarObjectData
    {
        public string name;
        public Vector2 dimensions; // Length, Width
        public AvatarObjectType objectType;
    }
}