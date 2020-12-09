using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReachBottleManager : MonoBehaviour
{
    public enum ReachBottleState { Idle, Selected, Correct, Wrong }

    [Header("Objects")]
    [SerializeField]
    private GameObject bottleInHand;
    
    [Header("Tolerance")]
    [SerializeField]
    Vector3 positionTolerance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    Vector3 rotationTolerance = new Vector3(5.0f, 15.0f, 15.0f);

    [Header("Colour configuration")]
    [SerializeField]
    private Color idleColor;
    [SerializeField]
    private Color selectedColor;
    [SerializeField]
    private Color correctColor;
    [SerializeField]
    private Color wrongColor;


    private ReachBottleState bottleState = ReachBottleState.Idle;
    
    private Renderer[] allRenderer;
    private Renderer bottleRenderer;
    private Renderer baseRenderer;
    public ReachBottleState BottleState { get => bottleState; }


    // Reset coroutine
    private Coroutine resetCoroutine;
    private bool isWaiting = false;

    //Constants
    private float WAIT_SECONDS = 1.0f;

    void Update()
    {
        //Debug.Log(BottleState);
        //Debug.Log(bottleState);
        /*
            if (CheckReaching())
                Debug.Log("The bottle is inside of the area");
            else
                Debug.Log("The bottle is outside of the area");
            */
    }

    void Start()
    {
        //bottleRenderer = GetComponent<Renderer>();
        allRenderer = GetComponentsInChildren<Renderer>();
        bottleRenderer = allRenderer[0];
        baseRenderer = allRenderer[0];

        bottleRenderer.material.color = idleColor;
        baseRenderer.material.color = idleColor;
    }

    /// <summary>
    /// Trigger when bottle is entering
    /// </summary>
    /// <param >
    /// <returns >
    private void OnTriggerStay(Collider other)
    {
        // Check if hand is gone to return to idle.
        if (other.tag == "Bottle")
        {
            //Debug.Log("Collide");
            if (CheckReached())
            {
                switch (bottleState)
                {
                    case ReachBottleState.Idle:
                        bottleState = ReachBottleState.Wrong;
                        bottleRenderer.material.color = wrongColor;
                        baseRenderer.material.color = wrongColor;
                        break;
                    case ReachBottleState.Selected:
                        bottleState = ReachBottleState.Correct;
                        bottleRenderer.material.color = correctColor;
                        baseRenderer.material.color = correctColor;
                        break;
                    case ReachBottleState.Correct:

                        break;
                    case ReachBottleState.Wrong:

                        break;
                }
            }
        }
    }

    /// <summary>
    /// Trigger when bottle is leaving
    /// </summary>
    /// <param >
    /// <returns >
    private void OnTriggerExit(Collider other)
    {
        // Check if hand is gone to return to idle.
        if (other.tag == "Bottle" && !isWaiting)
        {
            
            resetCoroutine = StartCoroutine(ReturnToIdle(WAIT_SECONDS));
            
        }
    }

    /// <summary>
    /// Check if the bottle reaches the target postion within the error tolerance.
    /// </summary>
    /// <param >
    /// <returns bool reached>
    private bool CheckReached()
    {
        bool positionReached = false;
        bool orientationReached = false;
        bool reached = false;

        Vector3 postionError = this.transform.position - bottleInHand.transform.position;
        //Quaternion quaterError = this.transform.rotation * Quaternion.Inverse(bottleInHand.transform.rotation);
        // Vector3 rotationError = quaterError.eulerAngles;

        Vector3 targetRotation = this.gameObject.transform.rotation.eulerAngles;
        if (targetRotation.x > 200) targetRotation.x -= 360;
        if (targetRotation.y > 200) targetRotation.y -= 360;
        if (targetRotation.z > 200) targetRotation.z -= 360;

        Vector3 bottleInHandRotation = bottleInHand.transform.rotation.eulerAngles;
        if (bottleInHandRotation.x > 200) bottleInHandRotation.x -= 360;
        if (bottleInHandRotation.y > 200) bottleInHandRotation.y -= 360;
        if (bottleInHandRotation.z > 200) bottleInHandRotation.z -= 360;

        Vector3 rotationError = targetRotation - bottleInHandRotation;

        if (Mathf.Abs(postionError.x) < positionTolerance.x && Mathf.Abs(postionError.y) < positionTolerance.y && Mathf.Abs(postionError.z) < positionTolerance.z )
            positionReached = true;  
        else
            positionReached = false;


        if (Mathf.Abs(rotationError.x) < rotationTolerance.x && Mathf.Abs(rotationError.z) < rotationTolerance.z) //
            orientationReached = true;
        else
            orientationReached = false;


        //bottleState = BottleState.Correct;
        reached = positionReached & orientationReached;
        //Debug.Log(bottleInHandRotation);
        // Debug.Log(postionError);
        //Debug.Log(rotationError);
        if(bottleState == ReachBottleState.Selected)
            Debug.Log(bottleInHandRotation);
        return reached;
    }

    /// <summary>
    /// Wait some seconds before returning to idle state.
    /// If it was selected in the middle of the wait, then just stay selected.
    /// </summary>
    /// <param name="waitSeconds">The number of seconds to wait.</param>
    /// <returns>IEnumerator used for the Coroutine.</returns>
        
    private IEnumerator ReturnToIdle(float waitSeconds)
    {
        isWaiting = true;       
        yield return new WaitForSecondsRealtime(waitSeconds);
        bottleState = ReachBottleState.Idle;
        bottleRenderer.material.color = idleColor;
        baseRenderer.material.color = idleColor;

        isWaiting = false;
       // Debug.Log(bottleRenderer.material.color);
    }

    #region public methods
    /// <summary>
    /// Set the in hand bottle gameobject
    /// </summary>
    /// <param GameObject bottleInHand>
    /// <returns>
    public void SetBottlInHand(GameObject bottleInHand)
    {
        this.bottleInHand = bottleInHand;
    }


    /// <summary>
    /// Set bottle the selected one.
    /// </summary>
    public void SetSelection()
    {
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        bottleState = ReachBottleState.Selected;
        bottleRenderer.material.color = selectedColor;
        baseRenderer.material.color = selectedColor;
    }

    /// <summary>
    /// Resets the selection to idle.
    /// </summary>
    public void ClearSelection()
    {
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ReturnToIdle(0.1f));
    }

    #endregion




}
