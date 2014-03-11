using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class StoragePacket
    {
        public enum StorageErrors
        {
            InventoryFullOrNot = 0x08,
            NotEnoughMesos = 0x0B,
            StorageIsFull = 0x0C
        }
        public static void HandleStorage(Character chr, Packet pr)
        {
            if (chr.TrunkNPCID == 0) return;
            byte opcode = pr.ReadByte();
            Console.WriteLine("operation : " + opcode);
            switch (opcode)
            {
                case 0x04: // Remove
                    {
                        byte inventory = pr.ReadByte();
                        byte slot = pr.ReadByte();
                        Item item = chr.Storage.GetItem(slot);
                        if (item == null)
                        {
                            return;
                        }
                        if (chr.Inventory.HasSlotsFreeForItem(item.ItemID, item.Amount, inventory != 1))
                        {
                            chr.Inventory.AddItem2(new Item(item));
                            chr.Storage.TakeItem(slot);
                            SendChangeItem(chr, inventory, false);
                        }
                        else
                        {
                            SendError(chr, StorageErrors.InventoryFullOrNot);
                        }
                        break;
                    }
                case 0x05: // Add
                    {
                        short slot = pr.ReadShort();
                        int itemid = pr.ReadInt();
                        short amount = pr.ReadShort();
                        NPCData data = DataProvider.NPCs[chr.TrunkNPCID];
                        if (chr.Inventory.mMesos < data.Trunk)
                        {
                            SendError(chr, StorageErrors.NotEnoughMesos);
                            return;
                        }
                        byte inventory = Constants.getInventory(itemid);
                        Item item = chr.Inventory.GetItem(inventory, slot);
                        if (item == null || item.ItemID != itemid)
                        {
                            // hax
                            return;
                        }
                        if (chr.Storage.mItems.Count == chr.Storage.mSlots)
                        {
                            SendError(chr, StorageErrors.StorageIsFull);
                            return;
                        }
                        if (!Constants.isStackable(itemid))
                        {
                            amount = 1;
                        }
                        else if (amount <= 0 || amount > item.Amount)
                        {
                            // More hax.
                            return;
                        }
                        //chr.mStorage.ChangeMesos(data.Trunk);
                        chr.Inventory.TakeItemAmountFromSlot(inventory, slot, Constants.isRechargeable(itemid) ? item.Amount : amount, true);

                        Item tehitem = new Item(item);
                        tehitem.Amount = amount;
                        chr.Storage.AddItem(tehitem);
                        SendChangeItem(chr, inventory, true);
                        chr.AddMesos(-100); //why did you forget this diamondo :P
                        break;
                    }
                case 0x06:
                    {
                        int mesos = pr.ReadInt();
                        if (mesos < 0)
                        {
                            // Store
                            if (Math.Abs(mesos) <= chr.Inventory.mMesos)
                            {
                                chr.AddMesos(mesos);
                                chr.Storage.ChangeMesos(mesos);
                            }
                        }
                        else
                        {
                            // Withdraw
                            if (Math.Abs(mesos) <= chr.Storage.mMesos)
                            {
                                chr.AddMesos(mesos);
                                chr.Storage.ChangeMesos(mesos);
                            }

                        }
                        break;
                    }
               
                default:
                    {
                        string op = "Unknown Storage action: ";
                        foreach (Byte bit in pr.ToArray())
                        {
                            op += string.Format("{0:X2} ", bit);
                        }
                        Console.WriteLine(op);
                        break;
                    }
            }
        }

        public static void SendShowStorage(Character chr, int NPCID)
        {
            Console.WriteLine("storage!!");
            Packet pw = new Packet();
            pw.WriteByte(0xB3);
            pw.WriteInt(NPCID);
            pw.WriteByte(chr.Storage.mSlots);
            pw.WriteShort(0x7E);
            pw.WriteInt(chr.Storage.mMesos);
            AddInvItems(chr, pw, 1);
            AddInvItems(chr, pw, 2);
            AddInvItems(chr, pw, 3);
            AddInvItems(chr, pw, 4);
            AddInvItems(chr, pw, 5); // lolwut :P?!
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            chr.sendPacket(pw);
        }

        public static void SendChangedMesos(Character chr)
        {
            Console.WriteLine("changedmesos");
            Packet pw = new Packet();
            pw.WriteByte(0xB4);
            pw.WriteByte(0x0E);
            pw.WriteByte(chr.Storage.mSlots);
            pw.WriteShort(0x02);
            pw.WriteInt(chr.Storage.mMesos);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            chr.sendPacket(pw);
        }

        public static void SendChangeItem(Character chr, byte inventory, bool add)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xB4);
            pw.WriteByte((byte)(add ? 0x09 : 0x07));
            pw.WriteByte(chr.Storage.mSlots);
            byte type = 0x00;
            switch (inventory)
            {
                case 1: type = 0x04; break;
                case 2: type = 0x08; break;
                case 3: type = 0x10; break;
                case 4: type = 0x20; break;
                case 5: type = 0x40; break;
            }
            if (add) type |= 0x02;
            pw.WriteShort(type);
            if (add) pw.WriteInt(chr.Storage.mMesos);
            AddInvItems(chr, pw, inventory);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            chr.sendPacket(pw);
        }

        public static void SendError(Character chr, StorageErrors what)
        {
            Console.WriteLine("send error");
            Packet pw = new Packet();
            pw.WriteByte(0xB4);
            pw.WriteByte((byte)what);
            chr.sendPacket(pw);
        }

        public static void AddInvItems(Character chr, Packet pw, byte inv)
        {
            pw.WriteByte(chr.Storage.GetNumItemsPerInv(inv));
            Console.WriteLine("{0} has {1} items", inv, chr.Storage.GetNumItemsPerInv(inv));
            foreach (KeyValuePair<byte, Item> kvp in chr.Storage.mItems)
            {
                if (kvp.Value != null && Constants.getInventory(kvp.Value.ItemID) == inv)
                {
                    PacketHelper.AddItemData(pw, kvp.Value, 0, false);
                    Console.WriteLine("{0} | Added {1}", kvp.Key, kvp.Value.ItemID);
                }
            }
        }
    }
}