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

namespace WvsBeta.Game
{
    class Server
    {
        public static Server Instance { get; private set; }

        public CharacterRandom Randomizer { get; set; }
        public LoopingID MiniRoomIDs { get; set; }
        public LoopingID PartyIDs { get; set; }
        public LoopingID MatchCardIDs { get; set; }
        
        public double RateMobEXP = 1.0d;
        public double RateMesoAmount = 1.0d;
        public double RateDropChance = 1.0d;

        public byte ID { get; set; }
        public string Name { get; set; }
        public string WorldName { get; set; }
        public byte WorldID { get; set; }
        public bool AdultWorld { get; set; }
        public IPAddress MasterIP { get; set; }

        public int CenterPort { get; set; }
        public IPAddress CenterIP { get; set; }

        public ushort Port { get; set; }
        public IPAddress PublicIP { get; set; }
        public IPAddress PrivateIP { get; set; }

        public CenterSocket CenterConnection { get; set; }

        public bool Initialized { get; private set; }

        private GameAcceptor GameAcceptor { get; set; }
        private MySQL_Connection cd;
        public MySQL_Connection CharacterDatabase
        {
            get
            {
                if (cd == null) cd = new MySQL_Connection(MasterThread.Instance, connectstring[0], connectstring[1], connectstring[2], connectstring[3]);
                return cd;

            }
            set { cd = value; }
        }
        public Dictionary<string, int> HackingAutoBlockValues { get; set; } // {Type, amount of months}
        public List<int> LoggedCharacters { get; set; } // {charid}

        public Dictionary<string, Player> PlayerList { get; set; }
        public Dictionary<int, Character> CharacterList { get; set; }
        public static List<Party> Party { get; set; }
        public Dictionary<int, Character> Members { get; set; }
        public List<Character> mUsers { get; set; }


        public Dictionary<string, INpcScript> mAvailableNPCScripts { get; set; }

        public string mScrollingHeader { get; set; }

        private string[] connectstring { get; set; }

        public void LogToLogfile(string what)
        {
            Program.LogFile.WriteLine(what);
        }

        public void CheckMaps(DateTime pNow)
        {
            foreach (KeyValuePair<int, Map> kvp in DataProvider.Maps)
            {
                kvp.Value.MapTimer(pNow);
            }
        }

