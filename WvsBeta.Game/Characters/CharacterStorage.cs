using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Database;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Common;

namespace WvsBeta.Game {
	public class CharacterStorage {
		public Character mCharacter { get; set; }
		public Dictionary<byte, Item> mItems { get; set; }
		public byte mSlots { get; set; }
		public int mMesos { get; set; }

		public CharacterStorage(Character chr) {
			mCharacter = chr;
			mItems = new Dictionary<byte, Item>();
		}

		~CharacterStorage() {
			mItems.Clear();
		}

		public void Load() {
			Server.Instance.CharacterDatabase.RunQuery("SELECT slots, mesos FROM storage WHERE userid = " + mCharacter.UserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());
			
			MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
			if (!data.HasRows) {
				mSlots = 4;
				mMesos = 0;
				Server.Instance.CharacterDatabase.RunQuery("INSERT INTO storage (userid, world_id) VALUES (" + mCharacter.UserID.ToString() + ", " + Server.Instance.WorldID.ToString() + ")");
			}
			else {
				data.Read();
				mSlots = (byte)data.GetInt16(0);
				mMesos = data.GetInt32(1);
			}


			Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM storage_items WHERE userid = " + mCharacter.UserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());
			data = Server.Instance.CharacterDatabase.Reader;
			while (data.Read()) {
				Item item = new Item();
				item.ItemID = data.GetInt32("itemid");
				item.Amount = data.GetInt16("amount");
				item.Slots = (byte)data.GetInt16("slots");
				item.Scrolls = (byte)data.GetInt16("scrolls");
				item.Str = data.GetInt16("istr");
				item.Dex = data.GetInt16("idex");
				item.Int = data.GetInt16("iint");
				item.Luk = data.GetInt16("iluk");
				item.HP = data.GetInt16("ihp");
				item.MP = data.GetInt16("imp");
				item.Watk = data.GetInt16("iwatk");
				item.Matk = data.GetInt16("imatk");
				item.Wdef = data.GetInt16("iwdef");
				item.Mdef = data.GetInt16("imdef");
				item.Acc = data.GetInt16("iacc");
				item.Avo = data.GetInt16("iavo");
				item.Hands = data.GetInt16("ihand");
				item.Speed = data.GetInt16("ispeed");
				item.Jump = data.GetInt16("ijump");
				item.Name = data.GetString("name");
				item.Expiration = data.GetInt64("expiration");
				AddItem(item);
			}
		}

		public void Save() {
			string query = "";
			bool firstrun = true;

			Server.Instance.CharacterDatabase.RunQuery("UPDATE storage SET slots = " + mSlots + ", mesos = " + mMesos + " WHERE userid = " + mCharacter.UserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());


			query = "DELETE FROM storage_items WHERE userid = " + mCharacter.UserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString();
			Server.Instance.CharacterDatabase.RunQuery(query);

			Item item;
			for (byte i = 0; i < mItems.Count; i++) {
				item = GetItem(i);
				if (item == null) continue;

				if (firstrun) {
					query = "INSERT INTO storage_items VALUES (";
					firstrun = false;
				}
				else {
					query += ", (";
				}

				query += mCharacter.UserID.ToString() + ", ";
				query += Server.Instance.WorldID.ToString() + ", ";
				query += (i + 1).ToString() + ", ";
				query += item.ItemID.ToString() + ", ";
				query += item.Amount.ToString() + ", ";
				query += item.Slots.ToString() + ", ";
				query += item.Scrolls.ToString() + ", ";
				query += item.Str.ToString() + ", ";
				query += item.Dex.ToString() + ", ";
				query += item.Int.ToString() + ", ";
				query += item.Luk.ToString() + ", ";
				query += item.HP.ToString() + ", ";
				query += item.MP.ToString() + ", ";
				query += item.Watk.ToString() + ", ";
				query += item.Matk.ToString() + ", ";
				query += item.Wdef.ToString() + ", ";
				query += item.Mdef.ToString() + ", ";
				query += item.Acc.ToString() + ", ";
				query += item.Avo.ToString() + ", ";
				query += item.Hands.ToString() + ", ";
				query += item.Speed.ToString() + ", ";
				query += item.Jump.ToString() + ", '";
				query += MySqlHelper.EscapeString(item.Name.ToString()) + "', ";
				query += item.Expiration.ToString() + ")";
			}
			if (!firstrun) {
				Server.Instance.CharacterDatabase.RunQuery(query);
			}
		}

		public void AddItem(Item item) {
			mItems.Add((byte)mItems.Count, item);
		}

		public void TakeItem(byte slot) {
			mItems.Remove(slot);
		}

		public Item GetItem(byte slot) {
			if (mItems.ContainsKey(slot)) {
				return mItems[slot];
			}
			return null;
		}

		public void SetSlots(byte amount) {
			if (amount < 4) amount = 4;
			else if (amount > 100) amount = 4;
			mSlots = amount;
		}

		public byte GetNumItemsPerInv(byte inv) {
			byte amount = 0;
			foreach (KeyValuePair<byte, Item> kvp in mItems) {
				if (kvp.Value != null && Constants.getInventory(kvp.Value.ItemID) == inv) {
					amount++;
				}
			}
			return amount;
		}

		public void ChangeMesos(int value) {
			int newMesos = 0;
			if (value < 0) { //if value is less than zero 
				if ((mMesos - value) < 0) newMesos = 0;
				else newMesos = mMesos - value; // neg - neg = pos
			}
			else {
				if ((mMesos + value) > int.MaxValue) newMesos = int.MaxValue;
				else newMesos = mMesos - value; //this was the little fucker that fucked everything up
			}
			mMesos = newMesos;

			StoragePacket.SendChangedMesos(mCharacter);
		}
	}
}
