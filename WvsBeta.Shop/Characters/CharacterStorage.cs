using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Database;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Shop
{
    public class CharacterStorage
    {
        public Character mCharacter { get; set; }
        public Dictionary<byte, Item> mItems { get; set; }
        public Dictionary<long, Item> mCashStorageItems { get; set; }
        public byte mSlots { get; set; }
        public int mMesos { get; set; }
        public int mNX { get; set; }
        public int mMaplePoints { get; set; }

        public CharacterStorage(Character chr)
        {
            mCharacter = chr;
            mItems = new Dictionary<byte, Item>();
            mCashStorageItems = new Dictionary<long, Item>();
        }

        ~CharacterStorage()
        {
            mItems.Clear();
        }

        public void LoadNXValues()
        {
            Server.Instance.CharacterDatabase.RunQuery("SELECT credit_nx, maplepoints FROM storage WHERE userid = " + mCharacter.mUserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());

            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
            data.Read();
            mNX = data.GetInt32(0);
            mMaplePoints = data.GetInt32(1);
        }

        public void Load()
        {
            mCashStorageItems.Clear();
            mItems.Clear();
            Server.Instance.CharacterDatabase.RunQuery("SELECT slots, mesos FROM storage WHERE userid = " + mCharacter.mUserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());

            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
            if (!data.HasRows)
            {
                mSlots = 4;
                mMesos = 0;
                Server.Instance.CharacterDatabase.RunQuery("INSERT INTO storage (userid, world_id) VALUES (" + mCharacter.mUserID.ToString() + ", " + Server.Instance.WorldID.ToString() + ")");
            }
            else
            {
                data.Read();
                mSlots = (byte)data.GetInt16(0);
                mMesos = data.GetInt32(1);
            }

            LoadNXValues();


            Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM storage_items WHERE userid = " + mCharacter.mUserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());
            data = Server.Instance.CharacterDatabase.Reader;
            while (data.Read())
            {
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

            Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM storage_cashshop WHERE userid = " + mCharacter.mUserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());
            data = Server.Instance.CharacterDatabase.Reader;
            while (data.Read())
            {
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
                item.CashId = data.GetInt64("cashid");
                item.Expiration = data.GetInt64("expiration");
                AddCashItem(item);
            }
        }


        public void SaveNXValues()
        {
            Server.Instance.CharacterDatabase.RunQuery("UPDATE storage SET credit_nx = " + mNX + ", maplepoints = " + mMaplePoints + " WHERE userid = " + mCharacter.mUserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());
        }

        public void Save()
        {
            string query = "";
            bool firstrun = true;

            Server.Instance.CharacterDatabase.RunQuery("UPDATE storage SET slots = " + mSlots + ", mesos = " + mMesos + " WHERE userid = " + mCharacter.mUserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString());

            query = "DELETE FROM storage_items WHERE userid = " + mCharacter.mUserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString();
            Server.Instance.CharacterDatabase.RunQuery(query);

            Item item;
            for (byte i = 0; i < mItems.Count; i++)
            {
                item = GetItem(i);
                if (item == null) continue;

                if (firstrun)
                {
                    query = "INSERT INTO storage_items VALUES (";
                    firstrun = false;
                }
                else
                {
                    query += ", (";
                }

                query += mCharacter.mUserID.ToString() + ", ";
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
                query += item.CashId.ToString() + ")";
            }
            if (!firstrun)
            {
                Server.Instance.CharacterDatabase.RunQuery(query);
            }

            query = "DELETE FROM storage_cashshop WHERE userid = " + mCharacter.mUserID.ToString() + " AND world_id = " + Server.Instance.WorldID.ToString();
            Server.Instance.CharacterDatabase.RunQuery(query);

            firstrun = true;
            foreach (KeyValuePair<long, Item> kvp in mCashStorageItems)
            {
                item = kvp.Value;
                if (item == null) continue;

                if (firstrun)
                {
                    query = "INSERT INTO storage_cashshop VALUES (";
                    firstrun = false;
                }
                else
                {
                    query += ", (";
                }

                query += mCharacter.mUserID.ToString() + ", ";
                query += Server.Instance.WorldID.ToString() + ", ";
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
                query += item.CashId.ToString() + ", ";
                query += item.Expiration.ToString() + ")";
            }
            if (!firstrun)
            {
                Server.Instance.CharacterDatabase.RunQuery(query);
            }
        }

        public void AddItem(Item item)
        {
            mItems.Add((byte)mItems.Count, item);
        }

        public void TakeItem(byte slot)
        {
            mItems.Remove(slot);
        }

        public Item GetItem(byte slot)
        {
            if (mItems.ContainsKey(slot))
            {
                return mItems[slot];
            }
            return null;
        }

        public void AddCashItem(Item item)
        {
            mCashStorageItems.Add(item.CashId, item);
        }

        public void TakeCashItem(long serial)
        {
            mCashStorageItems.Remove(serial);
        }

        public Item GetCashItem(long serial)
        {
            if (mCashStorageItems.ContainsKey(serial))
            {
                return mCashStorageItems[serial];
            }
            return null;
        }

        public void SetSlots(byte amount)
        {
            if (amount < 4) amount = 4;
            else if (amount > 100) amount = 4;
            mSlots = amount;
        }

        public byte GetNumItemsPerInv(byte inv)
        {
            byte amount = 0;
            foreach (KeyValuePair<byte, Item> kvp in mItems)
            {
                if (kvp.Value != null && Constants.getInventory(kvp.Value.ItemID) == inv)
                {
                    amount++;
                }
            }
            return amount;
        }

        public void ChangeMesos(int value)
        {
            mMesos -= value;
        }
    }
}