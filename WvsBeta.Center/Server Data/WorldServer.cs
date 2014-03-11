using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;


namespace WvsBeta.Center
{
    public class WorldServer
    {
        public byte ID { get; set; }
        public string Name { get; set; }
        public short Channels { get; set; }
        public string EventDescription { get; set; }
        public bool AdultWorld { get; set; }
        public Dictionary<byte, LocalServer> GameServers { get; set; }
        public LocalServer ShopServer { get; set; }

        public WorldServer(byte id)
        {
            GameServers = new Dictionary<byte, LocalServer>();
            ID = id;
            ShopServer = null;
        }

        public void SendPacketToEveryGameserver(Packet packet)
        {
            foreach (KeyValuePair<byte, LocalServer> kvp in GameServers)
                if (kvp.Value.Connected)
                    kvp.Value.Connection.SendPacket(packet);
        }

        public void SendNotice(string what)
        {
            Packet pw = new Packet(0x23);
            pw.WriteByte(0x00);
            pw.WriteString(what);
            foreach (KeyValuePair<byte, LocalServer> kvp in GameServers)
                kvp.Value.Connection.SendPacket(pw);
        }


        public byte getFreeGameServerSlot()
        {
            for (byte i = 0; i < Channels; i++)
            {
                if (GameServers.ContainsKey(i))
                {
                    continue;
                }
                return i;
            }
            return 0xff;
        }

        public int CalculateWorldLoad()
        {
            int load = 0;
            foreach (KeyValuePair<byte, LocalServer> gs in GameServers)
            {
                load += gs.Value.Connections;
            }
            return load;
        }

        public void AddWarning(Packet pw)
        {
            int load = CalculateWorldLoad();
            if (load > 400) pw.WriteByte(2);
            else if (load > 100) pw.WriteByte(1);
            else pw.WriteByte(0);
        }
    }
}
