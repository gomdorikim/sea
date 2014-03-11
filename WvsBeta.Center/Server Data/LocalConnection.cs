using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Center
{
    public class LocalConnection : AbstractConnection
    {
        public LocalServer Server { get; set; }
        public static List<Messenger> MessengerRooms { get; set; }

        public LocalConnection(System.Net.Sockets.Socket pSocket)
            : base(pSocket)
        {

        }


        public void Init()
        {
            //Pinger.Connections.Add(this);
            SendHandshake(40, "WvsBeta Server", 0xF8);
        }

        public void SendRates()
        {
            Packet packet = new Packet(ISServerMessages.ChangeRates);
            packet.WriteDouble(Server.RateMobEXP);
            packet.WriteDouble(Server.RateMesoAmount);
            packet.WriteDouble(Server.RateDropChance);
            SendPacket(packet);
        }


        public override void OnDisconnect()
        {
            if (Server != null)
            {
                Console.WriteLine(string.Format("Server disconnected: {0}", Server.Name));
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
                Server.GameID = 0;
                Server.WorldID = 0;
                Server.Connected = false;
                Server.IsReallyUsed = false;
            }
            Pinger.Connections.Remove(this);
        }

        public override void AC_OnPacketInbound(Packet packet)
        {
            try
            {
                if (Server == null)
                {
                    switch ((ISClientMessages)packet.ReadByte())
                    {
                        case ISClientMessages.ServerRequestAllocation:
                            {
                                string serverName = packet.ReadString();
                                if (!CenterServer.Instance.LocalServers.ContainsKey(serverName))
                                {
                                    Console.WriteLine("Server doesn't exist in configuration: " + serverName + ". Disconnecting.");
                                    this.Disconnect();
                                }
                                else if (CenterServer.Instance.LocalServers[serverName].Connected == true)
                                {
                                    Console.WriteLine(string.Format("Server is already connected: {0}. Disconnecting.", serverName));
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
                                   
                                    Console.WriteLine(string.Format("Server connecting... Name: {0}, Public IP: {1}, Port {2}", serverName, Server.PublicIP.ToString(), Server.Port));

                                    if (serverName.Contains("Game"))
                                    {
                                        byte worldid = packet.ReadByte();
                                        if (!CenterServer.Instance.Worlds.ContainsKey(worldid))
                                        {
                                            Console.WriteLine(string.Format("Gameserver disconnected because it didn't have a valid world ID ({0}) (world didn't exist... Did you forget to start the Login server?)", worldid));
                                            Disconnect();
                                        }
                                        else
                                        {
                                            WorldServer world = CenterServer.Instance.Worlds[worldid];
                                            byte id = world.getFreeGameServerSlot();
                                            Console.WriteLine(string.Format("Current empty slot: {0}", id));

                                            if (id == 0xff)
                                            {
                                                Console.WriteLine(string.Format("Gameserver disconnected because there are no slots free for this world: {0}...", worldid));
                                                Disconnect();
                                            }
                                            else
                                            {
                                                
                                                //Console.WriteLine(Server.AdultWorld.ToString());
                                                world.GameServers.Add(id, Server);
                                                Server.WorldID = worldid;
                                                Server.GameID = id;
                                                Server.Connected = true;
                                                Server.Type = LocalServerType.Game;
                                                Server.IsReallyUsed = true;
                                                Server.AdultWorld = packet.ReadBool();
                                                Server.MasterIP = packet.ReadString();
                                                Packet pw = new Packet(ISServerMessages.ServerAssignmentResult);
                                                pw.WriteByte(id);
                                                SendPacket(pw);
                                                //SendRates();
                                                Console.WriteLine(string.Format("Gameserver assigned! Name {0}; Channel ID {1}; World {2}", serverName, id, worldid));
                                            }
                                        }
                                    }
                                    else if (serverName.Contains("Login"))
                                    {
                                        Console.WriteLine("Login connected. Initializing worlds");

                                        WorldServer world;
                                        byte id;
                                        string name;
                                        short channels;
                                        byte worlds = packet.ReadByte();
                                        string eventDescr;
                                       
                                        Console.WriteLine("Worlds: " + worlds.ToString());
                                        Server.IsReallyUsed = true;
                                        for (byte i = 0; i < worlds; i++)
                                        {
                                            id = packet.ReadByte();
                                            name = packet.ReadString();
                                            channels = packet.ReadShort();
                                            eventDescr = packet.ReadString();
                                            
                                            if (CenterServer.Instance.Worlds.ContainsKey(id))
                                            {
                                                Console.WriteLine(string.Format("Already got world {0}", id));
                                                continue;
                                            }

                                            Server.Type = LocalServerType.Login;

                                            world = new WorldServer(id);
                                            world.Name = name;
                                            world.Channels = channels;
                                            world.EventDescription = eventDescr;
                                            
                                            CenterServer.Instance.Worlds.Add(id, world);
                                            Console.WriteLine(string.Format("World {0} ({1}) with {2} channels added.", id, name, channels));
                                        }
                                    }
                                    else if (serverName.Contains("Shop"))
                                    {
                                        byte worldid = packet.ReadByte();
                                        if (!CenterServer.Instance.Worlds.ContainsKey(worldid))
                                        {
                                            Console.WriteLine("Disconnected session (World doesn't exist)");
                                            Disconnect();
                                        }
                                        else if (CenterServer.Instance.Worlds[worldid].ShopServer != null)
                                        {
                                            Console.WriteLine("Disconnected session (Shop already connected for world " + worldid + ")");
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

                                            Console.WriteLine(string.Format("Shopserver assigned for World {0}", worldid));
                                        }
                                    }
                                }
                                break;
                            }
                    }
                }
                else
                {
                    switch ((ISClientMessages)packet.ReadByte())
                    {
                        case ISClientMessages.ChangeRates:
                            {
                                Server.RateMobEXP = packet.ReadDouble();
                                Server.RateMesoAmount = packet.ReadDouble();
                                Server.RateDropChance = packet.ReadDouble();
                                break;
                            }
                        case ISClientMessages.ServerSetConnectionsValue:
                            {
                                Server.Connections = packet.ReadInt();
                                break;
                            }
                        case ISClientMessages.PlayerChangeServer:
                            {
                                
                                string hash = packet.ReadString();
                                int charid = packet.ReadInt();
                                byte world = packet.ReadByte();
                                byte channel = packet.ReadByte();
                                bool CCing = packet.ReadBool();

                                Console.WriteLine(hash);
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
                                        //.Connection.SendPacket(pw);
                                        //Console.WriteLine(ls.PublicIP.ToString() + ls.Port);

                                        Character chr = CenterServer.Instance.FindCharacter(charid);
                                        if (chr != null)
                                        {
                                            chr.isCCing = true;
                                            if (CCing)
                                            {
                                                Console.WriteLine("heh");
                                                chr.isConnectingFromLogin = false;
                                            }
                                           
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
                                //0x11 : Asiasoft account is suspended
                                //0x0B : Not over 21
                                //0x0D : Unable to log on as master
                                //0x14 : Wrong gateway
                                string hash = packet.ReadString();
                                byte world = packet.ReadByte();
                                byte channel = packet.ReadByte();
                                bool isAdmin = packet.ReadBool();
                                string ip = packet.ReadString();
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
                                    pw.WriteByte(2); //No idea

                                    pw.WriteString(bd.Value.EventDescription);

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
                                        pw.WriteByte(0); 
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

                                    Console.WriteLine(string.Format("Character with ID {0} not found", sender));
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
                                        Console.WriteLine(string.Format("Whispering! Player '{0}' not found, sending to id {1}", receiver, sender));
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
                                        Console.WriteLine(string.Format("Finding! Character '{0}' found, sending player with ID {1} that the victim is on channel {2}.", receiver, sender, senderChar.ChannelID));

                                        pw = new Packet(ISServerMessages.PlayerWhisperOrFindOperationResult);
                                        pw.WriteBool(true); // Whisper
                                        pw.WriteBool(true); // Found.
                                        pw.WriteInt(receiverChar.ID);
                                        pw.WriteString(senderChar.Name);
                                        pw.WriteByte(senderChar.ChannelID);
                                        pw.WriteString(message);
                                        LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[receiverChar.ChannelID];
                                        victimChannel.Connection.SendPacket(pw);
                                        Console.WriteLine(string.Format("Whispering! Player '{0}' found! Message from {1} Sending to id {2} on channel {3} and line: {4}", receiver, sender, receiverChar.ID, receiverChar.ChannelID, message));
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
                                        Console.WriteLine(string.Format("Finding! Character '{0}' is not found, sending it player with ID {1}", receiver, sender));
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
                                        Console.WriteLine(string.Format("Finding! Character '{0}' found, sending player with ID {1} that the victim is on channel {2}.", receiver, sender, receiverChar.ChannelID));
                                    }

                                }
                                break;
                            }

                        case ISClientMessages.PartyOperation:
                            {
                                switch (packet.ReadByte())
                                {
                                    case 1: //CREATE PARTY 
                                        {
                                            int pID = packet.ReadInt();
                                            Character pLeader = CenterServer.Instance.FindCharacter(pID);
                                            if (pLeader != null)
                                            {
                                                MapleParty MSP = new MapleParty(pLeader);
                                                pLeader.Party = MSP;
                                                pLeader.PartyID = MSP.ID;
                                                pLeader.Leader = true;

                                                Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                pw.WriteByte(1);
                                                pw.WriteInt(pLeader.ID);
                                                pw.WriteInt(pLeader.PartyID);
                                                SendPacket(pw);
                                            }
                                            break;
                                        }
                                    case 2: //LEAVE
                                        {
                                            int cID = packet.ReadInt();
                                            int pID = packet.ReadInt();
                                            Character lCharacter = CenterServer.Instance.FindCharacter(cID);

                                            foreach (KeyValuePair<int, MapleParty> p in MapleParty.LocalParties)
                                            {
                                                Console.WriteLine("PartyID = " + p.Value.ID.ToString());
                                            }

                                            try
                                            {
                                                
                                                MapleParty lParty = MapleParty.LocalParties[pID];
                                                Console.WriteLine(lCharacter.Name + " " + lParty.Leader.Name);
                                                if (lCharacter != null && lParty != null)
                                                {
                                                    Console.WriteLine(lParty.Leader.Name.ToString());
                                                    if (lParty.Leader == lCharacter) //Disband
                                                    {
                                                        Console.WriteLine("Yes, you are leader");
                                                        lParty.Members.Remove(lCharacter);
                                                        lCharacter.Party = null;

                                                        foreach (Character pMember in lParty.Members)
                                                        {
                                                            pMember.Party = null;
                                                            if (pMember != lCharacter)
                                                            {
                                                                if (CenterServer.Instance.IsOnline(pMember))
                                                                {
                                                                    LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pMember.ChannelID];
                                                                    Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                                    pw.WriteByte(2);
                                                                    pw.WriteInt(pMember.ID);
                                                                    pw.WriteBool(false);
                                                                    pw.WriteBytes(PartyPacket.Disbanded(pMember, lParty));
                                                                    victimChannel.Connection.SendPacket(pw);
                                                                }
                                                                else
                                                                {
                                                                    CenterServer.Instance.CharacterDatabase.RunQuery("UPDATE characters SET party = -1 WHERE ID = " + pMember.ID + "");
                                                                }

                                                            }

                                                        }

                                                        Packet pPacket = new Packet(ISServerMessages.PartyOperation);
                                                        pPacket.WriteByte(2);
                                                        pPacket.WriteInt(lCharacter.ID);
                                                        pPacket.WriteBool(true);
                                                        pPacket.WriteBytes(PartyPacket.DisbandParty(lCharacter, lCharacter.PartyID));
                                                        SendPacket(pPacket);

                                                        lParty = null;
                                                    }
                                                    else //Leaving
                                                    {
                                                        lParty.Members.Remove(lCharacter);
                                                        lCharacter.Party = null;

                                                        Packet pPacket = new Packet(ISServerMessages.PartyOperation);
                                                        pPacket.WriteByte(2);
                                                        pPacket.WriteInt(lCharacter.ID);
                                                        pPacket.WriteBool(true);
                                                        pPacket.WriteBytes(PartyPacket.LeaveParty(lCharacter, lParty));
                                                        SendPacket(pPacket);

                                                        foreach (Character pMember in lParty.Members)
                                                        {
                                                            LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pMember.ChannelID];
                                                            Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                            pw.WriteByte(2);
                                                            pw.WriteInt(pMember.ID);
                                                            pw.WriteBool(false);
                                                            pw.WriteBytes(PartyPacket.Update(false, lCharacter, lParty, 1, 0, lCharacter.Name, lCharacter.ID));
                                                            victimChannel.Connection.SendPacket(pw);
                                                            Console.WriteLine("Member leaving... sending update to : " + pMember.ChannelID.ToString());
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine(DateTime.Now + " Player " + cID + " tried to leave a party without the correct ID! Prevented GameServer Crash." + ex.ToString());
                                            }
                                            break;
                                        }
                                    case 3: //JOIN
                                        {
                                            int jID = packet.ReadInt();
                                            int pID = packet.ReadInt();

                                            Console.WriteLine(pID.ToString());
                                            Character pJoiner = CenterServer.Instance.FindCharacter(jID);
                                            if (pJoiner != null)
                                            {
                                                try
                                                {
                                                    MapleParty jParty = MapleParty.LocalParties[pID];

                                                    if (pJoiner.Party != null)
                                                    {
                                                        Console.WriteLine("PARTY IS NOT NULL");
                                                        Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                        pw.WriteByte(9);
                                                        pw.WriteInt(pJoiner.ID);
                                                        pw.WriteBytes(PartyPacket.PartyResult(pJoiner, 8));
                                                        SendPacket(pw);
                                                    }
                                                    else if (jParty.Members.Count == 6)
                                                    {
                                                        Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                        pw.WriteByte(9);
                                                        pw.WriteInt(pJoiner.ID);
                                                        pw.WriteBytes(PartyPacket.PartyResult(pJoiner, 10));
                                                        SendPacket(pw);
                                                    }
                                                    else
                                                    {

                                                        if (jParty != null)
                                                        {
                                                            if (!jParty.Members.Contains(pJoiner))
                                                            {
                                                                jParty.AddMember(pJoiner);


                                                                //Update Party Status
                                                                foreach (Character pMember in jParty.Members)
                                                                {

                                                                    if (pMember != null)
                                                                    {
                                                                        LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pMember.ChannelID];

                                                                        Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                                        pw.WriteByte(3);
                                                                        pw.WriteInt(pMember.ID);
                                                                        pw.WriteInt(pID);
                                                                        pw.WriteBytes(PartyPacket.JoinParty(pJoiner, jParty));
                                                                        victimChannel.Connection.SendPacket(pw);
                                                                        Console.WriteLine(pMember.Name + pMember.ChannelID);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                      
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine(ex.ToString());
                                                }
                                            }
                                            break;
                                        }
                                    case 5: //EXPELL
                                        {
                                            int eID = packet.ReadInt();
                                            int pID = packet.ReadInt();
                                            string eName = CenterServer.Instance.CharacterDatabase.getCharacterNameByID(eID);
                                            Character eCharacter = CenterServer.Instance.FindCharacter(eID);

                                            try
                                            {
                                                MapleParty eParty = MapleParty.LocalParties[pID];
                                                if (eParty != null)
                                                {
                                                    if (CenterServer.Instance.IsOnline(eCharacter))
                                                    {
                                                        eParty.Members.Remove(eCharacter);
                                                        eCharacter.Party = null;
                                                       
                                                        foreach (Character pMember in eParty.Members)
                                                        {
                                                            if (pMember != null)
                                                            {
                                                                LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pMember.ChannelID];
                                                                Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                                pw.WriteByte(5);
                                                                pw.WriteInt(pMember.ID);
                                                                pw.WriteBool(false);
                                                                pw.WriteBytes(PartyPacket.Update(true, pMember, eParty, 1, 1, eName, eID));
                                                                victimChannel.Connection.SendPacket(pw);
                                                            }
                                                        }

                                                        LocalServer eVictimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[eCharacter.ChannelID];
                                                        Packet pPacket = new Packet(ISServerMessages.PartyOperation);
                                                        pPacket.WriteByte(5);
                                                        pPacket.WriteInt(eCharacter.ID);
                                                        pPacket.WriteBool(true);
                                                        pPacket.WriteBytes(PartyPacket.Expelled(eCharacter, eParty));
                                                        eVictimChannel.Connection.SendPacket(pPacket);
                                                    }
                                                    else
                                                    {
                                                        foreach (Character pMember in eParty.Members.ToList())
                                                        {
                                                            if (pMember.ID == eID)
                                                            {
                                                                eParty.Members.Remove(pMember);
                                                                CenterServer.Instance.CharacterDatabase.RunQuery("UPDATE characters SET party = -1 WHERE ID = " + pMember.ID + "");

                                                            }
                                                            else
                                                            {
                                                                LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pMember.ChannelID];
                                                                Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                                pw.WriteByte(5);
                                                                pw.WriteInt(pMember.ID);
                                                                pw.WriteBool(false);
                                                                pw.WriteBytes(PartyPacket.Update(false, pMember, eParty, 1, 1, eName, eID));
                                                                victimChannel.Connection.SendPacket(pw);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine(ex.ToString());
                                            }
                                            
                                            break;
                                        }
                                    case 6:
                                        {
                                            int cID = packet.ReadInt();
                                            int pID = packet.ReadInt();
                                            string pMessage = packet.ReadString();

                                            int OnlineCount = 0;
                                            Character pCharacter = CenterServer.Instance.FindCharacter(cID);
                                            if (pCharacter != null)
                                            {
                                                try
                                                {
                                                    MapleParty cParty = MapleParty.LocalParties[pID];

                                                    if (cParty != null)
                                                    {
                                                        foreach (Character pMember in cParty.Members)
                                                        {
                                                            if (CenterServer.Instance.IsOnline(pMember))
                                                            {
                                                                OnlineCount++;
                                                            }
                                                        }
                                                        if (OnlineCount > 1)
                                                        {
                                                            foreach (Character Member in cParty.Members)
                                                            {
                                                                if (Member != null && Member != pCharacter)
                                                                {
                                                                    LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[Member.ChannelID];
                                                                    Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                                    pw.WriteByte(6);
                                                                    pw.WriteInt(Member.ID);
                                                                    pw.WriteBytes(GroupMessagePacket.GroupMessage(pCharacter, pMessage, 1));
                                                                    victimChannel.Connection.SendPacket(pw);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                            pw.WriteByte(6);
                                                            pw.WriteInt(pCharacter.ID);
                                                            pw.WriteBytes(GroupMessagePacket.NoneOnline(pCharacter));
                                                            SendPacket(pw);
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine("Exception caught during Party Chat! Prevented Game Server crash. " + ex.ToString());
                                                }
                                            }
                                            break;
                                        }
                                    case 7:
                                        {
                                            Console.WriteLine("RECEIVED CASE 7");
                                            int cID = packet.ReadInt();
                                            int pID = packet.ReadInt();
                                            int MapID = packet.ReadInt();
                                            Character cCharacter = CenterServer.Instance.FindCharacter(cID);
                                            //cCharacter.ChannelID = packet.ReadByte();
                                            if (cCharacter != null)
                                            {
                                                cCharacter.Party = MapleParty.LocalParties[pID];

                                                foreach (Character Member in cCharacter.Party.Members)
                                                {
                                                    if (Member.ID == cCharacter.ID)
                                                    {
                                                        Member.MapID = MapID; 
                                                    }
                                                }
                                                foreach (Character pMember in cCharacter.Party.Members)
                                                {
                                                    if (pMember.ID != cCharacter.ID) //
                                                    {
                                                        LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pMember.ChannelID];
                                                        Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                        pw.WriteByte(7); //CC Update
                                                        pw.WriteInt(pMember.ID);
                                                        pw.WriteBytes(PartyPacket.SilentUpdate(pMember, cCharacter.Party));
                                                        victimChannel.Connection.SendPacket(pw);
                                                        Console.WriteLine("case 1... channel : " + pMember.ChannelID);

                                                    }
                                                    else
                                                    {
                                                        Packet pw2 = new Packet(ISServerMessages.PartyOperation);
                                                        pw2.WriteByte(7); //CC Update
                                                        pw2.WriteInt(pMember.ID);
                                                        pw2.WriteBytes(PartyPacket.SilentUpdate(pMember, cCharacter.Party));
                                                        SendPacket(pw2);
                                                        Console.WriteLine("case 2... channel : " + cCharacter.ChannelID.ToString());
                                                    }
                                                    Console.WriteLine("PARTY INFO!!" + pMember.Name.ToString() + " " + pMember.ChannelID.ToString());
                                                }
                                            }
                                            break;
                                        }
                                    case 8:
                                        {
                                            bool logoff = packet.ReadBool();
                                            int gID = packet.ReadInt();
                                            int pID = packet.ReadInt();  
                                            MapleParty gParty = MapleParty.LocalParties[pID];
                                            if (!logoff)
                                            {
                                                Character pMember = CenterServer.Instance.FindCharacter(gID);
                                                for (int i = 0; i < gParty.Members.Count; i++)
                                                {
                                                    if (gParty.Members[i].ID == gID)
                                                    {
                                                        gParty.Members[i] = pMember;
                                                    }
                                                }
                                                
                                            }
                                            break;
                                        }
                                    case 0x15:
                                        {
                                            string leaderName = packet.ReadString();
                                            string pInviteename = packet.ReadString();
                                            Character pLeader = CenterServer.Instance.FindCharacter(CenterServer.Instance.CharacterDatabase.AccountIdByName(leaderName));
                                            if (CenterServer.Instance.IsOnline(pLeader))
                                            {

                                                LocalServer eVictimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pLeader.ChannelID];
                                                Packet pPacket = new Packet(ISServerMessages.PartyOperation);
                                                pPacket.WriteByte(0x15);
                                                pPacket.WriteInt(pLeader.ID);
                                                pPacket.WriteBytes(PartyPacket.CharacterPartyMessage(pLeader, pInviteename, 0x15));
                                                eVictimChannel.Connection.SendPacket(pPacket);
                                            }

                                            break;
                                        }
                                }
                                break;
                            }
                        
                        case ISClientMessages.PartyDisconnect:
                            {                    
                                    int id = packet.ReadInt();
                                    int PartyID = packet.ReadInt();
                                    Character disconnecter = CenterServer.Instance.GetCharacterByCID(id);

                                    MapleParty party = MapleParty.LocalParties[PartyID];
                                    if (!disconnecter.isCCing) //just making sure you're not switching channels lol
                                    {
                                        foreach (Character pMember in party.Members)
                                        {
                                            if (pMember.ID == disconnecter.ID)
                                            {
                                                pMember.MapID = -1;
                                            }
                                        }

                                        foreach (Character Member in party.Members)
                                        {
                                            if (CenterServer.Instance.IsOnline(Member))
                                            {
                                                LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[Member.ChannelID];
                                                Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                pw.WriteByte(7); //CC Update
                                                pw.WriteInt(Member.ID);
                                                pw.WriteBytes(PartyPacket.SilentUpdate(Member, party));
                                                victimChannel.Connection.SendPacket(pw);
                                                Console.WriteLine("case 1... channel : " + Member.ChannelID);
                                            }
                                        }
                                    }
                                    Console.WriteLine("received disconnection packet... updating party info");
                                    break;
                            }
                        case ISClientMessages.PlayerBuffUpdate:
                            {
                                int CID = packet.ReadInt();
                                Character pBuffer = CenterServer.Instance.GetCharacterByCID(CID);
                                if (pBuffer != null && CenterServer.Instance.IsOnline(pBuffer))
                                {

                                }
                                break;
                            }
                       
                        case ISClientMessages.PlayerUpdateMap: //For Parties :/
                            {
                                int pID = packet.ReadInt();
                                int mID = packet.ReadInt();
                                int PartyID = packet.ReadInt();
                                Character pCharacter = CenterServer.Instance.FindCharacter(pID);
                                if (pCharacter != null)
                                {
                                    pCharacter.MapID = mID;
                                    try
                                    {
                                        Console.WriteLine(pCharacter.MapID.ToString());

                                        if (PartyID != -1)
                                        {
                                            pCharacter.Party = MapleParty.LocalParties[PartyID];
                                            Console.WriteLine(pCharacter.Party.Members.Count.ToString());
                                            foreach (Character pMember in pCharacter.Party.Members)
                                            {
                                                if (pMember.ID != pCharacter.ID) //
                                                {
                                                    LocalServer victimChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pMember.ChannelID];
                                                    Packet pw = new Packet(ISServerMessages.PartyOperation);
                                                    pw.WriteByte(7); //CC Update
                                                    pw.WriteInt(pMember.ID);
                                                    pw.WriteBytes(PartyPacket.SilentUpdate(pMember, pCharacter.Party));
                                                    victimChannel.Connection.SendPacket(pw);

                                                }
                                                else
                                                {
                                                    Packet pw2 = new Packet(ISServerMessages.PartyOperation);
                                                    pw2.WriteByte(7); //CC Update
                                                    pw2.WriteInt(pMember.ID);
                                                    pw2.WriteBytes(PartyPacket.SilentUpdate(pMember, pCharacter.Party));
                                                    SendPacket(pw2);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                }
                                break;
                            }
                        
                        case ISClientMessages.BuddyInvite:
                            {
                                string pReceiverName = packet.ReadString();
                                string sendername = packet.ReadString();
                                int senderId = packet.ReadInt();
                                Character pSender = CenterServer.Instance.FindCharacter(sendername, (byte)this.Server.WorldID);
                                Character pReceiver = CenterServer.Instance.FindCharacter(pReceiverName, (byte)this.Server.WorldID);
                               
                                break;
                            }

                        case ISClientMessages.Buddychat:
                            {
                                int fWho = packet.ReadInt();
                                string Who = packet.ReadString();
                                string what = packet.ReadString();

                                Character pWho = CenterServer.Instance.FindCharacter(fWho);

                                if (pWho != null)
                                {
                                    foreach (KeyValuePair<int, Buddy> kvp in pWho.FriendsList)
                                    {
                                        if (CenterServer.Instance.IsOnline(kvp.Value.CharacterID))
                                        {
                                            Character Buddy = CenterServer.Instance.GetCharacterByCID(kvp.Value.CharacterID);

                                            Packet pw = new Packet(ISServerMessages.Buddychat);
                                            pw.WriteInt(Buddy.ID);
                                            pw.WriteBytes(GroupMessagePacket.GroupMessage(pWho, what, 0));

                                            LocalServer BuddyChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[Buddy.ChannelID];

                                            BuddyChannel.Connection.SendPacket(pw);
                                        }
                                    }
                                }
                                break;
                            }

                        case ISClientMessages.MessengerOperation:
                            {
                                byte pOperation = packet.ReadByte();
                                Character pCharacter = CenterServer.Instance.GetCharacterByCID(packet.ReadInt());
                                switch (pOperation)
                                {
                                    case 0x00:
                                        {
                                            int MessengerID = packet.ReadInt();
                                            if (pCharacter != null)
                                            {
                                                pCharacter.Gender = packet.ReadByte();
                                                pCharacter.Skin = packet.ReadByte();
                                                pCharacter.Face = packet.ReadInt();
                                                packet.ReadByte();
                                                pCharacter.Hair = packet.ReadInt();
                                                pCharacter.EquipData = packet.ReadLeftoverBytes();

                                                if (MessengerID == 0)
                                                {
                                                    Messenger m = new Messenger(pCharacter);
                                                    pCharacter.MessengerSlot = 0;
                                                }
                                                else
                                                {
                                                    Messenger m = Messenger.GetMessenger(MessengerID);

                                                    m.AddUser(pCharacter);
                                                }
                                            }
                                            break;
                                        }
                                    case 0x03:
                                        {
                                            Messenger m = Messenger.GetMessenger(pCharacter.ID);
                                            if (m != null)
                                            {
                                                Console.WriteLine("received case 3!");
                                                string Who = packet.ReadString();
                                                Character To = CenterServer.Instance.GetCharacterByName(Who);

                                                if (CenterServer.Instance.IsOnline(To))
                                                {
                                                    SendPacket(Messenger.MessengerResponse(pCharacter.ID, Who, 1));

                                                    LocalServer ToChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[To.ChannelID];
                                                    ToChannel.Connection.SendPacket(Messenger.MessengerInvite(pCharacter.Name, m, To.ID));
                                                }
                                                else
                                                {
                                                    SendPacket(Messenger.MessengerResponse(pCharacter.ID, Who, 0));
                                                }
                                            }
                                            break;
                                        }
                                }
                                break;
                            }

                        case ISClientMessages.BuddyOperation:
                            {
                                byte pOperation = packet.ReadByte();
                                switch (pOperation)
                                {
                                    case 0: //Load BuddyList from database
                                        {
                                            Character chr = CenterServer.Instance.GetCharacterByCID(packet.ReadInt());
                                            if (chr != null)
                                            {
                                                chr.FriendsList.LoadBuddies(chr.ID);
                                                Packet pw = new Packet(ISServerMessages.BuddyOperation);
                                                pw.WriteByte(0);
                                                pw.WriteInt(chr.ID);
                                                pw.WriteBytes(BuddyPacket.UpdateBuddyList(chr));
                                                SendPacket(pw);
                                            }
                                            break;
                                        }
                                    case 1:
                                        {
                                            int fromWho = packet.ReadInt();
                                            string toWho = packet.ReadString();

                                            Character Who = CenterServer.Instance.GetCharacterByCID(fromWho);
                                            Character To = CenterServer.Instance.GetCharacterByName(toWho);

                                            int WhoID = CenterServer.Instance.CharacterDatabase.AccountIdByName(toWho);

                                            if (Who != null)
                                            {
                                                Packet pw = new Packet(ISServerMessages.BuddyOperation);
                                                pw.WriteByte(1);

                                                if (Who.FriendsList.Count >= 20)
                                                {
                                                    pw.WriteInt(Who.ID);
                                                    pw.WriteBytes(BuddyPacket.BuddyMessage(BuddyResults.BuddyListFull));
                                                    SendPacket(pw);
                                                }
                                                else if (!BuddyList.CharacterRegistered(toWho))
                                                {
                                                    pw.WriteInt(Who.ID);
                                                    pw.WriteBytes(BuddyPacket.BuddyMessage(BuddyResults.CharacterNotRegistered));
                                                    SendPacket(pw);
                                                }
                                                else if (BuddyList.OfflineVictimAdmin(toWho))
                                                {
                                                    pw.WriteInt(Who.ID);
                                                    pw.WriteBytes(BuddyPacket.BuddyMessage(BuddyResults.GameMasterNotAvailable));
                                                    SendPacket(pw);
                                                }
                                                else if (Who.FriendsList.ContainsKey(CenterServer.Instance.CharacterDatabase.AccountIdByName(toWho)))
                                                {
                                                    pw.WriteInt(Who.ID);
                                                    pw.WriteBytes(BuddyPacket.BuddyMessage(BuddyResults.AlreadyOnList));
                                                    SendPacket(pw);
                                                }
                                                else if (BuddyList.OfflineVictimBuddyListCount(toWho) >= 20)
                                                {
                                                    pw.WriteInt(Who.ID);
                                                    pw.WriteBytes(BuddyPacket.BuddyMessage(BuddyResults.UserBuddyListFull));
                                                    SendPacket(pw);
                                                }
                                                else
                                                {
                                                    if (CenterServer.Instance.IsOnline(To))
                                                    {
                                                        Who.FriendsList.AddBuddy(Who, To, false);
                                                    
                                                        pw.WriteInt(To.ID);
                                                        pw.WriteBytes(BuddyPacket.RequestAddBuddy(To, Who));
                                                        LocalServer ToChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[To.ChannelID];

                                                        ToChannel.Connection.SendPacket(pw);

                                                        Packet pPacket = new Packet(ISServerMessages.BuddyOperation);
                                                        pPacket.WriteByte(0);
                                                        pPacket.WriteInt(Who.ID);
                                                        pPacket.WriteBytes(BuddyPacket.UpdateBuddyList(Who));
                                                        SendPacket(pPacket);
                                                    }
                                                    else
                                                    {
                                                        //Pending request
                                                        BuddyList.AddPendingRequest(Who.ID, WhoID, toWho, Who.Name);
                                                        Who.FriendsList.Add(WhoID, new Buddy(Who.ID, toWho, -1, true));

                                                        Packet p = new Packet(ISServerMessages.BuddyOperation);
                                                        p.WriteByte(0);
                                                        p.WriteInt(Who.ID);
                                                        p.WriteBytes(BuddyPacket.UpdateBuddyList(Who));
                                                        SendPacket(p);
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    case 2:
                                        {
                                            Character Accepter = CenterServer.Instance.GetCharacterByCID(packet.ReadInt());
                                            int Adder = packet.ReadInt();

                                            Character pAdder = CenterServer.Instance.GetCharacterByCID(Adder);
                                            BuddyList.AcceptBuddyRequest(Accepter, Adder, CenterServer.Instance.CharacterDatabase.getCharacterNameByID(Adder));
                                            if (CenterServer.Instance.IsOnline(Adder))
                                            {
                                                Packet pw = new Packet(ISServerMessages.BuddyOperation);
                                                pw.WriteByte(2);
                                                pw.WriteInt(Adder);
                                                pw.WriteBytes(BuddyPacket.UpdateBuddyList(pAdder));

                                                LocalServer AdderChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pAdder.ChannelID];
                                                AdderChannel.Connection.SendPacket(pw);

                                                
                                                Packet p = new Packet(ISServerMessages.BuddyOperation);
                                                p.WriteByte(4);
                                                p.WriteInt(Adder);
                                                p.WriteBytes(BuddyPacket.UpdateChannel(Accepter.ChannelID, Accepter.ID));

                                                AdderChannel.Connection.SendPacket(pw);
                                            }

                                            if (!Accepter.FriendsList.ContainsKey(Adder))
                                            {
                                                if (CenterServer.Instance.IsOnline(pAdder))
                                                {
                                                    Accepter.FriendsList.Add(Adder, new Buddy(Adder, pAdder.Name, pAdder.ChannelID, true));

                                                    Packet p = new Packet(ISServerMessages.BuddyOperation);
                                                    p.WriteByte(4);
                                                    p.WriteInt(Adder);
                                                    p.WriteBytes(BuddyPacket.UpdateChannel(Accepter.ChannelID, Accepter.ID));
                                                    LocalServer AdderChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[pAdder.ChannelID];

                                                    AdderChannel.Connection.SendPacket(p);
                                                }
                                                else
                                                {
                                                    Accepter.FriendsList.Add(Adder, new Buddy(Adder, CenterServer.Instance.CharacterDatabase.getCharacterNameByID(Adder), -1, true));
                                                }
                                            }
                                            else
                                            {
                                                Accepter.FriendsList[Adder].Assigned = true;
                                            }             
                                            Packet pPacket = new Packet(ISServerMessages.BuddyOperation);
                                            pPacket.WriteByte(2);
                                            pPacket.WriteInt(Accepter.ID);
                                            pPacket.WriteBytes(BuddyPacket.UpdateBuddyList(Accepter));
                                            SendPacket(pPacket);

                                            break;
                                        }
                                    case 3:
                                        {
                                            Character Who = CenterServer.Instance.GetCharacterByCID(packet.ReadInt());
                                            int toDelete = packet.ReadInt();
                                            Character Delete = CenterServer.Instance.GetCharacterByCID(toDelete);
                                            Who.FriendsList.Remove(toDelete);
                                            BuddyList.ChangePending(Who, toDelete, true);
                                            if (CenterServer.Instance.IsOnline(Delete))
                                            {
                                                Delete.FriendsList.Remove(Who.ID);

                                                Packet pPacket = new Packet(ISServerMessages.BuddyOperation);
                                                pPacket.WriteByte(3);
                                                pPacket.WriteInt(Delete.ID);
                                                pPacket.WriteBytes(BuddyPacket.UpdateBuddyList(Delete));
                                                LocalServer DeleteChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[Delete.ChannelID];

                                                DeleteChannel.Connection.SendPacket(pPacket);
                                            }
                                            Packet pw = new Packet(ISServerMessages.BuddyOperation);
                                            pw.WriteByte(3);
                                            pw.WriteInt(Who.ID);
                                            pw.WriteBytes(BuddyPacket.UpdateBuddyList(Who));
                                            SendPacket(pw);
                                            break;
                                        }
                                    case 4: //Update buddy channel
                                        {
                                            Character pCharacter = CenterServer.Instance.GetCharacterByCID(packet.ReadInt());
                                            sbyte Channel = packet.ReadSByte();
                                            if (pCharacter != null)
                                            {
                                                foreach (KeyValuePair<int, Buddy> kvp in pCharacter.FriendsList)
                                                {
                                                    if (CenterServer.Instance.IsOnline(kvp.Value.CharacterID))
                                                    {
                                                        Character Buddy = CenterServer.Instance.GetCharacterByCID(kvp.Value.CharacterID);

                                                        Packet pw = new Packet(ISServerMessages.BuddyOperation);
                                                        pw.WriteByte(4);
                                                        pw.WriteInt(Buddy.ID);
                                                        pw.WriteBytes(BuddyPacket.UpdateChannel((int)Channel, pCharacter.ID));

                                                        LocalServer BuddyChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[Buddy.ChannelID];

                                                        BuddyChannel.Connection.SendPacket(pw);
                                                        
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    case 5:
                                        {
                                            Character pCharacter = CenterServer.Instance.GetCharacterByCID(packet.ReadInt());
                                            if (pCharacter != null)
                                            {
                                                foreach (KeyValuePair<int, Buddy> kvp in pCharacter.FriendsList)
                                                {
                                                    if (CenterServer.Instance.IsOnline(kvp.Value.CharacterID))
                                                    {
                                                        Character Buddy = CenterServer.Instance.GetCharacterByCID(kvp.Value.CharacterID);

                                                        if (Buddy.FriendsList.ContainsKey(pCharacter.ID))
                                                        {
                                                            Buddy.FriendsList[pCharacter.ID].Channel = -1;

                                                            Packet pw = new Packet(ISServerMessages.BuddyOperation);
                                                            pw.WriteByte(4);
                                                            pw.WriteInt(Buddy.ID);
                                                            pw.WriteBytes(BuddyPacket.UpdateChannel(-1, pCharacter.ID));

                                                            LocalServer BuddyChannel = CenterServer.Instance.Worlds[(byte)Server.WorldID].GameServers[Buddy.ChannelID];

                                                            if (!pCharacter.isCCing)
                                                            BuddyChannel.Connection.SendPacket(pw);
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        case ISClientMessages.BuddyDisconnect:
                            {
                                int id = packet.ReadInt();
                                Character disconnecter = CenterServer.Instance.GetCharacterByCID(id);
                                if (!disconnecter.isCCing) //just making sure you're not switching channels lol
                                {
                                    disconnecter.bChannelID = -1;
                                    //Buddy.UpdateChannel(disconnecter);
                                   
                                }
                                
                                
                                Console.WriteLine("received disconnection packet... updating buddies's buddylists");
                                break;
                            }
                        case ISClientMessages.AdminMessage:
                            {
                                Packet pw = new Packet(ISServerMessages.AdminMessage);
                                pw.WriteString(packet.ReadString());
                                pw.WriteByte(packet.ReadByte());
                                CenterServer.Instance.Worlds[Server.WorldID].SendPacketToEveryGameserver(pw);
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
                                    //Console.WriteLine("heheheheh unregister player");
                                    CenterServer.Instance.mOnlineCharacters.Remove(CenterServer.Instance.FindCharacter(charid));
                                }

                                break;
                            }

                            /**
                       
                        
                        
                            **/
                    
                    case ISClientMessages.MessengerJoin:
                    {
                        int MessengerID = packet.ReadInt();
                        int JoiningCID = packet.ReadInt();

                        //Character Data
                        string pName = packet.ReadString();
                        byte pGender = packet.ReadByte();
                        byte pSkin = packet.ReadByte();
                        int pFace = packet.ReadInt();
                        int pHair = packet.ReadInt();

                        Dictionary<byte, int> equips = new Dictionary<byte, int>();
                        while (true)
                        {
                            byte slot = packet.ReadByte();
                            if (slot == 0xFF)
                                break;
                            int itemid = packet.ReadInt();
                            equips.Add(slot, itemid);
                        }
                        Character pCharacter = CenterServer.Instance.GetCharacterByCID(JoiningCID);
                        pCharacter.Gender = pGender;
                        pCharacter.Skin = pSkin;
                        pCharacter.Face = pFace;
                        pCharacter.Hair = pHair;
                        pCharacter.Equips = equips;

                        if (MessengerID == 0)
                        {
                            //Self Enter Result (No Packet is needed)
                            //Messenger mr = Messenger.CreateMessenger(pCharacter);
                        }
                        break;
                    }

                    
                            /**
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
                             * 
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
                                chr.bChannelID = chr.LastChannel;
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
                                        chr.MovingToCashShop = true;
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