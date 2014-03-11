using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class MiscPacket
    {

        public static void SendGotMesosFromLucksack(Character chr, int amount)
        {
            Packet pw = new Packet(0x65);
            pw.WriteInt(amount);
            chr.sendPacket(pw);
        }

        public static void SendMesoFromLucksackFailed(Character chr)
        {
            Packet pw = new Packet(0x66);
            pw.WriteByte(0);
            chr.sendPacket(pw);
        }

    }
}
