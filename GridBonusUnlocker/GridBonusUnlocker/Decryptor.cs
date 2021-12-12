using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Syroot.BinaryData;

namespace GridBonusUnlocker
{
    public class Decryptor
    {
        public const string EncryptedKey = "BF238E52208261B11FB50901E78E45AC4660153565F09295305484E1F05166EC";

        public static Aes GetAESContext()
        {
            // This would be used to generate an IV, but its not really used
            MTRandom rand = new MTRandom(0x12345678);
            char[] ivHex = new char[16];
            for (int i = 0; i < 16; i++)
            {
                int v = rand.getInt32(0, 15);
                if (v >= 10)
                    ivHex[i] = (char)(v + 0x37);
                else
                    ivHex[i] = (char)(v + 0x30);
            }
            //byte[] iv = StringToByteArrayFastest(new string(ivHex) + "0DF0ADBA0DF0ADBA"); Not used

            Aes aes = Aes.Create();
            byte[] key = StringToByteArrayFastest(EncryptedKey);

            aes.KeySize = 256;
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.Zeros;

            return aes;
        }

        public static byte[] DecryptFile(Stream stream)
        {
            using Aes aes = Decryptor.GetAESContext();
            ICryptoTransform decryptor = aes.CreateDecryptor();

            stream.Position = 0x10;

            using var ms = new MemoryStream();
            using CryptoStream csDecrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);

            byte[] chunkBuffer = new byte[0x400];
            while (true)
            {
                if (stream.IsEndOfStream())
                    break;

                uint chunkSize = stream.ReadUInt32();
                if (chunkSize > 0)
                {
                    csDecrypt.Read(chunkBuffer);
                    ms.Write(chunkBuffer);
                }
                else
                    break;
            }

            ms.Flush();
            return ms.ToArray();
        }

        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
