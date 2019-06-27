using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureBehaviour : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float roughness;

    private void OnMouseUpAsButton()
    {
        GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
        gameController.GetComponent<BasicTesterGM>().SetRoughness(roughness);

    }
}
