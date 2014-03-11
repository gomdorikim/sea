using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{

    public class BuffPacket
    {
        public static void AddMapBuffValues(Character chr, Packet pw, uint pBuffFlags = 0xFFFFFFFF)
        {
            CharacterPrimaryStats ps = chr.PrimaryStats;
            uint added = 0;
            int tmp = pw.Position;
            pw.WriteUInt(added);

            if ((pBuffFlags & (uint)BuffValueTypes.Speed) == (uint)BuffValueTypes.Speed && ps.Speed_N > 0)
            {
                pw.WriteByte((byte)ps.Speed_N);
                added |= (uint)BuffValueTypes.Speed;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.ComboAttack) == (uint)BuffValueTypes.ComboAttack && ps.ComboAttack_N > 0)
            {
                pw.WriteByte((byte)ps.ComboAttack_N);
                added |= (uint)BuffValueTypes.ComboAttack;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.Charges) == (uint)BuffValueTypes.Charges && ps.Charges_N > 0)
            {
                pw.WriteInt(ps.Charges_R);
                added |= (uint)BuffValueTypes.Charges;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.Stun) == (uint)BuffValueTypes.Stun && ps.Stun_N > 0)
            {
                pw.WriteInt(ps.Stun_R);
                added |= (uint)BuffValueTypes.Stun;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.Darkness) == (uint)BuffValueTypes.Darkness && ps.Darkness_N > 0)
            {
                pw.WriteInt(ps.Darkness_R);
                added |= (uint)BuffValueTypes.Darkness;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.Seal) == (uint)BuffValueTypes.Seal && ps.Seal_N > 0)
            {
                pw.WriteInt(ps.Seal_R);
                added |= (uint)BuffValueTypes.Seal;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.Weakness) == (uint)BuffValueTypes.Weakness && ps.Weakness_N > 0)
            {
                pw.WriteInt(ps.Weakness_R);
                added |= (uint)BuffValueTypes.Weakness;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.Curse) == (uint)BuffValueTypes.Curse && ps.Curse_N > 0)
            {
                pw.WriteInt(ps.Curse_R);
                added |= (uint)BuffValueTypes.Curse;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.Poison) == (uint)BuffValueTypes.Poison && ps.Poison_N > 0)
            {
                pw.WriteShort((short)ps.Poison_N);
                added |= (uint)BuffValueTypes.Poison;
            }
            if ((pBuffFlags & (uint)BuffValueTypes.SoulArrow) == (uint)BuffValueTypes.SoulArrow && ps.SoulArrow_N > 0) 
                added |= (uint)BuffValueTypes.SoulArrow;
            if ((pBuffFlags & (uint)BuffValueTypes.ShadowPartner) == (uint)BuffValueTypes.ShadowPartner && ps.ShadowPartner_N > 0) 
                added |= (uint)BuffValueTypes.ShadowPartner;
            if ((pBuffFlags & (uint)BuffValueTypes.DarkSight) == (uint)BuffValueTypes.DarkSight && ps.DarkSight_N > 0) 
                added |= (uint)BuffValueTypes.DarkSight;

            pw.SetUInt(tmp, added);
        }

        public static void SetTempStats(Character chr, uint pFlagsAdded, short pDelay = 0)
        {
            Packet pw = new Packet(0x18);
            chr.PrimaryStats.EncodeForLocal(pw, pFlagsAdded);
            pw.WriteShort(pDelay);
            pw.WriteLong(0);
            chr.sendPacket(pw);
            //MessagePacket.SendNotice("SET TEMMP STATS IS CALLED", chr);
        }

        public static void ResetTempStats(Character chr, uint removedFlags)
        {
            //MessagePacket.SendNotice("removed buff...?", chr);
            Packet pw = new Packet(0x19);
            pw.WriteUInt(removedFlags);
            pw.WriteLong(0);
            chr.sendPacket(pw);
        }
    }
}