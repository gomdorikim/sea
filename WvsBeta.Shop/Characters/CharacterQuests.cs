using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Shop
{
    public class QuestData
    {
        public int ID { get; set; }
        public int QuestID { get; set; }
        public string Data { get; set; }
        public bool Complete { get; set; }
        public Dictionary<int, QuestMobData> Mobs { get; set; }
    }

    public class QuestMobData
    {
        public int QuestID { get; set; }
        public int MobID { get; set; }
        public int Killed { get; set; }
        public int Needed { get; set; }
    }

    public class CharacterQuests
    {
        private Character mCharacter { get; set; }
        public int RealQuests { get; set; }
        public Dictionary<int, QuestData> mQuests { get; private set; }
        public Dictionary<int, QuestData> mCompletedQuests { get; private set; }

        public CharacterQuests(Character character)
        {
            mCharacter = character;
            mQuests = new Dictionary<int, QuestData>();
            mCompletedQuests = new Dictionary<int, QuestData>();
        }

        public void SaveQuests()
        {
            int id = mCharacter.mID;
            string query = "";

            bool first = true;
            bool first2 = true;

            string query2 = "";
            Server.Instance.CharacterDatabase.RunQuery("DELETE mobs.* FROM character_quest_mobs mobs LEFT JOIN character_quests quests ON mobs.id = quests.id WHERE quests.charid = " + mCharacter.mID.ToString());
            Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_quests WHERE charid = " + mCharacter.mID.ToString());

            foreach (KeyValuePair<int, QuestData> kvp in mQuests)
            {
                if (first)
                {
                    query = "INSERT INTO character_quests (id, charid, questid, data, complete) VALUES ";
                    first = false;
                }
                else
                {
                    query += ", ";
                }
                query += "(" + kvp.Value.ID + ", " + mCharacter.mID.ToString() + ", " + kvp.Key.ToString() + ", '" + MySqlHelper.EscapeString(kvp.Value.Data) + "', " + kvp.Value.Complete + ")";

                if (kvp.Value.Mobs.Count > 0)
                {
                    if (first2)
                    {

                        first2 = false;
                    }
                    //    else
                    //       {
                    //query2 += ", ";
                    //}                 
                    //string test = "INSERT INTO character_quest_mobs (id, mobid, killed, needed) VALUES (" + kvp.Value.ID + ", " + kvp2.Value.MobID.ToString() + ", " + kvp2.Value.Killed.ToString() + ", " + kvp2.Value.Needed +")";
                    //query2 = "INSERT INTO character_quest_mobs (id, mobid, killed, needed) VALUES (";
                    Server.Instance.CharacterDatabase.RunQuery("DELETE mobs.* FROM character_quest_mobs mobs LEFT JOIN character_quests quests ON mobs.id = quests.id WHERE quests.charid = " + mCharacter.mID.ToString());
                    foreach (KeyValuePair<int, QuestMobData> kvp2 in kvp.Value.Mobs)
                    {
                        Server.Instance.CharacterDatabase.RunQuery("INSERT INTO character_quest_mobs (id, mobid, killed, needed) VALUES (" + kvp.Value.ID + ", " + kvp2.Value.MobID.ToString() + ", " + kvp2.Value.Killed.ToString() + ", " + kvp2.Value.Needed + ")");
                        //query2 += kvp.Value.ID + ", " + kvp2.Value.MobID.ToString() + ", " + kvp2.Value.Killed.ToString() + ", " + kvp2.Value.Needed;
                        //break;
                    }
                    //query2 += ")";

                }
            }


            if (!first)
            {
                Server.Instance.CharacterDatabase.RunQuery(query);
            }
            if (!first2)
            {
                //Server.Instance.CharacterDatabase.RunQuery(query2);
            }


        }

        public bool LoadQuests()
        {
            MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_quests WHERE charid = " + mCharacter.mID.ToString()) as MySqlDataReader;
            if (!data.HasRows)
            {
                return false; // Couldn't load character.
            }
            else
            {
                while (data.Read())
                {
                    QuestData qd = new QuestData();
                    qd.ID = data.GetInt32("id");
                    qd.QuestID = data.GetInt32("questid");
                    qd.Data = data.GetString("data");
                    qd.Complete = data.GetBoolean("complete");
                    qd.Mobs = new Dictionary<int, QuestMobData>();

                    if (qd.Complete)
                    {
                        mCompletedQuests.Add(qd.QuestID, qd);

                        mQuests.Add(qd.QuestID, qd);
                        //.Count--;
                    }
                    else
                    {
                        mQuests.Add(qd.QuestID, qd);
                        RealQuests++;
                        Console.WriteLine("real quests : " + RealQuests);
                    }
                }

                foreach (KeyValuePair<int, QuestData> kvp in mQuests)
                {
                    MySqlDataReader mdr = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_quest_mobs WHERE id = " + kvp.Value.ID.ToString()) as MySqlDataReader;
                    if (mdr.HasRows)
                    {
                        while (mdr.Read())
                        {
                            QuestMobData qmd = new QuestMobData();
                            qmd.MobID = mdr.GetInt32("mobid");
                            qmd.Killed = mdr.GetInt32("killed");
                            qmd.Needed = mdr.GetInt32("needed");
                            kvp.Value.Mobs.Add(qmd.MobID, qmd);
                            Console.WriteLine("loaded mob data!");
                        }
                    }
                }
                return true;
            }
        }
    }
}
