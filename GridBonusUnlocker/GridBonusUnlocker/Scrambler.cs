using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridBonusUnlocker
{
    public class Scrambler
    {
        public const string Alphabet = "YGPNELSQZIK";

        // SystemFile -> QEHGIXXYKM
        // Profile -> NXDSMWW
        // eventSpeech -> CBTAXDHUDKR
        // voiceCounters -> TUXPINGKMBOPY
        // Unlocks -> STABGVK
        // career manager sdu name -> AGGRIC CZVKEKG WOM MIWC
        // rewards_save -> PKLNVOK_RIFC
        // Achievements -> YIWVIGWCDVDQ
        // RaceHistory -> PGRRLTKJNZI
        // ebay_manager_save -> CHPL_XSDZOOP_HNZP
        // netPresence -> LKICVPKUMKO
        // NetSaveData -> LKIFEGWTCIDY
        // VideoPlayedManager -> TOSRSADQXMNKGCNKPJ
        // inputData -> GTEHXOSJZ
        // TickerTape -> RORXICLQOM

        public static string ScrambleString(string str)
        {
            char[] newStr = str.ToUpper().ToCharArray();

            for (int i = 0; i < newStr.Length; i++)
            {
                if (newStr[i] > 'A' && newStr[i] < 'Z')
                {
                    newStr[i] = (char)((Alphabet[i % Alphabet.Length] + newStr[i] - 130) % 26 + 'A');
                }
            }

            return new string(newStr);
        }
    }
}
