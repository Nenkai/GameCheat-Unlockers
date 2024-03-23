using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RD3Tools;

public class Utils
{
    public static bool IsElevated()
    {
        return WindowsIdentity.GetCurrent().Owner
          .IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
    }

    public static int GetCurrentProductId(bool generateIfNeeded = false)
    {
        using RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion");
        if (key != null)
        {
            var val = key.GetValue("ProductId");
            if (val is not int)
            {
                if (generateIfNeeded)
                {
                    if (!Utils.IsElevated())
                    {
                        Console.WriteLine("Error:");
                        Console.WriteLine("-> Unlocking bonuses on newer Windows OS'es requires the tool to be run as Admin as the tool has to set the 'SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\ProductId' registry key" +
                            " so that the game can generate one access code, or else it will display '????' in the bonus menu.");
                        Console.WriteLine();
                        Console.WriteLine("Please start the tool with elevated permissions.");
                        return -1;
                    }

                    Console.WriteLine("Write 'SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\ProductId' registry key for allowing the game to generate an access code? (Y/N)");
                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    {
                        using (var key2 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion"))
                        {
                            key2.SetValue("ProductId", 1, RegistryValueKind.DWord); // Will generate an access code of '1234'
                            Console.WriteLine("Registry value 'SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion->ProductId' successfully set as '1'.");
                        }

                        return 1;
                    }
                    else
                        return -1;

                }
                else
                    return -1;
            }
            else
                return (int)val;
        }
        else
            return -1;

    }


    public static BigInteger DoModular(BigInteger val, int count, BigInteger[] dataVal)
    {
        for (int i = 0; i < count; i++)
        {
            return BigInteger.ModPow(val, dataVal[1], dataVal[0]);
        }

        return 0;
    }

    /// <summary>
    /// Calculates the modular multiplicative inverse of <paramref name="a"/> modulo <paramref name="m"/>
    /// using the extended Euclidean algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation comes from the pseudocode defining the inverse(a, n) function at
    /// https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm
    /// </remarks>
    public static BigInteger ModInverse(BigInteger a, BigInteger n)
    {
        BigInteger t = 0, nt = 1, r = n, nr = a;

        if (n < 0)
        {
            n = -n;
        }

        if (a < 0)
        {
            a = n - (-a % n);
        }

        while (nr != 0)
        {
            var quot = r / nr;

            var tmp = nt; nt = t - quot * nt; t = tmp;
            tmp = nr; nr = r - quot * nr; r = tmp;
        }

        if (r > 1) throw new ArgumentException(nameof(a) + " is not convertible.");
        if (t < 0) t = t + n;
        return t;
    }
}
