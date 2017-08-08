using System;
using System.Linq;

namespace NET.Algebra
{
    public class Matrix
    {
        #region Constructors
        public Matrix(double[] array)
        {
            this.values = new double[1, array.Length];
            for (int i = 0; i < this.NumCols; i++)
            {
                this[0, i] = array[i];
            }
        }
        
        public Matrix(int[] array)
        {
            this.values = new double[1, array.Length];
            for (int i = 0; i < this.NumCols; i++)
            {
                this[0, i] = array[i];
            }
        }
        
        public Matrix(double[,] array)
        {
            this.values = array;
        }
        
        public Matrix(int[,] array)
        {
            this.values = new double[array.GetLength(0), array.GetLength(1)];
            for (int y = 0; y < this.NumRows; y++)
            {
                for (int x = 0; x < this.NumCols; x++)
                {
                    this[y, x] = array[y, x];
                }
            }
        }
        
        public Matrix(int numRows, int numCols)
        {
            this.values = new double[numRows, numCols];
        }
        #endregion
        
        /// <summary>
        /// An indexer for the matrix.
        /// </summary>
        public double this[int row, int col]
        {
            get { return this.values[row, col]; }
            set { this.values[row, col] = value; }
        }
        
        #region Properties
        private readonly double[,] values;
        
        public int NumCols { get { return values.GetLength(1); } }
        public int NumRows { get { return values.GetLength(0); } }
        public int Size { get { return values.Length; } }
        #endregion
        
        #region Methods
        /// <summary>
        /// Computes the product of the current matrix and another matrix.
        /// </summary>
        /// <param name="mat">A matrix by which to multiply the current matrix.</param>
        /// <returns>A matrix that is the product of the input matrix and the other matrix.</returns>
        public unsafe Matrix Dot(Matrix mat)
        {
            int numRows = this.NumRows;
            int width = mat.NumCols;
            int numCols = this.NumCols;
            var result = new double[numRows, width];
            fixed (double *pRes = result, pMat1 = this.values, pMat2 = mat.values)
            {
                for (int y = 0; y < numRows; y++)
                {
                    int i = y * numCols;
                    for (int x = 0; x < width; x++)
                    {
                        int j = x;
                        double res = 0d;
                        for (int z = 0; z < numCols; z++, j += width)
                        {
                            res += pMat1[i + z] * pMat2[j];
                        }
                        pRes[y * width + x] = res;
                    }
                }
            }
            return new Matrix(result);
        }
        
        public unsafe void FillWithRandomValues(double min, double max)
        {
            var rnd = new Random();
            double range = max - min;
            int size = this.Size;
            fixed (double *pRes = this.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = rnd.NextDouble() * range + min;
                }
            }
        }
        
        /// <summary>
        /// Checks whether the current matrix is a square matrix.
        /// </summary>
        /// <returns>Returns true if the input matrix is a square matrix; 
        /// otherwise, it returns false.</returns>
        public bool IsSquare()
        {
            return this.NumRows == this.NumCols;
        }
        
        public void PrintToConsole()
        {
            for (int y = 0; y < this.NumRows; y++)
            {
                for (int x = 0; x < this.NumCols; x++)
                {
                    Console.Write("{0} ", this[y, x].ToString().PadLeft(19, ' '));
                }
                Console.WriteLine();
            }
        }
        
        public Matrix RotateClockwise()
        {
            int numRows = this.NumRows;
            int numCols = this.NumCols;
            var result = new double[numCols, numRows];
            for (int y = numRows - 1; y >= 0 ; y--)
            {
                for (int x = 0; x < numCols; x++)
                {
                    result[x, numRows - 1 - y] = this[y, x];
                }
            }
            return new Matrix(result);
        }
        
        public double Trace()
        {
            double sum = 0d;
            for (int i = 0; i < this.NumRows; i++)
            {
                if (i < this.NumCols)
                {
                    sum += this[i, i];
                }
            }
            return sum;
        }
        
        /// <summary>
        /// Computes the transpose of the current matrix.
        /// </summary>
        /// <returns>A matrix that is the transpose of the input matrix.</returns>
        public Matrix Transpose()
        {
            var result = new double[this.NumCols, this.NumRows];
            for (int y = 0; y < this.NumRows; y++)
            {
                for (int x = 0; x < this.NumCols; x++)
                {
                    result[x, y] = this[y, x];
                }
            }
            return new Matrix(result);
        }
        #endregion
        
        #region Operators
        public unsafe static Matrix operator +(Matrix mat1, Matrix mat2)
        {
            int size = mat1.Size;
            var result = new double[mat1.NumRows, mat1.NumCols];
            fixed (double *pRes = result, pMat1 = mat1.values, pMat2 = mat2.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = pMat1[i] + pMat2[i];
                }
            }
            return new Matrix(result);
        }
        
        public static Matrix operator +(Matrix mat, double val)
        {
            return AddValue(mat, val);
        }
        
        public static Matrix operator +(double val, Matrix mat)
        {
            return AddValue(mat, val);
        }
        
        private unsafe static Matrix AddValue(Matrix mat, double val)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = pMat[i] + val;
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator -(Matrix mat1, Matrix mat2)
        {
            int size = mat1.Size;
            var result = new double[mat1.NumRows, mat1.NumCols];
            fixed (double *pRes = result, pMat1 = mat1.values, pMat2 = mat2.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = pMat1[i] - pMat2[i];
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator -(Matrix mat, double val)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = pMat[i] - val;
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator -(double val, Matrix mat)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = val - pMat[i];
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator *(Matrix mat1, Matrix mat2)
        {
            int size = mat1.Size;
            var result = new double[mat1.NumRows, mat1.NumCols];
            fixed (double *pRes = result, pMat1 = mat1.values, pMat2 = mat2.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = pMat1[i] * pMat2[i];
                }
            }
            return new Matrix(result);
        }
        
        public static Matrix operator *(Matrix mat, double val)
        {
            return MyltiplyByValue(mat, val);
        }
        
        public static Matrix operator *(double val, Matrix mat)
        {
            return MyltiplyByValue(mat, val);
        }
        
        private unsafe static Matrix MyltiplyByValue(Matrix mat, double val)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = pMat[i] * val;
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator /(Matrix mat1, Matrix mat2)
        {
            int size = mat1.Size;
            var result = new double[mat1.NumRows, mat1.NumCols];
            fixed (double *pRes = result, pMat1 = mat1.values, pMat2 = mat2.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = pMat1[i] / pMat2[i];
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator /(Matrix mat, double val)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values)
            {
                for (int i = 0; i < size; i++)
                {
                    pRes[i] = pMat[i] / val;
                }
            }
            return new Matrix(result);
        }
        #endregion
    }
}