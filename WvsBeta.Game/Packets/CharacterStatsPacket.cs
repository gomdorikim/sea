using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class DamageReflectorSkillData
    {
        public byte Reduction = 0;
        public int Damage = 0;
        public int ObjectID = 0;
        public bool IsPhysical = false;
        public Pos Position = new Pos(0, 0);
    }

    public class CharacterStatsPacket
    {
        public enum Constants : uint
        {
            Skin = 0x01,
            Eyes = 0x02,
            Hair = 0x04,
            Pet = 0x08,
            Level = 0x10,
            Job = 0x20,
            Str = 0x40,
            Dex = 0x80,
            Int = 0x100,
            Luk = 0x200,
            Hp = 0x400,
            MaxHp = 0x800,
            Mp = 0x1000,
            MaxMp = 0x2000,
            Ap = 0x4000,
            Sp = 0x8000,
            Exp = 0x10000,
            Fame = 0x20000,
            Mesos = 0x40000
        };

        public static void HandleStats(Character chr, Packet packet)
        {
            uint flag = packet.ReadUInt();
            if (chr.PrimaryStats.AP == 0)
            {
                return;
            }

            short max = 2000;
            short max2 = 30000;

            switch ((Constants)flag)
            {
                case Constants.Str:
                    {
                        if (chr.PrimaryStats.Str >= max)
                        {
                            InventoryPacket.NoChange(chr);
                            return;
                        }
                        chr.AddStr(1);
                        break;
                    }
                case Constants.Dex:
                    {
                        if (chr.PrimaryStats.Dex >= max)
                        {
                            InventoryPacket.NoChange(chr);
                            return;
                        }
                        chr.AddDex(1);
                        break;
                    }
                case Constants.Int:
                    {
                        if (chr.PrimaryStats.Int >= max)
                        {
                            InventoryPacket.NoChange(chr);
                            return;
                        }
                        chr.AddInt(1);
                        break;
                    }
                case Constants.Luk:
                    {
                        if (chr.PrimaryStats.Luk >= max)
                        {
                            InventoryPacket.NoChange(chr);
                            return;
                        }
                        chr.AddLuk(1);
                        break;
                    }
                case Constants.MaxHp:
                    {
                        if (chr.PrimaryStats.MaxHP >= max2)
                        {
                            InventoryPacket.NoChange(chr);
                            return;
                        }
                        chr.ModifyMaxHP(1);
                        break;
                    }
                case Constants.MaxMp:
                    {
                        if (chr.PrimaryStats.MaxMP >= max2)
                        {
                            InventoryPacket.NoChange(chr);
                            return;
                        }
                        chr.ModifyMaxMP(1);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unknown type {0:X4}", flag);
                        break;
                    }
            }
            chr.AddAP(-1);
            chr.PrimaryStats.CalculateAdditions(false, false);
            InventoryPacket.NoChange(chr);
        }

        public static void HandleHeal(Character chr, Packet packet)
        {
            // 2B 00 14 00 00 00 00 03 00 00
            packet.Skip(4);
            short hp = packet.ReadShort();
            short mp = packet.ReadShort();
            if (chr.PrimaryStats.HP == 0 || hp > 400 || mp > 1000 || (hp > 0 && mp > 0))
            {
                return;
            }

            if (hp > 0)
            {
                chr.ModifyHP(hp);
                /**
                if (chr.mParty != null)
                {
                    chr.mParty.UpdatePartyMemberHP(chr);
                    chr.mParty.ReceivePartyMemberHP(chr);
                }
                 * **/
            }

            if (mp > 0)
            {
                chr.ModifyMP(mp);
            }
        }

        public static void SendStatChange(Character chr, uint flag, byte value, bool isBySelf = false)
        {
            Packet pw = new Packet(0x17);
            pw.WriteBool(isBySelf);
            pw.WriteUInt(flag);
            pw.WriteByte(value);
            chr.sendPacket(pw);
        }

        public static void SendStatChange(Character chr, uint flag, short value, bool isBySelf = false)
        {
            Packet pw = new Packet(0x17);
            pw.WriteBool(isBySelf);
            pw.WriteUInt(flag);
            pw.WriteShort(value);
            chr.sendPacket(pw);
        }

        public static void SendStatChange(Character chr, uint flag, int value, bool isBySelf = false)
        {
            Packet pw = new Packet(0x17);
            pw.WriteBool(isBySelf);
            pw.WriteUInt(flag);
            pw.WriteInt(value);
            chr.sendPacket(pw);
        }

        public static void SendStatChange(Character chr, uint flag, long value, bool isBySelf = false)
        {
            Packet pw = new Packet(0x16);
            pw.WriteBool(isBySelf);
            pw.WriteUInt(flag);
            pw.WriteLong(value);
            chr.sendPacket(pw);
        }

        public static void HandleCharacterDamage(Character chr, Packet pr)
        {
            //1A FF 03 00 00 00 00 00 00 00 00 04 87 01 00 00 00
            sbyte type = pr.ReadSByte();
            int damage = pr.ReadInt();

            if (damage == 0)
            {
                chr.CharacterCheatCheck.HandleMiss();
            }
            else if (damage > 0)
            {
                chr.CharacterCheatCheck.ResetMisses();
            }

            int mobid = 0;
            if (type != -2)
            {
                mobid = pr.ReadInt();
            }

            if (chr.Buffs.GetActiveSkillLevel((int)WvsBeta.Common.Constants.Magician.Skills.MagicGuard) > 0 && chr.PrimaryStats.MP > 0)
            {
                // Absorbs X amount of damage. :)
                byte mg = chr.Buffs.GetActiveSkillLevel((int)WvsBeta.Common.Constants.Magician.Skills.MagicGuard);

                SkillLevelData sld = DataProvider.Skills[(int)WvsBeta.Common.Constants.Magician.Skills.MagicGuard][mg];
                int damageEaten = (int)Math.Round((damage * (double)(sld.XValue / 100.0d)));
                damage = (int)Math.Round((damage * (double)((100 - sld.XValue) / 100.0d)));
                chr.ModifyMP((short)-damageEaten);
            }
            /**
            if (chr.mParty != null)
            {
                chr.mParty.UpdatePartyMemberHP(chr);
            }
             * **/

            chr.ModifyHP((short)-damage);

            SendCharacterDamage(chr, type, damage, mobid, 0, 0, 0, new DamageReflectorSkillData());
        }

        public static void SendCharacterDamage(Character chr, sbyte what, int amount, int mobid, byte hit, byte stance, int noDamageSkill, DamageReflectorSkillData pgmr)
        {
            Packet pw = new Packet(0x60);
            pw.WriteInt(chr.ID);
            pw.WriteByte(0xFE);// what);
            pw.WriteInt(amount);
            pw.WriteInt(amount);

            if (what == -2)
            {
                pw.WriteInt(amount);
            }
            else
            {
                pw.WriteInt((int)(pgmr.Reduction > 0 ? pgmr.Damage : amount));
                pw.WriteInt(mobid);
                pw.WriteByte(hit);
                pw.WriteByte(pgmr.Reduction);
                if (pgmr.Reduction > 0)
                {
                    pw.WriteBool(pgmr.IsPhysical);
                    pw.WriteShort(pgmr.Position.X);
                    pw.WriteShort(pgmr.Position.Y);
                }
                pw.WriteByte(stance);
                if (noDamageSkill > 0)
                {
                    pw.WriteInt(noDamageSkill);
                }
            }
            pw.WriteInt(amount);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void SendGainEXP(Character chr, int amount, bool white, bool inChat)
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(0x03);
            pw.WriteBool(white);
            pw.WriteInt(amount);
            pw.WriteBool(inChat);
            chr.sendPacket(pw);
        }

        public static void SendGainDrop(Character chr, bool isMesos, int id, short amount)
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(0x00);
            pw.WriteBool(isMesos);
            pw.WriteInt(id);

            byte inv = (byte)(id / 1000000);
            if (inv != 1)
            {
                //pw.WriteShort(amount);
            }
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            chr.sendPacket(pw);
        }

        /*
        public static void SendCharacterStatchange(Character chr, Dictionary<uint, long> values)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x14);
            pw.WriteByte(0); // Don't know actually
            uint flags = 0x0;
            foreach (KeyValuePair<uint, long> kvp in values)
            {
                flags += kvp.Key;
            }
            pw.WriteUInt(flags);
            if (values.ContainsKey((uint)Constants.Skin))
            {
                pw.WriteByte((byte)values[(uint)Constants.Skin]);
            }
            if (values.ContainsKey((uint)Constants.Eyes))
            {
                pw.WriteInt((int)values[(uint)Constants.Eyes]);
            }
            if (values.ContainsKey((uint)Constants.Hair))
            {
                pw.WriteInt((int)values[(uint)Constants.Hair]);
            }
            if (values.ContainsKey((uint)Constants.Pet))
            {
                pw.WriteLong((int)values[(uint)Constants.Pet]);
            }
            if (values.ContainsKey((uint)Constants.Level))
            {
                pw.WriteByte((byte)values[(uint)Constants.Level]);
            }
            if (values.ContainsKey((uint)Constants.Level))
            {
                pw.WriteByte((byte)values[(uint)Constants.Level]);
            }

            
        }*/
    }
}