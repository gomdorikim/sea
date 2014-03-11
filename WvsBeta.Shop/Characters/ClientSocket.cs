using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Shop
{
    public class ClientSocket : AbstractConnection
    {
        public Player Player { get; set; }
        public bool Loaded { get; set; }

        public ClientSocket(System.Net.Sockets.Socket pSocket)
            : base(pSocket)
        {
            Loaded = false;

        }

        public void Init()
        {
            Pinger.Connections.Add(this);

            Player = new Player();
            Player.Socket = this;
            Player.Character = null;

            Server.Instance.AddPlayer(Player);

            SendHandshake(15, "", 0x07);
           
        }

        public override void OnDisconnect()
        {
            try
            {
                if (Player != null && Loaded)
                {
                    if (Player.Character != null)
                    {
                        Console.WriteLine("[{0}] {1} disconnected from Server.", DateTime.Now.ToString(), Player.Character.mName);

                        Server.Instance.CenterConnection.UnregisterCharacter(Player.Character.mID, Player.SaveOnDisconnect == false);
                        Server.Instance.CharacterList.Remove(Player.Character.mID);
                        if (Player.SaveOnDisconnect)
                        {
                            Player.Character.Save();
                        }
                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users INNER JOIN characters ON users.id = characters.userid SET users.online = 0, characters.online = 0 WHERE characters.id = " + Player.Character.mID.ToString());
                        Program.MainForm.changeLoad(false);
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
                if (!Loaded)
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

                        case 0x0E: break; // Kernel32.dll errors :S i dont even know lol
                        case 0x13: Server.Instance.CenterConnection.RequestCharacterConnectToWorld(Player.SessionHash, Player.Character.mID, Server.Instance.WorldID); break;
                        case 0x51: break; //pet move lol
                        //case 0x73: MessagePacket.SendCharge(Player.Character); break;
                        case 0x10:
                            {
                                Program.MainForm.appendToLog("case 10!!");
                                //EnableCSUse(Player.Character);
                                break;
                            }
                        //case 0x7D: Player.Character.mStorage.LoadNXValues(); CashPacket.SendCashAmounts(Player.Character); break;
                        case 0x7E:
                            {
                                try
                                {
                                    CashPacket.HandleCashPacket(Player.Character, packet);
                                }
                                catch (Exception ex)
                                {
                                    Program.MainForm.appendToLog(ex.ToString());
                                }
                            }
                            break;
                        //case 0x76: CouponHandler.HandleCoupon(Player.Character, packet); break;
                        //case 0x09: break; // PONG
                        //case 0x0B: break; // Some hash thing, 1 integer....?
                        default:
                            {
                                string what = "[GS][" + Player.Character.mID.ToString() + "][" + DateTime.Now.ToString() + "] Unknown packet found: ";
                                foreach (byte bit in packet.ToArray())
                                {
                                    what += string.Format("{0:X2} ", bit);
                                }
                                //FileWriter.WriteLine(@"connection_log\" + Player.Character.mID.ToString() + ".txt", what, true);
                                Program.MainForm.appendToLog(what);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                FileWriter.WriteLine(@"etclog\ExceptionCatcher.log", "[Shop Server][" + DateTime.Now.ToString() + "] Exception caught: " + ex.Message + Environment.NewLine + Environment.NewLine + "Stacktrace: " + ex.StackTrace, true);
                Disconnect();
            }
        }

        public void connectToServer(int charid, byte[] IP, ushort port)
        {
            Program.MainForm.appendToLog("connect to server");
            Packet pw = new Packet(0x0A);
            pw.WriteBool(true);
            pw.WriteBytes(IP);
            pw.WriteUShort(port);
            SendPacket(pw);
        }

        public void PingTest(Character chr)
        {
            Packet pw = new Packet(0x0B);
            pw.WriteByte(0);
            chr.sendPacket(pw);
        }

        public static void EnableCSUse(Character chr)
        {
            //To dispose the client I guess ? 
            Packet pw = new Packet(0x0E);
            pw.WriteByte(0x01);
            pw.WriteInt(0);
            chr.sendPacket(pw);
        }
        public void OnPlayerLoad(Packet packet)
        {
            int playerid = packet.ReadInt();

            if (Server.Instance.CharacterList.ContainsKey(playerid))
            {
                Program.MainForm.appendToLog(string.Format("Player tried to login while already being loggedin. Playerid: {0}", playerid));
                Console.WriteLine("Disconnected session 12");
                Disconnect();
                return;
            }

            Character character = new Character(playerid);
            if (!character.Load())
            {
                Program.MainForm.appendToLog(string.Format("Player tried to login, but we failed loading the char! Playerid: {0}", playerid));
                Console.WriteLine("Disconnected session 5");
                Disconnect();
                character = null;
                return;
            }

            Player.Character = character;
            character.mPlayer = Player;
            //Player.Socket.Session.mID = playerid;
            Program.MainForm.appendToLog(string.Format("[{0}] {1} connected!", DateTime.Now.ToString(), character.mName));
            //Program.MainForm.changeLoad(true);
            Server.Instance.CharacterList.Add(playerid, character);
            Loaded = true;

            MapPacket.SendJoinCashServer(character);
            //PingTest(character);
            CashPacket.SendInfo(character);
            //EnableCSUse(character);

            Server.Instance.CenterConnection.RegisterCharacter(playerid, character.mName, character.mPrimaryStats.Job, character.mPrimaryStats.Level);

           // MessagePacket.SendScrollingHeader(Server.Instance.ScrollingHeader, character);
        }
    }
}