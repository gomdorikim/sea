using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Game
{
    public class Buddy
    {
        public string Name { get; set; }
        public int CharacterID { get; set; }
        public int Channel { get; set; }
        public bool Assigned { get; set; }

        public Buddy(int charid, string name, int channel, bool assigned)
        {
            this.Name = name;
            this.CharacterID = charid;
            this.Channel = channel;
            this.Assigned = assigned;
        }

        public bool IsOnline
        {
            get
            {
                return this.Channel > 0;
            }
        }
    }

    public class BuddyList : Dictionary<int, Buddy>
    {
        public void LoadBuddies(Character chr)
        {
            MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM buddylist WHERE charid = '" + chr.ID + "'") as MySqlDataReader;
            if (data.HasRows)
            {
                while (data.Read())
                {
                    /**
                    int buddycharid = data.GetInt32("buddy_charid");
                    string buddyname = data.GetString("buddy_charname");
                    if (CenterServer.Instance.OnlineCharacters.ContainsValue(buddycharid))
                    {
                        foreach (KeyValuePair<int, int> kvp in CenterServer.Instance.OnlineCharacters)
                        {
                            this.Add(kvp.Value, new Buddy(kvp.Value, buddyname, kvp.Key, true));
                        }
                    }
                    else
                    {
                        this.Add(buddycharid, new Buddy(buddycharid, buddyname, -1, true));
                    }
                     * **/
                }

            }
        }

        public static bool OfflineVictimAdmin(string Victim)
        {
            int ID = Server.Instance.CharacterDatabase.UserIDByName(Victim);
            bool admin;
            using (MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM users WHERE ID = '" + ID + "'") as MySqlDataReader)
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

        public static void AddPendingRequest(string charname, string invitername, int inviterid)
        {
            int charid = Server.Instance.CharacterDatabase.AccountIdByName(charname);
            Server.Instance.CharacterDatabase.RunQuery("INSERT INTO buddylist_pending (char_id, `inviter_name`, inviter_id) VALUES (" + charid + ", '" + MySqlHelper.EscapeString(invitername) + "', " + inviterid + ")");
        }
    }
}
