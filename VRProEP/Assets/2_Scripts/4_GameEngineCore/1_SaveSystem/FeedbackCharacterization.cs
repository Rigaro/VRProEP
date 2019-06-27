//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// The types of Feedback supported.
    /// </summary>
    public enum FeedbackType
    {
        Vibrotactile,
        Electrotactile,
        Mechanotactile,
        BoneConduction
    }

    /// <summary>
    /// Structure to save user data for Feedback Characterization.
    /// </summary>
    public abstract class FeedbackCharacterization
    {
    }

}
