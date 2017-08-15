using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace LinearAlgebra
{
    public static class Utils
    {
        public unsafe static double[] BitmapToFloatingPoint(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            var gray = new double[width * height];
            var rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int padding = bmpData.Stride - width;
            var bmpPtr = (byte*)bmpData.Scan0;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    gray[y * width + x] = bmpPtr[0] / 255d;
                    bmpPtr++;
                }
                bmpPtr += padding;
            }
            bmp.UnlockBits(bmpData);
            return gray;
        }
        
        public static double DegToRad(double deg)
        {
            return deg / 180 * Math.PI;
        }
    
        public static double RadToDeg(double rad)
        {
            return rad * 180 / Math.PI;
        }
        
        public static int RoundUp(int numToRound, int multiple)
        {
            if (multiple == 0) {
                return numToRound;
            }
            int remainder = numToRound % multiple;
            if (remainder == 0) {
                return numToRound;
            }
            return numToRound + multiple - remainder;
        }
    }
}
