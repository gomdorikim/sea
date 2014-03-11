using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class SummonPacket
    {
        public static void HandleSummonDamage(Character chr, Packet packet)
        {
            int summonid = packet.ReadInt();
            Summon summon = chr.Summons.GetSummon(summonid);
            if (summon == null || summon != chr.Summons.mPuppet)
            {
                return;
            }

            sbyte unk = packet.ReadSByte();
            int damage = packet.ReadInt();
            int mobid = packet.ReadInt();
            byte unk2 = packet.ReadByte();

            SendDamageSummon(chr, summonid, unk, damage, mobid, unk2);

            if (summon.mHP - damage < 0)
            {
                chr.Summons.RemoveSummon(summon == chr.Summons.mPuppet, 0x02);
            }
            else
            {
                summon.mHP -= damage;
            }
        }

        public static void HandleSummonMove(Character chr, Packet packet)
        {
            int summonid = packet.ReadInt();
            Summon summon = chr.Summons.GetSummon(summonid);
            if (summon == null || summon == chr.Summons.mPuppet)
            {
                return;
            }
            PacketHelper.ParseMovementData(summon, packet);
            packet.Reset(5);
            SendMoveSummon(chr, summonid, packet.ReadLeftoverBytes());
        }

        public static void SendShowSummon(Character chr, Summon summon, bool animated, Character victim)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x4A);
            pw.WriteInt(chr.ID);
            pw.WriteInt(summon.mSummonID);
            pw.WriteByte(summon.mLevel);
            pw.WriteShort(summon.Position.X);
            pw.WriteShort(summon.Position.Y);
            pw.WriteByte(summon.Stance);
            pw.WriteShort(summon.Foothold);
            pw.WriteBool(!(summon.mSummonID == (int)Constants.Sniper.Skills.Puppet || summon.mSummonID == (int)Constants.Ranger.Skills.Puppet));
            pw.WriteBool(!animated);
            if (victim != null) victim.sendPacket(pw);
            else DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void SendRemoveSummon(Character chr, int summonid, byte message)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x4B);
            pw.WriteInt(chr.ID);
            pw.WriteInt(summonid);
            pw.WriteByte(message);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void SendMoveSummon(Character chr, int summonid, byte[] data)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x4C);
            pw.WriteInt(chr.ID);
            pw.WriteInt(summonid);
            pw.WriteBytes(data);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendSummonAttack(Character chr, AttackPacket.AttackData ad)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x4D);
            pw.WriteInt(chr.ID);
            pw.WriteInt(ad.SummonID);
            pw.WriteByte(ad.Animation);
            pw.WriteByte(ad.Targets);
            foreach (KeyValuePair<int, List<int>> kvp in ad.Damages)
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
            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendDamageSummon(Character chr, int summonid, sbyte unk, int damage, int mobid, byte unk2)
        {
            // Needs to be fixed.
            Packet pw = new Packet();
            pw.WriteByte(0x4E);
            pw.WriteInt(chr.ID);
            pw.WriteInt(summonid);
            pw.WriteSByte(-1);
            pw.WriteInt(damage);
            pw.WriteInt(mobid);

            pw.WriteByte(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }
    }
}