using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Game
{

    public class CharacterInventory
    {
        public Item[][] mEquips { get; set; }

        public Item[][] mItems { get; set; }
        public byte[] mMaxSlots { get; set; }
        public Dictionary<int, short> mItemAmounts { get; set; }
        public int mMesos { get; set; }
        public int[] mTeleRockLocation { get; set; }

        private Character mCharacter { get; set; }

        public CharacterInventory(Character character)
        {
            mCharacter = character;
            mMesos = 0;

            mEquips = new Item[2][];
            for (byte i = 0; i < 2; i++)
            {
                if (i == 1)
                {
                    mEquips[i] = new Item[120]; //Pet equips -.- 
                }
                else
                {
                    mEquips[i] = new Item[17];
                }
            }
            mItemAmounts = new Dictionary<int, short>();
            mMaxSlots = new byte[5]; // 5 inventories!
            mItems = new Item[5][];
            mTeleRockLocation = new int[5];
            for (byte i = 0; i < 5; i++)
            {
                mItems[i] = new Item[101]; // 100 default slots...?
            }
        }


        ~CharacterInventory()
        {
            // Clearing up the Item data for normal inventories
            for (byte i = 0; i < 5; i++)
            {
                for (short j = 0; j < mMaxSlots[i]; j++)
                {
                    mItems[i][j] = null;
                }
                mItems[i] = null;
            }

            // Clearing up equips
            for (byte i = 0; i < 2; i++)
            {
                for (short j = 0; j < 17; j++)
                {
                    mEquips[i][j] = null;
                }
                mEquips[i] = null;
            }
        }

        public void SaveInventory()
        {
            int id = mCharacter.ID;
            string query = "UPDATE characters SET ";
            query += "mesos = " + mMesos.ToString() + " ,";
            query += "equip_slots = " + mMaxSlots[0].ToString() + ", ";
            query += "use_slots = " + mMaxSlots[1].ToString() + ", ";
            query += "setup_slots = " + mMaxSlots[2].ToString() + ", ";
            query += "etc_slots = " + mMaxSlots[3].ToString() + ", ";
            query += "cash_slots = " + mMaxSlots[4].ToString() + " ";
            query += "WHERE ID = " + id.ToString();

            Server.Instance.CharacterDatabase.RunQuery(query);

            query = "DELETE FROM items WHERE charid = " + id.ToString();
            Server.Instance.CharacterDatabase.RunQuery(query);

            query = "DELETE FROM teleport_rock_locations WHERE charid = " + id.ToString();
            Server.Instance.CharacterDatabase.RunQuery(query);

            bool firstrun = true;
            for (int i = 0; i < 5; i++)
            {
                if (firstrun)
                {
                    query = "INSERT INTO teleport_rock_locations VALUES (";
                    firstrun = false;
                }
                else
                {
                    query += ", (";
                }
                query += id.ToString() + ", " + i.ToString() + ", " + mTeleRockLocation[i].ToString() + ")";
            }
            Server.Instance.CharacterDatabase.RunQuery(query);

            firstrun = true;
            Item item;
            for (byte i = 1; i <= 5; i++)
            {
                for (short j = 1; j <= mMaxSlots[i - 1]; j++)
                {
                    item = GetItem(i, j);
                    if (item == null) continue;

                    if (firstrun)
                    {
                        query = "INSERT INTO items VALUES (";
                        firstrun = false;
                    }
                    else
                    {
                        query += ", (";
                    }

                    query += id.ToString() + ", ";
                    query += i.ToString() + ", ";
                    query += item.InventorySlot.ToString() + ", ";
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
            }

            for (short i = 0; i < 2; i++)
            {
                foreach (Item item2 in mEquips[i])
                {
                    item = item2;
                    if (item == null) continue;

                    if (firstrun)
                    {
                        query = "INSERT INTO items VALUES (";
                        firstrun = false;
                    }
                    else
                    {
                        query += ", (";
                    }

                    query += id.ToString() + ", ";
                    query += "1, ";
                    query += item.InventorySlot.ToString() + ", ";
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
            }
            if (!firstrun)
            {
                Server.Instance.CharacterDatabase.RunQuery(query);
            }
        }

        public void LoadInventory()
        {

            using (var data = Server.Instance.CharacterDatabase.RunQuery("SELECT mesos, equip_slots, use_slots, setup_slots, etc_slots, cash_slots FROM characters WHERE id = " + mCharacter.ID.ToString()) as MySqlDataReader)
            {
                data.Read();
                mMesos = data.GetInt32("mesos");
                mMaxSlots[0] = (byte)data.GetInt16("equip_slots");
                mMaxSlots[1] = (byte)data.GetInt16("use_slots");
                mMaxSlots[2] = (byte)data.GetInt16("setup_slots");
                mMaxSlots[3] = (byte)data.GetInt16("etc_slots");
                mMaxSlots[4] = (byte)data.GetInt16("cash_slots");
            }

            using (var data = Server.Instance.CharacterDatabase.RunQuery("SELECT mapindex, mapid FROM teleport_rock_locations WHERE charid = " + mCharacter.ID.ToString()) as MySqlDataReader)
            {
                while (data.Read())
                {
                    mTeleRockLocation[data.GetInt16("mapindex")] = data.GetInt32("mapid");
                }
                for (int i = mTeleRockLocation.Length; i < 5; i++)
                {
                    mTeleRockLocation[i] = 999999999;
                }
            }


            using (var data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM items WHERE charid = " + mCharacter.ID.ToString()) as MySqlDataReader)
            {

                Item item;
                while (data.Read())
                {
                    item = new Item();
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
                    item.CashId = data.GetInt64("cashid");

                    AddItem((byte)data.GetInt16("inv"), data.GetInt16("slot"), item, true);
                }
            }
            foreach (Item it in mItems[4]) // Err.. isPet? lol
            {
                if (it != null && Constants.isPet(it.ItemID)) mCharacter.Pets.LoadPet(it);
            }
        }

        public void AddItem(byte inventory, short slot, Item item, bool isLoading)
        {
            try
            {
                int itemid = item.ItemID;
                item.InventorySlot = slot;
                if (!mItemAmounts.ContainsKey(itemid))
                {
                    mItemAmounts.Add(itemid, item.Amount);
                }
                else
                {
                    mItemAmounts[itemid] += item.Amount;
                }
                if (slot < 0)
                {
                    slot = Math.Abs(slot);
                    if (slot > 100)
                    {
                        mEquips[1][(byte)(slot - 100)] = item;
                    }
                    else
                    {
                        mEquips[0][(byte)slot] = item;
                    }
                    mCharacter.PrimaryStats.AddEquipStarts(slot, item, isLoading);
                }
                else
                {
                    mItems[inventory - 1][slot] = item;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[{0}] {1}", mCharacter.Name, ex.ToString());
            }
        }

        public void SetItem(byte Inventory, short Slot, Item item)
        {
            Inventory -= 1;
            if (item != null) item.InventorySlot = Slot;
            if (Slot < 0)
            {
                Slot = Math.Abs(Slot);

                if (Slot > 100)
                {
                    mEquips[1][(byte)(Slot - 100)] = item;
                }
                else
                {
                    mEquips[0][(byte)Slot] = item;
                    mCharacter.PrimaryStats.AddEquipStarts(Slot, item, false);
                }
            }
            else
            {
                mItems[Inventory][Slot] = item;
            }
        }

        public int GetEquippedItemID(short Slot, bool Cash)
        {
            if (!Cash)
            {
                Slot = Math.Abs(Slot);
                if (mEquips[0].Length > Slot)
                {
                    if (mEquips[0][Slot] != null)
                    {
                        return mEquips[0][Slot].ItemID;
                    }
                }
            }
            else
            {
                if (Slot < -100)
                {
                    Slot += 100;
                }
                Slot = Math.Abs(Slot);
                if (mEquips[1].Length > Slot)
                {
                    if (mEquips[1][Slot] != null)
                    {
                        return mEquips[1][Slot].ItemID;
                    }
                }
            }
            return 0;
        }

        public int GetItemAmount(int itemid)
        {
            int amount = 0;
            Item temp = null;


            for (byte Inventory = 1; Inventory <= 5; Inventory++)
            {
                for (short i = 1; i <= mMaxSlots[Inventory - 1]; i++)
                { // Slot 1 - 24, not 0 - 23
                    temp = GetItem(Inventory, i);
                    if (temp != null && temp.ItemID == itemid) amount += temp.Amount;
                }
            }

            return amount;
        }

        public Item GetItem(byte Inventory, short Slot)
        {
            try
            {
                Inventory -= 1;
                Item itm;
                if (Slot < 0)
                {
                    Slot = Math.Abs(Slot);
                    // Equip.
                    if (Slot > 100)
                    {
                        Console.WriteLine("Moved to slot : " + Slot);
                        itm = mEquips[1][(short)(Slot - 100)];
                    }
                    else
                    {
                        itm = mEquips[0][Slot];
                    }
                }
                else
                {
                    itm = mItems[Inventory][Slot];
                }
                return itm;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't get item {0} {1} mItems[{2}][{3}] , {4}", Inventory, Slot, mItems.Length, mItems[Inventory].Length, ex.ToString());
            }
            return null;
        }

        public short AddItem2(Item item, bool sendpacket = true)
        {
            byte Inventory = Constants.getInventory(item.ItemID);
            short Slot = 0;
            // see if there's a free slot
            Item temp = null;
            short maxSlots = 1;
            if (DataProvider.Items.ContainsKey(item.ItemID))
            {
                maxSlots = (short)DataProvider.Items[item.ItemID].MaxSlot;
                if (maxSlots == 0)
                {
                    // 1, 100 or specified
                    maxSlots = 100;
                }
            }
            for (short i = 1; i <= mMaxSlots[Inventory - 1]; i++)
            { // Slot 1 - 24, not 0 - 23
                temp = GetItem(Inventory, i);
                if (temp != null)
                {
                    if (Constants.isStackable(item.ItemID) && item.ItemID == temp.ItemID && temp.Amount < maxSlots)
                    {
                        if (item.Amount + temp.Amount > maxSlots)
                        {
                            
                            short amount = (short)(maxSlots - temp.Amount);
                            item.Amount -= amount;
                            temp.Amount = (short)maxSlots;
                            if (sendpacket)
                                InventoryPacket.AddItem(mCharacter, Inventory, temp, false);
                        }
                        else
                        {
                            item.Amount += temp.Amount;
                            SetItem(Inventory, i, null);
                            AddItem(Inventory, i, item, false);
                            if (sendpacket)
                                InventoryPacket.AddItem(mCharacter, Inventory, item, false);
                            return 0;
                        }
                    }
                }
                else if (Slot == 0)
                {
                    Slot = i;
                    if (!Constants.isStackable(item.ItemID))
                    {
                        break;
                    }
                }
            }
            if (Slot != 0)
            {
                SetItem(Inventory, Slot, item);
                if (sendpacket)
                    InventoryPacket.AddItem(mCharacter, Inventory, item, true);
                return 0;
            }
            else
            {
                return item.Amount;
            }
        }

        public short AddNewItem(int id, short amount) // Only normal items!
        {
            if (!DataProvider.Items.ContainsKey(id) && !DataProvider.Equips.ContainsKey(id) && !DataProvider.Pets.ContainsKey(id))
            {
                return 0;
            }

            short max = 1;
            if (!Constants.isEquip(id) && !Constants.isPet(id))
            {
                max = (short)DataProvider.Items[id].MaxSlot;
                if (max == 0)
                {
                    max = 100;
                }
            }
            short thisAmount = 0, givenAmount = 0;

            if (Constants.isRechargeable(id))
            {
                thisAmount = max; // + Recharge bonus!
                amount -= 1;
            }
            else if (Constants.isEquip(id) || Constants.isPet(id))
            {
                thisAmount = 1;
                amount -= 1;
            }
            else if (amount > max)
            {
                thisAmount = max;
                amount -= max;
            }
            else
            {
                thisAmount = amount;
                amount = 0;
            }

            if (Constants.isPet(id))
            {
                CreateNewPet(id);
                if (amount > 0)
                {
                    givenAmount += AddNewItem(id, amount);
                }
            }
            else
            {
                Item item = new Item();
                item.Amount = thisAmount;
                item.ItemID = id;
                if (Constants.isEquip(id))
                {
                    item.GiveStats(false);
                }
                givenAmount += thisAmount;
                if (AddItem2(item) == 0 && amount > 0)
                {
                    givenAmount += AddNewItem(id, amount);
                }
            }

            return givenAmount;
        }

        public bool HasSlotsFreeForItem(int itemid, short amount, bool stackable)
        {
            short slotsRequired = 0;
            byte inventory = Constants.getInventory(itemid);
            if (!Constants.isStackable(itemid))
            {
                slotsRequired = amount;
            }
            else
            {
                short maxPerSlot = (short)DataProvider.Items[itemid].MaxSlot;
                if (maxPerSlot == 0) maxPerSlot = 100; // default 100 O.o >_>
                short amountAlready = (short)(mItemAmounts.ContainsKey(itemid) ? mItemAmounts[itemid] : 0);
                if (stackable && amountAlready > 0)
                {
                    //o-o
                }
                else
                {
                    slotsRequired = (short)(amount / maxPerSlot);
                    if ((amount % maxPerSlot) > 0)
                        slotsRequired++;
                }
            }
            return GetOpenSlotsInInventory(inventory) >= slotsRequired;
        }

        public int ItemAmountAvailable(int itemid)
        {
            byte inv = Constants.getInventory(itemid);
            int available = 0;
            short maxPerSlot = (short)(DataProvider.Items.ContainsKey(itemid) ? DataProvider.Items[itemid].MaxSlot : 1); // equip
            if (maxPerSlot == 0) maxPerSlot = 100; // default 100 O.o >_>

            short OpenSlots = GetOpenSlotsInInventory(inv);
            available += (OpenSlots * maxPerSlot);

            Item temp = null;

            for (short i = 1; i <= mMaxSlots[inv - 1]; i++)
            {
                temp = GetItem(inv, i);
                if (temp != null && temp.ItemID == itemid)
                    available += (maxPerSlot - temp.Amount);
            }

            return available;
        }

        public short GetOpenSlotsInInventory(byte inventory)
        {
            short amount = 0;
            for (short i = 1; i <= mMaxSlots[inventory - 1]; i++)
            {
                if (GetItem(inventory, i) == null)
                    amount++;
            }
            return amount;
        }

        public bool AddRockLocation(int map)
        {
            for (int i = 0; i < 5; i++)
            {
                if (mTeleRockLocation[i] == 999999999)
                {
                    mTeleRockLocation[i] = map;
                    return true;
                }
            }
            return false;
        }

        public bool RemoveRockLocation(int map)
        {
            for (int i = 0; i < 5; i++)
            {
                if (mTeleRockLocation[i] == map)
                {
                    mTeleRockLocation[i] = 999999999;
                    return true;
                }
            }
            return false;
        }

        public bool HasRockLocation(int map)
        {
            for (int i = 0; i < 5; i++)
            {
                if (mTeleRockLocation[i] == map)
                {
                    return true;
                }
            }
            return false;
        }


        public void GenerateInventoryTest(Packet packet)
        {
             foreach (Item item in mEquips[0])
                {
                    if (item != null)            /// CASH EQUIP
                    {
                        PacketHelper.AddItemDataTest(packet, item, item.InventorySlot);
                        break;
                    }
                }
                packet.WriteByte(0);
        }

        public void GenerateInventoryPacket(Packet packet)
        {
            try
            {
               // packet.WriteInt(mMesos);

                foreach (Item item in mEquips[0])
                {
                    if (item != null)            /// CASH EQUIP
                    {
                        PacketHelper.AddItemData(packet, item, item.InventorySlot, false);
                        Console.WriteLine(Constants.getIsEquip(1002186).ToString());
                    }
                }
                packet.WriteByte(0);

                foreach (Item item in mEquips[1])
                {
                    if (item != null)            /// REGULAR EQUIP
                    {
                        PacketHelper.AddItemData(packet, item, item.InventorySlot, false);
                    }
                }
                packet.WriteByte(0);


                for (int i = 0; i < 5; i++)
                {
                    
                   // packet.WriteByte(mMaxSlots[i]);
                    foreach (Item item in mItems[i])
                    {
                        if (item != null && item.InventorySlot > 0)
                        {
                            PacketHelper.AddItemData(packet, item, item.InventorySlot, false);
                        }
                    }
                     
                    packet.WriteByte(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[{0}] {1}", mCharacter.Name, ex.ToString());
            }
        }

        public void SetInventorySlots(byte inventory, byte amount)
        {
            if (inventory >= 1 && inventory <= 5)
            {
                if (amount < 24) amount = 24;
                if (amount > 100) amount = 100;
                mMaxSlots[inventory - 1] = amount;

                InventoryPacket.IncreaseSlots(mCharacter, inventory, amount);
            }
        }

        public void TakeItem(int itemid, short amount)
        {
            byte inventory = Constants.getInventory(itemid);
            for (short i = 1; i <= mMaxSlots[inventory - 1]; i++)
            {
                Item item = GetItem(inventory, i);
                if (item == null) continue;
                if (item.ItemID == itemid)
                {
                    if (item.Amount >= amount)
                    {
                        item.Amount -= amount;
                        if (item.Amount == 0 && !Constants.isRechargeable(itemid))
                        {
                            SetItem(inventory, i, null);
                            InventoryPacket.SwitchSlots(mCharacter, i, 0, inventory);
                        }
                        else
                        {
                            InventoryPacket.AddItem(mCharacter, inventory, item, false);
                        }
                        break;
                    }
                    else if (!Constants.isRechargeable(itemid))
                    {
                        amount -= item.Amount;
                        item.Amount = 0;
                        SetItem(inventory, i, null);
                        InventoryPacket.SwitchSlots(mCharacter, i, 0, inventory);
                    }
                }
            }
        }

        public void TakeItemAmountFromSlot(byte Inventory, short slot, short amount, bool takeStars)
        {
            Item item = GetItem(Inventory, slot);
            if (item == null || item.Amount - amount < 0)
            {
                return;
            }

            item.Amount -= amount;
            if ((item.Amount == 0 && !Constants.isRechargeable(item.ItemID)) || (takeStars && Constants.isRechargeable(item.ItemID)))
            {
                SetItem(Inventory, slot, null);
                InventoryPacket.SwitchSlots(mCharacter, slot, 0, Inventory);
            }
            else
            {
                InventoryPacket.AddItem(mCharacter, Inventory, item, false);
            }
        }

        public void GeneratePlayerPacket(Packet packet)
        {
            Dictionary<byte, int> shown = new Dictionary<byte, int>();


            foreach (Item item in mEquips[1])
            {
                if (item != null)
                {
                    byte slotuse = (byte)Math.Abs(item.InventorySlot);
                    if (slotuse > 100) slotuse -= 100;
                    shown.Add(slotuse, item.ItemID);
                }
            }

            foreach (Item item in mEquips[0])
            {
                if (item != null && !shown.ContainsKey((byte)Math.Abs(item.InventorySlot)))
                {
                    shown.Add((byte)Math.Abs(item.InventorySlot), item.ItemID);
                }
            }

            foreach (KeyValuePair<byte, int> kvp in shown)
            {
                packet.WriteByte(kvp.Key);
                packet.WriteInt(kvp.Value);
            }

            shown.Clear();
            shown = null;
        }

        public void AddRockPacket(Packet pw)
        {
            for (int i = 0; i < 5; i++)
            {
                pw.WriteInt(mTeleRockLocation[i]);
            }
        }

        public Dictionary<byte, int> GetVisibleEquips()
        {
            Dictionary<byte, int> shown = new Dictionary<byte, int>();


            foreach (Item item in mEquips[1])
            {
                if (item != null)
                {
                    byte slotuse = (byte)Math.Abs(item.InventorySlot);
                    if (slotuse > 100) slotuse -= 100;
                    shown.Add(slotuse, item.ItemID);
                }
            }

            foreach (Item item in mEquips[0])
            {
                if (item != null && !shown.ContainsKey((byte)Math.Abs(item.InventorySlot)))
                {
                    shown.Add((byte)Math.Abs(item.InventorySlot), item.ItemID);
                }
            }
            return shown;
        }

        public int GetTotalWAttackInEquips(bool star)
        {
            int totalWat = 0;
            foreach (Item item in mEquips[0])
            {
                if (item != null)
                {
                    if (item.Watk > 0)
                    {
                        totalWat += item.Watk;
                    }
                }
            }
            if (star)
            {
                foreach (Item item in mItems[1])
                {
                    if (item != null)
                    {
                        if (Constants.isStar(item.ItemID))
                        {
                            //ok they dont have wat values :S
                            switch (item.ItemID)
                            {
                                case 2070000: totalWat += 15; break;
                                case 2070001:
                                case 2070008: totalWat += 17; break;
                                case 2070002:
                                case 2070009: totalWat += 19; break;
                                case 2070003:
                                case 2070010:
                                case 2070011: totalWat += 21; break;
                                case 2070012:
                                case 2070013: totalWat += 20; break;
                                case 2070004: totalWat += 23; break;
                                case 2070005: totalWat += 25; break;
                                case 2070006:
                                case 2070007: totalWat += 27; break;
                            }
                            break;
                        }
                    }
                }
            }
            Console.WriteLine(totalWat.ToString());
            return totalWat;
        }

        public double GetExtraEXPRate()
        {
            // Holiday stuff here.
            double rate = 1;

            foreach (Item item in this.mItems[3])
            {
                if (item == null || item.ItemID < 4100000 || item.ItemID >= 4200000) continue;
                ItemData id = DataProvider.Items[item.ItemID];
                if (ItemData.RateCardEnabled(id, false))
                {
                    if (rate < id.Rate) rate = id.Rate;
                }
            }
            return rate;
        }

        public void CreateNewPet(int itemid, string petname = "")
        {
            MySqlDataReader data;
            if (petname == string.Empty || petname.Length > 13)
            {
                Server.Instance.CharacterDatabase.RunQuery("SELECT objectname FROM data_ids WHERE objectid = " + itemid.ToString() + " AND objecttype = 'item'");
                data = Server.Instance.CharacterDatabase.Reader;
                if (data.Read())
                {
                    petname = data.GetString("objectname");
                }
                else
                {
                    petname = "UNKNOWNLOL";
                }
            }
            Server.Instance.CharacterDatabase.RunQuery("UPDATE inc_table SET last_cash_serial = last_cash_serial + 1;");
            Server.Instance.CharacterDatabase.RunQuery("SELECT last_cash_serial FROM inc_table;");
            data = Server.Instance.CharacterDatabase.Reader;
            data.Read();
            long cashid = data.GetInt64("last_cash_serial");

            // Create pet

            Item item = new Item();
            item.Amount = 1;
            item.ItemID = itemid;
            item.CashId = cashid;
            AddItem2(item, false);


            Server.Instance.CharacterDatabase.RunQuery("INSERT INTO pets (id, `name`, expiration) VALUES (" + cashid.ToString() + ", '" + MySqlHelper.EscapeString(petname) + "', " + Tools.GetTicksWithAddition(new TimeSpan(90, 0, 0, 0)).ToString() + ")");

            mCharacter.Pets.LoadPet(item);
            InventoryPacket.AddItem(mCharacter, 5, item, true);
            PetsPacket.SendSpawnPet(mCharacter, item.Pet);
        }
    }
}