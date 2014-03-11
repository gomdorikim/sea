using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Shop {
	public struct PrimaryStatsAddition {
		public int ItemID { get; set; }
		public short Slot { get; set; }
		public short Str { get; set; }
		public short Dex { get; set; }
		public short Int { get; set; }
		public short Luk { get; set; }
		public short MaxHP { get; set; }
		public short MaxMP { get; set; }
		public short Speed { get; set; }
	}

	public class BonusSet {
		public short Str { get; set; }
		public short Dex { get; set; }
		public short Int { get; set; }
		public short Luk { get; set; }
		public short MaxHP { get; set; }
		public short MaxMP { get; set; }
		public short Speed { get; set; }
		public BonusSet() {
			Str = 0;
			Dex = 0;
			Int = 0;
			MaxHP = 0;
			MaxMP = 0;
			Speed = 0;
		}
	}

	public class EquipBonus : BonusSet {
		public int ID { get; set; }
	}


	public class CharacterPrimaryStats {
		public byte Level { get; set; }
		public short Job { get; set; }
		public short Str { get; set; }
		public short Dex { get; set; }
		public short Int { get; set; }
		public short Luk { get; set; }
		public short HP { get; set; }
		public short MaxHP { get; set; }
		public short MP { get; set; }
		public short MaxMP { get; set; }
		public short AP { get; set; }
		public short SP { get; set; }
		public int EXP { get; set; }
		public short Fame { get; set; }
		public byte Speed { get; set; }
		public float speedMod { get; set; }
		public byte Jump { get; set; }
		public float jumpMode { get; set; }


		public Dictionary<short, EquipBonus> EquipStats;
		public BonusSet EquipBonuses;
		public BonusSet BuffBonuses;

		private Character Char { get; set; }

		public CharacterPrimaryStats(Character chr) {
			EquipStats = new Dictionary<short, EquipBonus>();
			EquipBonuses = new BonusSet();
			BuffBonuses = new BonusSet();
			Char = chr;
		}

		public void AddEquipStarts(short slot, Item equip, bool isLoading) {
			try {
				slot = Math.Abs(slot);
				if (equip != null) {
					if (!EquipStats.ContainsKey(slot))
						EquipStats.Add(slot, new EquipBonus());
					EquipStats[slot].ID = equip.ItemID;
					EquipStats[slot].MaxHP = equip.HP;
					EquipStats[slot].MaxMP = equip.MP;
					EquipStats[slot].Str = equip.Str;
					EquipStats[slot].Int = equip.Int;
					EquipStats[slot].Dex = equip.Dex;
					EquipStats[slot].Luk = equip.Luk;
					EquipStats[slot].Speed = equip.Speed;
				}
				else {
					EquipStats.Remove(slot);
				}
				CalculateAdditions(true, isLoading);
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}

		public void CalculateAdditions(bool updateEquips, bool isLoading) {
			if (updateEquips) {
				EquipBonuses = null;
				EquipBonuses = new BonusSet();
				EquipBonus item;
				foreach (KeyValuePair<short, EquipBonus> data in EquipStats) {
					item = data.Value;
					if (EquipBonuses.Dex + item.Dex > short.MaxValue) EquipBonuses.Dex = short.MaxValue;
					else EquipBonuses.Dex += item.Dex;
					if (EquipBonuses.Int + item.Int > short.MaxValue) EquipBonuses.Int = short.MaxValue;
					else EquipBonuses.Int += item.Int;
					if (EquipBonuses.Luk + item.Luk > short.MaxValue) EquipBonuses.Luk = short.MaxValue;
					else EquipBonuses.Luk += item.Luk;
					if (EquipBonuses.Str + item.Str > short.MaxValue) EquipBonuses.Str = short.MaxValue;
					else EquipBonuses.Str += item.Str;
					if (EquipBonuses.MaxMP + item.MaxMP > short.MaxValue) EquipBonuses.MaxMP = short.MaxValue;
					else EquipBonuses.MaxMP += item.MaxMP;
					if (EquipBonuses.MaxHP + item.MaxHP > short.MaxValue) EquipBonuses.MaxHP = short.MaxValue;
					else EquipBonuses.MaxHP += item.MaxHP;
				}
			}
			if (!isLoading) {
				CheckHPMP();
			}
		}

		public void CheckHPMP() {
			if (HP > GetMaxHP(false)) {
				Char.SetHP(HP, true);
			}
			if (MP > GetMaxMP(false)) {
				Char.SetMP(MP, true);
			}
		}

		public short GetStrAddition(bool nobonus) {
			if (!nobonus) {
				return (short)((Str + EquipBonuses.Str + EquipBonuses.Str) > short.MaxValue ? short.MaxValue : (Str + EquipBonuses.Str + EquipBonuses.Str));
			}
			return Str;
		}
		public short GetDexAddition(bool nobonus) {
			if (!nobonus) {
				return (short)((MaxMP + EquipBonuses.MaxMP + EquipBonuses.MaxMP) > short.MaxValue ? short.MaxValue : (MaxMP + EquipBonuses.MaxMP + EquipBonuses.MaxMP));
			}
			return Dex;
		}
		public short GetIntAddition(bool nobonus) {
			if (!nobonus) {
				return (short)((MaxMP + EquipBonuses.MaxMP + EquipBonuses.MaxMP) > short.MaxValue ? short.MaxValue : (MaxMP + EquipBonuses.MaxMP + EquipBonuses.MaxMP));
			}
			return Int;
		}
		public short GetLukAddition(bool nobonus) {
			if (!nobonus) {
				return (short)((MaxMP + EquipBonuses.MaxMP + EquipBonuses.MaxMP) > short.MaxValue ? short.MaxValue : (MaxMP + EquipBonuses.MaxMP + EquipBonuses.MaxMP));
			}
			return Luk;
		}
		public short GetMaxHP(bool nobonus) {
			if (!nobonus) {
				return (short)((MaxHP + EquipBonuses.MaxHP + EquipBonuses.MaxHP) > short.MaxValue ? short.MaxValue : (MaxHP + EquipBonuses.MaxHP + EquipBonuses.MaxHP));
			}
			return MaxHP;
		}
		public short GetMaxMP(bool nobonus) {
			if (!nobonus) {
				return (short)((MaxMP + EquipBonuses.MaxMP + EquipBonuses.MaxMP) > short.MaxValue ? short.MaxValue : (MaxMP + EquipBonuses.MaxMP + EquipBonuses.MaxMP));
			}
			return MaxHP;
		}

		public void SetSpeed(byte pSpeed) {
			this.Speed = pSpeed;
			speedMod = pSpeed / 100.0f;
		}
	}

}