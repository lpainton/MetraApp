using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    /// <summary>
    /// Static class containing utilities for board operations.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// This method adds two bytes together and returns the result.
        /// Function is commutative.
        /// </summary>
        /// <param name="a">First byte to add.</param>
        /// <param name="b">Second byte to add.</param>
        /// <returns>The result of a+b</returns>
        public static byte AddBytes(byte a, byte b)
        {
            byte carry = (byte)(a & b);
            byte result = (byte)(a ^ b);
            
            while(!carry.Equals(0x00))
            {
                byte shiftedcarry = (byte)(carry << 1);
                carry = (byte)(result & shiftedcarry);
                result ^= shiftedcarry;
            }
            
            return result;
        }

        /*public static void TestConsoleWrite(bool testMode, string s)
        {
            if (testMode) Console.WriteLine(s);
        }*/
    }
}
