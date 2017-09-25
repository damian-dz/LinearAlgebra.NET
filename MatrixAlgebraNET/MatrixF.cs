namespace MatrixAlgebraNET
{
    using System;
    using System.IO;
    
    /// <summary>
    /// A single-precision matrix class.
    /// </summary>
    public class MatrixF
    {
        public MatrixF(int numRows, int numCols)
        {
            this.values = new float[numRows, numCols];
            this.NumRows = numRows;
            this.NumCols = numCols;
            this.TotalSize = numRows * numCols;
        }
        
        public MatrixF(float[,] values)
        {
            this.values = values;
            this.NumRows = values.GetLength(0);
            this.NumCols = values.GetLength(1);
            this.TotalSize = this.NumRows * this.NumCols;
        }
        
        public unsafe MatrixF(float[] values, int numCols)
        {
            int numRows = Utils.RoundUp(values.Length, numCols) / numCols;
            this.values = new float[numRows, numCols];
            fixed (float *pThis = this.values) {
                for (int i = 0; i < values.Length; i++) {
                    pThis[i] = values[i];
                }
            }
            this.NumRows = numRows;
            this.NumCols = numCols;
            this.TotalSize = numRows * numCols;
        }
        
        public unsafe MatrixF(int[] values, int numCols)
        {
            int numRows = Utils.RoundUp(values.Length, numCols) / numCols;
            this.values = new float[numRows, numCols];
            fixed (float *pThis = this.values) {
                for (int i = 0; i < values.Length; i++) {
                    pThis[i] = values[i];
                }
            }
            this.NumRows = numRows;
            this.NumCols = numCols;
            this.TotalSize = numRows * numCols;
        }
        
        public unsafe MatrixF(int[,] values, int numCols)
        {
            int numRows = Utils.RoundUp(values.Length, numCols) / numCols;
            this.values = new float[numRows, numCols];
            fixed (float *pThis = this.values) {
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
        
        public float[,] values;
        
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public int TotalSize { get; private set; }
        public int ByteCount { get { return TotalSize * sizeof(float); } }
        
        /// <summary>
        /// An indexer for the matrix.
        /// </summary>
        public float this[int rowIndex, int colIndex] {
            get { return this.values[rowIndex, colIndex]; }
            set { this.values[rowIndex, colIndex] = value; }
        }
        
        // Provides a fixed-memory pointer to the underlying data.
        internal unsafe float *Data {
            get {
                fixed (float *p = this.values)
                    return p;
            }
        }
        
        public unsafe void AllOnes()
        {
            float *pThis = this.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pThis[i] = 1f;
            }
        }
        
        public unsafe void AllZeros()
        {
            float *pThis = this.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pThis[i] = 0f;
            }
        }
        
        /// <summary>
        /// Computes the product of the current matrix and another matrix.
        /// </summary>
        /// <param name="mat">A matrix by which to multiply the current matrix.</param>
        /// <returns>A matrix that is the product of the input matrix and the other matrix.</returns>
        public unsafe MatrixF Dot(MatrixF mat)
        {
            var result = new MatrixF(this.NumRows, mat.NumCols);
            float *pResult = result.Data;
            float *pThis = this.Data;
            float *pMat = mat.Data;
            for (int y = 0; y < this.NumRows; y++) {
                int i = y * this.NumCols;
                for (int x = 0; x < mat.NumCols; x++) {
                    int j = x;
                    float sum = 0f;
                    for (int z = 0; z < this.NumCols; z++, j += mat.NumCols) {
                        sum += pThis[i + z] * pMat[j];
                    }
                    pResult[y * mat.NumCols + x] = sum;
                }
            }
            return result;
        }
        
        public unsafe void FillWithConstant(float val)
        {
            float *pThis = this.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pThis[i] = val;
            }
        }
        
        /// <summary>
        /// Fills the current matrix with (pseudo-)random values from the specified interval.
        /// </summary>
        /// <param name="minVal">The minimum value.</param>
        /// <param name="maxVal">The maximum value.</param>
        public unsafe void FillWithRandomValues(float minVal, float maxVal)
        {
            var rnd = new Random();
            double range = maxVal - minVal;
            float *pThis = this.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pThis[i] = (float)(rnd.NextDouble() * range + minVal);
            }
        }
        
        /// <summary>
        /// Transforms the current matrix into an identity one.
        /// </summary>
        public unsafe void Identity()
        {
            float *pThis = this.Data;
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    pThis[y * this.NumCols + x] = x == y ? 1f : 0f;
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
        
        public unsafe float Maximum()
        {
            float *pThis = this.Data;
            var maxVal = float.MinValue;
            for (int i = 0; i < this.TotalSize; i++) {
                if (pThis[i] > maxVal) {
                    maxVal = pThis[i];
                }
            }
            return maxVal;
        }
        
        /// <summary>
        /// Finds the minimum and maximum values in the current matrix.
        /// This is potentially more efficient than calling Mininum() and Maximum() separately.
        /// </summary>
        /// <returns>The minimum and maximum values.</returns>
        public unsafe Tuple<float, float> MinMax()
        {
            float *pThis = this.Data;
            var minVal = float.MaxValue;
            var maxVal = float.MinValue;
            for (int i = 0; i < this.TotalSize; i++) {
                if (pThis[i] > maxVal) {
                    maxVal = pThis[i];
                }
                if (pThis[i] < minVal) {
                    minVal = pThis[i];
                }
            }
            return new Tuple<float, float>(minVal, maxVal);
        }
        
        public unsafe float Minimum()
        {
            float *pThis = this.Data;
            var minVal = float.MaxValue;
            for (int i = 0; i < this.TotalSize; i++) {
                if (pThis[i] < minVal) {
                    minVal = pThis[i];
                }
            }
            return minVal;
        }
        
        public void PrintColumnToConsole(int colIndex, int padding = 10, int numDecimalPlaces = 8)
        {
            string format = Utils.ProvideStringFormat(numDecimalPlaces);
            for (int y = 0; y < this.NumRows - 1; y++) {
                Console.WriteLine("{0}", this[y, colIndex].ToString(format).PadLeft(padding, ' '));
            }
            Console.Write("{0}", this[this.NumRows - 1, colIndex].ToString(format).PadLeft(padding, ' '));
        }
        
        public void PrintRowToConsole(int rowIndex, int padding = 10, int numDecimalPlaces = 8)
        {
            string format = Utils.ProvideStringFormat(numDecimalPlaces);
            for (int x = 0; x < this.NumCols; x++) {
                Console.Write("{0}", this[rowIndex, x].ToString(format).PadLeft(padding, ' '));
            }
        }
        
        public void PrintToConsole(int padding = 10, int numDecimalPlaces = 8)
        {
            string format = Utils.ProvideStringFormat(numDecimalPlaces);
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    Console.Write("{0}", this[y, x].ToString(format).PadLeft(padding, ' '));
                }
                Console.WriteLine();
            }
        }
        
        public unsafe static MatrixF ReadFromMat(string path)
        {
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open))) {
                if (new string(reader.ReadChars(6)) != "single") {
                    throw new IOException("The input matrix is not single-precision.");
                }
                var result = new MatrixF(reader.ReadInt32(), reader.ReadInt32());
                float *pResult = result.Data;
                for (int i = 0; i < result.TotalSize; i++) {
                    pResult[i] = reader.ReadSingle();
                }
                return result;
            }
        }
        
        public MatrixF Reshape(int numCols)
        {
            int numRows = Utils.RoundUp(this.TotalSize, numCols) / numCols;
            var result = new MatrixF(numRows, numCols);
            Buffer.BlockCopy(this.values, 0, result.values, 0, this.ByteCount);
            return result;
        }
        
        /// <summary>
        /// Converts the current single-precision matrix to a double-precision one.
        /// </summary>
        /// <returns>A Matrix object.</returns>
        public unsafe Matrix ToMatrix()
        {
            var result = new Matrix(this.NumRows, this.NumCols);
            float *pThis = this.Data;
            double *pResult = result.Data;
            for (int i = 0; i < this.TotalSize; i++) {
                pResult[i] = pThis[i];
            }
            return result;
        }
        
        public unsafe float Trace()
        {
            float sum = 0f;
            float *pThis = this.Data;
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
        public unsafe MatrixF Transpose()
        {
            var result = new MatrixF(this.NumCols, this.NumRows);
            float *pResult = result.Data;
            float *pThis= this.Data;
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    pResult[x * this.NumRows + y] = pThis[y * this.NumCols + x];
                }
            }
            return result;
        }
        
        public unsafe void WriteToMat(string path)
        {
            float *pThis = this.Data;
            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create))) {
                writer.Write("single".ToCharArray());
                writer.Write(this.NumRows);
                writer.Write(this.NumCols);
                for (int i = 0; i < this.TotalSize; i++) {
                    writer.Write(pThis[i]);
                }
            }
        }
        
        #region Operators
        public static explicit operator MatrixF(Matrix mat)
        {
            return mat.ToMatrixF();
        }
        
        public unsafe static MatrixF operator +(MatrixF mat1, MatrixF mat2)
        {
            var result = new MatrixF(mat1.NumRows, mat1.NumCols);
            float *pResult = result.Data;
            float *pMat1 = mat1.Data;
            float *pMat2 = mat2.Data;
            for (int i = 0; i < mat1.TotalSize; i++) {
                pResult[i] = pMat1[i] + pMat2[i];
            }
            return result;
        }
        
        public static MatrixF operator +(MatrixF mat, float val)
        {
            return AddValue(mat, val);
        }
        
        public static MatrixF operator +(float val, MatrixF mat)
        {
            return AddValue(mat, val);
        }
        
        private unsafe static MatrixF AddValue(MatrixF mat, float val)
        {
            var result = new MatrixF(mat.NumRows, mat.NumCols);
            float *pResult = result.Data;
            float *pMat = mat.Data;
            for (int i = 0; i < mat.TotalSize; i++) {
                pResult[i] = pMat[i] + val;
            }
            return result;
        }
        
        public unsafe static MatrixF operator -(MatrixF mat, float val)
        {
            var result = new MatrixF(mat.NumRows, mat.NumCols);
            float *pResult = result.Data;
            float *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] - val;
            }
            return result;
        }
        
        public unsafe static MatrixF operator -(float val, MatrixF mat)
        {
            var result = new MatrixF(mat.NumRows, mat.NumCols);
            float *pResult = result.Data;
            float *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = val - pMat[i];
            }
            return result;
        }
        
        public unsafe static MatrixF operator *(MatrixF mat1, MatrixF mat2)
        {
            var result = new MatrixF(mat1.NumRows, mat1.NumCols);
            float *pResult = result.Data;
            float *pMat1 = mat1.Data;
            float *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] * pMat2[i];
            }
            return result;
        }
        
        public static MatrixF operator *(MatrixF mat, float val)
        {
            return MultiplyByValue(mat, val);
        }
        
        public static MatrixF operator *(float val, MatrixF mat)
        {
            return MultiplyByValue(mat, val);
        }
        
        private unsafe static MatrixF MultiplyByValue(MatrixF mat, float val)
        {
            var result = new MatrixF(mat.NumRows, mat.NumCols);
            float *pResult = result.Data;
            float *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] * val;
            }
            return result;
        }
        
        public unsafe static MatrixF operator /(MatrixF mat1, MatrixF mat2)
        {
            var result = new MatrixF(mat1.NumRows, mat1.NumCols);
            float *pResult = result.Data;
            float *pMat1 = mat1.Data;
            float *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] / pMat2[i];
            }
            return result;
        }
        
        public unsafe static MatrixF operator /(MatrixF mat, float val)
        {
            var result = new MatrixF(mat.NumRows, mat.NumCols);
            float *pResult = result.Data;
            float *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] / val;
            }
            return result;
        }
        
        public unsafe static MatrixF operator /(float val, MatrixF mat)
        {
            var result = new MatrixF(mat.NumRows, mat.NumCols);
            float *pResult = result.Data;
            float *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = val / pMat[i];
            }
            return result;
        }
        #endregion
        
    }
}
