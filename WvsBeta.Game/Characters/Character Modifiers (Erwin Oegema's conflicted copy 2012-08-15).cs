using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game {
	public partial class Character {
		public void SetJob(short value) {
			PrimaryStats.Job = value;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Job, value);
		}

		public void SetEXP(int value) {
			PrimaryStats.EXP = value;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Exp, value);
		}

		public void SetHP(short value, bool sendPacket = true) {
			if (value < 0) value = 0;
			PrimaryStats.HP = value;
			if (sendPacket) {
				CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Hp, value);
			}
			SetMaxHP(value);
		}

		public void ModifyHP(short value, bool sendPacket = true) {
			if ((PrimaryStats.HP + value) < 0) {
				PrimaryStats.HP = 0;
			}
			else if ((PrimaryStats.HP + value) > PrimaryStats.GetMaxHP(false)) {
				PrimaryStats.HP = PrimaryStats.GetMaxHP(false);
			}
			else {
				PrimaryStats.HP = (short)(PrimaryStats.HP + value);
			}
			if (sendPacket) {
				CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Hp, PrimaryStats.HP);
			}
			ModifiedHP();
		}

		public void DamageHP(short amount) {
			PrimaryStats.HP = (short)(amount > PrimaryStats.HP ? 0 : amount - PrimaryStats.HP);
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Hp, PrimaryStats.HP);
			ModifiedHP();
		}

		public void ModifiedHP() {
			if (PrimaryStats.HP == 0) {
				// lose exp
				loseEXP();

				Summons.RemoveSummon(false, 0x03);
				Summons.RemoveSummon(true, 0x03);
			}
		}

		public void SetMP(short value, bool isBySelf = false) {
			if (value < 0) value = 0;
			PrimaryStats.MP = value;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mp, value, isBySelf);
			SetMaxMP(value);
		}

		public void ModifyMP(short value, bool isSelf = false) {
			if ((PrimaryStats.MP + value) < 0) {
				PrimaryStats.MP = 0;
			}
			else if ((PrimaryStats.MP + value) > PrimaryStats.GetMaxMP(false)) {
				PrimaryStats.MP = PrimaryStats.GetMaxMP(false);
			}
			else {
				PrimaryStats.MP = (short)(PrimaryStats.MP + value);
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mp, PrimaryStats.MP, isSelf);
		}

		public void DamageMP(short amount) {
			PrimaryStats.MP = (short)(amount > PrimaryStats.MP ? 0 : amount - PrimaryStats.MP);
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mp, PrimaryStats.MP, false);
		}

		public void ModifyMaxMP(short value) {
			PrimaryStats.MaxMP = (short)(((PrimaryStats.MaxMP + value) > Constants.MaxMaxMp) ? Constants.MaxMaxMp : (PrimaryStats.MaxMP + value));
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.MaxMp, PrimaryStats.MaxMP);
		}

		public void ModifyMaxHP(short value) {
			PrimaryStats.MaxHP = (short)(((PrimaryStats.MaxHP + value) > Constants.MaxMaxHp) ? Constants.MaxMaxHp : (PrimaryStats.MaxHP + value));
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.MaxHp, PrimaryStats.MaxHP);
		}

		public void SetMaxHP(short value) {
			if (value > Constants.MaxMaxHp) value = Constants.MaxMaxHp;
			else if (value < Constants.MinMaxHp) value = Constants.MinMaxHp;
			PrimaryStats.MaxHP = value;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.MaxHp, value);
		}

		public void SetMaxMP(short value) {
			if (value > Constants.MaxMaxMp) value = Constants.MaxMaxMp;
			else if (value < Constants.MinMaxMp) value = Constants.MinMaxMp;
			PrimaryStats.MaxMP = value;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.MaxMp, value);
		}

		public void SetLevel(byte value) {
			PrimaryStats.Level = value;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Level, value);
			MapPacket.SendPlayerLevelupAnim(this);
			Save();
		}

		public void AddFame(short value) {
			if (PrimaryStats.Fame + value > short.MaxValue) {
				SetFame(short.MaxValue);
			}
			else if (PrimaryStats.Fame + value < 0) {
				SetFame(0);
			}
			else {
				SetFame((short)(PrimaryStats.Fame + value));
			}
		}

		public void SetFame(short value) {
			PrimaryStats.Fame = value;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Fame, value);
		}

		public void AddEXP(uint value) {
			int amount = (int)(value > int.MaxValue ? int.MaxValue : value);
			uint amnt = (uint)(PrimaryStats.EXP + amount);
			if (PrimaryStats.Level >= 200) return;
			if (value != 0)
				CharacterStatsPacket.SendGainEXP(this, amount, true, false);
			byte level = PrimaryStats.Level;
			if (amnt > Constants.GetLevelEXP(PrimaryStats.Level)) {
				byte levelsGained = 0;
				short apgain = 0;
				short spgain = 0;
				short mpgain = 0;
				short hpgain = 0;
                short job = (short)(PrimaryStats.Job % 100); // idk why but this variable was the reason why SP gain upon level wasn't working in the first place...
				short x = 1;
				short intt = (short)(PrimaryStats.GetIntAddition(true) / 10);

				while (amnt > Constants.GetLevelEXP(PrimaryStats.Level) && levelsGained < 1) {
					amnt -= (uint)Constants.GetLevelEXP(PrimaryStats.Level);
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

                    if (PrimaryStats.Job != 0) // ugh finally fixed SP gain upon leveling. fuuuuuck this one was annoying.. -__-
                    {
						spgain = Constants.SpPerLevel;
					}

					if (level >= 200) {
						amnt = 0;
						break;
					}
				}

				if (amnt >= Constants.GetLevelEXP(PrimaryStats.Level)) {
					amnt = (uint)(Constants.GetLevelEXP(PrimaryStats.Level) - 1);
				}

				if (levelsGained > 0) {
					ModifyMaxHP(hpgain);
					ModifyMaxMP(mpgain);
					SetLevel(level);
					AddAP(apgain);
                    AddSP(spgain);
					SetHP(PrimaryStats.GetMaxHP(false));
					SetMP(PrimaryStats.GetMaxMP(false));
				}
			}

			PrimaryStats.EXP = (int)amnt;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Exp, PrimaryStats.EXP);
		}

        public void AddEXP(double value)
        {
            int amount = (int)(value > int.MaxValue ? int.MaxValue : value);
            uint amnt = (uint)(PrimaryStats.EXP + amount);
            if (PrimaryStats.Level >= 200) return;
            if (value != 0)
                CharacterStatsPacket.SendGainEXP(this, amount, true, false);
            byte level = PrimaryStats.Level;
            if (amnt > Constants.GetLevelEXP(PrimaryStats.Level))
            {
                byte levelsGained = 0;
                short apgain = 0;
                short spgain = 0;
                short mpgain = 0;
                short hpgain = 0;
                short job = (short)(PrimaryStats.Job % 100);
                short x = 1;
                short intt = (short)(PrimaryStats.GetIntAddition(true) / 10);

                while (amnt > Constants.GetLevelEXP(PrimaryStats.Level) && levelsGained < 1)
                {
                    amnt -= (uint)Constants.GetLevelEXP(PrimaryStats.Level);
                    level++;
                    levelsGained++;

                    apgain += Constants.ApPerLevel;

                    switch (job)
                    {
                        case 0:
                            {
                                hpgain += GetHPFromLevelup(Constants.BaseHp.Beginner, 0);
                                mpgain += GetMPFromLevelup(Constants.BaseMp.Beginner, intt);
                                break;
                            }
                        case 1:
                            {
                                hpgain += GetHPFromLevelup(Constants.BaseHp.Warrior, 0);
                                mpgain += GetMPFromLevelup(Constants.BaseMp.Warrior, intt);
                                break;
                            }
                        case 2:
                            {
                                hpgain += GetHPFromLevelup(Constants.BaseHp.Magician, 0);
                                mpgain += GetMPFromLevelup(Constants.BaseMp.Magician, (short)(2 * x + intt));
                                break;
                            }
                        case 3:
                            {
                                hpgain += GetHPFromLevelup(Constants.BaseHp.Bowman, 0);
                                mpgain += GetMPFromLevelup(Constants.BaseMp.Bowman, intt);
                                break;
                            }
                        case 4:
                            {
                                hpgain += GetHPFromLevelup(Constants.BaseHp.Thief, 0);
                                mpgain += GetMPFromLevelup(Constants.BaseMp.Thief, intt);
                                break;
                            }
                        default:
                            {
                                hpgain += Constants.BaseHp.Gm;
                                mpgain += Constants.BaseMp.Gm;
                                break;
                            }
                    }

                    if (PrimaryStats.Job != 0) {
                        spgain = Constants.SpPerLevel;
                    }

                    if (level >= 200)
                    {
                        amnt = 0;
                        break;
                    }
                }

                if (amnt >= Constants.GetLevelEXP(PrimaryStats.Level))
                {
                    amnt = (uint)(Constants.GetLevelEXP(PrimaryStats.Level) - 1);
                }

                if (levelsGained > 0)
                {
                    ModifyMaxHP(hpgain);
                    ModifyMaxMP(mpgain);
                    SetLevel(level);
                    AddAP(apgain);
                    AddSP(spgain);
                    SetHP(PrimaryStats.GetMaxHP(false));
                    SetMP(PrimaryStats.GetMaxMP(false));
                }
            }

            PrimaryStats.EXP = (int)amnt;
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Exp, PrimaryStats.EXP);
        }

		public void AddMesos(int value, bool isSelf = false) {
			int newMesos = 0;
			if (value < 0) {
				if ((Inventory.mMesos - value) < 0) newMesos = 0;
				else newMesos = Inventory.mMesos + value; // neg - neg = pos
			}
			else {
				if ((Inventory.mMesos + value) > int.MaxValue) newMesos = int.MaxValue;
				else newMesos = Inventory.mMesos + value;
			}
			Inventory.mMesos = newMesos;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mesos, Inventory.mMesos, isSelf);
		}

        public void AddMaplePoints(int value, Character chr)
        {
            Server.Instance.CharacterDatabase.RunQuery("UPDATE storage SET maplepoints = " + value + " WHERE UserID = " + chr.UserID);
        }

		public void SetMesos(int value, bool isSelf = false) {
			if (value > int.MaxValue) value = int.MaxValue;
			else if (value < 0) value = 0;
			Inventory.mMesos = value;
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mesos, Inventory.mMesos, isSelf);
		}

		public void AddAP(short value) {
			if (value + PrimaryStats.AP > short.MaxValue) {
				PrimaryStats.AP = short.MaxValue;
			}
			else {
				PrimaryStats.AP += value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Ap, PrimaryStats.AP);
		}

		public void SetAP(short value) {
			if (value > short.MaxValue) {
				PrimaryStats.AP = short.MaxValue;
			}
			else {
				PrimaryStats.AP = value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Ap, PrimaryStats.AP);
		}

		public void AddSP(short value) {
			if (value + PrimaryStats.SP > short.MaxValue) {
				PrimaryStats.SP = short.MaxValue;
			}
			else {
				PrimaryStats.SP += value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Sp, PrimaryStats.SP);
		}

		public void SetSP(short value) {
			if (value > short.MaxValue) {
				PrimaryStats.SP = short.MaxValue;
			}
			else {
				PrimaryStats.SP = value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Sp, PrimaryStats.SP);
		}

		public void AddStr(short value) {
			if (value + PrimaryStats.Str > short.MaxValue) {
				PrimaryStats.Str = short.MaxValue;
			}
			else {
				PrimaryStats.Str += value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Str, PrimaryStats.Str);
		}

		public void SetStr(short value) {
			if (value > short.MaxValue) {
				PrimaryStats.Str = short.MaxValue;
			}
			else {
				PrimaryStats.Str = value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Str, PrimaryStats.Str);
		}

		public void AddDex(short value) {
			if (value + PrimaryStats.Dex > short.MaxValue) {
				PrimaryStats.Dex = short.MaxValue;
			}
			else {
				PrimaryStats.Dex += value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Dex, PrimaryStats.Dex);
		}

		public void SetDex(short value) {
			if (value > short.MaxValue) {
				PrimaryStats.Dex = short.MaxValue;
			}
			else {
				PrimaryStats.Dex = value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Dex, PrimaryStats.Dex);
		}

		public void AddInt(short value) {
			if (value + PrimaryStats.Int > short.MaxValue) {
				PrimaryStats.Int = short.MaxValue;
			}
			else {
				PrimaryStats.Int += value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Int, PrimaryStats.Int);
		}

		public void SetInt(short value) {
			if (value > short.MaxValue) {
				PrimaryStats.Int = short.MaxValue;
			}
			else {
				PrimaryStats.Int = value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Int, PrimaryStats.Int);
		}

		public void AddLuk(short value) {
			if (value + PrimaryStats.Luk > short.MaxValue) {
				PrimaryStats.Luk = short.MaxValue;
			}
			else {
				PrimaryStats.Luk += value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Luk, PrimaryStats.Luk);
		}

		public void SetLuk(short value) {
			if (value > short.MaxValue) {
				PrimaryStats.Luk = short.MaxValue;
			}
			else {
				PrimaryStats.Luk = value;
			}
			CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Luk, PrimaryStats.Luk);
		}

		public void loseEXP() {
			if (PrimaryStats.Job != 0 && PrimaryStats.Level < 200) {
				Map loc = DataProvider.Maps[Map];
				byte exploss = 10;
				if (loc.Town) {
					exploss = 1;
				}
				else {
					switch ((Constants.JobTracks.Tracks)Constants.getJobTrack(PrimaryStats.Job)) {
						case Constants.JobTracks.Tracks.Magician: exploss = 7; break;
						case Constants.JobTracks.Tracks.Thief: exploss = 5; break;
					}
				}
				int exp = PrimaryStats.EXP - ((Constants.GetLevelEXP(PrimaryStats.Level) * exploss) / 100);
				SetEXP(exp);
			}
		}

		public void ChangeMap(int mapid) {
			Map map = DataProvider.Maps[Map];
			Map newMap = DataProvider.Maps[mapid];
			if (MapChair != -1) {
				map.UsedSeats.Remove(MapChair);
				MapChair = -1;
				MapPacket.SendCharacterSit(this, -1);
			}

			// mSummons.RemoveSummon(false, 0x04);
			Summons.RemoveSummon(true, 0x04);

			map.RemovePlayer(this);

			PortalCount++;
			Map = mapid;

			Portal portal;
			Random rnd = new Random();
			portal = newMap.SpawnPoints[rnd.Next(0, newMap.SpawnPoints.Count)];
			MapPosition = portal.ID;

			Position = new Pos(portal.X, (short)(portal.Y - 40));
			Stance = 0;
			Foothold = 0;

			MapPacket.SendChangeMap(this);

			newMap.AddPlayer(this);

            if (Summons.mSummon != null)
            {
                SummonPacket.SendShowSummon(this, Summons.mSummon, true, null);
            }
		}

		public void ChangeMap(int mapid, byte mappos) {
			Map map = DataProvider.Maps[Map];
			Map newMap = DataProvider.Maps[mapid];
			if (MapChair != -1) {
				map.UsedSeats.Remove(MapChair);
				MapChair = -1;
				MapPacket.SendCharacterSit(this, -1);
			}

			Summons.RemoveSummon(false, 0x04);
			Summons.RemoveSummon(true, 0x04);

			map.RemovePlayer(this);

			PortalCount++;
			Map = mapid;
			MapPosition = mappos;

			Portal portal;
			if (newMap.SpawnPoints.ContainsKey(MapPosition)) {
				portal = newMap.SpawnPoints[MapPosition];
				MapPosition = portal.ID;
			}
			else {
				portal = newMap.SpawnPoints[0];
				MapPosition = 0;
			}

			Position = new Pos(portal.X, (short)(portal.Y - 40));
			Stance = 0;
			Foothold = 0;

			MapPacket.SendChangeMap(this);

			newMap.AddPlayer(this);

		}

		public void ChangeMap(int mapid, Portal to) {
			Map map = DataProvider.Maps[Map];
			Map newMap = DataProvider.Maps[mapid];
			if (MapChair != -1) {
				map.UsedSeats.Remove(MapChair);
				MapChair = -1;
				MapPacket.SendCharacterSit(this, -1);
			}

			Summons.RemoveSummon(false, 0x04); // Check later if these two lines of code play into the factor of the bug of losing your summon upon changing map. TODO
			Summons.RemoveSummon(true, 0x04);

			map.RemovePlayer(this);

			PortalCount++;
			Map = mapid;

			MapPosition = to.ID;

			Position = new Pos(to.X, (short)(to.Y - 40));
			Stance = 0;
			Foothold = 0;

			MapPacket.SendChangeMap(this);

			newMap.AddPlayer(this);
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

		public void SetHair(int id) {
			Hair = id;
			CharacterStatsPacket.SendStatChange(this, (int)CharacterStatsPacket.Constants.Hair, id);
		}

		public void SetFace(int id) {
			Face = id;
			CharacterStatsPacket.SendStatChange(this, (int)CharacterStatsPacket.Constants.Eyes, id);
		}

		public void SetSkin(byte id) {
			Skin = id;
			CharacterStatsPacket.SendStatChange(this, (byte)CharacterStatsPacket.Constants.Eyes, id);
		}

	}
}
