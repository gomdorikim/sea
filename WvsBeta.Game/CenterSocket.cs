using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using System.Configuration;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class CenterSocket : AbstractConnection
    {

        public CenterSocket()
            : base(Server.Instance.CenterIP.ToString(), (ushort)Server.Instance.CenterPort)
        {

        }

        public override void OnDisconnect()
        {
            Console.WriteLine("Disconnected from the Center Server! Something went wrong! :S");
            // release all connections
            Environment.Exit(0);
        }

        public override void OnHandshakeInbound(Packet pPacket)
        {
            Packet packet2 = new Packet(ISClientMessages.ServerRequestAllocation);
            packet2.WriteString(Server.Instance.Name);
            packet2.WriteString(Server.Instance.PublicIP.ToString());
            packet2.WriteUShort(Server.Instance.Port);
            packet2.WriteByte(Server.Instance.WorldID);
            //packet2.WriteString(Server.Instance.WorldName);
            packet2.WriteBool(Server.Instance.AdultWorld);
            packet2.WriteString(Server.Instance.MasterIP.ToString());
            SendPacket(packet2);
        }

        public override void AC_OnPacketInbound(Packet packet)
        {
            ISServerMessages msg = (ISServerMessages)packet.ReadByte();
            try
            {
                switch (msg)
                {
                    case ISServerMessages.ChangeRates:
                        {
                            double mobexprate = packet.ReadDouble();
                            double mesosamountrate = packet.ReadDouble();
                            double dropchancerate = packet.ReadDouble();

                            if (mobexprate > 0)
                            {
                                Server.Instance.RateMobEXP = mobexprate;
                                Console.WriteLine("Changed EXP Rate to {0}", mobexprate);
                            }
                            if (mesosamountrate > 0)
                            {
                                Server.Instance.RateMesoAmount = mesosamountrate;
                                Console.WriteLine("Changed Meso Rate to {0}", mesosamountrate);
                            }
                            if (dropchancerate > 0)
                            {
                                Server.Instance.RateDropChance = dropchancerate;
                                Console.WriteLine("Changed Drop Rate to {0}", dropchancerate);
                            }
                            SendUpdateRates();
                            break;
                        }

                    case ISServerMessages.PlayerChangeServerResult:
                        {
                       
                            string session = packet.ReadString();
                            Player player = Server.Instance.GetPlayer(session);
                            if (player != null)
                            {
                                int charid = packet.ReadInt();
                                byte[] ip = packet.ReadBytes(4);
                                ushort port = packet.ReadUShort();
                                if (port == 0)
                                {
                                    Packet pw = new Packet(ServerMessages.IncorrectChannelNumber);
                                    player.Character.sendPacket(pw);
                                }
                                else
                                {
                                    
                                    player.Character.Save();
                                    player.SaveOnDisconnect = false;
                                    player.Socket.SendConnectToServer(charid, ip, port);
                                   
                                }
                            }

                            break;
                        }
                    case ISServerMessages.PlayerWhisperOrFindOperationResult:
                        {
                            bool whisper = packet.ReadBool();
                            bool found = packet.ReadBool();
                            int victim = packet.ReadInt();
                            Character victimChar = Server.Instance.GetCharacter(victim);
                            if (victimChar == null) return;
                            if (whisper)
                            {
                                if (found)
                                {
                                    string sender = packet.ReadString();
                                    byte channel = packet.ReadByte();
                                    string message = packet.ReadString();
                                    MessagePacket.Whisper(victimChar, sender, channel, message);
                                }
                                else
                                {
                                    string sender = packet.ReadString();
                                    MessagePacket.Find(victimChar, sender, -1, 0, false);

                                }
                            }
                            else
                            {
                                if (found)
                                {
                                    string sender = packet.ReadString();
                                    sbyte channel = packet.ReadSByte();
                                    sbyte wat = packet.ReadSByte();
                                    MessagePacket.Find(victimChar, sender, channel, wat, false);
                                }
                                else
                                {
                                    string sender = packet.ReadString();
                                    MessagePacket.Find(victimChar, sender, -1, 0, false);
                                }


                            }
                            break;
                        }

                    case ISServerMessages.PlayerSuperMegaphone:
                        {
                            MessagePacket.SendSuperMegaphoneMessage(packet.ReadString(), packet.ReadBool(), packet.ReadByte());
                            break;
                        }

                    case ISServerMessages.MessengerOperation:
                        {
                            byte Operation = packet.ReadByte();
                            switch (Operation)
                            {
                                case 0x00:
                                    {
                                        
                                        Character pCharacter = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (pCharacter != null)
                                        {
                                            pCharacter.sendPacket(packet.ReadLeftoverBytes());
                                            InventoryPacket.NoChange(pCharacter);
                                        }
                                        break;
                                    }
                                case 0x01:
                                    {
                                        
                                        Character pCharacter = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (pCharacter != null)
                                        {
                                            pCharacter.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        break;
                                    }
                                case 0x03:
                                    {
                                      
                                        Character pCharacter = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (pCharacter != null)
                                        {
                                            pCharacter.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        break;
                                    }
                                case 0x05:
                                    {
                                        
                                        Character pCharacter = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (pCharacter != null)
                                        {
                                            pCharacter.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case ISServerMessages.BuddyOperation:
                        {
                            byte Operation = packet.ReadByte();

                            switch (Operation)
                            {
                                case 0:
                                    {
                                        Character To = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (To != null)
                                        {
                                            To.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        InventoryPacket.NoChange(To);
                                        break;
                                    }
                                case 1:
                                    {
                                        Character To = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (To != null)
                                        {
                                            To.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        InventoryPacket.NoChange(To);
                                        break;
                                    }
                                case 2:
                                    {
                                        Character To = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (To != null)
                                        {
                                            To.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        InventoryPacket.NoChange(To);
                                        break;
                                    }
                                case 3:
                                    {
                                        Character To = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (To != null)
                                        {
                                            To.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        InventoryPacket.NoChange(To);
                                        break;
                                    }
                                case 4:
                                    {
                                        Character Buddy = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (Buddy != null)
                                        {
                                            Buddy.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        InventoryPacket.NoChange(Buddy);
                                        break;
                                    }
                                case 5:
                                    {
                                        Character Buddy = Server.Instance.GetCharacter(packet.ReadInt());
                                        if (Buddy != null)
                                        {
                                            Buddy.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        InventoryPacket.NoChange(Buddy);
                                        break;
                                    }

                            }
                            break;
                        }

                    case ISServerMessages.BuddyInvite:
                        {
                            byte Result = packet.ReadByte();

                            if (Result != 1)
                            {
                                string Sender = packet.ReadString();
                                Character sender = Server.Instance.GetCharacter(Sender);
                                BuddyPacket.BuddyMessage(sender, Result);
                            }
                            else
                            {
                                bool Pending = packet.ReadBool();
                                if (!Pending) //Pending is handled from CenterServer 
                                {
                                    bool ToInviter = packet.ReadBool();
                                    if (ToInviter)
                                    {
                                        string sendername = packet.ReadString();
                                        byte[] toUpdate = packet.ReadLeftoverBytes();
                                        Character bSender = Server.Instance.GetCharacter(sendername);
                                        bSender.sendPacket(toUpdate);

                                    }
                                    string pReceiver = packet.ReadString();
                                    string pSender = packet.ReadString();
                                    int pSenderID = packet.ReadInt();
                                    int pReceiverChannel = packet.ReadInt();
                                    Character pBuddyReceiver = Server.Instance.GetCharacter(pReceiver);
                                    if (pBuddyReceiver != null)
                                    {

                                        BuddyPacket.RequestBuddyListAdd(pBuddyReceiver, pSenderID, pSender);
                                    }
                                }
                                else if (Pending)
                                {
                                    string Invitee = packet.ReadString();
                                    string Invited = packet.ReadString();
                                    int channel = packet.ReadInt();
                                    byte[] tUpdate = packet.ReadLeftoverBytes();
                                    Character bpSender = Server.Instance.GetCharacter(Invitee);
                                    bpSender.sendPacket(tUpdate);
                                }

                            }
                            break;
                        }

                    case ISServerMessages.BuddyDisconnect:
                        {
                            int characterid = packet.ReadInt();
                            byte[] bytes = packet.ReadLeftoverBytes();
                            Character toUpdate = Server.Instance.GetCharacter(characterid);
                            toUpdate.sendPacket(bytes);
                            break;
                        }

                    case ISServerMessages.Buddychat:
                        {
                            int CID = packet.ReadInt();
                            Character pCharacter = Server.Instance.GetCharacter(CID);
                            if (pCharacter != null)
                            {
                                pCharacter.sendPacket(packet.ReadLeftoverBytes());
                            }
                            break;
                        }

                    case ISServerMessages.PlayerPartyOperation:
                        {
                            break;
                        }

                    case ISServerMessages.PartyOperation:
                        {
                            byte pOperation = packet.ReadByte();
                            switch (pOperation)
                            {
                                case 1:
                                    {
                                        int cID = packet.ReadInt();
                                        int pID = packet.ReadInt();

                                        Character cCharacter = Server.Instance.GetCharacter(cID);
                                        if (cCharacter != null)
                                        {
                                            cCharacter.PartyID = pID;
                                            cCharacter.Leader = true;
                                        }
                                        break;
                                    }
                                case 2: //Leave
                                    {
                                        int cID = packet.ReadInt();
                                        bool Leaver = packet.ReadBool();
                                        Character pMember = Server.Instance.GetCharacter(cID);
                                        if (pMember != null)
                                        {
                                            pMember.sendPacket(packet.ReadLeftoverBytes());

                                            if (Leaver)
                                            {
                                                pMember.PartyID = -1;
                                                pMember.Leader = false;
                                            }
                                        }

                                        break;
                                    }
                                case 3: //Join 
                                    {
                                        int pMember = packet.ReadInt();
                                        int pID = packet.ReadInt();
                                        Character Member = Server.Instance.GetCharacter(pMember);
                                        if (Member != null)
                                        {
                                            byte[] remaining = packet.ReadLeftoverBytes();
                                            Member.PartyID = pID;
                                            Member.sendPacket(remaining);

                                            MapPacket.UpdatePartyMemberHP(Member);
                                            MapPacket.ReceivePartyMemberHP(Member);
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        int pMember = packet.ReadInt();
                                        bool pExpelled = packet.ReadBool();
                                        Character Member = Server.Instance.GetCharacter(pMember);                                 
                                        if (Member != null)
                                        {
                                            Member.sendPacket(packet.ReadLeftoverBytes());

                                            if (pExpelled)
                                            {
                                                Member.PartyID = -1;
                                            }
                                            else
                                            {
                                                this.PlayerPartyOperation(Member, 7, Member.PartyID);
                                            }

                                        }
                                        break;
                                    }
                                case 6: //Party Chat
                                    {
                                        int cID = packet.ReadInt();
                                        Character pMember = Server.Instance.GetCharacter(cID);
                                        if (pMember != null)
                                        {
                                            pMember.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        break;
                                    }
                                case 7: //CC Update
                                    {
                                        int pID = packet.ReadInt();
                                        Character pCharacter = Server.Instance.GetCharacter(pID);
                                        if (pCharacter != null)
                                        {
                                            byte[] remaining = packet.ReadLeftoverBytes();
                                            pCharacter.sendPacket(remaining);
                                            //MessagePacket.SendNotice("received!!!", pCharacter);
                                        }
                                        break;
                                    }
                                case 8: //Get Party Info
                                    {
                                       //eh
                                        
                                        break;
                                    }
                                case 9:
                                    {
                                        int cID = packet.ReadInt();
                                        Character pCharacter = Server.Instance.GetCharacter(cID);
                                        if (pCharacter != null)
                                        {
                                            pCharacter.sendPacket(packet.ReadLeftoverBytes());
                                        }
                                        break;
                                    }
                                case 0x15:
                                    {
                                        int lID = packet.ReadInt();
                                        Character pLeader = Server.Instance.GetCharacter(lID);
                                        pLeader.sendPacket(packet.ReadLeftoverBytes());
                                        break;
                                    }
                            }
                            break;
                        }

               

                    /**
                case ISServerMessages.BuddyInvite:
                    {
                        bool what = packet.ReadBool();
                        if (what == true)
                        {
                            string charname = packet.ReadString();
                            string sendername = packet.ReadString();
                            int senderID = packet.ReadInt();
                            int channel = packet.ReadInt();
                            Character buddyInviter = Server.Instance.GetCharacter(senderID);
                            Character buddyReceiver = Server.Instance.GetCharacter(charname);
                            if (buddyReceiver.Admin)
                            {
                                BuddyPacket.BuddyMessage(buddyInviter, 0x0E);
                            }
                            else
                            {
                                if (buddyInviter.Buddylist.Count >= buddyInviter.PrimaryStats.BuddyListCapacity)
                                {
                                    BuddyPacket.BuddyMessage(buddyInviter, 0x0B);
                                }
                                else
                                {
                                    if (buddyInviter.Buddylist.ContainsKey(buddyReceiver.ID))
                                    {
                                        BuddyPacket.BuddyMessage(buddyInviter, 0x0D);
                                    }
                                    else
                                    {
                                        BuddyPacket.RequestBuddyListAdd(buddyReceiver, senderID);
                                        buddyInviter.Buddylist.Add(buddyReceiver.ID, new Buddy(buddyReceiver.ID, buddyReceiver.Name, (byte)channel, false));
                                    }
                                }
                                    
                            }
                        }
                        else
                        {
                            string pSender = packet.ReadString();
                            string pReceiver = packet.ReadString();
                            Character Sender = Server.Instance.GetCharacter(pSender);
                            using (MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM characters WHERE name = '" + MySqlHelper.EscapeString(pReceiver) + "'") as MySqlDataReader)
                            {
                                if (data.HasRows)
                                {
                                    if (BuddyList.OfflineVictimAdmin(pReceiver))
                                    {
                                        BuddyPacket.BuddyMessage(Sender, 0x0E);
                                    }
                                    else
                                    {
                                        if (Sender.Buddylist.ContainsKey(Server.Instance.CharacterDatabase.AccountIdByName(pReceiver)))
                                        {
                                            BuddyPacket.BuddyMessage(Sender, 0x0D);
                                        }
                                        else
                                        {
                                            Sender.Buddylist.Add(Server.Instance.CharacterDatabase.AccountIdByName(pReceiver), new Buddy(Server.Instance.CharacterDatabase.AccountIdByName(pReceiver), pReceiver, 0, false));
                                            BuddyPacket.UpdateBuddyList(Sender, 0);
                                            BuddyList.AddPendingRequest(pReceiver, Sender.Name, Sender.ID);
                                            //Add pending request
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                                        
                      **/
                    case ISServerMessages.AdminMessage:
                        {
                            string message = packet.ReadString();
                            byte type = packet.ReadByte();

                            Packet pw = new Packet(0x23);
                            pw.WriteByte(type);
                            pw.WriteString(message);
                            if (type == 4)
                            {
                                pw.WriteBool((message.Length == 0 ? false : true));
                            }
                            foreach (KeyValuePair<int, Map> kvp in DataProvider.Maps)
                            {
                                kvp.Value.SendPacket(pw);
                            }
                            break;
                        }

                    case ISServerMessages.ServerAssignmentResult:
                        {
                            Server.Instance.ID = packet.ReadByte();
                            int amount = (int)Server.Instance.CharacterDatabase.RunQuery("UPDATE users INNER JOIN characters ON users.id = characters.userid SET users.online = 0, characters.online = 0 WHERE users.online = " + ((int)(20000 + (int)Server.Instance.WorldID * 100 + Server.Instance.ID)));
                            Console.WriteLine(string.Format("Handling as Game Server {0} on World {1} ({2})", Server.Instance.ID, Server.Instance.WorldID, Server.Instance.WorldName));
                            Console.WriteLine(string.Format("Freed {0} players.", amount));
                            //RunTimedFunction.AddTimedFunction(delegate { Server.Instance.CheckMaps(); }, new TimeSpan(), new TimeSpan(0, 0, 20), BetterTimerTypes.MapClearup, 0, 0);
                            break;
                        }
                    case ISServerMessages.PlayerSendPacket:
                        {
                            Character pChar = Server.Instance.GetCharacter(packet.ReadInt());
                            if (pChar != null)
                                pChar.sendPacket(packet.ReadLeftoverBytes());
                            break;
                        }

                    default: break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + "\r\nPACKET: " + packet.ToString());
            }
        }




        public void SendUpdateRates()
        {
            Packet ret = new Packet(ISClientMessages.ChangeRates);
            ret.WriteDouble(Server.Instance.RateMobEXP);
            ret.WriteDouble(Server.Instance.RateMesoAmount);
            ret.WriteDouble(Server.Instance.RateDropChance);
            SendPacket(ret);
        }

        public void SendUpdateConnections(int value)
        {
            Packet packet = new Packet(ISClientMessages.ServerSetConnectionsValue);
            packet.WriteInt(value);
            SendPacket(packet);
        }

        public void RequestCharacterConnectToWorld(string Hash, int charid, byte world, byte channel)
        {
            Packet packet = new Packet(ISClientMessages.PlayerChangeServer);
            packet.WriteString(Hash);
            packet.WriteInt(charid);
            packet.WriteByte(world);
            packet.WriteByte(channel);
            packet.WriteBool(true);
            SendPacket(packet);
        }

        public void UnregisterCharacter(int charid, bool cc)
        {
            Packet packet = new Packet(ISClientMessages.ServerRegisterUnregisterPlayer);
            packet.WriteInt(charid);
            packet.WriteBool(false);
            SendPacket(packet);
        }

        public void RegisterCharacter(int charid, string name, short job, byte level)
        {
            Packet packet = new Packet(ISClientMessages.ServerRegisterUnregisterPlayer);
            packet.WriteInt(charid);
            packet.WriteBool(true);
            packet.WriteString(name);
            packet.WriteShort(job);
            packet.WriteByte(level);
            SendPacket(packet);
        }

        public void RequestBuddyListLoad(string charname, bool test, int buddycapacity)
        {
            Packet packet = new Packet(ISClientMessages.BuddyOperation);
            packet.WriteString(charname);
            packet.WriteBool(test);
            packet.WriteInt(buddycapacity);
            SendPacket(packet);
        }
        public void PlayerWhisper(int charid, string name, string message)
        {
            Packet packet = new Packet(ISClientMessages.PlayerWhisperOrFindOperation);
            packet.WriteInt(charid);
            packet.WriteBool(true);
            packet.WriteString(name);
            packet.WriteString(message);
            SendPacket(packet);
        }

        public void BuddyInvite(string charname, string sendername, int senderid)
        {
            Packet packet = new Packet(ISClientMessages.BuddyInvite);
            packet.WriteString(charname);
            packet.WriteString(sendername);
            packet.WriteInt(senderid);
            SendPacket(packet);
        }

        public void BuddyListUpdate(int charid)
        {
            // Packet packet = new Packet(ISClientMessages.BuddyListUpdate);
            //packet.WriteInt(charid);
            //SendPacket(packet);
        }

        public void BuddyInviteTest(string charname, int senderid)
        {
            Packet packet = new Packet(ISClientMessages.BuddyOperation);
            packet.WriteString(charname);
            packet.WriteInt(senderid);
            SendPacket(packet);
        }

        public void BuddyDisconnect(int charid)
        {
            Packet packet = new Packet(ISClientMessages.BuddyDisconnect);
            packet.WriteInt(charid);
            SendPacket(packet);
        }

        public void AdminMessage(string message, byte type)
        {
            Packet packet = new Packet(ISClientMessages.AdminMessage);
            packet.WriteString(message);
            packet.WriteByte(type);
            SendPacket(packet);
        }
        public void PlayerFind(int charid, string name)
        {
            Packet packet = new Packet(ISClientMessages.PlayerWhisperOrFindOperation);
            packet.WriteInt(charid);
            packet.WriteBool(false);
            packet.WriteString(name);
            SendPacket(packet);
        }

        public void FindPlayerInOtherGameServer(string name)
        {
            Packet packet = new Packet(ISClientMessages.FindPlayer);
            packet.WriteString(name);
            SendPacket(packet);
        }

        public void PlayerSuperMegaphone(string message, bool whisperetc)
        {
            Packet packet = new Packet(ISClientMessages.PlayerUsingSuperMegaphone);
            packet.WriteString(message);
            packet.WriteBool(whisperetc);
            packet.WriteByte(Server.Instance.ID);
            SendPacket(packet);
        }


        public void MessengerJoin(int messengerid, int cid, string name, byte gender, byte skin, int face, int hair, Dictionary<byte, int> equips)
        {
            Packet packet = new Packet(ISClientMessages.MessengerJoin);
            packet.WriteInt(messengerid);
            packet.WriteInt(cid);
            packet.WriteString(name);
            packet.WriteByte(gender);
            packet.WriteByte(skin);
            packet.WriteInt(face);
            packet.WriteInt(hair);
            foreach (KeyValuePair<byte, int> equip in equips)
            {
                packet.WriteByte(equip.Key);
                packet.WriteInt(equip.Value);
            }
            packet.WriteByte(0xFF);
            SendPacket(packet);
        }

        public void MessengerLeave(int cid)
        {
            Packet packet = new Packet(ISClientMessages.MessengerLeave);
            packet.WriteInt(cid);
            SendPacket(packet);
        }

        public void MessengerInvite(int cid, string cinvitee)
        {
            Packet packet = new Packet(ISClientMessages.MessengerInvite);
            packet.WriteInt(cid);
            packet.WriteString(cinvitee);
            SendPacket(packet);
        }

        public void MessengerBlock(int cid, string invitee, string inviter, byte blockmode)
        {
            Packet packet = new Packet(ISClientMessages.MessengerBlocked);
            packet.WriteInt(cid);
            packet.WriteString(invitee);
            packet.WriteString(inviter);
            packet.WriteByte(blockmode);
            SendPacket(packet);
        }

        public void MessengerChat(int cid, string chatmsg)
        {
            Packet packet = new Packet(ISClientMessages.MessengerChat);
            packet.WriteInt(cid);
            packet.WriteString(chatmsg);
            SendPacket(packet);
        }

        public void PlayerUpdateMap(Character pCharacter)
        {
            Packet packet = new Packet(ISClientMessages.PlayerUpdateMap);
            packet.WriteInt(pCharacter.ID);
            packet.WriteInt(pCharacter.Map);
            packet.WriteInt(pCharacter.PartyID);
            SendPacket(packet);
        }

        public void TestTest(Character pCharacter, int PartyID)
        {
            Packet packet = new Packet(ISClientMessages.Test);
            packet.WriteInt(pCharacter.ID);
            packet.WriteInt(PartyID);
            SendPacket(packet);
        }

        public void PlayerMessengerOperation(Character pCharacter, byte pOperation, int MessengerID = 0, string Message = "")
        {
            Packet packet = new Packet(ISClientMessages.MessengerOperation);
            switch (pOperation)
            {
                case 0:
                    {
                        packet.WriteByte(0x00);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteInt(MessengerID);

                        packet.WriteByte(pCharacter.Gender);
                        packet.WriteByte(pCharacter.Skin);
                        packet.WriteInt(pCharacter.Face);
                        packet.WriteByte(0);
                        packet.WriteInt(pCharacter.Hair);
                        pCharacter.Inventory.GeneratePlayerPacket(packet);
                        packet.WriteByte(0xFF);
                        packet.WriteInt(pCharacter.GetPetID());
                        packet.WriteInt(0);
                        break;
                    }
                case 3:
                    {
                        packet.WriteByte(0x03);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteString(Message);
                        break;
                    }
            }
            SendPacket(packet);
        }

        public void PlayerBuddyOperation(Character pCharacter, byte pOperation, string toAdd = "", int cid = 0, sbyte Channel = 0)
        {
            Packet packet = new Packet(ISClientMessages.BuddyOperation);
            switch (pOperation)
            {
                case 0:
                    {
                        packet.WriteByte(0x00);
                        packet.WriteInt(pCharacter.ID);
                        break;
                    }
                case 1:
                    {
                        packet.WriteByte(0x01);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteString(toAdd);
                        break;
                    }
                case 2:
                    {
                        packet.WriteByte(0x02);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteInt(cid);
                        break;
                    }
                case 3:
                    {
                        packet.WriteByte(0x03);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteInt(cid);
                        break;
                    }
                case 4:
                    {
                        packet.WriteByte(0x04);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteSByte(Channel); ;
                        break;
                    }
                case 5:
                    {
                        packet.WriteByte(0x05);
                        packet.WriteInt(pCharacter.ID);
                        break;
                    }
                    
            }
            SendPacket(packet);
        }
        public void PlayerPartyOperation(Character pCharacter, byte pOperation, int PartyID = 0, int VictimID = 0, bool logoff = false, string leadername = "", string pinvitee = "", string pChat = "", byte Channel = 0)
        {
            Packet packet = new Packet(ISClientMessages.PartyOperation);
            switch (pOperation)
            {
                case 1: //Create
                    {
                        packet.WriteByte(1); //Create
                        packet.WriteInt(pCharacter.ID);
                        break;
                    }
                case 2: //Leave
                    {
                        packet.WriteByte(2);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteInt(PartyID);
                        break;
                    }
                case 3: //join
                    {
                        packet.WriteByte(3);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteInt(PartyID);
                        break;
                    }
                
                case 5: //Expell
                    {
                        packet.WriteByte(5);
                        packet.WriteInt(VictimID);
                        packet.WriteInt(PartyID);
                        break;
                    }
                case 6: //Party chat
                    {
                        packet.WriteByte(6);
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteInt(PartyID);
                        packet.WriteString(pChat);
                        break;
                    }
                case 7: //CC
                    {
                        packet.WriteByte(7); //CC update
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteInt(pCharacter.PartyID);
                        packet.WriteInt(pCharacter.Map);
                        break;
                    }
                case 8: //Update Player Info !
                    {
                        packet.WriteByte(8);
                        if (logoff)
                        {
                            packet.WriteBool(true);
                        }
                        else
                        {
                            packet.WriteBool(false);
                        }
                        packet.WriteInt(pCharacter.ID);
                        packet.WriteInt(PartyID);
                        break;
                    }
                case 0x15:
                    {
                        packet.WriteByte(0x15);
                        packet.WriteString(leadername);
                        packet.WriteString(pinvitee);
                        break;
                    }
                
            }
            SendPacket(packet);
        }
        public void PartyDisconnect(Character chr, int PartyID)
        {
            Packet packet = new Packet(ISClientMessages.PartyDisconnect);
            packet.WriteInt(chr.ID);
            packet.WriteInt(PartyID);
            SendPacket(packet);
        }

        public void BuffsUpdate(Character chr, int buffId, DateTime due)
        {
            Packet packet = new Packet(ISClientMessages.PlayerBuffUpdate);
            packet.WriteInt(chr.ID);
            packet.WriteInt(buffId);
            packet.WriteLong(Tools.GetTimeAsMilliseconds(due));
            SendPacket(packet);
        }

        public void BuddyChat(Character chr, string what)
        {
            Packet packet = new Packet(ISClientMessages.Buddychat);
            packet.WriteInt(chr.ID);
            packet.WriteString(chr.Name);
            packet.WriteString(what);
            SendPacket(packet);
        }
    }
}
