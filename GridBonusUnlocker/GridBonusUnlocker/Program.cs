using System;
using System.Text;
using System.Numerics;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

using Syroot.BinaryData;

using Microsoft.Win32;

namespace GridBonusUnlocker;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("GridBonusUnlocker by Nenkai#9075");
        Console.WriteLine();

        if (args.Length != 1)
        {
            Console.WriteLine("   Input: GridBonusUnlocker <path to save directory>");
            Console.WriteLine(@"  Save directory is located at 'Documents\Codemasters\GRID\savegame'.");
            return;
        }

        if (!Directory.Exists(args[0]))
        {
            Console.WriteLine("Not a valid directory.");
            return;
        }

        var decryptedUnlockFilePath = Path.Combine(args[0], Scrambler.ScrambleString("Unlocks"));
        if (!File.Exists(decryptedUnlockFilePath))
        {
            Console.WriteLine($"Save directory is missing the Unlocks file ({Scrambler.ScrambleString("Unlocks")}), make sure this is a valid save game directory.");
            return;
        }

        var decryptedUnlockFile = SaveFile.DecryptSaveFile(decryptedUnlockFilePath);
        if (decryptedUnlockFile.Length < 0x800)
        {
            Console.WriteLine("Unlocks file is invalid or corrupted.");
            return;
        }

        UnlocksFile unlocks = new UnlocksFile(decryptedUnlockFile);

        while (true)
        {
            Console.WriteLine("Bonuses:");
            for (int i = 0; i < unlocks.BonusMap.Count; i++)
            {
                var state = unlocks.GetBonusState(unlocks.BonusMap.Keys.ElementAt(i));
                Console.WriteLine($"  {unlocks.BonusMap.Keys.ElementAt(i),2}. {unlocks.BonusMap.ElementAt(i).Value} [{(state ? "Enabled" : "Disabled")}]");
            }
            Console.WriteLine();
            Console.WriteLine("Enter Cheat ID to Enable/Unlock, X to save, Q to quit the program:");
            Console.Write(">");

            string? line = Console.ReadLine();

            if (int.TryParse(line, out int id) && unlocks.BonusMap.TryGetValue(id, out _))
            {
                var state = unlocks.GetBonusState(id);
                state = !state;
                unlocks.SetBonusState(id, state ? 2 : 0);
            }
            else if (line == "X")
                break;
            else if (line == "Q")
                return;

            Console.Clear();
        }

        Console.WriteLine();
        Console.WriteLine("Saving Unlocks file..");
        SaveFile.SaveSaveFile(decryptedUnlockFilePath, unlocks.GetData(), UnlocksFile.FullLength);
        Console.WriteLine("Done.");
    }
}