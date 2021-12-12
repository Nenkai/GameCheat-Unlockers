using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;

namespace GridBonusUnlocker
{
    public class UnlocksFile
    {
        private byte[] _file;

        public const int FullLength = 0x4050;

        public UnlocksFile(byte[] fileData)
        {
            _file = fileData;
        }

        public Dictionary<int, string> BonusMap = new()
        {
            { 1, "Unlock Race Day Events" },
            { 2, "Unlock No Damage" },
            { 3, "Unlock AI Driver" },
            { 4, "Unlock Speed Boost" },
            { 5, "Unlock Repulsor Field" },
            { 6, "Unlock All Touring Cars" },
            { 7, "Unlock All GT Cars" },
            { 8, "Unlock Prototype Cars" },
            { 9, "Unlock eBay Ford Mustang GT-R" },
            { 10, "Unlock All Muscle Cars (MUS59279)" },
            { 11, "Unlock All Drift Cars (TUN58396)" },
            { 12, "Unlock Micromania Pagani Zonda R (M38572343)" },
            { 13, "Unlock Play.com Aston Martin DB9 (P47203845)" },
            { 14, "Unlock Gamestation BMW 320si (G29782655)" },
            { 15, "Unlock Buchbinder Emotional Engineering BMW 320si (F93857372)" }
        };

        public byte[] GetData()
            => _file;

        public bool GetBonusState(int cheatId)
        {
            int maxId = BinaryPrimitives.ReadInt32LittleEndian(_file.AsSpan(0x274));
            if (cheatId > maxId)
                throw new IndexOutOfRangeException("CheatID out of range");

            int idx = cheatId - 1;
            return BinaryPrimitives.ReadInt32LittleEndian(_file.AsSpan(0x278 + (idx * 0x08) + sizeof(uint))) > 0;
        }

        public void SetBonusState(int cheatId, int state)
        {
            int maxId = BinaryPrimitives.ReadInt32LittleEndian(_file.AsSpan(0x274));
            if (cheatId > maxId)
                throw new IndexOutOfRangeException("CheatID out of range");

            int idx = cheatId - 1;
            BinaryPrimitives.WriteInt32LittleEndian(_file.AsSpan(0x278 + (idx * 0x08) + sizeof(uint)), state);
            BinaryPrimitives.WriteInt32LittleEndian(_file.AsSpan(0x678 + (idx * 0x08) + sizeof(uint)), state);
        }
    }
}
