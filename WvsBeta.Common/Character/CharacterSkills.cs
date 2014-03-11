using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Common;

namespace WvsBeta.Game {
	public class CharacterSkills {
		public Character mCharacter { get; set; }
		public Dictionary<int, byte> mSkills { get; set; }

		public CharacterSkills(Character character) {
			mCharacter = character;
			mSkills = new Dictionary<int, byte>();
		}

		public void SaveSkills() {
			int id = mCharacter.ID;
			string query = "DELETE FROM skills WHERE charid = " + mCharacter.ID.ToString();
			Server.Instance.CharacterDatabase.RunQuery(query);

			bool first = true;
			foreach (KeyValuePair<int, byte> kvp in mSkills) {
				if (first) {
					query = "INSERT INTO skills (charid, skillid, points) VALUES ";
					first = false;
				}
				else {
					query += ", ";
				}
				query += "(" + id.ToString() + ", " + kvp.Key.ToString() + ", " + kvp.Value.ToString() + ")";
			}
			if (!first) {
				Server.Instance.CharacterDatabase.RunQuery(query);
			}
		}

		public bool LoadSkills() {
			Server.Instance.CharacterDatabase.RunQuery("SELECT skillid, points FROM skills WHERE charid = " + mCharacter.ID.ToString());

			MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
			if (!data.HasRows) {
				return false; // Couldn't load character.
			}
			else {
				while (data.Read()) {
					mSkills.Add(data.GetInt32("skillid"), (byte)data.GetInt16("points"));
				}
				return true;
			}
		}

		public void AddSkillPoint(int skillid) {
			if (mSkills.ContainsKey(skillid)) {
				mSkills[skillid] += 1;
			}
			else {
				mSkills.Add(skillid, 1);
			}
			SkillPacket.SendAddSkillPoint(mCharacter, skillid, mSkills[skillid]);
		}

		public void SetSkillPoint(int skillid, byte level, bool packet = true) {
			if (mSkills.ContainsKey(skillid)) {
				mSkills[skillid] = level;
			}
			else {
				mSkills.Add(skillid, level);
			}
			if (packet)
				SkillPacket.SendAddSkillPoint(mCharacter, skillid, mSkills[skillid]);
		}

		public void AddSkills(Packet packet) {
			packet.WriteShort((short)mSkills.Count);

			foreach (KeyValuePair<int, byte> kvp in mSkills) {
				packet.WriteInt(kvp.Key);
				packet.WriteInt(kvp.Value);
			}
		}

		public void DoSkillCost(int skillid, byte level) {
			if (!DataProvider.Skills.ContainsKey(skillid) || level == 0) {
				return;
			}
			SkillLevelData data = DataProvider.Skills[skillid][level];
			short mp = data.MPUsage;
			short hp = data.HPUsage;
			short cash = data.MesosUsage;
			int item = data.ItemIDUsage;

			if (mp > 0) {
				mCharacter.ModifyMP((short)-mp, true);
			}
			if (hp > 0) {
				mCharacter.ModifyHP((short)-hp, true);
			}
			if (item > 0) {
				mCharacter.Inventory.TakeItem(item, data.ItemAmountUsage);
			}
			if (cash > 0) {
				short min = (short)(cash - (80 + level * 5));
				short max = (short)(cash + (80 + level * 5));
				Random rnd = new Random();
				short realAmount = (short)rnd.Next(min, max);
				if (mCharacter.Inventory.mMesos - realAmount >= 0) {
					mCharacter.AddMesos(-realAmount);
				}
				else {
					// HAX
					return;
				}
			}
		}

        

		public void UseMeleeAttack(int skillid) {
			if (skillid != 0) {
				byte level = (byte)(mSkills.ContainsKey(skillid) ? mSkills[skillid] : 0);
				if (!DataProvider.Skills.ContainsKey(skillid) || level == 0) {
					return;
				}
				DoSkillCost(skillid, level);
			}
		}
        

