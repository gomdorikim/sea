using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace WvsBeta.Database
{
    public class MySQL_Connection
    {
        public MySqlDataReader Reader { get; private set; }
        public  bool Stop { get; set; }

        private MySqlConnection _connection;
        private MySqlCommand _command;
        private string _connectionString;
        private Common.Logfile _logFile;
        private Stack<KeyValuePair<string, string>> _queryList = new Stack<KeyValuePair<string, string>>();

        private void AddQuery(string pQuery)
        {
            if (_queryList.Count > 5) _queryList.Pop();
            _queryList.Push(new KeyValuePair<string, string>(pQuery, new StackTrace().ToString()));
        }

        private string GetLastQueries()
        {
            string ret = "---------- BEGIN LIST -------------\r\n";
            foreach (KeyValuePair<string, string> kvp in _queryList)
            {
                ret += "Query: " + kvp.Key + "\r\n";
                ret += "Stacktrace:\r\n" + kvp.Value + "\r\n";
            }
            ret += "------------- END LIST ---------------\r\n";
            return ret;
        }

        public MySQL_Connection(MasterThread pMasterThread, string pUsername, string pPassword, string pDatabase, string pHost, ushort pPort = 3306)
        {
            if (pMasterThread == null)
            {
                throw new Exception("MasterThread shouldn't be NULL at this time!");
            }

            pMasterThread.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Database Pinger",
                (date) =>
                {
                    bool res = false;
                    using (MySqlDataReader mdr = (MySqlDataReader)RunQuery("SELECT 1"))
                    {
                        mdr.Read();
                        if (mdr.GetByte(0) == 1) res = true;
                    }
                    if (!res)
                    {
                        _logFile.WriteLine("Failure pinging the server!!!!");
                    }
                },
                5 * 1000, 5 * 1000));


            _logFile = new Common.Logfile("Database", true, "Logs\\" + pMasterThread.ServerName + "\\Database");
            Stop = false;
            _connectionString = "Server=" + pHost + "; Port=" + pPort + "; Database=" + pDatabase + "; Uid=" + pUsername + "; Pwd=" + pPassword;
            Connect();
        }

        public void Connect()
        {
            try
            {
                _logFile.WriteLine("Connecting to database...");
                _connection = new MySqlConnection(_connectionString);

                _logFile.WriteLine("Adding StateChange handler...");
                _connection.StateChange += new System.Data.StateChangeEventHandler(connection_StateChange);

                _logFile.WriteLine("Opening connection");
                _connection.Open();

                string line = string.Format("Connected with MySQL server with version info: {0} and uses {1}compression", _connection.ServerVersion, _connection.UseCompression ? "" : "no ");

                _logFile.WriteLine(line);
                Console.WriteLine(line);
            }
            catch (Exception ex)
            {
                string line = string.Format("Got exception at MySQL_Connection.Connect():\r\n {0}", ex.ToString());
                _logFile.WriteLine(line);

                Console.WriteLine(ex.ToString());
                throw new Exception(line);
            }
        }

        void connection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            if (e.CurrentState == System.Data.ConnectionState.Closed && !Stop)
            {
                Console.WriteLine("MySQL connection closed. Reconnecting!");
                _logFile.WriteLine("Lost connection (connection Closed). Reconnecting.");
                _connection.StateChange -= connection_StateChange;
                Connect();
            }
            else if (e.CurrentState == System.Data.ConnectionState.Open)
            {
                _logFile.WriteLine("Connected to server!");
                Console.WriteLine("MySQL connection opened!");
            }
        }

        public int AccountIdByName(string username)
        {
            using (MySqlDataReader mdr = (MySqlDataReader)RunQuery("SELECT `ID` FROM characters WHERE `name` = '" + MySqlHelper.EscapeString(username) + "'"))
            {
                if (mdr.HasRows)
                {
                    mdr.Read();
                    return mdr.GetInt32(0);
                }
            }
            return -1;
        }

        public int UserIDByName(string username)
        {
            using (MySqlDataReader mdr = (MySqlDataReader)RunQuery("SELECT `userid` FROM characters WHERE `name` = '" + MySqlHelper.EscapeString(username) + "'"))
            {
                if (mdr.HasRows)
                {
                    mdr.Read();
                    return mdr.GetInt32(0);
                }
            }
            return -1;
        }

        public bool IsBanned(int id)
        {
            using (MySqlDataReader mdr = (MySqlDataReader)RunQuery("SELECT banned_expire >= NOW() FROM users WHERE ID = '" + id + "'"))
            {
                if (mdr.HasRows)
                {
                    mdr.Read();
                    return mdr.GetBoolean(0);
                }
            }
            return false;
        }

        public string getCharacterNameByID(int id)
        {
            using (MySqlDataReader mdr = (MySqlDataReader)RunQuery("SELECT name FROM characters WHERE ID = '" + id + "'"))
            {
                if (mdr.HasRows)
                {
                    mdr.Read();
                    return mdr.GetString(0);
                }
            }
            return "";
        }

        public void ClearParties()
        {
            RunQuery("UPDATE characters SET party = -1 WHERE online = 0");
        }

        public void ClearLeaders()
        {
            RunQuery("UPDATE characters SET leader = 0 WHERE online = 0");
        }

        public List<object> GetColumnsFromCharacterTable(int pID, params string[] pColumns)
        {
            List<object> ret = null;
            using (MySqlDataReader mdr = (MySqlDataReader)RunQuery("SELECT " + string.Join(",", pColumns) + " FROM characters WHERE ID = '" + pID + "'"))
            {
                if (mdr.HasRows)
                {
                    mdr.Read();
                    ret = new List<object>();
                    for (int i = 0; i < pColumns.Length; i++)
                        ret.Add(mdr.GetValue(i));
                }
            }
            return ret;
        }

        public object RunQuery(string pQuery)
        {
            try
            {
                if (Reader != null && !Reader.IsClosed)
                {
                    Reader.Close();
                    Reader.Dispose();
                    Reader = null;
                }


                _command = new MySqlCommand(pQuery, _connection);
                AddQuery(pQuery);
                if (pQuery.StartsWith("SELECT"))
                {
                    Reader = _command.ExecuteReader();
                    return Reader;
                }
                else if (pQuery.StartsWith("DELETE") || pQuery.StartsWith("UPDATE") || pQuery.StartsWith("INSERT"))
                    return _command.ExecuteNonQuery();

            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Lost connection to DB... Trying to reconnect and wait a second before retrying to run query.");
                _logFile.WriteLine("Lost connection (InvalidOperation). Reconnecting.");
                Connect();
                System.Threading.Thread.Sleep(1000);
                RunQuery(pQuery);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 2055)
                {
                    Console.WriteLine("Lost connection to DB... Trying to reconnect and wait a second before retrying to run query.");
                    _logFile.WriteLine("Lost connection (MySQL Exception?). Reconnecting.");
                    Connect();
                    System.Threading.Thread.Sleep(1000);
                    RunQuery(pQuery);
                }
                else
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine(pQuery);
                    _logFile.WriteLine(GetLastQueries());
                    _logFile.WriteLine("Got exception @ MySQL_Connection::RunQuery({0}) :\r\n{1}", pQuery, ex.ToString());
                    throw new Exception(string.Format("[{0}][DB LIB] Got exception @ MySQL_Connection::RunQuery({1}) : {2}", DateTime.Now.ToString(), pQuery, ex.ToString()));
                }
            }
            return 0;
        }

        public int GetLastInsertId()
        {
            return (int)_command.LastInsertedId;
        }


        public bool Ping()
        {
            if (Reader != null && !Reader.IsClosed)
                return false;
            return _connection.Ping();
        }
    }
}