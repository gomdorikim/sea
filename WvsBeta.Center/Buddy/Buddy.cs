using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Center
{
    public enum BuddyResults : byte
    {
        BuddyListFull = 0x0B,
        UserBuddyListFull = 0x0C,
        AlreadyOnList = 0x0D,
        GameMasterNotAvailable = 0x0E,
        CharacterNotRegistered = 0x0F
    }

    public class Buddy
    {
        public string Name { get; set; }
        public int CharacterID { get; set; }
        public int Channel { get; set; }
        public bool Assigned { get; set; }

        public Buddy(int charid, string name, int channel, bool assigned)
        {
            Character chr = CenterServer.Instance.GetCharacterByCID(charid);
            this.Name = name;
            this.CharacterID = charid;
            if (chr != null)
            {
                this.Channel = chr.ChannelID;
            }
            else
            {
                this.Channel = channel;
            }
            this.Assigned = assigned;
        }

        public bool IsOnline
        {
            get
            {
                return this.Channel > 0;
            }
        }

        public static void UpdateChannel(Character chr, int NewChannel)
        {
            foreach(KeyValuePair<int, Buddy> kvp in chr.FriendsList)
            {
                Character Buddy = CenterServer.Instance.GetCharacterByCID(kvp.Value.CharacterID);
                if (CenterServer.Instance.IsOnline(Buddy))
                {
                    try
                    {
                        Buddy.FriendsList[chr.ID].Channel = NewChannel;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Caught exception in BuddyList! {0} : {1}", chr.ID, ex.ToString()));
                    }
                }
            }
        }
    }

    public class BuddyList : Dictionary<int, Buddy>
    {
        public List<int> PendingRequests { get; set; }

        public void LoadBuddies(int charid)
        {
            MySqlDataReader data = CenterServer.Instance.CharacterDatabase.RunQuery("SELECT * FROM buddylist WHERE charid = '" + charid + "'") as MySqlDataReader;
            if (data.HasRows)
            {
                while (data.Read())
                {
                    int buddycharid = data.GetInt32("buddy_charid");
                    string buddyname = data.GetString("buddy_charname");
                    byte assigned = data.GetByte("assigned");
                    bool isAssigned = (assigned == 1 ? false : true);
                    Character buddy = CenterServer.Instance.GetCharacterByCID(buddycharid);
                    if (CenterServer.Instance.mOnlineCharacters.Contains(buddy))
                    {
                        this.Add(buddycharid, new Buddy(buddycharid, buddyname, buddy.bChannelID, isAssigned));
                    }
                    else
                    {
                        this.Add(buddycharid, new Buddy(buddycharid, buddyname, -1, isAssigned));
                    }
                }
            }
        }

        public void AddBuddy(Character chr, Character ToAdd, bool Offline)
        {
            if (!Offline)
            {
                chr.FriendsList.Add(ToAdd.ID, new Buddy(ToAdd.ID, ToAdd.Name, -1, true));
                AddPendingRequest(chr.ID, ToAdd.ID, ToAdd.Name, chr.Name);
            }

        }

        public static void AcceptBuddyRequest(Character pAcceptor, int AcceptedID, string AcceptedName)
        {
            //pAcceptor.FriendsList.Add(AcceptedID, new Buddy(AcceptedID, AcceptedName, -1, true));
            CenterServer.Instance.CharacterDatabase.RunQuery("UPDATE buddylist SET assigned = 0 WHERE charid = " + pAcceptor.ID + " and buddy_charid = " + AcceptedID);
            if (CenterServer.Instance.IsOnline(AcceptedID))
            {
                Character Accepted = CenterServer.Instance.GetCharacterByCID(AcceptedID);
                //pAcceptor.FriendsList.Add(pAcceptor.ID, new Buddy(pAcceptor.ID, pAcceptor.Name, pAcceptor.ChannelID, false));
            }

        }
        public static bool OfflineVictimAdmin(string Victim)
        {
            int ID = CenterServer.Instance.CharacterDatabase.UserIDByName(Victim);
            bool admin;
            using (MySqlDataReader data = CenterServer.Instance.CharacterDatabase.RunQuery("SELECT * FROM users WHERE ID = '" + ID + "'") as MySqlDataReader)
            {
                if (!data.HasRows)
                {
                    return false;
                }
                else
                {
                    data.Read();
                    admin = data.GetBoolean("admin");
                }
                return admin;
            }
        }

        public static int OfflineVictimBuddyListCount(string Victim)
        {
            List<int> buddycount = new List<int>();
            int ID = CenterServer.Instance.CharacterDatabase.UserIDByName(Victim);
            using (MySqlDataReader data = CenterServer.Instance.CharacterDatabase.RunQuery("SELECT * FROM buddylist WHERE charid = '" + ID + "'") as MySqlDataReader)
            {
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        int buddycharid = data.GetInt32("buddy_charid");
                        buddycount.Add(buddycharid);
                        return buddycount.Count;
                    }
                }
            }
            return buddycount.Count;
        }

        public static int OfflineVictimBuddyCapacity(string Victim)
        {
            int amount = 0;
            using (MySqlDataReader data = CenterServer.Instance.CharacterDatabase.RunQuery("SELECT * FROM characters WHERE name = '" + MySqlHelper.EscapeString(Victim) + "'") as MySqlDataReader)
            {
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        amount = data.GetInt32("buddylist_size");
                    }
                }
                return amount;
            }
        }

        public static bool CharacterRegistered(string Victim)
        {
            using (MySqlDataReader data = CenterServer.Instance.CharacterDatabase.RunQuery("SELECT * FROM characters WHERE name = '" + MySqlHelper.EscapeString(Victim) + "'") as MySqlDataReader)
            {
                if (!data.HasRows) return false; else return true;
            }
        }

        public static void AddPendingRequest(int fromWho, int toWho, string buddyname, string addername)
        {
            //int charid = CenterServer.Instance.CharacterDatabase.AccountIdByName(charname);
            CenterServer.Instance.CharacterDatabase.RunQuery("INSERT INTO buddylist (charid, buddy_charid, `buddy_charname`, assigned) VALUES (" + fromWho + ", " + toWho + ", '" + MySqlHelper.EscapeString(buddyname) + "', 0)");
            CenterServer.Instance.CharacterDatabase.RunQuery("INSERT INTO buddylist (charid, buddy_charid, `buddy_charname`, assigned) VALUES (" + toWho + ", " + fromWho + ", '" + MySqlHelper.EscapeString(addername) + "', 1)");
        }


        public static void ChangePending(Character chr, int BuddyID, bool Remove)
        {
            if (Remove)
            {
                CenterServer.Instance.CharacterDatabase.RunQuery("DELETE FROM buddylist WHERE charid = '" + chr.ID + "' and buddy_charid = '" + BuddyID + "'");
                CenterServer.Instance.CharacterDatabase.RunQuery("DELETE FROM buddylist WHERE charid = '" + BuddyID + "' and buddy_charid = '" + chr.ID + "'");
            }
        }
    }
}