        public void MakeAvailableScript(Character chr, string name)
        {
            string filename = Application.StartupPath + @"\..\DataSvr\Scripts\" + name + ".s";
            if (!File.Exists(filename)) filename = Application.StartupPath + @"\..\DataSvr\Scripts\" + name + ".cs";
            if (File.Exists(filename))
            {
                try
                {
                    CompilerResults results;
                    FileInfo fi = new FileInfo(filename);
                    results = Scripting.CompileScript(filename);
                    if (results.Errors.Count > 0)
                    {
                        if (chr != null)
                        {
                            MessagePacket.SendNotice("Couldn't compile the file (" + fi.Name + ") correctly:", chr);
                            foreach (CompilerError error in results.Errors)
                            {
                                MessagePacket.SendNotice("Line " + error.Line.ToString() + ", Column " + error.Column.ToString() + ": " + error.ErrorText, chr);
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Couldn't compile the file ({0}) correctly:", filename));
                            foreach (CompilerError error in results.Errors)
                            {
                                Console.WriteLine(string.Format("Line {0}, Column {1}: {2}", error.Line, error.Column, error.ErrorText), true);
                            }
                        }
                    }
                    else
                    {
                        if (chr != null)
                        {
                            MessagePacket.SendNotice("Compiled " + fi.Name + "!", chr);
                        }
                        else
                        {
                            Console.WriteLine("Compiled {0}!", fi.Name);
                        }
                        string savename = fi.Name.Replace(".s", "").Replace(".cs", "");
                        if (mAvailableNPCScripts.ContainsKey(savename))
                        {
                            mAvailableNPCScripts[savename] = (INpcScript)Scripting.FindInterface(results.CompiledAssembly, "INpcScript");
                        }
                        else
                        {
                            mAvailableNPCScripts.Add(savename, (INpcScript)Scripting.FindInterface(results.CompiledAssembly, "INpcScript"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        public void MakeAvailableScripts(Character chr)
        {
            if (mAvailableNPCScripts == null)
            {
                mAvailableNPCScripts = new Dictionary<string, INpcScript>();
            }
            else
            {
                mAvailableNPCScripts.Clear();
            }
            if (chr == null)
            {
                Console.WriteLine("-------- SCRIPT COMPILER START ---------");
            }

            CompilerResults results;

               List<string> FilesNeedingRecompiling = ScriptDataChecker.GetFilesNeedingRecompiling().ToList();
            foreach (string filename in FilesNeedingRecompiling)
            {
                try
                {
                    FileInfo fi = new FileInfo(filename);

                    results = Scripting.CompileScript(filename);
                    if (results.Errors.Count > 0)
                    {
                        if (chr != null)
                        {
                            MessagePacket.SendNotice("Couldn't compile the file (" + fi.Name + ") correctly;", chr);
                            foreach (CompilerError error in results.Errors)
                            {
                                MessagePacket.SendNotice("Line " + error.Line.ToString() + ", Column " + error.Column.ToString() + ": " + error.ErrorText, chr);
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Couldn't compile the file ({0}) correctly;", filename));
                            foreach (CompilerError error in results.Errors)
                            {
                                Console.WriteLine(string.Format("Line {0}, Column {1}: {2}", error.Line, error.Column, error.ErrorText));
                            }
                        }
                    }
                    else
                    {
                        /*
                        if (chr != null)
                        {
                            MessagePacket.SendNotice("Compiled " + fi.Name + "!", chr);
                        }
                        else
                        {
                            Program.MainForm.appendToLog(string.Format("Compiled {0}!", fi.Name));
                        }
                        */
                        mAvailableNPCScripts.Add(fi.Name.Replace(".s", "").Replace(".cs", ""), (INpcScript)Scripting.FindInterface(results.CompiledAssembly, "INpcScript"));
                    }

                    fi = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            if (chr == null)
            {
                Console.WriteLine("-------- SCRIPT COMPILER END ---------");
            }
            ScriptDataChecker.GenerateNewScriptHashes();
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
                if (kvp.Value != null && kvp.Value.Name.ToLowerInvariant() == name)
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
                Name = configFile,
                ID = 0xFF,
                Randomizer = new CharacterRandom()
            };
            Instance.Load();
        }

        void Load()
        {
            Initialized = false;
            LoadConfig(Application.StartupPath + @"\..\DataSvr\" + Name + ".img");
            LoadDBConfig(Application.StartupPath + @"\..\DataSvr\Database.img");



            //Server.Instance.MakeAvailableScripts(null);


            
            MiniRoomIDs = new LoopingID();
            PartyIDs = new LoopingID();
            MatchCardIDs = new LoopingID();
            CenterConnection = new CenterSocket();

            GameAcceptor = new GameAcceptor();
            Initialized = true;
        }

        private void LoadDBConfig(string configFile)
        {
            ConfigReader reader = new ConfigReader(configFile);
            string Username = reader.getString("", "dbUsername");
            string Password = reader.getString("", "dbPassword");
            string Database = reader.getString("", "dbDatabase");
            string Host = reader.getString("", "dbHost");

            connectstring = new string[] { Username, Password, Database, Host };
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
            AdultWorld = reader.getBool("", "AdultWorld");
            MasterIP = IPAddress.Parse(reader.getString("", "AllowedIP"));

            Console.WriteLine(AdultWorld.ToString());
            if (tmpHeader == "")
            {
                tmpHeader = "";
            }
            else if (tmpHeader == "EMPTY")
            {
                tmpHeader = "";
            }

            mScrollingHeader = tmpHeader;


            HackingAutoBlockValues = new Dictionary<string, int>();
            List<string> blockTypes = reader.getBlocks("hackingAutoBlock", true);
            foreach (string blockType in blockTypes)
            {
                HackingAutoBlockValues.Add(blockType, reader.getInt("hackingAutoBlock", blockType));
            }

            LoggedCharacters = new List<int>();
            List<string> charids = reader.getBlocks("logcharacter", true);
            foreach (string i in charids)
            {
                LoggedCharacters.Add(reader.getInt("logcharacter", i));
            }
        }

    }
}