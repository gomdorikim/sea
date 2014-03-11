using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Game
{

    public partial class Character : CharacterBase
    {
        public int UserID { get; set; }
        public bool Admin { get; set; }
        public short MapChair { get; set; }

        public int Map { get; set; }
        public byte MapPosition { get; set; }
        public byte PortalCount { get; set; }

        public bool GMHideEnabled { get { return Buffs.HasGMHide(); } }
        public bool Donator { get; private set; }

        public MiniRoomBase Room { get; set; }
        public Door Door { get; set; }
        public byte RoomSlotId { get; set; }
        public bool UsingTimer { get; set; }
        public int Channel { get; set; }

        public CharacterCheatInspector CharacterCheatCheck { get; set; }
        public CharacterInventory Inventory { get; private set; }
        public CharacterSkills Skills { get; private set; }
        public CharacterBuffs Buffs { get; private set; }
        public CharacterPrimaryStats PrimaryStats { get; private set; }
        public CharacterRandom Randomizer { get; private set; }
        public CharacterSummons Summons { get; private set; }
        public CharacterStorage Storage { get; private set; }
        public CharacterQuests Quests { get; private set; }
        public CharacterVariables Variables { get; private set; }
        public CharacterPets Pets { get; private set; }
        public CharacterGameStats GameStats { get; private set; }
        
        public BuddyList Buddylist { get; set; }
        public Reactor NearestReactor { get; set; }
        public Ring pRing { get; set; }

        public List<int> Wishlist { get; private set; }

        public NpcChatSession NpcSession { get; set; }
        public int ShopNPCID { get; set; }
        public int TrunkNPCID { get; set; }

        public Player mPlayer { get; set; }

        public Character(int CharacterID)
        {
            ID = CharacterID;
            PortalCount = 0;
            ShopNPCID = 0;
            TrunkNPCID = 0;
            NpcSession = null;
            MapChair = -1;

        }

        ~Character()
        {
            Inventory = null;
            Skills = null;
            Buffs = null;
            PrimaryStats = null;
            Randomizer = null;
            NpcSession = null;
            Storage = null;
            Quests = null;
            Variables = null;
            Room = null;
        }

        public void sendPacket(byte[] pw)
        {
            if (mPlayer != null && mPlayer.Socket != null)
            {
                mPlayer.Socket.SendData(pw);
            }
        }

        public void sendPacket(Packet pw)
        {
            if (mPlayer != null && mPlayer.Socket != null)
            {
                mPlayer.Socket.SendPacket(pw);
                //FileWriter.WriteLine("Logs\\derp.txt", string.Format("hey look a packet" + HexEncoding.byteArrayToString(pw.ToArray())));
            }
        }


        public int GetPetID()
        {
            if (Pets.mSpawned != 0 && Inventory.GetItem(5, Pets.mSpawned) != null)
            {
                return Inventory.GetItem(5, Pets.mSpawned).ItemID;
            }
            return 0;
        }

        public void HandleDeath()
        {
            int tomap = Map;
            if (DataProvider.Maps.ContainsKey(Map))
            {
                tomap = DataProvider.Maps[Map].ReturnMap;
            }
            ModifyHP(50, false);

            ChangeMap(tomap);
        }



        public void Save()
        {
            try
            {
                string query = "UPDATE characters SET ";
                query += "skin = '" + Skin.ToString() + "', ";
                query += "hair = '" + Hair.ToString() + "', ";
                query += "eyes = '" + Face.ToString() + "', ";
                query += "map = '" + Map.ToString() + "', ";
                query += "pos = '" + MapPosition.ToString() + "', ";
                query += "level = '" + PrimaryStats.Level.ToString() + "', ";
                query += "job = '" + PrimaryStats.Job.ToString() + "', ";
                query += "chp = '" + PrimaryStats.HP.ToString() + "', ";
                query += "cmp = '" + PrimaryStats.MP.ToString() + "', ";
                query += "mhp = '" + PrimaryStats.MaxHP.ToString() + "', ";
                query += "mmp = '" + PrimaryStats.MaxMP.ToString() + "', ";
                query += "`int` = '" + PrimaryStats.Int.ToString() + "', ";
                query += "dex = '" + PrimaryStats.Dex.ToString() + "', ";
                query += "str = '" + PrimaryStats.Str.ToString() + "', ";
                query += "luk = '" + PrimaryStats.Luk.ToString() + "', ";
                query += "ap = '" + PrimaryStats.AP.ToString() + "', ";
                query += "sp = '" + PrimaryStats.SP.ToString() + "', ";
                query += "fame = '" + PrimaryStats.Fame.ToString() + "', ";
                query += "exp = '" + PrimaryStats.EXP.ToString() + "', ";
                query += "tested = '" + PrimaryStats.HasTest.ToString() + "', ";
                query += "party = '" + PartyID.ToString() + "', ";

                if (this.Leader)
                {
                    query += "leader = '1', ";
                }
                else
                {
                    query += "leader = '0', ";
                }
                query += "buffmhp = '" + PrimaryStats.BuffMHP + "' ";
                query += "WHERE ID = " + ID.ToString();

                Server.Instance.CharacterDatabase.RunQuery(query);


                Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_wishlist WHERE charid = " + ID.ToString());

                if (Wishlist.Count > 0)
                {
                    bool start = true;
                    foreach (int serial in Wishlist)
                    {
                        if (start)
                        {
                            query = "INSERT INTO character_wishlist VALUES (";
                            start = false;
                        }
                        else
                        {
                            query += ", (";
                        }
                        query += ID.ToString() + ", " + serial.ToString() + ")";
                    }
                    Server.Instance.CharacterDatabase.RunQuery(query);
                }



                Inventory.SaveInventory();
                Skills.SaveSkills();
                Storage.Save();
                Quests.SaveQuests();
                Pets.Save();
                Variables.Save();
                GameStats.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool Load()
        {
            Server.Instance.CharacterDatabase.RunQuery("SELECT characters.*, users.admin, users.donator FROM characters LEFT JOIN users ON users.id = characters.userid WHERE characters.id = " + ID.ToString());

            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
            if (!data.HasRows)
            {
                return false; // Couldn't load character.
            }
            else
            {
                data.Read();
                UserID = data.GetInt32("userid");
                Admin = data.GetBoolean("admin");
                Donator = data.GetBoolean("donator");
                Name = data.GetString("name");
                Gender = data.GetByte("gender");
                Skin = data.GetByte("skin");
                Hair = data.GetInt32("hair");
                Face = data.GetInt32("eyes");

                Map = data.GetInt32("map");
                Console.WriteLine(Map.ToString());
                PartyID = data.GetInt32("party");
                if (!DataProvider.Maps.ContainsKey(Map))
                {
                    Map = 0;
                    MapPosition = 0;
                }
                else
                {
                    if (DataProvider.Maps[Map].ForcedReturn != 999999999)
                    {
                        Map = DataProvider.Maps[Map].ForcedReturn;
                        MapPosition = 0;
                    }
                    else
                    {
                        MapPosition = (byte)data.GetInt16("pos");
                    }
                }

                {
                    Map map = DataProvider.Maps[Map];
                    int cnt = (int)Server.Instance.Randomizer.ValueBetween(0, map.SpawnPoints.Count - 1);
                    Portal portal = map.SpawnPoints[cnt];

                    Position = new Pos(portal.X, (short)(portal.Y - 40));
                }
                Stance = 0;
                Foothold = 0;

                Randomizer = new CharacterRandom();


                PrimaryStats = new CharacterPrimaryStats(this);
                PrimaryStats.Level = data.GetByte("level");
                PrimaryStats.Job = data.GetInt16("job");
                PrimaryStats.Str = data.GetInt16("str");
                PrimaryStats.Dex = data.GetInt16("dex");
                PrimaryStats.Int = data.GetInt16("int");
                PrimaryStats.Luk = data.GetInt16("luk");
                PrimaryStats.HP = data.GetInt16("chp");
                PrimaryStats.MaxHP = data.GetInt16("mhp");
                PrimaryStats.MP = data.GetInt16("cmp");
                PrimaryStats.MaxMP = data.GetInt16("mmp");
                PrimaryStats.AP = data.GetInt16("ap");
                PrimaryStats.SP = data.GetInt16("sp");
                PrimaryStats.EXP = data.GetInt32("exp");
                PrimaryStats.Fame = data.GetInt16("fame");
                PrimaryStats.BuddyListCapacity = data.GetInt32("buddylist_size");
                PrimaryStats.HasTest = data.GetInt16("tested");
                Leader = data.GetBoolean("leader");
                PrimaryStats.BuffMHP = data.GetInt16("buffmhp");
                if (PrimaryStats.BuffMHP == 0)
                {
                    PrimaryStats.BuffMHP = PrimaryStats.MaxHP;
                }
                PrimaryStats.SetSpeed(100);

                Pets = new CharacterPets(this);

                //Buddylist = new BuddyList();
                //Buddylist.LoadBuddies(this);

                Inventory = new CharacterInventory(this);
                Inventory.LoadInventory();

                Ring.LoadRings(this);

                Skills = new CharacterSkills(this);
                Skills.LoadSkills();

                Storage = new CharacterStorage(this);
                Storage.Load();

                Buffs = new CharacterBuffs(this);

                Summons = new CharacterSummons(this);

                Quests = new CharacterQuests(this);
                Quests.LoadQuests();

                Variables = new CharacterVariables(this);
                
                Variables.Load();

                GameStats = new CharacterGameStats(this);
                GameStats.Load();

                using (data = (MySqlDataReader)Server.Instance.CharacterDatabase.RunQuery("SELECT serial FROM character_wishlist WHERE charid = " + ID))
                {
                    Wishlist = new List<int>();

                    while (data.Read())
                    {
                        Wishlist.Add(data.GetInt32(0));
                    }
                }
                PrimaryStats.CheckHPMP();
                CharacterCheatCheck = new CharacterCheatInspector(this);

                Server.Instance.CharacterDatabase.RunQuery("UPDATE users INNER JOIN characters ON users.id = characters.userid SET users.online = " +
                    ((int)(20000 + (int)Server.Instance.WorldID * 100 + Server.Instance.ID)).ToString() +
                    ", characters.online = 1 WHERE characters.id = " + ID.ToString());
                return true;
            }
        }
    }
}