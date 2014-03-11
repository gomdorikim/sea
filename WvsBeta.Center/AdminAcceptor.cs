using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Center
{
    public class AdminAcceptor : Acceptor
    {
        public AdminAcceptor() :
            base(CenterServer.Instance.AdminPort)
        {

        }

        public override void OnAccept(System.Net.Sockets.Socket pSocket)
        {
            new AdminSocket(pSocket);
        }
    }

    public class AdminSocket : AbstractConnection
    {
        public AdminSocket(System.Net.Sockets.Socket pSocket)
            : base(pSocket)
        {
            Pinger.Connections.Add(this);
            SendHandshake(10, "WvsBeta Center Server", 0xF8);
        }

        bool loggedin = false;

        public override void AC_OnPacketInbound(Packet pPacket)
        {
            byte header = pPacket.ReadByte();
            if (!loggedin)
            {
                if (pPacket.ReadString() == "0812dewfnm012bngpinhe3rohjigvnjgv;lhkgrlw0gt0w3gvij3ibtu30wvctn0ujv30uvhj32j0tixjw5loa8p9sa8p9sa8p9sa8p9sa8p9sa8p9sa8p9sa8p9sg[eu89wsgv/lon y qy 8qwe2tr5q789w2g5b;b    t 34tq3btq3480 tq0r q2nrq0rhq8092ggggggbvfgutoq3489yh33y6249hui4obnOABGVTFE9R5AQ3489WEBGLOUB8EFPWE;weghogfr89ewfrhewh")
                {
                    CenterServer.Instance.AdminSockets.Add(this);
                    Console.WriteLine("Admin Socket connected from " + IP);
                    SendServers();
                    loggedin = true;
                }
                else
                {
                    Console.WriteLine("Uncertain AdminSocket connected from " + IP);
                    Disconnect();
                }
            }
            else
            {
                switch (header)
                {
                    case 1:
                        {
                            // Apply rates to server
                            string name = pPacket.ReadString();
                            double mobexprate = pPacket.ReadDouble();
                            double mesosamountrate = pPacket.ReadDouble();
                            double dropchancerate = pPacket.ReadDouble();

                            if (CenterServer.Instance.LocalServers.ContainsKey(name))
                            {
                                LocalServer ls = CenterServer.Instance.LocalServers[name];
                                ls.RateMobEXP = mobexprate;
                                ls.RateDropChance = dropchancerate;
                                ls.RateMesoAmount = mesosamountrate;
                                Console.WriteLine(string.Format("Changing rates of {0}: {1}; {2}; {3}", name, mobexprate, mesosamountrate, dropchancerate));
                                if (ls.Connected)
                                {
                                    ls.Connection.SendRates();
                                }
                            }

                            break;
                        }
                    case 2:
                        {
                            SendServers();
                            //Program.MainForm.updateServerList();
                            break;
                        }
                }
            }
        }

        public override void OnDisconnect()
        {
            CenterServer.Instance.AdminSockets.Remove(this);
            Pinger.Connections.Remove(this);
            Console.WriteLine("Admin Socket DISCONNECTED (from " + IP + ")");
        }

        public void SendServers()
        {
            Packet packet = new Packet((byte)0);
            packet.WriteShort((short)CenterServer.Instance.LocalServers.Count);

            foreach (KeyValuePair<string, LocalServer> Server in CenterServer.Instance.LocalServers)
            {
                packet.WriteString(Server.Key);
                packet.WriteInt(Server.Value.Connections);
                packet.WriteDouble(Server.Value.RateMobEXP);
                packet.WriteDouble(Server.Value.RateMesoAmount);
                packet.WriteDouble(Server.Value.RateDropChance);
            }
            SendPacket(packet);
        }
    }
}
