using System;
using System.Text;
using System.Numerics;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

using System.IO;
using Syroot.BinaryData;

using Microsoft.Win32;

namespace GridBonusUnlocker;

public class SaveFile
{
    public static byte[] DecryptSaveFile(string path)
    {
        using var fs = File.Open(path, FileMode.Open);
        using var bs = new BinaryStream(fs);

        if (!VerifySaveFileHash(bs))
            throw new Exception("Hash did not match.");

        return Decryptor.DecryptFile(bs);
    }

    public static void SaveSaveFile(string path, byte[] data, int fullLen)
    {
        using var ms = new FileStream(path, FileMode.Create);
        using var bs = new BinaryStream(ms);
        bs.Position = 4;
        bs.WriteUInt32(0x1FD); // Version?
        bs.WriteUInt32(0x962AADB);
        bs.WriteUInt32(0);

        long rem = data.Length;

        using var aes = Decryptor.GetAESContext();
        using var cryptStream = new CryptoStream(bs, aes.CreateEncryptor(), CryptoStreamMode.Write);

        Span<byte> currentChunk = data;
        while (rem > 0)
        {
            int chunkSize = (int)Math.Min(0x400, rem);
            bs.WriteUInt32((uint)chunkSize);

            cryptStream.Write(currentChunk[..chunkSize]); // Cut to chunk size
            currentChunk = currentChunk[chunkSize..]; // Skip to next chunk data

            rem -= chunkSize;
        }

        // Do checksum
        bs.Position = 0x10;
        uint checksum = 0;
        while (!bs.IsEndOfStream())
            checksum += bs.ReadUInt32();
        bs.Position = 0;
        bs.WriteUInt32(checksum);


        bs.SetLength(fullLen);
    }

    public static bool VerifySaveFileHash(BinaryStream bs)
    {
        bs.Position = 0;
        uint hash = bs.ReadUInt32();

        bs.Position = 0x10;
        //int fSize = 0x100 + 0x400 + 0x100 + 0x100 + 0x24;
        //int fullSize = 1028 * ((fSize + 1023) >> 10) + 0x10;

        uint cHash = 0;
        for (int i = 1; i < (bs.Length - 0x10) / sizeof(uint); i++)
            cHash += bs.ReadUInt32();

        return hash == cHash;
    }
}

