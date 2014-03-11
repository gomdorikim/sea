using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using System.Configuration;
using System.Windows.Forms;

namespace WvsBeta.Login
{
    public class CenterSocket : AbstractConnection
    {

        public CenterSocket()
            : base(Server.Instance.PrivateIP.ToString(), 8383)
        {

        }

        public override void OnDisconnect()
        {
            Console.WriteLine("Disconnected from the server");
            // release all connections
            Environment.Exit(0);
        }

        public override void OnHandshakeInbound(Packet pPacket)
        {
            Packet packet2 = new Packet(ISClientMessages.ServerRequestAllocation);
            packet2.WriteString(Server.Instance.Name);
            packet2.WriteString(Server.Instance.PublicIP.ToString());
            packet2.WriteUShort(Server.Instance.Port);
            packet2.WriteByte((byte)Server.Instance.Worlds.Count);
            foreach (KeyValuePair<byte, World> world in Server.Instance.Worlds)
            {
                packet2.WriteByte(world.Key);
                packet2.WriteString(world.Value.Name);
                packet2.WriteShort(world.Value.Channels);
                packet2.WriteString(world.Value.EventDescription);
            }
            SendPacket(packet2);

            Console.WriteLine("Connected to the CenterServer!");
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
                            player.Socket.connectToServer(charid, ip, port);
                        }

                        break;
                    }
                case ISServerMessages.PlayerRequestWorldLoadResult:
                    {
                        string session = packet.ReadString();
                        Player player = Server.Instance.GetPlayer(session);
                        if (player != null)
                        {
                            
                            Packet pack = new Packet(0x03);
                            pack.WriteByte(0);
                            player.Socket.SendPacket(pack);
                        }

                        break;
                    }
                case ISServerMessages.PlayerRequestChannelStatusResult:
                    {
                        
                        string session = packet.ReadString();
                        Player player = Server.Instance.GetPlayer(session);
                        if (player != null)
                        {
                            byte ans = packet.ReadByte();
                            if (ans != 0x00)
                            {
                                Packet pack = new Packet(0x05);
                                pack.WriteByte(ans);
                                player.Socket.SendPacket(pack);
                            }
                            else
                            {
                                Console.WriteLine("lol");
                                player.Socket.DoChannelSelect(packet.ReadByte());
                                //FileWriter.WriteLine("Logs\\derp.txt", string.Format("LOL" + HexEncoding.byteArrayToString(packet.ToArray())));
                                //Console.WriteLine("LOL" + HexEncoding.byteArrayToString(packet.ToArray()));

                            }
                        }

                        break;
                    }
                case ISServerMessages.PlayerRequestWorldListResult:
                    {
                        
                        string session = packet.ReadString();
                        Player player = Server.Instance.GetPlayer(session);
                        if (player != null)
                        {
                            Packet pack = new Packet(0x04);
                            pack.WriteBytes(packet.ReadLeftoverBytes());
                            
                            
                            
                            player.Socket.SendPacket(pack);

                        }

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

        public void RequestCharacterConnectToWorld(string Hash, int charid, byte world, byte channel)
        {
            Packet packet = new Packet(ISClientMessages.PlayerChangeServer);
            packet.WriteString(Hash);
            packet.WriteInt(charid);
            packet.WriteByte(world);
            packet.WriteByte(channel);
            packet.WriteBool(false);
            SendPacket(packet);
        }

        public void RequestCharacterGetWorldLoad(string Hash, byte world)
        {
            Packet packet = new Packet(ISClientMessages.PlayerRequestWorldLoad);
            packet.WriteString(Hash);
            packet.WriteByte(world);
            SendPacket(packet);
        }

        public void RequestCharacterIsChannelOnline(string Hash, byte world, byte channel, bool isAdmin, string ip)
        {
            Packet packet = new Packet(ISClientMessages.PlayerRequestChannelStatus);
            packet.WriteString(Hash);
            packet.WriteByte(world);
            packet.WriteByte(channel);
            packet.WriteBool(isAdmin);
            packet.WriteString(ip);
            SendPacket(packet);
        }

        public void RequestCharacterWorldList(string Hash)
        {
            Packet packet = new Packet(ISClientMessages.PlayerRequestWorldList);
            packet.WriteString(Hash);
            SendPacket(packet);
        }
    }
}
