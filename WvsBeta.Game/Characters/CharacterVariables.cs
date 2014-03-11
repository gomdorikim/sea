using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Game
{
    public class CharacterVariables
    {
        private Character _character { get; set; }
        private Dictionary<string, string> _variables { get; set; }

        public CharacterVariables(Character character)
        {
            _character = character;
            _variables = new Dictionary<string, string>();
        }

        public void Save()
        {
            int id = _character.ID;
            string query = "";

            bool first = true;
            Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_variables WHERE charid = " + _character.ID.ToString());
            string queryblock = string.Join(",", _variables.Select((a) => { return "(" + _character.ID.ToString() + ", '" + MySqlHelper.EscapeString(a.Key) + "', '" + MySqlHelper.EscapeString(a.Value) + "')"; }).ToList());
            
            foreach (KeyValuePair<string, string> kvp in _variables)
            {
                if (first)
                {
                    query = "INSERT INTO character_variables (charid, key, `value`) VALUES ";
                    first = false;
                }
                else
                {
                    query += ", ";
                }
                query += "(" + _character.ID.ToString() + ", '" + MySqlHelper.EscapeString(kvp.Key) + "', '" + MySqlHelper.EscapeString(kvp.Value) + "')";

            }
            if (!first)
            {
                Server.Instance.CharacterDatabase.RunQuery(query);
            }
        }

        public bool Load()
        {
            using (MySqlDataReader data = (MySqlDataReader)Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM character_variables WHERE charid = " + _character.ID))
            {
                if (!data.HasRows)
                {
                    return false;
                }
                else
                {
                    while (data.Read())
                    {
                        _variables.Add(data.GetString("key"), data.GetString("value"));
                    }
                    return true;
                }
            }
        }

        public string GetVariableData(string pName)
        {
            if (_variables.ContainsKey(pName)) return _variables[pName];
            return null;
        }

        public List<string> GetVariableDataList(string pName)
        {
            return SplitData(GetVariableData(pName));
        }

        public void SetVariableData(string pName, string pVariableData)
        {
            if (pVariableData == null) return;

            if (!_variables.ContainsKey(pName)) _variables.Add(pName, pVariableData);
            else _variables[pName] = pVariableData;
        }

        public void SetVariableDataList(string pName, List<string> pVariableDataList)
        {
            if (pVariableDataList == null) return;

            if (!_variables.ContainsKey(pName)) _variables.Add(pName, JoinData(pVariableDataList));
            else _variables[pName] = JoinData(pVariableDataList);
        }

        public bool RemoveVariable(string pName)
        {
            if (_variables.ContainsKey(pName))
            {
                _variables.Remove(pName);
                return true;
            }
            return false;
        }

        public static List<string> SplitData(string pVariableData)
        {
            if (pVariableData == null) return null;
            return pVariableData.Split(';').ToList();
        }

        public static string JoinData(List<string> pDataList)
        {
            if (pDataList == null) return null;
            return string.Join(";", pDataList);
        }
    }
}