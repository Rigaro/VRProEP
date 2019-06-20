using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalSpinner : MonoBehaviour
{
    public float rotationSpeed = 1.0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.0f, rotationSpeed * Time.deltaTime, 0.0f);
    }
}
