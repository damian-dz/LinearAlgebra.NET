using System;

namespace Algebra
{
    public static class Utils
    {
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
