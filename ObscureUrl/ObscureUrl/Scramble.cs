#region Copyright(c) 2016-2018, Arnaud Colin Licence GNU GPL version 3
/* Copyright(c) 2016-2018, Arnaud Colin
 * All rights reserved.
 *
 * Licence GNU GPL version 3
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *   -> Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *
 *   -> Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityExtention
{
    class Scramble
    {
        public unsafe static void Shuffle(byte[] toShuffle, int key)
        {
            fixed (byte* inData = toShuffle)
            {
                UnsafeShuffle(inData, toShuffle.Length, key);
            }
        }

        public unsafe static void DeShuffle(byte[] toShuffle, int key)
        {
            fixed (byte* inData = toShuffle)
            {
                UnsafeDeShuffle(inData, toShuffle.Length, key);
            }
        }

        public unsafe static void UnsafeDeShuffle(byte* inData, int size, int key)
        {
            var rand = new MyRandom(key);

            int curr = size - 1;

            int[] exchanges = new int[size];
            fixed (int* intData = exchanges)
            {
                int* intPtr = intData;
                int* endintPtr = intPtr + (size) - 1;

                unchecked
                {
                    while (true)
                    {
                        if (endintPtr < intPtr)
                            goto _AllIntConsumed;

                        int n = rand.Next(curr + 1);

                        *(endintPtr) = n;

                        endintPtr--;
                        curr--;

                    }
                    _AllIntConsumed:
                    ;
                }

                Byte* inputPtr = inData;
                Byte* endInputPtr = inputPtr + size;

                intPtr = intData;
                endintPtr = intPtr + size;

                Byte* CurrPtr;
                byte tmp;

                unchecked
                {
                    while (true)
                    {
                        if (intPtr >= endintPtr)
                            goto _AllInputConsumed;

                        CurrPtr = inData + *(intPtr);


                        if (CurrPtr == inputPtr)
                            goto _next;

                        tmp = *(CurrPtr);
                        *(CurrPtr) = *(inputPtr);
                        *(inputPtr) = tmp;

                        _next:
                        inputPtr++;
                        intPtr++;
                    }
                    _AllInputConsumed:
                    ;
                }

            }

        }
        public unsafe static void UnsafeShuffle(byte* inData, int size, int key)
        {
            var rand = new MyRandom(key);

            Byte* inputPtr = inData;
            Byte* endInputPtr = inputPtr + size - 1;

            Byte* CurrPtr;
            byte tmp;
            int curr = size - 1;

            unchecked
            {
                while (true)
                {
                    if (endInputPtr < inputPtr)
                        goto _AllInputConsumed;

                    int n = rand.Next(curr + 1);
                    CurrPtr = inputPtr + n;

                    tmp = *(endInputPtr);

                    *(endInputPtr) = *(CurrPtr);
                    *(CurrPtr) = tmp;

                    endInputPtr--;
                    curr--;

                }
                _AllInputConsumed:
                ;
            }
        }
    }
}
