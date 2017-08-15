using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LinearAlgebra
{
    public class Matrix
    {
        #region Constructors
        public Matrix(double[] array)
        {
            this.values = new double[1, array.Length];
            for (int i = 0; i < this.NumCols; i++) {
                this[0, i] = array[i];
            }
        }
        
        public unsafe Matrix(double[] array, int width)
        {
            int numRows = Utils.RoundUp(array.Length, width) / width;
            this.values = new double[numRows, width];
            fixed (double *pRes = this.values) {
                for (int i = 0; i < array.Length; i++) {
                    pRes[i] = array[i];
                }
            }
        }
        
        public Matrix(int[] array)
        {
            this.values = new double[1, array.Length];
            for (int i = 0; i < this.NumCols; i++) {
                this[0, i] = array[i];
            }
        }
        
        public unsafe Matrix(int[] array, int width)
        {
            int numRows = Utils.RoundUp(array.Length, width) / width;
            this.values = new double[numRows, width];
            fixed (double *pRes = this.values) {
                for (int i = 0; i < array.Length; i++) {
                    pRes[i] = array[i];
                }
            }
        }
        
        public Matrix(double[,] array)
        {
            this.values = array;
        }
        
        public unsafe Matrix(int[,] array)
        {
            this.values = new double[array.GetLength(0), array.GetLength(1)];
            int size = this.Size;
            fixed (double *pRes = this.values) {
                fixed (int *pArr = array) {
                    for (int i = 0; i < size; i++) {
                        pRes[i] = pArr[i];
                    }
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
        public double this[int row, int col] {
            get { return this.values[row, col]; }
            set { this.values[row, col] = value; }
        }
        
        #region Properties
        private readonly double[,] values;
        
        public int ByteCount { get { return values.Length * sizeof(double); } }
        public int NumCols { get { return values.GetLength(1); } }
        public int NumRows { get { return values.GetLength(0); } }
        public int Size { get { return values.Length; } }
        // Provides a fixed-memory pointer to the underlying data.
        internal unsafe double *Data {
            get {
                fixed (double *p = this.values)
                    return p;
            }
        }
        #endregion
        
        #region Methods
        public unsafe void AllOnes()
        {
            int size = this.Size;
            fixed (double *pRes = this.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = 1d;
                }
            }
        }
        
        public unsafe void AllZeros()
        {
            int size = this.Size;
            fixed (double *pRes = this.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = 0d;
                }
            }
        }
        
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
            fixed (double *pRes = result, pMat1 = this.values, pMat2 = mat.values) {
                for (int y = 0; y < numRows; y++) {
                    int i = y * numCols;
                    for (int x = 0; x < width; x++) {
                        int j = x;
                        double res = 0d;
                        for (int z = 0; z < numCols; z++, j += width) {
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
            fixed (double *pRes = this.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = rnd.NextDouble() * range + min;
                }
            }
        }
        
        public unsafe Matrix FlipHorizontally()
        {
            int numRows = this.NumRows;
            int numCols = this.NumCols;
            var result = new double[numRows, numCols];
            fixed (double *pRes = result, pMat = this.values) {
                for (int y = 0; y < numRows; y++) {
                    for (int x = 0; x < numCols; x++) {
                        pRes[y * numCols + x] = pMat[(y * numCols) + numCols - 1 - x];
                    }
                }
            }
            return new Matrix(result);
        }
        
        public unsafe Matrix FlipVertically()
        {
            int numRows = this.NumRows;
            int numCols = this.NumCols;
            var result = new double[numRows, numCols];
            fixed (double *pRes = result, pMat = this.values) {
                for (int y = 0; y < numRows; y++) {
                    for (int x = 0; x < numCols; x++) {
                        pRes[y * numCols + x] = pMat[(numRows - 1 - y) * numCols + x];
                    }
                }
            }
            return new Matrix(result);
        }
        
        public static Matrix FromCSV(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            string[] line = lines[0].Split(',');
            int numRows = lines.Length;
            int numCols = line.Length;
            var result = new double[numRows, numCols];
            for (int x = 0; x < numCols; x++) {
                result[0, x] = double.Parse(line[x]);
            }
            for (int y = 1; y < numRows; y++) {
                line = lines[y].Split(',');
                for (int x = 0; x < numCols; x++) {
                    result[y, x] = double.Parse(line[x]);
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix FromIndexedBitmap(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            var result = new double[height, width];
            var rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int padding = bmpData.Stride - width;
            var bmpPtr = (byte *)bmpData.Scan0;
            for (int y = 0; y < height; y++, bmpPtr += padding) {
                for (int x = 0; x < width; x++, bmpPtr++) {
                    result[y, x] = bmpPtr[0];
                }
            }
            bmp.UnlockBits(bmpData);
            return new Matrix(result);
        }
        
        public void Identity()
        {
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    this[y, x] = x == y ? 1 : 0d;
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
        
        public void LoadRowFromArray(int row, double[] data)
        {
            int length = data.Length;
            for (int i = 0; i < length; i++) {
                this[row, i] = data[i];
            }
        }
        
        public delegate double Method(double x);
        
        /// <summary>
        /// Iterates over the current matrix
        /// and applies the specified method to each of its elements.
        /// </summary>
        /// <param name="method">A method that accepts and returns a double.</param>
        /// <returns>The modified matrix.</returns>
        public unsafe Matrix LoopThroughElements(Method method)
        {
            int size = this.Size;
            var result = new double[this.NumRows, this.NumCols];
            fixed (double *pRes = result, pMat = this.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = method(pMat[i]);
                }
            }
            return new Matrix(result);
        }
        
        public void PrintToConsole(int padding = 19)
        {
            for (int y = 0; y < this.NumRows; y++) {
                for (int x = 0; x < this.NumCols; x++) {
                    Console.Write("{0} ", this[y, x].ToString().PadLeft(padding, ' '));
                }
                Console.WriteLine();
            }
        }
        
        /// <summary>
        /// Rotates the current matrix clockwise by 90 degrees.
        /// </summary>
        /// <returns>The rotated matrix.</returns>
        public Matrix RotateClockwise()
        {
            int numRows = this.NumRows;
            int numCols = this.NumCols;
            var result = new double[numCols, numRows];
            for (int y = numRows - 1; y >= 0; y--) {
                for (int x = 0; x < numCols; x++) {
                    result[x, numRows - 1 - y] = this[y, x];
                }
            }
            return new Matrix(result);
        }
        
        public double[,] ToArray()
        {
            return this.values;
        }
        
        public void ToCSV(string filename)
        {
            int numRows = this.NumRows;
            int numCols = this.NumCols;
            using (var writer = new StreamWriter(filename)) {
                string line = null;
                for (int x = 0; x < numCols; x++) {
                    line += this[0, x];
                    if (x != numCols - 1) {
                        line += ",";
                    }
                }
                writer.Write(line);
                line = null;
                for (int y = 1; y < numRows; y++) {
                    for (int x = 0; x < numCols; x++) {
                        line += this[y, x];
                        if (x != numCols - 1) {
                            line += ",";
                        }
                    }
                    writer.Write(Environment.NewLine);
                    writer.Write(line);
                    line = null;
                }
            }
        }
        
        public unsafe Bitmap ToGrayscaleBitmap()
        {
            int width = this.NumCols;
            int height = this.NumRows;
            var bmpGray = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bmpGray.LockBits(rect, ImageLockMode.WriteOnly, bmpGray.PixelFormat);
            int padding = bmpData.Stride - width;
            var bmpPtr = (byte *)bmpData.Scan0;
            fixed (double *pMat = this.values) {
                for (int y = 0; y < height; y++, bmpPtr += padding) {
                    for (int x = 0; x < width; x++, bmpPtr++) {
                        bmpPtr[0] = Convert.ToByte(pMat[y * width + x]);
                    }
                }
            }
            bmpGray.UnlockBits(bmpData);
            var palette = bmpGray.Palette;
            Color[] entries = palette.Entries;
            for (int i = 0; i < 256; i++) {
                entries[i] = Color.FromArgb(i, i, i);
            }
            bmpGray.Palette = palette;
            return bmpGray;
        }
        
        public double Trace()
        {
            double sum = 0d;
            for (int i = 0; i < this.NumRows; i++) {
                if (i < this.NumCols) {
                    sum += this[i, i];
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
            int numRows = this.NumRows;
            int numCols = this.NumCols;
            var result = new double[numCols, numRows];
            fixed (double *pRes = result, pMat = this.values) {
                for (int y = 0; y < numRows; y++) {
                    for (int x = 0; x < numCols; x++) {
                        pRes[x * numRows + y] = pMat[y * numCols + x];
                    }
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
            fixed (double *pRes = result, pMat1 = mat1.values, pMat2 = mat2.values) {
                for (int i = 0; i < size; i++) {
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
            fixed (double *pRes = result, pMat = mat.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = pMat[i] + val;
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator -(Matrix mat1, Matrix mat2)
        {
            int size = mat1.Size;
            var result = new double[mat1.NumRows, mat1.NumCols];
            fixed (double *pRes = result, pMat1 = mat1.values, pMat2 = mat2.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = pMat1[i] - pMat2[i];
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator -(Matrix mat, double val)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = pMat[i] - val;
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator -(double val, Matrix mat)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = val - pMat[i];
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator *(Matrix mat1, Matrix mat2)
        {
            int size = mat1.Size;
            var result = new double[mat1.NumRows, mat1.NumCols];
            fixed (double *pRes = result, pMat1 = mat1.values, pMat2 = mat2.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = pMat1[i] * pMat2[i];
                }
            }
            return new Matrix(result);
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
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = pMat[i] * val;
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator /(Matrix mat1, Matrix mat2)
        {
            int size = mat1.Size;
            var result = new double[mat1.NumRows, mat1.NumCols];
            fixed (double *pRes = result, pMat1 = mat1.values, pMat2 = mat2.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = pMat1[i] / pMat2[i];
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator /(Matrix mat, double val)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = pMat[i] / val;
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix operator /(double val, Matrix mat)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            fixed (double *pRes = result, pMat = mat.values) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = val / pMat[i];
                }
            }
            return new Matrix(result);
        }
        #endregion
    }
}