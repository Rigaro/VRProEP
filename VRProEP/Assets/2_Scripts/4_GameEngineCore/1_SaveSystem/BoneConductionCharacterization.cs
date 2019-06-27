//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// Structure to save bone conduction characterization user data for persistency.
    /// XXXX 
    /// </summary>
    public class BoneConductionCharacterization : FeedbackCharacterization
    {
        public float[] frequencyPerceptionThreshold;
        public float[] perceptionThreshold;
    }

}
