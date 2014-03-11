using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Center
{
    public class LocalConnection : AbstractConnection
    {
        public LocalServer Server { get; set; }
        public static List<MessengerRoom> MessengerRooms { get; set; }

        public LocalConnection(System.Net.Sockets.Socket pSocket)
            : base(pSocket)
        {

        }


        public void Init()
        {
            Pinger.Connections.Add(this);
            SendHandshake(40, "WvsBeta Server", 0xF8);
        }

        public override void OnDisconnect()
        {
            if (Server != null)
            {
                Program.MainForm.appendToLog(string.Format("Server disconnected: {0}", Server.Name));
                if (Server.Name.Contains("Game") && Server.IsReallyUsed)
                {
                    if (CenterServer.Instance.Worlds.ContainsKey(Server.WorldID))
                    {
                        CenterServer.Instance.Worlds[Server.WorldID].GameServers.Remove(Server.GameID);
                    }
                }
                if (Server.Name.Contains("Shop") && Server.IsReallyUsed)
                {
                    if (CenterServer.Instance.Worlds.ContainsKey(Server.WorldID))
                    {
                        CenterServer.Instance.Worlds[Server.WorldID].ShopServer = null;
                    }
                }
                Server.Connection = null;
                Server.Connections = 0;
                Server.Connected = false;
                Server.IsReallyUsed = false;
            }
            Pinger.Connections.Remove(this);
            Program.RefreshServerlist();
        }

        public override void AC_OnPacketInbound(Packet packet)
        {
            try
            {
                switch ((ISClientMessages)packet.ReadByte())
                {
                    case ISClientMessages.ServerRequestAllocation:
                        {
                            string serverName = packet.ReadString();
                            if (!CenterServer.Instance.LocalServers.ContainsKey(serverName))
                            {
                                Program.MainForm.appendToLog("Server doesn't exist in configuration: " + serverName + ". Disconnecting.");
                                this.Disconnect();
                            }
                            else if (CenterServer.Instance.LocalServers[serverName].Connected == true)
                            {
                                Program.MainForm.appendToLog(string.Format("Server is already connected: {0}. Disconnecting.", serverName));
                                this.Disconnect();
                            }
                            else
                            {
                                LocalServer ls = CenterServer.Instance.LocalServers[serverName];
                                ls.Connected = true;
                                Server = ls;
                                Server.Connection = this;
                                Server.Connections = 0;

                                Server.PublicIP = System.Net.IPAddress.Parse(packet.ReadString());
                                Server.Port = packet.ReadUShort();
                                Server.Name = serverName;
                                Program.MainForm.appendToLog(string.Format("Server connecting... Name: {0}, Public IP: {1}, Port {2}", serverName, Server.PublicIP.ToString(), Server.Port));

                                if (serverName.Contains("Game"))
                                {
                                    byte worldid = packet.ReadByte();
                                    if (!CenterServer.Instance.Worlds.ContainsKey(worldid))
                                    {
                                        Program.MainForm.appendToLog(string.Format("Gameserver disconnected because it didn't have a valid world ID ({0}) (world didn't exist... Did you forget to start the Login server?)", worldid));
                                        Disconnect();
                                    }
                                    else
                                    {
                                        WorldServer world = CenterServer.Instance.Worlds[worldid];
                                        byte id = world.getFreeGameServerSlot();
                                        Program.MainForm.appendToLog(string.Format("Current empty slot: {0}", id));
                                            
                                        if (id == 0xff)
                                        {
                                            Program.MainForm.appendToLog(string.Format("Gameserver disconnected because there are no slots free for this world: {0}...", worldid));
                                            Disconnect();
                                        }
                                        else
                                        {
                                            world.GameServers.Add(id, Server);
                                            Server.WorldID = worldid;
                                            Server.GameID = id;
                                            Server.Connected = true;
                                            Server.Type = LocalServerType.Game;
                                            Server.IsReallyUsed = true;
                                            Packet pw = new Packet(ISServerMessages.ServerAssignmentResult);
                                            pw.WriteByte(id);
                                            SendPacket(pw);
                                            Program.MainForm.appendToLog(string.Format("Gameserver assigned! ID {0} on World {1}", id, worldid));
                                        }
                                    }
                                }
                                else if (serverName.Contains("Login"))
                                {
                                    Program.MainForm.appendToLog("Login connected. Initializing worlds");
                                            
                                    WorldServer world;
                                    byte id;
                                    string name;
                                    short channels;
                                    byte worlds = packet.ReadByte();
                                    Program.MainForm.appendToLog("Worlds: " + worlds.ToString());
                                    Server.IsReallyUsed = true;
                                    for (byte i = 0; i < worlds; i++)
                                    {
                                        id = packet.ReadByte();
                                        name = packet.ReadString();
                                        channels = packet.ReadShort();
                                        if (CenterServer.Instance.Worlds.ContainsKey(id))
                                        {
                                            Program.MainForm.appendToLog(string.Format("Already got world {0}", id));
                                            continue;
                                        }

                                        Server.Type = LocalServerType.Login;

                                        world = new WorldServer(id);
                                        world.Name = name;
                                        world.Channels = channels;
                                        CenterServer.Instance.Worlds.Add(id, world);
                                        Program.MainForm.appendToLog(string.Format("World {0} ({1}) with {2} channels added.", id, name, channels));
                                    }
                                }
                                else if (serverName.Contains("Shop"))
                                {
                                    byte worldid = packet.ReadByte();
                                    if (!CenterServer.Instance.Worlds.ContainsKey(worldid))
                                    {
                                        Program.MainForm.appendToLog("Disconnected session (World doesn't exist)");
                                        Disconnect();
                                    }
                                    else if (CenterServer.Instance.Worlds[worldid].ShopServer != null)
                                    {
                                        Program.MainForm.appendToLog("Disconnected session (Shop already connected for world " + worldid + ")");
                                        Disconnect();
                                    }
                                    else
                                    {
                                        WorldServer world = CenterServer.Instance.Worlds[worldid];
                                        Server.WorldID = worldid;
                                        Server.Type = LocalServerType.Shop;
                                        world.ShopServer = Server;
                                        Server.Connected = true;
                                        Server.IsReallyUsed = true;

                                        Packet pw = new Packet(ISServerMessages.ServerAssignmentResult);
                                        SendPacket(pw);

                                        Program.MainForm.appendToLog(string.Format("Shopserver assigned for World {0}", worldid));
                                    }
                                }
                            }
                            Program.RefreshServerlist();
                            break;
                        }
                    case ISClientMessages.ServerSetConnectionsValue:
                        {
                            Server.Connections = packet.ReadInt();
                            Program.RefreshServerlist();
                            break;
                        }
                    case ISClientMessages.PlayerChangeServer:
                        {
                            string hash = packet.ReadString();
                            int charid = packet.ReadInt();
                            byte world = packet.ReadByte();
                            byte channel = packet.ReadByte();


                            Packet pw = new Packet(ISServerMessages.PlayerChangeServerResult);
                            pw.WriteString(hash);
                            pw.WriteInt(charid);

                            if (CenterServer.Instance.Worlds.ContainsKey(world))
                            {
                                if (channel != 50 && CenterServer.Instance.Worlds[world].GameServers.ContainsKey(channel))
                                {
                                    LocalServer ls = CenterServer.Instance.Worlds[world].GameServers[channel];
                                    pw.WriteBytes(ls.PublicIP.GetAddressBytes());
                                    pw.WriteUShort(ls.Port);

                                    Character chr = CenterServer.Instance.FindCharacter(charid);
                                    if (chr != null)
                                    {
                                        chr.isCCing = true;
                                    }
                                }
                                else if (channel == 50 && CenterServer.Instance.Worlds[world].ShopServer != null)
                                {
                                    LocalServer ls = CenterServer.Instance.Worlds[world].ShopServer;
                                    pw.WriteBytes(ls.PublicIP.GetAddressBytes());
                                    pw.WriteUShort(ls.Port);

                                    Character chr = CenterServer.Instance.FindCharacter(charid);
                                    if (chr != null)
                                    {
                                        chr.isCCing = true;
                                        chr.LastChannel = chr.ChannelID;
                                    }
                                }
                                else
                                {
                                    pw.WriteInt(0);
                                    pw.WriteShort(0);
                                }
                            }
                            else
                            {
                                pw.WriteInt(0);
                                pw.WriteShort(0);
                            }
                            SendPacket(pw);
                            break;
                        }
                    case ISClientMessages.PlayerRequestWorldLoad: // world load check... :D
                        {
                            string hash = packet.ReadString();
                            byte world = packet.ReadByte();

                            Packet pw = new Packet(ISServerMessages.PlayerRequestWorldLoadResult);
                            pw.WriteString(hash);

                            if (CenterServer.Instance.Worlds.ContainsKey(world))
                            {
                                CenterServer.Instance.Worlds[world].AddWarning(pw);
                            }
                            else
                            {
                                pw.WriteByte(2); // full load :D
                            }

                            SendPacket(pw);
                            break;
                        }
                    case ISClientMessages.PlayerRequestChannelStatus: // channel online check
                        {
                            string hash = packet.ReadString();
                            byte world = packet.ReadByte();
                            byte channel = packet.ReadByte();

                            Packet pw = new Packet(ISServerMessages.PlayerRequestChannelStatusResult);
                            pw.WriteString(hash);

                            if (CenterServer.Instance.Worlds.ContainsKey(world) && CenterServer.Instance.Worlds[world].GameServers.ContainsKey(channel))
                            {
                                pw.WriteByte(0);
                                pw.WriteByte(channel);
                            }
                            else
                            {
                                pw.WriteByte(0x09); // Channel Offline
                            }

                            SendPacket(pw);
                            break;
                        }
                    case ISClientMessages.PlayerRequestWorldList: // Worldlist
                        {
                            string hash = packet.ReadString();

                            Packet pw;
                            foreach (KeyValuePair<byte, WorldServer> bd in CenterServer.Instance.Worlds)
                            {
                                pw = new Packet(ISServerMessages.PlayerRequestWorldListResult);
                                pw.WriteString(hash);
                                pw.WriteByte(bd.Key);
                                pw.WriteString(bd.Value.Name);
                                pw.WriteByte((byte)bd.Value.Channels);
                                for (byte i = 0; i < (byte)bd.Value.Channels; i++)
                                {
                                    pw.WriteString(string.Format("{0}-{1}", bd.Value.Name, i + 1));
                                    if (bd.Value.GameServers.ContainsKey(i))
                                    {
                                        pw.WriteInt(bd.Value.GameServers[i].Connections);
                                    }
                                    else
                                    {
                                        pw.WriteInt(9001); // Max load.
                                    }

                                    pw.WriteByte(bd.Key);
                                    pw.WriteByte(i);
                                    pw.WriteByte(0x00);
                                }


                                SendPacket(pw);
                            }
                            pw = new Packet(ISServerMessages.PlayerRequestWorldListResult);
                            pw.WriteString(hash);
                            pw.WriteByte(0xFF);
                            SendPacket(pw);
                            break;
                        }
                    case ISClientMessages.PlayerWhisperOrFindOperation: // WhisperOrFind
                        {
                            int sender = packet.ReadInt();
                            Character senderChar = CenterServer.Instance.FindCharacter(sender);
                            if (senderChar == null)
                            {

                                Program.MainForm.appendToLog(string.Format("Character with ID {0} not found", sender));
                                return;
                            }

                            bool whisper = packet.ReadBool();
                            string receiver = packet.ReadString();
                            Character receiverChar = CenterServer.Instance.FindCharacter(receiver, (byte)this.Server.WorldID);
                            if (whisper)
                            {
                                string message = packet.ReadString();
                                if (receiverChar == null || !CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers.ContainsKey(receiverChar.ChannelID))
                                {
                                    Packet pw = new Packet(ISServerMessages.PlayerWhisperOrFindOperationResult);
                                    pw.WriteBool(true); // Whisper
                                    pw.WriteBool(false); // Not found.
                                    pw.WriteInt(sender);
                                    pw.WriteString(receiver);
                                    SendPacket(pw);
                                    Program.MainForm.appendToLog(string.Format("Whispering! Player '{0}' not found, sending to id {1}", receiver, sender));
                                }
                                else
                                {
                                    Packet pw = new Packet(ISServerMessages.PlayerWhisperOrFindOperationResult);
                                    pw.WriteBool(false); // Find
                                    pw.WriteBool(true); // Found.
                                    pw.WriteInt(sender);
                                    pw.WriteString(receiver);
                                    pw.WriteSByte(-1);
                                    pw.WriteSByte(-1);
                                    SendPacket(pw);
                                    Program.MainForm.appendToLog(string.Format("Finding! Character '{0}' found, sending player with ID {1} that the victim is on channel {2}.", receiver, sender, senderChar.ChannelID));

                                    pw = new Packet(ISServerMessages.PlayerWhisperOrFindOperationResult);
                                    pw.WriteBool(true); // Whisper
                                    pw.WriteBool(true); // Found.
                                    pw.WriteInt(receiverChar.ID);
                                    pw.WriteString(senderChar.Name);
                                    pw.WriteByte(senderChar.ChannelID);
                                    pw.WriteString(message);
                                    LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[receiverChar.ChannelID];
                                    victimChannel.Connection.SendPacket(pw);
                                    Program.MainForm.appendToLog(string.Format("Whispering! Player '{0}' found! Message from {1} Sending to id {2} on channel {3} and line: {4}", receiver, sender, receiverChar.ID, receiverChar.ChannelID, message));
                                }
                            }
                            else
                            {
                                if (receiverChar == null || !CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers.ContainsKey(receiverChar.ChannelID))
                                {
                                    Packet pw = new Packet(ISServerMessages.PlayerWhisperOrFindOperationResult);
                                    pw.WriteBool(false); // Find
                                    pw.WriteBool(false); // Not found.
                                    pw.WriteInt(sender);
                                    pw.WriteString(receiver);
                                    SendPacket(pw);
                                    Program.MainForm.appendToLog(string.Format("Finding! Character '{0}' is not found, sending it player with ID {1}", receiver, sender));
                                }
                                else
                                {
                                    Packet pw = new Packet(ISServerMessages.PlayerWhisperOrFindOperationResult);
                                    pw.WriteBool(false); // Find
                                    pw.WriteBool(true); // Found.
                                    pw.WriteInt(senderChar.ID);
                                    pw.WriteString(receiverChar.Name);
                                    pw.WriteByte(receiverChar.ChannelID);
                                    pw.WriteSByte(0);
                                    SendPacket(pw);
                                    Program.MainForm.appendToLog(string.Format("Finding! Character '{0}' found, sending player with ID {1} that the victim is on channel {2}.", receiver, sender, receiverChar.ChannelID));
                                }

                            }
                            break;
                        }



                    case ISClientMessages.ServerRegisterUnregisterPlayer: // Register/unregister character
                        {

                            int charid = packet.ReadInt();
                            bool add = packet.ReadBool();
                            if (add)
                            {
                                string charname = packet.ReadString();
                                short job = packet.ReadShort();
                                byte level = packet.ReadByte();
                                CenterServer.Instance.AddCharacter(charname, charid, (byte)Server.WorldID, (byte)Server.GameID, job, level);
                            }
                            else
                            {
                                CenterServer.Instance.mOnlineCharacters.Remove(CenterServer.Instance.FindCharacter(charid));
                            }

                            break;
                        }

                    /**
                case ISClientMessages.MessengerJoin:
                {
                    int messengerid = packet.ReadInt();
                    int joiningcid = packet.ReadInt();
                    string name = packet.ReadString();
                    byte gender = packet.ReadByte();
                    byte skin = packet.ReadByte();
                    int face = packet.ReadInt();
                    int hair = packet.ReadInt();
                    Dictionary<byte, int> equips = new Dictionary<byte, int>();
                    while (true)
                    {
                        byte slot = packet.ReadByte();
                        if (slot == 0xFF)
                            break;
                        int itemid = packet.ReadInt();
                        equips.Add(slot, itemid);
                    }
                    Character chr = CenterServer.Instance.GetCharacterByCID(joiningcid);
                    chr.Gender = gender;
                    chr.Skin = skin;
                    chr.Face = face;
                    chr.Hair = hair;
                    chr.Equips = equips;
                    if (messengerid == 0) //occours when opening window without invite
                    {
                        MessengerRoom newroom = new MessengerRoom(FirstAvailableMessengerRoom());
                        newroom.Enter(chr.ID, chr, (byte)newroom.FirstAvailableSlot());//don't send here, the client only has one slot right now 
                        //CenterServer.Instance.MessengerRooms.Add(newroom);
                        break;
                    }
                    MessengerRoom room = CenterServer.Instance.GetMessengerRoomByRoomID(messengerid);
                    if (room.GetNumberOfPlayers() == 0)
                        break;
                    byte messengerslot = room.FirstAvailableSlot();
                    sendPacket(room.Enter(chr.ID, chr, messengerslot));
                    if (room.roomSlots[0] != null)
                    {
                        if (room.roomSlots[1] != null)
                            sendPacket(room.SelfEnterResult(room.roomSlots[0].ID, 1, room.roomSlots[1]));
                        if (room.roomSlots[2] != null)
                            sendPacket(room.SelfEnterResult(room.roomSlots[0].ID, 2, room.roomSlots[2]));
                    }
                    if (room.roomSlots[1] != null)
                    {
                        if (room.roomSlots[0] != null)
                            sendPacket(room.SelfEnterResult(room.roomSlots[1].ID, 0, room.roomSlots[0]));
                        if (room.roomSlots[2] != null)
                            sendPacket(room.SelfEnterResult(room.roomSlots[1].ID, 2, room.roomSlots[2]));
                    }
                    if (room.roomSlots[2] != null)
                    {
                        if (room.roomSlots[0] != null)
                            sendPacket(room.SelfEnterResult(room.roomSlots[2].ID, 0, room.roomSlots[0]));
                        if (room.roomSlots[1] != null)
                            sendPacket(room.SelfEnterResult(room.roomSlots[2].ID, 1, room.roomSlots[1]));
                    }

                    /*foreach (Character ch in chars.Values)
                        if (ch != null)
                            sendPacket(room.SelfEnterResult(ch.ID, messengerslot, chr));
                    break;
                }
                case ISClientMessages.MessengerLeave:
                {
                    int cid = packet.ReadInt();
                    MessengerRoom room = CenterServer.Instance.GetMessengerRoomByCID(cid);
                    if (room == null)
                        break;
                    byte slot = room.GetPosition(cid);
                    if (room.roomSlots[0] != null)
                    {
                        sendPacket(room.Leave(room.roomSlots[0].ID, slot));
                    }
                    if (room.roomSlots[1] != null)
                    {
                        sendPacket(room.Leave(room.roomSlots[1].ID, slot));
                    }
                    if (room.roomSlots[2] != null)
                    {
                        sendPacket(room.Leave(room.roomSlots[2].ID, slot));
                    }
                    if (room.GetNumberOfPlayers() == 0)
                        CenterServer.Instance.MessengerRooms.Remove(room);
                    break;
                }
                case ISClientMessages.MessengerInvite:
                {
                    int sendercid = packet.ReadInt();
                    string cinvitee = packet.ReadString();
                    Character sender = CenterServer.Instance.GetCharacterByCID(sendercid);
                    MessengerRoom room = CenterServer.Instance.GetMessengerRoomByCID(sendercid);
                    Character deliver = CenterServer.Instance.GetCharacterByName(cinvitee);
                    if (room == null || sender == null)
                        break;
                    if (deliver != null)
                        sendPacket(room.Invite(deliver.ID, sender));
                    foreach (Character ch in room.roomSlots.Values)
                        if (ch != null)
                            sendPacket(room.InviteResult(ch.ID, cinvitee, deliver == null ? (byte)0 : (byte)1));
                    break;
                }
                case ISClientMessages.MessengerBlocked:
                {
                    int cid = packet.ReadInt();
                    string invitee = packet.ReadString();
                    string inviter = packet.ReadString();
                    byte blockmode = packet.ReadByte();
                    MessengerRoom room = CenterServer.Instance.GetMessengerRoomByName(inviter);
                    if (room == null)
                        break;
                    foreach (Character ch in room.roomSlots.Values)
                        if (ch != null)
                            sendPacket(room.Blocked(ch.ID, invitee, blockmode));
                    break;
                }
                case ISClientMessages.MessengerChat:
                {
                    int cid = packet.ReadInt();
                    MessengerRoom room = CenterServer.Instance.GetMessengerRoomByCID(cid);
                    if (room == null)
                        break;
                    string message = packet.ReadString();
                    foreach (Character ch in room.roomSlots.Values)
                        if (ch != null && ch.ID != cid)
                            sendPacket(room.Chat(ch.ID, message, cid));
                    break;
                }
                     * **/

                    case ISClientMessages.PlayerUsingSuperMegaphone:
                        {
                            Packet pw = new Packet(ISServerMessages.PlayerSuperMegaphone);
                            pw.WriteString(packet.ReadString());
                            pw.WriteBool(packet.ReadBool());
                            pw.WriteByte(packet.ReadByte());
                            CenterServer.Instance.Worlds[Server.WorldID].SendPacketToEveryGameserver(pw);
                            break;
                        }
                    case ISClientMessages.PlayerQuitCashShop: // CC back to channel from cashserver
                        {
                            string hash = packet.ReadString();
                            int charid = packet.ReadInt();
                            byte world = packet.ReadByte();
                            Character chr = CenterServer.Instance.FindCharacter(charid);
                            if (chr == null) return;

                            Packet pw = new Packet(ISServerMessages.PlayerChangeServerResult);
                            pw.WriteString(hash);
                            pw.WriteInt(charid);

                            if (CenterServer.Instance.Worlds.ContainsKey(world) && CenterServer.Instance.Worlds[world].GameServers.ContainsKey(chr.LastChannel))
                            {
                                LocalServer ls = CenterServer.Instance.Worlds[world].GameServers[chr.LastChannel];
                                pw.WriteBytes(ls.PublicIP.GetAddressBytes());
                                pw.WriteUShort(ls.Port);

                                if (chr != null)
                                {
                                    chr.isCCing = true;
                                    chr.LastChannel = 0;
                                }
                            }
                            else
                            {
                                pw.WriteInt(0);
                                pw.WriteShort(0);
                            }
                            SendPacket(pw);

                            break;
                        }
                }
                packet = null;
            }
            catch (Exception ex)
            {
                Program.LogFile.WriteLine("Exception Caught:\r\n{0}", ex.ToString());
                //FileWriter.WriteLine(@"etclog\ExceptionCatcher.log", "[Center Server][" + DateTime.Now.ToString() + "] Exception caught: " + ex.Message + Environment.NewLine + Environment.NewLine + "Stacktrace: " + ex.StackTrace, true);
                //Disconnect();
                Disconnect();
            }
        }
    }
}
