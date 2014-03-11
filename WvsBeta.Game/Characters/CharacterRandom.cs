﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game {
	public class CharacterRandom {
		private uint[] mSeeds { get; set; }

		public CharacterRandom() {
			mSeeds = new uint[3];

            uint beginSeed = 0;
            if (Server.Instance == null)
            {
                beginSeed = (uint)new Random().Next();
            }
            else
            {
                beginSeed = Server.Instance.Randomizer.NextSeed();
            }
			ResetSeeds(beginSeed, beginSeed, beginSeed);
		}

		public void ResetSeeds(uint seed1, uint seed2, uint seed3) {
			mSeeds[0] = seed1 | 0x100000;
			mSeeds[1] = seed2 | 0x1000;
			mSeeds[2] = seed3 | 0x10;
		}

		public uint NextSeed() {
			mSeeds[0] = ((mSeeds[0] & 0xFFFFFFFE) << 12) ^ (((mSeeds[0] << 13) ^ mSeeds[0]) >> 19);
			mSeeds[1] = ((mSeeds[1] & 0xFFFFFFF8) << 4) ^ (((mSeeds[1] << 2) ^ mSeeds[1]) >> 25);
			mSeeds[2] = ((mSeeds[2] & 0xFFFFFFF0) << 17) ^ (((mSeeds[2] << 3) ^ mSeeds[2]) >> 11);

			return (mSeeds[0] ^ mSeeds[1] ^ mSeeds[2]);
		}

        public int NextSeedINT()
        {
            return Math.Abs((int)NextSeed());
        }

        public int ValueBetween(int min = 0, int max = int.MaxValue)
        {
            if (max == 0) return 0;
            int inval = NextSeedINT();
            inval %= max;
            if (inval < min) inval = min;
            return inval;
        }

		public void GenerateConnectPacket(Packet pw) {
			uint seed1 = NextSeed();
			uint seed2 = NextSeed();
			uint seed3 = NextSeed();

			ResetSeeds(seed1, seed2, seed3);
            pw.WriteUInt(mSeeds[0]);
            pw.WriteUInt(mSeeds[1]);
            pw.WriteUInt(mSeeds[2]);
            pw.WriteUInt(mSeeds[2]);
		}
	}
}
