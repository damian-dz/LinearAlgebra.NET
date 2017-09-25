namespace MatrixAlgebraNET
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    
    /// <summary>
    /// A double-precision matrix class.
    /// </summary>
    public class Matrix
    {
        public Matrix(int numRows, int numCols)
        {
            this.values = new double[numRows, numCols];
            this.NumRows = numRows;
            this.NumCols = numCols;
            this.TotalSize = numRows * numCols;
        }
        
        public Matrix(double[,] values)
        {
            this.values = values;
            this.NumRows = values.GetLength(0);
            this.NumCols = values.GetLength(1);
            this.TotalSize = this.NumRows * this.NumCols;
        }
        
        public unsafe Matrix(double[] values, int numCols)
        {
            int numRows = Utils.RoundUp(values.Length, numCols) / numCols;
            this.values = new double[numRows, numCols];
            fixed (double *pThis = this.values) {
                for (int i = 0; i < values.Length; i++) {
                    pThis[i] = values[i];
                }
            }
            this.NumRows = numRows;
            this.NumCols = numCols;
            this.TotalSize = numRows * numCols;
        }
        
        public unsafe Matrix(int[] values, int numCols)
        {
            int numRows = Utils.RoundUp(values.Length, numCols) / numCols;
            this.values = new double[numRows, numCols];
            fixed (double *pThis = this.values) {
                for (int i = 0; i < values.Length; i++) {
                    pThis[i] = values[i];
                }
            }
            this.NumRows = numRows;
            this.NumCols = numCols;
            this.TotalSize = numRows * numCols;
        }
        
        public unsafe Matrix(int[,] values, int numCols)
        {
            int numRows = Utils.RoundUp(values.Length, numCols) / numCols;
            this.values = new double[numRows, numCols];
            fixed (double *pThis = this.values) {
                fixed (int *pValues = values) {
                    for (int i = 0; i < values.Length; i++) {
                        pThis[i] = pValues[i];
                    }
                }
            }
            this.NumRows = values.GetLength(0);
            this.NumCols = values.GetLength(1);
            this.TotalSize = this.NumRows * this.NumCols;
        }
        
        public double[,] values;
        
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public int TotalSize { get; private set; }
        public int ByteCount { get { return TotalSize * sizeof(double); } }
        
        /// <summary>
        /// An indexer for the matrix.
        /// </summary>
        public double this[int rowIndex, int colIndex] {
            get { return this.values[rowIndex, colIndex]; }
            set { this.values[rowIndex, colIndex] = value; }
        }
        
        // Provides a fixed-memory pointer to the underlying data.
        internal unsafe double *Data {
            get {
                fixed (double *p = this.values)
                    return p;
            }
        }
        
        public unsafe void AllOnes()
        {
            double *pThis = this.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pThis[i] = 1d;
            }
        }
        
        public unsafe void AllZeros()
        {
            double *pThis = this.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pThis[i] = 0d;
            }
        }
        
        /// <summary>
        /// Computes the product of the current matrix and another matrix.
        /// </summary>
        /// <param name="mat">A matrix by which to multiply the current matrix.</param>
        /// <returns>A matrix that is the product of the input matrix and the other matrix.</returns>
        public unsafe Matrix Dot(Matrix mat)
        {
            var result = new Matrix(this.NumRows, mat.NumCols);
            double *pResult = result.Data;
            double *pThis = this.Data;
            double *pMat = mat.Data;
            for (int y = 0; y < this.NumRows; y++) {
                int i = y * this.NumCols;
                for (int x = 0; x < mat.NumCols; x++) {
                    int j = x;
                    double sum = 0d;
                    for (int z = 0; z < this.NumCols; z++, j += mat.NumCols) {
                        sum += pThis[i + z] * pMat[j];
                    }
                    pResult[y * mat.NumCols + x] = sum;
                }
            }
            return result;
        }
        
        public unsafe void FillWithConstant(double val)
        {
            double *pThis = this.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pThis[i] = val;
            }
        }
        
        /// <summary>
        /// Fills the current matrix with (pseudo-)random values from the specified interval.
        /// </summary>
        /// <param name="minVal">The minimum value.</param>
        /// <param name="maxVal">The maximum value.</param>
        public unsafe void FillWithRandomValues(double minVal, double maxVal)
        {
            var rnd = new Random();
            double range = maxVal - minVal;
            double *pThis = this.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pThis[i] = rnd.NextDouble() * range + minVal;
            }
        }
        
        /// <summary>
        /// Transforms the current matrix into an identity one.
        /// </summary>
        public unsafe void Identity()
        {
            double *pThis = this.Data;
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    pThis[y * this.NumCols + x] = x == y ? 1f : 0d;
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
        
        public unsafe double Maximum()
        {
            double *pThis = this.Data;
            var maxVal = double.MinValue;
            for (int i = 0; i < this.TotalSize; i++) {
                if (pThis[i] > maxVal) {
                    maxVal = pThis[i];
                }
            }
            return maxVal;
        }
        
        public unsafe double Minimum()
        {
            double *pThis = this.Data;
            var minVal = double.MaxValue;
            for (int i = 0; i < this.TotalSize; i++) {
                if (pThis[i] < minVal) {
                    minVal = pThis[i];
                }
            }
            return minVal;
        }
        
        /// <summary>
        /// Finds the minimum and maximum values in the current matrix.
        /// This is potentially more efficient than calling Mininum() and Maximum() separately.
        /// </summary>
        /// <returns>The minimum and maximum values.</returns>
        public unsafe Tuple<double, double> MinMax()
        {
            double *pThis = this.Data;
            var minVal = double.MaxValue;
            var maxVal = double.MinValue;
            for (int i = 0; i < this.TotalSize; i++) {
                if (pThis[i] > maxVal) {
                    maxVal = pThis[i];
                }
                if (pThis[i] < minVal) {
                    minVal = pThis[i];
                }
            }
            return new Tuple<double, double>(minVal, maxVal);
        }
        
        public void PrintColumnToConsole(int colIndex, int padding = 18, int numDecimalPlaces = 15)
        {
            string format = Utils.ProvideStringFormat(numDecimalPlaces);
            for (int y = 0; y < this.NumRows - 1; y++) {
                Console.WriteLine("{0}", this[y, colIndex].ToString(format).PadLeft(padding, ' '));
            }
            Console.Write("{0}", this[this.NumRows - 1, colIndex].ToString(format).PadLeft(padding, ' '));
        }
        
        public void PrintRowToConsole(int rowIndex, int padding = 18, int numDecimalPlaces = 15)
        {
            string format = Utils.ProvideStringFormat(numDecimalPlaces);
            for (int x = 0; x < this.NumCols; x++) {
                Console.Write("{0}", this[rowIndex, x].ToString(format).PadLeft(padding, ' '));
            }
        }
        
        public void PrintToConsole(int padding = 18, int numDecimalPlaces = 15)
        {
            string format = Utils.ProvideStringFormat(numDecimalPlaces);
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    Console.Write("{0}", this[y, x].ToString(format).PadLeft(padding, ' '));
                }
                Console.WriteLine();
            }
        }
        
        public unsafe static Matrix ReadFromMat(string path)
        {
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open))) {
                if (new string(reader.ReadChars(6)) != "double") {
                    throw new IOException("The input matrix is not double-precision.");
                }
                var result = new Matrix(reader.ReadInt32(), reader.ReadInt32());
                double *pResult = result.Data;
                for (int i = 0; i < result.TotalSize; i++) {
                    pResult[i] = reader.ReadDouble();
                }
                return result;
            }
        }
        
        public Matrix Reshape(int numCols)
        {
            int numRows = Utils.RoundUp(this.TotalSize, numCols) / numCols;
            var result = new Matrix(numRows, numCols);
            Buffer.BlockCopy(this.values, 0, result.values, 0, this.ByteCount);
            return result;
        }
        
        /// <summary>
        /// Converts the current double-precision matrix to a single-precision one.
        /// </summary>
        /// <returns>A MatrixF object.</returns>
        public unsafe MatrixF ToMatrixF()
        {
            var result = new MatrixF(this.NumRows, this.NumCols);
            double *pThis = this.Data;
            float *pResult = result.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pResult[i] = (float)pThis[i];
            }
            return result;
        }
        
        public unsafe double Trace()
        {
            double sum = 0d;
            double *pThis = this.Data;
            for (int i = 0; i < this.NumRows; i++) {
                if (i < this.NumCols) {
                    sum += pThis[i * this.NumCols + i];
                }
            }
            return sum;
        }
        
        /// <summary>
        /// Computes the transpose of the current matrix.
        /// </summary>
        /// <returns>A matrix that is the transpose of the input matrix.</returns>
        public unsafe Matrix Transpose()
        {
            var result = new Matrix(this.NumCols, this.NumRows);
            double *pResult = result.Data;
            double *pThis= this.Data;
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    pResult[x * this.NumRows + y] = pThis[y * this.NumCols + x];
                }
            }
            return result;
        }
        
        public unsafe void WriteToMat(string path)
        {
            double *pThis = this.Data;
            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create))) {
                writer.Write("double".ToCharArray());
                writer.Write(this.NumRows);
                writer.Write(this.NumCols);
                for (int i = 0; i < this.TotalSize; i++) {
                    writer.Write(pThis[i]);
                }
            }
        }
        
        #region Operators
        public static implicit operator Matrix(MatrixF mat)
        {
            return mat.ToMatrix();
        }
                
        public unsafe static Matrix operator +(Matrix mat1, Matrix mat2)
        {
            var result = new Matrix(mat1.NumRows, mat1.NumCols);
            double *pResult = result.Data;
            double *pMat1 = mat1.Data;
            double *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] + pMat2[i];
            }
            return result;
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
            var result = new Matrix(mat.NumRows, mat.NumCols);
            double *pResult = result.Data;
            double *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] + val;
            }
            return result;
        }
        
        public unsafe static Matrix operator -(Matrix mat1, Matrix mat2)
        {
            var result = new Matrix(mat1.NumRows, mat1.NumCols);
            double *pResult = result.Data;
            double *pMat1 = mat1.Data;
            double *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] - pMat2[i];
            }
            return result;
        }
        
        public unsafe static Matrix operator -(Matrix mat, double val)
        {
            var result = new Matrix(mat.NumRows, mat.NumCols);
            double *pResult = result.Data;
            double *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] - val;
            }
            return result;
        }
        
        public unsafe static Matrix operator -(double val, Matrix mat)
        {
            var result = new Matrix(mat.NumRows, mat.NumCols);
            double *pResult = result.Data;
            double *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = val - pMat[i];
            }
            return result;
        }
        
        public unsafe static Matrix operator *(Matrix mat1, Matrix mat2)
        {
            var result = new Matrix(mat1.NumRows, mat1.NumCols);
            double *pResult = result.Data;
            double *pMat1 = mat1.Data;
            double *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] * pMat2[i];
            }
            return result;
        }
        
        public static Matrix operator *(Matrix mat, double val)
        {
            return MultiplyByValue(mat, val);
        }
        
        public static Matrix operator *(double val, Matrix mat)
        {
            return MultiplyByValue(mat, val);
        }
        
        private unsafe static Matrix MultiplyByValue(Matrix mat, double val)
        {
            var result = new Matrix(mat.NumRows, mat.NumCols);
            double *pResult = result.Data;
            double *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] * val;
            }
            return result;
        }
        
        public unsafe static Matrix operator /(Matrix mat1, Matrix mat2)
        {
            var result = new Matrix(mat1.NumRows, mat1.NumCols);
            double *pResult = result.Data;
            double *pMat1 = mat1.Data;
            double *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] / pMat2[i];
            }
            return result;
        }
        
        public unsafe static Matrix operator /(Matrix mat, double val)
        {
            var result = new Matrix(mat.NumRows, mat.NumCols);
            double *pResult = result.Data;
            double *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] / val;
            }
            return result;
        }
        
        public unsafe static Matrix operator /(double val, Matrix mat)
        {
            var result = new Matrix(mat.NumRows, mat.NumCols);
            double *pResult = result.Data;
            double *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = val / pMat[i];
            }
            return result;
        }
        #endregion
        
    }
}