namespace MatrixAlgebraNET
{
    using System;
    
    public struct ComplexF
    {
        public float Real;
        public float Imag;

        public ComplexF(float real)
        {
            this.Real = real;
            this.Imag = 0f;
        }

        public ComplexF(float real, float imag)
        {
            this.Real = real;
            this.Imag = imag;
        }
        
        public ComplexF Conjugate()
        {
            return new ComplexF(this.Real, -this.Imag);
        }
        
        public float Modulus()
        {
            return (float)Math.Sqrt(this.Real * this.Real + this.Imag * this.Imag);
        }
        
        public static implicit operator ComplexF(float x)
        {
            return new ComplexF(x, 0);
        }
        
        public static explicit operator ComplexF(Complex x)
        {
            return new ComplexF((float)x.Real, (float)x.Imag);
        }
        
        public static ComplexF operator +(ComplexF a, ComplexF b)
        {
            return new ComplexF(a.Real + b.Real, a.Imag + b.Imag);
        }

        public static ComplexF operator -(ComplexF a, ComplexF b)
        {
            return new ComplexF(a.Real - b.Real, a.Imag - b.Imag);
        }

        public static ComplexF operator *(ComplexF a, ComplexF b)
        {
            return new ComplexF(a.Real * b.Real - a.Imag * b.Imag,
                               a.Real * b.Imag + a.Imag * b.Real);
        }

