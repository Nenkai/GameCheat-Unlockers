using System;
using System.Text;
using System.Numerics;
using System.Buffers.Binary;

using Microsoft.Win32;

namespace RD3Tools;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("RD3Tools by Nenkai#9075");
        Console.WriteLine();
        if (args.Length > 1 && args[0].EndsWith(".sav"))
        {
            if (File.Exists(args[0]))
            {
                Console.WriteLine("File {args[0]} does not exist.");
                return;
            }

            var saveFile = RD3SaveFile.Load(args[0]);

            bool doSave = true;
            if (args.Contains("--unlock-bonuses"))
                doSave = saveFile.UnlockBonuses();

            if (doSave)
                saveFile.Save(args[0]);
        }
        else
        {
            Console.WriteLine("Arguments:");
            Console.WriteLine("  ");
            Console.WriteLine("RD3Tools <path_to_save_file> [--unlock-bonuses] (Will fix up a save checksum for editing, and optionally unlock bonuses.)");
        }

        /*
        byte[] buf = new byte[0x10];
        Encoding.ASCII.GetBytes("R4XM651GCWVJJ", buf);

        for (byte i = 0; i < 0x0C; i++)
            CheckCode(buf, 14, Key1, 1, 1027, i);

        GetCodeForCheat(1027, 7);
        */
    }

}
