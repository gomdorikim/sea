using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class InventoryPacket
    {
        public static void HandleUseItemPacket(Character chr, Packet packet)
        {
            short slot = packet.ReadShort();
            int itemid = packet.ReadInt();

            Item item = chr.Inventory.GetItem(2, slot);
            if (item == null || item.ItemID != itemid || !DataProvider.Items.ContainsKey(itemid))
            {
                return;
            }

            ItemData data = DataProvider.Items[itemid];
            if (data.HP > 0)
            {
                chr.ModifyHP(data.HP);
            }
            if (data.MP > 0)
            {
                chr.ModifyMP(data.MP);
            }
            if (data.HPRate > 0)
            {
                chr.ModifyHP((short)(data.HPRate * chr.PrimaryStats.GetMaxHP(false) / 100), true);
            }
            if (data.MPRate > 0)
            {
                chr.ModifyMP((short)(data.MPRate * chr.PrimaryStats.GetMaxMP(false) / 100), true);
            }

            if (data.BuffTime > 0)
            {
                chr.Buffs.AddItemBuff(itemid);
            }
            bool delete = false;
            if (item.Amount <= 1)
            {
                item = null;
                delete = true;
            }
            else
            {
                item.Amount -= 1;
            }
            chr.Inventory.SetItem(2, slot, item);

            if (delete)
            {
                chr.Inventory.SetItem(2, slot, null);
                SwitchSlots(chr, slot, 0, 2);
            }
            else
            {
                AddItem(chr, 2, item, false);
            }
        }

        public static void HandleInventoryPacket(Character chr, Packet packet)
        {
            try
            {
                byte inventory = packet.ReadByte();
                short slot1 = packet.ReadShort();
                short slot2 = packet.ReadShort();

                if (slot1 == 0 || inventory < 0 || inventory > 5)
                    return;
                Item item1 = chr.Inventory.GetItem(inventory, slot1);
                Item item2 = chr.Inventory.GetItem(inventory, slot2);

                if (item1 == null)
                {
                    Console.WriteLine("Item1 = null: {0} {1} {2}  | {3}", inventory, slot1, slot2, chr.ID);
                    return;
                }

                if (slot2 == 0)
                {
                    short amount = packet.ReadShort();
                    Item DroppedItem;
                    bool delete = true;
                    if (Constants.isRechargeable(item1.ItemID) || amount >= item1.Amount)
                    {
                        DroppedItem = item1;
                    }
                    else
                    {
                        DroppedItem = new Item();
                        DroppedItem.Amount = amount;
                        DroppedItem.ItemID = item1.ItemID;
                        item1.Amount -= amount;
                        delete = false;
                    }
                    Pos droppos = DataProvider.Maps[chr.Map].FindFloor(chr.Position);
                    Drop drop = new Drop(chr.Map, DroppedItem, droppos, chr.ID, true, chr.ID);
                    drop.Time = 0;
                    drop.DoDrop(chr.Position, false);

                    if (delete)
                    {
                        chr.Inventory.SetItem(inventory, slot1, null);
                        SwitchSlots(chr, slot1, 0, inventory);
                        
                    }
                    else
                    {
                        AddItem(chr, inventory, item1, false);
                    }
                }
                else
                {
                    if (item1 == null) return;

                    if (item2 != null) //switching slots or adding to stack 
                    {
                        if (Constants.isStackable(item2.ItemID))
                        {
                            if (item2.Amount <= 100 && item2.Amount > 0 && item2.ItemID == item1.ItemID) //adding to stack
                            {
                                short amount = item2.Amount;
                                short leftover = (short)(100 - amount);
                                if (leftover < item1.Amount)
                                {
                                    item2.Amount += leftover;
                                    item1.Amount -= leftover;
                                    AddItem2(chr, inventory, item2, false, item2.Amount);
                                    AddItem2(chr, inventory, item1, false, item1.Amount);
                                }
                                else if (leftover >= item1.Amount)
                                {
                                    item2.Amount += item1.Amount;
                                    AddItem2(chr, inventory, item2, false, item2.Amount);
                                    item1.Amount = 0;
                                    chr.Inventory.TakeItem(item1.ItemID, item1.Amount);
                                    NoChange(chr);
                                }
                                
                            }
                        }
                        else
                        {
                            chr.Inventory.SetItem(inventory, slot1, item2);
                            chr.Inventory.SetItem(inventory, slot2, item1);
                            SwitchSlots(chr, slot1, slot2, inventory);
                            MapPacket.SendPlayerChangeEquips(chr);
                        }
                    }
                    else
                    {
                        chr.Inventory.SetItem(inventory, slot1, item2);
                        chr.Inventory.SetItem(inventory, slot2, item1);
                        SwitchSlots(chr, slot1, slot2, inventory);

                        MapPacket.SendPlayerChangeEquips(chr);
                    }
                    if (inventory == 1 && slot2 >= 0 && slot2 <= 100) //unequip 
                    {
                        
                        if (chr.pRing != null)
                        {
                            chr.pRing.Equipped = false;
                        }
                    }
                    else
                    {
                        if (chr.pRing != null)
                        {
                            chr.pRing.Equipped = true;
                        }
                    }
                    if (inventory == 5 && slot1 == chr.Pets.mSpawned)
                    {
                        chr.Pets.mSpawned = slot2;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[{0}] Exception item movement handler: {1}", chr.ID, ex.ToString());
            }
        }

        public static void HandleUseSummonSack(Character chr, Packet packet)
        {
            short slot = packet.ReadShort();
            int itemid = packet.ReadInt();

            Item item = chr.Inventory.GetItem(2, slot);
            if (item == null || item.ItemID != itemid)
            {
                NoChange(chr);
                return;
            }

            ItemData data = DataProvider.Items[itemid];
            if (data == null || data.Summons.Count == 0)
            {
                NoChange(chr);
                return;
            }

            chr.Inventory.TakeItem(itemid, 1);

            Random rnd = new Random();
            Map map = DataProvider.Maps[chr.Map];
            foreach (ItemSummonInfo isi in data.Summons)
            {
                if (DataProvider.Mobs.ContainsKey(isi.MobID))
                {
                    if (rnd.Next() % 100 < isi.Chance)
                    {
                        map.spawnMobNoRespawn(isi.MobID, chr.Position, 0);
                    }
                }
                else
                {
                    Console.WriteLine("Summon sack {0} has mobid that doesn't exist: {1}", itemid, isi.MobID);
                }
            }
            NoChange(chr);
        }

        public static void HandleUseReturnScroll(Character chr, Packet packet)
        {
            short slot = packet.ReadShort();
            int itemid = packet.ReadInt();

            Item item = chr.Inventory.GetItem(2, slot);
            if (item == null || item.ItemID != itemid)
            {
                NoChange(chr);
                return;
            }

            ItemData data = DataProvider.Items[itemid];
            if (data == null || data.MoveTo == 0)
            {
                NoChange(chr);
                return;
            }
            int map;
            if (data.MoveTo == 999999999 || !DataProvider.Maps.ContainsKey(data.MoveTo))
            {
                map = DataProvider.Maps[chr.Map].ReturnMap;
            }
            else
            {
                map = data.MoveTo;
            }
            byte mappos = 0;
            Random rnd = new Random();
            mappos = (byte)rnd.Next(0, DataProvider.Maps[chr.Map].SpawnPoints.Count);

            chr.Inventory.TakeItem(itemid, 1);

            chr.ChangeMap(map, mappos);
        }

        public static void HandleScrollItem(Character chr, Packet packet)
        {
            short scrollslot = packet.ReadShort();
            short itemslot = packet.ReadShort();

            if (itemslot < -100)
            {
                NoChange(chr);
                return;
            }

            Item scroll = chr.Inventory.GetItem(2, scrollslot);
            Item equip = chr.Inventory.GetItem(1, itemslot);
            if (scroll == null || equip == null || Constants.itemTypeToScrollType(equip.ItemID) != Constants.getScrollType(scroll.ItemID))
            {
                NoChange(chr);
                return;
            }

            ItemData scrollData = DataProvider.Items[scroll.ItemID];
            if (scrollData.ScrollSuccessRate == 0 || equip.Slots == 0)
            {
                NoChange(chr);
                return;
            }
            chr.Inventory.TakeItem(scroll.ItemID, 1);

            Random rnd = new Random();
            if (rnd.Next() % 100 < scrollData.ScrollSuccessRate)
            {
                equip.Str += scrollData.IncStr;
                equip.Dex += scrollData.IncDex;
                equip.Int += scrollData.IncInt;
                equip.Luk += scrollData.IncLuk;
                equip.HP += scrollData.IncMHP;
                equip.MP += scrollData.IncMMP;
                equip.Watk += scrollData.IncWAtk;
                equip.Wdef += scrollData.IncWDef;
                equip.Matk += scrollData.IncMAtk;
                equip.Mdef += scrollData.IncMDef;
                equip.Acc += scrollData.IncAcc;
                equip.Avo += scrollData.IncAvo;
                equip.Jump += scrollData.IncJump;
                equip.Speed += scrollData.IncSpeed;
                equip.Scrolls++;
                equip.Slots--;
                AddItem(chr, 1, equip, true);
                MapPacket.SendPlayerChangeEquips(chr);
                SendItemScrolled(chr, true);
            }
            else
            {
                if (rnd.Next() % 100 < scrollData.ScrollCurseRate)
                {
                    SwitchSlots(chr, itemslot, 0, 1);
                    chr.Inventory.SetItem(1, itemslot, null);
                    SendItemScrolled(chr, false);
                }
                else
                {
                    equip.Slots--;
                    AddItem(chr, 1, equip, true);
                    SendItemScrolled(chr, false);
                }
            }

        }

        public static void SwitchSlots(Character chr, short slot1, short slot2, byte inventory)
        {
            Packet pw = new Packet(0x15);
            pw.WriteByte(0x01);
            pw.WriteByte(0x01);
            pw.WriteByte(0x02);
            pw.WriteByte(inventory);
            pw.WriteShort(slot1);
            pw.WriteShort(slot2);
            pw.WriteByte(0x00);
            chr.sendPacket(pw);
        }

        public static void ShowScrollEffect(Character chr)
        {
            Packet pw = new Packet(0x49);
            pw.WriteInt(chr.ID);
            pw.WriteInt(2044501);
            //pw.WriteInt(2044501);
            chr.sendPacket(pw);
        }

        public static void AddItemTest(Character chr)
        {
            bool isNew = true;
            Packet pw = new Packet(0x15);
            pw.WriteByte(0x01);
            pw.WriteByte(0x01); //if greater than 0 :S
            pw.WriteBool(!isNew);
            pw.WriteByte(4);
            pw.WriteShort(4);

            pw.WriteByte(2); //ITEM
            pw.WriteInt(4000000);
            pw.WriteBool(false);
            pw.WriteLong(Item.NoItemExpiration);
            pw.WriteShort(1);
            pw.WriteString(""); //Owner
            pw.WriteByte(0);
            chr.sendPacket(pw);

        }
        

        public static void AddItem(Character chr, byte inventory, Item item, bool isNew)
        {
            //Console.WriteLine("additem lolol " + inventory);
            Packet pw = new Packet(0x15);
            pw.WriteByte(1);
            pw.WriteByte(1);
            pw.WriteBool(!isNew);
            pw.WriteByte(inventory);
            if (isNew)
            {
                  PacketHelper.AddItemData(pw, item, item.InventorySlot, true);
            }
            else
            {
                pw.WriteShort(item.InventorySlot);
                pw.WriteShort(item.Amount);
                pw.WriteShort(0);
            }
            pw.WriteLong(0x00);
            pw.WriteLong(0x00);
            pw.WriteLong(0x00);
            chr.sendPacket(pw);
        }

        public static void AddItem2(Character chr, byte inventory, Item item, bool isNew, short amount)
        {
            //Console.WriteLine("new item lolol");
            Packet pw = new Packet(0x15);
            pw.WriteByte(0x01);
            pw.WriteByte(0x01);
            pw.WriteBool(!isNew);
            pw.WriteByte(inventory);
            if (isNew)
            {
                PacketHelper.AddItemData(pw, item, item.InventorySlot, true);
            }
            else
            {
               // Console.WriteLine("is not new");
                pw.WriteShort(item.InventorySlot);
                pw.WriteShort(amount);
                pw.WriteShort(0);
            }
            pw.WriteLong(0x00);
            pw.WriteLong(0x00);
            pw.WriteLong(0x00);
            chr.sendPacket(pw);
        }

        public static void NoChange(Character chr)
        {
            Packet pw = new Packet(0x15);
            pw.WriteByte(1);
            pw.WriteByte(0);
            pw.WriteByte(0);
            chr.sendPacket(pw);
        }

        public static void IncreaseSlots(Character chr, byte inventory, byte amount)
        {
            Packet pw = new Packet(0x1C);
            pw.WriteByte(inventory);
            pw.WriteByte(amount);
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }

        public static void SendItemScrolled(Character chr, bool pSuccessfull)
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(6); // 4 = fame
            pw.WriteBool(pSuccessfull);
            chr.sendPacket(pw);
        }

        public static void SendItemsExpired(Character chr, List<int> pExpiredItems) // "The item [name] has been expired, and therefore, deleted from your inventory." * items
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(5);
            pw.WriteByte((byte)pExpiredItems.Count);
            foreach (var item in pExpiredItems) 
                pw.WriteInt(item);
            chr.sendPacket(pw);
        }

        public static void SendItemExpired(Character chr, int pExpiredItem) // "The available time for the cash item [name] has passedand the item is deleted."
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(2);
            pw.WriteInt(pExpiredItem);
            chr.sendPacket(pw);
        }
    }
}