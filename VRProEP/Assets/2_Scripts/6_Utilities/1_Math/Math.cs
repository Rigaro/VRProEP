using System.Collections;
using System.Collections.Generic;

namespace VRProEP.Utilities
{
    public static class Math
    {
        /// <summary>
        /// Performs a matrix x vector multiplication A*V, where A is a matrix and V a vector.
        /// A should have as many columns as V has rows.
        /// </summary>
        /// <param name="matrix">The matrix A.</param>
        /// <param name="vector">The vector V.</param>
        /// <returns></returns>
        public static float[] MatrixVectorMultiplication(float[][] matrix, float[] vector)
        {
            if (matrix == null || vector == null)
                throw new System.ArgumentNullException("The provided matrix or vector is empty");
            if (matrix[0].Length != vector.Length)
                throw new System.ArgumentOutOfRangeException("The matrix and vector are not of the same length.");

            float[] result = new float[matrix.Length];
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < vector.Length; j++)
                {
                    result[i] += matrix[i][j] * vector[j];
                }
            }
            return result;
        }

        /// <summary>
        /// Performs a matrix x vector multiplication A*V, where A is a matrix and V a vector.
        /// A should have as many columns as V has rows.
        /// </summary>
        /// <param name="vector">The vector V.</param>
        /// <param name="matrix">The matrix A.</param>
        /// <returns></returns>
        public static float[] VectorMatrixMultiplication(float[] vector, float[][] matrix)
        {
            if (matrix == null || vector == null)
                throw new System.ArgumentNullException("The provided matrix or vector is empty");
            if (matrix[0].Length != vector.Length)
                throw new System.ArgumentOutOfRangeException("The matrix and vector are not of the same length.");

            float[] result = new float[matrix.Length];
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < vector.Length; j++)
                {
                    result[i] += vector[j] * matrix[j][i];
                }
            }
            return result;
        }

        /// <summary>
        /// Performs a matrix multiplication A*B.
        /// </summary>
        /// <param name="m1">The first matrix A.</param>
        /// <param name="m2">The second matrix B.</param>
        /// <returns></returns>
        public static float[][] MatrixMultiplication(float[][] m1, float[][] m2)
        {
            if (m1 == null || m2 == null)
                throw new System.ArgumentNullException("A provided matrix is empty.");
            if (m1.Length != m2.Length)
                throw new System.ArgumentOutOfRangeException("The matrices are not of the same size.");
            if (m1[0].Length != m2[0].Length)
                throw new System.ArgumentOutOfRangeException("The matrices are not of the same length.");

            // Create the output matrix
            float[][] result = new float[m1.Length][];
            for (int i = 0; i < m1.Length; i++)
            {
                result[i] = new float[m2[0].Length];
            }

            // Add matrices together
            for (int i = 0; i < m1.Length; i++)
            {
                for (int j = 0; j < m1[0].Length; j++)
                {
                    for (int k = 0; k < m1[0].Length; k++)
                    {

                        result[i][j] += m1[i][k] * m2[k][j];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs vector x vector multiplication V1*V2, where V1 is a row vector and V2 a column vector.
        /// Vectors should be of same length.
        /// </summary>
        /// <param name="v1">The row vector.</param>
        /// <param name="v2">The column vector.</param>
        /// <returns></returns>
        public static float VectorDotProduct(float[] v1, float[] v2)
        {
            if (v1 == null || v2 == null)
                throw new System.ArgumentNullException("A provided vector is empty.");
            if (v1.Length != v2.Length)
                throw new System.ArgumentOutOfRangeException("The vectors are not of the same length.");

            float result = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                result += v1[i] * v2[i];
            }
            return result;
        }

        /// <summary>
        /// Performs vector x vector multiplication V1*V2, where V1 is a row vector and V2 a column vector.
        /// Vectors should be of same length.
        /// </summary>
        /// <param name="v1">The column vector.</param>
        /// <param name="v2">The row vector.</param>
        /// <returns></returns>
        public static float[][] ColRowVectorMultiplication(float[] v1, float[] v2)
        {
            if (v1 == null || v2 == null)
                throw new System.ArgumentNullException("A provided vector is empty.");
            if (v1.Length != v2.Length)
                throw new System.ArgumentOutOfRangeException("The vectors are not of the same length.");

            // Create the output matrix
            float[][] result = new float[v1.Length][];
            for (int i = 0; i < v1.Length; i++)
            {
                result[i] = new float[v2.Length];
            }

            // Multiply columns times rows
            for (int i = 0; i < v1.Length; i++)
            {
                for (int j = 0; j < v2.Length; j++)
                {
                    result[i][j] = v1[i] * v2[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Performs a multiplication between a vector and a scalar.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="scalar">The scalar.</param>
        /// <returns></returns>
        public static float[] VectorScalarMultiplication(float[] vector, float scalar)
        {
            if (vector == null)
                throw new System.ArgumentNullException("The provided vector is empty.");

            float[] result = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] * scalar;
            }
            return result;
        }

        /// <summary>
        /// Performs a multiplication between the elements of two vectors.
        /// </summary>
        /// <param name="v1">The first vector.</param>
        /// <param name="v2">The second vector.</param>
        /// <returns></returns>
        public static float[] VectorElementMultiplication(float[] v1, float[] v2)
        {
            if (v1 == null || v2 == null)
                throw new System.ArgumentNullException("A provided vector is empty.");
            if (v1.Length != v2.Length)
                throw new System.ArgumentOutOfRangeException("The vectors are not of the same length.");

            float[] result = new float[v1.Length];
            for (int i = 0; i < v1.Length; i++)
            {
                result[i] = v1[i] * v2[i];
            }
            return result;
        }

        /// <summary>
        /// Adds two vectors together. Vectors should be of the same length.
        /// </summary>
        /// <param name="v1">The first vector.</param>
        /// <param name="v2">The second vector</param>
        /// <returns></returns>
        public static float[] VectorAddition(float[] v1, float[] v2)
        {
            if (v1 == null || v2 == null)
                throw new System.ArgumentNullException("A provided vector is empty.");
            if (v1.Length != v2.Length)
                throw new System.ArgumentOutOfRangeException("The vectors are not of the same length.");

            float[] result = new float[v1.Length];
            for (int i = 0; i < v1.Length; i++)
            {
                result[i] = v1[i] + v2[i];
            }
            return result;
        }

        /// <summary>
        /// Adds two matrices together. Matrices should be of the same size.
        /// </summary>
        /// <param name="v1">The first matrix.</param>
        /// <param name="v2">The second matrix</param>
        /// <returns></returns>
        public static float[][] MatrixAddition(float[][] m1, float[][] m2)
        {
            if (m1 == null || m2 == null)
                throw new System.ArgumentNullException("A provided matrix is empty.");
            if (m1.Length != m2.Length)
                throw new System.ArgumentOutOfRangeException("The matrices are not of the same size.");
            if (m1[0].Length != m2[0].Length)
                throw new System.ArgumentOutOfRangeException("The matrices are not of the same length.");

            // Create the output matrix
            float[][] result = new float[m1.Length][];
            for (int i = 0; i < m1.Length; i++)
            {
                result[i] = new float[m2[0].Length];
            }

            // Add matrices together
            for (int i = 0; i < m1.Length; i++)
            {
                for (int j = 0; j < m1[0].Length; j++)
                {
                    result[i][j] = m1[i][j] * m2[i][j];
                }
            }

            return result;
        }
        /// <summary>
        /// Performs a matrix transpose.
        /// </summary>
        /// <param name="m1">The matrix to be transposed.</param>
        /// <returns>The matrix transpose</returns>
        public static float[][] MatrixTranspose(float[][] m1)
        {
            if (m1 == null)
                throw new System.ArgumentNullException("A provided matrix is empty.");
            if (m1.Length != m1[0].Length)
                throw new System.ArgumentOutOfRangeException("The matrix is not square.");

            // Create the output matrix
            float[][] result = new float[m1.Length][];
            for (int i = 0; i < m1.Length; i++)
            {
                result[i] = new float[m1[0].Length];
            }

            // Transpose
            for (int i = 0; i < m1.Length; i++)
            {
                for (int j = 0; j < m1[0].Length; j++)
                {
                    result[j][i] = m1[i][j];
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a diagonal matrix of "size" with value "val".
        /// </summary>
        /// <param name="size">The size of the identity matrix.</param>
        /// <param name="val">The value to inizialize the diagonal matrix with.</param>
        /// <returns></returns>
        public static float[][] DiagonalMatrix(int size, float val)
        {
            if (size <= 0)
                throw new System.ArgumentNullException("The matrix size must be greater than 0.");

            // Create the output matrix
            float[][] diag = new float[size][];
            for (int i = 0; i < size; i++)
            {
                diag[i] = new float[size];
                diag[i][i] = val;
            }

            return diag;
        }
    }
}
