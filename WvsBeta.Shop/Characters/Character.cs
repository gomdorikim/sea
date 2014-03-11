using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;


namespace WvsBeta.Shop {

	public partial class Character {
		public int mID { get; set; }
		public int mUserID { get; set; }
		public bool mAdmin { get; set; }
		public string mName { get; set; }
		public string mUserName { get; set; }
		public byte mGender { get; set; }
		public byte mSkin { get; set; }
		public int mHair { get; set; }
		public int mFace { get; set; }

		public int mMap { get; set; }
		public byte mMapPosition { get; set; }

		public CharacterInventory mInventory { get; set; }
		public CharacterSkills mSkills { get; set; }
		public CharacterPrimaryStats mPrimaryStats { get; set; }
		public CharacterStorage mStorage { get; set; }
		public CharacterQuests mQuests { get; set; }
		public CharacterPets mPets { get; set; }
        public Dictionary<int, Character> CharacterList { get; set; }
		public List<int> mWishlist { get; set; }

		public Player mPlayer { get; set; }

        

		public Character(int CharacterID) {
			mID = CharacterID;
		}

		~Character() {
			mInventory = null;
			mSkills = null;
			mPrimaryStats = null;
			mStorage = null;
			mQuests = null;
		}

		public void sendPacket(Packet pw) {
			if (mPlayer != null && mPlayer.Socket != null) {
				mPlayer.Socket.SendPacket(pw);
			}
		}

		public void Save() {
			try {
				string Command = "UPDATE characters SET ";
				Command += "skin = '" + mSkin.ToString() + "', ";
				Command += "hair = '" + mHair.ToString() + "', ";
				Command += "eyes = '" + mFace.ToString() + "', ";
				Command += "map = '" + mMap.ToString() + "', ";
				Command += "pos = '" + mMapPosition.ToString() + "', ";
				Command += "level = '" + mPrimaryStats.Level.ToString() + "', ";
				Command += "job = '" + mPrimaryStats.Job.ToString() + "', ";
				Command += "chp = '" + mPrimaryStats.HP.ToString() + "', ";
				Command += "cmp = '" + mPrimaryStats.MP.ToString() + "', ";
				Command += "mhp = '" + mPrimaryStats.MaxHP.ToString() + "', ";
				Command += "mmp = '" + mPrimaryStats.MaxMP.ToString() + "', ";
				Command += "`int` = '" + mPrimaryStats.Int.ToString() + "', ";
				Command += "dex = '" + mPrimaryStats.Dex.ToString() + "', ";
				Command += "str = '" + mPrimaryStats.Str.ToString() + "', ";
				Command += "luk = '" + mPrimaryStats.Luk.ToString() + "', ";
				Command += "ap = '" + mPrimaryStats.AP.ToString() + "', ";
				Command += "sp = '" + mPrimaryStats.SP.ToString() + "', ";
				Command += "fame = '" + mPrimaryStats.Fame.ToString() + "', ";
				Command += "exp = '" + mPrimaryStats.EXP.ToString() + "' ";
				Command += "WHERE ID = " + mID.ToString();

				Server.Instance.CharacterDatabase.RunQuery(Command);


				Server.Instance.CharacterDatabase.RunQuery("DELETE FROM character_wishlist WHERE charid = " + mID.ToString());

				if (mWishlist.Count > 0) {
					bool start = true;
					foreach (int serial in mWishlist) {
						if (start) {
							Command = "INSERT INTO character_wishlist VALUES (";
							start = false;
						}
						else {
							Command += ", (";
						}
						Command += mID.ToString() + ", " + serial.ToString() + ")";
					}
					Server.Instance.CharacterDatabase.RunQuery(Command);
				}

				mInventory.SaveInventory();
				mSkills.SaveSkills();
				mStorage.Save();
				mQuests.SaveQuests();
				mPets.Save();
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}

		public bool Load() {
			Server.Instance.CharacterDatabase.RunQuery("SELECT characters.*, users.admin, users.username AS uname FROM characters LEFT JOIN users ON users.id = characters.userid WHERE characters.id = " + mID.ToString());

			MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
			if (!data.HasRows) {
				return false; // Couldn't load character.
			}
			else {
				data.Read();
				mUserID = data.GetInt32("userid");
				mAdmin = data.GetBoolean("admin");
				mName = data.GetString("name");
				mGender = data.GetByte("gender");
				mSkin = data.GetByte("skin");
				mHair = data.GetInt32("hair");
				mFace = data.GetInt32("eyes");
				mUserName = data.GetString("uname");

				mMap = data.GetInt32("map");
				
				Random rnd = new Random();
				
				mPrimaryStats = new CharacterPrimaryStats(this);
				mPrimaryStats.Level = data.GetByte("level");
				mPrimaryStats.Job = data.GetInt16("job");
				mPrimaryStats.Str = data.GetInt16("str");
				mPrimaryStats.Dex = data.GetInt16("dex");
				mPrimaryStats.Int = data.GetInt16("int");
				mPrimaryStats.Luk = data.GetInt16("luk");
				mPrimaryStats.HP = data.GetInt16("chp");
				mPrimaryStats.MaxHP = data.GetInt16("mhp");
				mPrimaryStats.MP = data.GetInt16("cmp");
				mPrimaryStats.MaxMP = data.GetInt16("mmp");
				mPrimaryStats.AP = data.GetInt16("ap");
				mPrimaryStats.SP = data.GetInt16("sp");
				mPrimaryStats.EXP = data.GetInt32("exp");
				mPrimaryStats.Fame = data.GetInt16("fame");
				mPrimaryStats.SetSpeed(100);

				mPets = new CharacterPets(this);

				mInventory = new CharacterInventory(this);
				mInventory.LoadInventory();

				mSkills = new CharacterSkills(this);
				mSkills.LoadSkills();

				mStorage = new CharacterStorage(this);
				mStorage.Load();

				mQuests = new CharacterQuests(this);
				mQuests.LoadQuests();

				Server.Instance.CharacterDatabase.RunQuery("SELECT serial FROM character_wishlist WHERE charid = " + mID.ToString());

				data = Server.Instance.CharacterDatabase.Reader;

				mWishlist = new List<int>();

				while (data.Read()) {
					mWishlist.Add(data.GetInt32(0));
				}

				mPrimaryStats.CheckHPMP();


				Server.Instance.CharacterDatabase.RunQuery("UPDATE users INNER JOIN characters ON users.id = characters.userid SET users.online = " +
					((int)(20000 + (int)Server.Instance.WorldID * 100 + 50)).ToString() +
					", characters.online = 1 WHERE characters.id = " + mID.ToString());
				return true;
			}
		}
	}
}
