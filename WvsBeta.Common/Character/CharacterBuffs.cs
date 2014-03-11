using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game {
	public class CharacterBuffs {
		public List<BuffData> mBuffs { get; set; }
		public Character mCharacter { get; set; }
		public byte mComboCount { get; set; }
		public Dictionary<int, byte> mActiveSkillLevels { get; set; }

		public void CheckExpired() {
			DateTime now = DateTime.Now;
			List<BuffData> buffs = new List<BuffData>(mBuffs);
			foreach (BuffData buff in buffs) {
				if (buff.Expires <= now) {
					RemoveBuff(buff.SkillID);
				}
			}
		}

		public CharacterBuffs(Character chr) {
			mCharacter = chr;
			mBuffs = new List<BuffData>();
			mActiveSkillLevels = new Dictionary<int, byte>();
		}

		public void RemoveBuff(int SkillID) {
			uint removedFlags = 0x00;
			foreach (BuffData buff in mBuffs.FindAll(b => b.SkillID == SkillID)) {
				removedFlags += buff.Flag;
				mBuffs.Remove(buff);
			}
			if (mActiveSkillLevels.ContainsKey(SkillID)) mActiveSkillLevels.Remove(SkillID);
			BuffPacket.GiveDebuffs(mCharacter, removedFlags);
			MapPacket.SendPlayerDebuffed(mCharacter, removedFlags);
		}

		public bool HasBuff(uint flag) {
			try {
				bool val = false;
				mBuffs.ForEach(b => { if (b.Flag == flag) val = true; });
				return val;
			}
			catch { }
			return false;
		}

		public bool HasGMHide() {
			return mBuffs.Find(b => b.SkillID == (int)Constants.Gm.Skills.Hide) != null;
		}

		public BuffData GetBuffData(int skill) { return mBuffs.Find(b => b.SkillID == skill); }
		public BuffData GetBuffData(uint Flag) { return mBuffs.Find(b => b.Flag == Flag); }
		public BuffData GetBuffData(BuffValueTypes Flag) { return mBuffs.Find(b => b.Flag == (uint)Flag); }

		public byte GetActiveSkillLevel(int skill) {
			return mActiveSkillLevels.ContainsKey(skill) ? mActiveSkillLevels[skill] : (byte)0;
		}

		public void AddItemBuff(int itemid) {
			ItemData data = DataProvider.Items[itemid];
			short time = (short)(data.BuffTime / 1000);
			List<BuffData> buffs = new List<BuffData>();

			if (data.Accuracy > 0) {
				BuffData bd = new BuffData();
				bd.Flag = (uint)BuffValueTypes.Accurancy;
				bd.Value = data.Accuracy;
				bd.SkillID = 2301004;
				//bd.SkillID = itemid;
				//bd.IsItemBuff = true;
				bd.Time = time;
				bd.SetExpires();
				buffs.Add(bd);
				mBuffs.Add(bd);
			}
			if (data.Avoidance > 0) {
				BuffData bd = new BuffData();
				bd.Flag = (uint)BuffValueTypes.Avoidability;
				bd.Value = data.Avoidance;
				bd.SkillID = 2301004;
				//bd.SkillID = itemid;
				//bd.IsItemBuff = true;
				bd.Time = time;
				bd.SetExpires();
				buffs.Add(bd);
				mBuffs.Add(bd);
			}
			if (data.Speed > 0) {
				BuffData bd = new BuffData();
				bd.Flag = (uint)BuffValueTypes.Speed;
				bd.Value = data.Speed;
				bd.SkillID = itemid;
				bd.IsItemBuff = true;
				bd.Time = time;
				bd.SetExpires();
				buffs.Add(bd);
				mBuffs.Add(bd);
			}
			if (data.MagicAttack > 0) {
				BuffData bd = new BuffData();
				bd.Flag = (uint)BuffValueTypes.MagicAttack;
				bd.Value = data.MagicAttack;
				bd.SkillID = 2301004;
				//bd.SkillID = itemid;
				//bd.IsItemBuff = true;
				bd.Time = time;
				bd.SetExpires();
				buffs.Add(bd);
				mBuffs.Add(bd);
			}
			if (data.WeaponAttack > 0) {
				BuffData bd = new BuffData();
				bd.Flag = (uint)BuffValueTypes.WeaponAttack;
				bd.Value = data.WeaponAttack;
				bd.SkillID = 2301004;
				//bd.SkillID = itemid;
				//bd.IsItemBuff = true;
				bd.Time = time;
				bd.SetExpires();

				

				buffs.Add(bd);
				mBuffs.Add(bd);
			}

			BuffPacket.GiveBuffs(mCharacter, buffs);
			MapPacket.SendPlayerBuffed(mCharacter);
		}

		public void AddBuff(int SkillID) {
			if (BuffDataProvider.mSkillBuffValues.ContainsKey(SkillID)) {
				List<BuffValueTypes> flags = BuffDataProvider.mSkillBuffValues[SkillID];
				byte skillLevel = (byte)mCharacter.Skills.mSkills[SkillID];
				SkillLevelData data = DataProvider.Skills[SkillID][skillLevel];
				List<BuffData> buffs = new List<BuffData>();
				foreach (BuffValueTypes bft in flags) {
					BuffData bd;// = new BuffData();
					if (!HasBuff((uint)bft)) bd = new BuffData();
					else {
						bd = mBuffs.Find(b => b.Flag == (uint)bft);
					}
					bd.Flag = (uint)bft;
					switch (bft) {
						case BuffValueTypes.Accurancy: bd.Value = data.Accurancy; break;
						case BuffValueTypes.Avoidability: bd.Value = data.Avoidability; break;
						case BuffValueTypes.WeaponAttack: bd.Value = data.WeaponAttack; break;
						case BuffValueTypes.WeaponDefence: bd.Value = data.WeaponDefence; break;
						case BuffValueTypes.MagicAttack: bd.Value = data.MagicAttack; break;
						case BuffValueTypes.MagicDefence: bd.Value = data.MagicDefence; break;
						case BuffValueTypes.Jump: bd.Value = data.Jump; break;
						case BuffValueTypes.Speed: bd.Value = data.Speed; break;

						case BuffValueTypes.ComboAttack: bd.Value = (short)(mComboCount + 1); break;
						case BuffValueTypes.ShadowPartner: bd.Value = (short)(data.XValue * 256 + data.YValue); break;
						case BuffValueTypes.DragonBlood: bd.Value = skillLevel; break;
						//case BuffValueTypes.HyperBody:
						case BuffValueTypes.Stun: bd.Value = data.YValue; break;
						default: bd.Value = data.XValue; break;
					}

					if (mActiveSkillLevels.ContainsKey(SkillID)) mActiveSkillLevels[SkillID] = skillLevel;
					else mActiveSkillLevels.Add(SkillID, skillLevel);
					bd.SkillID = SkillID;
					bd.Time = (short)data.BuffTime;
					bd.SetExpires();
					mBuffs.Add(bd);
					buffs.Add(bd);
				}

				BuffPacket.GiveBuffs(mCharacter, buffs);
				MapPacket.SendPlayerBuffed(mCharacter);
			}
		}

		public bool AddBuff(int SkillID, byte level) {
			if (BuffDataProvider.mSkillBuffValues.ContainsKey(SkillID)) {
				List<BuffValueTypes> flags = BuffDataProvider.mSkillBuffValues[SkillID];
				SkillLevelData data = DataProvider.Skills[SkillID][level];
				List<BuffData> buffs = new List<BuffData>();
				foreach (BuffValueTypes bft in flags) {
					if (!HasBuff((uint)bft)) {
						BuffData bd = new BuffData();
						bd.Flag = (uint)bft;
						switch (bft) {
							case BuffValueTypes.Accurancy: bd.Value = data.Accurancy; break;
							case BuffValueTypes.Avoidability: bd.Value = data.Avoidability; break;
							case BuffValueTypes.WeaponAttack: bd.Value = data.WeaponAttack; break;
							case BuffValueTypes.WeaponDefence: bd.Value = data.WeaponDefence; break;
							case BuffValueTypes.MagicAttack: bd.Value = data.MagicAttack; break;
							case BuffValueTypes.MagicDefence: bd.Value = data.MagicDefence; break;
							case BuffValueTypes.Jump: bd.Value = data.Jump; break;
							case BuffValueTypes.Speed: bd.Value = data.Speed; break;

							case BuffValueTypes.ComboAttack: bd.Value = (short)(mComboCount + 1); break;
							case BuffValueTypes.ShadowPartner: bd.Value = (short)(data.XValue * 256 + data.YValue); break;
							case BuffValueTypes.DragonBlood: bd.Value = level; break;
                            //case BuffValueTypes.HyperBody: bd.Value = data.XValue; break;
							case BuffValueTypes.Stun: bd.Value = data.YValue; break;
							default: bd.Value = data.XValue; break;
						}
						if (mActiveSkillLevels.ContainsKey(SkillID)) mActiveSkillLevels[SkillID] = level;
						else mActiveSkillLevels.Add(SkillID, level);

						bd.Time = (short)data.BuffTime;
						bd.SetExpires();
						bd.SkillID = SkillID;
						mBuffs.Add(bd);
						buffs.Add(bd);
					}
				}

				BuffPacket.GiveBuffs(mCharacter, buffs);
				return false;
			}
			else {
				return true;
			}
		}

		public void AddBuff(int skillid, short value, short time, uint flag) {
			List<BuffData> buffs = new List<BuffData>();
			if (HasBuff(flag)) {
				BuffData bd = null;
				mBuffs.ForEach(b => { if (b.SkillID == skillid) bd = b; });
				bd.Value = value;
				bd.Time = time;
				buffs.Add(bd);
			}
			else {
				BuffData bd = new BuffData();
				bd.SkillID = skillid;
				bd.Value = value;
				bd.Time = time;
				bd.Flag = flag;
				mBuffs.Add(bd);
				buffs.Add(bd);
			}

			BuffPacket.GiveBuffs(mCharacter, buffs);
		}
	}
}
