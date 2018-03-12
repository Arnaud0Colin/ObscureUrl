#region Copyright(c) 1998-2018, Microsoft 
/* Copyright(c) 1998-2018,  Microsoft
 * All rights reserved.
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityExtention
{
    public class MyRandom
    {
        //
        // Private Constants 
        //
        private const int MBIG = Int32.MaxValue;
        private const int MSEED = 161803398;
        private const int MZ = 0;


        //
        // Member Variables
        //
        private int inext;
        private int inextp;
        private int[] SeedArray = new int[56];

        //
        // Public Constants
        //

        //
        // Native Declarations
        //

        //
        // Constructors
        //

        public MyRandom()
          : this(Environment.TickCount)
        {
        }

        public unsafe MyRandom(int Seed)
        {

            int mj, mk;

            //Initialize our Seed array.
            //This algorithm comes from Numerical Recipes in C (2nd Ed.)
            int subtraction = (Seed == Int32.MinValue) ? Int32.MaxValue : Math.Abs(Seed);
            mj = MSEED - subtraction;
            SeedArray[55] = mj;
            mk = 1;

            fixed (int* inData = SeedArray)
            {
                int* inputPtr;

                for (int i = 1; i < 55; i++)
                {
                    inputPtr = inData + ((21 * i) % 55);
                    *(inputPtr) = mk;
                    mk = mj - mk;
                    if (mk < 0) mk += MBIG;
                    mj = *(inputPtr);
                }

                for (int k = 1; k < 5; k++)
                {
                    inputPtr = inData;
                    int* endInputPtr = inputPtr + 56;

                    int* CurrPtr;
                    int i = 1;
                    unchecked
                    {
                        while (true)
                        {
                            if (inputPtr >= endInputPtr)
                                goto _AllInputConsumed;

                            CurrPtr = inData + (1 + (i++ + 30) % 55);
                            *(inputPtr) -= *(CurrPtr);
                            if (*(inputPtr) < 0) *(inputPtr) += MBIG;

                            inputPtr++;
                        }
                    }
                    _AllInputConsumed:
                    ;
                }


            }

            inext = 0;
            inextp = 21;
            Seed = 1;
        }

        //
        // Package Private Methods
        //

        /*====================================Sample====================================
        **Action: Return a new random number [0..1) and reSeed the Seed array.
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        protected virtual double Sample()
        {
            //Including this division at the end gives us significantly improved
            //random number distribution.
            return (InternalSample() * (1.0 / MBIG));
        }

        private int InternalSample()
        {
            int retVal;
            int locINext = inext;
            int locINextp = inextp;

            if (++locINext >= 56) locINext = 1;
            if (++locINextp >= 56) locINextp = 1;

            retVal = SeedArray[locINext] - SeedArray[locINextp];

            if (retVal == MBIG) retVal--;
            if (retVal < 0) retVal += MBIG;

            SeedArray[locINext] = retVal;

            inext = locINext;
            inextp = locINextp;

            return retVal;
        }

        //
        // Public Instance Methods
        // 


        /*=====================================Next=====================================
        **Returns: An int [0..Int32.MaxValue)
        **Arguments: None
        **Exceptions: None.
        ==============================================================================*/
        public virtual int Next()
        {
            return InternalSample();
        }


        /*=====================================Next=====================================
        **Returns: An int [0..maxValue)
        **Arguments: maxValue -- One more than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                return 0;
            }

            return (int)(Sample() * maxValue);
        }

    }
}
