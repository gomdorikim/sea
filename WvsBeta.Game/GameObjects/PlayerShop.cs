using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class PlayerShopItem
    {
        public int Price { get; set; }
        public short Bundles { get; set; }
        public short BundleAmount { get; set; }
        public byte ShopSlot { get; set; }
        public Item sItem { get; set; }

        public PlayerShopItem(Item Item)
        {
            this.sItem = Item;
            this.Price = 0;
            this.Bundles = 0;
            this.ShopSlot = 0;
        }
    }

    public class PlayerShop : MiniRoomBase
    {
        public Dictionary<byte, PlayerShopItem> Items { get; set; }
        public List<PlayerShopItem> BoughtItems { get; set; }
        public int Mesos { get; set; }
        public Character Owner { get; private set; }

        public PlayerShop(Character pOwner) : base(4, RoomType.PersonalShop)
        {
            pOwner = Owner;
        }

        public void AddOwner(Character pOwner)
        {
            EnteredUsers++;
            pOwner.RoomSlotId = GetEmptySlot();
            Users[pOwner.RoomSlotId] = pOwner;
            Items = new Dictionary<byte, PlayerShopItem>();
            BoughtItems = new List<PlayerShopItem>();
        }

        public void AddUser(Character pTo)
        {
            EnteredUsers++;
            pTo.RoomSlotId = GetEmptySlot();
            Users[pTo.RoomSlotId] = pTo;
        }

        public void RevertItems(Character pOwner)
        {
            if (pOwner == pOwner.Room.Users[0])
            {
                foreach (KeyValuePair<byte, PlayerShopItem> pst in Items)
                {
                    if (pst.Value.sItem.Amount != 0) //If an item is set to 0, no point of adding it.
                    {
                        pOwner.Inventory.AddItem2(pst.Value.sItem);
                        pst.Value.sItem = null;
                    }
                }
            }
        }

        public override void RemovePlayer(Character pCharacter, byte pReason)
        {
            if (pCharacter.Room.Type == MiniRoomBase.RoomType.PersonalShop)
            {
                RevertItems(pCharacter);
            }
            base.RemovePlayer(pCharacter, pReason);
        }

        public static void HandleShopUpdateItem(Character pCharacter, byte inv, short invslot, short bundle, short bundleamount, int price)
        {
            MiniRoomBase mrb = pCharacter.Room;
            Item tehItem = pCharacter.Inventory.GetItem(inv, invslot);
            if (tehItem == null)
            {
                //Doesn't have item in inventory
                ReportManager.FileNewReport("Tried adding an item into player shop without having it.", pCharacter.ID, 0);
                InventoryPacket.NoChange(pCharacter);
            }
            Item newItem = new Item(tehItem);
            newItem.InventorySlot = (short)invslot;

            if (newItem == null || newItem.Amount < bundle || bundle <= 0 || bundle > 100)
            {
                //Packet edits 
                ReportManager.FileNewReport("Tried adding an item into player shop with an incorrect amount/incorrect itemid.", pCharacter.ID, 0);
                InventoryPacket.NoChange(pCharacter);
                return;
            }
            else
            {
                PlayerShopItem pst = new PlayerShopItem(newItem);
                pst.Price = price;
                pst.sItem.Amount = (short)(bundle * bundleamount);
                pst.Bundles = bundle;
                pst.BundleAmount = bundleamount;
                mrb.AddItemToShop(pCharacter, pst);
                if (Constants.isStackable(pst.sItem.ItemID))
                {
                        pCharacter.Inventory.TakeItemAmountFromSlot(inv, (short)invslot, (short)(bundle * bundleamount), false);
                }
                else
                {
                    pCharacter.Inventory.TakeItem(pst.sItem.ItemID, bundle);
                }
                InventoryPacket.NoChange(pCharacter); //-.-
            }

        }

        public override void OnPacket(Character pCharacter, byte pOpcode, Common.Sessions.Packet pPacket)
        {
        }

        public override void AddItemToShop(Character pCharacter, PlayerShopItem Item)
        {
            if (Items.Count == 0)
            {
                Items.Add(0, Item);
                Item.ShopSlot = 0;
            }
            else
            {
                Items.Add((byte)Items.Count, Item);
                Item.ShopSlot = (byte)Items.Count;
            }
            PlayerShopPackets.PersonalShopRefresh(pCharacter, this);
            base.AddItemToShop(pCharacter, Item);
        }

        public void HandleMoveItemBack(Character pCharacter, byte slot)
        {
            PlayerShopItem pst = Items[slot];
            pCharacter.Inventory.AddItem2(pst.sItem);
            if (Items.Count == 1)
            {
                PlayerShopPackets.MoveItemToInventory(pCharacter, 0, (short)slot);
            }
            else
            {
                byte left = (byte)(Items.Count - 1); //amount left
                PlayerShopPackets.MoveItemToInventory(pCharacter, left, (short)slot);
            }
            InventoryPacket.NoChange(pCharacter);
        }

        public void BuyItem(Character pCharacter, byte slot, short quantity)
        {
            //This may seem confusing, but the client calculates the amount left itself.
            //The formula is bundles * bundleamount, so if you have 2 bundles with 25 in each, it will obviously show 50. If you have 100 items in 1 bundle, it will show you 100
            PlayerShopItem pst = Items[slot];
            PlayerShop ps = MiniRoomBase.PlayerShops[pCharacter.Room.ID];
            if (pst != null)
            {
                if (pst.BundleAmount > 1)
                {
                    if (pCharacter.Inventory.mMesos > quantity * pst.Price)
                    {
                        if (quantity > 1)
                        {
                            pCharacter.Inventory.AddNewItem(pst.sItem.ItemID, (short)(quantity * pst.BundleAmount));
                            pCharacter.AddMesos(-(quantity * pst.Price));
                            pCharacter.Room.Users[0].AddMesos(quantity * pst.Price);
                            pst.Bundles -= quantity;
                            pst.sItem.Amount = (short)(pst.Bundles * pst.BundleAmount);
                            PlayerShopPackets.PersonalShopRefresh(pCharacter, ps);
                            PlayerShopPackets.SoldItemResult(Users[0], pCharacter, slot, quantity);
                        }
                        else
                        {
                            pCharacter.Inventory.AddNewItem(pst.sItem.ItemID, pst.BundleAmount);
                            pCharacter.AddMesos(-pst.Price);
                            pCharacter.Room.Users[0].AddMesos(pst.Price);
                            pst.Bundles -= quantity;
                            pst.sItem.Amount = (short)(pst.Bundles * pst.BundleAmount);
                            PlayerShopPackets.PersonalShopRefresh(pCharacter, ps);
                            PlayerShopPackets.SoldItemResult(Users[0], pCharacter, slot, quantity);
                        }
                    }
                    else
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.PopupBox, "You don't have enough mesos.", pCharacter, MessagePacket.MessageMode.ToPlayer);
                        InventoryPacket.NoChange(pCharacter);
                    }
                }
                else
                {
                    if (pCharacter.Inventory.mMesos > quantity * pst.Price)
                    {
                        pCharacter.Inventory.AddNewItem(pst.sItem.ItemID, quantity);
                        pCharacter.AddMesos(-(quantity * pst.Price));
                        pCharacter.Room.Users[0].AddMesos(quantity * pst.Price);
                        pst.Bundles -= quantity;
                        pst.sItem.Amount -= quantity;
                        PlayerShopPackets.PersonalShopRefresh(pCharacter, ps);
                        PlayerShopPackets.SoldItemResult(Users[0], pCharacter, slot, quantity);

                    }
                    else
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.PopupBox, "You don't have enough mesos.", pCharacter, MessagePacket.MessageMode.ToPlayer);
                        InventoryPacket.NoChange(pCharacter);
                    }
                }
            }
        }
    }
}
