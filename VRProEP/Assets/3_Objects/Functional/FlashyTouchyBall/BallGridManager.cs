using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGridManager : MonoBehaviour
{
    [SerializeField]
    private GameObject touchyBallPrefab;
    [SerializeField]
    private int rows;
    [SerializeField]
    private int cols;
    [SerializeField]
    private float spacing;

    private List<TouchyBallManager> balls = new List<TouchyBallManager>();

    private bool run = true;

    // Start is called before the first frame update
    void Start()
    {
        SpawnGrid(rows, cols, spacing);
    }

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
            throw new System.Exception("The number of spawned balls do not match the requested balls.");

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
        int index = (row * rows) + col;
        balls[index].SetSelected();
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
    }

    // Update is called once per frame
    void Update()
    {
        if (run)
        {
            SelectBall(4, 4);
            run = false;
        }
    }
}
