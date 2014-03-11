using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Shop {
	public class CharacterSkills {
		public Character mCharacter { get; set; }
		public Dictionary<int, byte> mSkills { get; set; }

		public CharacterSkills(Character character) {
			mCharacter = character;
			mSkills = new Dictionary<int, byte>();
		}

		public void SaveSkills() {
			int id = mCharacter.mID;
			string query = "DELETE FROM skills WHERE charid = " + mCharacter.mID.ToString();
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
			Server.Instance.CharacterDatabase.RunQuery("SELECT skillid, points FROM skills WHERE charid = " + mCharacter.mID.ToString());

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
		}

		public void AddSkills(Packet packet) {
			packet.WriteShort((short)mSkills.Count);

			foreach (KeyValuePair<int, byte> kvp in mSkills) {
				packet.WriteInt(kvp.Key);
				packet.WriteInt(kvp.Value);
			}
		}

		public byte GetSkillLevel(int skillid) {
			if (mSkills.ContainsKey(skillid))
				return mSkills[skillid];
			return 0;
		}

		public short GetRechargeableBonus() {
			short bonus = 0;
			switch (mCharacter.mPrimaryStats.Job) {
				case (short)Constants.Assassin.ID:
				case (short)Constants.Hermit.ID: bonus = (short)(GetSkillLevel((int)Constants.Assassin.Skills.ClawMastery) * 10); break;
			}
			return bonus;
		}

		public int GetMastery() {
			int masteryid = 0;
			switch (Constants.getItemType(mCharacter.mInventory.GetEquippedItemID((short)Constants.EquipSlots.Slots.Weapon, false))) {
				case (int)Constants.Items.Types.ItemTypes.Weapon1hSword:
				case (int)Constants.Items.Types.ItemTypes.Weapon2hSword:
					switch (mCharacter.mPrimaryStats.Job) {
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
	}
}
