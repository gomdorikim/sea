using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class PetsPacket
    {
        public static void HandleSpawnPet(Character chr, short slot)
        {
            if (chr.Inventory.GetItem(5, slot) == null)
            {
                InventoryPacket.NoChange(chr);
                return;
            }

            if (chr.Pets.mSpawned == slot)
            {
                chr.Pets.mSpawned = 0;
                chr.Inventory.GetItem(5, slot).Pet.Spawned = false;
                SendRemovePet(chr);
            }
            else
            {

                chr.Pets.mSpawned = slot;
                Pet pet = chr.Inventory.GetItem(5, slot).Pet;

                pet.Spawned = true;
                pet.Position = new Pos(chr.Position);
                SendSpawnPet(chr, pet);

            }
            InventoryPacket.NoChange(chr); // ._. stupid nexon
        }

        public static void HandleMovePet(Character chr, Packet packet)
        {
            // 48 00 00 00 00 03 00 00 00 D1 00 00 00 9E 02 00 00 06 E0 01 00 00 00 D7 00 00 00 00 00 00 00 06 09 00 00 00 00 D7 00 00 00 00 00 88 00 04 15 00 00 
            if (chr.Pets.mSpawned == 0 || chr.Inventory.GetItem(5, chr.Pets.mSpawned) == null) return;
            Pet pet = chr.Inventory.GetItem(5, chr.Pets.mSpawned).Pet;
            PacketHelper.ParseMovementData(pet, packet);
            SendMovePet(chr, packet);
        }

        public static void HandleAction(Character chr, Packet packet)
        {
            if (chr.Pets.mSpawned == 0 || chr.Inventory.GetItem(5, chr.Pets.mSpawned) == null) return;
            Pet pet = chr.Inventory.GetItem(5, chr.Pets.mSpawned).Pet;
            // 4A 00 00 
            byte action = packet.ReadByte();
            byte act = packet.ReadByte(); // dunno lol
            if (!DataProvider.Pets.ContainsKey(pet.Item.ItemID) || !DataProvider.Pets[pet.Item.ItemID].Reactions.ContainsKey(act)) return;
            PetReactionData prd = DataProvider.Pets[pet.Item.ItemID].Reactions[act];
            Random luk = new Random();
            bool success = (luk.Next() % 100 < prd.Prob);
            if (success)
            {
                chr.Pets.AddCloseness(prd.Inc);
            }
            SendPetAction(chr, act, success);
        }

        public static void HandlePetLoot(Character chr, Packet packet)
        {
            // 4B 23 06 D7 00 3A 00 00 00
            if (chr.Pets.mSpawned == 0 || chr.Inventory.GetItem(5, chr.Pets.mSpawned) == null) return;
            Pet pet = chr.Inventory.GetItem(5, chr.Pets.mSpawned).Pet;
            packet.Skip(4); // X, Y
            int dropid = packet.ReadInt();
            if (!DataProvider.Maps[chr.Map].Drops.ContainsKey(dropid)) return;
            Drop drop = DataProvider.Maps[chr.Map].Drops[dropid];
            if (!drop.IsMesos() && !chr.Admin) return;

            short pickupAmount = drop.GetAmount();
            if (drop.IsMesos())
            {
                chr.AddMesos(drop.Mesos);
            }
            else
            {
                if (chr.Inventory.AddItem2(drop.ItemData) == drop.ItemData.Amount)
                {
                    DropPacket.CannotLoot(chr, -1);
                    InventoryPacket.NoChange(chr); // ._. stupid nexon
                    return;
                }

            }
            CharacterStatsPacket.SendGainDrop(chr, drop.IsMesos(), drop.GetObjectID(), pickupAmount);
            drop.TakeDrop(chr, true);
        }

        public static void HandlePetChat(Character chr, Packet packet)
        {
            byte invid = packet.ReadByte();
            short what = packet.ReadShort();
            string message = packet.ReadString();
            SendPetChat(chr, message);
            

        }
        public static void HandlePetFeed(Character chr, Packet packet)
        {
            // 26 06 00 40 59 20 00 
        }

        public static void SendPetChat(Character chr, string text)
        {
            Packet pw = new Packet(0x45);
            pw.WriteInt(chr.ID);
            pw.WriteByte(0); //??
            pw.WriteByte(0); //??
            pw.WriteString(text);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void SendPetNamechange(Character chr, string name)
        {
            Packet pw = new Packet(0x4E);
            pw.WriteInt(chr.ID);
            pw.WriteString(name);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        

        public static void SendPetLevelup(Character chr, byte wat = 0)
        {
            Packet pw = new Packet(0x6C);
            pw.WriteByte(0x04);
            pw.WriteByte(wat); // 0 = levelup, 1 = teleport to base, 2 = teleport to your back
            chr.sendPacket(pw);

            pw = new Packet(0x5B);
            pw.WriteInt(chr.ID);
            pw.WriteByte(0x04);
            pw.WriteByte(wat);
            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendPetAction(Character chr, byte action, bool inc)
        {
            Packet pw = new Packet(0x4F);
            pw.WriteInt(chr.ID);
            pw.WriteBool(action == 1);
            pw.WriteByte(action);
            pw.WriteBool(inc);
            if (action == 1)
            {
                pw.WriteShort(0);
            }
            else
            {
                pw.WriteShort(1);
            }
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void SendMovePet(Character chr, Packet buff)
        {
            buff.Reset(1);
            Packet pw = new Packet(0x4C);
            pw.WriteInt(chr.ID);
            pw.WriteBytes(buff.ReadLeftoverBytes());
            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendSpawnPet(Character chr, Pet pet, Character tochar = null)
        {
            Console.WriteLine("spawn pet!");
            // 43 10000000 01 404B4C00 0300312031 3A00000000000000 0000 00 0000  000000000000000000000000000000000000000000000000000000 
            Packet pw = new Packet(0x4B);
            pw.WriteInt(chr.ID);
            pw.WriteBool(true); // Spawns
            pw.WriteInt(pet.Item.ItemID);
            pw.WriteString(pet.Name);
            pw.WriteLong(pet.Item.CashId);
            pw.WriteShort(pet.Position.X);
            pw.WriteShort(pet.Position.Y);
            pw.WriteByte(pet.Stance);
            pw.WriteShort(pet.Foothold);
            pw.WriteLong(0);
            pw.WriteLong(0);
            if (tochar == null)
                DataProvider.Maps[chr.Map].SendPacket(pw);
            else
                tochar.sendPacket(pw);
        }

        public static void SendRemovePet(Character chr, bool gmhide = false)
        {
            Packet pw = new Packet(0x4B);
            pw.WriteInt(chr.ID);
            pw.WriteBool(false);
            pw.WriteLong(0);
            pw.WriteLong(0);
            DataProvider.Maps[chr.Map].SendPacket(pw, (gmhide ? chr : null));
        }
    }
}