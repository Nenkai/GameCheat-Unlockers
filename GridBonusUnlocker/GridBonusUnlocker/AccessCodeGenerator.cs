using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

namespace GridBonusUnlocker
{
    public class AccessCodeGenerator
    {
        public static int GenerateAccessCode(string valueName = "GRiD")
        {
            using (var subKey = Registry.CurrentUser.OpenSubKey("Software\\Codemasters\\UUID"))
            {
                if (subKey is null)
                    return 10000;

                var val = subKey.GetValue(valueName);
                if (val is int uuid)
                {
                    return (uuid ^ 20085) % 65534;
                }
                else
                {
                    MTRandom r = new MTRandom(); // Normally based on time
                    int accessCode = r.getInt32(10000, 65534);

                    uuid = accessCode ^ 20085;
                    subKey.SetValue(valueName, uuid);

                    return accessCode;
                }
            }
        }
    }
}
