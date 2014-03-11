using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class ClientSocket : AbstractConnection
    {
        public Player Player { get; set; }
        public bool Loaded { get; set; }


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
            SendHandshake(40, "", 0x05);
        }

        public override void OnDisconnect()
        {
            try
            {
                if (Player != null && Loaded)
                {
                    if (Player.Character != null)
                    {
                        Program.MainForm.LogAppendFormat("[{0}] {1} disconnected from Server.Instance.", DateTime.Now.ToString(), Player.Character.Name);


                        if (Player.Character.MapChair != -1)
                        {
                            Map map = DataProvider.Maps[Player.Character.Map];
                            map.UsedSeats.Remove(Player.Character.MapChair);
                            Player.Character.MapChair = -1;
                            MapPacket.SendCharacterSit(Player.Character, -1);
                        }

                        DataProvider.Maps[Player.Character.Map].RemovePlayer(Player.Character);
                        Server.Instance.CharacterList.Remove(Player.Character.ID);
                        if (Player.SaveOnDisconnect)
                        {
                            Player.Character.Save();
                        }
                        Server.Instance.CenterConnection.UnregisterCharacter(Player.Character.ID, Player.SaveOnDisconnect == false);
                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users INNER JOIN characters ON users.id = characters.userid SET users.online = 0, characters.online = 0 WHERE characters.id = " + Player.Character.ID.ToString());

                        Program.MainForm.ChangeLoad(false);
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
                Program.MainForm.LogAppendFormat(ex.ToString());
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
                        case 0x05: OnPlayerLoad(packet); break;
                    }
                }
                else
                {
                    switch (header)
                    {
                        case 0x11: MapPacket.OnEnterPortal(packet, Player.Character); break;
                        case 0x12: OnChangeChannel(packet); break;
                        case 0x13: Server.Instance.CenterConnection.RequestCharacterConnectToWorld(Player.SessionHash, Player.Character.ID, Server.Instance.WorldID, 50); break;
                        case 0x14: MapPacket.HandleMove(Player.Character, packet); break;
                        case 0x15: MapPacket.HandleSitChair(Player.Character, packet); break;

                        case 0x16: AttackPacket.HandleMeleeAttack(Player.Character, packet); break;
                        case 0x17: AttackPacket.HandleRangedAttack(Player.Character, packet); break;
                        case 0x18: AttackPacket.HandleMagicAttack(Player.Character, packet); break;

                        case 0x1A: CharacterStatsPacket.HandleCharacterDamage(Player.Character, packet); break;
                        case 0x1B: MessagePacket.HandleChat(Player.Character, packet); break;
                        case 0x1C: MapPacket.SendEmotion(Player.Character, packet.ReadInt()); break;

                        case 0x1F: MapPacket.HandleNPCChat(Player.Character, packet); break;
                        case 0x20: NpcPacket.HandleNPCChat(Player.Character, packet); break;
                        case 0x21: NpcPacket.HandleNPCShop(Player.Character, packet); break;
                        case 0x22: StoragePacket.HandleStorage(Player.Character, packet); break;

                        case 0x23: InventoryPacket.HandleInventoryPacket(Player.Character, packet); break;
                        case 0x24: InventoryPacket.HandleUseItemPacket(Player.Character, packet); break;
                        case 0x25: InventoryPacket.HandleUseSummonSack(Player.Character, packet); break;
                        case 0x27: CashPacket.HandleCashItem(Player.Character, packet); break;
                        case 0x28: InventoryPacket.HandleUseReturnScroll(Player.Character, packet); break;
                        case 0x29: InventoryPacket.HandleScrollItem(Player.Character, packet); break;

                        case 0x2A: CharacterStatsPacket.HandleStats(Player.Character, packet); break;
                        case 0x2B: CharacterStatsPacket.HandleHeal(Player.Character, packet); break;

                        case 0x2C: SkillPacket.HandleAddSkillLevel(Player.Character, packet); break;
                        case 0x2D: SkillPacket.HandleUseSkill(Player.Character, packet); break;
                        case 0x2E: SkillPacket.HandleStopSkill(Player.Character, packet); break;

                        case 0x30: DropPacket.HandleDropMesos(Player.Character, packet.ReadInt()); break;
                        case 0x31: FamePacket.HandleFame(Player.Character, packet); break;
                        case 0x33: MapPacket.SendPlayerInfo(Player.Character, packet); break;
                        case 0x34: PetsPacket.HandleSpawnPet(Player.Character, packet.ReadShort()); break;

                        case 0x37: CashPacket.HandleTeleRockFunction(Player.Character, packet); break;
                        case 0x38: ReportPacket.HandleReport(Player.Character, packet); break;
                        case 0x3C: MessagePacket.HandleCommand(Player.Character, packet); break;
                        case 0x3E: PlayerInteractionPacket.HandleInteraction(Player.Character, packet); break;
                        //case 0x3D: CUIMessengerPacket.OnPacket(Player.Character, packet); break;
                        case 0x3F: PartyPacket.HandleCreateParty(Player.Character, packet); break;

                        case 0x41: CommandHandling.HandleAdminCommand(Player.Character, packet); break;
                        case 0x42: CommandHandling.HandleAdminCommandLog(Player.Character, packet); break;
                        case 0x43: BuddyPacket.HandleBuddy(Player.Character, packet); break;
                        case 0x48: PetsPacket.HandleMovePet(Player.Character, packet); break;
                        case 0x4A: PetsPacket.HandleAction(Player.Character, packet); break;
                        case 0x4B: PetsPacket.HandlePetLoot(Player.Character, packet); break;

                        case 0x4E: SummonPacket.HandleSummonMove(Player.Character, packet); break;
                        case 0x4F: AttackPacket.HandleSummonAttack(Player.Character, packet); break;
                        case 0x50: SummonPacket.HandleSummonDamage(Player.Character, packet); break;

                        case 0x56: MobPacket.HandleMobControl(Player.Character, packet); break;
                        case 0x57: MobPacket.HandleDistanceFromBoss(Player.Character, packet); break;
                        case 0x58: MobPacket.HandleMobLootDrop(Player.Character, packet); break;
                        case 0x5B: MapPacket.HandleNPCAnimation(Player.Character, packet); break;
                        case 0x5F: DropPacket.HandlePickupDrop(Player.Character, packet); break;
                        case 0x68: CoconutPackets.HandleEvent(Player.Character, packet); break;
                        case 0x70: MapPacket.HandleBoatStatusRequest(Player.Character, packet); break;
                        case 0x09: if (Player.Character.Buffs != null) Player.Character.Buffs.CheckExpired(MasterThread.CurrentDate); break; // PONG
                        case 0x0E: break; // Some hash thing, 1 integer....?
                        default:
                            {
                                string what = "[" + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString("D3") + "] Unknown packet found: " + packet.ToString();
                                FileWriter.WriteLine(@"connection_log\" + Player.Character.ID.ToString() + ".txt", what, true);
                                if (Player.Character.Admin)
                                {
                                    MessagePacket.SendNotice("Unknown packet received! " + packet.ToString(), Player.Character);
                                }
                                Program.MainForm.LogAppendFormat("[{0}] Unknown packet received! " + packet.ToString(), Player.Character);

                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.MainForm.LogAppend(string.Format("---- ERROR ----\r\n{0}", ex.ToString()));
                Program.MainForm.LogAppend(string.Format("Packet: {0}", packet.ToString()));
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
            Packet pw = new Packet(0x09);
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
                Program.MainForm.LogAppend("Disconnected playerid " + playerid + ". Already connected.");
                Disconnect();
                return;
            }

            Character character = new Character(playerid);
            if (!character.Load())
            {
                Program.MainForm.LogAppend("Disconnected playerid " + playerid + ". Unable to load.");
                character = null;
                Disconnect();
                return;
            }

            Player.Character = character;
            character.mPlayer = Player;
            //Player.Socket.Session.mID = playerid;
            Program.MainForm.LogAppend(string.Format("[{0}] {1} connected!", DateTime.Now.ToString(), character.Name));
            Program.MainForm.ChangeLoad(true);
            Server.Instance.CharacterList.Add(playerid, character);
            Loaded = true;


            MapPacket.SendJoinGame(character);

            DataProvider.Maps[Player.Character.Map].AddPlayer(character);

            MessagePacket.SendScrollingHeader(Server.Instance.mScrollingHeader, character);
            Server.Instance.CenterConnection.RegisterCharacter(character.ID, character.Name, character.PrimaryStats.Job, character.PrimaryStats.Level);
        }
    }
}