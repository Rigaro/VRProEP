using System.Collections.Generic;
using System.Linq;

namespace VRProEP.AdaptationCore
{
    public class MovingAverageFilter : IFilter
    {

        private int windowSize;
        private Queue<float> dataBuffer;

        /// <summary>
        /// Creates a moving average filter for a float dataset with given window size.
        /// </summary>
        /// <param name="windowSize">The filter window size.</param>
        public MovingAverageFilter(int windowSize)
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
        private void Append(float data)
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
        /// <param name="u">The data to be appended.</param>
        /// <returns></returns>
        public float Update(float u)
        {
            Append(u);
            return dataBuffer.Average();
        }

        /// <summary>
        /// Clear's the filter's buffer.
        /// </summary>
        public void Reset()
        {
            dataBuffer.Clear();
        }


        /// <summary>
        /// NOT NEEDED FOR MOVING AVERAGE FILTER.
        /// </summary>
        /// <param name="wo">The filter cut-off frequency 'wo' in rad/s.</param>
        public void SetCutOffFrequency(float wo)
        {
        }
        /// <summary>
        /// NOT NEEDED FOR MOVING AVERAGE FILTER.
        /// </summary>
        /// <param name="G"></param>
        public void SetGain(float g)
        {
        }
        /// <summary>
        /// NOT NEEDED FOR MOVING AVERAGE FILTER.
        /// </summary>
        /// <param name="ts">The filter (system) sampling time.</param>
        public void SetSamplingTime(float ts)
        {
        }
    }
}