using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;
using WvsBeta.Game.Events;

namespace WvsBeta.Game
{
    public class AttackPacket
    {

        public static byte IncrementFromStage = 0;
        
        public enum AttackTypes
        {
            Melee,
            Ranged,
            Magic,
            Summon
        }

        public class AttackData
        {
            public int SkillID { get; set; }
            public byte SkillLevel { get; set; }
            public byte PortalsEntered { get; set; }
            public byte Targets { get; set; }
            public byte Hits { get; set; }

            public byte Display { get; set; }
            public byte WeaponSpeed { get; set; }
            public byte Animation { get; set; }
            public byte WeaponClass { get; set; }
            public short StarPosition { get; set; }
            public int Charge { get; set; }
            public int StarID { get; set; }
            public int SummonID { get; set; }
            public long TotalDamage { get; set; }

            public Pos ProjectilePosition { get; set; }
            public Pos PlayerPosition { get; set; }

            public Dictionary<int, List<int>> Damages { get; set; }
            public Dictionary<int, Pos> Positions { get; set; }

            public bool IsMesoExplosion { get; set; }
            
            public AttackData()
            {
                Damages = new Dictionary<int, List<int>>();
                Positions = new Dictionary<int, Pos>();
                IsMesoExplosion = false;
            }

        }

        public static void ParseAttackData(Character chr, Packet packet, out AttackData data, AttackTypes type)
        {

            AttackData ad = new AttackData();
            byte hits;
            byte targets;
            int skillid = 0;

            if (type != AttackTypes.Summon)
            {
                byte TByte = packet.ReadByte();
                skillid = packet.ReadInt();
                if (skillid != 0 && chr.Skills.mSkills.ContainsKey(skillid))
                {
                    ad.SkillLevel = (byte)chr.Skills.mSkills[skillid];
                }
                else
                {
                    ad.SkillLevel = 0;
                }
                if (skillid == (int)Constants.ChiefBandit.Skills.MesoExplosion)
                {
                    ad.IsMesoExplosion = true;
                }
                targets = (byte)(TByte / 0x10);
                hits = (byte)(TByte % 0x10);

                packet.Skip(1);
                ad.Display = packet.ReadByte();
                ad.Animation = packet.ReadByte();
            }
            else
            {
                ad.SummonID = packet.ReadInt();
                ad.Animation = packet.ReadByte();
                targets = 1;
                hits = 1;
            }


            if (type == AttackTypes.Ranged)
            {
                ad.StarPosition = packet.ReadShort();
                Item item = chr.Inventory.GetItem(2, ad.StarPosition);
                if (item != null)
                {
                    ad.StarID = item.ItemID;
                }
                packet.Skip(1);
            }

            ad.Targets = targets;
            ad.Hits = hits;
            ad.SkillID = skillid;

            for (byte i = 0; i < targets; i++)
            {
                int objectid = packet.ReadInt();
                packet.Skip(4); // 06 ?? ?? 01
                ad.Positions.Add(objectid, new Pos(packet));
                packet.Skip(4); // damage pos
                if (type == AttackTypes.Summon)
                {
                    packet.Skip(1);
                }
                else if (!ad.IsMesoExplosion)
                {
                    packet.Skip(2);
                }

                for (byte j = 0; j < hits; j++)
                {
                    if (!ad.Damages.ContainsKey(objectid))
                    {
                        ad.Damages.Add(objectid, new List<int>());
                    }
                    int dmg = packet.ReadInt();
                    ad.Damages[objectid].Add(dmg);
                    ad.TotalDamage += dmg;
                }
            }
            ad.PlayerPosition = new Pos(packet.ReadShort(), packet.ReadShort());
            data = ad;
        }



