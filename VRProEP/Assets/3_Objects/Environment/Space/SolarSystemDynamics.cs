using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemDynamics : MonoBehaviour
{
    public List<Transform> celestialBodies = new List<Transform>();

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0.0f, -0.05f, 0.0f) * Time.deltaTime);
        foreach (Transform celestialBodyTransform in celestialBodies)
            celestialBodyTransform.Rotate(new Vector3(0.0f, -0.5f, 0.0f) * Time.deltaTime);
    }
}
