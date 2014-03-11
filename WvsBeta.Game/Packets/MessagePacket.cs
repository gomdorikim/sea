using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;


namespace WvsBeta.Game {
	public class MessagePacket {
		public enum MessageTypes : byte {
			Notice = 0x00,
			PopupBox = 0x01,
			Megaphone = 0x02,
			SuperMegaphone = 0x03,
			Header = 0x04,
			RedText = 0x05
		}
		public enum MessageMode : byte { ToPlayer, ToMap, ToChannel }


		public static void HandleChat(Character chr, Packet packet) {
			string what = packet.ReadString();
            if (!CommandHandling.HandleChat(chr, what) && !CheatInspector.CheckTextSpam(what) && !CheatInspector.CheckSpam(chr.CharacterCheatCheck, DateTime.Now))
            {
                if (CheatInspector.CheckCurse(what))
                {
                    MapPacket.SendChatMessage(chr, what);
                }
                else
                {
                    if (Server.Instance.AdultWorld || chr.Admin)
                    {
                        MapPacket.SendChatMessage(chr, what);
                    }
                    else
                    {
                        SendText(MessageTypes.PopupBox, "Cursing is not allowed.", chr, MessageMode.ToPlayer);
                    }
                }
            }
            else if (CheatInspector.CheckTextSpam(what)) //user typed passed 138 characters
            {
                chr.mPlayer.Socket.Disconnect();
            }
            else if (CheatInspector.CheckSpam(chr.CharacterCheatCheck, DateTime.Now))
            {
                ReportManager.FileNewReport(what, chr.ID, 3);
                chr.mPlayer.Socket.Disconnect();
            }
            CheatInspector.CheckSuspiciousText(chr, what);
		}

        public static void HandleSpecialChat(Character chr, Packet packet)
        {
            //to be handled via center server
            byte Type = packet.ReadByte();
            byte CountOfRecipents = packet.ReadByte();
            int[] Recipents = new int[CountOfRecipents];
            for (int i = 0; i < CountOfRecipents; i++)
            {
                Recipents[i] = packet.ReadInt();
            }
            string Message = packet.ReadString();
            switch (Type)
            {
                case 0: //Buddy chat
                    Server.Instance.CenterConnection.BuddyChat(chr, Message);
                    break;
                case 1: //Party Chat
                    Server.Instance.CenterConnection.PlayerPartyOperation(chr, 6, chr.PartyID, 0, false, "", "", Message);
                    break;
            }
        }

		public static void HandleCommand(Character chr, Packet packet) {
			byte type = packet.ReadByte();
			string victim = packet.ReadString();

			Character victimChar = Server.Instance.GetCharacter(victim);

			switch (type) {
				case 0x05:
					if (victimChar != null) {
						Find(chr, victim, victimChar.Map, 0, true);
					}
					else {
						Server.Instance.CenterConnection.PlayerFind(chr.ID, victim);
					}
					break;
				case 0x06:
					string message = packet.ReadString();
					if (victimChar != null) {
						Whisper(victimChar, chr.Name, Server.Instance.ID, message);
						Find(chr, victim, -1, 1, false);
					}
					else {
						Server.Instance.CenterConnection.PlayerWhisper(chr.ID, victim, message);
					}
					break;
			}
		}



		public static void SendMegaphoneMessage(string who, string what, bool bot = false) {
			Packet pw = new Packet(0x2C);
			pw.WriteByte((byte)MessageTypes.Megaphone);
            pw.WriteString(what); //Bugged 
            

			foreach (KeyValuePair<int, Map> kvp in DataProvider.Maps) {
				kvp.Value.SendPacket(pw);
			}
		}

		public static void SendMegaphoneMessage(string what, int mapid) {
			Packet pw = new Packet(0x2C);
			pw.WriteByte((byte)MessageTypes.Megaphone);
			pw.WriteString(what);
			DataProvider.Maps[mapid].SendPacket(pw);
		}

