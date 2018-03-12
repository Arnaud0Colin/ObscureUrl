using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecurityExtention.Test
{
    [TestClass]
    public class ObscureTest
    {

        static byte[] _XorKey = { 226, 245, 107, 211, 54, 158, 173, 18, 171, 24, 234, 236, 53, 154, 136, 24, 70, 35, 178, 102, 76, 9, 119, 152, 225, 150, 179, 38, 17, 109, 213, 87, 81, 177, 127, 6, 24, 22, 177, 95, 184, 143, 234, 239, 35, 5, 116, 76, 71, 114, 211, 54, 17, 161, 71, 164, 195, 211, 106, 6, 37, 202, 24, 29, 58, 211, 121, 117, 87, 24, 170, 108, 154, 64, 226, 127, 13, 244, 13, 49, 175, 108, 129, 127, 111, 62, 252, 29, 131, 208, 142, 168, 34, 184, 62, 150, 180, 127, 74, 76, 226, 245, 107, 211, 54, 158, 173, 18, 171, 24, 234, 236, 53, 154, 136, 24, 70, 35, 178, 102, 76, 9, 119, 152, 225, 150, 179, 38, 17, 109, 213, 87, 81, 177, 127, 6, 24, 22, 177, 95, 184, 143, 234, 239, 35, 5, 116, 76, 71, 114, 211, 54, 17, 161, 71, 164, 195, 211, 106, 6, 37, 202, 24, 29, 58, 211, 121, 117, 87, 24, 170, 108, 154, 64, 226, 127, 13, 244, 13, 49, 175, 108, 129, 127, 111, 62, 252, 29, 131, 208, 142, 168, 34, 184, 62, 150, 180, 127, 74, 76, 226, 245, 107, 211, 54, 158, 173, 18, 171, 24, 234, 236, 53, 154, 136, 24, 70, 35, 178, 102, 76, 9, 119, 152, 225, 150, 179, 38, 17, 109, 213, 87, 81, 177, 127, 6, 24, 22, 177, 95, 184, 143, 234, 239, 35, 5, 116, 76, 71, 114, 211, 54, 17, 161, 71, 164, 195, 211, 106, 6, 37, 202, 24, 29, 58, 211, 121, 117, 87, 24, 170, 108, 154, 64, 226, 127, 13, 244, 13, 49, 175, 108, 129, 127, 111, 62, 252, 29, 131, 208, 142, 168, 34, 184, 62, 150, 180, 127, 74, 76 };



        [TestMethod]
        public void TestCharge()
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10000; i++)
            {
                string Source = KeyGenerator.GetUniqueKey(10000);
                var zaz = new SecurityExtention.Obscure(_XorKey).Encoder(Source, SecurityExtention.ObscureStringMode.ASCII);
                string Result = new SecurityExtention.Obscure(_XorKey).Dec_String(zaz, SecurityExtention.ObscureStringMode.ASCII);

                Assert.AreEqual(Source, Result);
            }
            sw.Stop();

            Console.WriteLine($" - {sw.ElapsedTicks}");

        }


        [TestMethod]
        public void TestChargeParallel()
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.For(0, 1100000, Index =>
         {
             string Source = KeyGenerator.GetUniqueKey(509);
             var zaz = new SecurityExtention.Obscure(_XorKey).Encoder(Source, SecurityExtention.ObscureStringMode.ASCII);

            // Assert.IsFalse(string.IsNullOrWhiteSpace(zaz) || zaz.Length % 4 != 0);

                 string Result = new SecurityExtention.Obscure(_XorKey).Dec_String(zaz, SecurityExtention.ObscureStringMode.ASCII);

             Assert.AreEqual(Source, Result);
         }
            );
            sw.Stop();

            Console.WriteLine($" - {sw.ElapsedTicks}");

        }

      



        public class KeyGenerator
        {
            public static string GetUniqueKey(int maxSize)
            {
                char[] chars = new char[62];
                chars =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
                byte[] data = new byte[1];
                using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
                {
                    crypto.GetNonZeroBytes(data);
                    data = new byte[maxSize];
                    crypto.GetNonZeroBytes(data);
                }
                StringBuilder result = new StringBuilder(maxSize);
                foreach (byte b in data)
                {
                    result.Append(chars[b % (chars.Length)]);
                }
                return result.ToString();
            }
        }

    }
}