		public void UseRangedAttack(int skillid, short pos) {
			byte level = 0;
			if (skillid != 0) {
				level = (byte)(mSkills.ContainsKey(skillid) ? mSkills[skillid] : 0);
				if (!DataProvider.Skills.ContainsKey(skillid) || level == 0) {
					return;
				}
				DoSkillCost(skillid, level);
			}
			short hits = 1;
			if (skillid != 0) {
				short bullets = DataProvider.Skills[skillid][level].BulletUsage;
				if (bullets > 0)
					hits = bullets;
			}
			if (mCharacter.Buffs.HasBuff((uint)BuffValueTypes.ShadowPartner)) {
				hits *= 2;
			}

			if (pos > 0 && !(mCharacter.Buffs.HasBuff((uint)BuffValueTypes.SoulArrow))) {
				mCharacter.Inventory.TakeItemAmountFromSlot(2, pos, hits, false);
			}
		}

		public byte GetSkillLevel(int skillid) {
			if (mSkills.ContainsKey(skillid))
				return mSkills[skillid];
			return 0;
		}

		public short GetRechargeableBonus() {
			short bonus = 0;
			switch (mCharacter.PrimaryStats.Job) {
				case (short)Constants.Assassin.ID:
				case (short)Constants.Hermit.ID: bonus = (short)(GetSkillLevel((int)Constants.Assassin.Skills.ClawMastery) * 10); break;
			}
			return bonus;
		}

		public int GetMastery() {
			int masteryid = 0;
			switch (Constants.getItemType(mCharacter.Inventory.GetEquippedItemID((short)Constants.EquipSlots.Slots.Weapon, false))) {
				case (int)Constants.Items.Types.ItemTypes.Weapon1hSword:
				case (int)Constants.Items.Types.ItemTypes.Weapon2hSword:
					switch (mCharacter.PrimaryStats.Job) {
						case Constants.Fighter.ID:
						case Constants.Crusader.ID:
							masteryid = (int)Constants.Fighter.Skills.SwordMastery;
							break;
						case Constants.Page.ID:
						case Constants.WhiteKnight.ID:
							masteryid = (int)Constants.Page.Skills.SwordMastery;
							break;
					}
					break;
				case (int)Constants.Items.Types.ItemTypes.Weapon1hAxe:
				case (int)Constants.Items.Types.ItemTypes.Weapon2hAxe:
					masteryid = (int)Constants.Fighter.Skills.AxeMastery;
					break;
				case (int)Constants.Items.Types.ItemTypes.Weapon1hMace:
				case (int)Constants.Items.Types.ItemTypes.Weapon2hMace:
					masteryid = (int)Constants.Page.Skills.BwMastery;
					break;
				case (int)Constants.Items.Types.ItemTypes.WeaponSpear: masteryid = (int)Constants.Spearman.Skills.SpearMastery; break;
				case (int)Constants.Items.Types.ItemTypes.WeaponPolearm: masteryid = (int)Constants.Spearman.Skills.PolearmMastery; break;
				case (int)Constants.Items.Types.ItemTypes.WeaponDagger: masteryid = (int)Constants.Bandit.Skills.DaggerMastery; break;
				case (int)Constants.Items.Types.ItemTypes.WeaponBow: masteryid = (int)Constants.Hunter.Skills.BowMastery; break;
				case (int)Constants.Items.Types.ItemTypes.WeaponCrossbow: masteryid = (int)Constants.Crossbowman.Skills.CrossbowMastery; break;
				case (int)Constants.Items.Types.ItemTypes.WeaponClaw: masteryid = (int)Constants.Assassin.Skills.ClawMastery; break;
			}
			return masteryid;
		}

		public SkillLevelData GetSkillLevelData(int skill, byte level) {
			if (!DataProvider.Skills.ContainsKey(skill)) return null;
			else if (!DataProvider.Skills[skill].ContainsKey(level)) return null;
			return DataProvider.Skills[skill][level];
		}
	}
}
