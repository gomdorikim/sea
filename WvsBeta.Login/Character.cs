using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Login
{

    public class Character
    {
        public int mID { get; set; }
        public string mName { get; set; }
        public byte mGender { get; set; }
        public byte mSkin { get; set; }
        public int mHair { get; set; }
        public int mFace { get; set; }

        public int mMap { get; set; }
        public byte mMapPosition { get; set; }

		public int mWorldPos { get; set; }
		public int mWorldOldPos { get; set; }
		public int mJobPos { get; set; }
		public int mJobOldPos { get; set; }

        public Dictionary<byte, int> mShownEquips { get; set; }
        public Dictionary<byte, int> mHiddenEquips { get; set; }

        public PrimaryStats mPrimaryStats { get; set; }

        public Character(int CharacterID)
        {
            mID = CharacterID;
            mShownEquips = new Dictionary<byte, int>();
            mHiddenEquips = new Dictionary<byte, int>();
        }


        public bool Load()
        {
            Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM characters WHERE id = " + mID.ToString());

            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
            if (!data.HasRows)
            {
                return false; // Couldn't load character.
            }
            else
            {
                data.Read();
                mName = data.GetString("name");
                mGender = data.GetByte("gender");
                mSkin = data.GetByte("skin");
                mHair = data.GetInt32("hair");
                mFace = data.GetInt32("eyes");

                mMap = data.GetInt32("map");
                mMapPosition = (byte)data.GetInt16("pos");

                mPrimaryStats = new PrimaryStats();
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

				mWorldPos = data.GetInt32("world_cpos");
				mWorldOldPos = data.GetInt32("world_cpos") - data.GetInt32("world_opos");
				mJobPos = data.GetInt32("job_cpos");
				mJobOldPos = data.GetInt32("job_cpos") - data.GetInt32("job_opos");

                //data.Close();
                //data.Dispose();

                Server.Instance.CharacterDatabase.RunQuery("SELECT itemid, slot FROM items WHERE charid = " + mID.ToString() + " AND inv = 1 AND slot < 0 ORDER BY slot ASC");
                // Add items
                data = Server.Instance.CharacterDatabase.Reader;

                while (data.Read())
                {
                    int ItemID = data.GetInt32("itemid");
                    short slot = data.GetInt16("slot");
					if (slot < 0) {
                        if (slot < -100)
                        {
							slot += 100;
							slot = Math.Abs(slot);
							mShownEquips[(byte)slot] = ItemID;
                        }
                        else
                        {
							slot = Math.Abs(slot);
							mHiddenEquips[(byte)slot] = ItemID;
                        }
                    }
                }
                return true;
            }
        }
    }
}
