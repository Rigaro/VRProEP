using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.GameEngineCore;


public class TargetGridManager : MonoBehaviour
{
    // Config variables
    public enum TargetType { Bottle, Ball}

    [Header("Objects")]
    [SerializeField]
    private GameObject reachBottlePrefab;
    [SerializeField]
    private GameObject reachBallPrefab;
    [SerializeField]
    private GameObject bottleInHand;

    [Header("Use which prefab")]
    [SerializeField]
    private TargetType targetType;


    // Bottle list
    private List<ReachBottleManager> bottles = new List<ReachBottleManager>();// List of bottles
    private List<TouchyBallManager> balls = new List<TouchyBallManager>(); // List of balls


    // Postion of rotations of the bottles in the grid
    private List<Vector3> targetPositions = new List<Vector3>();// List of the bottle postions
    private List<Vector3> targetRotations = new List<Vector3>();// List of the bottle rotations
    private float gridCloseDistanceFactor = 0.75f;
    private float gridMidDistanceFactor = 1.0f;
    private float gridFarDistanceFactor = 1.5f;
    private float gridHeightFactor = 0.5f;

    // Signs
    private bool hasSelected = false; // Whether a bottle has been selected
    private int selectedIndex = -1;
    private bool selectedTouched = false;

    // Subject information
    private float subjectHeight;
    private float subjectArmLength;
    private float subjectFALength;
    private float subjectUALength;
    private float subjectTrunkLength2SA;
    private float subjectHeight2SA;
    private float subjectShoulderBreadth;
    private float sagittalOffset;

    // Accessor
    public bool SelectedTouched { get => selectedTouched; }
    public int TargetNumber {
        get
        {
            if (targetType == TargetType.Ball)
                return balls.Count;
            else
                return bottles.Count;
        }
                             }
    public TargetType CurrentTargetType { get => targetType; set => targetType = value; }

    /*
    public float SubjectHeight { set => subjectHeight = value; }
    public float SubjectArmLength { set => subjectArmLength = value; }
    public float SubjectTrunkLength2SA { set => subjectTrunkLength2SA = value; }
    public float SubjectHeight2SA { set => subjectHeight2SA = value; }
    */
    /*
    public float GridCloseDistanceFactor { set => gridCloseDistanceFactor = value;  }
    public float GridMidDistanceFactor { set => gridMidDistanceFactor = value; }
    public float GridFarDistanceFactor { set => gridFarDistanceFactor = value; }
    public float GridHeightFactor { set => gridHeightFactor = value; }
    */

    /// <summary>
    /// Config the grid paramenters
    /// </summary>
    /// <param >
    /// <returns>
    /// 
    public void ConfigGridPositionFactors(float close, float mid, float far, float height)
    {
        gridCloseDistanceFactor = close;
        gridMidDistanceFactor = mid;
        gridFarDistanceFactor = far;
        gridHeightFactor = height;
    }

    /// <summary>
    /// Config user physiology data
    /// </summary>
    /// <param >
    /// <returns>
    /// 
    public void ConfigUserData()
    {
        subjectHeight = SaveSystem.ActiveUser.height;
        subjectArmLength = SaveSystem.ActiveUser.upperArmLength + SaveSystem.ActiveUser.forearmLength + (SaveSystem.ActiveUser.handLength / 2);
        subjectFALength = SaveSystem.ActiveUser.forearmLength + (SaveSystem.ActiveUser.handLength / 2);
        subjectUALength = SaveSystem.ActiveUser.upperArmLength;
        subjectTrunkLength2SA = SaveSystem.ActiveUser.trunkLength2SA;
        subjectHeight2SA = SaveSystem.ActiveUser.height2SA;
        subjectShoulderBreadth = SaveSystem.ActiveUser.shoulderBreadth;
        sagittalOffset = -subjectShoulderBreadth / 4.0f;
    }

    void Start()
    {
        // Debug
        /*
        GenerateBottleLocations();
        SpawnBottleGrid();
        ResetBottleSelection();
        */
    }

    void Update()
    {
        // Debug: Change selected bottle for debug
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            selectedIndex = selectedIndex + 1;
            if (selectedIndex > bottles.Count-1) selectedIndex = 0;
            SelectTarget(selectedIndex);

        }
        
