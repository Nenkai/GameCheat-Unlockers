﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Buffers.Binary;
using System.Security.Cryptography;

using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace RD3Tools;

public class CodeCheck
{
    static Dictionary<byte, string> Cheats = new()
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

    public const ulong CommonXorKey = 0xCC_0D_E3_A5_4E_75_50_F4;

    public static int GetAccessCode(bool generateIfNeeded = false)
    {
        int val = Utils.GetCurrentProductId(generateIfNeeded);
        if (val == -1)
            return -1;

        byte[] productId = BitConverter.GetBytes(val);
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

    public static void GetCurrentCodes()
    {
        int accessCode = GetAccessCode(generateIfNeeded: true);
        if (accessCode == -1)
        {
            return;
        }

        Console.WriteLine($"Access Code: {accessCode}");
        foreach (var cheatInfo in Cheats)
        {
            string cheat = GetCodeForCheat(accessCode, cheatInfo.Key);
            Console.WriteLine($"{cheatInfo.Value}: {cheat}");
        }
    }

    public static string GetCodeForCheat(int accessCode, byte cheatId)
    {
        BigInteger p = new(KeyStore.RSAKeys["RD3_PC"].P);
        BigInteger q = new(KeyStore.RSAKeys["RD3_PC"].Q);

        BigInteger n = p * q;
        BigInteger phiOfN = (p - 1) * (q - 1);
        BigInteger d = Utils.ModInverse(new BigInteger(KeyStore.RSAKeys["RD3_PC"].Exponent), phiOfN);

        byte[] saltBuf = new byte[0x10];
        n.TryWriteBytes(saltBuf, out int written);
        ushort salt = BinaryPrimitives.ReadUInt16LittleEndian(saltBuf);

        byte[] k = new byte[6];
        k[0] = 0;
        k[1] = cheatId;
        BinaryPrimitives.WriteInt16LittleEndian(k.AsSpan(0x02), (short)accessCode);
        BinaryPrimitives.WriteInt16LittleEndian(k.AsSpan(0x04), (short)salt);

        var numb = new BigInteger(k);
        numb ^= CommonXorKey;

        BigInteger encNumber = BigInteger.ModPow(numb, d, n);
        string dec = "";
        while (encNumber > 0)
        {
            byte b = (byte)(encNumber & 0x1F);
            char ch = (char)(b + (byte)'0');
            if (ch >= ':')
                ch = (char)(b + (byte)'7');
            if (ch >= 'I')
                ch = (char)((byte)ch + 1);
            if (ch >= 'O')
                ch = (char)((byte)ch + 1);
            if (ch >= 'S')
                ch = (char)((byte)ch + 1);
            if (ch >= 'Z')
                ch = (char)((byte)ch + 1);

            dec += ch;
            encNumber >>= 5;
        }

        return dec;
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

            var inputDecrypted = Utils.DoModular(v, 1, pubKey);
            inputDecrypted ^= CommonXorKey;

            byte[] kBuf = new byte[0x10];
            pubKey[0].TryWriteBytes(kBuf, out _);
            ushort kShort = BinaryPrimitives.ReadUInt16LittleEndian(kBuf);

            byte[] target = new byte[6];
            target[0] = 0;
            target[1] = (byte)cheatId;
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
