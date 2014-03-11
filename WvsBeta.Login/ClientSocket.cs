using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

using WvsBeta.Common;

namespace WvsBeta.Login
{
    public class ClientSocket : AbstractConnection
    {
        public Player Player { get; set; }
        public bool Loaded { get; set; }

        public ClientSocket(System.Net.Sockets.Socket pSocket)
            : base(pSocket)
        {
            Player = new Player();
            Player.LoggedOn = false;
            Player.Socket = this;
            Loaded = false;
            Pinger.Connections.Add(this);
            Server.Instance.AddPlayer(Player);
            SendHandshake(15, "", 0x07);
            
        }

        public override void OnDisconnect()
        {
            if (Player != null)
            {
                Server.Instance.RemovePlayer(Player.SessionHash);
                if (Player.LoggedOn)
                {
                    Program.MainForm.changeLoad(false);

                    Player.Characters.Clear();
                    Player.Socket = null;
                    Player = null;
                }
            }

            Pinger.Connections.Remove(this);
        }

        public override void AC_OnPacketInbound(Packet packet)
        {
            try
            {
                ClientMessages header = (ClientMessages)packet.ReadByte();
                if (header != ClientMessages.CLIENT_PONG && header != ClientMessages.Client_Hash2)
                {
                    Console.WriteLine(header + "");
                }
                if (header == ClientMessages.CLIENT_PONG)
                {
                    Console.WriteLine(header + "");
                }
                if (!Loaded)
                {
                    switch (header)
                    {
                        case ClientMessages.LOGIN_CHECK_PASSWORD: OnCheckPassword(packet); 
                            Console.WriteLine(packet.ToString());
                            break;
                        //case ClientMessages.CLIENT_CRASH_REPORT: FileWriter.WriteLine("ClientCrashes\\" + base.IP + ".txt", packet.ReadString()); Console.WriteLine("Received a crashlog!!!"); break;
                   }
                }
               else
                {
                    Console.WriteLine(packet.ToString());
                  switch (header)
                   {
                        
                        case ClientMessages.LOGIN_SELECT_CHANNEL: OnChannelSelect(packet); break;
                        case ClientMessages.LOGIN_WORLD_SELECT:OnWorldSelect(packet); break;
                        case ClientMessages.LOGIN_SELECT_CHARACTER:OnSelectCharacter(packet); break;
                        case ClientMessages.LOGIN_CHECK_NAME: OnCharNamecheck(packet); break;
                        case ClientMessages.LOGIN_CREATE_CHARACTER: OnCharCreation(packet); break;
                        case ClientMessages.LOGIN_REMOVE_CHARACTER: OnCharDeletion(packet); break;
                        case ClientMessages.CLIENT_PONG: break;
                        case ClientMessages.CLIENT_HASH: break;
                        case ClientMessages.Client_Hash2:
                            {
                                //PingTest();
                                break;
                            }
                        default:
                            {
                                //Console.WriteLine("[LS][{0}][{1}] Unknown packet found: {2}", DateTime.Now.ToString(), Player.ID, packet.ToString());
                                //Program.LogFile.WriteLine("Unknown packet found " + packet.ToString());
                                
                                break;
                            }
                     }
                }
                packet = null;
            }
            catch (Exception ex)
            {
                Program.LogFile.WriteLine("Exception caught: " + ex.Message + Environment.NewLine + Environment.NewLine + "Stacktrace: " + ex.StackTrace);
                Disconnect();
            }
        }

