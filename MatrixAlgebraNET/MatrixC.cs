namespace MatrixAlgebraNET
{
    using System;
    
    public struct Complex
    {
        public double Real;
        public double Imag;

        public Complex(double real)
        {
            this.Real = real;
            this.Imag = 0d;
        }

        public Complex(double real, double imag)
        {
            this.Real = real;
            this.Imag = imag;
        }
        
        public Complex Conjugate()
        {
            return new Complex(this.Real, -this.Imag);
        }
        
        public double Modulus()
        {
            return Math.Sqrt(this.Real * this.Real + this.Imag * this.Imag);
        }
        
        public static implicit operator Complex(double x)
        {
            return new Complex(x, 0);
        }
        
        public static implicit operator Complex(ComplexF x)
        {
            return new Complex(x.Real, x.Imag);
        }
        
        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Real + b.Real, a.Imag + b.Imag);
        }

        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Real - b.Real, a.Imag - b.Imag);
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.Real * b.Real - a.Imag * b.Imag,
                               a.Real * b.Imag + a.Imag * b.Real);
        }

        public static Complex operator /(Complex a, Complex b)
        {
            double divisor = b.Real * b.Real + b.Imag * b.Imag;
            return new Complex((a.Real * b.Real + a.Imag * b.Imag) / divisor,
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
    /// A double-precision matrix that supports complex number aritmetic.
    /// </summary>
    public class MatrixC
    {
        public MatrixC(int numRows, int numCols)
        {
            this.values = new Complex[numRows, numCols];
            this.NumRows = numRows;
            this.NumCols = numCols;
            this.TotalSize = numRows * numCols;
        }
        
        public MatrixC(Complex[,] values)
        {
            this.values = values;
            this.NumRows = values.GetLength(0);
            this.NumCols = values.GetLength(1);
            this.TotalSize = this.NumRows * this.NumCols;
        }
        
        public Complex[,] values;
        
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public int TotalSize { get; private set; }
        public int ByteCount { get { return TotalSize * sizeof(double) * 2; } }
        
        /// <summary>
        /// An indexer for the matrix.
        /// </summary>
        public Complex this[int rowIndex, int colIndex] {
            get { return this.values[rowIndex, colIndex]; }
            set { this.values[rowIndex, colIndex] = value; }
        }
        
        // Provides a fixed-memory pointer to the underlying data.
        internal unsafe Complex *Data {
            get {
                fixed (Complex *p = this.values)
                    return p;
            }
        }
        
        public unsafe void AllOnes()
        {
            Complex *pThis = this.Data;
            var pThisDouble = (double *)pThis;
            int complexSize = this.TotalSize * 2;
            for (int i = 0; i < complexSize; i += 2) {
                pThisDouble[i] = 1d;
            }
        }
        
        /// <summary>
        /// Computes the product of the current matrix and another matrix.
        /// </summary>
        /// <param name="mat">A matrix by which to multiply the current matrix.</param>
        /// <returns>A matrix that is the product of the input matrix and the other matrix.</returns>
        public unsafe MatrixC Dot(MatrixC mat)
        {
            var result = new MatrixC(this.NumRows, mat.NumCols);
            Complex *pResult = result.Data;
            Complex *pThis = this.Data;
            Complex *pMat = mat.Data;
            for (int y = 0; y < this.NumRows; y++) {
                int i = y * this.NumCols;
                for (int x = 0; x < mat.NumCols; x++) {
                    int j = x;
                    var sum = new Complex();
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
        public unsafe void FillWithRandomValues(double minReal, double maxReal, double minImag, double maxImag)
        {
            var rnd = new Random();
            double rangeReal = maxReal - minReal;
            double rangeImag = maxImag - minImag;
            Complex *pThis = this.Data;
            var pThisDouble = (double *)pThis;
            int complexSize = this.TotalSize * 2;
            for (int i = 0; i < complexSize; i += 2) {
                pThisDouble[i] = rnd.NextDouble() * rangeReal + minReal;
                pThisDouble[i + 1] = rnd.NextDouble() * rangeImag + minImag;
            }
        }
        
        public void PrintToConsole(int padding = 41, int numDecimalPlaces = 15)
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
        public unsafe static MatrixC operator +(MatrixC mat1, MatrixC mat2)
        {
            var result = new MatrixC(mat1.NumRows, mat1.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat1 = mat1.Data;
            Complex *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] + pMat2[i];
            }
            return result;
        }
        
        public static MatrixC operator +(MatrixC mat, Complex val)
        {
            return AddValue(mat, val);
        }
        
        public static MatrixC operator +(Complex val, MatrixC mat)
        {
            return AddValue(mat, val);
        }
        
        private unsafe static MatrixC AddValue(MatrixC mat, Complex val)
        {
            var result = new MatrixC(mat.NumRows, mat.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] + val;
            }
            return result;
        }
        
        public unsafe static MatrixC operator -(MatrixC mat1, MatrixC mat2)
        {
            var result = new MatrixC(mat1.NumRows, mat1.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat1 = mat1.Data;
            Complex *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] - pMat2[i];
            }
            return result;
        }
        
        public unsafe static MatrixC operator -(MatrixC mat, Complex val)
        {
            var result = new MatrixC(mat.NumRows, mat.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] - val;
            }
            return result;
        }
        
        public unsafe static MatrixC operator -(Complex val, MatrixC mat)
        {
            var result = new MatrixC(mat.NumRows, mat.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = val - pMat[i];
            }
            return result;
        }
        
        public unsafe static MatrixC operator *(MatrixC mat1, MatrixC mat2)
        {
            var result = new MatrixC(mat1.NumRows, mat1.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat1 = mat1.Data;
            Complex *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] * pMat2[i];
            }
            return result;
        }
        
        public static MatrixC operator *(MatrixC mat, Complex val)
        {
            return MultiplyByValue(mat, val);
        }
        
        public static MatrixC operator *(Complex val, MatrixC mat)
        {
            return MultiplyByValue(mat, val);
        }
        
        private unsafe static MatrixC MultiplyByValue(MatrixC mat, Complex val)
        {
            var result = new MatrixC(mat.NumRows, mat.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] * val;
            }
            return result;
        }
        
        public unsafe static MatrixC operator /(MatrixC mat1, MatrixC mat2)
        {
            var result = new MatrixC(mat1.NumRows, mat1.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat1 = mat1.Data;
            Complex *pMat2 = mat2.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat1[i] / pMat2[i];
            }
            return result;
        }
        
        public unsafe static MatrixC operator /(MatrixC mat, Complex val)
        {
            var result = new MatrixC(mat.NumRows, mat.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = pMat[i] / val;
            }
            return result;
        }
        
        public unsafe static MatrixC operator /(Complex val, MatrixC mat)
        {
            var result = new MatrixC(mat.NumRows, mat.NumCols);
            Complex *pResult = result.Data;
            Complex *pMat = mat.Data;
            for (int i = 0; i < result.TotalSize; i++) {
                pResult[i] = val / pMat[i];
            }
            return result;
        }
        #endregion
        
    }
}
