using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using WvsBeta.Game.Events;

namespace WvsBeta.Game
{
    class CoconutPackets
    {
        public static void HandleEvent(Character chr, Packet packet)
        {
            short CoconutID = packet.ReadShort();
            short CharStance = packet.ReadShort();
            CoconutOperation(chr, Events.CoconutOperation.Hit, CoconutID, CharStance);
            //wish i had more battery life on my laptop so i could finish this :(
        }

        public static void CoconutScore(Character chr, short maple, short story)
        {
            //Only works on map 109080002 :S
            Packet pw = new Packet(0xA8);
            pw.WriteShort(maple);
            pw.WriteShort(story);
            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SpawnCoconut(Character chr, bool spawn, int id, int type)
        {
            Packet pw = new Packet(0xA7);
            pw.WriteShort(0); //Coconut ID
            pw.WriteShort(0); //Type of hit?
            pw.WriteByte(0); //0 = spawn 1 = hit 
            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void HitCoconut(Character chr, short cID, short Stance)
        {
            Packet pw = new Packet(0x9C);
            pw.WriteShort(cID); //Coconut ID
            pw.WriteShort(Stance); //Delay! lol
            pw.WriteByte(1); //0 = spawn 1 = hit 2 = break 3 = destroy
            chr.sendPacket(pw);
        }

        public static void CoconutOperation(Character chr, CoconutOperation operation, short ID, short stance) 
        {
            Packet pw = new Packet(0xA7);
            pw.WriteShort(ID);
            pw.WriteShort(stance);
            pw.WriteByte((byte)operation);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void ForcedEquip(Character chr, byte team)
        {
            Packet pw = new Packet(0x35);
            pw.WriteByte(team); //0 : red, 1 : blue
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }
    }
}
