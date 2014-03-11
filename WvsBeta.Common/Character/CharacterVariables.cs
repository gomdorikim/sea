using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Game {
	public class CharacterVariables {

		public Character mCharacter { get; set; }
		public Dictionary<string, string> mVariables { get; set; }

		public CharacterVariables(Character character) {
			mCharacter = character;
			mVariables = new Dictionary<string, string>();
		}

		public void Save() {
			int id = mCharacter.ID;
			string query = "";

			bool first = true;
			Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_variables WHERE charid = " + mCharacter.ID.ToString());
			foreach (KeyValuePair<string, string> kvp in mVariables) {
				if (first) {
					query = "INSERT INTO character_variables (charid, key, `value`) VALUES ";
					first = false;
				}
				else {
					query += ", ";
				}
				query += "(" + mCharacter.ID.ToString() + ", '" + MySqlHelper.EscapeString(kvp.Key) + "', '" + MySqlHelper.EscapeString(kvp.Value) + "')";

			}
			if (!first) {
				Server.Instance.CharacterDatabase.RunQuery(query);
			}
		}

		public bool Load() {
			Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_variables WHERE charid = " + mCharacter.ID.ToString());

			MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
			if (!data.HasRows) {
				return false;
			}
			else {
				while (data.Read()) {
					mVariables.Add(data.GetString("key"), data.GetString("value"));
				}
				return true;
			}
		}
	}
}
