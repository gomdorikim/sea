using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game {
	public class AdminPacket {
        public static void HandleAdminCommandMessage(Character chr, Packet packet)
        {
            byte to = packet.ReadByte();
            byte TypeMessage = packet.ReadByte(); //   /alert, /notice, /slide
            string Message = packet.ReadString();
            switch (to)
            {
                case 0x00: //To every game server
                    MessagePacket.SendAdminMessage(chr, Message, TypeMessage, 0);
                    break;
                case 0x01: //To channel
                    MessagePacket.SendAdminMessage(chr, Message, TypeMessage, 1);
                    break;
                case 0x02: //To map
                    MessagePacket.SendAdminMessage(chr, Message, TypeMessage, 2);
                    break;
            }
        }
        
        public static void Hide(Character chr, bool hide) {
			Packet pw = new Packet(0x3C);
			pw.WriteByte(0x0F);
			pw.WriteBool(hide);
			chr.sendPacket(pw);
		}

        public static void BanCharacterMessage(Character chr)
        {
            Packet pw = new Packet(0x33);
            pw.WriteByte(0x04);
            pw.WriteByte(0);
            chr.sendPacket(pw);
        }

        public static void InvalidNameMessage(Character chr)
        {
            Packet pw = new Packet(0x33);
            pw.WriteByte(0x06);
            pw.WriteByte(0x0A);
            chr.sendPacket(pw);
        }
        public static void NPCwhat(Character chr) // I have no idea what this is supposed to do :S (Something with NPC's)
        {
            //format ; {string} : {string} = {string} 
            Packet pw = new Packet(0x33);
            pw.WriteByte(0x08);
            pw.WriteString("");
            pw.WriteString("");
            pw.WriteString("");
            chr.sendPacket(pw);
        }
	}
}
