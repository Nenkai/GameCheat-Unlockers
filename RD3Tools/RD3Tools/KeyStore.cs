using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RD3Tools;

public static class KeyStore
{
    // Shoutouts to codemasters for using absolutely tiny modulus numbers
    // Which makes factoring private primes so easy
    public static Dictionary<string, RSAParameters> RSAKeys = new()
    {
        {
            "RD3_PC", new RSAParameters()
            {
                Modulus = new byte[] { 0x2F, 0x6C, 0x57, 0xCA, 0x2E, 0x26, 0x0C, 0x76, 0x01 },
                Exponent = BitConverter.GetBytes(65537),
                P = BitConverter.GetBytes(800275543),
                Q = BitConverter.GetBytes(33679599593),
            }
        },

        {
            "RD3_XBOX", new RSAParameters()
            {
                Exponent = BitConverter.GetBytes(65537),
                P = BitConverter.GetBytes(975859499),
                Q = BitConverter.GetBytes(20674904099),            
            }
        },

        {
            "RD3_PS2", new RSAParameters()
            {
                Exponent = BitConverter.GetBytes(65537),
                P = BitConverter.GetBytes(755290271),
                Q = BitConverter.GetBytes(25307185187),
            }
        },
    };
}
