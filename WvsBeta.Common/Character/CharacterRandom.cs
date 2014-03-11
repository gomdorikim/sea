using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game {
	public class CharacterRandom {
		private uint[] mSeeds { get; set; }

		public CharacterRandom() {
			mSeeds = new uint[3];
            uint beginSeed = Server.Instance.Randomizer.NextSeed();
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

		public void GenerateConnectPacket(Packet pw) {
			uint seed1 = NextSeed();
			uint seed2 = NextSeed();
			uint seed3 = NextSeed();

			ResetSeeds(seed1, seed2, seed3);
			/*
			pw.WriteUInt(seed1);
			pw.WriteUInt(seed2);
			pw.WriteUInt(seed3);
			*/
			pw.WriteHexString("24 65 E2 0D E7 B8 CE 45 C8 B4 EB 88"); // >_>
		}
	}
}