        // Check if the selected bottle is reached or not
         CheckReached();
        //Debug.Log(selectedTouched);

    }


    /// <summary>
    /// Check if the selected bottle is reached
    /// </summary>
    /// <param >
    /// <returns bool reached>
    private void CheckReached()
    {
      
        // Check if the selected ball has been touched

        if (hasSelected)
        {
            switch (targetType)
            {
                case TargetType.Bottle:
                    if (this.bottles[selectedIndex].BottleState == ReachBottleManager.ReachBottleState.Correct)
                        selectedTouched = true;
                    else
                        selectedTouched = false;
                    break;

                case TargetType.Ball:
                    if (this.balls[selectedIndex].BallState == TouchyBallManager.TouchyBallState.Correct)
                        selectedTouched = true;
                    else
                        selectedTouched = false;
                    break;
            }
            
        }
        //Debug.Log(this.bottles[selectedIndex].BottleState.ToString());
    }

    #region public methods
    /// <summary>
    /// Add the locations of the grid
    /// </summary>
    /// <param >
    /// <returns 
    public void AddTargetLocation(Vector3 position)
    {
        targetPositions.Add(position);

        
    }

    /// <summary>
    /// Add the rotations of the grid
    /// </summary>
    /// <param >
    /// <returns 
    public void AddTargetRotation(Vector3 rotation)
    {
        targetRotations.Add(rotation);


    }


    /// <summary>
    /// Generate the locations of the bottle grid
    /// </summary>
    /// <param >
    /// <returns 
    /// 
    # region Deprecated
    public void GenerateTargetLocations()
    {
        //
        // Constants
        // 
        float bottleheight = 0.2f;

        //
        // Anchor Positions
        //
        Vector3 anchorTargetVeryClose = new Vector3(subjectArmLength * 0.5f, subjectHeight2SA - bottleheight / 2, sagittalOffset);
        Vector3 anchorTargetClose = new Vector3(subjectArmLength * gridCloseDistanceFactor, subjectHeight2SA - bottleheight / 2, sagittalOffset);
        Vector3 anchorTargetMid = new Vector3(subjectArmLength * gridMidDistanceFactor, subjectHeight2SA - bottleheight / 2, sagittalOffset);
        Vector3 anchorTargetFar = new Vector3(subjectArmLength * gridFarDistanceFactor, subjectHeight2SA - bottleheight / 2, sagittalOffset);
        Vector3[] childTargetClose;
        Vector3[] childTargetMid;

        //Very Close

        targetPositions.Add(anchorTargetVeryClose);

        // Close
        targetPositions.Add(anchorTargetClose);
        
        childTargetClose = GenerateChildTargetsClose(JointAngleAtAnchor(anchorTargetClose)[0], JointAngleAtAnchor(anchorTargetClose)[1]); 
        for (int i = 0; i < childTargetClose.Length; i++)
        {
            targetPositions.Add(childTargetClose[i]); // Add child ones
        }
        
       

        // Mid
        targetPositions.Add(anchorTargetMid);
        childTargetMid = GenerateChildTargetsMid(JointAngleAtAnchor(anchorTargetMid)[0], JointAngleAtAnchor(anchorTargetMid)[1]);
        for (int i = 0; i < childTargetMid.Length; i++)
        {
            targetPositions.Add(childTargetMid[i]); // Add child ones
        }
        
        // Far
        targetPositions.Add(anchorTargetFar);

        //
        // Rotations
        //
        targetRotations.Add(new Vector3(0.0f, 0.0f, 0.0f));
        targetRotations.Add(new Vector3(45.0f, 0.0f, 0.0f));
        targetRotations.Add(new Vector3(-45.0f, 0.0f, 0.0f));
    }

    /// <summary>
    /// Calculate shoulder and elbow angles of anchor position
    /// </summary>
    /// <param >
    /// <returns 

    private Vector3[] GenerateChildTargetsClose(float qShoulder, float qElbow)
    {
        float DELTA1 = 20f;
        float DELTA2 = 20f;

        Vector3[] target = new Vector3[2];


        target[0].x = subjectUALength * Mathf.Sin(Mathf.Deg2Rad * qShoulder) + subjectFALength * Mathf.Sin(Mathf.Deg2Rad * (qShoulder + qElbow + DELTA1) );
        target[0].y = subjectHeight2SA - subjectUALength * Mathf.Cos(Mathf.Deg2Rad * qShoulder) - subjectFALength * Mathf.Cos(Mathf.Deg2Rad * (qShoulder + qElbow + DELTA1));
        target[0].z = sagittalOffset;

        
        target[1].x = subjectUALength * Mathf.Sin(Mathf.Deg2Rad * qShoulder) + subjectFALength * Mathf.Sin(Mathf.Deg2Rad * (qShoulder + qElbow - DELTA2));
        target[1].y = subjectHeight2SA - subjectUALength * Mathf.Cos(Mathf.Deg2Rad * qShoulder) -subjectFALength * Mathf.Cos(Mathf.Deg2Rad * (qShoulder + qElbow - 3*DELTA2));
        target[1].z = sagittalOffset;

        return target;
    
    }

    /// <summary>
    /// Calculate shoulder and elbow angles of anchor position
    /// </summary>
    /// <param >
    /// <returns 

    private Vector3[] GenerateChildTargetsMid(float qShoulder, float qElbow)
    {
        float DELTA = 25f;

        Vector3[] target = new Vector3[2];

        target[0].x = subjectUALength * Mathf.Sin(Mathf.Deg2Rad * qShoulder) + subjectFALength * Mathf.Sin(Mathf.Deg2Rad * (qShoulder + qElbow + DELTA));
        target[0].y = subjectHeight2SA - subjectUALength * Mathf.Cos(Mathf.Deg2Rad * qShoulder) - subjectFALength * Mathf.Cos(Mathf.Deg2Rad * (qShoulder + qElbow + DELTA));
        target[0].z = sagittalOffset;


        target[1].x = subjectUALength * Mathf.Sin(Mathf.Deg2Rad * qShoulder) + subjectFALength * Mathf.Sin(Mathf.Deg2Rad * (qShoulder + qElbow + 2 * DELTA));
        target[1].y = subjectHeight2SA - subjectUALength * Mathf.Cos(Mathf.Deg2Rad * qShoulder) - subjectFALength * Mathf.Cos(Mathf.Deg2Rad * (qShoulder + qElbow + 2 * DELTA));
        target[1].z = sagittalOffset;


        return target;
    }

    #endregion

    /// <summary>
    /// Calculate shoulder and elbow angles of anchor position
    /// </summary>
    /// <param >
    /// <returns 
    private float[] JointAngleAtAnchor(Vector3 anchorLocation)
    {
        float qShoulder = 0;
        float qElbow = 0;
        if (subjectArmLength > anchorLocation.x)
        {
            float alpha = Mathf.Acos((Mathf.Pow(subjectUALength, 2) + Mathf.Pow(subjectFALength, 2) - Mathf.Pow(anchorLocation.x, 2))
                                    / (2 * subjectFALength * subjectUALength));
            qElbow = 180 - Mathf.Rad2Deg * alpha;

            float beta = Mathf.Asin(subjectFALength * Mathf.Sin(alpha) / (anchorLocation.x));

            qShoulder = 90 - Mathf.Rad2Deg * beta;

            return new float[] { qShoulder, qElbow };
        }
        else
        {
            return new float[] { 90, 0 };
        }
        
    }

    /// <summary>
    /// Spawn the boottle grid
    /// </summary>
    /// <param >
    /// <returns bool reached>
    public void SpawnTargetGrid()
    {
        for (int i = 0; i <= targetPositions.Count-1; i++)
        {

            if (targetType == TargetType.Ball)
            {
                // Spawn a new bottle with this as parent
                GameObject target = Instantiate(reachBallPrefab, this.transform);
                // Move the local position of the ball.
                target.transform.localPosition = targetPositions[i];
                // Add bottle to collection
                TouchyBallManager manager = target.GetComponent<TouchyBallManager>();
                balls.Add(manager);

                // Disable the in hand bottle
                bottleInHand.SetActive(false);
            }

            // Only bottle will use rotation requirements
            for (int j = 0; j <= targetRotations.Count-1; j++)
            {

                if (targetType == TargetType.Bottle)
                {
                    // Spawn a new bottle with this as parent
                    GameObject target = Instantiate(reachBottlePrefab, this.transform);
                    // Move the local position of the ball.
                    target.transform.localPosition = targetPositions[i];
                    target.transform.Rotate(targetRotations[j]);
                    // Add bottle to collection
                    ReachBottleManager manager = target.GetComponent<ReachBottleManager>();
                    bottles.Add(manager);

                    // Eable the in hand bottle
                    bottleInHand.SetActive(true);
                    manager.SetBottlInHand(this.bottleInHand); // Set in hand bottle gameobject 
                }

                
 
            }
        }

    }

    /// <summary>
    /// Select a bottle by index
    /// </summary>
    /// <param int index>
    /// <returns>
    public void SelectTarget(int index)
    {
        // Reset previous selections
        ResetTargetSelection();

        selectedIndex = index;
        
        switch (targetType)
        {
            case TargetType.Bottle:
                bottles[index].SetSelected();
                break;
            case TargetType.Ball:
                balls[index].SetSelected();
                break;
        }

        hasSelected = true;
        selectedTouched = false;
    }

    /// <summary>
    /// Clears the ball selection
    /// </summary>
    public void ResetTargetSelection()
    {
        switch (targetType)
        {
            case TargetType.Bottle:
                foreach (ReachBottleManager bottle in bottles)
                {
                    bottle.ClearSelection();
                }
                break;
            case TargetType.Ball:
                foreach (TouchyBallManager ball in balls)
                {
                    ball.ClearSelection();
                }
                break;
        }
               
        hasSelected = false;
        selectedTouched = false;
    }
    #endregion
}