        public static void HandleMeleeAttack(Character chr, Packet packet)
        {
            AttackData ad;
            ParseAttackData(chr, packet, out ad, AttackTypes.Melee);
            SendMeleeAttack(chr, ad);


            Mob mob;

            if (ad.SkillID != 0)
            {
                chr.Skills.UseMeleeAttack(ad.SkillID);
            }



            int map = chr.Map;
            byte pickLevel = chr.Buffs.GetActiveSkillLevel((int)Constants.ChiefBandit.Skills.Pickpocket);
            bool pickOk = !ad.IsMesoExplosion && pickLevel > 0;

            SkillLevelData sld = chr.Skills.GetSkillLevelData((int)Constants.ChiefBandit.Skills.Pickpocket, pickLevel);
            Pos origin;
            List<int> pickDamages = new List<int>();

            foreach (KeyValuePair<int, List<int>> dmgs in ad.Damages)
            {
                try
                {
                    int targetTotal = 0;
                    byte connectedHits = 0;

                    mob = DataProvider.Maps[chr.Map].GetMob(dmgs.Key);
                    if (mob == null) continue;

                    origin = mob.Position;
                    foreach (int amount in dmgs.Value)
                    {
                        //mob.giveDamage(chr, amount);
                        if (amount != 0)
                        {
                            connectedHits++;
                            targetTotal += amount;
                        }
                        if (pickOk && ((chr.Randomizer.NextSeedINT() % 100) < sld.Property))
                        {
                            pickDamages.Add(amount);
                        }
                        if (mob == null)
                        {
                            if (pickOk) continue;
                            break;
                        }
                        if (ad.SkillID == (int)Constants.Bandit.Skills.Steal && !DataProvider.Mobs[mob.MobID].Boss)
                        {
                            DropPacket.HandleDrops(chr, map, Constants.getDropName(mob.MobID, true), mob.SpawnID, origin, false, false, true);
                        }

                        mob.GiveDamage(chr, amount);

                    }
                    if (targetTotal > 0)
                    {
                        if (mob != null && mob.HP > 0)
                        {

                        }
                    }

                    byte ppsize = (byte)pickDamages.Count;
                    for (byte pick = 0; pick < ppsize; pick++)
                    {
                        Console.WriteLine("dodrop attackpacket");
                        Pos pppos = origin;
                        pppos.X += (short)((ppsize % 2 == 0 ? 5 : 0) + (ppsize / 2) - 20 * ((ppsize / 2) - pick));

                        int ppmesos = ((pickDamages[pick] * sld.XValue) / 10000);
                        if (ppmesos == 0) continue;
                        Drop drop = new Drop(chr.Map, ppmesos, pppos, chr.ID, true);
                        drop.Time = 100;
                        drop.DoDrop(origin, true);
                    }



                    mob.CheckDead(ad.Positions[dmgs.Key]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            switch (ad.SkillID)
            {
                case 0: // Normal wep
                    {
                        if (chr.Inventory.GetEquippedItemID((short)Constants.EquipSlots.Slots.Helm, true) == 1002258 && chr.Admin) // Blue Diamondy Bandana
                        {
                            List<Mob> mobs = DataProvider.Maps[chr.Map].GetMobsInRange(chr.Position, new Pos(-10000, -10000), new Pos(10000, 10000));
                            foreach (Mob m in mobs)
                            {
                                MobPacket.SendMobDamageOrHeal(chr.Map, m.SpawnID, 1337, false);
                                if (m.GiveDamage(chr, 1337))
                                {
                                    m.CheckDead();
                                }
                            }
                        }
                        break;
                    }
                case (int)Constants.ChiefBandit.Skills.MesoExplosion:
                    {
                        byte items = packet.ReadByte();
                        map = chr.Map;
                        for (byte i = 0; i < items; i++)
                        {
                            int objectID = packet.ReadInt();
                            packet.Skip(1);
                            if (!DataProvider.Maps[map].Drops.ContainsKey(objectID)) continue;
                            Drop drop = DataProvider.Maps[map].Drops[objectID];
                            if (drop.IsMesos())
                            {
                                DropPacket.ExplodeDrop(drop);
                                drop.RemoveDrop(false);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        break;
                    }


            }
            if (chr.Map == 109060000)
            {
                if (EventManager.Instance.RegisteredEvents.ContainsKey(Event.EventType.Snowball))
                {
                    int Playerpos = chr.Position.Y;
                    if (chr.Position.Y >= -146 && chr.Position.Y <= MapleSnowball.Top) //is on Top : snowball1
                    {
                        if (MapleSnowball.NearSnowman(chr) && !MapleSnowball.snowball0.HasThread)
                        {
                            MapleSnowball.snowball1.CalculateSnowmanDamage(chr, 3);
                        }
                    }
                    else if (chr.Position.Y >= 94 && chr.Position.Y <= MapleSnowball.Bottom)
                    {
                        if (MapleSnowball.NearSnowman(chr) && !MapleSnowball.snowball1.HasThread)
                        {
                            MapleSnowball.snowball0.CalculateSnowmanDamage(chr, 2); //is on Bottom : snowball0
                        }
                    }

                    if (chr.Position.X <= MapleSnowball.snowball0.pos.X + 30 && chr.Position.X >= MapleSnowball.snowball0.pos.X - 60)
                    {
                        if (Playerpos >= 94 && Playerpos <= MapleSnowball.Bottom)
                        {
                            
                            SnowballPackets.HitSnowball(chr, 0, 10, (short)0x5E);
                            if (MapleSnowball.snowball0.AllowableDamage != 0)
                            {
                                for (int i = 0; i < 3; i++) //the pos must increment by 3 to keep up with the character -.-
                                {
                                    MapleSnowball.snowball0.pos.X++;
                                }
                                MapleSnowball.snowball0.IncrementFromStage++;
                                SnowballPackets.RollSnowball(chr, 0, IncrementFromStage, 0, 0, 0);
                            }
                        }
                        else if (Playerpos >= -146 && Playerpos <= MapleSnowball.Top)
                        {

                            SnowballPackets.HitSnowball(chr, 1, 10, (short)0x5E);
                            if (MapleSnowball.snowball1.AllowableDamage != 0)
                            {
                                for (int i = 0; i < 3; i++) //the pos must increment by 3 to keep up with the character -.-
                                {
                                    MapleSnowball.snowball1.pos.X++;
                                }
                                MapleSnowball.snowball1.IncrementFromStage++;
                                SnowballPackets.RollSnowball(chr, 1, 0, IncrementFromStage, 0, 0);
                            }
                        }
                    }
                }
            }
        }
        

        public static void HandleRangedAttack(Character chr, Packet packet)
        {
            //MessagePacket.SendNotice(packet.ToString(), chr);
            AttackData ad;
            ParseAttackData(chr, packet, out ad, AttackTypes.Ranged);
            SendRangedAttack(chr, ad);
            Mob mob;

            if (ad.SkillID != 0)
            {
                chr.Skills.UseRangedAttack(ad.SkillID, ad.StarPosition);
            }

            if (ad.StarID != 0)
            {
                chr.Skills.UseRangedAttack(ad.SkillID, ad.StarPosition);
            }

            foreach (KeyValuePair<int, List<int>> dmgs in ad.Damages)
            {
                try
                {
                    mob = DataProvider.Maps[chr.Map].GetMob(dmgs.Key);
                    if (mob != null)
                    {
                        foreach (int amount in dmgs.Value)
                        {
                            mob.GiveDamage(chr, amount);
                        }
                        mob.CheckDead(ad.Positions[dmgs.Key]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void HandleMagicAttack(Character chr, Packet packet)
        {
            AttackData ad;
            ParseAttackData(chr, packet, out ad, AttackTypes.Magic);
            SendMagicAttack(chr, ad);
            Mob mob;

            if (ad.SkillID != 0)
            {
                chr.Skills.UseMeleeAttack(ad.SkillID);
            }


            foreach (KeyValuePair<int, List<int>> dmgs in ad.Damages)
            {
                try
                {
                    mob = DataProvider.Maps[chr.Map].GetMob(dmgs.Key);
                    if (mob != null)
                    {
                        foreach (int amount in dmgs.Value)
                        {
                            mob.GiveDamage(chr, amount);
                        }
                        bool died = mob.CheckDead(ad.Positions[dmgs.Key]);
                        if (!died)
                        {
                            SkillLevelData sld = DataProvider.Skills[ad.SkillID][ad.SkillLevel];
                            MobData md = DataProvider.Mobs[mob.MobID];

                            if (sld.ElementFlags == (byte)SkillElement.Ice)
                            {
                                mob.Status.Freeze_N = 1;
                                mob.Status.Freeze_R = ad.SkillID;
                                mob.Status.Freeze_T = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate) + (1000 * sld.BuffTime);
                                MobPacket.SendMobStatsTempSet(mob, 0, (uint)MobStatus.MobStatValue.Freeze);
                            }
                            

                            if (chr.Skills.GetSkillLevel((int)Constants.FPWizard.Skills.MpEater) > 0 && !mob.IsBoss)
                            {
                                if (mob.MpEats < 3 && mob.MP != 0 && chr.Randomizer.NextSeed() % 100 > sld.Property)
                                {
                                    int eaten = mob.MaxMP * sld.XValue / 100;
                                    eaten = Math.Min(eaten, mob.MP);
                                    mob.MP -= eaten;
                                    chr.ModifyMP((short)Math.Min(eaten, Constants.MaxMaxMp));
                                    MapPacket.SendPlayerSkillAnimThirdParty(chr, ad.SkillID, ad.SkillLevel, false, false);
                                    MapPacket.SendPlayerSkillAnimThirdParty(chr, ad.SkillID, ad.SkillLevel, false, true);
                                    mob.MpEats++;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void HandleSummonAttack(Character chr, Packet packet)
        {
            AttackData ad;
            ParseAttackData(chr, packet, out ad, AttackTypes.Summon);

            Summon summon = chr.Summons.GetSummon(ad.SummonID);
            if (summon == null)
            {
                return;
            }
            SendMagicAttack(chr, ad);
            Mob mob;

            foreach (KeyValuePair<int, List<int>> dmgs in ad.Damages)
            {
                try
                {
                    mob = DataProvider.Maps[chr.Map].GetMob(dmgs.Key);
                    if (mob != null)
                    {
                        foreach (int amount in dmgs.Value)
                        {
                            mob.GiveDamage(chr, amount);
                        }
                        mob.CheckDead(ad.Positions[dmgs.Key]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        public static void SendMeleeAttack(Character chr, AttackData data)
        {
            byte tbyte = (byte)((data.Targets * 0x10) + data.Hits);
            Packet pw = new Packet(0x5B);
            pw.WriteInt(chr.ID);

            pw.WriteByte(tbyte);
            pw.WriteByte(data.SkillLevel);
            if (data.SkillID != 0)
            {
                pw.WriteInt(data.SkillID);
            }

            pw.WriteByte(data.Display);
            pw.WriteByte(data.Animation);

            int mastery = chr.Skills.GetMastery();
            pw.WriteByte((byte)(mastery > 0 ? Constants.getMasteryDisplay(chr.Skills.GetSkillLevel(mastery)) : 0));

            pw.WriteInt(0); // starid

            foreach (KeyValuePair<int, List<int>> kvp in data.Damages)
            {
                pw.WriteInt(kvp.Key);
                pw.WriteByte(0x06);
                if (data.IsMesoExplosion)
                {
                    pw.WriteByte((byte)kvp.Value.Count);
                }
                foreach (int dmg in kvp.Value)
                {
                    pw.WriteInt(dmg);
                }
            }
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendRangedAttack(Character chr, AttackData data)
        {
            DamageFormula df = new DamageFormula(); 
            byte tbyte = (byte)((data.Targets * 0x10) + data.Hits);
            Packet pw = new Packet(0x5C);
            pw.WriteInt(chr.ID);
            pw.WriteByte(tbyte);
            pw.WriteByte(data.SkillLevel);
            if (data.SkillID != 0)
            {
                pw.WriteInt(data.SkillID);
            }

            pw.WriteByte(data.Display);
            pw.WriteByte(data.Animation);

            int mastery = chr.Skills.GetMastery();
            pw.WriteByte((byte)(mastery > 0 ? Constants.getMasteryDisplay(chr.Skills.GetSkillLevel(mastery)) : 0));
            pw.WriteInt(data.StarID); // starid

            foreach (KeyValuePair<int, List<int>> kvp in data.Damages)
            {
                pw.WriteInt(kvp.Key);
                pw.WriteByte(0x06);
                foreach (int dmg in kvp.Value)
                {
                    //sending a negative number will show a critical hit
                    //-2147483647 = 1
                    //-2147483646 = 2 , etc
                    //todo : critical formula 

                    if (data.SkillID == 0)
                    {
                        if (Constants.getJobTrack(chr.PrimaryStats.Job, true) == 3)
                        {
                            Mob mob = DataProvider.Maps[chr.Map].GetMob(kvp.Key);
                            if (mob != null)
                            {
                                if (dmg >= (df.CritDamage(chr.PrimaryStats.Dex, chr.PrimaryStats.Str, chr.Inventory.GetTotalWAttackInEquips(false), 1, chr.Skills.GetSkillLevel(chr.Skills.GetMastery()), 1, chr.Inventory.GetTotalWAttackInEquips(false), (int)chr.PrimaryStats.Level, mob.Data.PDD, mob.Data.Level)))
                                {
                                    Console.WriteLine("crit hit");
                                    //No idea what this formula is lol, for single attacks only :S
                                    pw.WriteInt(-2147382647 - 101001 + dmg);
                                }
                                else
                                {
                                    pw.WriteInt(dmg);
                                }
                            }
                            else
                            {
                                pw.WriteInt(dmg);
                            }
                        }
                        else if (Constants.getJobTrack(chr.PrimaryStats.Job, true) == 4)
                        {
                            Mob mob = DataProvider.Maps[chr.Map].GetMob(kvp.Key);
                            if (mob != null)
                            {
                                if (dmg >= (df.CritDamage(chr.PrimaryStats.Luk, chr.PrimaryStats.Dex, chr.Inventory.GetTotalWAttackInEquips(true), 0, chr.Skills.GetSkillLevel(chr.Skills.GetMastery()), chr.PrimaryStats.Str, chr.Inventory.GetTotalWAttackInEquips(true), (int)chr.PrimaryStats.Level, mob.Data.PDD, mob.Data.Level)))
                                    pw.WriteInt(-2147382647 -101001 + dmg);
                                else
                                    pw.WriteInt(dmg);
                            }
                            else
                            {
                                pw.WriteInt(dmg);
                            }
                        }
                        else
                        {
                            pw.WriteInt(dmg);
                        }

                    }
                    if (data.SkillID != 0)
                    {
                        Mob mob = DataProvider.Maps[chr.Map].GetMob(kvp.Key);
                        if (mob != null)
                        {
                            if (Constants.getJobTrack(chr.PrimaryStats.Job, true) == 3)
                            {
                                //MessagePacket.SendNotice(df.GeneralDamageWithSkillWithCrit(chr.PrimaryStats.Dex, chr.Inventory.GetTotalWAttackInEquips(false), chr.PrimaryStats.Dex, chr.PrimaryStats.Str, chr.Inventory.GetTotalWAttackInEquips(false), 1, chr.Skills.GetSkillLevel(chr.Skills.GetMastery()), chr.PrimaryStats.Str, chr.Inventory.GetTotalWAttackInEquips(false), data.SkillID, chr.PrimaryStats.Level, mob.Data.PDD, mob.Data.Level, Constants.getJobTrack(chr.PrimaryStats.Job, true)).ToString(), chr);
                                if (dmg >= (df.GeneralDamageWithSkillWithCrit(chr.PrimaryStats.Dex, chr.Inventory.GetTotalWAttackInEquips(false), chr.PrimaryStats.Dex, chr.PrimaryStats.Str, chr.Inventory.GetTotalWAttackInEquips(false), 1, chr.Skills.GetSkillLevel(chr.Skills.GetMastery()), chr.PrimaryStats.Str, chr.Inventory.GetTotalWAttackInEquips(false), data.SkillID, chr.PrimaryStats.Level, mob.Data.PDD, mob.Data.Level, Constants.getJobTrack(chr.PrimaryStats.Job, true))))
                                {
                                    //MessagePacket.SendNotice(dmg.ToString(), chr);
                                    pw.WriteInt(-2147483647 + (dmg - 1));
                                }
                                else
                                {
                                    pw.WriteInt(dmg);
                                }
                            }
                            else if (Constants.getJobTrack(chr.PrimaryStats.Job, true) == 4 && (chr.PrimaryStats.Job != 420 || chr.PrimaryStats.Job != 421 || chr.PrimaryStats.Job != 400))
                            {
                                if (dmg >= (df.GeneralDamageWithSkillWithCrit(chr.PrimaryStats.Luk, chr.Inventory.GetTotalWAttackInEquips(true), chr.PrimaryStats.Luk, chr.PrimaryStats.Dex, chr.Inventory.GetTotalWAttackInEquips(true), 0, chr.Skills.GetSkillLevel(chr.Skills.GetMastery()), chr.PrimaryStats.Str, chr.Inventory.GetTotalWAttackInEquips(true), data.SkillID, chr.PrimaryStats.Level, mob.Data.PDD, mob.Data.Level, Constants.getJobTrack(chr.PrimaryStats.Job, true))))
                                {
                                    pw.WriteInt(-2147483647 + (dmg - 1));
                                }
                                else
                                {
                                    pw.WriteInt(dmg);
                                }
                            }
                            else
                            {
                                pw.WriteInt(dmg);
                            }
                        }
                        else
                        {
                            pw.WriteInt(dmg);
                        }
                    }
                    else
                    {
                        pw.WriteInt(dmg);
                    }
                    
                }
            }
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendMagicAttack(Character chr, AttackData data)
        {
            byte tbyte = (byte)((data.Targets * 0x10) + data.Hits);
            Packet pw = new Packet(0x5D);
            pw.WriteInt(chr.ID);

            pw.WriteByte(tbyte);
            pw.WriteByte(data.SkillLevel);
            if (data.SkillID != 0)
            {
                pw.WriteInt(data.SkillID);
            }

            pw.WriteByte(data.Display);
            pw.WriteByte(data.Animation);

            int mastery = chr.Skills.GetMastery();
            pw.WriteByte((byte)(mastery > 0 ? Constants.getMasteryDisplay(chr.Skills.GetSkillLevel(mastery)) : 0));

            pw.WriteInt(0); // starid

            foreach (KeyValuePair<int, List<int>> kvp in data.Damages)
            {
                pw.WriteInt(kvp.Key);
                pw.WriteByte(0x06);
                foreach (int dmg in kvp.Value)
                {
                    pw.WriteInt(dmg);
                }
            }
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            if (data.SkillID != (int)Constants.Cleric.Skills.Heal)
            {
                DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
            }
        }


    }
}