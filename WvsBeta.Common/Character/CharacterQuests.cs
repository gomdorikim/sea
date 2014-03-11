using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Game {
	public class QuestData {
		public int ID { get; set; }
		public int QuestID { get; set; }
		public string Data { get; set; }
		public Dictionary<int, QuestMobData> Mobs { get; set; }
	}

	public class QuestMobData {
		public int QuestID { get; set; }
		public int MobID { get; set; }
		public int Killed { get; set; }
		public int Needed { get; set; }
	}

	public class CharacterQuests {
		public Character mCharacter { get; set; }
		public Dictionary<int, QuestData> mQuests { get; set; }

		public CharacterQuests(Character character) {
			mCharacter = character;
			mQuests = new Dictionary<int, QuestData>();
		}

		public void SaveQuests() {
			int id = mCharacter.ID;
			string query = "";

			bool first = true;
			bool first2 = true;
			string query2 = "";
			Server.Instance.CharacterDatabase.RunQuery("DELETE mobs.* FROM character_quest_mobs mobs LEFT JOIN character_quests quests ON mobs.id = quests.id WHERE quests.charid = " + mCharacter.ID.ToString());
			Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_quests WHERE charid = " + mCharacter.ID.ToString());
			foreach (KeyValuePair<int, QuestData> kvp in mQuests) {
				if (first) {
					query = "INSERT INTO character_quests (id, charid, questid, data) VALUES ";
					first = false;
				}
				else {
					query += ", ";
				}
				query += "(" + kvp.Value.ID + ", " + mCharacter.ID.ToString() + ", " + kvp.Key.ToString() + ", '" + MySqlHelper.EscapeString(kvp.Value.Data) + "')";

				if (kvp.Value.Mobs.Count > 0) {
					if (first2) {
						query2 = "INSERT INTO character_quest_mobs (id, mobid, killed, needed) VALUES ";
						first2 = false;
					}
					else {
						query2 += ", ";
					}
					foreach (KeyValuePair<int, QuestMobData> kvp2 in kvp.Value.Mobs) {
						query2 += "(" + kvp.Value.ID + ", " + kvp2.Value.MobID.ToString() + ", " + kvp2.Value.Killed.ToString() + ", " + kvp2.Value.Needed + ")";
					}
				}
			}
			if (!first) {
				Server.Instance.CharacterDatabase.RunQuery(query);
			}
			if (!first2) {
				Server.Instance.CharacterDatabase.RunQuery(query2);
			}
		}

		public bool LoadQuests() {
			Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_quests WHERE charid = " + mCharacter.ID.ToString());

			MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
			if (!data.HasRows) {
				return false; // Couldn't load character.
			}
			else {
				while (data.Read()) {
					QuestData qd = new QuestData();
					qd.ID = data.GetInt32("id");
					qd.QuestID = data.GetInt32("questid");
					qd.Data = data.GetString("data");
					qd.Mobs = new Dictionary<int, QuestMobData>();
					mQuests.Add(qd.QuestID, qd);
				}

				foreach (KeyValuePair<int, QuestData> kvp in mQuests) {
					if (Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_quest_mobs WHERE id = " + kvp.Key.ToString()) == 1) {
						data = Server.Instance.CharacterDatabase.Reader;
						QuestMobData qmd = new QuestMobData();
						qmd.MobID = data.GetInt32("mobid");
						qmd.Killed = data.GetInt32("killed");
						qmd.Needed = data.GetInt32("needed");
						kvp.Value.Mobs.Add(qmd.MobID, qmd);
					}
				}
				return true;
			}
		}

		public int AddNewQuest(int QuestID, string Data) {
			if (mQuests.ContainsKey(QuestID)) return -1;
			else {
				Server.Instance.CharacterDatabase.RunQuery("INSERT INTO character_quests (id, charid, questid, data) VALUES (NULL, " + mCharacter.ID.ToString() + ", " + QuestID + ", '" + MySqlHelper.EscapeString(Data) + "')");
				int ID = Server.Instance.CharacterDatabase.GetLastInsertId();
				QuestData qd = new QuestData();
				qd.ID = ID;
				qd.Data = Data;
				qd.Mobs = new Dictionary<int, QuestMobData>();
				qd.QuestID = QuestID;
				mQuests.Add(qd.QuestID, qd);
				QuestPacket.SendQuestDataUpdate(mCharacter, QuestID, Data);
				return ID;
			}
		}

		public void AddOrSetQuestMob(int QuestID, int MobID, int Needed) {
			if (mQuests.ContainsKey(QuestID)) {
				QuestData qd = mQuests[QuestID];
				if (qd.Mobs.ContainsKey(MobID)) {
					qd.Mobs[MobID].Needed = Needed;
				}
				else {
					QuestMobData qmd = new QuestMobData();
					qmd.MobID = MobID;
					qmd.QuestID = QuestID;
					qmd.Needed = Needed;
					qmd.Killed = 0;

					qd.Mobs.Add(MobID, qmd);
				}
			}
		}

		public bool HasQuestMob(int QuestID, int MobID) {
			if (mQuests.ContainsKey(QuestID)) {
				QuestData qd = mQuests[QuestID];
				if (qd.Mobs.ContainsKey(MobID)) {
					return true;
				}
			}
			return false;
		}

		public bool HasQuest(int QuestID) {
			if (mQuests.ContainsKey(QuestID)) {
				return true;
			}
			return false;
		}

		public string GetQuestData(int QuestID) {
			if (mQuests.ContainsKey(QuestID)) {
				return mQuests[QuestID].Data;
			}
			return "";
		}

	}
}
