using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class MiniRoomBase
    {
        public static Dictionary<int, MiniRoomBase> MiniRooms = new Dictionary<int, MiniRoomBase>();
        public static Dictionary<int, PlayerShop> PlayerShops = new Dictionary<int, PlayerShop>();
        public static Dictionary<int, Omok> Omoks = new Dictionary<int, Omok>();

        public enum RoomType : byte
        {
            Omok = 1,
            MemoryGame = 2,
            Trade = 3,
            PersonalShop = 4,
            EntrustedShop = 5,
        }


        public int ID { get; protected set; }
        public int BalloonID { get; protected set; }
        public string Title { get; protected set; }
        public string Password { get; protected set; }
        public byte MaxUsers { get; protected set; }
        public byte EnteredUsers { get; protected set; }
        public Character[] Users { get; protected set; }
        public bool Opened { get; protected set; }
        public bool Private { get; protected set; }
        public bool CloseRequest { get; protected set; }
        public bool GameStarted { get; set; }
        public bool Tournament { get; protected set; }
        public int RoundID { get; protected set; }
        public Pos mHost { get; protected set; }
        public RoomType Type { get; private set; }
        public int ObjectID { get; private set; }
        public byte PieceType { get; private set; }
        public byte mWinnerIndex { get; set; }

        public MiniRoomBase(byte pMaxUsers, RoomType pType)
        {
            ID = Server.Instance.MiniRoomIDs.NextValue();
            MiniRooms.Add(ID, this);
            Title = "";
            Password = "";
            MaxUsers = pMaxUsers;
            Users = new Character[MaxUsers];
            Opened = false;
            CloseRequest = false;
            Tournament = false;
            GameStarted = false;
            Type = pType;
        }

        public void Close(byte pReason)
        {
            MiniRooms.Remove(ID);
        }


        public byte GetEmptySlot()
        {
            for (byte i = 0; i < MaxUsers; i++)
            {
                if (Users[i] == null) return i;
            }
            return 0xFF;
        }

        public byte GetCharacterSlotID(Character pCharacter)
        {
            return pCharacter.RoomSlotId;
        }

        public void BroadcastPacket(Packet pPacket, Character pSkipMeh = null)
        {
            foreach (Character chr in Users) if (chr != null && chr != pSkipMeh) chr.sendPacket(pPacket);
        }

        public bool IsFull() { return EnteredUsers == MaxUsers; }

        public virtual void RemovePlayer(Character pCharacter, byte pReason)
        {
            //Users[pCharacter.RoomSlotId] = null;
            
            MiniRoomPacket.ShowLeave(this, pCharacter, pReason);
            Users[pCharacter.RoomSlotId] = null;
            pCharacter.Room = null;
            pCharacter.RoomSlotId = 0;
            EnteredUsers--;

        }

        public void RemovePlayerFromShop(Character pCharacter)
        {
            MiniRoomBase mrb = pCharacter.Room;
            if (pCharacter == Users[0])
            {
                for (int i = 0; i < EnteredUsers; i++)
                {
                    if (pCharacter != Users[i])
                    {
                        PlayerShopPackets.CloseShop(Users[i], 2);
                        EnteredUsers--;
                        Users[i].Room = null;
                        Users[i].RoomSlotId = 0;
                    }
                    
                }
                PlayerShop ps = PlayerShops[pCharacter.Room.ID];
                ps.RevertItems(pCharacter);
                MiniGamePacket.RemoveAnnounceBox(pCharacter);
                DataProvider.Maps[pCharacter.Map].PlayerShops.Remove(pCharacter.Room.ID);
                pCharacter.Room = null;
                pCharacter.RoomSlotId = 0;
            }
            else
            {
                PlayerShopPackets.RemovePlayer(pCharacter, mrb);
                EnteredUsers--;
                Users[pCharacter.RoomSlotId] = null;
                pCharacter.Room = null;
                pCharacter.RoomSlotId = 0;
            }
        }
        public virtual void AddPlayer(Character pCharacter)
        {
            EnteredUsers++;
            pCharacter.RoomSlotId = GetEmptySlot();
            Users[pCharacter.RoomSlotId] = pCharacter;
        }

        public bool CheckPassword(string pPass) { return Password.Equals(pPass); }

        public virtual void EncodeLeave(Character pCharacter, Packet pPacket)
        {
        }
        public virtual void EncodeEnter(Character pCharacter, Packet pPacket)
        {
        }
        public virtual void EncodeEnterResult(Character pCharacter, Packet pPacket)
        {
        }

        public virtual void OnPacket(Character pCharacter, byte pOpcode, Packet pPacket)
        {
        }

        public virtual void AddItemToShop(Character pCharacter, PlayerShopItem Item)
        {
        }


        public static MiniRoomBase CreateRoom(Character pOwner, byte pType, Packet pPacket, bool pTournament, int pRound)
        {
            switch ((RoomType)pType)
            {
                case RoomType.Trade:
                    {
                        Trade trade = new Trade(pOwner);
                        trade.AddPlayer(pOwner);
                        return trade;
                    }
                case RoomType.Omok:
                    {
                        Omok omok = new Omok(pOwner);
                        omok.Title = pPacket.ReadString();
                        omok.Private = pPacket.ReadBool();
                        if (omok.Private == true)
                        {
                            omok.Password = pPacket.ReadString();
                        }
                        pPacket.Skip(7); //Important ? :S
                        omok.PieceType = pPacket.ReadByte();
                        omok.AddOwner(pOwner);
                        omok.mWinnerIndex = 1;
                        Omoks.Add(omok.ID, omok);
                        return omok;
                    }
                case RoomType.PersonalShop:
                    {
                        PlayerShop ps = new PlayerShop(pOwner);
                        ps.Title = pPacket.ReadString();
                        ps.Private = pPacket.ReadBool();
                        short x = pPacket.ReadShort();
                        ps.ObjectID = pPacket.ReadInt();
                        ps.AddOwner(pOwner);

                        PlayerShops.Add(ps.ID, ps);
                        return ps; 

                    }
                default:
                    return null;
            }
        }
    }
    





    public class Trade : MiniRoomBase
    {
        public class TradeItem
        {
            public Item OriginalItem { get; set; }
        }

        public bool[] Locked;
        private TradeItem[][] ItemList;

        public Character Owner { get; private set; }

        private int[] Mesos;

        public Trade(Character pOwner)
            : base(2, RoomType.Trade)
        {
            Owner = pOwner;
            ItemList = new TradeItem[2][];
            ItemList[0] = new TradeItem[10];
            ItemList[1] = new TradeItem[10];
            Locked = new bool[2] { false, false };
            Mesos = new int[2] { 0 , 0 };

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 10; j++)
                    ItemList[i][j] = null;
        }

        private void RevertItems()
        {
            for (int i = 0; i < 2; i++)
            {
                Character chr = Users[i];
                if (chr == null) continue;

                for (int j = 0; j < 10; j++)
                {
                    TradeItem ti = ItemList[i][j];
                    if (ti != null && ti.OriginalItem != null) //just to make sure that the player actually has items in trade..
                    {
                        chr.Inventory.AddItem2(ti.OriginalItem);
                        ti.OriginalItem = null;
                    }
                }
            }
            
        }

        public void CompleteTrade()
        {
            Character pCharacter1 = Users[0];
            Character pCharacter2 = Users[1];
            AddItems(pCharacter1);
            AddItems(pCharacter2);
            MiniRoomBase mrb = MiniRoomBase.MiniRooms[pCharacter1.Room.ID];
            pCharacter1.Room = null;
            pCharacter1.RoomSlotId = 0;
            pCharacter2.Room = null;
            pCharacter2.RoomSlotId = 0;
            EnteredUsers--;


        }

        private bool ContinueTrade()
        {
            
                if (CheckInventory(Users[0]) && CheckInventory(Users[1])) //Both Inventories are checked, and have room
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }

        private bool CheckInventory(Character chr)
        {
            Dictionary<byte, int> NeededSlots = new Dictionary<byte, int>();
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (j != chr.RoomSlotId)
                    {
                        TradeItem ti = ItemList[j][i];

                        if (ti != null && ti.OriginalItem != null)
                        {
                            byte inv = Constants.getInventory(ti.OriginalItem.ItemID);
                            if (!NeededSlots.ContainsKey(inv))
                            {
                                NeededSlots.Add(inv, 1);
                            }
                            else
                            {
                                NeededSlots[inv] += 1;
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<byte, int> lol in NeededSlots)
            {
                if (chr.Inventory.GetOpenSlotsInInventory(lol.Key) < NeededSlots[lol.Key])
                {
                    return false;
                }
            }
            return true;
        }
 

        private void AddItems(Character chr)
        {
            for (int i = 0; i < 2; i++)
            {
                if (i != chr.RoomSlotId)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        TradeItem ti = ItemList[i][j];
                        if (ti != null && ti.OriginalItem != null)
                        {
                            chr.Inventory.AddItem2(ti.OriginalItem);
                            ti.OriginalItem = null;
                        }
                    }

                    if (Mesos[i] != 0)
                    {
                        chr.AddMesos(Mesos[i]);
                    }
                }
            }
       }
        

        public override void RemovePlayer(Character pCharacter, byte pReason)
        {
            // Give items back
            RevertItems();
            if (Mesos[pCharacter.RoomSlotId] != 0)
            {
                pCharacter.AddMesos(Mesos[pCharacter.RoomSlotId]);
            }
            base.RemovePlayer(pCharacter, pReason);
        }

        public override void OnPacket(Character pCharacter, byte pOpcode, Packet pPacket)
        {
            switch (pOpcode)
            {
                case 13:
                    {
                        //MessagePacket.SendMegaphoneMessage(pCharacter.Name, "hehwat");
                        // Put Item
                        if (!IsFull())
                        {
                            // You can't put items while the second char isn't there yet
                            return;
                        }

                        byte inventory = pPacket.ReadByte();
                        short slot = pPacket.ReadShort();
                        short amount = pPacket.ReadShort();
                        byte toslot = pPacket.ReadByte();
                        Item demItem = pCharacter.Inventory.GetItem(inventory, slot);
                        Item tehItem = new Item(demItem);
                        
                        if (demItem == null || toslot < 1 || toslot > 9) // Todo: trade check
                        {
                            // HAX
                            ReportManager.FileNewReport("Player tried to add an item in trade with to an incorrect slot.", pCharacter.ID, 0);
                            InventoryPacket.NoChange(pCharacter);
                            return;
                        }

                        if (Constants.isRechargeable(demItem.ItemID))
                        {
                            amount = demItem.Amount;
                        }
                       
                        else if (amount < 0 || amount > demItem.Amount)
                        {
                            ReportManager.FileNewReport("Player tried adding an item in trade with an incorrect amount.", pCharacter.ID, 0);
                            InventoryPacket.NoChange(pCharacter);
                            return;
                        }
                        
                        byte charslot = pCharacter.RoomSlotId;
                        
                        if (ItemList[charslot][toslot] == null)
                        {
                            ItemList[charslot][toslot] = new TradeItem()
                            {
                                OriginalItem = tehItem
                            };
                        }

                        tehItem.Amount = amount; 
                        Item pTradeItem = ItemList[charslot][toslot].OriginalItem;

                        if (Constants.isStackable(tehItem.ItemID))
                        {
                            pCharacter.Inventory.TakeItemAmountFromSlot(inventory, slot, amount, false);
                        }
                        else
                        {
                            pCharacter.Inventory.TakeItem(demItem.ItemID, amount); 
                        }
                      
                        TradePacket.AddItem(pCharacter, Users[0], toslot, pTradeItem);
                        TradePacket.AddItem(pCharacter, Users[1], toslot, pTradeItem);
                        InventoryPacket.NoChange(pCharacter); //-.-
                        break;
                    }
                case 14: // Put mesos
                    {
                        int amount = pPacket.ReadInt();
                        if (amount < 0 || pCharacter.Inventory.mMesos < amount)
                        {
                            // HAX
                            ReportManager.FileNewReport("Player tried putting an incorrect meso amount in trade.", pCharacter.ID, 0);
                            return;
                        }

                        pCharacter.AddMesos(-amount, true);
                        Mesos[pCharacter.RoomSlotId] += amount;

                        TradePacket.PutCash(pCharacter, Users[0], amount, 0);
                        TradePacket.PutCash(pCharacter, Users[1], amount, 1);
                        break;
                    }
                case 0xF:
                    {
                        byte charslot = pCharacter.RoomSlotId;
                        Locked[charslot] = true;
                        for (int i = 0; i < 2; i++)
                        {
                            Character chr = Users[i];
                            if (chr != pCharacter)
                            TradePacket.SelectTrade(chr);
                        }
                        if (ContinueTrade() && Locked[0] == true && Locked[1] == true)
                        {
                            Character chr = Users[0];
                            Character chr2 = Users[1];
                            CompleteTrade();

                            TradePacket.TradeSuccessful(chr);
                            TradePacket.TradeSuccessful(chr2);

                            //MessagePacket.SendMegaphoneMessage(pCharacter.Name, "No");
                        }
                        else
                        {
                            if (!ContinueTrade() && Locked[0] == true && Locked[1] == true)
                            {
                                MiniRoomBase mrb = MiniRoomBase.MiniRooms[pCharacter.Room.ID];
                                for (int i = 0; i < 2; i++)
                                {
                                    Character chr = Users[i];
                                    if (chr == null) continue;
                                    mrb.RemovePlayer(chr, 6); 
                                }
                                
                            }
                        }
                        break;
                    }
            }
        }
    }
}
