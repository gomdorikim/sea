using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class DropPacket
    {
        public enum DropTypes
        {
            ShowDrop = 0,
            DropAnimation = 1,
            ShowExisting = 2,
            DisappearDuringDrop = 3
        };

        public static void HandleDrops(Character chr, int map, string Dropper, int ObjectID, Pos origin, bool explosive, bool freeforall, bool steal)
        {
            if (!DataProvider.Drops.ContainsKey(Dropper)) return;
            List<DropData> Drops = DataProvider.Drops[Dropper];
            Drops.Shuffle();

            short mod = 25;
            short d = 0;
            Pos DropPos = new Pos();
            foreach (DropData drop in Drops)
            {
                if (drop.ItemID != 0 && !DataProvider.Items.ContainsKey(drop.ItemID) && !DataProvider.Equips.ContainsKey(drop.ItemID))
                {
                    //Program.MainForm.AppendToLogFormat("Server does not contain data for dropid: {0}", drop.ItemID);
                    continue;
                }

                short DropAmount = (short)Server.Instance.Randomizer.ValueBetween(drop.Min, drop.Max);
                Drop realDrops;
                int chance = drop.Chance * Server.dropRate;

                if (steal)
                {
                    chance = chance * 3 / 10;
                }
                int v = Server.Instance.Randomizer.ValueBetween(0, 1000000);
                if (chance > v)
                {
                    if (explosive)
                    {
                        mod = 35;
                    }
                    DropPos.X = (short)(origin.X + ((d % 2) == 1 ? (mod * (d + 1) / 2) : -(mod * (d / 2))));
                    DropPos.Y = origin.Y;
                    DropPos = DataProvider.Maps[map].FindFloor(DropPos);

                    if (drop.Mesos > 0)
                    {
                        int randomValYouGet = (int)Math.Round((double)(Server.Instance.Randomizer.ValueBetween(1, drop.Mesos) / 100.0)) * Server.mesoRate;
                        bool isUp = Server.Instance.Randomizer.ValueBetween(0, 100) > 50;

                        realDrops = new Drop(map, drop.Mesos + (isUp ? randomValYouGet : -randomValYouGet), DropPos, (chr != null ? chr.ID : 0), false, ObjectID);
                    }
                    else
                    {

                        if (DataProvider.Items.ContainsKey(drop.ItemID))
                        {
                            Item itm = new Item();
                            itm.Amount = (short)(DropAmount <= 0 ? 1 : DropAmount);
                            itm.ItemID = drop.ItemID;
                            realDrops = new Drop(map, itm, DropPos, (chr != null ? chr.ID : 0), false, ObjectID);
                        }
                        else
                        {
                            Item itm = new Item();
                            itm.Amount = 1;
                            itm.ItemID = drop.ItemID;
                            itm.GiveStats(true);
                            realDrops = new Drop(map, itm, DropPos, (chr != null ? chr.ID : 0), false, ObjectID);
                        }
                    }

                    if (realDrops != null)
                    {
                        if (explosive)
                        {
                            realDrops.Type = DropType.Explosive;
                        }
                        else if (freeforall)
                        {
                            realDrops.Type = DropType.FreeForAll;
                        }
                        realDrops.Time = 100;
                        realDrops.DoDrop(origin);
                        d++;
                    }
                }
            }

        }

        public static void HandleDropsPremium(Character chr, int map, string Dropper, int ObjectID, Pos origin, bool explosive, bool freeforall, bool steal)
        {
            if (!DataProvider.Drops.ContainsKey(Dropper)) return;
            List<DropData> Drops = DataProvider.Drops[Dropper];
            Drops.Shuffle();

            short mod = 25;
            short d = 0;
            Pos DropPos = new Pos();
            foreach (DropData drop in Drops)
            {
                if (drop.ItemID != 0 && !DataProvider.Items.ContainsKey(drop.ItemID) && !DataProvider.Equips.ContainsKey(drop.ItemID))
                {
                    //Program.MainForm.AppendToLogFormat("Server does not contain data for dropid: {0}", drop.ItemID);
                    continue;
                }
                short DropAmount = (short)Server.Instance.Randomizer.ValueBetween(drop.Min, drop.Max);
                Drop realDrops;
                double chance = drop.Chance * 1.5 * Server.dropRate;

                if (steal)
                {
                    chance = chance * 3 / 10;
                }
                int v = Server.Instance.Randomizer.ValueBetween(0, 1000000);
                if (chance > v)
                {
                    if (explosive)
                    {
                        mod = 35;
                    }
                    DropPos.X = (short)(origin.X + ((d % 2) == 1 ? (mod * (d + 1) / 2) : -(mod * (d / 2))));
                    DropPos.Y = origin.Y;
                    DropPos = DataProvider.Maps[map].FindFloor(DropPos);

                    if (drop.Mesos > 0)
                    {
                        int randomValYouGet = (int)Math.Round((double)(Server.Instance.Randomizer.ValueBetween(1, drop.Mesos) / 100.0)) * Server.mesoRate;
                        bool isUp = Server.Instance.Randomizer.ValueBetween(0, 100) > 50;

                        realDrops = new Drop(map, drop.Mesos + (isUp ? randomValYouGet : -randomValYouGet), DropPos, (chr != null ? chr.ID : 0), false, ObjectID); //meso rate
                    }
                    else
                    {

                        if (DataProvider.Items.ContainsKey(drop.ItemID))
                        {
                            Item itm = new Item();
                            itm.Amount = (short)(DropAmount <= 0 ? 1 : DropAmount);
                            itm.ItemID = drop.ItemID;
                            realDrops = new Drop(map, itm, DropPos, (chr != null ? chr.ID : 0), false, ObjectID);
                        }
                        else
                        {
                            Item itm = new Item();
                            itm.Amount = 1;
                            itm.ItemID = drop.ItemID;
                            itm.GiveStats(true);
                            realDrops = new Drop(map, itm, DropPos, (chr != null ? chr.ID : 0), false, ObjectID);
                        }
                    }

                    if (realDrops != null)
                    {
                        if (explosive)
                        {
                            realDrops.Type = DropType.Explosive;
                        }
                        else if (freeforall)
                        {
                            realDrops.Type = DropType.FreeForAll;
                        }
                        realDrops.Time = 100;
                        realDrops.DoDrop(origin);
                        d++;
                    }
                }
            }

        }

        public static void HandleDropMesos(Character chr, int amount)
        {
            //30 E8 03 00 00 
            if (amount < 10 || amount > 50000 || amount > chr.Inventory.mMesos)
            {
                return;
            }
            chr.AddMesos(-amount, true);

            Pos dpos = DataProvider.Maps[chr.Map].FindFloor(chr.Position);

            Drop drop = new Drop(chr.Map, amount, dpos, chr.ID, true);
            drop.Time = 0;
            // Tradable check here
            drop.DoDrop(chr.Position);
            InventoryPacket.NoChange(chr);
        }

        public static void HandlePickupDrop(Character chr, Packet packet)
        {
            // 5F 18 FF 12 01 00 00 00 00 
            packet.Skip(4); // pos?
            int dropid = packet.ReadInt();
            if (!DataProvider.Maps[chr.Map].Drops.ContainsKey(dropid))
            {
                InventoryPacket.NoChange(chr);
                return;
            }
            Drop drop = DataProvider.Maps[chr.Map].Drops[dropid];
            short pickupAmount = drop.GetAmount();
            if (drop.IsMesos())
            {
                chr.AddMesos(drop.Mesos, true);
            }
            else
            {
                if (chr.Inventory.AddItem2(drop.ItemData) == drop.ItemData.Amount)
                {
                    CannotLoot(chr, -1);
                    InventoryPacket.NoChange(chr); // ._. stupid nexon
                    return;
                }

            }
            CharacterStatsPacket.SendGainDrop(chr, drop.IsMesos(), drop.GetObjectID(), pickupAmount);
            drop.TakeDrop(chr, false);
        }


        public static void ShowDrop(Character chr, Drop drop, byte type, bool newDrop, Pos OriginalPosition)
        {
            Packet pw = new Packet(0x83);
            pw.WriteByte(type);
            pw.WriteInt(drop.ID);
            pw.WriteBool(drop.IsMesos());
            pw.WriteInt(drop.GetObjectID());
            pw.WriteInt(drop.Owner);
            pw.WriteByte((byte)drop.Type);
            pw.WriteShort(drop.Position.X);
            pw.WriteShort(drop.Position.Y);
            pw.WriteInt(drop.DropperID);

            if (type != (byte)DropTypes.ShowExisting)
            {
                pw.WriteShort(OriginalPosition.X);
                pw.WriteShort(OriginalPosition.Y);
                pw.WriteShort(0); // Delay lol?
            }

            if (!drop.IsMesos())
            {
                pw.WriteLong(drop.ItemData.Expiration);
            }
            pw.WriteBool(!drop.PlayerDrop);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            if (chr != null)
            {
                chr.sendPacket(pw);
            }
            else
            {
                DataProvider.Maps[drop.MapID].SendPacket(pw);
            }

            if (newDrop)
            {
                ShowDrop(chr, drop, (byte)DropTypes.ShowDrop, false, OriginalPosition);
            }
        }

        public static void RemoveDrop(Drop drop)
        {
            Packet pw = new Packet(0x84);
            pw.WriteByte(0);
            pw.WriteInt(drop.ID);
            pw.WriteLong(0);
            DataProvider.Maps[drop.MapID].SendPacket(pw);
        }

        public static void ExplodeDrop(Drop drop)
        {
            Packet pw = new Packet(0x84);
            pw.WriteByte(4);
            pw.WriteInt(drop.ID);
            pw.WriteByte(0x8F);
            pw.WriteByte(0x02);
            DataProvider.Maps[drop.MapID].SendPacket(pw);
        }

        public static void MobLootDrop(Drop drop, int mobid)
        {
            Packet pw = new Packet(0x84);
            pw.WriteByte(0x03);
            pw.WriteInt(drop.ID);
            pw.WriteInt(mobid);
            DataProvider.Maps[drop.MapID].SendPacket(pw);
        }

        public static void CannotLoot(Character chr, sbyte reason)
        {
            Packet pw = new Packet(0x1A);
            pw.WriteByte(0);
            pw.WriteSByte(reason);
            chr.sendPacket(pw);
        }

        public static void TakeDrop(Character chr, Drop drop, bool petPickup)
        {
            Packet pw = new Packet(0x84);
            pw.WriteByte((byte)(petPickup ? 0x05 : 0x02));
            pw.WriteInt(drop.ID);
            pw.WriteInt(chr.ID);
            pw.WriteLong(0);
            if (drop.QuestID == 0)
            {
                DataProvider.Maps[drop.MapID].SendPacket(pw);
            }
            else
            {
                chr.sendPacket(pw);
            }
        }
    }
}