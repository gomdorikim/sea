using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Shop {
	public class MessagePacket {
		public static void SendCharge(Character victim) {
			Packet pw = new Packet();
			pw.WriteByte(0xB9);
			pw.WriteString("ilub");
            pw.WriteString("Diamondo25");
			pw.WriteByte(0);
			pw.WriteShort(0);
			pw.WriteInt(0);
			pw.WriteInt(0);
			victim.sendPacket(pw);
		}

		public static void SendCharge(Character victim, bool derp) {
			Packet pw = new Packet();
			pw.WriteByte(0xBB);
			pw.WriteByte(0x4D);
			pw.WriteString("ilub");
			Random rnd = new Random();
			pw.WriteInt(rnd.Next());
			pw.WriteByte((byte)rnd.Next(0, 0xFF));
			victim.sendPacket(pw);
		}

		public static void SendNotice(string what, Character victim) {
			Packet pw = new Packet();
			pw.WriteByte(0x23);
			pw.WriteByte(0);
			pw.WriteString(what);
			victim.sendPacket(pw);
		}

		public static void SendScrollingHeader(string what, Character victim) {
			Packet pw = new Packet();
			pw.WriteByte(0x23);
			pw.WriteByte(4);
			pw.WriteBool((what.Length == 0 ? false : true));
			pw.WriteString(what);
			victim.sendPacket(pw);
			/*
			pw = new Packet();
			pw.WriteByte(0xBB);
			pw.WriteByte(0x2E);
			pw.WriteByte(0x12);
			victim.sendPacket(pw);

			pw = new Packet();
			pw.WriteByte(0xBB);
			pw.WriteByte(0x2E);
			pw.WriteString("test");
			pw.WriteInt(1002140);
			pw.WriteShort(2);
			pw.WriteLong(0);
			pw.WriteLong(0);
			pw.WriteLong(0);
			victim.sendPacket(pw);
			*/
		}
	}
}
