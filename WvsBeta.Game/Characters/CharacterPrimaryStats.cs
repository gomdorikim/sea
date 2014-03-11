using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public struct PrimaryStatsAddition
    {
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

    public class BonusSet
    {
        public short Str { get; set; }
        public short Dex { get; set; }
        public short Int { get; set; }
        public short Luk { get; set; }
        public short MaxHP { get; set; }
        public short MaxMP { get; set; }
        public short Speed { get; set; }
        public BonusSet()
        {
            Str = 0;
            Dex = 0;
            Int = 0;
            MaxHP = 0;
            MaxMP = 0;
            Speed = 0;
        }
    }

    public class EquipBonus : BonusSet
    {
        public int ID { get; set; }
    }


    public class CharacterPrimaryStats
    {
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
        public int BuddyListCapacity { get; set; }
        public int HasTest { get; set; }
        public short BuffMHP { get; set; }

        public Dictionary<short, EquipBonus> EquipStats;
        public BonusSet EquipBonuses;
        public BonusSet BuffBonuses;

        public Dictionary<CharacterPrimaryStats, DateTime> ActiveBuffs { get; set; }
        private Character Char { get; set; }


        // Real Stats

        public int WeaponAttack_N { get; set; }
        public int WeaponAttack_R { get; set; }
        public long WeaponAttack_T { get; set; }
        public int WeaponDefense_N { get; set; }
        public int WeaponDefense_R { get; set; }
        public long WeaponDefense_T { get; set; }
        public int MagicAttack_N { get; set; }
        public int MagicAttack_R { get; set; }
        public long MagicAttack_T { get; set; }
        public int MagicDefense_N { get; set; }
        public int MagicDefense_R { get; set; }
        public long MagicDefense_T { get; set; }
        public int Accurancy_N { get; set; }
        public int Accurancy_R { get; set; }
        public long Accurancy_T { get; set; }
        public int Avoidability_N { get; set; }
        public int Avoidability_R { get; set; }
        public long Avoidability_T { get; set; }
        public int Hands_N { get; set; }
        public int Hands_R { get; set; }
        public long Hands_T { get; set; }
        public int Speed_N { get; set; }
        public int Speed_R { get; set; }
        public long Speed_T { get; set; }
        public int Jump_N { get; set; }
        public int Jump_R { get; set; }
        public long Jump_T { get; set; }
        public int MagicGuard_N { get; set; }
        public int MagicGuard_R { get; set; }
        public long MagicGuard_T { get; set; }
        public int DarkSight_N { get; set; }
        public int DarkSight_R { get; set; }
        public long DarkSight_T { get; set; }
        public int Booster_N { get; set; }
        public int Booster_R { get; set; }
        public long Booster_T { get; set; }
        public int PowerGuard_N { get; set; }
        public int PowerGuard_R { get; set; }
        public long PowerGuard_T { get; set; }
        public int MaxHP_N { get; set; }
        public int MaxHP_R { get; set; }
        public long MaxHP_T { get; set; }
        public int MaxMP_N { get; set; }
        public int MaxMP_R { get; set; }
        public long MaxMP_T { get; set; }
        public int Invincible_N { get; set; }
        public int Invincible_R { get; set; }
        public long Invincible_T { get; set; }
        public int SoulArrow_N { get; set; }
        public int SoulArrow_R { get; set; }
        public long SoulArrow_T { get; set; }
        public int Stun_N { get; set; }
        public int Stun_R { get; set; }
        public long Stun_T { get; set; }
        public int Poison_N { get; set; }
        public int Poison_R { get; set; }
        public long Poison_T { get; set; }
        public int Seal_N { get; set; }
        public int Seal_R { get; set; }
        public long Seal_T { get; set; }
        public int Darkness_N { get; set; }
        public int Darkness_R { get; set; }
        public long Darkness_T { get; set; }
        public int ComboAttack_N { get; set; }
        public int ComboAttack_R { get; set; }
        public long ComboAttack_T { get; set; }
        public int Charges_N { get; set; }
        public int Charges_R { get; set; }
        public long Charges_T { get; set; }
        public int DragonBlood_N { get; set; }
        public int DragonBlood_R { get; set; }
        public long DragonBlood_T { get; set; }
        public int HolySymbol_N { get; set; }
        public int HolySymbol_R { get; set; }
        public long HolySymbol_T { get; set; }
        public int MesoUP_N { get; set; }
        public int MesoUP_R { get; set; }
        public long MesoUP_T { get; set; }
        public int ShadowPartner_N { get; set; }
        public int ShadowPartner_R { get; set; }
        public long ShadowPartner_T { get; set; }
        public int PickPocketMesoUP_N { get; set; }
        public int PickPocketMesoUP_R { get; set; }
        public long PickPocketMesoUP_T { get; set; }
        public int MesoGuard_N { get; set; }
        public int MesoGuard_R { get; set; }
        public long MesoGuard_T { get; set; }
        public int Thaw_N { get; set; }
        public int Thaw_R { get; set; }
        public long Thaw_T { get; set; }
        public int Weakness_N { get; set; }
        public int Weakness_R { get; set; }
        public long Weakness_T { get; set; }
        public int Curse_N { get; set; }
        public int Curse_R { get; set; }
        public long Curse_T { get; set; }


        public CharacterPrimaryStats(Character chr)
        {
            EquipStats = new Dictionary<short, EquipBonus>();
            EquipBonuses = new BonusSet();
            BuffBonuses = new BonusSet();
            Char = chr;
            Reset();  
        }


        public void AddEquipStarts(short slot, Item equip, bool isLoading)
        {
            try
            {
                slot = Math.Abs(slot);
                if (equip != null)
                {
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
                else
                {
                    EquipStats.Remove(slot);
                }
                CalculateAdditions(true, isLoading);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void CalculateAdditions(bool updateEquips, bool isLoading)
        {
            if (updateEquips)
            {
                EquipBonuses = null;
                EquipBonuses = new BonusSet();
                EquipBonus item;
                foreach (KeyValuePair<short, EquipBonus> data in EquipStats)
                {
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
            if (!isLoading)
            {
                CheckHPMP();
            }
        }

        public void CheckHPMP()
        {
            if (HP > GetMaxHP(false))
            {
                Char.SetHP(HP);
            }
            if (MP > GetMaxMP(false))
            {
                Char.SetMP(MP);
            }
        }

        public short GetStrAddition(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((Str + EquipBonuses.Str + BuffBonuses.Str) > short.MaxValue ? short.MaxValue : (Str + EquipBonuses.Str + BuffBonuses.Str));
            }
            return Str;
        }
        public short GetDexAddition(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((Dex + EquipBonuses.Dex + BuffBonuses.Dex) > short.MaxValue ? short.MaxValue : (Dex + EquipBonuses.Dex + BuffBonuses.Dex));
            }
            return Dex;
        }
        public short GetIntAddition(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((Int + EquipBonuses.Int + BuffBonuses.Int) > short.MaxValue ? short.MaxValue : (Int + EquipBonuses.Int + BuffBonuses.Int));
            }
            return Int;
        }
        public short GetLukAddition(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((Luk + EquipBonuses.Luk + BuffBonuses.Luk) > short.MaxValue ? short.MaxValue : (Luk + EquipBonuses.Luk + BuffBonuses.Luk));
            }
            return Luk;
        }
        public short GetMaxHP(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((MaxHP + EquipBonuses.MaxHP + BuffBonuses.MaxHP) > short.MaxValue ? short.MaxValue : (MaxHP + EquipBonuses.MaxHP + BuffBonuses.MaxHP));
            }
            return MaxHP;
        }
        public short GetMaxMP(bool nobonus = false)
        {
            if (!nobonus)
            {
                return (short)((MaxMP + EquipBonuses.MaxMP + BuffBonuses.MaxMP) > short.MaxValue ? short.MaxValue : (MaxMP + EquipBonuses.MaxMP + BuffBonuses.MaxMP));
            }
            return MaxMP;
        }

        public void SetSpeed(byte pSpeed)
        {
            this.Speed = pSpeed;
            speedMod = pSpeed / 100.0f;
        }


        public void Reset()
        {
            WeaponAttack_N = 0;
            WeaponAttack_R = 0;
            WeaponAttack_T = 0;
            WeaponDefense_N = 0;
            WeaponDefense_R = 0;
            WeaponDefense_T = 0;
            MagicAttack_N = 0;
            MagicAttack_R = 0;
            MagicAttack_T = 0;
            MagicDefense_N = 0;
            MagicDefense_R = 0;
            MagicDefense_T = 0;
            Accurancy_N = 0;
            Accurancy_R = 0;
            Accurancy_T = 0;
            Avoidability_N = 0;
            Avoidability_R = 0;
            Avoidability_T = 0;
            Hands_N = 0;
            Hands_R = 0;
            Hands_T = 0;
            Speed_N = 0;
            Speed_R = 0;
            Speed_T = 0;
            Jump_N = 0;
            Jump_R = 0;
            Jump_T = 0;
            MagicGuard_N = 0;
            MagicGuard_R = 0;
            MagicGuard_T = 0;
            DarkSight_N = 0;
            DarkSight_R = 0;
            DarkSight_T = 0;
            Booster_N = 0;
            Booster_R = 0;
            Booster_T = 0;
            PowerGuard_N = 0;
            PowerGuard_R = 0;
            PowerGuard_T = 0;
            MaxHP_N = 0;
            MaxHP_R = 0;
            MaxHP_T = 0;
            MaxMP_N = 0;
            MaxMP_R = 0;
            MaxMP_T = 0;
            Invincible_N = 0;
            Invincible_R = 0;
            Invincible_T = 0;
            SoulArrow_N = 0;
            SoulArrow_R = 0;
            SoulArrow_T = 0;
            Stun_N = 0;
            Stun_R = 0;
            Stun_T = 0;
            Poison_N = 0;
            Poison_R = 0;
            Poison_T = 0;
            Seal_N = 0;
            Seal_R = 0;
            Seal_T = 0;
            Darkness_N = 0;
            Darkness_R = 0;
            Darkness_T = 0;
            ComboAttack_N = 0;
            ComboAttack_R = 0;
            ComboAttack_T = 0;
            Charges_N = 0;
            Charges_R = 0;
            Charges_T = 0;
            DragonBlood_N = 0;
            DragonBlood_R = 0;
            DragonBlood_T = 0;
            HolySymbol_N = 0;
            HolySymbol_R = 0;
            HolySymbol_T = 0;
            MesoUP_N = 0;
            MesoUP_R = 0;
            MesoUP_T = 0;
            ShadowPartner_N = 0;
            ShadowPartner_R = 0;
            ShadowPartner_T = 0;
            PickPocketMesoUP_N = 0;
            PickPocketMesoUP_R = 0;
            PickPocketMesoUP_T = 0;
            MesoGuard_N = 0;
            MesoGuard_R = 0;
            MesoGuard_T = 0;
            Thaw_N = 0;
            Thaw_R = 0;
            Thaw_T = 0;
            Weakness_N = 0;
            Weakness_R = 0;
            Weakness_T = 0;
            Curse_N = 0;
            Curse_R = 0;
            Curse_T = 0;
        }

        public void CheckExpired(DateTime pNow)
        {
            //Console.WriteLine("check expired..");
            long currentTime = Tools.GetTimeAsMilliseconds(pNow);
            uint endFlag = 0;

            if (WeaponAttack_N > 0 && currentTime - WeaponAttack_T > 0)
            {
                Console.WriteLine("added weaponattack to endFlag");
                WeaponAttack_N = 0;
                WeaponAttack_R = 0;
                WeaponAttack_T = 0;
                endFlag |= (uint)BuffValueTypes.WeaponAttack;
            }
            if ((WeaponDefense_N > 0 || WeaponDefense_N < 0) && currentTime - WeaponDefense_T > 0)
            {
                Console.WriteLine("added weapondefense to endFlag");
                WeaponDefense_N = 0;
                WeaponDefense_R = 0;
                WeaponDefense_T = 0;
                endFlag |= (uint)BuffValueTypes.WeaponDefense;
            }
            if (MagicAttack_N > 0 && currentTime - MagicAttack_T > 0)
            {
                MagicAttack_N = 0;
                MagicAttack_R = 0;
                MagicAttack_T = 0;
                endFlag |= (uint)BuffValueTypes.MagicAttack;
            }
            if (MagicDefense_N > 0 && currentTime - MagicDefense_T > 0)
            {
                MagicDefense_N = 0;
                MagicDefense_R = 0;
                MagicDefense_T = 0;
                endFlag |= (uint)BuffValueTypes.MagicDefense;
            }
            if (Accurancy_N > 0 && currentTime - Accurancy_T > 0)
            {
                Accurancy_N = 0;
                Accurancy_R = 0;
                Accurancy_T = 0;
                endFlag |= (uint)BuffValueTypes.Accurancy;
            }
            if (Avoidability_N > 0 && currentTime - Avoidability_T > 0)
            {
                Avoidability_N = 0;
                Avoidability_R = 0;
                Avoidability_T = 0;
                endFlag |= (uint)BuffValueTypes.Avoidability;
            }
            if (Hands_N > 0 && currentTime - Hands_T > 0)
            {
                Hands_N = 0;
                Hands_R = 0;
                Hands_T = 0;
                endFlag |= (uint)BuffValueTypes.Hands;
            }
            if (Speed_N > 0 && currentTime - Speed_T > 0)
            {
                Speed_N = 0;
                Speed_R = 0;
                Speed_T = 0;
                endFlag |= (uint)BuffValueTypes.Speed;
            }
            if (Jump_N > 0 && currentTime - Jump_T > 0)
            {
                Jump_N = 0;
                Jump_R = 0;
                Jump_T = 0;
                endFlag |= (uint)BuffValueTypes.Jump;
            }
            if (MagicGuard_N > 0 && currentTime - MagicGuard_T > 0)
            {
                MagicGuard_N = 0;
                MagicGuard_R = 0;
                MagicGuard_T = 0;
                endFlag |= (uint)BuffValueTypes.MagicGuard;
            }
            if (DarkSight_N > 0 && currentTime - DarkSight_T > 0)
            {
                DarkSight_N = 0;
                DarkSight_R = 0;
                DarkSight_T = 0;
                endFlag |= (uint)BuffValueTypes.DarkSight;
            }
            if (Booster_N > 0 && currentTime - Booster_T > 0)
            {
                Booster_N = 0;
                Booster_R = 0;
                Booster_T = 0;
                endFlag |= (uint)BuffValueTypes.Booster;
            }
            if (PowerGuard_N > 0 && currentTime - PowerGuard_T > 0)
            {
                PowerGuard_N = 0;
                PowerGuard_R = 0;
                PowerGuard_T = 0;
                endFlag |= (uint)BuffValueTypes.PowerGuard;
            }
            if (MaxHP_N > 0 && currentTime - MaxHP_T > 0)
            {
                MaxHP_N = 0;
                MaxHP_R = 0;
                MaxHP_T = 0;
                endFlag |= (uint)BuffValueTypes.MaxHP;
            }
            if (MaxMP_N > 0 && currentTime - MaxMP_T > 0)
            {
                MaxMP_N = 0;
                MaxMP_R = 0;
                MaxMP_T = 0;
                endFlag |= (uint)BuffValueTypes.MaxMP;
            }
            if (Invincible_N > 0 && currentTime - Invincible_T > 0)
            {
                Invincible_N = 0;
                Invincible_R = 0;
                Invincible_T = 0;
                endFlag |= (uint)BuffValueTypes.Invincible;
            }
            if (SoulArrow_N > 0 && currentTime - SoulArrow_T > 0)
            {
                SoulArrow_N = 0;
                SoulArrow_R = 0;
                SoulArrow_T = 0;
                endFlag |= (uint)BuffValueTypes.SoulArrow;
            }
            if (Stun_N > 0 && currentTime - Stun_T > 0)
            {
                Stun_N = 0;
                Stun_R = 0;
                Stun_T = 0;
                endFlag |= (uint)BuffValueTypes.Stun;
            }
            if (Poison_N > 0 && currentTime - Poison_T > 0)
            {
                Poison_N = 0;
                Poison_R = 0;
                Poison_T = 0;
                endFlag |= (uint)BuffValueTypes.Poison;
            }
            if (Seal_N > 0 && currentTime - Seal_T > 0)
            {
                Seal_N = 0;
                Seal_R = 0;
                Seal_T = 0;
                endFlag |= (uint)BuffValueTypes.Seal;
            }
            if (Darkness_N > 0 && currentTime - Darkness_T > 0)
            {
                Darkness_N = 0;
                Darkness_R = 0;
                Darkness_T = 0;
                endFlag |= (uint)BuffValueTypes.Darkness;
            }
            if (ComboAttack_N > 0 && currentTime - ComboAttack_T > 0)
            {
                ComboAttack_N = 0;
                ComboAttack_R = 0;
                ComboAttack_T = 0;
                endFlag |= (uint)BuffValueTypes.ComboAttack;
            }
            if (Charges_N > 0 && currentTime - Charges_T > 0)
            {
                Charges_N = 0;
                Charges_R = 0;
                Charges_T = 0;
                endFlag |= (uint)BuffValueTypes.Charges;
            }
            if (DragonBlood_N > 0 && currentTime - DragonBlood_T > 0)
            {
                DragonBlood_N = 0;
                DragonBlood_R = 0;
                DragonBlood_T = 0;
                endFlag |= (uint)BuffValueTypes.DragonBlood;
            }
            if (HolySymbol_N > 0 && currentTime - HolySymbol_T > 0)
            {
                HolySymbol_N = 0;
                HolySymbol_R = 0;
                HolySymbol_T = 0;
                endFlag |= (uint)BuffValueTypes.HolySymbol;
            }
            if (MesoUP_N > 0 && currentTime - MesoUP_T > 0)
            {
                MesoUP_N = 0;
                MesoUP_R = 0;
                MesoUP_T = 0;
                endFlag |= (uint)BuffValueTypes.MesoUP;
            }
            if (ShadowPartner_N > 0 && currentTime - ShadowPartner_T > 0)
            {
                ShadowPartner_N = 0;
                ShadowPartner_R = 0;
                ShadowPartner_T = 0;
                endFlag |= (uint)BuffValueTypes.ShadowPartner;
            }
            if (PickPocketMesoUP_N > 0 && currentTime - PickPocketMesoUP_T > 0)
            {
                PickPocketMesoUP_N = 0;
                PickPocketMesoUP_R = 0;
                PickPocketMesoUP_T = 0;
                endFlag |= (uint)BuffValueTypes.PickPocketMesoUP;
            }
            if (MesoGuard_N > 0 && currentTime - MesoGuard_T > 0)
            {
                MesoGuard_N = 0;
                MesoGuard_R = 0;
                MesoGuard_T = 0;
                endFlag |= (uint)BuffValueTypes.MesoGuard;
            }
            if (Thaw_N > 0 && currentTime - Thaw_T > 0)
            {
                Thaw_N = 0;
                Thaw_R = 0;
                Thaw_T = 0;
                endFlag |= (uint)BuffValueTypes.Thaw;
            }
            if (Weakness_N > 0 && currentTime - Weakness_T > 0)
            {
                Weakness_N = 0;
                Weakness_R = 0;
                Weakness_T = 0;
                endFlag |= (uint)BuffValueTypes.Weakness;
            }
            if (Curse_N > 0 && currentTime - Curse_T > 0)
            {
                Curse_N = 0;
                Curse_R = 0;
                Curse_T = 0;
                endFlag |= (uint)BuffValueTypes.Curse;
            }
            if (endFlag != 0)
            {
                BuffPacket.ResetTempStats(Char, endFlag);
                MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_buffs WHERE cid = " + Char.ID.ToString()) as MySqlDataReader;
                if (!data.HasRows)
                {
                    Console.WriteLine("Warning! Trying to remove player buff without flag(s) being in database! " + endFlag.ToString());
                }
                else
                {
                    List<long> RemoveBuffs = new List<long>();
                    long ticks = 0;
                    while (data.Read())
                    {
                        ticks = data.GetInt64("time");
                        if (ticks <= Tools.GetTimeAsMilliseconds(pNow))
                        {
                            int bid = data.GetInt32("bid");
                            RemoveBuffs.Add(bid);

                            if (bid == (int)Constants.Spearman.Skills.HyperBody)
                            {
                                int inc1 = data.GetInt32("sinc");
                                int inc2 = data.GetInt32("sinc2");
                                Console.WriteLine("current max HP : " + Char.PrimaryStats.MaxHP);
                                Char.ModifyMaxHP((short)-inc1, true, false);
                                Char.ModifyMaxMP((short)-inc2, true);
                                if (Char.PrimaryStats.HP > Char.PrimaryStats.MaxHP)
                                {
                                    Char.ModifyHP((short)(Char.PrimaryStats.HP - Char.PrimaryStats.MaxHP)); 
                                }
                                if (Char.PrimaryStats.MP > Char.PrimaryStats.MaxMP)
                                {
                                    Char.ModifyMP((short)(Char.PrimaryStats.MP - Char.PrimaryStats.MaxMP));
                                }
                            }
                        }
                    }
                    foreach (long l in RemoveBuffs)
                    {
                        Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_buffs WHERE cid = " + Char.ID.ToString() + " AND bid = " + l.ToString());
                    }
                }
            }
        }

        public void RemoveHyperBody()
        {
            MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_buffs WHERE cid = " + Char.ID.ToString()) as MySqlDataReader;
            if (!data.HasRows)
            {
                Console.WriteLine("Warning! Trying to remove player buff without flag(s) being in database!");
            }
            else
            {
                List<long> RemoveBuffs = new List<long>();
                while (data.Read())
                {

                    int bid = data.GetInt32("bid");
                    RemoveBuffs.Add(bid);

                    if (bid == (int)Constants.Spearman.Skills.HyperBody)
                    {
                        int inc1 = data.GetInt32("sinc");
                        int inc2 = data.GetInt32("sinc2");

                        Char.ModifyMaxHP((short)-inc1, false);
                        Char.ModifyMaxMP((short)-inc2, true);
                        if (Char.PrimaryStats.HP > Char.PrimaryStats.MaxHP)
                        {
                            Char.ModifyHP((short)(Char.PrimaryStats.HP - Char.PrimaryStats.MaxHP));
                        }
                        if (Char.PrimaryStats.MP > Char.PrimaryStats.MaxMP)
                        {
                            Char.ModifyMP((short)(Char.PrimaryStats.MP - Char.PrimaryStats.MaxMP));
                        }
                    }
                }
                foreach (long l in RemoveBuffs)
                {
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_buffs WHERE cid = " + Char.ID.ToString() + " AND bid = " + l.ToString());
                }
            }
        }

        public void RemoveByValue(int pBuffValue)
        {
            Console.WriteLine("remove by value..");
            uint endFlag = 0;
            if (WeaponAttack_R == pBuffValue)
            {
                WeaponAttack_N = 0;
                WeaponAttack_R = 0;
                WeaponAttack_T = 0;
                endFlag |= (uint)BuffValueTypes.WeaponAttack;
            }
            if (WeaponDefense_R == pBuffValue)
            {
                WeaponDefense_N = 0;
                WeaponDefense_R = 0;
                WeaponDefense_T = 0;
                endFlag |= (uint)BuffValueTypes.WeaponDefense;
            }
            if (MagicAttack_R == pBuffValue)
            {
                MagicAttack_N = 0;
                MagicAttack_R = 0;
                MagicAttack_T = 0;
                endFlag |= (uint)BuffValueTypes.MagicAttack;
            }
            if (MagicDefense_R == pBuffValue)
            {
                MagicDefense_N = 0;
                MagicDefense_R = 0;
                MagicDefense_T = 0;
                endFlag |= (uint)BuffValueTypes.MagicDefense;
            }
            if (Accurancy_R == pBuffValue)
            {
                Accurancy_N = 0;
                Accurancy_R = 0;
                Accurancy_T = 0;
                endFlag |= (uint)BuffValueTypes.Accurancy;
            }
            if (Avoidability_R == pBuffValue)
            {
                Avoidability_N = 0;
                Avoidability_R = 0;
                Avoidability_T = 0;
                endFlag |= (uint)BuffValueTypes.Avoidability;
            }
            if (Hands_R == pBuffValue)
            {
                Hands_N = 0;
                Hands_R = 0;
                Hands_T = 0;
                endFlag |= (uint)BuffValueTypes.Hands;
            }
            if (Speed_R == pBuffValue)
            {
                Speed_N = 0;
                Speed_R = 0;
                Speed_T = 0;
                endFlag |= (uint)BuffValueTypes.Speed;
            }
            if (Jump_R == pBuffValue)
            {
                Jump_N = 0;
                Jump_R = 0;
                Jump_T = 0;
                endFlag |= (uint)BuffValueTypes.Jump;
            }
            if (MagicGuard_R == pBuffValue)
            {
                MagicGuard_N = 0;
                MagicGuard_R = 0;
                MagicGuard_T = 0;
                endFlag |= (uint)BuffValueTypes.MagicGuard;
            }
            if (DarkSight_R == pBuffValue)
            {
                DarkSight_N = 0;
                DarkSight_R = 0;
                DarkSight_T = 0;
                endFlag |= (uint)BuffValueTypes.DarkSight;
            }
            if (Booster_R == pBuffValue)
            {
                Booster_N = 0;
                Booster_R = 0;
                Booster_T = 0;
                endFlag |= (uint)BuffValueTypes.Booster;
            }
            if (PowerGuard_R == pBuffValue)
            {
                PowerGuard_N = 0;
                PowerGuard_R = 0;
                PowerGuard_T = 0;
                endFlag |= (uint)BuffValueTypes.PowerGuard;
            }
            if (MaxHP_R == pBuffValue)
            {
                MaxHP_N = 0;
                MaxHP_R = 0;
                MaxHP_T = 0;
                endFlag |= (uint)BuffValueTypes.MaxHP;
            }
            if (MaxMP_R == pBuffValue)
            {
                MaxMP_N = 0;
                MaxMP_R = 0;
                MaxMP_T = 0;
                endFlag |= (uint)BuffValueTypes.MaxMP;
            }
            if (Invincible_R == pBuffValue)
            {
                Invincible_N = 0;
                Invincible_R = 0;
                Invincible_T = 0;
                endFlag |= (uint)BuffValueTypes.Invincible;
            }
            if (SoulArrow_R == pBuffValue)
            {
                SoulArrow_N = 0;
                SoulArrow_R = 0;
                SoulArrow_T = 0;
                endFlag |= (uint)BuffValueTypes.SoulArrow;
            }
            if (Stun_R == pBuffValue)
            {
                Stun_N = 0;
                Stun_R = 0;
                Stun_T = 0;
                endFlag |= (uint)BuffValueTypes.Stun;
            }
            if (Poison_R == pBuffValue)
            {
                Poison_N = 0;
                Poison_R = 0;
                Poison_T = 0;
                endFlag |= (uint)BuffValueTypes.Poison;
            }
            if (Seal_R == pBuffValue)
            {
                Seal_N = 0;
                Seal_R = 0;
                Seal_T = 0;
                endFlag |= (uint)BuffValueTypes.Seal;
            }
            if (Darkness_R == pBuffValue)
            {
                Darkness_N = 0;
                Darkness_R = 0;
                Darkness_T = 0;
                endFlag |= (uint)BuffValueTypes.Darkness;
            }
            if (ComboAttack_R == pBuffValue)
            {
                ComboAttack_N = 0;
                ComboAttack_R = 0;
                ComboAttack_T = 0;
                endFlag |= (uint)BuffValueTypes.ComboAttack;
            }
            if (Charges_R == pBuffValue)
            {
                Charges_N = 0;
                Charges_R = 0;
                Charges_T = 0;
                endFlag |= (uint)BuffValueTypes.Charges;
            }
            if (DragonBlood_R == pBuffValue)
            {
                DragonBlood_N = 0;
                DragonBlood_R = 0;
                DragonBlood_T = 0;
                endFlag |= (uint)BuffValueTypes.DragonBlood;
            }
            if (HolySymbol_R == pBuffValue)
            {
                HolySymbol_N = 0;
                HolySymbol_R = 0;
                HolySymbol_T = 0;
                endFlag |= (uint)BuffValueTypes.HolySymbol;
            }
            if (MesoUP_R == pBuffValue)
            {
                MesoUP_N = 0;
                MesoUP_R = 0;
                MesoUP_T = 0;
                endFlag |= (uint)BuffValueTypes.MesoUP;
            }
            if (ShadowPartner_R == pBuffValue)
            {
                ShadowPartner_N = 0;
                ShadowPartner_R = 0;
                ShadowPartner_T = 0;
                endFlag |= (uint)BuffValueTypes.ShadowPartner;
            }
            if (PickPocketMesoUP_R == pBuffValue)
            {
                PickPocketMesoUP_N = 0;
                PickPocketMesoUP_R = 0;
                PickPocketMesoUP_T = 0;
                endFlag |= (uint)BuffValueTypes.PickPocketMesoUP;
            }
            if (MesoGuard_R == pBuffValue)
            {
                MesoGuard_N = 0;
                MesoGuard_R = 0;
                MesoGuard_T = 0;
                endFlag |= (uint)BuffValueTypes.MesoGuard;
            }
            if (Thaw_R == pBuffValue)
            {
                Thaw_N = 0;
                Thaw_R = 0;
                Thaw_T = 0;
                endFlag |= (uint)BuffValueTypes.Thaw;
            }
            if (Weakness_R == pBuffValue)
            {
                Weakness_N = 0;
                Weakness_R = 0;
                Weakness_T = 0;
                endFlag |= (uint)BuffValueTypes.Weakness;
            }
            if (Curse_R == pBuffValue)
            {
                Curse_N = 0;
                Curse_R = 0;
                Curse_T = 0;
                endFlag |= (uint)BuffValueTypes.Curse;
            }

            if (endFlag != 0)
            {
                BuffPacket.ResetTempStats(Char, endFlag);
            }
        }


        public void EncodeForLocal(Packet pPacket, uint pSpecificFlag = 0xFFFFFFFF)
        {
            long currentTime = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate);
            int tmpBuffPos = pPacket.Position;
            uint endFlag = 0;
            pPacket.WriteUInt(endFlag);

            if (WeaponAttack_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.WeaponAttack) == (uint)BuffValueTypes.WeaponAttack)
            {
                pPacket.WriteShort((short)WeaponAttack_N);
                pPacket.WriteInt(WeaponAttack_R);
                pPacket.WriteShort((short)(WeaponAttack_T - currentTime));
                endFlag |= (uint)BuffValueTypes.WeaponAttack;
            }
            if (WeaponDefense_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.WeaponDefense) == (uint)BuffValueTypes.WeaponDefense)
            {
                pPacket.WriteShort((short)WeaponDefense_N);
                pPacket.WriteInt(WeaponDefense_R);
                pPacket.WriteShort((short)(WeaponDefense_T - currentTime));
                endFlag |= (uint)BuffValueTypes.WeaponDefense;
            }
            if (MagicAttack_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.MagicAttack) == (uint)BuffValueTypes.MagicAttack)
            {
                pPacket.WriteShort((short)MagicAttack_N);
                pPacket.WriteInt(MagicAttack_R);
                pPacket.WriteShort((short)(MagicAttack_T - currentTime));
                endFlag |= (uint)BuffValueTypes.MagicAttack;
            }
            if (MagicDefense_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.MagicDefense) == (uint)BuffValueTypes.MagicDefense)
            {
                pPacket.WriteShort((short)MagicDefense_N);
                pPacket.WriteInt(MagicDefense_R);
                pPacket.WriteShort((short)(MagicDefense_T - currentTime));
                endFlag |= (uint)BuffValueTypes.MagicDefense;
            }
            if (Accurancy_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Accurancy) == (uint)BuffValueTypes.Accurancy)
            {
                pPacket.WriteShort((short)Accurancy_N);
                pPacket.WriteInt(Accurancy_R);
                pPacket.WriteShort((short)(Accurancy_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Accurancy;
            }
            if (Avoidability_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Avoidability) == (uint)BuffValueTypes.Avoidability)
            {
                pPacket.WriteShort((short)Avoidability_N);
                pPacket.WriteInt(Avoidability_R);
                pPacket.WriteShort((short)(Avoidability_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Avoidability;
            }
            if (Hands_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Hands) == (uint)BuffValueTypes.Hands)
            {
                pPacket.WriteShort((short)Hands_N);
                pPacket.WriteInt(Hands_R);
                pPacket.WriteShort((short)(Hands_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Hands;
            }
            if (Speed_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Speed) == (uint)BuffValueTypes.Speed)
            {
                pPacket.WriteShort((short)Speed_N);
                pPacket.WriteInt(Speed_R);
                pPacket.WriteShort((short)(Speed_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Speed;
            }
            if (Jump_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Jump) == (uint)BuffValueTypes.Jump)
            {
                pPacket.WriteShort((short)Jump_N);
                pPacket.WriteInt(Jump_R);
                pPacket.WriteShort((short)(Jump_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Jump;
            }
            if (MagicGuard_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.MagicGuard) == (uint)BuffValueTypes.MagicGuard)
            {
                pPacket.WriteShort((short)MagicGuard_N);
                pPacket.WriteInt(MagicGuard_R);
                pPacket.WriteShort((short)(MagicGuard_T - currentTime));
                endFlag |= (uint)BuffValueTypes.MagicGuard;
            }
            if (DarkSight_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.DarkSight) == (uint)BuffValueTypes.DarkSight)
            {
                pPacket.WriteShort((short)DarkSight_N);
                pPacket.WriteInt(DarkSight_R);
                pPacket.WriteShort((short)(DarkSight_T - currentTime));
                endFlag |= (uint)BuffValueTypes.DarkSight;
            }
            if (Booster_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Booster) == (uint)BuffValueTypes.Booster)
            {
                pPacket.WriteShort((short)Booster_N);
                pPacket.WriteInt(Booster_R);
                pPacket.WriteShort((short)(Booster_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Booster;
            }
            if (PowerGuard_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.PowerGuard) == (uint)BuffValueTypes.PowerGuard)
            {
                pPacket.WriteShort((short)PowerGuard_N);
                pPacket.WriteInt(PowerGuard_R);
                pPacket.WriteShort((short)(PowerGuard_T - currentTime));
                endFlag |= (uint)BuffValueTypes.PowerGuard;
            }
            if (MaxHP_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.MaxHP) == (uint)BuffValueTypes.MaxHP)
            {
                pPacket.WriteShort((short)MaxHP_N);
                pPacket.WriteInt(MaxHP_R);
                pPacket.WriteShort((short)(MaxHP_T - currentTime));
                endFlag |= (uint)BuffValueTypes.MaxHP;
            }
            if (MaxMP_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.MaxMP) == (uint)BuffValueTypes.MaxMP)
            {
                pPacket.WriteShort((short)MaxMP_N);
                pPacket.WriteInt(MaxMP_R);
                pPacket.WriteShort((short)(MaxMP_T - currentTime));
                endFlag |= (uint)BuffValueTypes.MaxMP;
            }
            if (Invincible_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Invincible) == (uint)BuffValueTypes.Invincible)
            {
                pPacket.WriteShort((short)Invincible_N);
                pPacket.WriteInt(Invincible_R);
                pPacket.WriteShort((short)(Invincible_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Invincible;
            }
            if (SoulArrow_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.SoulArrow) == (uint)BuffValueTypes.SoulArrow)
            {
                pPacket.WriteShort((short)SoulArrow_N);
                pPacket.WriteInt(SoulArrow_R);
                pPacket.WriteShort((short)(SoulArrow_T - currentTime));
                endFlag |= (uint)BuffValueTypes.SoulArrow;
            }
            if (Stun_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Stun) == (uint)BuffValueTypes.Stun)
            {
                pPacket.WriteShort((short)Stun_N);
                pPacket.WriteInt(Stun_R);
                pPacket.WriteShort((short)(Stun_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Stun;
            }
            if (Poison_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Poison) == (uint)BuffValueTypes.Poison)
            {
                pPacket.WriteShort((short)Poison_N);
                pPacket.WriteInt(Poison_R);
                pPacket.WriteShort((short)(Poison_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Poison;
            }
            if (Seal_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Seal) == (uint)BuffValueTypes.Seal)
            {
                pPacket.WriteShort((short)Seal_N);
                pPacket.WriteInt(Seal_R);
                pPacket.WriteShort((short)(Seal_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Seal;
            }
            if (Darkness_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Darkness) == (uint)BuffValueTypes.Darkness)
            {
                pPacket.WriteShort((short)Darkness_N);
                pPacket.WriteInt(Darkness_R);
                pPacket.WriteShort((short)(Darkness_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Darkness;
            }
            if (ComboAttack_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.ComboAttack) == (uint)BuffValueTypes.ComboAttack)
            {
                pPacket.WriteShort((short)ComboAttack_N);
                pPacket.WriteInt(ComboAttack_R);
                pPacket.WriteShort((short)(ComboAttack_T - currentTime));
                endFlag |= (uint)BuffValueTypes.ComboAttack;
            }
            if (Charges_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Charges) == (uint)BuffValueTypes.Charges)
            {
                pPacket.WriteShort((short)Charges_N);
                pPacket.WriteInt(Charges_R);
                pPacket.WriteShort((short)(Charges_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Charges;
            }
            if (DragonBlood_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.DragonBlood) == (uint)BuffValueTypes.DragonBlood)
            {
                pPacket.WriteShort((short)DragonBlood_N);
                pPacket.WriteInt(DragonBlood_R);
                pPacket.WriteShort((short)(DragonBlood_T - currentTime));
                endFlag |= (uint)BuffValueTypes.DragonBlood;
            }
            if (HolySymbol_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.HolySymbol) == (uint)BuffValueTypes.HolySymbol)
            {
                pPacket.WriteShort((short)HolySymbol_N);
                pPacket.WriteInt(HolySymbol_R);
                pPacket.WriteShort((short)(HolySymbol_T - currentTime));
                endFlag |= (uint)BuffValueTypes.HolySymbol;
            }
            if (MesoUP_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.MesoUP) == (uint)BuffValueTypes.MesoUP)
            {
                pPacket.WriteShort((short)MesoUP_N);
                pPacket.WriteInt(MesoUP_R);
                pPacket.WriteShort((short)(MesoUP_T - currentTime));
                endFlag |= (uint)BuffValueTypes.MesoUP;
            }
            if (ShadowPartner_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.ShadowPartner) == (uint)BuffValueTypes.ShadowPartner)
            {
                pPacket.WriteShort((short)ShadowPartner_N);
                pPacket.WriteInt(ShadowPartner_R);
                pPacket.WriteShort((short)(ShadowPartner_T - currentTime));
                endFlag |= (uint)BuffValueTypes.ShadowPartner;
            }
            if (PickPocketMesoUP_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.PickPocketMesoUP) == (uint)BuffValueTypes.PickPocketMesoUP)
            {
                pPacket.WriteShort((short)PickPocketMesoUP_N);
                pPacket.WriteInt(PickPocketMesoUP_R);
                pPacket.WriteShort((short)(PickPocketMesoUP_T - currentTime));
                endFlag |= (uint)BuffValueTypes.PickPocketMesoUP;
            }
            if (MesoGuard_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.MesoGuard) == (uint)BuffValueTypes.MesoGuard)
            {
                pPacket.WriteShort((short)MesoGuard_N);
                pPacket.WriteInt(MesoGuard_R);
                pPacket.WriteShort((short)(MesoGuard_T - currentTime));
                endFlag |= (uint)BuffValueTypes.MesoGuard;
            }
            if (Thaw_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Thaw) == (uint)BuffValueTypes.Thaw)
            {
                pPacket.WriteShort((short)Thaw_N);
                pPacket.WriteInt(Thaw_R);
                pPacket.WriteShort((short)(Thaw_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Thaw;
            }
            if (Weakness_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Weakness) == (uint)BuffValueTypes.Weakness)
            {
                pPacket.WriteShort((short)Weakness_N);
                pPacket.WriteInt(Weakness_R);
                pPacket.WriteShort((short)(Weakness_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Weakness;
            }
            if (Curse_T > 0 && (pSpecificFlag & (uint)BuffValueTypes.Curse) == (uint)BuffValueTypes.Curse)
            {
                pPacket.WriteShort((short)Curse_N);
                pPacket.WriteInt(Curse_R);
                pPacket.WriteShort((short)(Curse_T - currentTime));
                endFlag |= (uint)BuffValueTypes.Curse;
            }

            pPacket.SetUInt(tmpBuffPos, endFlag);
        }

        public bool HasBuff(int pBuffValue)
        {
            if (WeaponAttack_R == pBuffValue)
				return true;
            else if (WeaponDefense_R == pBuffValue)
				return true;
            else if (MagicAttack_R == pBuffValue)
				return true;
            else if (MagicDefense_R == pBuffValue)
				return true;
            else if (Accurancy_R == pBuffValue)
				return true;
            else if (Avoidability_R == pBuffValue)
				return true;
            else if (Hands_R == pBuffValue)
				return true;
            else if (Speed_R == pBuffValue)
				return true;
            else if (Jump_R == pBuffValue)
				return true;
            else if (MagicGuard_R == pBuffValue)
				return true;
            else if (DarkSight_R == pBuffValue)
				return true;
            else if (Booster_R == pBuffValue)
				return true;
            else if (PowerGuard_R == pBuffValue)
				return true;
            else if (MaxHP_R == pBuffValue)
				return true;
            else if (MaxMP_R == pBuffValue)
				return true;
            else if (Invincible_R == pBuffValue)
				return true;
            else if (SoulArrow_R == pBuffValue)
				return true;
            else if (Stun_R == pBuffValue)
				return true;
            else if (Poison_R == pBuffValue)
				return true;
            else if (Seal_R == pBuffValue)
				return true;
            else if (Darkness_R == pBuffValue)
				return true;
            else if (ComboAttack_R == pBuffValue)
				return true;
            else if (Charges_R == pBuffValue)
				return true;
            else if (DragonBlood_R == pBuffValue)
				return true;
            else if (HolySymbol_R == pBuffValue)
				return true;
            else if (MesoUP_R == pBuffValue)
				return true;
            else if (ShadowPartner_R == pBuffValue)
				return true;
            else if (PickPocketMesoUP_R == pBuffValue)
				return true;
            else if (MesoGuard_R == pBuffValue)
				return true;
            else if (Thaw_R == pBuffValue)
				return true;
            else if (Weakness_R == pBuffValue)
				return true;
            else if (Curse_R == pBuffValue)
				return true;
            return false;
        }
    }
}