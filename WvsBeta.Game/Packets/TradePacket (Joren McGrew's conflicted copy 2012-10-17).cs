//Author: anoob123
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class TradePacket
    {
        public static void TradeStart(Character chr, byte number) //this packet is not correct
        {
            Packet pw = new Packet();
            pw.WriteByte(0xAE);
            pw.WriteByte(5);
            pw.WriteByte(3);
            //pw.WriteByte(2);
            //pw.WriteByte(number);
            if (number == 1)
            {
                pw.WriteByte(0);
                pw.WriteString(chr.Name);
                pw.WriteByte(chr.Gender);
                pw.WriteByte(chr.Skin);
                pw.WriteInt(chr.Face);
                pw.WriteInt(chr.Hair);
                foreach (KeyValuePair<byte, int> equip in chr.Inventory.GetVisibleEquips())
                {
                    pw.WriteByte(equip.Key);
                    pw.WriteInt(equip.Value);
                }
            }
            pw.WriteByte(number);
            pw.WriteString(chr.Name);
            pw.WriteByte(chr.Gender);
            pw.WriteByte(chr.Skin);
            pw.WriteInt(chr.Face);
            pw.WriteInt(chr.Hair);
            foreach (KeyValuePair<byte, int> equip in chr.Inventory.GetVisibleEquips())
            {
                pw.WriteByte(equip.Key);
                pw.WriteInt(equip.Value);
            }
            pw.WriteString("lulul");
            pw.WriteByte(0xFF);
            chr.sendPacket(pw);

        }
    }
}