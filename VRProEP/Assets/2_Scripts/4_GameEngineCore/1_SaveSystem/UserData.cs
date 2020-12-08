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
        Ablebodied
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
        public float weight;
        public float height;
        public float upperArmLength;
        public float upperArmWidth;
        public float forearmLength;
        public float forearmWidth;
        public float handLength;
        public float trunkLength2SA;
        public float height2SA;
        public UserType type = UserType.Ablebodied;
        public bool lefty = false;

        public void GenerateUserID()
        {
            id = name.ToCharArray()[0].ToString() + familyName.ToCharArray()[0].ToString() + yearOfBirth.ToString() + (100*height).ToString();
        }
    }

}
