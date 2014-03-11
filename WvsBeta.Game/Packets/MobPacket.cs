using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class MobPacket
    {
        public static void HandleMobControl(Character victim, Packet packet)
        {
            int mobid = packet.ReadInt();
            Mob mob = DataProvider.Maps[victim.Map].GetMob(mobid);
            if (mob == null) return;
            short moveID = packet.ReadShort();
            bool useSkill = packet.ReadBool();
            sbyte skill = packet.ReadSByte();
            byte level = 0;

            Pos projectileTarget = new Pos();
            projectileTarget.X = packet.ReadShort();
            projectileTarget.Y = packet.ReadShort();

            bool isOK = PacketHelper.ParseMovementData(mob, packet);
            if (!isOK && victim.Admin)
            {
                ReportManager.FileNewReport("Suspicious movement detected!", victim.ID, 2);
                return;
            }

            if (useSkill && (skill == -1 || skill == 0))
            {
                if (DataProvider.Mobs.ContainsKey(mob.MobID) && DataProvider.Mobs[mob.MobID].Skills != null) 
                {
                    //MessagePacket.SendMegaphoneMessage("", "ontains mob data", true);
                    byte skills = (byte)DataProvider.Mobs[mob.MobID].Skills.Count;

                    bool usedSkill = false;

                    if (skills > 0)
                    {
                        bool stop = false;
                        byte RandomSkill = 0;
                        Random rnd = new Random();
                        RandomSkill = (byte)rnd.Next(0, skills - 1);
                        MobSkillData msd = DataProvider.Mobs[mob.MobID].Skills[RandomSkill];
                        skill = (sbyte)msd.SkillID;
                        level = msd.Level;
                        MobSkillLevelData theChosenOne = DataProvider.MobSkills[msd.SkillID][msd.Level];
                        switch ((Constants.MobSkills.Skills)msd.SkillID)
                        {
                            /*
                        case Constants.MobSkills.Skills.WeaponAttackUpAoe:
                        case Constants.MobSkills.Skills.WeaponAttackUp:

                            break;
                        case Constants.MobSkills.Skills.MagicAttackUp:
                        case Constants.MobSkills.Skills.MagicAttackUpAoe:
                        case Constants.MobSkills.Skills.WeaponDefenseUp:
                        case Constants.MobSkills.Skills.WeaponDefenseUpAoe:
                        case Constants.MobSkills.Skills.MagicDefenseUp:
                        case Constants.MobSkills.Skills.MagicDefenseUpAoe:
                            */
                            case Constants.MobSkills.Skills.Summon:
                                {
                                    short limit = theChosenOne.SummonLimit;
                                    if (limit == 5000)
                                    {
                                        limit = (short)(30 + DataProvider.Maps[mob.MapID].Characters.Count * 2);
                                    }

                                    break;
                                }

                        }
                        if (!stop)
                        {
                            DateTime now = MasterThread.CurrentDate;
                            if (!mob.LastSkillUse.ContainsKey(theChosenOne.SkillID) || now.Subtract(mob.LastSkillUse[theChosenOne.SkillID]).Ticks > theChosenOne.Cooldown)
                            {
                                mob.LastSkillUse[theChosenOne.SkillID] = now;
                                long reqHP = mob.HP * 100;
                                reqHP /= mob.MaxHP;
                                if (theChosenOne.HPLimit == 0 || (byte)reqHP <= theChosenOne.HPLimit)
                                {
                                    if (msd.EffectAfter == 0)
                                    {
                                        HandleMobSkill(mob, theChosenOne.SkillID, msd.Level);
                                    }
                                    else
                                    {
                                        //System.Timers.Timer tmr = new System.Timers.Timer(msd.EffectAfter * 1000);
                                        //tmr.Elapsed += new System.Timers.ElapsedEventHandler(tmr_Elapsed);
                                        //HiSpeedTimerProvider.Enqueue(new HandleMobSkillDelegate(HandleMobSkill), this, msd.EffectAfter * 1000);

                                        HandleMobSkill(mob, theChosenOne.SkillID, msd.Level);

                                    }
                                    usedSkill = true;
                                }
                            }

                        }
                        if (!usedSkill)
                        {
                            skill = 0;
                            level = 0;
                        }
                    }
                }
            }

            SendMobControlResponse(victim, mobid, moveID, useSkill, mob.MP, (byte)skill, level);
            packet.Reset(13);
            SendMobControlMove(victim, mobid, useSkill, (byte)skill, projectileTarget, packet.ReadLeftoverBytes());
        }

        public static void HandleDistanceFromBoss(Character chr, Packet packet)
        {
            int mapmobid = packet.ReadInt();
            int distance = packet.ReadInt();
            // Do something with it :P
        }

        public static void HandleMobLootDrop(Character chr, Packet packet)
        {
            int mobid = packet.ReadInt();
            int dropid = packet.ReadInt();
            if (!chr.Admin) return; // hehe, anti hax

            Mob mob = DataProvider.Maps[chr.Map].GetMob(mobid);
            if (mob == null) return;

            if (!DataProvider.Maps[chr.Map].Drops.ContainsKey(dropid))
            {
                return;
            }

            DataProvider.Maps[chr.Map].Drops[dropid].TakeDropMob(mobid);
        }

        public delegate void HandleMobSkillDelegate(Mob mob, byte skill, byte level);

        public static void HandleMobSkill(Mob mob, byte skill, byte level)
        {
            MobSkillLevelData data = DataProvider.MobSkills[skill][level];
            Map map = DataProvider.Maps[mob.MapID];
            long currentTime = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate);

            int nValue = data.X;
            int rValue = skill | (level << 16);
            long tValue = currentTime + (data.Time * 1000);
            bool AOE = false;
            uint AddedBuff = 0;
            switch ((Constants.MobSkills.Skills)skill)
            {
                case Constants.MobSkills.Skills.WeaponAttackUpAoe:
                    {
                        mob.Status.PhysicalDamage_N = nValue;
                        mob.Status.PhysicalDamage_R = rValue;
                        mob.Status.PhysicalDamage_T = tValue;
                        AOE = true;
                        AddedBuff = (uint)MobStatus.MobStatValue.PhysicalDamage;
                        break;
                    }
                case Constants.MobSkills.Skills.WeaponAttackUp:
                    {
                        mob.Status.PhysicalDamage_N = nValue;
                        mob.Status.PhysicalDamage_R = rValue;
                        mob.Status.PhysicalDamage_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.PhysicalDamage;
                        break;
                    }
                case Constants.MobSkills.Skills.MagicAttackUpAoe:
                    {
                        mob.Status.MagicDamage_N = nValue;
                        mob.Status.MagicDamage_R = rValue;
                        mob.Status.MagicDamage_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.MagicDamage;
                        AOE = true;
                        break;
                    }
                case Constants.MobSkills.Skills.MagicAttackUp:
                    {
                        mob.Status.MagicDamage_N = nValue;
                        mob.Status.MagicDamage_R = rValue;
                        mob.Status.MagicDamage_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.MagicDamage;
                        break;
                    }




                case Constants.MobSkills.Skills.WeaponDefenseUpAoe:
                    {
                        mob.Status.PhysicalDefense_N = nValue;
                        mob.Status.PhysicalDefense_R = rValue;
                        mob.Status.PhysicalDefense_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.PhysicalDefense;
                        AOE = true;
                        break;
                    }
                case Constants.MobSkills.Skills.WeaponDefenseUp:
                    {
                        mob.Status.PhysicalDefense_N = nValue;
                        mob.Status.PhysicalDefense_R = rValue;
                        mob.Status.PhysicalDefense_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.PhysicalDefense;
                        break;
                    }
                case Constants.MobSkills.Skills.MagicDefenseUpAoe:
                    {
                        mob.Status.MagicDefense_N = nValue;
                        mob.Status.MagicDefense_R = rValue;
                        mob.Status.MagicDefense_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.MagicDefense;
                        AOE = true;
                        break;
                    }
                case Constants.MobSkills.Skills.MagicDefenseUp:
                    {
                        mob.Status.MagicDefense_N = nValue;
                        mob.Status.MagicDefense_R = rValue;
                        mob.Status.MagicDefense_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.MagicDefense;
                        break;
                    }



                case Constants.MobSkills.Skills.PoisonMist:
                    {
                        DataProvider.Maps[mob.MapID].CreateMist(mob, mob.SpawnID, skill, level, data.Time, data.LTX, data.LTY, data.RBX, data.RBY);
                        break;
                    }






                case Constants.MobSkills.Skills.WeaponImmunity:
                    {
                        mob.Status.PhysicalImmune_N = nValue;
                        mob.Status.PhysicalImmune_R = rValue;
                        mob.Status.PhysicalImmune_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.PhysicalImmune;
                        break;
                    }
                case Constants.MobSkills.Skills.MagicImmunity:
                    {
                        mob.Status.MagicImmune_N = nValue;
                        mob.Status.MagicImmune_R = rValue;
                        mob.Status.MagicImmune_T = tValue;
                        AddedBuff = (uint)MobStatus.MobStatValue.MagicImmune;
                        break;
                    }

                case Constants.MobSkills.Skills.Summon:
                    {
                        short miny = (short)(mob.Position.Y + data.LTY);
                        short maxy = (short)(mob.Position.Y + data.RBY);
                        short minx = (short)(mob.Position.X + data.LTX);
                        short maxx = (short)(mob.Position.X + data.RBX);
                        short d = 0;
                        Random rnd = new Random();

                        for (short i = 0; i < data.Summons.Count; i++)
                        {
                            int summonid = data.Summons[i];
                            short summony = (short)rnd.Next(miny, maxy);

                            short summonx = (short)(mob.Position.X + ((d % 2) == 1 ? (35 * (d + 1) / 2) : -(40 * (d / 2))));

                            Pos tehfloor = map.FindFloor(new Pos(summonx, summony));
                            if (tehfloor.Y == summony)
                            {
                                tehfloor.Y = mob.Position.Y;
                            }

                            map.spawnMobNoRespawn(summonid, tehfloor, 0, mob, data.SummonEffect);
                            d++;
                        }
                        break;
                    }
            }
            if (AddedBuff != 0)
            {
                SendMobStatsTempSet(mob, 0, AddedBuff);
            }
        }

        public static void SendMobSpawn(Character victim, Mob mob, byte summonEffect, Mob owner, bool spawn, bool show)
        {
            Packet pw = new Packet(0x75);
            pw.WriteInt(mob.SpawnID);
            pw.WriteInt(mob.MobID);
            pw.WriteShort(mob.Position.X);
            pw.WriteShort(mob.Position.Y);
            byte bitfield = (byte)(owner != null ? 0x08 : 0x02);
            pw.WriteByte(bitfield); // Bitfield
            pw.WriteShort(mob.Foothold);
            pw.WriteShort(mob.OriginalFoothold); // Original foothold, doesn't really matter

            if (owner != null)
            {
                pw.WriteByte((byte)(summonEffect != 0 ? summonEffect : -3));
                pw.WriteInt(owner.SpawnID);
            }
            else
            {
                pw.WriteByte((byte)(spawn ? -2 : -1));
            }
            pw.WriteByte(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);


            if (show && victim != null)
            {
                victim.sendPacket(pw);
            }
            else
            {
                DataProvider.Maps[mob.MapID].SendPacket(pw);
            }

        }

        public static void SendMobDeath(Mob mob, byte how)
        {
            Packet pw = new Packet(0x76);
            pw.WriteInt(mob.SpawnID);
            pw.WriteByte(how);
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[mob.MapID].SendPacket(pw);
        }

        public static void SendMobRequestControl(Character victim, Mob mob, bool spawn, Character display)
        {
            Packet pw = new Packet(0x77);
            pw.WriteByte(0x01);
            pw.WriteInt(mob.SpawnID);
            pw.WriteInt(mob.MobID);
            pw.WriteShort(mob.Position.X);
            pw.WriteShort(mob.Position.Y);
            pw.WriteByte(0x02); // Bitfield
            pw.WriteShort(mob.Foothold);
            pw.WriteShort(mob.OriginalFoothold); // Original foothold, doesn't really matter
            pw.WriteByte((byte)(spawn ? -2 : -1));
            pw.WriteByte(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            if (victim != null)
            {
                victim.sendPacket(pw);
            }
            else if (display != null)
            {
                display.sendPacket(pw);
            }
            else
            {
                DataProvider.Maps[mob.MapID].SendPacket(pw);
            }
        }

        public static void SendMobRequestEndControl(Character victim, Mob mob)
        {
            Packet pw = new Packet(0x77);
            pw.WriteByte(0x00);
            if (victim == null)
            {
                DataProvider.Maps[mob.MapID].SendPacket(pw);
            }
            else
            {
                victim.sendPacket(pw);
            }
        }

        public static void SendMobControlResponse(Character victim, int mobid, short moveid, bool useSkill, int MP, byte skill, byte level)
        {
            Packet pw = new Packet(0x7A);
            pw.WriteInt(mobid);
            pw.WriteShort(moveid);
            pw.WriteBool(useSkill);
            pw.WriteShort((short)MP);
            pw.WriteByte(skill);
            pw.WriteByte(level);
            pw.WriteLong(0);
            pw.WriteLong(0);
            victim.sendPacket(pw);
        }

        public static void SendMobControlMove(Character victim, int mobid, bool useSkill, byte skill, Pos projectileTarget, byte[] Buffer)
        {
            Packet pw = new Packet(0x79);
            pw.WriteInt(mobid);
            pw.WriteBool(useSkill);
            pw.WriteByte(skill);
            pw.WriteInt(0); // Unknown
            pw.WriteBytes(Buffer);
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[victim.Map].SendPacket(pw, victim, false);
        }

        public static void SendMobDamageOrHeal(Character victim, int mobid, int amount, bool isHeal)
        {
            Packet pw = new Packet(0x80);
            pw.WriteInt(mobid);
            pw.WriteByte(0);
            pw.WriteInt((isHeal ? -amount : amount));
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[victim.Map].SendPacket(pw);
        }

        public static void SendMobDamageOrHeal(int mapid, int mobid, int amount, bool isHeal)
        {
            Packet pw = new Packet(0x80);
            pw.WriteInt(mobid);
            pw.WriteByte(0);
            pw.WriteInt((isHeal ? -amount : amount));
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[mapid].SendPacket(pw);
        }




        public static void SendMobStatsTempSet(Mob pMob, short pDelay, uint pSpecificFlag = 0xFFFFFFFF)
        {
            Packet pw = new Packet(0x7C);
            pw.WriteInt(pMob.SpawnID);

            pMob.Status.Encode(pw, pSpecificFlag);

            pw.WriteShort(pDelay);

            DataProvider.Maps[pMob.MapID].SendPacket(pw);
        }

        public static void SendMobStatsTempReset(Mob pMob, uint pFlags)
        {
            Packet pw = new Packet(0x7D);
            pw.WriteInt(pMob.SpawnID);

            pw.WriteUInt(pFlags); // Flags

            DataProvider.Maps[pMob.MapID].SendPacket(pw);
        }
    }
}