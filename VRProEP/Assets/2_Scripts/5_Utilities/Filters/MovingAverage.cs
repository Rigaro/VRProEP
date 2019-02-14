using System.Collections.Generic;
using System.Linq;

public class MovingAverage {
    
    private int windowSize;
    private Queue<float> dataBuffer;

    /// <summary>
    /// Creates a moving average filter for a float dataset with given window size.
    /// </summary>
    /// <param name="windowSize">The filter window size.</param>
    public MovingAverage(int windowSize)
    {
        if (windowSize < 1)
            throw new System.ArgumentOutOfRangeException("The moving average filter window size should be greater than 0.");
        this.windowSize = windowSize;
        dataBuffer = new Queue<float>(windowSize);
    }

    /// <summary>
    /// Appends input data to buffer.
    /// </summary>
    /// <param name="data">The data to be appended.</param>
    public void Append(float data)
    {
        // Append data to buffer when window size not reached.
        if (dataBuffer.Count < windowSize)
        {
            dataBuffer.Enqueue(data);
        }
        // Otherwise drop last item and append new one.
        else
        {
            dataBuffer.Dequeue();
            dataBuffer.Enqueue(data);
        }
    }

    /// <summary>
    /// Updates the filter with the given input data.
    /// </summary>
    /// <param name="data">The data to be appended.</param>
    /// <returns></returns>
    public float Update(float data)
    {
        Append(data);
        return dataBuffer.Average();
    }
}
