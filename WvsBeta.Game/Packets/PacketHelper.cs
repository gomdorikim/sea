using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class PacketHelper
    {

        public static void AddItemDataTest(Packet packet, Item item, short slot)
        {
            packet.WriteByte((byte)slot); 
            int itemType = (item.ItemID / 1000000);
            
            packet.WriteInt(item.ItemID);
            packet.WriteBool(false);
            packet.WriteLong(item.Expiration);

            if (itemType == 1)
            {
                packet.WriteByte(item.Slots);
                packet.WriteByte(item.Scrolls);
                packet.WriteShort(item.Str);
                packet.WriteShort(item.Dex);
                packet.WriteShort(item.Int);
                packet.WriteShort(item.Luk);
                packet.WriteShort(item.HP);
                packet.WriteShort(item.MP);
                packet.WriteShort(item.Watk);
                packet.WriteShort(item.Matk);
                packet.WriteShort(item.Wdef);
                packet.WriteShort(item.Mdef);
                packet.WriteShort(item.Acc);
                packet.WriteShort(item.Avo);
                packet.WriteShort(item.Hands);
                packet.WriteShort(item.Speed);
                packet.WriteShort(item.Jump);
                packet.WriteShort(-1);
            }         
        }
        public static void AddItemData(Packet packet, Item item, short slot, bool shortslot)
        {
            
            if (slot != 0)
            {
                if (shortslot)
                {
                   packet.WriteShort(slot);
                }
                else
                {
                    slot = Math.Abs(slot);
                    if (slot > 100) slot -= 100;
                   packet.WriteByte((byte)slot);
                }
            }
            
            
            int itemType = (item.ItemID / 1000000);
            if (itemType == 5)
            {
                packet.WriteByte(3);
            }
            else
            {
                packet.WriteByte(Constants.getIsEquip(item.ItemID));
            }
            packet.WriteInt(item.ItemID);
            if (item.CashId > 0)
            {
                packet.WriteBool(true);
                packet.WriteLong(item.CashId);
            }
            else
            {
                packet.WriteBool(false);
            }

            packet.WriteLong(Item.NoItemExpiration);

            if (itemType == 1)
            {
                packet.WriteByte(item.Slots);
                packet.WriteByte(item.Scrolls);
                packet.WriteShort(item.Str);
                packet.WriteShort(item.Dex);
                packet.WriteShort(item.Int);
                packet.WriteShort(item.Luk);
                packet.WriteShort(item.HP);
                packet.WriteShort(item.MP);
                packet.WriteShort(item.Watk);
                packet.WriteShort(item.Matk);
                packet.WriteShort(item.Wdef);
                packet.WriteShort(item.Mdef);
                packet.WriteShort(item.Acc);
                packet.WriteShort(item.Avo);
                packet.WriteShort(item.Hands);
                packet.WriteShort(item.Speed);
                packet.WriteShort(item.Jump);
                packet.WriteString(""); //No Clue
            }
            else if (itemType == 5)
            {
                packet.WriteString(item.Pet.Name, 13); //decodeBuffer (13)
                packet.WriteByte(item.Pet.Level); 
                packet.WriteShort(item.Pet.Closeness);
                packet.WriteByte(item.Pet.Fullness);
                packet.WriteLong(item.Pet.Expiration); //decodeBuffer 
            }
            else
            {
                packet.WriteShort(item.Amount);
                packet.WriteString(""); //no clue
            }
        }

        public static void AddItemData2(Packet packet, int itemid, short slot, bool shortslot)
        {

            if (slot != 0)
            {
                if (shortslot)
                {
                    packet.WriteShort(slot);
                }
                else
                {
                    slot = Math.Abs(slot);
                    if (slot > 100) slot -= 100;
                    packet.WriteByte((byte)slot);
                }
            }


            int itemType = (itemid / 1000000);
            packet.WriteByte(Constants.getIsEquip(itemid));
            packet.WriteInt(itemid);
           
                packet.WriteBool(false);
           
            packet.WriteLong(Item.NoItemExpiration);

          
           
                Console.WriteLine("lolol");
                packet.WriteShort(10);
                packet.WriteString("");
            
        }

        public static void AddItemDataWithAmount(Packet packet, Item item, short slot, bool shortslot, short amount)
        {
            if (slot != 0)
            {
                if (shortslot)
                {
                    packet.WriteByte((byte)slot);
                }
                else
                {
                    slot = Math.Abs(slot);
                    if (slot > 100) slot -= 100;
                    packet.WriteByte((byte)slot);
                }
            }

            int itemType = (item.ItemID / 1000000);
            packet.WriteByte(2);
            packet.WriteInt(item.ItemID);
            if (item.CashId > 0)
            {
                packet.WriteBool(true);
                packet.WriteLong(item.CashId);
            }
            else
            {
                packet.WriteBool(false);
            }
            packet.WriteLong(item.Expiration);

            if (itemType == 1)
            {
                Console.WriteLine("lolol " + itemType);
                packet.WriteByte(item.Slots);
                packet.WriteByte(item.Scrolls);
                packet.WriteShort(item.Str);
                packet.WriteShort(item.Dex);
                packet.WriteShort(item.Int);
                packet.WriteShort(item.Luk);
                packet.WriteShort(item.HP);
                packet.WriteShort(item.MP);
                packet.WriteShort(item.Watk);
                packet.WriteShort(item.Matk);
                packet.WriteShort(item.Wdef);
                packet.WriteShort(item.Mdef);
                packet.WriteShort(item.Acc);
                packet.WriteShort(item.Avo);
                packet.WriteShort(item.Hands);
                packet.WriteShort(item.Speed);
                packet.WriteShort(item.Jump);
            }
            else if (itemType == 5)
            {
                packet.WriteString(item.Pet.Name, 13);
                packet.WriteByte(item.Pet.Level);
                packet.WriteShort(item.Pet.Closeness);
                packet.WriteByte(item.Pet.Fullness);
                packet.WriteLong(item.Pet.Expiration);
            }
            else
            {
                Console.WriteLine("lolol");
                packet.WriteShort(amount);
                packet.WriteShort(-1); //flag
            }
        }

        public static void AddItemDataTrade(Packet packet, Item item, short slot, bool shortslot, short amount)
        {
            if (slot != 0)
            {
                if (shortslot)
                {
                    packet.WriteShort(slot);
                }
                else
                {
                    slot = Math.Abs(slot);
                    if (slot > 100) slot -= 100;
                    packet.WriteByte((byte)slot);
                }
            }

            int itemType = (item.ItemID / 1000000);

            packet.WriteInt(item.ItemID);
            if (item.CashId > 0)
            {
                packet.WriteBool(true);
                packet.WriteLong(item.CashId);
            }
            else
            {
                packet.WriteBool(false);
            }
            packet.WriteLong(item.Expiration);

            if (itemType == 1)
            {
                packet.WriteByte(item.Slots);
                packet.WriteByte(item.Scrolls);
                packet.WriteShort(item.Str);
                packet.WriteShort(item.Dex);
                packet.WriteShort(item.Int);
                packet.WriteShort(item.Luk);
                packet.WriteShort(item.HP);
                packet.WriteShort(item.MP);
                packet.WriteShort(item.Watk);
                packet.WriteShort(item.Matk);
                packet.WriteShort(item.Wdef);
                packet.WriteShort(item.Mdef);
                packet.WriteShort(item.Acc);
                packet.WriteShort(item.Avo);
                packet.WriteShort(item.Hands);
                packet.WriteShort(item.Speed);
                packet.WriteShort(item.Jump);
            }
            else if (itemType == 5)
            {
                packet.WriteString(item.Pet.Name, 13);
                packet.WriteByte(item.Pet.Level);
                packet.WriteShort(item.Pet.Closeness);
                packet.WriteByte(item.Pet.Fullness);
                packet.WriteLong(item.Pet.Expiration);
            }
            else
            {
                packet.WriteShort(amount);
            }
        }

        // 12 01 01 00 05 0200 424b4c00 00 0000000000000000 3031323334353637383900000000 01020012 0085012B46E617020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 
        public static bool ParseMovementData(MovableLife life, Packet packet)
        {
            packet.Skip(4); // X and Y...?
            byte amount = packet.ReadByte();
            byte type, Stance = life.Stance;
            short Foothold = life.Foothold, X = life.Position.X, Y = life.Position.Y;
            short startX = life.Position.X;
            short startY = life.Position.Y;
            bool toRet = true;
            bool needCheck = true;

            for (byte i = 0; i < amount; i++)
            {
                type = packet.ReadByte();
                switch (type)
                {
                    case 0x00: //normal move
                    case 0x05:
                        {
                            X = packet.ReadShort();
                            Y = packet.ReadShort();
                            life.Wobble.X = packet.ReadShort();
                            life.Wobble.Y = packet.ReadShort();
                            Foothold = packet.ReadShort();
                            Stance = packet.ReadByte();

                            if (Stance < 5)
                                life.Jumps = 0;
                            if (life.GetType() == typeof(Character) && Stance < 14 && Stance != 6 && Stance != 7)
                            {
                                toRet = CheatInspector.CheckSpeed(life.Wobble, ((Character)life).PrimaryStats.speedMod);
                            }
                            else if (life.GetType() == typeof(Mob) && life.Wobble.Y != 0)
                            {
                                if (Stance == 7 || Stance == 6)
                                    needCheck = false;
                                toRet = CheatInspector.CheckSpeed(life.Wobble, ((Mob)life).AllowedSpeed);
                            }

                            packet.Skip(2);
                            break;
                        }
                    case 0x01: //jump, here we check for jumpingshit
                        {

                            if (life.Jumps > 5) toRet = false;

                            X = packet.ReadShort();
                            Y = packet.ReadShort();
                            Stance = packet.ReadByte();
                            Foothold = packet.ReadShort();
                            life.Jumps++;
                            break;
                        }
                    case 0x02:
                    case 0x06:
                        {
                            X = packet.ReadShort();
                            Y = packet.ReadShort();
                            Stance = packet.ReadByte();
                            Foothold = packet.ReadShort();
                            break;
                        }
                    case 0x03:
                    case 0x04: //tele
                    case 0x07: //assaulter
                        {
                            X = packet.ReadShort();
                            Y = packet.ReadShort();
                            life.Wobble.X = packet.ReadShort();
                            life.Wobble.Y = packet.ReadShort();
                            Stance = packet.ReadByte();

                            if (type == 0x03 && life.GetType() == typeof(Character) && Stance != 7 && Stance != 6)
                            {
                                toRet = CheatInspector.CheckSpeed(life.Wobble, ((Character)life).PrimaryStats.speedMod);
                            }

                            break;
                        }
                    case 0x08:
                        {
                            packet.Skip(1);
                            break;
                        }
                    default:
                        {
                            Stance = packet.ReadByte();
                            //packet.Skip(2);
                            Foothold = packet.ReadShort();
                            break;
                        }
                }
            }
            /*
            if (!toRet) {
                Program.MainForm.AppendToLogFormat("return toRet called 1");
                return toRet;
            }
            */
            if (life.GetType() == typeof(Mob) && needCheck)
            {
                int PastMS = (int)(DateTime.Now - ((Mob)life).lastMove).TotalMilliseconds;
                ((Mob)life).lastMove = DateTime.Now;
                int allowedDistance = (int)((((Mob)life).AllowedSpeed + 0.5f) * PastMS);
                ushort Walkeddistance = (ushort)Math.Abs(X - startX);
                if ((allowedDistance) < Walkeddistance)
                {
                    // return false;
                }
            }

            life.Foothold = Foothold;
            life.Position.X = X;
            life.Position.Y = Y;
            life.Stance = Stance;
            return toRet;
        }


        public static void AddAvatar(Packet pPacket, Character pCharacter)
        {
            pPacket.WriteByte(pCharacter.Gender); 
            pPacket.WriteByte(pCharacter.Skin); 
            pPacket.WriteInt(pCharacter.Face); 
            pPacket.WriteByte(0); // Part of equips lol
            pPacket.WriteInt(pCharacter.Hair);
            pCharacter.Inventory.GeneratePlayerPacket(pPacket);
            pPacket.WriteByte(0xFF); // Equips shown end
            pPacket.WriteInt(pCharacter.GetPetID());
            pPacket.WriteInt(0); //probably ring or item effect
        }
    }
}