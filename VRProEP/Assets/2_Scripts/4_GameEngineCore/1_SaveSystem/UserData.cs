//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// The types of users supported.
    /// </summary>
    public enum UserType
    {
        Transhumeral,
        Transradial,
        AbleBodied
    }

    /// <summary>
    /// Structure to save user data for persistency.
    /// Lengths given in meters.
    /// </summary>
    public class UserData
    {
        public string name;
        public string familyName;
        public int yearOfBirth;
        public string id;
        public float upperArmLength;
        public float upperArmWidth;
        public float foreArmLength;
        public float foreArmWidth;
        public float handLength;
        public UserType type;
    }

}
