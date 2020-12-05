using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GameMaster includes
using VRProEP.ExperimentCore;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using VRProEP.Utilities;

public class BottleInHandManager : MonoBehaviour
{

    
    [SerializeField] [Range (-0.1f,0.1f)] private float yOffest; //-0.05f
    [SerializeField] [Range(-0.1f, 0.1f)] private float zOffest; //0f
    [SerializeField] [Range(-0.1f, 0.1f)] private float xOffest; //0.02f

    // Update is called once per frame
    // Attach the bottle to the hand
    void Update()
    {
        if (AvatarSystem.IsAvatarAvaiable)
        {
            //Debug.Log("Avatar find, attach bottle to hand!");
            GameObject hand = GameObject.FindGameObjectWithTag("Hand");           
            transform.rotation = hand.transform.rotation;  // Orientation
            transform.Rotate(Vector3.left, -90f);
            transform.Rotate(Vector3.up, -90f);
            transform.position = hand.transform.position + transform.forward*xOffest + transform.up * yOffest + transform.right * zOffest; //Position
        }
    }
}
