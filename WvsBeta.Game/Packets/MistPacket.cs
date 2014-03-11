using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class MistPacket
    {
        public static void SendMistSpawn(Mist pMist, Character pVictim = null, short pDelay = 0)
        {
            Packet packet = new Packet(0x8C);
            packet.WriteInt(pMist.SpawnID);
            packet.WriteBool(pMist.MobMist);
            packet.WriteInt(pMist.SkillID);
            packet.WriteByte(pMist.SkillLevel);
            packet.WriteShort(pDelay);
            packet.WriteInt(pMist.LT_X);
            packet.WriteInt(pMist.LT_Y);
            packet.WriteInt(pMist.RB_X);
            packet.WriteInt(pMist.RB_Y);

            if (pVictim == null)
            {
                DataProvider.Maps[pMist.MapID].SendPacket(packet);
            }
            else
            {
                pVictim.sendPacket(packet);
            }
        }

        public static void SendMistDespawn(Mist pMist)
        {
            Packet packet = new Packet(0x8D);
            packet.WriteInt(pMist.SpawnID);
            DataProvider.Maps[pMist.MapID].SendPacket(packet);
        }
    }
}