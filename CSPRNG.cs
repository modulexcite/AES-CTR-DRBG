using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Drbg_Test
{
    internal static class CSPRNG
    {
        #region Random Output Methods
        /// <summary>
        /// Get a random double
        /// </summary>
        /// <returns>Random double</returns>
        internal static double NextDouble()
        {
            return BitConverter.ToDouble(GetSeed16(), 0);
        }

        /// <summary>
        /// Get a random float
        /// </summary>
        /// <returns>Random double</returns>
        internal static double NextFloat()
        {
            return BitConverter.ToSingle(GetSeed16(), 0);
        }

        /// <summary>
        /// Get a random short integer
        /// </summary>
        /// <returns>Random Int16</returns>
        internal static Int16 NextInt16()
        {
            return BitConverter.ToInt16(GetSeed16(), 0);
        }

        /// <summary>
        /// Get a random short integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <returns>Random Int16</returns>
        internal static Int16 NextInt16(Int16 Maximum)
        {
            Int16 num = 0;
            while ((num = NextInt16()) > Maximum) { }
            return num;
        }

        /// <summary>
        /// Get a random short integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <param name="Minimum">Minimum value</param>
        /// <returns>Random Int16</returns>
        internal static Int16 NextInt16(Int16 Maximum, Int16 Minimum)
        {
            Int16 num = 0;
            while ((num = NextInt16()) > Maximum || num < Minimum) { }
            return num;
        }

        /// <summary>
        /// Get a random unsigned short integer
        /// </summary>
        /// <returns>Random UInt16</returns>
        internal static UInt16 NextUInt16()
        {
            return BitConverter.ToUInt16(GetSeed16(), 0);
        }

        /// <summary>
        /// Get a random unsigned short integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <returns>Random UInt16</returns>
        internal static UInt16 NextUInt16(UInt16 Maximum)
        {
            UInt16 num = 0;
            while ((num = NextUInt16()) > Maximum) { }
            return num;
        }

        /// <summary>
        /// Get a random unsigned short integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <param name="Minimum">Minimum value</param>
        /// <returns>Random UInt32</returns>
        internal static UInt16 NextUInt16(UInt16 Maximum, UInt16 Minimum)
        {
            UInt16 num = 0;
            while ((num = NextUInt16()) > Maximum || num < Minimum) { }
            return num;
        }

        /// <summary>
        /// Get a random 32bit integer
        /// </summary>
        /// <returns>Random Int32</returns>
        internal static Int32 NextInt32()
        {
            return BitConverter.ToInt32(GetSeed16(), 0);
        }

        /// <summary>
        /// Get a random integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <returns>Random Int32</returns>
        internal static Int32 NextInt32(Int32 Maximum)
        {
            Int32 num = 0;
            while ((num = NextInt32()) > Maximum) { }
            return num;
        }

        /// <summary>
        /// Get a random integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <param name="Minimum">Minimum value</param>
        /// <returns>Random Int32</returns>
        internal static Int32 NextInt32(Int32 Maximum, Int32 Minimum)
        {
            Int32 num = 0;
            while ((num = NextInt32()) > Maximum || num < Minimum) { }
            return num;
        }

        /// <summary>
        /// Get a random unsigned 32bit integer
        /// </summary>
        /// <returns>Random UInt32</returns>
        internal static UInt32 NextUInt32()
        {
            return BitConverter.ToUInt32(GetSeed16(), 0);
        }

        /// <summary>
        /// Get a random unsigned integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <returns>Random UInt32</returns>
        internal static UInt32 NextUInt32(UInt32 Maximum)
        {
            UInt32 num = 0;
            while ((num = NextUInt32()) > Maximum) { }
            return num;
        }

        /// <summary>
        /// Get a random unsigned integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <param name="Minimum">Minimum value</param>
        /// <returns>Random UInt32</returns>
        internal static UInt32 NextUInt32(UInt32 Maximum, UInt32 Minimum)
        {
            UInt32 num = 0;
            while ((num = NextUInt32()) > Maximum || num < Minimum) { }
            return num;
        }

        /// <summary>
        /// Get a random long integer
        /// </summary>
        /// <returns>Random Int64</returns>
        internal static Int64 NextInt64()
        {
            return BitConverter.ToInt64(GetSeed16(), 0);
        }

        /// <summary>
        /// Get a random long integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <returns>Random Int64</returns>
        internal static Int64 NextInt64(Int64 Maximum)
        {
            Int64 num = 0;
            while ((num = NextInt64()) > Maximum) { }
            return num;
        }

        /// <summary>
        /// Get a random long integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <param name="Minimum">Minimum value</param>
        /// <returns>Random Int64</returns>
        internal static Int64 NextInt64(Int64 Maximum, Int64 Minimum)
        {
            Int64 num = 0;
            while ((num = NextInt64()) > Maximum || num < Minimum) { }
            return num;
        }

        /// <summary>
        /// Get a random unsigned long integer
        /// </summary>
        /// <returns>Random UInt64</returns>
        internal static UInt64 NextUInt64()
        {
            return BitConverter.ToUInt64(GetSeed16(), 0);
        }

        /// <summary>
        /// Get a random unsigned long integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <returns>Random UInt64</returns>
        internal static UInt64 NextUInt64(UInt64 Maximum)
        {
            UInt64 num = 0;
            while ((num = NextUInt64()) > Maximum) { }
            return num;
        }

        /// <summary>
        /// Get a random unsigned long integer
        /// </summary>
        /// <param name="Maximum">Maximum value</param>
        /// <param name="Minimum">Minimum value</param>
        /// <returns>Random UInt64</returns>
        internal static UInt64 NextUInt64(UInt64 Maximum, UInt64 Minimum)
        {
            UInt64 num = 0;
            while ((num = NextUInt64()) > Maximum || num < Minimum) { }
            return num;
        }
        #endregion

        #region Seed Generators
        /// <summary>
        /// Get a 64 byte/512 bit seed
        /// </summary>
        /// <returns>Random seed [byte[]]</returns>
        internal static byte[] GetSeed64()
        {
            byte[] data = new byte[256];
            byte[] seed = new byte[64];

            using (RNGCryptoServiceProvider rngRandom = new RNGCryptoServiceProvider())
                rngRandom.GetBytes(data);

            // entropy extractor
            using (SHA512 shaHash = SHA512Managed.Create())
                Buffer.BlockCopy(shaHash.ComputeHash(data), 0, seed, 0, 64);

            return seed;
        }

        /// <summary>
        /// Get a 64 byte/512 bit seed, extra strength. 
        /// Double random feed(4x 128) are hashed, xor'd, then stacked
        /// </summary>
        /// <returns>Random seed [byte[]]</returns>
        internal static byte[] GetSeed64Xs()
        {
            byte[] data1 = new byte[128];
            byte[] data2 = new byte[128];
            byte[] data3 = new byte[128];
            byte[] data4 = new byte[128];
            byte[] seed = new byte[64];

            using (RNGCryptoServiceProvider rngRandom = new RNGCryptoServiceProvider())
            {
                // get the random seeds
                rngRandom.GetBytes(data1);
                rngRandom.GetBytes(data2);
                rngRandom.GetBytes(data3);
                rngRandom.GetBytes(data4);

                using (SHA256 shaHash = SHA256Managed.Create())
                {
                    // get the hash values
                    data1 = shaHash.ComputeHash(data1);
                    data2 = shaHash.ComputeHash(data2);
                    data3 = shaHash.ComputeHash(data3);
                    data4 = shaHash.ComputeHash(data4);
                }
            }

            // xor buffer 1 and 3
            for (int j = 0; j < 32; j++)
                data1[j] ^= data3[j];

            // xor buffer 2 and 4
            for (int j = 0; j < 32; j++)
                data2[j] ^= data4[j];

            // copy through
            Buffer.BlockCopy(data1, 0, seed, 0, 32);
            Buffer.BlockCopy(data2, 0, seed, 32, 32);

            return seed;
        }

        /// <summary>
        /// Get a 32 byte/256 bit seed
        /// </summary>
        /// <returns>Random seed [byte[]]</returns>
        internal static byte[] GetSeed32()
        {
            byte[] data = new byte[128];

            using (RNGCryptoServiceProvider rngRandom = new RNGCryptoServiceProvider())
                rngRandom.GetBytes(data);

            // entropy extractor
            using (SHA256 shaHash = SHA256Managed.Create())
                return shaHash.ComputeHash(data);
        }

        /// <summary>
        /// Get a 32 byte/256 bit seed, extra strength
        /// </summary>
        /// <returns>Random seed [byte[]]</returns>
        internal static byte[] GetSeed32Xs()
        {
            byte[] data1 = new byte[128];
            byte[] data2 = new byte[128];

            using (RNGCryptoServiceProvider rngRandom = new RNGCryptoServiceProvider())
            {
                // get the random seeds
                rngRandom.GetBytes(data1);
                rngRandom.GetBytes(data2);
            }

            using (SHA256 shaHash = SHA256Managed.Create())
            {
                // get the hash values
                data1 = shaHash.ComputeHash(data1);
                data2 = shaHash.ComputeHash(data2);
            }

            // xor buffer 1 and 2
            for (int j = 0; j < 32; j++)
                data1[j] ^= data2[j];

            // return
            return data1;
        }

        /// <summary>
        /// Get a 16 byte/128 bit seed
        /// </summary>
        /// <returns>Random seed [byte[]]</returns>
        internal static byte[] GetSeed16()
        {
            byte[] data = new byte[128];
            byte[] hash = new byte[32];
            byte[] result1 = new byte[16];
            byte[] result2 = new byte[16];

            using (RNGCryptoServiceProvider rngRandom = new RNGCryptoServiceProvider())
                rngRandom.GetBytes(data);

            // entropy extractor
            using (SHA256 shaHash = SHA256Managed.Create())
                hash = shaHash.ComputeHash(data);

            Buffer.BlockCopy(hash, 0, result1, 0, 16);
            Buffer.BlockCopy(hash, 16, result2, 0, 16);

            // xor the halves
            for (int j = 0; j < 16; j++)
                result1[j] ^= result2[j];

            return result1;
        }

        /// <summary>
        /// Get a 16 byte/128 bit seed, extra strength
        /// </summary>
        /// <returns>Random seed [byte[]]</returns>
        internal static byte[] GetSeed16Xs()
        {
            byte[] data1 = new byte[128];
            byte[] data2 = new byte[128];
            byte[] hash = new byte[32];
            byte[] result = new byte[16];
            byte[] result2 = new byte[16];

            using (RNGCryptoServiceProvider rngRandom = new RNGCryptoServiceProvider())
            {
                // get the random seeds
                rngRandom.GetBytes(data1);
                rngRandom.GetBytes(data2);
            }

            // xor buffer 1 and 2
            for (int j = 0; j < 128; j++)
                data1[j] ^= data2[j];

            // entropy extractor
            using (SHA256 shaHash = SHA256Managed.Create())
                hash = shaHash.ComputeHash(data1);

            Buffer.BlockCopy(hash, 0, result, 0, 16);
            Buffer.BlockCopy(hash, 16, result2, 0, 16);

            // xor the halves
            for (int j = 0; j < 16; j++)
                result[j] ^= result2[j];

            return result;
        }
        #endregion
    }
}
