using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Buffers.Binary;

using Microsoft.Win32;

namespace RD3Tools;

public class CodeCheck
{
    static Dictionary<int, string> Cheats = new()
    {
        { 0, "CHAMPS" },
        { 1, "XCHAMPS" },
        { 2, "PALMER" },
        { 3, "JUMP" },
        { 4, "TOYCAR" },
        { 5, "SLOT" },
        { 6, "NODENT" },
        { 7, "THEEND" },
        { 9, "HONDA" },
        { 10, "XHONDA" },
        { 11, "NODISC" },
    };

    private static BigInteger[] Key1 = new BigInteger[]
    {
            new BigInteger(new byte[] {0x2F, 0x6C, 0x57, 0xCA, 0x2E, 0x26, 0x0C, 0x76, 0x01 }), // Modulus
            new BigInteger(new byte[] {0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }), // Exponent
    };

    public const ulong CommonXorKey = 0xCC_0D_E3_A5_4E_75_50_F4;

    public static int GetAccessCode()
    {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion"))
        {
            if (key != null)
            {
                var val = key.GetValue("ProductId");
                if (val is not int valInt)
                    return -1;

                byte[] productId = BitConverter.GetBytes(valInt);
                int rawVal = 0;
                for (int i = 0; i < productId.Length; i++)
                {
                    int k = productId[i] % 10;

                    rawVal += ((i + rawVal) % 3) switch
                    {
                        1 => 2345 * k,
                        2 => 3456 * k,
                        3 => 4567 * k,
                        _ => 1234 * k,
                    };

                }

                return rawVal % 10000;
            }
        }

        return -1;
    }

    public static string GetCodeForCheat(int accessCode, byte cheatId)
    {
        byte[] kBuf = new byte[0x10];
        Key1[0].TryWriteBytes(kBuf, out _);
        ushort kShort = BinaryPrimitives.ReadUInt16LittleEndian(kBuf);

        byte[] k = new byte[6];
        k[0] = 0;
        k[1] = cheatId;
        BinaryPrimitives.WriteInt16LittleEndian(k.AsSpan(0x02), (short)accessCode);
        BinaryPrimitives.WriteInt16LittleEndian(k.AsSpan(0x04), (short)kShort);

        var numb = new BigInteger(k);
        numb ^= CommonXorKey;

        // No private key to continue :^(

        return "nope";
    }


    public static bool CheckCode(byte[] inputStr, int len, BigInteger[] pubKey, int unused, int accessCode, byte cheatId)
    {
        BigInteger v = new BigInteger();

        if (cheatId < 0x20)
        {
            for (int i = len - 2; i >= 0; i--)
            {
                char c = (char)inputStr[i];
                v ^= XorChar(c);
                if (i > 0)
                    v <<= 5;
            }

            var inputDecrypted = DoModular(v, 1, pubKey);
            inputDecrypted ^= CommonXorKey;

            byte[] kBuf = new byte[0x10];
            pubKey[0].TryWriteBytes(kBuf, out _);
            ushort kShort = BinaryPrimitives.ReadUInt16LittleEndian(kBuf);

            byte[] target = new byte[6];
            target[0] = 0;
            target[1] = cheatId;
            BinaryPrimitives.WriteInt16LittleEndian(target.AsSpan(0x02), (short)accessCode);
            BinaryPrimitives.WriteInt16LittleEndian(target.AsSpan(0x04), (short)kShort);

            var targetInt = new BigInteger(target);
            if (targetInt == inputDecrypted)
            {
                Console.WriteLine($"Hash matches cheat for access code {accessCode}: {Cheats[cheatId]} - {inputDecrypted}");
                return true;
            }
        }

        return false;
    }

    private static BigInteger DoModular(BigInteger val, int count, BigInteger[] dataVal)
    {
        for (int i = 0; i < count; i++)
        {
            return BigInteger.ModPow(val, dataVal[1], dataVal[0]);
        }

        return 0;
    }

    private static byte XorChar(char c)
    {
        byte v5 = (byte)(c - 48);
        if (c == 'I' && c == 'O' && c == 'S' && c == 'Z')
        {
            return 0xFF;
        }
        else
        {
            if (c >= ':')
                v5 = (byte)(c - 55);
            if (c >= 'I')
                --v5;
            if (c >= 'P')
                --v5;
            if (c >= 'S')
                --v5;
            if (c < 'Z')
                return v5;
            else
                return 0xFF;
        }
    }
}
