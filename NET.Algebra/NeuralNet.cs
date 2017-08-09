using System;

namespace Algebra
{
    public class NeuralNet
    {
        public unsafe static Matrix SigmoidActivate(Matrix mat)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            double* pMat = mat.Data;
            fixed (double *pRes = result) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = 1 / (1 + Math.Exp(-pMat[i]));
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix SigmoidDerive(Matrix mat)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            double* pMat = mat.Data;
            fixed (double *pRes = result) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = pMat[i] * (1 - pMat[i]);
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix TanhActivate(Matrix mat)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            double* pMat = mat.Data;
            fixed (double *pRes = result) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = Math.Tanh(pMat[i]);
                }
            }
            return new Matrix(result);
        }
        
        public unsafe static Matrix TanhDerive(Matrix mat)
        {
            int size = mat.Size;
            var result = new double[mat.NumRows, mat.NumCols];
            double* pMat = mat.Data;
            fixed (double *pRes = result) {
                for (int i = 0; i < size; i++) {
                    pRes[i] = 1 - (pMat[i] * pMat[i]);
                }
            }
            return new Matrix(result);
        }
    }
}
