//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.ExperimentCore
{
    public enum UserType
    {
        Transhumeral,
        Transradial,
        AbleBodied
    }

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
