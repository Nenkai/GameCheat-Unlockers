using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Buffers.Binary;
using System.Threading.Tasks;
using Syroot.BinaryData;

using Microsoft.Win32;
using System.Security.Principal;

namespace RD3Tools;

public class RD3SaveFile
{
    public DateTime Date { get; set; }

    public string ProfileName { get; set; }

    public float PercentCompletion { get; set; }

    public uint AccessCode { get; set; }

    public byte[] ProfileData { get; set; }

    public uint ProfileSize { get; set; }

    public uint UnkCount { get; set; }

    public uint UnkLast { get; set; }

    /*  0x01 - Unlock Championships (CHAMPS - 0)
        0x02 - Unlock Bonus Championships (XCHAMPS - 1)
        0x04 - Boost For All Cars (PALMER - 2)
        0x08 - Turbo Boost (JUMP - 3)
        0x10 - Toy Cars (TOYCAR - 4)
        0x20 - Unlock Slot Racer (SLOT - 5)
        0x40 - Invincible Cars (NODENT - 6)
        0x80 - Unlock Cutscenes (THEEND - 7)
        0x100 - Nothing
        0x200 - Unlock Honda (HONDA - 9)
        0x400 - Unlock Honda 2006 (XHONDA - 10)
        0x800 - No Streamed Car Sound (NODISC - 11)
    */

    public static RD3SaveFile Load(string file)
    {
        using var fs = File.Open(file, FileMode.Open);
        using var bs = new BinaryStream(fs);

        if (bs.ReadString(4) != "SGF2")
            throw new InvalidDataException("Not a Save game file.");

        RD3SaveFile save = new RD3SaveFile();

        uint sectorCount = bs.ReadUInt32();
        uint headerCrc = bs.ReadUInt32();
        bs.ReadUInt32();
        save.UnkCount = bs.ReadUInt32();

        int day = bs.ReadInt32();
        int month = bs.ReadInt32();
        int year = bs.ReadInt32();
        int hour = bs.ReadInt32();
        int minute = bs.ReadInt32();
        int second = bs.ReadInt32();
        save.Date = new DateTime(year, month, day, hour, minute, second);

        save.ProfileSize = bs.ReadUInt32();
        uint profileCrc = bs.ReadUInt32();
        save.ProfileName = bs.ReadString(64).TrimEnd('\0');
        save.PercentCompletion = bs.ReadSingle();
        save.AccessCode = bs.ReadUInt32();
        save.UnkLast = bs.ReadUInt32();

        Console.WriteLine($"Loaded save: {save.ProfileName} ({save.PercentCompletion}%) [{save.Date}] Access Code: {save.AccessCode}");

        bs.Position = 0x0C;
        uint inputHeaderCrc = CRC32.CRC32_0x04C11DB7(bs.ReadBytes(0x74), unchecked((uint)-1));
        if (inputHeaderCrc != headerCrc)
        {
            Console.WriteLine("Warning: Header CRC does not match, it will be fixed on next save");
        }

        save.ProfileData = bs.ReadBytes(0x25800);
        uint inputProfileCrc = CRC32.CRC32_0x04C11DB7(save.ProfileData.AsSpan(), unchecked((uint)-1));
        if (inputProfileCrc != profileCrc)
        {
            Console.WriteLine("Warning: Profile CRC does not match, it will be fixed on next save");
        }


        return save;
    }

    public bool UnlockBonuses()
    {
        Console.WriteLine("Attempting to unlock bonuses...");
        bool keyExists = false;
        bool regKeyCreated = false;

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion"))
        {
            if (key != null)
            {
                var val = key.GetValue("ProductId");
                if (val is int valInt)
                    keyExists = true;
            }
        }

        if (!keyExists)
        {
            if (!IsElevated())
            {
                Console.WriteLine("Error:");
                Console.WriteLine(" Unlocking bonuses on newer Windows OS'es requires the tool to be run as Admin as the tool has to set the 'SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\ProductId' registry key" +
                    " so that the game can generate one access code, or else it will display '????' in the bonus menu. Please start the tool as Administrator.");
                return false;
            }

            Console.WriteLine("Write 'SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\ProductId' registry key for allowing the game to generate an access code? (Y/N)");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion"))
                {
                    key.SetValue("ProductId", 1, RegistryValueKind.DWord); // Will generate an access code of '1234'
                    Console.WriteLine("Registry value 'SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion->ProductId' successfully set as '1'.");
                }

                regKeyCreated = true;
            }
            else
                return false;
        }

        if (regKeyCreated)
        {
            AccessCode = 1234;
            BinaryPrimitives.WriteUInt32LittleEndian(ProfileData.AsSpan(0x19C), 1234); // Access code again
        }

        BinaryPrimitives.WriteUInt32LittleEndian(ProfileData.AsSpan(0x194), 0xFFFFFFFF); // Cheat bonuses flags - 0xFF06 normally
        return true;
    }

    private static bool IsElevated()
    {
        return WindowsIdentity.GetCurrent().Owner
          .IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
    }

    public void Save(string path)
    {
        Console.WriteLine("Writing save & fixing checksums...");

        using var fs = File.Open(path, FileMode.Create);
        using var bs = new BinaryStream(fs);
        bs.WriteString("SGF2", StringCoding.Raw);
        bs.WriteUInt32(0x4C);
        bs.Position += 8;
        bs.WriteUInt32(UnkCount);
        bs.WriteInt32(Date.Day);
        bs.WriteInt32(Date.Month);
        bs.WriteInt32(Date.Year);
        bs.WriteInt32(Date.Hour);
        bs.WriteInt32(Date.Minute);
        bs.WriteInt32(Date.Second);
        bs.WriteUInt32(ProfileSize);
        bs.WriteUInt32(0);

        long pos = bs.Position;
        bs.WriteString(ProfileName, StringCoding.Raw);
        bs.Position = pos + 64;
        bs.WriteSingle(PercentCompletion);
        bs.WriteUInt32(AccessCode);
        bs.WriteUInt32(UnkLast);
        bs.WriteBytes(ProfileData);
        bs.Flush();

        // Write checksums, or else the save is rejected as "DAMAGED"
        uint profileCrc = CRC32.CRC32_0x04C11DB7(ProfileData, unchecked((uint)-1));
        bs.Position = 0x30;
        bs.WriteUInt32(profileCrc);

        bs.Position = 0x0C;
        uint headerCrc = CRC32.CRC32_0x04C11DB7(bs.ReadBytes(0x74), unchecked((uint)-1));
        bs.Position = 8;
        bs.WriteUInt32(headerCrc);

        Console.WriteLine("Done saving.");
    }
}
