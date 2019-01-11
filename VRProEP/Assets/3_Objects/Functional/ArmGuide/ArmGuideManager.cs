using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmGuideManager : MonoBehaviour
{
    public Transform shoulderLocationTransform;

    private float startAngle;
    private float endAngle;
    private float movementTime;

    private bool isRunning = false;
    private bool startFlag = false;
    private bool success = false;
    private float movementSpeed;
    private float currentAngle;
    private float runTime;
    private bool isHandInGuide;

    public float CurrentAngle { get => currentAngle; }
    public bool IsRunning { get => isRunning; }
    public float RunTime { get => runTime; set => runTime = value; }
    public bool IsHandInGuide { get => isHandInGuide; }
    public bool Success { get => success; }

    private void Start()
    {
        GoToStart();
        Initialize(startAngle, endAngle, movementTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GraspManager"))
            isHandInGuide = true;

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GraspManager"))
            isHandInGuide = false;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = shoulderLocationTransform.position;
        if (startFlag && isRunning && ((Mathf.Sign(movementSpeed) > 0 && currentAngle <= endAngle) || (Mathf.Sign(movementSpeed) < 0 && currentAngle >= endAngle)))
        {
            //Debug.Log(currentAngle + ", " + runTime);
            runTime += Time.fixedDeltaTime;
            currentAngle += movementSpeed * Time.fixedDeltaTime;
            transform.Rotate(new Vector3(-movementSpeed * Time.fixedDeltaTime, 0.0f, 0.0f));
        }
        else if ((Mathf.Sign(movementSpeed) > 0 && currentAngle > endAngle) || (Mathf.Sign(movementSpeed) < 0 && currentAngle < endAngle))
        {
            isRunning = false;
            startFlag = false;
            success = true;
        }
    }

    /// <summary>
    /// Starts the guiding motion
    /// </summary>
    public bool StartGuiding()
    {
        if (isHandInGuide && !isRunning)
        {
            runTime = 0.0f;
            isRunning = true;
            success = false;
            StartCoroutine(StartMoving());
            return true;
        }
        else
        {
            GoToStart();
            isRunning = false;
            startFlag = false;
            success = false;
            return false;
        }
    }

    private IEnumerator StartMoving()
    {
        yield return new WaitForSecondsRealtime(4.0f);
        startFlag = true;
    }

    /// <summary>
    /// Initializes the arm's motion configuration variables.
    /// </summary>
    /// <param name="startAngle"></param>
    /// <param name="endAngle"></param>
    /// <param name="movementTime"></param>
    public void Initialize(float startAngle, float endAngle, float movementTime)
    {
        if (startAngle < 0 || startAngle > 180 || endAngle < 0 || endAngle > 180 || movementTime <= 0)
            throw new System.ArgumentException("Invalid arm guide initialization argument.");

        this.startAngle = startAngle;
        this.endAngle = endAngle;
        currentAngle = startAngle;
        movementSpeed = (endAngle - startAngle) / movementTime;
    }

    /// <summary>
    /// Returns the arm to its start position.
    /// </summary>
    public void GoToStart()
    {
        if (!isRunning)
        {
            currentAngle = startAngle;
            transform.localRotation = Quaternion.Euler(-startAngle, 0.0f, 0.0f);
        }
    }
}
