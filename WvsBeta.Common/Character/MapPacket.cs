using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Common
{
    public class GroupMessagePacket
    {
        public static byte[] GroupMessage(CharacterBase chr, string text, byte group) //needs work 
        {
            //!packet 2D 01 04 00 6A 6F 65 70 01003400000000000000
            Packet pw = new Packet(0x36);
            pw.WriteByte(group);
            pw.WriteString(chr.Name);
            pw.WriteString(text);
            return pw.ToArray();
        }

        public static byte[] NoneOnline(CharacterBase pCharacter)
        {
            Packet pw = new Packet(0x2C);
            pw.WriteByte(0x05);
            pw.WriteString("Either the party doeesn't exist or no member of your party is logged on.");
            return pw.ToArray();
        }
    }
}
