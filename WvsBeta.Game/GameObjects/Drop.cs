using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public enum DropType : byte
    {
        Normal = 0,
        Party = 1,
        FreeForAll = 2,
        Explosive = 3
    }

    public class Drop
    {
        public DropType Type { get; set; }
        public int Time { get; set; }
        public int ID { get; set; }
        public short QuestID { get; private set; }
        public int Owner { get; private set; }
        public int MapID { get; private set; }
        public int Mesos { get; private set; }
        public int PlayerID { get; private set; }
        public bool PlayerDrop { get; private set; }
        public Pos Position { get; private set; }
        public Item ItemData { get; private set; }
        public DateTime Droptime { get; private set; }
        public int DropperID { get; private set; }

        public Drop(int mapid, int mesos, Pos position, int owner, bool playerdrop = false, int dropperid = 0)
        {
            QuestID = 0;
            Owner = owner;
            MapID = mapid;
            ItemData = null;
            Mesos = mesos;
            PlayerID = 0;
            PlayerDrop = playerdrop;
            Type = (byte)DropType.Normal;
            Position = position;
            Droptime = DateTime.Now;
            DropperID = dropperid;

            DataProvider.Maps[MapID].AddDrop(this);
        }


        public Drop(int mapid, Item item, Pos position, int owner, bool playerdrop = false, int dropperid = 0)
        {
            QuestID = 0;
            Owner = owner;
            MapID = mapid;
            ItemData = item;
            PlayerID = 0;
            PlayerDrop = playerdrop;
            Type = (byte)DropType.Normal;
            Position = position;
            Mesos = 0;
            Droptime = DateTime.Now;
            DropperID = dropperid;

            DataProvider.Maps[MapID].AddDrop(this);
        }

        public int GetObjectID()
        {
            return (Mesos > 0 ? Mesos : ItemData.ItemID);
        }

        public short GetAmount()
        {
            return (short)(Mesos > 0 ? 0 : ItemData.Amount);
        }

        public bool IsMesos()
        {
            return ItemData == null;
        }

        public void RemoveDrop(bool showPacket)
        {
            if (showPacket)
            {
                DropPacket.RemoveDrop(this);
            }
            DataProvider.Maps[MapID].RemoveDrop(this);
        }

        public void TakeDrop(Character chr, bool petPickup)
        {
            DropPacket.TakeDrop(chr, this, petPickup);
            DataProvider.Maps[MapID].RemoveDrop(this);
        }

        public void TakeDropMob(int mobid)
        {
            DropPacket.MobLootDrop(this, mobid);
            DataProvider.Maps[MapID].RemoveDrop(this);
        }

        public void ShowDrop(Character chr)
        {
            if (QuestID != 0 && chr.ID != PlayerID)
            {
                return;
            }
            DropPacket.ShowDrop(chr, this, (byte)DropPacket.DropTypes.ShowExisting, false, new Pos());
        }

        public bool Tradeable(int ItemID)
        {
            return DataProvider.UntradeableDrops.Contains(ItemID);
        }

        public void DoDrop(Pos Origin, bool byMonster)
        {
            Time = (int)DateTime.Now.Ticks;
            if (QuestID == 0)
            {
                if (!this.IsMesos() && Tradeable(this.ItemData.ItemID) && !byMonster)
                {
                    //MessagePacket.SendNoticeMap("untradeable", 50000);
                    DropPacket.ShowDrop(null, this, (byte)DropPacket.DropTypes.DisappearDuringDrop, false, Origin);
                }
                else
                {
                    Console.WriteLine("DoDrop");
                    DropPacket.ShowDrop(null, this, (byte)DropPacket.DropTypes.DropAnimation, true, Origin);
                }
            }
            else
            {
                Character chr = DataProvider.Maps[MapID].GetPlayer(PlayerID);
                if (chr != null)
                {
                    Console.WriteLine("DoDrop2");
                    DropPacket.ShowDrop(chr, this, (byte)DropPacket.DropTypes.DropAnimation, true, Origin);
                }
            }
        }
    }
}
