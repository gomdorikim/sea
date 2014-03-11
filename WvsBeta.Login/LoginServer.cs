using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

using WvsBeta.Database;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Login
{
    public struct World
    {
        public ushort Port { get; set; }
        public IPAddress IP { get; set; }
        public byte ID { get; set; }
        public short Channels { get; set; }
        public byte State { get; set; }
        public string EventDescription { get; set; }
        public bool BlockCharacterCreation { get; set; }
        public bool AdultWorld { get; set; }
        public string Name { get; set; }
    }

    public class Server
    {
        public static Server Instance { get; private set; }

		public ushort Port { get; set; }
		public ushort AdminPort { get; set; }
		public IPAddress PublicIP { get; set; }
		public IPAddress PrivateIP { get; set; }
		public Dictionary<byte, World> Worlds = new Dictionary<byte, World>();
		public string Name { get; set; }
		public CenterSocket CenterConnection { get; set; }
		private LoginAcceptor LoginAcceptor { get; set; }
		public MySQL_Connection CharacterDatabase { get; set; }

		private Dictionary<string, Player> PlayerList { get; set; }

		private string _mLogFile;

		public string mLogFile {
			get {
				return _mLogFile;
			}
			set {
				_mLogFile = "Logs\\" + value + "_" + DateTime.Now.ToString("MMddyyyy_HHmmsss") + ".log";
			}
		}

		public void LogToLogfile(string what) {
            Program.LogFile.Write(what);
		}

        Random rnd = new Random();
		public void AddPlayer(Player player)
        {
            string hash = Cryptos.GetNewSessionHash();
            while (PlayerList.ContainsKey(hash))
            {
                hash = Cryptos.GetNewSessionHash();
            }
            PlayerList.Add(hash, player);
            player.SessionHash = hash;
        }

		public void RemovePlayer(string hash)
        {
            PlayerList.Remove(hash);
        }

		public bool IsPlayer(string hash)
        {
            return PlayerList.ContainsKey(hash);
        }

		public Player GetPlayer(string hash)
        {
            return PlayerList[hash];
        }

		public static void Init(string configFile)
        {
            Instance = new Server()
            {
                PlayerList = new Dictionary<string, Player>(),
                Name = configFile
            };
            Instance.Load();
        }

        public void Load()
        {
            LoadConfig(Application.StartupPath + @"\..\DataSvr\" + Name + ".img");
            LoadDBConfig(Application.StartupPath + @"\..\DataSvr\Database.img");

            CenterConnection = new CenterSocket();

            LoginAcceptor = new LoginAcceptor();
            //Console.WriteLine("new login acceptor");
            //Console.Read();
        }

		private void LoadDBConfig(string configFile)
        {
            ConfigReader reader = new ConfigReader(configFile);
            string Username = reader.getString("", "dbUsername");
            string Password = reader.getString("", "dbPassword");
            string Database = reader.getString("", "dbDatabase");
            string Host = reader.getString("", "dbHost");

            CharacterDatabase = new MySQL_Connection(MasterThread.Instance, Username, Password, Database, Host);
        }

		private void LoadConfig(string configFile)
        {
            ConfigReader reader = new ConfigReader(configFile);
            Port = reader.getUShort("", "port");
            AdminPort = reader.getUShort("", "adminPort");
            PublicIP = IPAddress.Parse(reader.getString("", "PublicIP"));
            PrivateIP = IPAddress.Parse(reader.getString("", "PrivateIP"));
            List<string> worldNames = reader.getBlocks("center", true);
            World world;
            foreach (string worldName in worldNames)
            {
                world = new World();
                world.Channels = reader.getShort(worldName, "channelNo");
                world.ID = reader.getByte(worldName, "world");
                world.Port = reader.getUShort(worldName, "port");
                world.IP = IPAddress.Parse(reader.getString(worldName, "ip"));
                world.EventDescription = reader.getString(worldName, "eventDesc");
                world.AdultWorld = reader.getBool(worldName, "adult");
                world.BlockCharacterCreation = reader.getBool(worldName, "BlockCharCreation");
                world.State = reader.getByte(worldName, "worldState");
                world.Name = worldName;

                Worlds.Add(world.ID, world);
            }
        }

        public static int[] BeginnerEyes = new int[] { 20000, 20001, 20002, 21000, 21001, 21002, 20100, 20401, 20402, 21700, 21201, 21002 };
        public static int[] BeginnerHair = new int[] { 30000, 30020, 30030, 31000, 31040, 31050 };
        public static int[] BeginnerHairColor = new int[] { 0, 7, 3, 2 };
        public static int[] BeginnerBottom = new int[] { 1060002, 1060006, 1061002, 1061008, 1062115 };
        public static int[] BeginnerTop = new int[] { 1040002, 1040006, 1040010, 1041002, 1041006, 1041010, 1041011, 1042167 };
        public static int[] BeginnerShoes = new int[] { 1072001, 1072005, 1072037, 1072038, 1072383 };
        public static int[] BeginnerWeapons = new int[] { 1302000, 1322005, 1312004, 1442079 };
        public static int[] BeginnerSkinColor = new int[] { 0, 1, 2, 3 };
        
    }
}
