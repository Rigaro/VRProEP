using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleGridManager : MonoBehaviour
{
    // Config variables

    [Header("Objects")]
    [SerializeField]
    private GameObject reachBottlePrefab;
    [SerializeField]
    private GameObject bottleInHand;


    private List<ReachBottleManager> bottles = new List<ReachBottleManager>();// List of bottles

    // Postion of rotations of the bottles in the grid
    private List<Vector3> bottlePositions = new List<Vector3>();// List of the bottle postions
    private List<Vector3> bottleRotations = new List<Vector3>();// List of the bottle rotations

    private bool hasSelected = false; // Whether a bottle has been selected
    private int selectedIndex = -1;
    private bool selectedTouched = false;

    public bool SelectedTouched { get => selectedTouched; }



    void Start()
    {

        GenerateBottleLocations();
        SpawnBottleGrid();
        ResetBottleSelection();
    }

    void Update()
    {

        // Change selected bottle
        if (Input.GetKeyDown(KeyCode.F1))
        {
            selectedIndex = selectedIndex + 1;
            if (selectedIndex > bottles.Count-1) selectedIndex = 0;
            SelectBottle(selectedIndex);

        }

        // Check if the selected bottle is reached or not
        selectedTouched = CheckReached();
        //Debug.Log(selectedTouched);

    }


    /// <summary>
    /// Check if the selected bottle is reached
    /// </summary>
    /// <param >
    /// <returns bool reached>
    private bool CheckReached()
    {
        bool reached = false;

        // Check if the selected ball has been touched
        if (hasSelected)
        {
            if (this.bottles[selectedIndex].BottleState == ReachBottleManager.ReachBottleState.Correct)
                
                reached = true;
            else
                reached = false;
        }
        //Debug.Log(this.bottles[selectedIndex].BottleState.ToString());
        return reached;
    }

    #region public methods

    /// <summary>
    /// Generate the locations of the bottle grid
    /// </summary>
    /// <param >
    /// <returns 
    public void GenerateBottleLocations()
    {

        bottlePositions.Add(new Vector3(1.0f, 1.0f, 0.2f));

        bottlePositions.Add(new Vector3(1.0f, 1.0f, -0.2f));

        bottleRotations.Add(new Vector3(0.0f, 0.0f, 0.0f));

        bottleRotations.Add(new Vector3(30.0f, 0.0f, 0f));

        bottleRotations.Add(new Vector3(-30.0f, 0.0f, -0f));
    }


    /// <summary>
    /// Spawn the boottle grid
    /// </summary>
    /// <param >
    /// <returns bool reached>
    public void SpawnBottleGrid()
    {
        for (int i = 0; i <= bottlePositions.Count-1; i++)
        {
            for (int j = 0; j <= bottleRotations.Count-1; j++)
            {
                // Spawn a new bottle with this as parent
                GameObject bottle = Instantiate(reachBottlePrefab, this.transform);
                // Move the local position of the ball.
                bottle.transform.localPosition = bottlePositions[i];
                bottle.transform.Rotate(bottleRotations[j]);
                // Add bottle to collection
                ReachBottleManager manager = bottle.GetComponent<ReachBottleManager>();
                bottles.Add(manager);
                manager.SetBottlInHand(this.bottleInHand); // Set in hand bottle gameobject 
 
            }
        }

    }

    /// <summary>
    /// Select a bottle by index
    /// </summary>
    /// <param int index>
    /// <returns>
    public void SelectBottle(int index)
    {
        // Reset previous selections
        ResetBottleSelection();

        bottles[index].SetSelection();
        hasSelected = true;
        selectedTouched = false;
    }

    /// <summary>
    /// Clears the ball selection
    /// </summary>
    public void ResetBottleSelection()
    {
        foreach (ReachBottleManager bottle in bottles)
        {
            bottle.ClearSelection();
        }
        hasSelected = false;
        selectedTouched = false;
    }
    #endregion
}
