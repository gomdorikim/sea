using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game {
	public class Summon : MovableLife {
		public int mSummonID { get; set; }
		public byte mType { get; set; }
		public byte mLevel { get; set; }
		public int mHP { get; set; }
		public DateTime mExpires { get; set; }
	}

	public class CharacterSummons {
		public Character mCharacter { get; set; }
		public Summon mPuppet { get; set; }
		public Summon mSummon { get; set; }
		public Timer mPuppetExpirationTimer { get; set; }
		public Timer mSummonExpirationTimer { get; set; }

		public CharacterSummons(Character chr) {
			mCharacter = chr;
		}

		~CharacterSummons() {
			if (mPuppetExpirationTimer != null) {
				mPuppetExpirationTimer.Stop();
				mPuppetExpirationTimer.Elapsed -= mPuppetExpirationTimer_Elapsed;
				mPuppetExpirationTimer.Dispose();
				mPuppetExpirationTimer = null;
			}
			if (mSummonExpirationTimer != null) {
				mSummonExpirationTimer.Stop();
				mSummonExpirationTimer.Elapsed -= mSummonExpirationTimer_Elapsed;
				mSummonExpirationTimer.Dispose();
				mSummonExpirationTimer = null;
			}
			mPuppet = null;
			mSummon = null;
		}

		public void NewSummon(int skillid, byte level) {
			Summon summon = new Summon();
			summon.mSummonID = skillid;
			summon.mLevel = level;
			Pos playerPos = mCharacter.Position;
			Pos spawnPos = playerPos;
			bool puppet = (summon.mSummonID == (int)Constants.Sniper.Skills.Puppet || summon.mSummonID == (int)Constants.Ranger.Skills.Puppet);
			if (puppet) {
				short x = (short)(playerPos.X + 200 * (mCharacter.isFacingRight() ? 1 : -1));
				spawnPos = DataProvider.Maps[mCharacter.Map].FindFloor(new Pos(x, playerPos.Y));
				SkillLevelData sld = DataProvider.Skills[skillid][level];
				summon.mHP = sld.XValue;
			}
			summon.Position = new Pos(spawnPos); // Else there's copying
			AddSummon(summon, DataProvider.Skills[skillid][level].BuffTime);
			SummonPacket.SendShowSummon(mCharacter, summon, true, null);
		}

		public void AddSummon(Summon summon, int time) {
			summon.mExpires = DateTime.Now.Add(new TimeSpan(0, 0, time));
			if (summon.mSummonID == (int)Constants.Sniper.Skills.Puppet || summon.mSummonID == (int)Constants.Ranger.Skills.Puppet) {
				if (mPuppet != null) {
					RemoveSummon(true, 0x04);
				}
				mPuppet = summon;
				if (mPuppetExpirationTimer == null) {
					mPuppetExpirationTimer = new Timer();
					mPuppetExpirationTimer.AutoReset = false;
					mPuppetExpirationTimer.Elapsed += new ElapsedEventHandler(mPuppetExpirationTimer_Elapsed);
				}
				mPuppetExpirationTimer.Interval = time * 1000;
				mPuppetExpirationTimer.Start();
			}
			else {
				if (mSummon != null) {
					RemoveSummon(false, 0x04);
				}
				mSummon = summon;
				if (mSummonExpirationTimer == null) {
					mSummonExpirationTimer = new Timer();
					mSummonExpirationTimer.AutoReset = false;
					mSummonExpirationTimer.Elapsed += new ElapsedEventHandler(mSummonExpirationTimer_Elapsed);
				}
				mSummonExpirationTimer.Interval = time * 1000;
				mSummonExpirationTimer.Start();
			}
		}

		void mPuppetExpirationTimer_Elapsed(object sender, ElapsedEventArgs e) {
			RemoveSummon(true, 0x02);
		}

		void mSummonExpirationTimer_Elapsed(object sender, ElapsedEventArgs e) {
			RemoveSummon(false, 0x02);
		}

		public void RemoveSummon(bool puppet, byte msg) {
			if (!puppet && mSummon != null) {
				SummonPacket.SendRemoveSummon(mCharacter, mSummon.mSummonID, msg);
				mSummon = null;
				if (mSummonExpirationTimer != null) {
					mSummonExpirationTimer.Stop();
				}
			}
			else if (mPuppet != null) {
				SummonPacket.SendRemoveSummon(mCharacter, mPuppet.mSummonID, msg);
				mPuppet = null;
				if (mPuppetExpirationTimer != null) {
					mPuppetExpirationTimer.Stop();
				}
			}
		}

		public Summon GetSummon(int summonId) {
			if (mSummon != null && mSummon.mSummonID == summonId) {
				return mSummon;
			}
			if (mPuppet != null && mPuppet.mSummonID == summonId) {
				return mPuppet;
			}
			return null;
		}
	}
}
