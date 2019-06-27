//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// Structure to save bone conduction characterization user data for persistency.
    /// XXXX 
    /// </summary>
    public class BoneConductionCharacterization : FeedbackCharacterization
    {
        public float[] gains;
        public float[] offset;
        public float[] xMin;
        public float[] xMax;
    }

}
