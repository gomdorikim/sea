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
            if (!DataProvider.Drops.ContainsKey(Dropper))
                return;
            List<DropData> Drops = DataProvider.Drops[Dropper];
            Drops.Shuffle();

            short mod = 25;
            short d = 0;
            Pos DropPos = new Pos();
            bool premiumMap = Map.isPremium(map);
            foreach (DropData drop in Drops)
            {
                if (drop.ItemID != 0 && !DataProvider.Items.ContainsKey(drop.ItemID) && !DataProvider.Equips.ContainsKey(drop.ItemID))
                {
                    //.WriteLine(string.Format("Server does not contain data for dropid: {0}", drop.ItemID));
                    continue;
                }

                if (drop.Premium && !premiumMap)
                {
                    continue; // Premium map drops
                }

                short DropAmount = (short)Server.Instance.Randomizer.ValueBetween(drop.Min, drop.Max);
                Drop realDrops;

                double v_derp = 1000000000.0 / Server.Instance.RateDropChance / (steal ? 0.3 : 1.0);
                Random rd = new Random();

                //double v = Server.Instance.Randomizer.NextSeed() % v_derp;
                double v = rd.NextDouble();
                //Todo : fix drop rate :S
                if (v < drop.Chance)
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
                        int money = drop.Mesos;
                        double v1 = 2 * money / 5 + 1;
                        double v2 = 4 * money / 5;

                        double tmp = v2 + (Server.Instance.Randomizer.NextSeed() % v1);
                        tmp *= Server.Instance.RateMesoAmount;
                        if (tmp < 1.0)
                            tmp = 1.0;

                        realDrops = new Drop(map, (int)Math.Round(tmp), DropPos, (chr != null ? chr.ID : 0), false, ObjectID);
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
                        realDrops.DoDrop(origin, true);
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

            Drop drop = new Drop(chr.Map, amount, dpos, chr.ID, true, chr.ID);
            drop.Time = 0;

            drop.DoDrop(chr.Position, false);
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
            Packet pw = new Packet(0x8E);
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
            Packet pw = new Packet(0x8F);
            pw.WriteByte(0);
            pw.WriteInt(drop.ID);
            pw.WriteLong(0);
            DataProvider.Maps[drop.MapID].SendPacket(pw);
        }

        public static void ExplodeDrop(Drop drop)
        {
            Packet pw = new Packet(0x8F);
            pw.WriteByte(4);
            pw.WriteInt(drop.ID);
            pw.WriteByte(0x8F);
            pw.WriteByte(0x02);
            DataProvider.Maps[drop.MapID].SendPacket(pw);
        }

        public static void MobLootDrop(Drop drop, int mobid)
        {
            Packet pw = new Packet(0x8F);
            pw.WriteByte(0x03);
            pw.WriteInt(drop.ID);
            pw.WriteInt(mobid);
            DataProvider.Maps[drop.MapID].SendPacket(pw);
        }

        public static void CannotLoot(Character chr, sbyte reason)
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(0);
            pw.WriteSByte(reason);
            chr.sendPacket(pw);
        }

        public static void TakeDrop(Character chr, Drop drop, bool petPickup)
        {
            Packet pw = new Packet(0x8F);
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