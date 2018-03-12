#region Copyright(c) 1998-2018, Arnaud Colin Licence GNU GPL version 3
/* Copyright(c) 1998-2018, Arnaud Colin
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ObscureUrl
{

    public enum ObscureStringMode
    {
        ASCII,
        Unicode,
        Utf_8
    }


    public class Obscure
    {
        private byte[] _XorKey;
        private const int SizeCrc = 1;
        private const int SizeKey = 1;
        private int Complement { get; set; } = 0;
        public bool TimeStamp { get; set; } = false;

        public byte Shift { get; set; } = 5;

        public Obscure(byte[] Key)
        {
            this._XorKey = Key;
        }

        public Obscure(byte[] Key, bool TimeStamp = false) : this(Key)
        {
            this.TimeStamp = TimeStamp;
        }

        public Obscure(byte[] Key, int Complement = 0, bool TimeStamp = false) : this(Key, TimeStamp)
        {
            this.Complement = Complement;
        }

        #region Encoder

        [System.Security.SecurityCritical]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe string Encoder(string source, ObscureStringMode mode = ObscureStringMode.Utf_8)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            Encoding enc;

            switch (mode)
            {
                case ObscureStringMode.ASCII:
                    enc = Encoding.GetEncoding("ASCII");
                    break;
                case ObscureStringMode.Unicode:
                    enc = Encoding.GetEncoding("Unicode");
                    break;
                case ObscureStringMode.Utf_8:
                default:
                    enc = Encoding.GetEncoding("UTF-8");
                    break;
            }

            return Encoder(enc.GetBytes(source));
        }

        //[System.Security.SecurityCritical]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static unsafe string Encoder(long id)
        //{
        //    return Encoder(BitConverter.GetBytes(id));
        //}

        [System.Security.SecurityCritical]
        public unsafe string Encoder(byte[] Source)
        {
            if (Source == null)
                return null;

            byte idx;

            switch (Shift)
            {
                case 0:
                    idx = (byte)(DateTime.UtcNow.Millisecond % byte.MaxValue);
                    break;
                case 1:
                    idx = (byte)(DateTime.UtcNow.Second % byte.MaxValue);
                    break;
                case 2:
                    idx = (byte)(DateTime.UtcNow.Minute % byte.MaxValue);
                    break;
                case 3:
                    idx = (byte)(DateTime.UtcNow.Hour % byte.MaxValue);
                    break;
                case 5:
                    idx = (byte)(DateTime.UtcNow.Day % byte.MaxValue);
                    break;
                case 6:
                    idx = (byte)(DateTime.UtcNow.Month % byte.MaxValue);
                    break;
                case 7:
                    idx = (byte)(DateTime.UtcNow.Year % byte.MaxValue);
                    break;
                default:
                    idx = (byte)(DateTime.UtcNow.Day % byte.MaxValue);
                    break;
            }


            byte[] bFlag;
            byte EndFlag;

            if (TimeStamp)
            {
                bFlag = BitConverter.GetBytes(DateTime.Now.Ticks);
                EndFlag = (byte)(bFlag.Length);
            }
            else
            {
                bFlag = new byte[1];
                EndFlag = 0;
            }

            int newtaille = Source.Length + EndFlag + 1 + Complement + SizeCrc + SizeKey;

            byte[] ArrSource = new byte[newtaille];

            fixed (byte* InData1 = new byte[] { (byte)(byte.MaxValue - EndFlag) })
            {
                fixed (byte* InData2 = bFlag)
                {
                    fixed (byte* InData3 = Source)
                    {
                        fixed (byte* outData = ArrSource)
                        {

                            byte*[] Inff = { InData1, InData2, InData3 };
                            byte*[] endff = { InData1 + 1 - 1, InData2 + EndFlag - 1, InData3 + Source.Length - 1 };

                            Encode(outData, newtaille, Inff, endff, 3, idx);
                        }
                    }
                }
            }

            return MyBase64.ToBase64String(ArrSource);
        }

        [System.Security.SecurityCritical]
        private unsafe void Encode(byte* outSortie, int outcount, byte*[] inData, byte*[] endData, int incount, byte? index = null)
        {
            byte* outPtr = outSortie;
            byte* endoutPtr = outPtr + outcount;

            int idIn = incount - 1;
            byte vcrc = 0;
            int idx = 1;


            //  *(outPtr++) = (Byte)(Taq << 2);

            for (int i = 0; i < Complement; i++)
                *(outPtr++) = (Byte)_XorKey[(idx + _XorKey.Length + i) % _XorKey.Length]; // (byte.MaxValue);

            if (index.HasValue)
            {
                *(outPtr) = (Byte)(index.Value);
                idx = index.Value;
            }
            else
            {
                *(outPtr) = (Byte)(idx);
            }
            outPtr++;

            long crc = 0;

            byte pk = _XorKey[idx++ % _XorKey.Length];
            byte btmp = 0;

            byte currCode = 0;

            unchecked
            {
                while (true)
                {
                    // break when done: 
                    if (idIn < 0)
                        goto _AllInputConsumed;

                    if (outPtr >= endoutPtr)
                        goto _Error;

                    if (endData[idIn] < inData[idIn])
                        idIn--;
                    else
                    {
                        currCode = (byte)(*(endData[idIn]--));
                        *(outPtr++) = btmp = (byte)(currCode ^ (pk ^ _XorKey[idx++ % _XorKey.Length]));
                        pk = btmp;
                        crc += (currCode + crc) ^ btmp;
                    }
                }
                _AllInputConsumed:
                *(outPtr) = vcrc = (byte)((crc % byte.MaxValue));

            }

            _Error:
            return;

        }

        #endregion


        #region Decoder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe string Dec_String(string str, ObscureStringMode mode = ObscureStringMode.Utf_8)
        {
            var Result = Decoder(str);
            if (Result == null)
                return null;


            Encoding enc;

            switch (mode)
            {
                case ObscureStringMode.ASCII:
                    enc = Encoding.GetEncoding("ASCII");
                    break;
                case ObscureStringMode.Unicode:
                    enc = Encoding.GetEncoding("Unicode");
                    break;
                case ObscureStringMode.Utf_8:
                default:
                    enc = Encoding.GetEncoding("UTF-8");
                    break;
            }

            return enc.GetString(Result, 0, Result.Length);
        }




        [System.Security.SecurityCritical]
        public unsafe byte[] Decoder(string str)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length % 4 != 0)
                return null;


            byte[] Source = null;

            try
            {
                Source = MyBase64.FromBase64String(str);
            }
            catch
            {
                return null;
            }

            if (Source == null)
                return null;

            var sizeSortie = Source.Length - (Complement + SizeCrc + SizeKey);

            if (sizeSortie <= 0)
                return null;

            Byte[] DecSource = new Byte[sizeSortie];

            fixed (byte* outdate = DecSource)
            {
                fixed (byte* inData = Source)
                {
                    if (!Decode(inData, Source.Length, outdate, sizeSortie))
                        return null;
                }
            }

            byte EndFlag = (byte)(byte.MaxValue ^ (byte)DecSource[0]); //.FirstOrDefault()
            if (EndFlag > DecSource.Length)
                return null;

            int count = DecSource.Length - (1 + EndFlag);
            byte[] dtime = new byte[EndFlag];
            byte[] Sortie = new byte[count];

            fixed (byte* outSortie = Sortie)
            {
                fixed (byte* outdtime = dtime)
                {
                    fixed (byte* inData = DecSource)
                    {
                        Filtre(inData, DecSource.Length, outSortie, count, outdtime, EndFlag);
                    }
                }
            }

            if (EndFlag == 8)
            {
                DateTime d = DateTime.FromBinary(BitConverter.ToInt64(dtime, 0));
            }

            return Sortie;
        }


        [System.Security.SecurityCritical]
        private unsafe bool Decode(Byte* startInputPtr, Int32 inputLength, Byte* startDestPtr, Int32 destLength, byte? index = null)
        {
            byte vcrc = 0;
            int idx = 1;

            Byte* destPtr = startDestPtr;

            Byte* inputPtr = startInputPtr;
            // Pointers to the end of input and output:
            Byte* endInputPtr = inputPtr + inputLength;
            Byte* endDestPtr = destPtr + destLength - 1;

            if (index.HasValue)
                idx = index.Value;
            else
                idx = *(startInputPtr + Complement);

            inputPtr = startInputPtr + Complement + 1;
            endInputPtr = inputPtr + inputLength - Complement - 2;
            vcrc = *(startInputPtr + inputLength - 1);

            long crc = 0;

            byte pk = _XorKey[idx++ % _XorKey.Length];
            byte btmp = 0;

            byte currCode = 0;

            unchecked
            {
                while (true)
                {
                    // break when done:
                    if (inputPtr >= endInputPtr)
                        goto _AllInputConsumed;

                    if (endDestPtr < destPtr)
                        goto _Error;

                    currCode = (byte)(*inputPtr);
                    inputPtr++;

                    *(endDestPtr--) = btmp = (byte)(currCode ^ (pk ^ _XorKey[idx++ % _XorKey.Length]));
                    pk = currCode;
                    crc += (btmp + crc) ^ currCode;
                }
            }

            _AllInputConsumed:
            return vcrc == (byte)((crc % byte.MaxValue));

            _Error:
            return false;
        }


        private static unsafe void Filtre(byte* inData, int incount, byte* outSortie, int outcount, byte* outdtime, byte dtimeCount)
        {
            byte* InPtr = inData + 1;
            byte* outPtr = outSortie;
            byte* endInPtr = InPtr + incount;
            byte* endoutPtr = outPtr + outcount;
            byte* timePtr = outdtime;
            byte* endTimePtr = timePtr + dtimeCount;

            unchecked
            {
                while (true)
                {
                    // break when done:
                    if (InPtr >= endInPtr)
                        goto _AllInputConsumed;

                    if (outPtr >= endoutPtr)
                        goto _AllInputConsumed;


                    if (timePtr < endTimePtr)
                    {
                        *(timePtr++) = *(InPtr++);
                    }
                    else
                    {
                        *(outPtr++) = *(InPtr++);
                    }

                }
            }

            _AllInputConsumed:
            return;
        }

        #endregion


        public bool SafeCalcul(byte[] source, byte sens, out byte[] Resultat, byte? index = null)
        {
            List<byte> ret = new List<byte>();

            byte vcrc = 0;
            int idx = 1;

            byte[] btbase;
            if (sens == 0)
            {
                btbase = source;
                for (int i = 1; i <= Complement; i++)
                    ret.Add((Byte)_XorKey[(idx + _XorKey.Length + i) % _XorKey.Length]);

                if (index.HasValue)
                {
                    ret.Add(index.Value);
                    idx = index.Value;
                }
                else
                {
                    ret.Add((byte)idx);
                }
            }
            else
            {

                if (source.Length < (2 + Complement))
                {
                    Resultat = null;
                    return false;
                }

                if (index.HasValue)
                    idx = index.Value;
                else
                    //idx = source.Skip(Complement).FirstOrDefault();
                    idx = source[Complement];

                //var mSource = source.Skip(Complement + 1);
                //btbase = mSource.Take(mSource.Count() - 1);
                btbase = new byte[source.Length - (Complement + 1) - 1];

                Array.Copy(source, Complement + 1, btbase, 0, btbase.Length);

                vcrc = source.LastOrDefault();
            }

            long crc = 0;

            byte pk = _XorKey[idx++ % _XorKey.Length];
            byte btmp = 0;

            foreach (var bt in btbase)
            {
                ret.Add(btmp = (byte)(bt ^ (pk ^ _XorKey[idx++ % _XorKey.Length])));
                if (sens == 0)
                {
                    pk = btmp;
                    crc += (bt + crc) ^ btmp;
                }
                else
                {
                    pk = bt;
                    crc += (btmp + crc) ^ bt;
                }
            }

            if (sens == 0)
                ret.Add(vcrc = (byte)((crc % byte.MaxValue)));


            Resultat = ret.ToArray();
            return vcrc == (byte)((crc % byte.MaxValue));
        }

    }
}

