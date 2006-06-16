namespace Janrain.OpenId

import System
import System.Security.Cryptography
import System.Text
import Mono.Security.Cryptography

class CryptUtil:
    public static DEFAULT_GEN = (of byte: 2,)

    public static DEFAULT_MOD = (of byte:
        0, 220, 249, 58, 11, 136, 57, 114, 236, 14, 25, 152, 154, 197, 162,
        206, 49, 14, 29, 55, 113, 126, 141, 149, 113, 187, 118, 35, 115, 24,
        102, 230, 30, 247, 90, 46, 39, 137, 139, 5, 127, 152, 145, 194, 226,
        122, 99, 156, 63, 41, 182, 8, 20, 88, 28, 211, 178, 202, 57, 134, 210,
        104, 55, 5, 87, 125, 69, 194, 231, 229, 45, 200, 28, 122, 23, 24, 118,
        229, 206, 167, 75, 20, 72, 191, 223, 175, 24, 130, 142, 253, 37, 25,
        241, 78, 69, 227, 130, 102, 52, 175, 25, 73, 229, 181, 53, 204, 130,
        154, 72, 59, 138, 118, 34, 62, 93, 73, 10, 37, 127, 5, 189, 255, 22,
        242, 251, 34, 197, 131, 171)
    
    private static NONCE_LEN as uint = 8
    private static NONCE_CHARS = (
        array(byte, j for j as byte in range(97,123)) +
        array(byte, j for j as byte in range(65,91)) +
        array(byte, j for j as byte in range(48,58)))
    
    
    private static generator = RNGCryptoServiceProvider()
    private static base64Transform = ToBase64Transform()
    private static sha1 as SHA1 = SHA1CryptoServiceProvider()
    
    static def ToBase64String(inputBytes as (byte)):
        sb = StringBuilder()
        outputBytes as (byte) = array(byte, base64Transform.OutputBlockSize)
        # Initializie the offset size.
        inputOffset = 0
        # Iterate through inputBytes transforming by blockSize.
        inputBlockSize as int = base64Transform.InputBlockSize
        while (inputBytes.Length - inputOffset) > inputBlockSize:
            base64Transform.TransformBlock(
                inputBytes, inputOffset, inputBlockSize, outputBytes, 0)

            inputOffset += base64Transform.InputBlockSize
            sb.Append(Encoding.ASCII.GetString(
                    outputBytes, 0, base64Transform.OutputBlockSize))

        # Transform the final block of data.
        outputBytes = base64Transform.TransformFinalBlock(
            inputBytes, inputOffset, (inputBytes.Length - inputOffset))
        sb.Append(Encoding.ASCII.GetString(outputBytes, 0, outputBytes.Length))

        return sb.ToString()

    private static def EnsurePositive(inputBytes as (byte)):
        # XXX : if len < 1 : throw error
        i as int = Convert.ToInt32(inputBytes[0].ToString())
        if (i > 127):
            temp as (byte) = array(byte, (inputBytes.Length + 1))
            temp[0] = 0
            inputBytes.CopyTo(temp, 1)
            inputBytes = temp
        return inputBytes

    static def UnsignedToBase64(inputBytes as (byte)):
        return ToBase64String(EnsurePositive(inputBytes))

    private static def RandomSelection(tofill as (byte), choices as (byte)):
        # XXX: assert choices.Length < 257
        rand = array(byte, 1)
        for i in range(0, tofill.Length):
            generator.GetBytes(rand)
            tofill[i] = choices[(Convert.ToUInt32(rand[0]) % choices.Length)]

    static def CreateNonce():
        # Create a nonce for this openid exchange
        nonce as (byte) = array(byte, NONCE_LEN)
        RandomSelection(nonce, NONCE_CHARS)
        return ASCIIEncoding.ASCII.GetString(nonce)

    static def CreateDiffieHellman():
        return DiffieHellmanManaged(DEFAULT_MOD, DEFAULT_GEN, 1024)

    static def SHA1XorSecret(dh as DiffieHellman, keyEx as (byte),
                             encMacKey as (byte)):
        dhShared = dh.DecryptKeyExchange(keyEx)
        sha1DhShared = sha1.ComputeHash(EnsurePositive(dhShared))
        if sha1DhShared.Length != encMacKey.Length:
            raise ArgumentOutOfRangeException(
                "encMacKey's length is not 20 bytes: [${ToBase64String(encMacKey)}]")

        secret = array(byte, encMacKey.Length)
        for i in range(0, encMacKey.Length):
            secret[i] = cast(byte, (encMacKey[i] ^ sha1DhShared[i]))

        return secret

