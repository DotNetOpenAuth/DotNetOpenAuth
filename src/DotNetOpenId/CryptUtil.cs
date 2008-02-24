using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;


namespace DotNetOpenId
{
    internal class CryptUtil
    {

        #region Member Variables

        public static byte[] DEFAULT_GEN = {2};
        public static byte[] DEFAULT_MOD = {0, 220, 249, 58, 11, 136, 57, 114, 236, 14, 25, 152, 154, 197, 162,
        206, 49, 14, 29, 55, 113, 126, 141, 149, 113, 187, 118, 35, 115, 24,
        102, 230, 30, 247, 90, 46, 39, 137, 139, 5, 127, 152, 145, 194, 226,
        122, 99, 156, 63, 41, 182, 8, 20, 88, 28, 211, 178, 202, 57, 134, 210,
        104, 55, 5, 87, 125, 69, 194, 231, 229, 45, 200, 28, 122, 23, 24, 118,
        229, 206, 167, 75, 20, 72, 191, 223, 175, 24, 130, 142, 253, 37, 25,
        241, 78, 69, 227, 130, 102, 52, 175, 25, 73, 229, 181, 53, 204, 130,
        154, 72, 59, 138, 118, 34, 62, 93, 73, 10, 37, 127, 5, 189, 255, 22,
        242, 251, 34, 197, 131, 171};

        private static uint NONCE_LEN = 8;
        private static byte[] NONCE_CHARS = {
            97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,
            115,116,117,118,119,120,121,122,123,65,66,67,68,69,70,71,72,73,74,75,76,
            77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,48,49,50,51,52,53,54,55,56,57,
            58};
        private static Random generator = new Random();
        private static ToBase64Transform base64Transform = new ToBase64Transform();
        private static SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

        #endregion

        #region Public Methods

        public static string ToBase64String(byte[] inputBytes)
        {
            StringBuilder sb = new StringBuilder();
            byte[] outputBytes = new byte[base64Transform.OutputBlockSize];
            int inputOffset = 0;
            int inputBlockSize = base64Transform.InputBlockSize;

            while ((inputBytes.Length - inputOffset) > inputBlockSize)
            {
                base64Transform.TransformBlock(inputBytes, inputOffset, inputBlockSize, outputBytes, 0);
                inputOffset += base64Transform.InputBlockSize;
                sb.Append(Encoding.ASCII.GetString(outputBytes, 0, base64Transform.OutputBlockSize));
            }

            outputBytes = base64Transform.TransformFinalBlock(
                inputBytes, inputOffset, (inputBytes.Length - inputOffset));
            sb.Append(Encoding.ASCII.GetString(outputBytes, 0, outputBytes.Length));

            return sb.ToString();
        }

        public static string UnsignedToBase64(byte[] inputBytes)
        {
            return ToBase64String(EnsurePositive(inputBytes));
        }

        public static string CreateNonce()
        {
            byte[] nonce = new byte[NONCE_LEN];
            RandomSelection(ref nonce, NONCE_CHARS);
            return ASCIIEncoding.ASCII.GetString(nonce);
        }

        public static DiffieHellman CreateDiffieHellman()
        {
            return new DiffieHellmanManaged(DEFAULT_MOD, DEFAULT_GEN, 1024);
        }

        public static byte[] SHA1XorSecret(DiffieHellman dh, byte[] keyEx, byte[] encMacKey)
        {
            byte[] dhShared = dh.DecryptKeyExchange(keyEx);
            byte[] sha1DhShared = sha1.ComputeHash(EnsurePositive(dhShared));
            if (sha1DhShared.Length != encMacKey.Length)
            {
                throw new ArgumentOutOfRangeException("encMacKey's length is not 20 bytes: " + ToBase64String(encMacKey));
            }

            byte[] secret = new byte[encMacKey.Length];
            for (int i = 0; i < encMacKey.Length; i++)
            {
                secret[i] = (byte) (encMacKey[i] ^ sha1DhShared[i]);
            }
            return secret;
        }

        #endregion

        #region Private Methods

        private static byte[] EnsurePositive(byte[] inputBytes)
        {
            if (inputBytes.Length == 0) throw new ArgumentException("Invalid input passed to EnsurePositive. Array must have something in it.", "inputBytes");

            int i = (int)inputBytes[0];
            if (i > 127)
            {
                byte[] temp = new byte[inputBytes.Length + 1];
                temp[0] = 0;
                inputBytes.CopyTo(temp, 1);
                inputBytes = temp;
            }
            return inputBytes;
        }

        private static void RandomSelection(ref byte[] tofill, byte[] choices)
        {
            if (choices.Length <= 0) throw new ArgumentException("Invalid input passed to RandomSelection. Array must have something in it.", "choices");

            byte[] rand = new byte[1];
            for (int i = 0; i < tofill.Length; i++)
            {
                generator.NextBytes(rand);
                tofill[i] = choices[(Convert.ToInt32(rand[0]) % choices.Length)];
            }
        }

        #endregion

    }
}
