using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using System.Configuration;
using System.Windows.Forms;

namespace WvsBeta.Shop
{
    public class CenterSocket : AbstractConnection
    {
        public CenterSocket()
            : base(Server.Instance.CenterIP.ToString(), (ushort)Server.Instance.CenterPort)
        {

        }

        public override void OnDisconnect()
        {
            Program.MainForm.appendToLog("Disconnected from the server");
            // release all connections
            Application.Exit();
        }

        public override void OnHandshakeInbound(Packet pPacket)
        {
            Packet packet2 = new Packet(ISClientMessages.ServerRequestAllocation);
            packet2.WriteString(Server.Instance.Name);
            packet2.WriteString(Server.Instance.PublicIP.ToString());
            packet2.WriteUShort(Server.Instance.Port);
            packet2.WriteByte(Server.Instance.WorldID);
            packet2.WriteString(Server.Instance.WorldName);
            SendPacket(packet2);
        }

        public override void AC_OnPacketInbound(Packet packet)
        {
            switch ((ISServerMessages)packet.ReadByte())
            {
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
                                player.Socket.Disconnect();
                            }
                            else
                            {
                                player.Character.Save();
                                player.SaveOnDisconnect = false;
                                player.Socket.connectToServer(charid, ip, port);
                            }
                        }

                        break;
                    }
                case ISServerMessages.ServerAssignmentResult:
                    {
                        int amount = (int)Server.Instance.CharacterDatabase.RunQuery("UPDATE users INNER JOIN characters ON users.id = characters.userid SET users.online = 0, characters.online = 0 WHERE users.online = " + ((int)(20000 + (int)Server.Instance.WorldID * 100 + 50)));
                        Program.MainForm.appendToLog(string.Format("Handling as Shop Server on World {0} ({1})", Server.Instance.WorldID, Server.Instance.WorldName));
                        Program.MainForm.appendToLog(string.Format("Freed {0} players.", amount));
                        break;
                    }
                default: break;
            }
        }

        public void updateConnections(int value)
        {
            Packet packet = new Packet(ISClientMessages.ServerSetConnectionsValue);
            packet.WriteInt(value);
            SendPacket(packet);
        }

        public void RequestCharacterConnectToWorld(string Hash, int charid, byte world)
        {
            Packet packet = new Packet(ISClientMessages.PlayerQuitCashShop);
            packet.WriteString(Hash);
            packet.WriteInt(charid);
            packet.WriteByte(world);
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
    }
}