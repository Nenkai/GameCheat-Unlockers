using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridBonusUnlocker
{
    public struct MTRandom
    {
        private const uint N = 624;
        private const uint M = 397;
        private const uint MATRIX_A = 0x9908B0DF;        // constant vector a 
        private const uint UPPER_MASK = 0x80000000;      // most significant w-r bits
        private const uint LOWER_MASK = 0X7FFFFFFF;      // least significant r bits

        private static uint[] mt = new uint[N + 1];
        private static uint mti = N + 1;           // mti==N+1 means mt[N] is not initialized

        public MTRandom(uint seed)
        {
            mt[0] = seed | 1;
            for (mti = 1; mti < N; mti++)
                mt[mti] = (69069 * mt[mti - 1]) & 0xffffffff;

        }

        public int getInt32(int min, int max)
        {
            if (min == max)
                return max;

            uint r = (uint)getInt32();
            return (int)(min + r % (uint)(max - min));
        }

        public int getInt32()
        {
            if (mti >= N)
                shift();

            uint y = mt[mti++];

            /* Tempering */
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680u;
            y ^= (y << 15) & 0xefc60000u;
            y ^= (y >> 18);

            return (int)y;
        }

        public float getFloat()
        {
            int val = getInt32();
            float v;
            if (val < 0)
                v = (val & 1 | ((uint)val >> 1)) + (val & 1 | ((uint)val >> 1));
            else
                v = (float)val;

            return v * (1.0f / uint.MaxValue);
        }

        private static ulong shift()
        {
            ulong[] mag01 = new ulong[2];
            mag01[0] = 0x0UL;
            mag01[1] = MATRIX_A;

            uint y;
            // generate N words at one time
            uint kk;

            for (kk = 0; kk < N - M; kk++)
            {
                y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                mt[kk] = mt[kk + M] ^ (y >> 1) ^ (uint)mag01[y & 0x1U];
            }
            for (; kk < N - 1; kk++)
            {
                y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                mt[kk] = mt[kk - 227] ^ (y >> 1) ^ (uint)mag01[y & 0x1U];
            }
            y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
            mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ (uint)mag01[y & 0x1U];

            mti = 0;
            return y;
        }
    }
}