        public static ComplexF operator /(ComplexF a, ComplexF b)
        {
            float divisor = b.Real * b.Real + b.Imag * b.Imag;
            return new ComplexF((a.Real * b.Real + a.Imag * b.Imag) / divisor,
                               (a.Imag * b.Real - a.Real * b.Imag) / divisor);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}i)", this.Real, this.Imag);
        }
        
        public string ToString(string format)
        {
            return string.Format("({0}, {1}i)", 
                                 this.Real.ToString(format), this.Imag.ToString(format));
        }
    }
    
    /// <summary>
    /// A singleprecision matrix that supports ComplexF number aritmetic.
    /// </summary>
    public class MatrixCF
    {
        public MatrixCF(int numRows, int numCols)
        {
            this.values = new ComplexF[numRows, numCols];
            this.NumRows = numRows;
            this.NumCols = numCols;
            this.TotalSize = numRows * numCols;
        }
        
        public MatrixCF(ComplexF[,] values)
        {
            this.values = values;
            this.NumRows = values.GetLength(0);
            this.NumCols = values.GetLength(1);
            this.TotalSize = this.NumRows * this.NumCols;
        }
        
        public ComplexF[,] values;
        
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public int TotalSize { get; private set; }
        public int ByteCount { get { return TotalSize * sizeof(float) * 2; } }
        
        /// <summary>
        /// An indexer for the matrix.
        /// </summary>
        public ComplexF this[int rowIndex, int colIndex] {
            get { return this.values[rowIndex, colIndex]; }
            set { this.values[rowIndex, colIndex] = value; }
        }
        
        // Provides a fixed-memory pointer to the underlying data.
        internal unsafe ComplexF *Data {
            get {
                fixed (ComplexF *p = this.values)
                    return p;
            }
        }
        
        public unsafe void AllOnes()
        {
            ComplexF *pThis = this.Data;
            var pThisfloat = (float *)pThis;
            int ComplexFSize = this.TotalSize * 2;
            for (int i = 0; i < ComplexFSize; i += 2) {
                pThisfloat[i] = 1f;
            }
        }
        
        /// <summary>
        /// Computes the product of the current matrix and another matrix.
        /// </summary>
        /// <param name="mat">A matrix by which to multiply the current matrix.</param>
        /// <returns>A matrix that is the product of the input matrix and the other matrix.</returns>
        public unsafe MatrixCF Dot(MatrixCF mat)
        {
            var result = new MatrixCF(this.NumRows, mat.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pThis = this.Data;
            ComplexF *pMat = mat.Data;
            for (int y = 0; y < this.NumRows; y++) {
                int i = y * this.NumCols;
                for (int x = 0; x < mat.NumCols; x++) {
                    int j = x;
                    var sum = new ComplexF();
                    for (int z = 0; z < this.NumCols; z++, j += mat.NumCols) {
                        sum += pThis[i + z] * pMat[j];
                    }
                    pResult[y * mat.NumCols + x] = sum;
                }
            }
            return result;
        }
        
        /// <summary>
        /// Fills the current matrix with (pseudo-)random values from the specified interval.
        /// </summary>
        /// <param name="minReal">The minimum real value.</param>
        /// <param name="maxReal">The maximum real value.</param>
        /// <param name = "minImag">The minimum imaginary value.</param>
        /// <param name = "maxImag">The maximum imaginary value.</param>
        public unsafe void FillWithRandomValues(float minReal, float maxReal, float minImag, float maxImag)
        {
            var rnd = new Random();
            float rangeReal = maxReal - minReal;
            float rangeImag = maxImag - minImag;
            ComplexF *pThis = this.Data;
            var pThisfloat = (float *)pThis;
            int ComplexFSize = this.TotalSize * 2;
            for (int i = 0; i < ComplexFSize; i += 2) {
                pThisfloat[i] = (float)(rnd.NextDouble() * rangeReal + minReal);
                pThisfloat[i + 1] = (float)(rnd.NextDouble() * rangeImag + minImag);
            }
        }
        
        public void PrintToConsole(int padding = 25, int numDecimalPlaces = 8)
        {
            string format = Utils.ProvideStringFormat(numDecimalPlaces);
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    Console.Write("{0}", this[y, x].ToString(format).PadLeft(padding, ' '));
                }
                Console.WriteLine();
            }
        }
        
        #region Operators
        public unsafe static MatrixCF operator +(MatrixCF mat1, MatrixCF mat2)
        {
            var result = new MatrixCF(mat1.NumRows, mat1.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat1 = mat1.Data;
            ComplexF *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] + pMat2[i];
            }
            return result;
        }
        
        public static MatrixCF operator +(MatrixCF mat, ComplexF val)
        {
            return AddValue(mat, val);
        }
        
        public static MatrixCF operator +(ComplexF val, MatrixCF mat)
        {
            return AddValue(mat, val);
        }
        
        private unsafe static MatrixCF AddValue(MatrixCF mat, ComplexF val)
        {
            var result = new MatrixCF(mat.NumRows, mat.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] + val;
            }
            return result;
        }
        
        public unsafe static MatrixCF operator -(MatrixCF mat1, MatrixCF mat2)
        {
            var result = new MatrixCF(mat1.NumRows, mat1.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat1 = mat1.Data;
            ComplexF *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] - pMat2[i];
            }
            return result;
        }
        
        public unsafe static MatrixCF operator -(MatrixCF mat, ComplexF val)
        {
            var result = new MatrixCF(mat.NumRows, mat.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] - val;
            }
            return result;
        }
        
        public unsafe static MatrixCF operator -(ComplexF val, MatrixCF mat)
        {
            var result = new MatrixCF(mat.NumRows, mat.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = val - pMat[i];
            }
            return result;
        }
        
        public unsafe static MatrixCF operator *(MatrixCF mat1, MatrixCF mat2)
        {
            var result = new MatrixCF(mat1.NumRows, mat1.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat1 = mat1.Data;
            ComplexF *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] * pMat2[i];
            }
            return result;
        }
        
        public static MatrixCF operator *(MatrixCF mat, ComplexF val)
        {
            return MultiplyByValue(mat, val);
        }
        
        public static MatrixCF operator *(ComplexF val, MatrixCF mat)
        {
            return MultiplyByValue(mat, val);
        }
        
        private unsafe static MatrixCF MultiplyByValue(MatrixCF mat, ComplexF val)
        {
            var result = new MatrixCF(mat.NumRows, mat.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] * val;
            }
            return result;
        }
        
        public unsafe static MatrixCF operator /(MatrixCF mat1, MatrixCF mat2)
        {
            var result = new MatrixCF(mat1.NumRows, mat1.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat1 = mat1.Data;
            ComplexF *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] / pMat2[i];
            }
            return result;
        }
        
        public unsafe static MatrixCF operator /(MatrixCF mat, ComplexF val)
        {
            var result = new MatrixCF(mat.NumRows, mat.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] / val;
            }
            return result;
        }
        
        public unsafe static MatrixCF operator /(ComplexF val, MatrixCF mat)
        {
            var result = new MatrixCF(mat.NumRows, mat.NumCols);
            ComplexF *pResult = result.Data;
            ComplexF *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = val / pMat[i];
            }
            return result;
        }
        #endregion
        
    }
}
