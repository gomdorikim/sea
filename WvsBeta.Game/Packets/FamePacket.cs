using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using WvsBeta.Database;

namespace WvsBeta.Game {
	public class FamePacket {
		public static void HandleFame(Character chr, Packet pr) {
			int charId = pr.ReadInt();
			bool up = pr.ReadBool();

			if (charId == chr.ID) {
				return;
			}
			else if (DataProvider.Maps[chr.Map].GetPlayer(charId) == null) {
				SendFameError(chr, 0x01); // Incorrect User error
			}
			else {
				if (chr.PrimaryStats.Level < 15) {
					SendFameError(chr, 0x02); // Level under 15
					return;
				}
				Server.Instance.CharacterDatabase.RunQuery("SELECT `time` FROM `fame_log` WHERE `from` = " + chr.ID + " AND UNIX_TIMESTAMP(`time`) > UNIX_TIMESTAMP()-86400 ORDER BY `time` DESC LIMIT 1");
				MySql.Data.MySqlClient.MySqlDataReader reader = Server.Instance.CharacterDatabase.Reader;
				if (reader.HasRows) {
					SendFameError(chr, 0x03); // This Day error
					return;
				}

				Server.Instance.CharacterDatabase.RunQuery("SELECT `time` FROM `fame_log` WHERE `from` = " + chr.ID + " AND UNIX_TIMESTAMP(`time`) > UNIX_TIMESTAMP()-2592000 ORDER BY `time` DESC LIMIT 1");
				reader = Server.Instance.CharacterDatabase.Reader;
				if (reader.HasRows) {
					SendFameError(chr, 0x04); // This Month error
					return;
				}


				Character victim = DataProvider.Maps[chr.Map].GetPlayer(charId);
				victim.AddFame((short)(up ? 1 : -1));

				Server.Instance.CharacterDatabase.RunQuery("INSERT INTO fame_log (`from`, `to`, `time`) VALUES (" + chr.ID.ToString() + "," + victim.ID.ToString() + ", NOW())");

				SendFameSucceed(chr, victim, up);
			}
		}

		public static void SendFameError(Character chr, int error) {
			Packet pw = new Packet(0x1C);
			pw.WriteInt(error);
			chr.sendPacket(pw);
		}

		public static void SendFameSucceed(Character chr, Character victim, bool up) {
			Packet pw = new Packet(0x1C);
			pw.WriteByte(0x05);
			pw.WriteString(chr.Name);
			pw.WriteBool(up);
			victim.sendPacket(pw);

			pw = new Packet(0x1C);
			pw.WriteByte(0x00);
			pw.WriteString(victim.Name);
			pw.WriteBool(up);
			pw.WriteInt(victim.PrimaryStats.Fame);
			chr.sendPacket(pw);
		}
	}
}
