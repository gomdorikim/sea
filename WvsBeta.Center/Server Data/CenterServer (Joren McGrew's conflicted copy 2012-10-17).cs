using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using WvsBeta.Database;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Center
{
    public enum LocalServerType
    {
        Login,
        World,
        Game,
        Shop,
        MapGen,
        Claim,
        ITC,
        Unk
    }


    public class LocalServer
    {
        public ushort Port { get; set; }
        public IPAddress PublicIP { get; set; }
        public IPAddress PrivateIP { get; set; }
        public string Name { get; set; }
        public LocalServerType Type { get; set; }
        public int Connections { get; set; }
        public bool Connected { get; set; }
        public LocalConnection Connection { get; set; }
        public byte WorldID { get; set; }
        public byte GameID { get; set; }

        public bool IsReallyUsed { get; set; }

        public LocalServer()
        {
            Connected = false;
            Connections = 0;
            IsReallyUsed = false;
        }
    }


    public class CenterServer
    {
        public static CenterServer Instance { get; private set; }

        public string ConfigName { get; private set; }

        public ushort Port { get; set; }
        public ushort AdminPort { get; set; }
        public Dictionary<string, LocalServer> LocalServers { get; set; }
        public Dictionary<byte, WorldServer> Worlds { get; set; }
        public ServerConnectionAcceptor ConnectionAcceptor { get; set; }
        public MySQL_Connection CharacterDatabase { get; set; }

        public List<Character> mOnlineCharacters { get; set; }
        public List<MessengerRoom> MessengerRooms { get; set; }

        public List<AdminSocket> AdminSockets { get; private set; }

        public void LogToLogfile(string what)
        {
            Program.LogFile.WriteLine(what);
        }

        public Character FindCharacter(string name, byte world) { return mOnlineCharacters.Find(c => c.WorldID == world && c.Name.ToLower() == name.ToLower()); }
        public Character inviteCharacter(string name) { return mOnlineCharacters.Find(c => c.Name.ToLower() == name.ToLower()); }
        public Character FindCharacter(int id) { return mOnlineCharacters.Find(c => c.ID == id); }

        public AdminAcceptor AdminAcceptor { get; private set; }

        public void AddCharacter(string name, int id, byte world, byte channel, short job, byte level)
        {
            Character chr = FindCharacter(name, world);
            if (chr == null)
            {
                chr = new Character();
                chr.Name = name;
                chr.ID = id;
                chr.WorldID = world;
                chr.ChannelID = channel;
                chr.isCCing = false;

                mOnlineCharacters.Add(chr);
            }
            else if (chr.isCCing)
            {
                chr.ChannelID = channel;
                chr.isCCing = false;
            }
            chr.Job = job;
            chr.Level = level;
        }



        public static void Init(string configFile)
        {
            Instance = new CenterServer()
            {
                ConfigName = configFile,
                mOnlineCharacters = new List<Character>(),
                LocalServers = new Dictionary<string, LocalServer>(),
                Worlds = new Dictionary<byte, WorldServer>(),
                AdminSockets = new List<AdminSocket>()
            };
            Instance.Load();
        }

        public void Load()
        {
            LoadConfig(Application.StartupPath + @"\..\DataSvr\" + ConfigName + ".img");
            Program.MainForm.appendToLog(" Database ", false);
            LoadDBConfig(Application.StartupPath + @"\..\DataSvr\Database.img");

            ConnectionAcceptor = new ServerConnectionAcceptor();
            AdminAcceptor = new AdminAcceptor();
        }

        private void LoadDBConfig(string configFile)
        {
            Program.MainForm.appendToLog("Reading Config File... ", false);
            ConfigReader reader = new ConfigReader(configFile);
            string Username = reader.getString("", "dbUsername");
            string Password = reader.getString("", "dbPassword");
            string Database = reader.getString("", "dbDatabase");
            string Host = reader.getString("", "dbHost");

            CharacterDatabase = new MySQL_Connection(MasterThread.Instance, Username, Password, Database, Host);
            Program.MainForm.appendToLog(" Done!", false);
        }


        private void LoadConfig(string configFile)
        {
            ConfigReader reader = new ConfigReader(configFile);
            Port = reader.getUShort("", "port");
            AdminPort = reader.getUShort("", "adminPort");

            List<string> LocalServerList = reader.getBlocksFromBlock("", 1);
            List<string> LocalServerListInList;
            LocalServer ls;
            LocalServerType lst = LocalServerType.Unk;
            foreach (string localServerName in LocalServerList)
            {
                switch (localServerName)
                {
                    case "login": lst = LocalServerType.Login; break;
                    case "game": lst = LocalServerType.Game; break;
                    case "shop": lst = LocalServerType.Shop; break;
                    case "mapgen": lst = LocalServerType.MapGen; break;
                    case "claim": lst = LocalServerType.Claim; break;
                    case "itc": lst = LocalServerType.ITC; break;
                    default: lst = LocalServerType.Unk; break;
                }

                if (lst == LocalServerType.Unk)
                {
                    System.Windows.Forms.MessageBox.Show("Found unparsable block in center config file: " + localServerName);
                }
                else if (lst != LocalServerType.Claim)
                {
                    LocalServerListInList = reader.getBlocks(localServerName, true);
                    foreach (string LocalServerName2 in LocalServerListInList)
                    {
                        ls = new LocalServer();
                        ls.Name = LocalServerName2;
                        ls.Port = reader.getUShort(LocalServerName2, "port");
                        ls.PublicIP = IPAddress.Parse(reader.getString(LocalServerName2, "PublicIP"));
                        ls.PrivateIP = IPAddress.Parse(reader.getString(LocalServerName2, "PrivateIP"));
                        ls.Type = lst;
                        LocalServers.Add(LocalServerName2, ls);
                    }
                }
                else
                {
                    string LocalServerName2 = "claim";
                    ls = new LocalServer();
                    ls.Name = LocalServerName2;
                    ls.Port = reader.getUShort(LocalServerName2, "port");
                    ls.PublicIP = IPAddress.Parse(reader.getString(LocalServerName2, "PublicIP"));
                    ls.PrivateIP = IPAddress.Parse(reader.getString(LocalServerName2, "PrivateIP"));
                    ls.Type = lst;
                    LocalServers.Add(LocalServerName2, ls);
                }
            }
            reader = null;
        }



        public void SendPacketToServer(Packet pPacket, byte pWorldID, byte pChannelID = 0xFF)
        {
            if (!Worlds.ContainsKey(pWorldID))
            {
                Program.MainForm.appendToLog("Cannot send packet (world not found)");
            }
            else
            {
                if (pChannelID == 0xFF)
                {
                    Worlds[pWorldID].SendPacketToEveryGameserver(pPacket);
                }
                else
                {
                    if (!Worlds[pWorldID].GameServers.ContainsKey(pChannelID))
                    {
                        Program.MainForm.appendToLog("Cannot send packet (channel not found: " + pChannelID + ")");
                    }
                    else if (!Worlds[pWorldID].GameServers[pChannelID].Connected)
                    {
                        Program.MainForm.appendToLog("Cannot send packet (channel offline: " + pChannelID + ")");
                    }
                    else
                    {
                        Worlds[pWorldID].GameServers[pChannelID].Connection.SendPacket(pPacket);
                    }
                }
            }
        }






        public MessengerRoom GetMessengerRoomByRoomID(int roomid)
        {
            foreach (MessengerRoom room in MessengerRooms)
                if (room.roomID == roomid)
                    return room;
            return null;
        }

        public Character GetCharacterByCID(int cid)
        {
            foreach (Character chr in mOnlineCharacters)
                if (chr != null && chr.ID == cid)
                    return chr;
            return null;
        }

        public Character GetCharacterByName(string name)
        {
            foreach (Character chr in mOnlineCharacters)
                if (chr != null && chr.Name == name)
                    return chr;
            return null;
        }

        public MessengerRoom GetMessengerRoomByCID(int cid)
        {
            foreach (MessengerRoom room in MessengerRooms)
                foreach (Character ch in room.roomSlots.Values)
                    if (ch != null && ch.ID == cid)
                        return room;
            return null;
        }

        public MessengerRoom GetMessengerRoomByName(string name)
        {
            foreach (MessengerRoom room in MessengerRooms)
                foreach (Character ch in room.roomSlots.Values)
                    if (ch != null && ch.Name == name)
                        return room;
            return null;
        }


        public int FirstAvailableMessengerRoom()
        {
            int i = 1;
            foreach (MessengerRoom room in MessengerRooms)
                if (room.roomID == i)
                    i++;
                else
                    return room.roomID;
            return 1;
        }

    }
}