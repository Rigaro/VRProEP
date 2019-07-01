using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// Interfaces for Graspable objects that can be interacted with.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Sets the interaction force applied to the grasped object.
        /// </summary>
        /// <param name="force">The force to be applied.</param>
        void SetForce(float force);

        /// <summary>
        /// Returns the object's roughness.
        /// </summary>
        /// <returns>The object's roughness [0,1].</returns>
        float GetRoughness();
    }
}
