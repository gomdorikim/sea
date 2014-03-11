using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Game.Events;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class ClientSocket : AbstractConnection
    {
        public Player Player { get; set; }
        public bool Loaded { get; set; }
        public int DCID { get; set; }


        public ClientSocket(System.Net.Sockets.Socket pSocket)
            : base(pSocket)
        {
            Init();
        }

        public void Init()
        {
            Loaded = false;
            Pinger.Connections.Add(this);

            Player = new Player();
            Player.Socket = this;
            Player.Character = null;

            Server.Instance.AddPlayer(Player);
            SendHandshake(15, "", 0x07);
            Console.WriteLine("Connection received..");
           // SendConnectToServer(327, Server.Instance.CenterIP.GetAddressBytes(), (ushort)Server.Instance.CenterPort);

        }

        public override void OnDisconnect()
        {
            try
            {
                if (Player != null && Loaded)
                {
                    if (Player.Character != null)
                    {
                        Character chr = Player.Character;
                        Console.WriteLine("[{0}] {1} disconnected.", DateTime.Now.ToString(), chr.Name);
                        Server.Instance.CenterConnection.BuddyDisconnect(chr.ID);

                        if (chr.MapChair != -1)
                        {
                            Map map = DataProvider.Maps[chr.Map];
                            map.UsedSeats.Remove(chr.MapChair);
                            chr.MapChair = -1;
                            MapPacket.SendCharacterSit(chr, -1);
                        }
                        if (chr.Room != null)
                        {
                            if (chr.Room.Type == MiniRoomBase.RoomType.Trade)
                            {
                                chr.Room.RemovePlayer(chr, 1);
                            }
                            if (chr.Room.Type == MiniRoomBase.RoomType.PersonalShop)
                            {
                                PlayerShop ps = MiniRoomBase.PlayerShops[chr.Room.ID];
                                ps.RemovePlayerFromShop(chr);
                            }
                            if (chr.Room.Type == MiniRoomBase.RoomType.Omok)
                            {
                                Omok omok = MiniRoomBase.Omoks[chr.Room.ID];
                                if (chr.Room.Users[0] == chr)
                                {
                                    omok.CloseOmok(chr);
                                }
                                else
                                {
                                    if (omok.GameStarted)
                                    {
                                        omok.UpdateGame(omok.Users[0], false, true);
                                        omok.GameStarted = false;
                                        MiniRoomPacket.ShowLeaveRoom(chr.Room, chr, 2);
                                        omok.RemovePlayer(chr, 2);
                                    }
                                }
                            }
                        }
                        if (chr.PartyID != -1)
                        {
                            Server.Instance.CharacterDatabase.RunQuery("UPDATE characters SET party =  " + chr.PartyID + " WHERE ID = " + chr.ID + "");
                            Server.Instance.CenterConnection.PlayerPartyOperation(chr, 8, chr.PartyID, 0, true);
                            Server.Instance.CenterConnection.PartyDisconnect(chr, chr.PartyID);
                        }

                        if (EventManager.Instance.RegisteredShips.Count > 0)
                        {
                            foreach (KeyValuePair<Events.EventObjects.ShipType, Events.EventObjects.Ship> RegisteredShip in EventManager.Instance.RegisteredShips)
                            {
                                if (RegisteredShip.Value != null && RegisteredShip.Value.Passengers.Contains(chr))
                                {
                                    RegisteredShip.Value.UnregisterPassenger(chr);
                                }
                            }
                        }
                        Server.Instance.CenterConnection.PlayerBuddyOperation(chr, 5);
                        DataProvider.Maps[chr.Map].RemovePlayer(chr);
                        Server.Instance.CharacterList.Remove(chr.ID);

                        if (Player.SaveOnDisconnect)
                        {
                            chr.Save();
                        }
                        Server.Instance.CenterConnection.UnregisterCharacter(chr.ID, Player.SaveOnDisconnect == false);
                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users INNER JOIN characters ON users.id = characters.userid SET users.online = 0, characters.online = 0 WHERE characters.id = " + Player.Character.ID.ToString());

                        //Program.MainForm.ChangeLoad(false);
                        Player.Character = null;

                    }
                }
                if (Player != null)
                {
                    Player.Socket = null;
                    Server.Instance.RemovePlayer(Player.SessionHash);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Pinger.Connections.Remove(this);
        }

        public override void AC_OnPacketInbound(Packet packet)
        {
            try
            {
                short header = packet.ReadByte();
                if (!Loaded || Player.Character == null)
                {
                    switch (header)
                    {
                        case 0x07: OnPlayerLoad(packet); break;
                    }
                }
                else
                {
                    switch (header)
                    {
                        case 0x13: MapPacket.OnEnterPortal(packet, Player.Character); break;
                       // case 0x12: OnChangeChannel(packet); break;
                        case 0x14: OnChangeChannel(packet); break;
                        case 0x15: Server.Instance.CenterConnection.RequestCharacterConnectToWorld(Player.SessionHash, Player.Character.ID, Server.Instance.WorldID, 50); break;
                    //    case 0x14: MapPacket.HandleMove(Player.Character, packet); break;
                       // case 0x15: MapPacket.HandleSitChair(Player.Character, packet); break;

                        case 0x16: MapPacket.HandleMove(Player.Character, packet); break; 
                        case 0x18: AttackPacket.HandleMeleeAttack(Player.Character, packet); break;
                        case 0x19: AttackPacket.HandleRangedAttack(Player.Character, packet); break;
                        case 0x1A: AttackPacket.HandleMagicAttack(Player.Character, packet); break;
                        case 0x1C: CharacterStatsPacket.HandleCharacterDamage(Player.Character, packet); break;
                        case 0x1D: MessagePacket.HandleChat(Player.Character, packet); break;
                        case 0x1E: MapPacket.SendEmotion(Player.Character, packet.ReadInt()); break;
                        case 0x22: MapPacket.HandleNPCChat(Player.Character, packet); break;
                        case 0x23: NpcPacket.HandleNPCChat(Player.Character, packet); break;
                        case 0x24: NpcPacket.HandleNPCShop(Player.Character, packet); break;
                        case 0x25: StoragePacket.HandleStorage(Player.Character, packet); break;
                        //case 0x23: InventoryPacket.HandleInventoryPacket(Player.Character, packet); break;
                        //case 0x24: InventoryPacket.HandleUseItemPacket(Player.Character, packet); break;

                        case 0x26: InventoryPacket.HandleInventoryPacket(Player.Character, packet); break;
                        case 0x27: InventoryPacket.HandleUseItemPacket(Player.Character, packet); break; 
                     //   case 0x25: InventoryPacket.HandleUseSummonSack(Player.Character, packet); break;
                        case 0x2A: CashPacket.HandleCashItem(Player.Character, packet); break;
                      //  case 0x28: InventoryPacket.HandleUseReturnScroll(Player.Character, packet); break;
                      //  case 0x29: InventoryPacket.HandleScrollItem(Player.Character, packet); break;

                        case 0x2C: InventoryPacket.HandleScrollItem(Player.Character, packet); break;
                     //   case 0x2A: CharacterStatsPacket.HandleStats(Player.Character, packet); break;
                        case 0x2D: CharacterStatsPacket.HandleStats(Player.Character, packet); break; 
                        case 0x2E: CharacterStatsPacket.HandleHeal(Player.Character, packet); break;
                        case 0x2F: SkillPacket.HandleAddSkillLevel(Player.Character, packet); break;
                        //case 0x2E: SkillPacket.HandleStopSkill(Player.Character, packet); break;
                        case 0x29: PetsPacket.HandlePetFeed(Player.Character, packet); break; 
                        case 0x30: SkillPacket.HandleUseSkill(Player.Character, packet); break;
                        case 0x31: SkillPacket.HandleStopSkill(Player.Character, packet); break;
                        case 0x33: DropPacket.HandleDropMesos(Player.Character, packet.ReadInt()); break;
                        case 0x34: FamePacket.HandleFame(Player.Character, packet); break;
                        //case 0x30: DropPacket.HandleDropMesos(Player.Character, packet.ReadInt()); break;
                    //    case 0x31: FamePacket.HandleFame(Player.Character, packet); break;
                        //case 0x33: MapPacket.SendPlayerInfo(Player.Character, packet); break;
                        case 0x36: MapPacket.SendPlayerInfo(Player.Character, packet); break;
                       // case 0x34: PetsPacket.HandleSpawnPet(Player.Character, packet.ReadShort()); break;
                        case 0x37: PetsPacket.HandleSpawnPet(Player.Character, packet.ReadShort()); break;
                        case 0x39: MapPacket.OnMapTeleport(Player.Character, packet); break;
                        // case 0x36: // Map teleport: 36 04 00 70 73 30 31
                      //  case 0x37: CashPacket.HandleTeleRockFunction(Player.Character, packet); break;
                     //   case 0x38: ReportPacket.HandleReport(Player.Character, packet); break;
                   //     case 0x3A: AdminPacket.HandleAdminCommandMessage(Player.Character, packet); break;
                        //case 0x3B: MessagePacket.HandleSpecialChat(Player.Character, packet); break;
                        case 0x3B: LieDetectorPacket.HandleUseLieDetector(Player.Character, packet); break;
                        case 0x3C: LieDetectorPacket.AnswerLieDetector(Player.Character, packet); break;
                        case 0x3E: QuestPacket.HandleStartQuest(Player.Character, packet); break;
                        //case 0x3C: MessagePacket.HandleCommand(Player.Character, packet); break;
                        //case 0x3E: PlayerInteractionPacket.HandleInteraction(Player.Character, packet); break;
                        //case 0x3E: MiniRoomPacket.HandlePacket(Player.Character, packet); break;
                      //  case 0x3D: CUIMessengerPacket.OnPacket(Player.Character, packet); break;
                     //   case 0x3F: PartyPacket.HandleCreateParty(Player.Character, packet); break;
                     //   case 0x40: PartyPacket.HandlePartyMessages(Player.Character, packet); break;
                      //  case 0x41: CommandHandling.HandleAdminCommand(Player.Character, packet); break;
                        //case 0x42: CommandHandling.HandleAdminCommandLog(Player.Character, packet); break;
                        case 0x42: MessagePacket.HandleSpecialChat(Player.Character, packet); break;
                        case 0x43: MessagePacket.HandleCommand(Player.Character, packet); break;
                        case 0x44: MessengerPacket.HandleMessenger(Player.Character, packet); break;
                        case 0x45: MiniRoomPacket.HandlePacket(Player.Character, packet); break;
                        case 0x46: PartyPacket.HandleCreateParty(Player.Character, packet); break;
                        case 0x47: PartyPacket.HandlePartyMessages(Player.Character, packet); break;
                    //    case 0x48: PetsPacket.HandleMovePet(Player.Character, packet); break;
                   //     case 0x49: PetsPacket.HandlePetChat(Player.Character, packet); break;
                   //     case 0x4A: PetsPacket.HandleAction(Player.Character, packet); break;
                    //    case 0x4B: PetsPacket.HandlePetLoot(Player.Character, packet); break;
                        case 0x4C: BuddyPacket.HandleBuddy(Player.Character, packet); break;
                  //      case 0x4E: SummonPacket.HandleSummonMove(Player.Character, packet); break;
                   ////     case 0x4F: AttackPacket.HandleSummonAttack(Player.Character, packet); break;
                        case 0x50: SummonPacket.HandleSummonDamage(Player.Character, packet); break;
                        case 0x51: PetsPacket.HandleMovePet(Player.Character, packet); break;
                        case 0x53: PetsPacket.HandleAction(Player.Character, packet); break;
                        case 0x54: PetsPacket.HandlePetLoot(Player.Character, packet); break;
                        case 0x5F: MobPacket.HandleMobControl(Player.Character, packet); break;
                        case 0x68: DropPacket.HandlePickupDrop(Player.Character, packet); break;

                        case 0x6B: ReactorPacket.HandleReactorChangeState(Player.Character, packet); break;
                   //     case 0x56: MobPacket.HandleMobControl(Player.Character, packet); break;
                   //     case 0x57: MobPacket.HandleDistanceFromBoss(Player.Character, packet); break;
                    //    case 0x5B: MapPacket.HandleNPCAnimation(Player.Character, packet); break;
                    //    case 0x5F: DropPacket.HandlePickupDrop(Player.Character, packet); break;
                    //    case 0x65: EventPackets.HandleAdminEventStart(Player.Character, packet); break;

                    //    case 0x68: CoconutPackets.HandleEvent(Player.Character, packet); break;
                    //    case 0x6A: EventPackets.HandleAdminEventReset(Player.Character, packet); break;
                   //     case 0x70: MapPacket.HandleBoatStatusRequest(Player.Character, packet); break;
                        case 0x79: MapPacket.HandleBoatStatusRequest(Player.Character, packet); break;
                        case 0x10: //Only gets sent when player is on training map...  wtf :S
                            {
                                if (Player.Character.PrimaryStats != null) Player.Character.PrimaryStats.CheckExpired(MasterThread.CurrentDate);    
                            } break;
                        //case 0x0b: use for internet cafe!
                        case 0x0D: break; // Some hash thing, 1 integer....?
                        default:
                            {
                                string what = "[" + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString("D3") + "] Unknown packet found: " + packet.ToString();
                                try
                                {
                                    FileWriter.WriteLine(@"connection_log\" + Player.Character.ID.ToString() + ".txt", what, true);
                                }
                                catch (Exception ex)
                                {
                                    FileWriter.WriteLine(@"connection_log\Exceptions.txt", ex.ToString(), true);
                                }
                                if (Player.Character.Admin && packet.ToString() != "0B")
                                {
                                    MessagePacket.SendNotice("Unknown packet received! " + packet.ToString(), Player.Character);
                                }
                                if (packet.ToString() != "0B")
                                {
                                    Console.WriteLine("[{0}] Unknown packet received! " + packet.ToString(), Player.Character);

                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("---- ERROR ----\r\n{0}", ex.ToString()));
                Console.WriteLine(string.Format("Packet: {0}", packet.ToString()));
                FileWriter.WriteLine(@"etclog\ExceptionCatcher.log", "[Game Server " + Server.Instance.ID + "][" + DateTime.Now.ToString() + "] Exception caught: " + ex.Message + Environment.NewLine + Environment.NewLine + "Stacktrace: " + ex.StackTrace, true);
                Disconnect();
            }
        }

        public void OnChangeChannel(Packet packet)
        {
            byte channel = packet.ReadByte();
            Server.Instance.CenterConnection.RequestCharacterConnectToWorld(Player.SessionHash, Player.Character.ID, Server.Instance.WorldID, channel);
        }


        public void SendConnectToServer(int charid, byte[] IP, ushort port)
        {

            Packet pw = new Packet(0x0A);
            pw.WriteBool(true);
            pw.WriteBytes(IP);
            pw.WriteUShort(port);
            
            SendPacket(pw);
        }

        public void OnPlayerLoad(Packet packet)
        {
            int playerid = packet.ReadInt();

            if (Server.Instance.CharacterList.ContainsKey(playerid))
            {
                Console.WriteLine("Disconnected playerid " + playerid + ". Already connected.");
                Disconnect();
                return;
            }

            Character character = new Character(playerid);
            if (!character.Load())
            {
                Console.WriteLine("Disconnected playerid " + playerid + ". Unable to load.");
                character = null;
                Disconnect();
                return;
            }

            Player.Character = character;
            character.mPlayer = Player;
            //Player.Socket.Session.mID = playerid;
            Console.WriteLine(string.Format("[{0}] {1} connected!", DateTime.Now.ToString(), character.Name));
            //Program.MainForm.ChangeLoad(true);
            Server.Instance.CharacterList.Add(playerid, character);
            Loaded = true;

            //MapPacket.Test(character);
            MapPacket.SendJoinGame(character);
            //MapPacket.SendJoinGame(character);
            //character.ModifyMP(0);
            DataProvider.Maps[Player.Character.Map].AddPlayer(character);

            MessagePacket.SendScrollingHeader(Server.Instance.mScrollingHeader, character);

            if (character.Quests != null)
            {
                //MessagePacket.SendNotice("not null!", character);
                foreach (KeyValuePair<int, QuestData> kvp in character.Quests.mQuests)
                {
                    //MessagePacket.SendNotice(kvp.Value.Mobs.Count.ToString(), character);
                    if (kvp.Value.Mobs.Count > 0 && !kvp.Value.Complete)
                    {
                        //MessagePacket.SendNotice("not null!", character);
                        QuestPacket.SendQuestMobUpdate(character, (short)kvp.Value.QuestID);
                    }
                }
            }
            Server.Instance.CenterConnection.RegisterCharacter(character.ID, character.Name, character.PrimaryStats.Job, character.PrimaryStats.Level);
            character.Channel = Server.Instance.ID;

            //Server.Instance.CenterConnection.RequestBuddyListLoad(character.Name, false, character.PrimaryStats.BuddyListCapacity); //Sends a packet that request the buddylistload from the centerserver 
            character.IsConnected = true;
            Server.Instance.CenterConnection.PlayerUpdateMap(character);

            Server.Instance.CenterConnection.PlayerBuddyOperation(character, 0);
            Server.Instance.CenterConnection.PlayerBuddyOperation(character, 4, "", 0, (sbyte)Server.Instance.ID);
            //Server.Instance.CenterConnection.PlayerBuddyOperation(character, 2);

            //Load party Data
            if (character.PartyID != -1)
            {
                //MessagePacket.SendNotice(character.Channel.ToString(), character);
                Server.Instance.CenterConnection.PlayerPartyOperation(character, 7, character.PartyID, character.Channel);
                Server.Instance.CenterConnection.PlayerPartyOperation(character, 8, character.PartyID, 0, false);
                MapPacket.UpdatePartyMemberHP(character);
                MapPacket.ReceivePartyMemberHP(character);
            }

            character.Buffs.LoadBuffs();
            //so when a character logs out or d/c's, record log out time or w/e, then compare the log out time and the time it was due by time, add that to current time and there is your new long!
        }
    }
}