        public static void SendSuperMegaphoneMessage(string what, bool WhisperOrFind, byte channel)
        {
            Packet pw = new Packet(0x2C);
            pw.WriteByte((byte)MessageTypes.SuperMegaphone);
            pw.WriteString(what);
            if (channel == 1) channel = 0; // Bugged O.o
            pw.WriteByte(channel);
            pw.WriteBool(WhisperOrFind);
            foreach (KeyValuePair<int, Map> kvp in DataProvider.Maps)
                kvp.Value.SendPacket(pw);
        }

		public static void SendText(MessageTypes type, string what, Character victim, MessageMode mode) {
			Packet pw = new Packet(0x2C);
			pw.WriteByte((byte)type);
			pw.WriteString(what);
			if (type == MessageTypes.Header || type == MessageTypes.SuperMegaphone) return;
			switch (mode) {
				case MessageMode.ToPlayer: victim.sendPacket(pw); break;
				case MessageMode.ToMap: DataProvider.Maps[victim.Map].SendPacket(pw); break;
				case MessageMode.ToChannel: 
					foreach (KeyValuePair<int, Map> kvp in DataProvider.Maps) {
						kvp.Value.SendPacket(pw);
					} 
					break;
			}
		}
        
        

		public static void SendNotice(string what, Character victim) {
			Packet pw = new Packet(0x2C);
			pw.WriteByte((byte)MessageTypes.Notice);
			pw.WriteString(what);
			victim.sendPacket(pw);
		}

		public static void SendNoticeMap(string what, int mapid) {
			Packet pw = new Packet(0x2C);
			pw.WriteByte((byte)MessageTypes.Notice);
			pw.WriteString(what);
			DataProvider.Maps[mapid].SendPacket(pw);
		}

		public static void SendScrollingHeader(string what, Character victim) {
			Packet pw = new Packet(0x2C); //2A is party... ??
			pw.WriteByte((byte)MessageTypes.Header);
			pw.WriteBool((what.Length == 0 ? false : true));
			pw.WriteString(what);
            
			victim.sendPacket(pw);
		}

		public static void SendScrollingHeader(string what, int mapid) {
			Packet pw = new Packet(0x2C);
			pw.WriteByte((byte)MessageTypes.Header);
			pw.WriteBool((what.Length == 0 ? false : true));
			pw.WriteString(what);
			DataProvider.Maps[mapid].SendPacket(pw);
		}

        public static void SendAdminMessage(Character chr, string what, byte Type, byte to)
        {
            Packet pw = new Packet(0x2C);
            pw.WriteByte(Type);
            if (Type == 4)
            {
                pw.WriteBool((what.Length == 0 ? false : true));
            }
            pw.WriteString(what);
            switch (to)
            {
                case 0x00:
                    Server.Instance.CenterConnection.AdminMessage(what, Type);
                    break;
                case 0x01:
                    foreach (KeyValuePair<int, Map> kvp in DataProvider.Maps)
                    {
                        kvp.Value.SendPacket(pw);
                    } 
                break;
                case 0x02:
                DataProvider.Maps[chr.Map].SendPacket(pw);
                break;

            }
        }

		public static void Whisper(Character victim, string who, byte channel, string message) {
			Packet pw = new Packet(0x37);
			pw.WriteByte(0x12);
			pw.WriteString(who);
			pw.WriteByte(channel);
			pw.WriteString(message);
			victim.sendPacket(pw);
		}

        public static void Find(Character victim, string who, int map, sbyte dunno, bool isChannel)
        {
            Packet pw = new Packet(0x37);

            if (victim.Admin == true /* || victim == Character.isGM */ )
            {
                //Character should not be found!
                pw.WriteByte(0x05); //Red Text
                pw.WriteString("User is not found");
            }
            else if (map != -1)
            {
                pw.WriteByte(0x09);
                pw.WriteString(who);
                if (map == -2)
                {
                    // In cashshop
                    pw.WriteByte(0x02);
                }
                else if (isChannel)
                {
                    pw.WriteByte(0x01);
                    pw.WriteInt(map);
                    pw.WriteInt(0);
                }
                else
                {
                    pw.WriteByte(0x03);
                    pw.WriteInt(map);
                }
                pw.WriteInt(0);
            }
            else
            {
                pw.WriteByte(0x0A);
                pw.WriteString(who);
                pw.WriteSByte(dunno);
            }
            victim.sendPacket(pw);
		}
	}
}
