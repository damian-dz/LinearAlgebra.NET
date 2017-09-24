namespace MatrixAlgebraNET
{
    using System;
    
    internal static class Utils
    {
        internal static string ProvideStringFormat(int numDecimalPlaces)
        {
            string format = "0.";
            for (int i = 0; i < numDecimalPlaces; i++) {
                format += "#";
            }
            return format;
        }
        
        internal static int RoundUp(int numToRound, int multiple)
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
