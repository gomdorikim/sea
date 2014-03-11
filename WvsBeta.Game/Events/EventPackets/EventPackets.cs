using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game.Events
{
    class EventPackets
    {
        public static void HandleAdminEventStart(Character chr, Packet packet)
        {
            if (chr.Map == MapleSnowball.sMap)
            {
                EventManager.Instance.RegisterEvent(new Event(Event.EventType.Snowball));
                MapleSnowball.snowball0 = new Snowball(0);
                MapleSnowball.snowball1 = new Snowball(1);
                MapleSnowball msb = new MapleSnowball(MapleSnowball.snowball0, MapleSnowball.snowball1);
                MapPacket.MapTimer(chr, 600);
            }
            if (OlaOla.sMap.Contains(chr.Map))
            {
                EventManager.Instance.RegisterEvent(new Event(Event.EventType.OlaOla));
            }
        }
        public static void HandleAdminEventReset(Character chr, Packet packet)
        {
            if (chr.Map == MapleSnowball.sMap)
            {
                EventManager.Instance.RegisteredEvents.Remove(Event.EventType.Snowball);
            }
            if (OlaOla.sMap.Contains(chr.Map))
            {
                EventManager.Instance.RegisteredEvents.Remove(Event.EventType.OlaOla);
            }
        }
    }

    public class SnowballPackets
    {
        public static void HitSnowball(Character chr, byte up, short damage, short Stance)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x9B);
            pw.WriteByte(up);
            pw.WriteShort(damage); //always 10 anyways
            pw.WriteShort(Stance);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void HitSnowman(Character chr, byte up, short damage, short stance)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x9B);
            pw.WriteByte(up); //2 bottom, 3 top
            pw.WriteShort(damage);
            pw.WriteShort(stance);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void RollSnowball(Character chr, byte type, byte Distance0, byte Distance1, byte StageBottom, byte StageTop)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x9A);
            pw.WriteByte(type); //0 normal, 1 rolls from start to end, 2 bottom invisible, 3 top invisible
            pw.WriteByte(Distance0); //increment amount from stage
            pw.WriteByte(StageBottom);
            pw.WriteByte(00);
            pw.WriteByte(Distance1);
            pw.WriteByte(StageTop);
            pw.WriteLong(0);
            DataProvider.Maps[chr.Map].SendPacket(pw);

        }

        public static void SendSnowballRules(Character chr, int NpcID, string Text, bool back, bool next) //just the regular NPC packet :p
        {
            Packet pw = new Packet(0xA0);
            pw.WriteByte(0x04);
            pw.WriteInt(NpcID);
            pw.WriteByte(0);
            pw.WriteString(Text);
            pw.WriteBool(back);
            pw.WriteBool(next);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            chr.sendPacket(pw);

        }

        public static void OXQuiz(Character chr)
        {
            //It's in korean -.-
            Packet pw = new Packet(0x34);
            pw.WriteByte(1); //??
            pw.WriteByte(1); //??
            pw.WriteShort(1); //??
            chr.sendPacket(pw);
        }
    }
}
