using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class CharacterBuffs
    {
        public Character mCharacter { get; set; }
        public byte mComboCount { get; set; }
        public Dictionary<int, byte> mActiveSkillLevels { get; set; }

        public CharacterBuffs(Character chr)
        {
            mCharacter = chr;
            mActiveSkillLevels = new Dictionary<int, byte>();
        }

        public bool HasGMHide()
        {
            return mCharacter.PrimaryStats.HasBuff((int)Constants.Gm.Skills.Hide);
        }

        public byte GetActiveSkillLevel(int skill)
        {
            return mActiveSkillLevels.ContainsKey(skill) ? mActiveSkillLevels[skill] : (byte)0;
        }

        public void AddItemBuff(int itemid)
        {
            ItemData data = DataProvider.Items[itemid];
            long time = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate) + (data.BuffTime * 1000);

            CharacterPrimaryStats ps = mCharacter.PrimaryStats;
            uint added = 0;
            if (data.Accuracy > 0)
            {
                ps.Accurancy_N = data.Accuracy;
                ps.Accurancy_R = -itemid;
                ps.Accurancy_T = time;
                added |= (uint)BuffValueTypes.Accurancy;
            }
            if (data.Avoidance > 0)
            {
                ps.Avoidability_N = data.Avoidance;
                ps.Avoidability_R = -itemid;
                ps.Avoidability_T = time;
                added |= (uint)BuffValueTypes.Avoidability;
            }
            if (data.Speed > 0)
            {
                ps.Speed_N = data.Speed;
                ps.Speed_R = -itemid;
                ps.Speed_T = time;
                added |= (uint)BuffValueTypes.Speed;
            }
            if (data.MagicAttack > 0)
            {
                ps.MagicAttack_N = data.MagicAttack;
                ps.MagicAttack_R = -itemid;
                ps.MagicAttack_T = time;
                added |= (uint)BuffValueTypes.MagicAttack;
            }
            if (data.WeaponAttack > 0)
            {
                ps.WeaponAttack_N = data.WeaponAttack;
                ps.WeaponAttack_R = -itemid;
                ps.WeaponAttack_T = time;
                added |= (uint)BuffValueTypes.WeaponAttack;
            }


            BuffPacket.SetTempStats(mCharacter, added);
            MapPacket.SendPlayerBuffed(mCharacter, added);
        }

        public void AddBuffFromCC(int SkillID, long Time2, byte level = 0xFF, int sinc1 = 0, int sinc2 = 0)
        {
            if (!BuffDataProvider.mSkillBuffValues.ContainsKey(SkillID)) return;

            List<BuffValueTypes> flags = BuffDataProvider.mSkillBuffValues[SkillID];
            SkillLevelData data = null;

            foreach (BuffValueTypes type in flags)
            {
                Console.WriteLine(type.ToString());
            }
            if (level == 0xFF)
            {
                byte skillLevel = (byte)mCharacter.Skills.mSkills[SkillID];
                data = DataProvider.Skills[SkillID][skillLevel];
            }
            else
            {
                data = DataProvider.Skills[SkillID][level];
            }
            Check(SkillID);
            long addedtime = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate);
            long time = 0;
            long time1 = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate) + (data.BuffTime * 1000);

            time = Time2;
            Console.WriteLine("time for buff : " + time.ToString());
            CharacterPrimaryStats ps = mCharacter.PrimaryStats;
            uint added = 0;


            if (flags.Contains(BuffValueTypes.WeaponAttack))
            {
                ps.WeaponAttack_N = data.WeaponAttack;
                ps.WeaponAttack_R = SkillID;
                ps.WeaponAttack_T = time;
                added |= (uint)BuffValueTypes.WeaponAttack;
            }
            if (flags.Contains(BuffValueTypes.WeaponDefense))
            {
                ps.WeaponDefense_N = data.WeaponDefense;
                ps.WeaponDefense_R = SkillID;
                ps.WeaponDefense_T = time;
                added |= (uint)BuffValueTypes.WeaponDefense;
            }
            if (flags.Contains(BuffValueTypes.MagicAttack))
            {
                ps.MagicAttack_N = data.MagicAttack;
                ps.MagicAttack_R = SkillID;
                ps.MagicAttack_T = time;
                added |= (uint)BuffValueTypes.MagicAttack;
            }
            if (flags.Contains(BuffValueTypes.MagicDefense))
            {
                ps.MagicDefense_N = data.MagicDefense;
                ps.MagicDefense_R = SkillID;
                ps.MagicDefense_T = time;
                added |= (uint)BuffValueTypes.MagicDefense;
            }
            if (flags.Contains(BuffValueTypes.Accurancy))
            {
                ps.Accurancy_N = data.Accurancy;
                ps.Accurancy_R = SkillID;
                ps.Accurancy_T = time;
                added |= (uint)BuffValueTypes.Accurancy;
            }
            if (flags.Contains(BuffValueTypes.Avoidability))
            {
                ps.Avoidability_N = data.Avoidability;
                ps.Avoidability_R = SkillID;
                ps.Avoidability_T = time;
                added |= (uint)BuffValueTypes.Avoidability;
            }
            /*
            if (flags.Contains(BuffValueTypes.Hands))
            {
                ps.Hands_N = data.Hands;
                ps.Hands_R = SkillID;
                ps.Hands_T = time;
                added |= (uint)BuffValueTypes.Hands;
            }
            */
            if (flags.Contains(BuffValueTypes.Speed))
            {
                ps.Speed_N = data.Speed;
                ps.Speed_R = SkillID;
                ps.Speed_T = time;
                added |= (uint)BuffValueTypes.Speed;
            }
            if (flags.Contains(BuffValueTypes.Jump))
            {
                ps.Jump_N = data.Jump;
                ps.Jump_R = SkillID;
                ps.Jump_T = time;
                added |= (uint)BuffValueTypes.Jump;
            }
            if (flags.Contains(BuffValueTypes.MagicGuard))
            {
                ps.MagicGuard_N = data.XValue;
                ps.MagicGuard_R = SkillID;
                ps.MagicGuard_T = time;
                added |= (uint)BuffValueTypes.MagicGuard;
            }
            if (flags.Contains(BuffValueTypes.DarkSight))
            {
                ps.DarkSight_N = data.XValue;
                ps.DarkSight_R = SkillID;
                ps.DarkSight_T = time;
                added |= (uint)BuffValueTypes.DarkSight;
            }
            if (flags.Contains(BuffValueTypes.Booster))
            {
                ps.Booster_N = data.XValue;
                ps.Booster_R = SkillID;
                ps.Booster_T = time;
                added |= (uint)BuffValueTypes.Booster;
            }
            if (flags.Contains(BuffValueTypes.PowerGuard))
            {
                ps.PowerGuard_N = data.XValue;
                ps.PowerGuard_R = SkillID;
                ps.PowerGuard_T = time;
                added |= (uint)BuffValueTypes.PowerGuard;
            }
            if (flags.Contains(BuffValueTypes.MaxHP))
            {
                ps.MaxHP_N = data.XValue;
                ps.MaxHP_R = SkillID;
                ps.MaxHP_T = time;
                added |= (uint)BuffValueTypes.MaxHP;
            }
            if (flags.Contains(BuffValueTypes.MaxMP))
            {
                ps.MaxMP_N = data.XValue;
                ps.MaxMP_R = SkillID;
                ps.MaxMP_T = time;
                added |= (uint)BuffValueTypes.MaxMP;
            }
            if (flags.Contains(BuffValueTypes.Invincible))
            {
                ps.Invincible_N = data.XValue;
                ps.Invincible_R = SkillID;
                ps.Invincible_T = time;
                added |= (uint)BuffValueTypes.Invincible;
            }
            if (flags.Contains(BuffValueTypes.SoulArrow))
            {
                ps.SoulArrow_N = data.XValue;
                ps.SoulArrow_R = SkillID;
                ps.SoulArrow_T = time;
                added |= (uint)BuffValueTypes.SoulArrow;
            }
            if (flags.Contains(BuffValueTypes.ComboAttack))
            {
                ps.ComboAttack_N = 1;
                ps.ComboAttack_R = SkillID;
                ps.ComboAttack_T = time;
                added |= (uint)BuffValueTypes.ComboAttack;
            }
            if (flags.Contains(BuffValueTypes.Charges))
            {
                ps.Charges_N = data.XValue;
                ps.Charges_R = SkillID;
                ps.Charges_T = time;
                added |= (uint)BuffValueTypes.Charges;
            }
            if (flags.Contains(BuffValueTypes.DragonBlood))
            {
                ps.DragonBlood_N = data.XValue;
                ps.DragonBlood_R = SkillID;
                ps.DragonBlood_T = time;
                added |= (uint)BuffValueTypes.DragonBlood;
            }
            if (flags.Contains(BuffValueTypes.MesoUP))
            {
                ps.MesoUP_N = data.XValue;
                ps.MesoUP_R = SkillID;
                ps.MesoUP_T = time;
                added |= (uint)BuffValueTypes.MesoUP;
            }
            if (flags.Contains(BuffValueTypes.ShadowPartner))
            {
                ps.ShadowPartner_N = data.XValue;
                ps.ShadowPartner_R = SkillID;
                ps.ShadowPartner_T = time;
                added |= (uint)BuffValueTypes.ShadowPartner;
            }
            if (flags.Contains(BuffValueTypes.PickPocketMesoUP))
            {
                ps.PickPocketMesoUP_N = data.XValue;
                ps.PickPocketMesoUP_R = SkillID;
                ps.PickPocketMesoUP_T = time;
                added |= (uint)BuffValueTypes.PickPocketMesoUP;
            }
            if (flags.Contains(BuffValueTypes.MesoGuard))
            {
                ps.MesoGuard_N = data.XValue;
                ps.MesoGuard_R = SkillID;
                ps.MesoGuard_T = time;
                added |= (uint)BuffValueTypes.MesoGuard;
            }
            
            BuffPacket.SetTempStats(mCharacter, added);
            //MapPacket.SendPlayerBuffed(mCharacter, added);
            //ps.ActiveBuffs.Add(ps, due);
            
            DateTime due = (MasterThread.CurrentDate).AddMilliseconds(data.BuffTime * 1000);
            if (SkillID != (int)Constants.Spearman.Skills.HyperBody)
            {
                SaveBuffs(mCharacter.ID, SkillID, time, added, due, level);
            }
            else
            {
                
                //SaveBuffs(mCharacter.ID, SkillID, time, added, due, level, sinc1, sinc2);
            }
        }

        public void AddBuff(int SkillID, byte level = 0xFF, int sinc1 = 0, int sinc2 = 0)
        {

            if (!BuffDataProvider.mSkillBuffValues.ContainsKey(SkillID)) return;

            List<BuffValueTypes> flags = BuffDataProvider.mSkillBuffValues[SkillID];
            foreach (BuffValueTypes type in flags)
            {
                Console.WriteLine(type.ToString());
            }
            SkillLevelData data = null;
            if (level == 0xFF)
            {
                byte skillLevel = (byte)mCharacter.Skills.mSkills[SkillID];
                data = DataProvider.Skills[SkillID][skillLevel];
            }
            else
            {
                data = DataProvider.Skills[SkillID][level];
                foreach(BuffValueTypes type in flags)
                {
                    Console.WriteLine(type.ToString());
                }
            }
            Check(SkillID);
            long addedtime = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate);
            long time = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate) + (data.BuffTime * 1000);
            Console.WriteLine("time for buff : " + time.ToString());
            CharacterPrimaryStats ps = mCharacter.PrimaryStats;
            uint added = 0;

            //if (SkillID = 
            if (flags.Contains(BuffValueTypes.WeaponAttack))
            {
                //MessagePacket.SendNotice("weapon attack added!", mCharacter);
                ps.WeaponAttack_N = data.WeaponAttack;
                ps.WeaponAttack_R = SkillID;
                ps.WeaponAttack_T = time;
                added |= (uint)BuffValueTypes.WeaponAttack;
            }
            if (flags.Contains(BuffValueTypes.WeaponDefense))
            {
               // MessagePacket.SendNotice("weapon defense added!", mCharacter);
                ps.WeaponDefense_N = data.WeaponDefense;
                ps.WeaponDefense_R = SkillID;
                ps.WeaponDefense_T = time;
                added |= (uint)BuffValueTypes.WeaponDefense;
            }
            if (flags.Contains(BuffValueTypes.MagicAttack))
            {
                ps.MagicAttack_N = data.MagicAttack;
                ps.MagicAttack_R = SkillID;
                ps.MagicAttack_T = time;
                added |= (uint)BuffValueTypes.MagicAttack;
            }
            if (flags.Contains(BuffValueTypes.MagicDefense))
            {
                ps.MagicDefense_N = data.MagicDefense;
                ps.MagicDefense_R = SkillID;
                ps.MagicDefense_T = time;
                added |= (uint)BuffValueTypes.MagicDefense;
            }
            if (flags.Contains(BuffValueTypes.Accurancy))
            {
                ps.Accurancy_N = data.Accurancy;
                ps.Accurancy_R = SkillID;
                ps.Accurancy_T = time;
                added |= (uint)BuffValueTypes.Accurancy;
            }
            if (flags.Contains(BuffValueTypes.Avoidability))
            {
                ps.Avoidability_N = data.Avoidability;
                ps.Avoidability_R = SkillID;
                ps.Avoidability_T = time;
                added |= (uint)BuffValueTypes.Avoidability;
            }
            /*
            if (flags.Contains(BuffValueTypes.Hands))
            {
                ps.Hands_N = data.Hands;
                ps.Hands_R = SkillID;
                ps.Hands_T = time;
                added |= (uint)BuffValueTypes.Hands;
            }
            */
            if (flags.Contains(BuffValueTypes.Speed))
            {
                ps.Speed_N = data.Speed;
                ps.Speed_R = SkillID;
                ps.Speed_T = time;
                added |= (uint)BuffValueTypes.Speed;
            }
            if (flags.Contains(BuffValueTypes.Jump))
            {
                ps.Jump_N = data.Jump;
                ps.Jump_R = SkillID;
                ps.Jump_T = time;
                added |= (uint)BuffValueTypes.Jump;
            }
            if (flags.Contains(BuffValueTypes.MagicGuard))
            {
                ps.MagicGuard_N = data.XValue;
                ps.MagicGuard_R = SkillID;
                ps.MagicGuard_T = time;
                added |= (uint)BuffValueTypes.MagicGuard;
            }
            if (flags.Contains(BuffValueTypes.DarkSight))
            {
                ps.DarkSight_N = data.XValue;
                ps.DarkSight_R = SkillID;
                ps.DarkSight_T = time;
                added |= (uint)BuffValueTypes.DarkSight;
            }
            if (flags.Contains(BuffValueTypes.Booster))
            {
                ps.Booster_N = data.XValue;
                ps.Booster_R = SkillID;
                ps.Booster_T = time;
                added |= (uint)BuffValueTypes.Booster;
            }
            if (flags.Contains(BuffValueTypes.PowerGuard))
            {
                ps.PowerGuard_N = data.XValue;
                ps.PowerGuard_R = SkillID;
                ps.PowerGuard_T = time;
                added |= (uint)BuffValueTypes.PowerGuard;
            }
            if (flags.Contains(BuffValueTypes.MaxHP))
            {
                ps.MaxHP_N = data.XValue;
                ps.MaxHP_R = SkillID;
                ps.MaxHP_T = time;
                added |= (uint)BuffValueTypes.MaxHP;
            }
            if (flags.Contains(BuffValueTypes.MaxMP))
            {
                ps.MaxMP_N = data.XValue; 
                ps.MaxMP_R = SkillID;
                ps.MaxMP_T = time;
                added |= (uint)BuffValueTypes.MaxMP;
            }
            if (flags.Contains(BuffValueTypes.Invincible))
            {
                ps.Invincible_N = data.XValue;
                ps.Invincible_R = SkillID;
                ps.Invincible_T = time;
                added |= (uint)BuffValueTypes.Invincible;
            }
            if (flags.Contains(BuffValueTypes.SoulArrow))
            {
                ps.SoulArrow_N = data.XValue;
                ps.SoulArrow_R = SkillID;
                ps.SoulArrow_T = time;
                added |= (uint)BuffValueTypes.SoulArrow;
            }
            if (flags.Contains(BuffValueTypes.ComboAttack))
            {
                ps.ComboAttack_N = 1;
                ps.ComboAttack_R = SkillID;
                ps.ComboAttack_T = time;
                added |= (uint)BuffValueTypes.ComboAttack;
            }
            if (flags.Contains(BuffValueTypes.Charges))
            {
                ps.Charges_N = data.XValue;
                ps.Charges_R = SkillID;
                ps.Charges_T = time;
                added |= (uint)BuffValueTypes.Charges;
            }
            if (flags.Contains(BuffValueTypes.DragonBlood))
            {
                ps.DragonBlood_N = data.XValue;
                ps.DragonBlood_R = SkillID;
                ps.DragonBlood_T = time;
                added |= (uint)BuffValueTypes.DragonBlood;
            }
            if (flags.Contains(BuffValueTypes.MesoUP))
            {
                ps.MesoUP_N = data.XValue;
                ps.MesoUP_R = SkillID;
                ps.MesoUP_T = time;
                added |= (uint)BuffValueTypes.MesoUP;
            }
            if (flags.Contains(BuffValueTypes.ShadowPartner))
            {
                ps.ShadowPartner_N = data.XValue;
                ps.ShadowPartner_R = SkillID;
                ps.ShadowPartner_T = time;
                added |= (uint)BuffValueTypes.ShadowPartner;
            }
            if (flags.Contains(BuffValueTypes.PickPocketMesoUP))
            {
                ps.PickPocketMesoUP_N = data.XValue;
                ps.PickPocketMesoUP_R = SkillID;
                ps.PickPocketMesoUP_T = time;
                added |= (uint)BuffValueTypes.PickPocketMesoUP;
            }
            if (flags.Contains(BuffValueTypes.MesoGuard))
            {
                ps.MesoGuard_N = data.XValue;
                ps.MesoGuard_R = SkillID;
                ps.MesoGuard_T = time;
                added |= (uint)BuffValueTypes.MesoGuard;
            }

            //MessagePacket.SendNotice("added : " + added.ToString(), mCharacter);
            BuffPacket.SetTempStats(mCharacter, added);
            //MapPacket.SendPlayerBuffed(mCharacter, added);
            //ps.ActiveBuffs.Add(ps, due);

            DateTime due = (MasterThread.CurrentDate).AddMilliseconds(data.BuffTime * 1000);
            Console.WriteLine(due.TimeOfDay.ToString());
            if (SkillID != (int)Constants.Spearman.Skills.HyperBody)
            {
                SaveBuffs(mCharacter.ID, SkillID, time, added, due, level);
            }
            else
            {
                SaveBuffs(mCharacter.ID, SkillID, time, added, due, level, sinc1, sinc2);
            }
        }

        public void Check(int BuffID)
        {
            if (BuffID == 1101006 && mCharacter.PrimaryStats.HasBuff(1001003))
            {
                Console.WriteLine("hmm remove by value");
                mCharacter.PrimaryStats.RemoveByValue(1001003);
            }
        }

        public void SaveBuffs(int CID, int BuffID, long Time, uint flags, DateTime Due, int level, int sinc = 0, int sinc2 = 0)
        {
            MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_buffs WHERE cid = " + mCharacter.ID.ToString() + " AND bid = " + BuffID.ToString()) as MySqlDataReader;
            if (!data.HasRows)
            {
                Server.Instance.CharacterDatabase.RunQuery("INSERT INTO character_buffs (bid, cid, time, flags, level, sinc, sinc2) VALUES (" + BuffID.ToString() + ", " + mCharacter.ID.ToString() + ", " + Time.ToString() + ", " + flags.ToString() + ", " + level.ToString() + ", " + sinc.ToString() + ", " + sinc2.ToString() + ")");
            }
            else
            {
                Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_buffs WHERE cid = " + mCharacter.ID.ToString() + " AND bid = " + BuffID.ToString());
                Server.Instance.CharacterDatabase.RunQuery("INSERT INTO character_buffs (bid, cid, time, flags, level, sinc, sinc2) VALUES (" + BuffID.ToString() + ", " + mCharacter.ID.ToString() + ", " + Time.ToString() + ", " + flags.ToString() + ", " + level.ToString() + ", " + sinc.ToString() + ", " + sinc2.ToString() + ")"); 
            }
        }

        public void LoadBuffs()
        {
            Dictionary<int, Buff> Buffs = new Dictionary<int, Buff>();
            
            MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_buffs WHERE cid = " + mCharacter.ID.ToString()) as MySqlDataReader;
            if (data.HasRows)
            {
                while (data.Read())
                {
                    //data.GetDateTime
                    int bid = data.GetInt32("bid");
                    long time = data.GetInt64("time");
                    int level = data.GetInt32("level");
                    Buff lol = new Buff(level, time);
                    Buffs.Add(bid, lol);
                    Console.WriteLine("Buff added!!!");
                }
            }
            foreach (KeyValuePair<int, Buff> kvp in Buffs)
            {
                    AddBuffFromCC(kvp.Key, kvp.Value.mTime, (byte)kvp.Value.mLevel);
              
            }
        }
    }

    public class Buff
    {
        public int mLevel { get; set; }
        public long mTime { get; set; }

        public Buff(int Level, long Time)
        {
            mLevel = Level;
            mTime = Time;
        }
    }
}