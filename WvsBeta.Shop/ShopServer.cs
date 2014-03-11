using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using System.CodeDom.Compiler;

using WvsBeta.Database;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace WvsBeta.Shop
{
    class Server
    {
        public static Server Instance { get; private set; }

        public string Name { get; private set; }
        public string WorldName { get; private set; }
        public byte WorldID { get; private set; }

        public int CenterPort { get; private set; }
        public IPAddress CenterIP { get; private set; }

        public ushort Port { get; private set; }
        public IPAddress PublicIP { get; private set; }
        public IPAddress PrivateIP { get; private set; }

        public CenterSocket CenterConnection { get; private set; }

        private ShopAcceptor ShopAcceptor { get; set; }
        public MySQL_Connection CharacterDatabase { get; private set; }

        public Dictionary<string, Player> PlayerList { get; private set; }
        public Dictionary<int, Character> CharacterList { get; private set; }



        public string ScrollingHeader { get; set; }

        public void LogToLogfile(string what)
        {
            Program.LogFile.WriteLine(what);
        }

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

        public Character GetCharacter(int ID)
        {
            return CharacterList.ContainsKey(ID) ? CharacterList[ID] : null;
        }

        public Character GetCharacter(string name)
        {
            name = name.ToLowerInvariant();
            foreach (KeyValuePair<int, Character> kvp in CharacterList)
            {
                if (kvp.Value != null && kvp.Value.mName.ToLowerInvariant() == name)
                {
                    return kvp.Value;
                }
            }
            return null;
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
                CharacterList = new Dictionary<int, Character>(),
                PlayerList = new Dictionary<string, Player>(),
                Name = configFile
            };
            Instance.Load();
        }

        public void Load()
        {
            LoadConfig(Application.StartupPath + @"\..\DataSvr\" + Name + ".img");
            LoadDBConfig(Application.StartupPath + @"\..\DataSvr\Database.img");

            // mScrollingHeader = "The Cash Shop is now available! Go to the OriginalMaple Website to get your NX Cash today! Beware, some items are not available and are not working yet, but we're working on that and will get them listed and working as soon as possible! Happy shopping!";

            CenterConnection = new CenterSocket();

            ShopAcceptor = new ShopAcceptor();
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
            WorldID = reader.getByte("", "gameWorldId");
            PublicIP = IPAddress.Parse(reader.getString("", "PublicIP"));
            PrivateIP = IPAddress.Parse(reader.getString("", "PrivateIP"));

            CenterIP = IPAddress.Parse(reader.getString("center", "ip"));
            CenterPort = reader.getUShort("center", "port");
            WorldName = reader.getString("center", "worldName");


            string tmpHeader = reader.getString("", "scrollingHeader");
            if (tmpHeader == "")
            {
                tmpHeader = "Welcome to MapleTespia Alpha Testing! The current known bugs/broken features in the server are: Buddies -- Trade -- Parties -- Pets -- Rings -- Minigames -- Player Shops -- some skills here and there -- and there are some Portals that aren't correct and lead to the incorrect other Portal. As of right now, nothing is known to cause a crash. So if you find anything that is bugged or broken, please let us know by reporting that in a detailed manner via our website. We will be fixing these features as we progress. Please tell us what you think about the server! We love to hear feedback from our players! The current GMs in this server are NafN,e. Try whispering to one of us when we're online! Remember, we will not tolerate any hacking. Hope you all enjoy Alpha Testing!";
            }
            else if (tmpHeader == "EMPTY")
            {
                tmpHeader = "";
            }

            ScrollingHeader = tmpHeader;
        }

        public long GetNewCashSerial()
        {
            long ret = 0;
            using (MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT last_cash_serial FROM inc_table") as MySqlDataReader)
            {
                data.Read();
                ret = data.GetInt64(0);
            }
            Server.Instance.CharacterDatabase.RunQuery("UPDATE inc_table SET last_cash_serial = last_cash_serial + 1"); // Update next ID
            return ret;
        }

    }
}