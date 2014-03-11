using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Shop {
	public partial class Character {
		public void SetJob(short value) {
			mPrimaryStats.Job = value;
		}

		public void SetEXP(int value) {
			mPrimaryStats.EXP = value;
		}

		public void SetHP(short value, bool maxHPToo) {
			mPrimaryStats.HP = value;
			if (maxHPToo) {
				mPrimaryStats.MaxHP = value;
			}
		}

		public void ModifyHP(short value, bool sendPacket) {
			if (mPrimaryStats.HP + value < 0) {
				mPrimaryStats.HP = 0;
			}
			else if (mPrimaryStats.HP + value > mPrimaryStats.GetMaxHP(false)) {
				mPrimaryStats.HP = mPrimaryStats.GetMaxHP(false);
			}
			else {
				mPrimaryStats.HP += value;
			}
		}

		public void SetMP(short value, bool maxMPToo) {
			mPrimaryStats.MP = value;
			if (maxMPToo) {
				mPrimaryStats.MaxMP = value;
			}
		}

		public void ModifyMP(short value, bool sendPacket) {
			if (mPrimaryStats.MP + value < 0) {
				mPrimaryStats.MP = 0;
			}
			else if (mPrimaryStats.MP + value > mPrimaryStats.GetMaxMP(false)) {
				mPrimaryStats.MP = mPrimaryStats.GetMaxMP(false);
			}
			else {
				mPrimaryStats.MP += value;
			}
		}

		public void ModifyMaxMP(short value, bool sendPacket) {
			mPrimaryStats.MaxMP = (short)(((mPrimaryStats.MaxMP + value) > Constants.MaxMaxMp) ? Constants.MaxMaxMp : (mPrimaryStats.MaxMP + value));
		}

		public void ModifyMaxHP(short value, bool sendPacket) {
			mPrimaryStats.MaxHP = (short)(((mPrimaryStats.MaxHP + value) > Constants.MaxMaxHp) ? Constants.MaxMaxHp : (mPrimaryStats.MaxHP + value));
		}

		public void SetMaxHP(short value) {
			mPrimaryStats.MaxHP = value;
		}

		public void SetMaxMP(short value) {
			mPrimaryStats.MaxMP = value;
		}

		public void SetLevel(byte value) {
			mPrimaryStats.Level = value;
			Save();
		}

		public void AddFame(short value) {
			if (mPrimaryStats.Fame + value > short.MaxValue) {
				SetFame(short.MaxValue);
			}
			else if (mPrimaryStats.Fame + value < 0) {
				SetFame(0);
			}
			else {
				SetFame((short)(mPrimaryStats.Fame + value));
			}
		}

		public void SetFame(short value) {
			mPrimaryStats.Fame = value;
		}

		public void AddEXP(uint value) {
			int amount = (int)(value > int.MaxValue ? int.MaxValue : value);
			uint amnt = (uint)(mPrimaryStats.EXP + amount);
			if (mPrimaryStats.Level >= 200) return;
			byte level = mPrimaryStats.Level;
			if (amnt > Constants.GetLevelEXP(mPrimaryStats.Level)) {
				byte levelsGained = 0;
				short apgain = 0;
				short spgain = 0;
				short mpgain = 0;
				short hpgain = 0;
				short job = (short)(mPrimaryStats.Job % 100);
				short x = 1;
				short intt = (short)(mPrimaryStats.GetIntAddition(true) / 10);

				while (amnt > Constants.GetLevelEXP(mPrimaryStats.Level) && levelsGained < 1) {
					amnt -= (uint)Constants.GetLevelEXP(mPrimaryStats.Level);
					level++;
					levelsGained++;

					apgain += Constants.ApPerLevel;

					switch (job) {
						case 0: {
								hpgain += GetHPFromLevelup(Constants.BaseHp.Beginner, 0);
								mpgain += GetMPFromLevelup(Constants.BaseMp.Beginner, intt);
								break;
							}
						case 1: {
								hpgain += GetHPFromLevelup(Constants.BaseHp.Warrior, 0);
								mpgain += GetMPFromLevelup(Constants.BaseMp.Warrior, intt);
								break;
							}
						case 2: {
								hpgain += GetHPFromLevelup(Constants.BaseHp.Magician, 0);
								mpgain += GetMPFromLevelup(Constants.BaseMp.Magician, (short)(2 * x + intt));
								break;
							}
						case 3: {
								hpgain += GetHPFromLevelup(Constants.BaseHp.Bowman, 0);
								mpgain += GetMPFromLevelup(Constants.BaseMp.Bowman, intt);
								break;
							}
						case 4: {
								hpgain += GetHPFromLevelup(Constants.BaseHp.Thief, 0);
								mpgain += GetMPFromLevelup(Constants.BaseMp.Thief, intt);
								break;
							}
						default: {
								hpgain += Constants.BaseHp.Gm;
								mpgain += Constants.BaseMp.Gm;
								break;
							}
					}

					if (mPrimaryStats.Job != 0) {
						spgain = Constants.SpPerLevel;
					}

					if (level >= 200) {
						amnt = 0;
						break;
					}
				}

				if (amnt >= Constants.GetLevelEXP(mPrimaryStats.Level)) {
					amnt = (uint)(Constants.GetLevelEXP(mPrimaryStats.Level) - 1);
				}

				if (levelsGained > 0) {
					ModifyMaxHP(hpgain, true);
					ModifyMaxMP(mpgain, true);
					SetLevel(level);
					AddAP(apgain);
					AddSP(spgain);
					SetHP(mPrimaryStats.MaxHP, false);
					SetMP(mPrimaryStats.MaxMP, false);
				}
			}

			mPrimaryStats.EXP = (int)amnt;
		}

		public void AddMesos(int value) {
			if (value + mInventory.mMesos > int.MaxValue) {
				mInventory.mMesos = int.MaxValue;
			}
			else {
				mInventory.mMesos += value;
			}
		}

		public void SetMesos(int value) {
			if (value + mInventory.mMesos > int.MaxValue) {
				mInventory.mMesos = int.MaxValue;
			}
			else {
				mInventory.mMesos += value;
			}
		}

		public void AddAP(short value) {
			if (value + mPrimaryStats.AP > short.MaxValue) {
				mPrimaryStats.AP = short.MaxValue;
			}
			else {
				mPrimaryStats.AP += value;
			}
		}

		public void SetAP(short value) {
			if (value > short.MaxValue) {
				mPrimaryStats.AP = short.MaxValue;
			}
			else {
				mPrimaryStats.AP = value;
			}
		}

		public void AddSP(short value) {
			if (value + mPrimaryStats.SP > short.MaxValue) {
				mPrimaryStats.SP = short.MaxValue;
			}
			else {
				mPrimaryStats.SP += value;
			}
		}

		public void SetSP(short value) {
			if (value > short.MaxValue) {
				mPrimaryStats.SP = short.MaxValue;
			}
			else {
				mPrimaryStats.SP = value;
			}
		}

		public void AddStr(short value) {
			if (value + mPrimaryStats.Str > short.MaxValue) {
				mPrimaryStats.Str = short.MaxValue;
			}
			else {
				mPrimaryStats.Str += value;
			}
		}

		public void SetStr(short value) {
			if (value > short.MaxValue) {
				mPrimaryStats.Str = short.MaxValue;
			}
			else {
				mPrimaryStats.Str = value;
			}
		}

		public void AddDex(short value) {
			if (value + mPrimaryStats.Dex > short.MaxValue) {
				mPrimaryStats.Dex = short.MaxValue;
			}
			else {
				mPrimaryStats.Dex += value;
			}
		}

		public void SetDex(short value) {
			if (value > short.MaxValue) {
				mPrimaryStats.Dex = short.MaxValue;
			}
			else {
				mPrimaryStats.Dex = value;
			}
		}

		public void AddInt(short value) {
			if (value + mPrimaryStats.Int > short.MaxValue) {
				mPrimaryStats.Int = short.MaxValue;
			}
			else {
				mPrimaryStats.Int += value;
			}
		}

		public void SetInt(short value) {
			if (value > short.MaxValue) {
				mPrimaryStats.Int = short.MaxValue;
			}
			else {
				mPrimaryStats.Int = value;
			}
		}

		public void AddLuk(short value) {
			if (value + mPrimaryStats.Luk > short.MaxValue) {
				mPrimaryStats.Luk = short.MaxValue;
			}
			else {
				mPrimaryStats.Luk += value;
			}
		}

		public void SetLuk(short value) {
			if (value > short.MaxValue) {
				mPrimaryStats.Luk = short.MaxValue;
			}
			else {
				mPrimaryStats.Luk = value;
			}
		}

		public short GetRandomHPOnVariation() {
			Random rnd = new Random();
			return (short)rnd.Next(0, Constants.BaseHp.Variation);
		}
		public short GetRandomMPOnVariation() {
			Random rnd = new Random();
			return (short)rnd.Next(0, Constants.BaseMp.Variation);
		}

		public short GetHPFromLevelup(short value, short bonusValue) {
			return (short)(GetRandomHPOnVariation() + value + bonusValue);
		}
		public short GetMPFromLevelup(short value, short bonusValue) {
			return (short)(GetRandomMPOnVariation() + value + bonusValue);
		}

	}
}
