using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGridManager : MonoBehaviour
{
    // Config variables
    [SerializeField]
    private GameObject touchyBallPrefab;
    [SerializeField]
    private int rows;
    [SerializeField]
    private int cols;
    [SerializeField]
    private float spacing;

    // Management variables
    private List<TouchyBallManager> balls = new List<TouchyBallManager>();// The list of spawned balls
    private bool hasSelected = false; // Whether a ball has been selected
    private int selectedIndex = 0;
    private bool selectedTouched = false;
    
    public bool SelectedTouched { get => selectedTouched; }


    /// <summary>
    /// Spawns a new grid of touchy balls.
    /// </summary>
    /// <param name="rows">The number of rows. rows > 0.</param>
    /// <param name="cols">The number of columns. columns > 0.</param>
    /// <param name="spacing">The spacing between balls in cm.</param>
    public void SpawnGrid(int rows, int cols, float spacing)
    {
        // Check for allowed input
        if (rows <= 0 || cols <= 0 || spacing <= 0)
            throw new System.ArgumentException("The rows, columns, and spacing must be > 0.");

        this.rows = rows;
        this.cols = cols;
        this.spacing = spacing;

        float yPos = 0.0f;
        float xPos = 0.0f;

        // Spawn the grid while keeping the parent transform in the centre of the grid
        for (int r = rows - 1; r >= 0; r--)
        {
            for (int c = 0; c < cols; c++)
            {
                // Spawn a new ball with this as parent
                GameObject ball = Instantiate(touchyBallPrefab, this.transform);
                // Add ball to collection
                balls.Add(ball.GetComponent<TouchyBallManager>());

                // Calculate the spawning positions
                xPos = spacing * (c - (cols - 1) / 2.0f);
                yPos = spacing * (r - (rows - 1) / 2.0f);

                // Move the local position of the ball.
                ball.transform.localPosition = new Vector3(xPos, yPos, 0.0f);
            }
        }
        // Check that the right number of balls have been added
        if (balls.Count != rows * cols)
            throw new System.Exception("The number of spawned balls do not match the requested balls. Balls: " + balls.Count);

    }

    /// <summary>
    /// Selects the ball in the given row and column.
    /// </summary>
    /// <param name="row">The row in the range [0, maxRow - 1].</param>
    /// <param name="col">The column in the range [0, maxCol - 1].</param>
    public void SelectBall(int row, int col)
    {
        if (row >= rows || col >= cols || rows < 0 || cols < 0)
            throw new System.ArgumentOutOfRangeException("The requested location is out of range. Row range: [0, " + (rows - 1) + "]. Column range: [0, " + (cols - 1) + "].");

        // Reset previous selections
        ResetBallSelection();

        // Set selection index
        selectedIndex = (row * rows) + col;
        balls[selectedIndex].SetSelected();
        hasSelected = true;
        selectedTouched = false;
    }

    /// <summary>
    /// Selects a ball in the grid by the given index.
    /// </summary>
    /// <param name="index">The ball index [0, rows*cols - 1]</param>
    public void SelectBall(int index)
    {
        if (index < 0 || index >= rows*cols) throw new System.ArgumentOutOfRangeException("The requested location is out of range. Index: [0, " + ((rows*cols) - 1) + "].");

        // Reset previous selections
        ResetBallSelection();

        // Select ball
        selectedIndex = index;
        balls[index].SetSelected();
        hasSelected = true;
        selectedTouched = false;
    }

    /// <summary>
    /// Clears the ball selection
    /// </summary>
    public void ResetBallSelection()
    {
        foreach (TouchyBallManager ball in balls)
        {
            ball.ClearSelection();
        }
        hasSelected = false;
        selectedTouched = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the selected ball has been touched
        if (hasSelected)
        {
            if (balls[selectedIndex].BallState == TouchyBallManager.TouchyBallState.Correct)
                selectedTouched = true;
            else
                selectedTouched = false;
        }

    }
}
