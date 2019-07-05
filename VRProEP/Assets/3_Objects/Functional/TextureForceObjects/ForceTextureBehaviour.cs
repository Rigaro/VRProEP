using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.GameEngineCore;

public class ForceTextureBehaviour : MonoBehaviour, IInteractable
{
    public bool enableColourFeedback = true;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float force;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float forceTarget;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float roughness;
    [SerializeField]
    private Color noForceColour;
    [SerializeField]
    private Color targetForceColour;
    [SerializeField]
    private Color maxForceColour;

    private Material objectMaterial;

    // Get the material of the object this is attached to
    private void Start()
    {
        objectMaterial = GetComponent<Renderer>().material;
    }

    // Some debug stuff
    private void Update()
    {
        SetForce(force);
    }

    /// <summary>
    /// Sets the roughness of the object
    /// </summary>
    /// <param name="roughness">The object's roughnes [0,1].</param>
    public void SetRoughness(float roughness)
    {
        this.roughness = roughness;
    }

    /// <summary>
    /// Returns the roughness of the object
    /// </summary>
    /// <returns>The object's roughnes [0,1].</returns>
    public float GetRoughness()
    {
        return roughness;
    }

    public void SetTargetForce(float force)
    {
        forceTarget = force;
    }

    /// <summary>
    /// Sets the force being applied to the object.
    /// </summary>
    /// <param name="force">The force [0,1].</param>
    public void SetForce(float force)
    {
        // Set force
        this.force = force;
        // Update object colour
        if (enableColourFeedback && force <= forceTarget) // Interpolate between no force to target colours.
            objectMaterial.color = Color.Lerp(noForceColour, targetForceColour, force / forceTarget);
        else if (enableColourFeedback && force > forceTarget) // Interpolate between target and max colours.
            objectMaterial.color = Color.Lerp(targetForceColour, maxForceColour, (force - forceTarget) / (1 - forceTarget));
    }

    /// <summary>
    /// Sets the colour for when the object has no force applied to it.
    /// </summary>
    /// <param name="colour">The desired rest colour.</param>
    public void SetRestColour(Color colour)
    {
        if (colour == null) throw new System.ArgumentNullException();

        noForceColour = colour;
    }
}