        public static bool CheckCharnameTaken(Player pPlayer, string pName)
        {
            bool wat = false;
            if (pPlayer.Characters.Count < 3 && pName.Length >= 4 && pName.Length <= 12)
            {
                using (MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM characters WHERE name = '" + MySqlHelper.EscapeString(pName) + "'") as MySqlDataReader)
                {
                    wat = data.HasRows;
                }
            }

            return wat;
        }

        
        public void CreateCharacterItem(int itemid, int charid, int slot, int watk = 0, byte wdef = 0)
        {
            Server.Instance.CharacterDatabase.RunQuery(string.Format("INSERT INTO items (charid, inv, slot, itemid, iwatk, iwdef, slots) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, 7);", charid, itemid / 1000000, slot, itemid, watk, wdef));
        }

        public void connectToServer(int charid, byte[] IP, ushort port)
        {
            byte bit = 0, goPremium = 1;

            bit |= (byte)(goPremium << 0);



            Packet pw = new Packet(0x06);
            pw.WriteByte(0x00);
            pw.WriteByte(0x00);
            pw.WriteBytes(IP);
            pw.WriteUShort(port);
            pw.WriteInt(charid);
            pw.WriteByte(bit);
            pw.WriteInt(1); // minutes remaining in internet cafe
            //Console.WriteLine(pw.ToString());
            SendPacket(pw);
        }

        public void OnSelectCharacter(Packet packet)
        {
            if (!Player.LoggedOn) return;
            int charid = packet.ReadInt();
            Server.Instance.CenterConnection.RequestCharacterConnectToWorld(Player.SessionHash, charid, Player.World, Player.Channel);
        }


        public static void AddCharacterData(Packet pack, Character character, int pos)
        {
            pack.WriteInt(character.mID);
            pack.WriteString(character.mName, 13);
            pack.WriteByte(character.mGender); // Gender
            pack.WriteByte(character.mSkin); // Skin
            pack.WriteInt(character.mFace); // Face
            pack.WriteInt(character.mHair); // Hair

            pack.WriteLong(0); // Pet Cash ID :/
            //pack.WriteLong(0);
            //pack.WriteLong(0);

            pack.WriteByte(character.mPrimaryStats.Level); // Level
            pack.WriteShort(character.mPrimaryStats.Job); // Jobid
            pack.WriteShort(character.mPrimaryStats.Str); //charc.str);
            pack.WriteShort(character.mPrimaryStats.Dex); //charc.dex);
            pack.WriteShort(character.mPrimaryStats.Int); //charc.intt);
            pack.WriteShort(character.mPrimaryStats.Luk); //charc.luk);
            pack.WriteShort(character.mPrimaryStats.HP); //charc.hp);
            pack.WriteShort(character.mPrimaryStats.MaxHP); //charc.mhp);
            pack.WriteShort(character.mPrimaryStats.MP); //charc.mp);
            pack.WriteShort(character.mPrimaryStats.MaxMP); //charc.mmp);
            pack.WriteShort(character.mPrimaryStats.AP); //charc.ap);
            pack.WriteShort(character.mPrimaryStats.SP); //charc.sp);
            pack.WriteInt(character.mPrimaryStats.EXP); //charc.exp);
            pack.WriteShort(character.mPrimaryStats.Fame);

            //definitly good 
            
            //pack.WriteInt(0);
            pack.WriteInt(character.mMap); // Mapid
            pack.WriteByte(character.mMapPosition); // Mappos
            

            //Next Bit is 11 bytes long
            
            pack.WriteByte(character.mGender); // Gender
            pack.WriteByte(character.mSkin); // Skin
            pack.WriteInt(character.mFace); // Face
            pack.WriteByte(0); //no fucking clue..
            pack.WriteInt(character.mHair); //Hair


            foreach (KeyValuePair<byte, int> equip in character.mHiddenEquips)
            {
                pack.WriteByte(equip.Key);
                pack.WriteInt(equip.Value);
            }
            
            foreach (KeyValuePair<byte, int> equip in character.mShownEquips)
            {
                pack.WriteByte(equip.Key);
                pack.WriteInt(equip.Value);
            }
          
            pack.WriteByte(0xFF); // Equips shown end

            pack.WriteBool(true); // Rankings

            pack.WriteInt(0);
            pack.WriteInt(character.mWorldOldPos);
        }


        public void OnCharDeletion(Packet packet)
        {
            if (!Player.LoggedOn) return;

            string asiasoftid = packet.ReadString();
            int charid = packet.ReadInt();

            Character toDelete = Player.Characters.Find(c => c.mID == charid);
            if (toDelete == null) return;

            Server.Instance.CharacterDatabase.RunQuery("SELECT asiasoftid FROM users WHERE ID = '" + this.Player.ID + "'");
            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
            int howmany = 0;
            if (data.HasRows && (data.Read() && data.GetString(0) == asiasoftid))
            {
                Console.WriteLine("deleting");
                data.Close();

                int hm = (int)Server.Instance.CharacterDatabase.RunQuery("DELETE FROM characters WHERE ID = '" + charid + "' AND world_id = '" + Player.World + "' AND userid = '" + Player.ID + "'");
                if (hm == 1)
                {
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM items WHERE charid = '" + charid + "'");
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_wishlist WHERE charid = '" + charid + "'");
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_variables WHERE charid = '" + charid + "'");
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_quests WHERE charid = '" + charid + "'");
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM buddylist WHERE charid = '" + charid + "' or buddy_charid = '" + charid + "'");
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM cooldowns WHERE charid = '" + charid + "'");
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM fame_log WHERE `from` = '" + charid + "' OR `to` = '" + charid + "'");
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM teleport_rock_locations WHERE charid = '" + charid + "'");
                    Server.Instance.CharacterDatabase.RunQuery("DELETE FROM skills WHERE charid = '" + charid + "'");
                    howmany = 1;
                }
            }

            Packet pack = new Packet(0x09);
            pack.WriteInt(charid);
            if (howmany == 1)
            {
                pack.WriteByte(0x00);
            }
            else
            {
                pack.WriteByte(0x10); 
                
            }
            SendPacket(pack);
        }


        public void OnCharCreation(Packet packet)
        {
            if (!Player.LoggedOn) return;

            string charname = packet.ReadString();
            if (!CheckCharnameTaken(Player, charname))
            {
                int face = packet.ReadInt();
                int hair = packet.ReadInt();
                int haircolor = packet.ReadInt();
                int skin = packet.ReadInt();
                int top = packet.ReadInt();

                int bottom = packet.ReadInt();
                int shoes = packet.ReadInt();
                int weapon = packet.ReadInt();
                byte str = packet.ReadByte();
                byte dex = packet.ReadByte();
                byte intt = packet.ReadByte();
                byte luk = packet.ReadByte();

                if (str >= 13)
                    FileWriter.WriteLine(@"Suspicious/STRhacks.txt", string.Format("[{0}] : '{1}'  is under suspicion of using Cheat Engine to get 13 STR during character creation.", DateTime.Now, charname));


                if (!(str >= 4 && dex >= 4 && intt >= 4 && luk >= 4 && (str + dex + intt + luk) <= 25)) return;


                if (Array.IndexOf(Server.BeginnerEyes, face) < 0) return;
                if (Array.IndexOf(Server.BeginnerHair, hair) < 0) return;
                if (Array.IndexOf(Server.BeginnerHairColor, haircolor) < 0) return;
                if (Array.IndexOf(Server.BeginnerSkinColor, skin) < 0) return;
                if (Array.IndexOf(Server.BeginnerTop, top) < 0) return;
                if (Array.IndexOf(Server.BeginnerBottom, bottom) < 0) return;
                if (Array.IndexOf(Server.BeginnerShoes, shoes) < 0) return;
                if (Array.IndexOf(Server.BeginnerWeapons, weapon) < 0) return;

                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO characters (name, userid, world_id, eyes, hair, skin, gender, str, dex, `int`, luk) VALUES ");
                query.AppendFormat("('{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10});", MySqlHelper.EscapeString(charname), Player.ID, Player.World, face, hair + haircolor, skin, Player.Gender, str, dex, intt, luk);

               
                Server.Instance.CharacterDatabase.RunQuery(query.ToString());
                int id = Server.Instance.CharacterDatabase.GetLastInsertId();
                CreateCharacterItem(top, id, -5, 0, 3); // Give top
                CreateCharacterItem(bottom, id, -6, 0, 2); // Give bottom
                CreateCharacterItem(shoes, id, -7, 0, 2); // Give shoes
                CreateCharacterItem(weapon, id, -11, 17, 0); // Give weapon

                Character pc = new Character(id);
                bool couldload = pc.Load();

                Packet pack = new Packet(0x08);
                pack.WriteBool(!couldload);
                if (couldload)
                {
                    Player.Characters.Add(pc);
                    AddCharacterData(pack, pc, 1);
                }
                SendPacket(pack);
            }
            else
            {
                Packet pack = new Packet(0x06);
                pack.WriteString(charname);
                pack.WriteBool(true);
                SendPacket(pack);
            }
        }

        public void OnCharNamecheck(Packet packet)
        {
            if (!Player.LoggedOn) return;

            string name = packet.ReadString();
            bool yes = CheckCharnameTaken(Player, name);

            Packet pack = new Packet(0x07);
            pack.WriteString(name);
            pack.WriteBool(yes);
            SendPacket(pack);
        }

        public void OnChannelSelect(Packet packet)
        {
            if (!Player.LoggedOn) return;
            packet.Skip(2);
            Server.Instance.CenterConnection.RequestCharacterIsChannelOnline(Player.SessionHash, Player.World, packet.ReadByte(), Player.Admin, Player.Socket.IP);

        }

        public void DoChannelSelect(byte channel)
        {
            if (!Player.LoggedOn) return;
            Player.Channel = channel;

            Packet pack = new Packet(0x05);
            pack.WriteByte(0);

            pack.WriteByte((byte)Player.Characters.Count);


            foreach (Character character in Player.Characters)
            {
                AddCharacterData(pack, character, 1);
            }
            //pack.WriteByte(0xFF);
            //pack.WriteLong(0);
            //pack.WriteLong(0);
            //pack.WriteLong(0);
            SendPacket(pack);
        }

        public void OnWorldSelect(Packet packet)
        {
            if (!Player.LoggedOn) return;
            byte world = packet.ReadByte();
            if (!Server.Instance.Worlds.ContainsKey(world))
            {
                return;
            }

            Player.World = world;

            Server.Instance.CharacterDatabase.RunQuery("SELECT ID FROM characters WHERE userid = '" + Player.ID + "' AND world_id = " + Player.World);

            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;

            List<int> ids = new List<int>();
            for (byte i = 0; i < 3; i++) // 3 slots max.
            {
                if (!data.Read()) break;
                ids.Add(data.GetInt32(0));
            }

            foreach (int id in ids)
            {
                if (Player.ContainsCharacter(id)) continue; // Character already loaded O.o!
                Character pc = new Character(id);
                if (!pc.Load())
                {
                    //Console.WriteLine("Couldn't load character: {0}", id);
                    pc = null;
                }
                else
                {
                    Player.Characters.Add(pc);
                }
            }

            ids.Clear();

            Server.Instance.CenterConnection.RequestCharacterGetWorldLoad(Player.SessionHash, world);
        }

        public void OnCheckPassword(Packet packet)
        {
            if (Player.State == 1)
            {
                Console.WriteLine("Disconnected session 4");
                Console.WriteLine("Disconnected client (4)");
                Disconnect();
                return;
            }

            string username = packet.ReadString();
            string password = packet.ReadString();

            if (username.Length < 4 || username.Length > 12 || password.Length < 4 || password.Length > 12)
            {
                Console.WriteLine("LOLWAT!");
                Disconnect();
                return;
            }

            int startupThingy = packet.ReadInt();
            ulong machineID = packet.ReadULong() + packet.ReadULong(); // 16 bytes...

            using (var mdr = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM machine_ban WHERE machineid = " + machineID.ToString()) as MySqlDataReader)
            {
                if (mdr.HasRows)
                {
                    Server.Instance.CharacterDatabase.RunQuery("UPDATE machine_ban SET last_try = NOW(), last_username = '" + MySqlHelper.EscapeString(username) + "', last_ip = '" + IP + "' WHERE machineid = " + machineID.ToString());

                    Console.WriteLine("Account " + username + " tried to login on a machine-banned account.");
                    Disconnect();
                    return;
                }
            }


            MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM users WHERE username = '" + MySqlHelper.EscapeString(username) + "'") as MySqlDataReader;
            byte result = 0x00;


            if (!data.HasRows)
            {
                result = 0x05;
            }
            else
            {
                data.Read();
                string dbpass = data.GetString("password");
                if (!data.IsDBNull(3))
                {
                    //string salt = data.GetString("salt");
                    //string hash = salt.Length == 4 ? Cryptos.SHA1_ComputeHexaHash(salt + password).ToUpper() : Cryptos.SHA512_ComputeHexaHash(salt + password).ToUpper();
                    if (data.GetString("password").ToLower() == password)
                    {
                        result = 0x01;
                    }
                    else
                    {
                        result = 0x04;
                    }
                }
                else if (dbpass != password)
                {
                    result = 0x04; // Invalid password
                }
                else if (data.GetInt32("online") != 0)
                {
                    result = 0x07;
                }
                else if (data.GetInt32("ban_reason") != 0)
                {
                    result = 0x03;
                }
                Player.Admin = data.GetBoolean("admin");
                Player.Gender = data.GetByte("gender");
                Player.ID = data.GetInt32("ID");

                Player.Username = username;
            }
            // -Banned- = 2
            // Deleted or Blocked = 3
            // Invalid Password = 4
            // Not Registered = 5
            // Sys Error = 6
            // Already online = 7
            // System error = 9
            // Too many requests = 10
            // Older than 20 = 11
            // Master cannot login on this IP = 13

            Packet pack = new Packet(0x01);
            pack.WriteByte(result);
            if (result <= 0x01)
            {
                pack.WriteInt(Player.ID);
                pack.WriteByte(Player.Gender);
                pack.WriteBool(Player.Admin);
                pack.WriteString(username);
            }
            else if (result == 0x02)
            {
                pack.WriteByte(data.GetByte("ban_reason"));
                pack.WriteLong(data.GetMySqlDateTime("ban_expire").Value.ToFileTimeUtc());
            }
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);

            SendPacket(pack);


            if (result <= 0x01)
            {
                Player.LoggedOn = true;
                Console.WriteLine("Account " + username + " logged on. Machine ID: " + machineID.ToString());
                //Program.MainForm.changeLoad(true);
                Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET last_login = NOW() WHERE ID = " + Player.ID);

                Loaded = true;
                if (Player.Characters == null)
                {
                    Player.Characters = new List<Character>();
                }
            }

            if (result > 0x01) return;

            pack = new Packet(0x03);
            pack.WriteByte(0x04); // -.-
            SendPacket(pack);

            Server.Instance.CenterConnection.RequestCharacterWorldList(Player.SessionHash);
        }
    }
}
