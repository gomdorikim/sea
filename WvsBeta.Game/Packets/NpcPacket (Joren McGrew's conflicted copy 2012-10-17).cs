using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class NpcPacket
    {

        public static void HandleNPCChat(Character chr, Packet packet)
        {
            if (chr.NpcSession == null)
                return;

            NpcChatSession session = chr.NpcSession;
            byte state = packet.ReadByte();
            if (state != session.mLastSentType)
            {
                return;
            }

            byte option = packet.ReadByte();
            switch (state)
            {
                case 0:
                    {
                        switch (option)
                        {
                            case 0: // Back button...
                                {
                                    session.SendPreviousMessage();
                                    break;
                                }
                            case 1: // Next button...
                                {
                                    session.SendNextMessage();
                                    break;
                                }
                            default:
                                {
                                    session.Stop();
                                    break;
                                }
                        }
                        break;
                    }
                case 1:
                    {
                        switch (option)
                        {
                            case 0: // No.
                                {
                                    session.HandleThing(session.mRealState, 0, "", 0);
                                    break;
                                }
                            case 1: // Yes.
                                {
                                    session.HandleThing(session.mRealState, 1, "", 0);
                                    break;
                                }
                            default:
                                {
                                    session.Stop();
                                    break;
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        switch (option)
                        {
                            case 0: // No text :(
                                {
                                    session.Stop();
                                    break;
                                }
                            case 1: // Oh yea, text
                                {
                                    session.HandleThing(session.mRealState, 1, packet.ReadString(), 0);
                                    break;
                                }
                            default:
                                {
                                    session.Stop();
                                    break;
                                }
                        }
                        break;
                    }
                case 4:
                case 5:
                    {
                        switch (option)
                        {
                            case 0: // Stopping.
                                {
                                    session.Stop();
                                    break;
                                }
                            case 1: // Got answer
                                {
                                    session.HandleThing(session.mRealState, packet.ReadByte(), "", 0);
                                    break;
                                }
                            default:
                                {
                                    session.Stop();
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        session.Stop();
                        string op = "Unknown NPC chat action: ";
                        foreach (Byte bit in packet.ToArray())
                        {
                            op += string.Format("{0:X2} ", bit);
                        }
                        Program.MainForm.LogAppendFormat(op);
                        break;
                    }

            }
        }

        public static void HandleNPCShop(Character chr, Packet packet)
        {
            if (chr.ShopNPCID == 0) return;

            byte type = packet.ReadByte();
            switch (type)
            {
                case 0x00: // Buy item
                    {
                        short slot = packet.ReadShort();
                        int itemid = packet.ReadInt();
                        short amount = packet.ReadShort();
                        if (slot >= 0 && slot < DataProvider.NPCs[chr.ShopNPCID].Shop.Count)
                        {
                            ShopItemData sid = DataProvider.NPCs[chr.ShopNPCID].Shop[slot];
                            int costs = amount * sid.Price;
                            /*if (sid.Stock == 0) {
                                SendShopResult(chr, 5);
                                return;
                            }
                            else*/
                            if (sid.ID != itemid)
                            {
                                SendShopResult(chr, 4);
                                return;
                            }
                            else if (costs > chr.Inventory.mMesos)
                            {
                                SendShopResult(chr, 4);
                                return;
                            }
                            else
                            {
                                short buyAmount = amount;
                                if (Constants.isRechargeable(itemid))
                                {
                                    short amountOnSet = (short)DataProvider.Items[itemid].MaxSlot;
                                    if (amount > amountOnSet) // You can't but multiple sets at once
                                    {
                                        SendShopResult(chr, 4);
                                        return;
                                    }
                                }
                                bool hasEnoughSlots = chr.Inventory.HasSlotsFreeForItem(itemid, buyAmount, true);
                                if (chr.Inventory.HasSlotsFreeForItem(itemid, buyAmount, true))
                                {
                                    chr.Inventory.AddNewItem(itemid, buyAmount);
                                    SendShopResult(chr, 0);
                                    sid.Stock -= buyAmount;
                                    chr.AddMesos(-costs);

                                    return;
                                }
                                else
                                {
                                    SendShopResult(chr, 3);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            SendShopResult(chr, 7);
                        }
                        break;
                    }
                case 0x01: // Sell item
                    {
                        short itemslot = packet.ReadShort();
                        int itemid = packet.ReadInt();
                        short amount = packet.ReadShort();
                        byte inv = Constants.getInventory(itemid);
                        Item item = chr.Inventory.GetItem(inv, itemslot);

                        

                        if (item == null || item.ItemID != itemid || amount > item.Amount)
                        {
                            chr.mPlayer.Socket.Disconnect();
                            return;
                        }
                        else
                        {


                            int Cash = 0;
                            if (inv == 1)
                            {
                                EquipData ed = DataProvider.Equips[itemid];
                                if (ed == null)
                                {
                                    chr.mPlayer.Socket.Disconnect();
                                    return;
                                }
                                else
                                {
                                    Cash = ed.Price;
                                }
                            }
                            else
                            {
                                ItemData id = DataProvider.Items[itemid];
                                if (id == null)
                                {
                                    chr.mPlayer.Socket.Disconnect();
                                    return;
                                }
                                else
                                {
                                    Cash = id.Price * amount;
                                }
                            }
                            // Change amount here.
                            if (Constants.isRechargeable(item.ItemID))
                            {
                                amount = item.Amount;
                            }

                            if (amount == item.Amount)
                            {
                                chr.Inventory.SetItem(inv, itemslot, null);
                                InventoryPacket.SwitchSlots(chr, itemslot, 0, inv);

                            }
                            else
                            {
                                item.Amount -= amount;
                                InventoryPacket.AddItem(chr, inv, item, false);
                            }
                            chr.AddMesos(Cash);
                            SendShopResult(chr, 0);
                        }
                        break;
                    }
                case 0x02: // recharge
                    {
                        short itemslot = packet.ReadShort();

                        byte inv = 2;
                        Item item = chr.Inventory.GetItem(inv, itemslot);
                        if (item == null || !Constants.isRechargeable(item.ItemID))
                        {
                            chr.mPlayer.Socket.Disconnect();
                            return;
                        }
                        else
                        {
                            ItemData data = DataProvider.Items[item.ItemID];
                            int Cash = 1;
                            short maxslot = (short)(data.MaxSlot + chr.Skills.GetRechargeableBonus());

                            Cash = (int)(-1 * 5.0 * maxslot);
                            if ((Cash < 0) && (chr.Inventory.mMesos > -Cash))
                            {
                                chr.AddMesos(Cash);
                                item.Amount = maxslot;
                                InventoryPacket.AddItem(chr, inv, item, false);
                                SendShopResult(chr, 0);
                            }
                        }
                        break;
                    }
                case 0x03: chr.ShopNPCID = 0; chr.NpcSession = null; break;
                default:
                    {
                        string op = "Unknown NPC shop action: ";
                        foreach (Byte bit in packet.ToArray())
                        {
                            op += string.Format("{0:X2} ", bit);
                        }
                        Program.MainForm.LogAppendFormat(op);
                        break;
                    }

            }

        }

        public static void SendShowNPCShop(Character chr, int NPCID)
        {
            Packet pw = new Packet(0xA3);
            pw.WriteInt(NPCID);

            List<ShopItemData> ShopItems = DataProvider.NPCs[NPCID].Shop;

            short maxSlots = 1;

            pw.WriteShort((short)ShopItems.Count);
            foreach (ShopItemData item in ShopItems)
            {
                pw.WriteInt(item.ID);
                pw.WriteInt(item.Price);
                if (Constants.isRechargeable(item.ID))
                {
                    pw.WriteLong(BitConverter.DoubleToInt64Bits(5.0));
                }
                if (DataProvider.Items.ContainsKey(item.ID))
                {
                    maxSlots = (short)DataProvider.Items[item.ID].MaxSlot;
                    if (maxSlots == 0)
                    {
                        // 1, 100 or specified
                        maxSlots = 100;
                    }
                }

                pw.WriteShort(maxSlots);
            }
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }

        public static void SendShopResult(Character chr, byte ans)
        {
            Packet pw = new Packet(0xA4);
            pw.WriteByte(ans);

            chr.sendPacket(pw);
        }

        public static void SendNPCChatTextSimple(Character chr, int NpcID, string Text, bool back, bool next)
        {
            chr.NpcSession.mLastSentType = 0;
            Packet pw = new Packet(0xA0);
            pw.WriteByte(0x04);
            pw.WriteInt(NpcID);
            pw.WriteByte(0);
            pw.WriteString(Text);
            pw.WriteBool(back);
            pw.WriteBool(next);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }

        public static void SendNPCChatTextMenu(Character chr, int NpcID, string Text)
        {
            chr.NpcSession.mLastSentType = 4;
            Packet pw = new Packet();
            pw.WriteByte(0xA0);
            pw.WriteByte(0x04);
            pw.WriteInt(NpcID);
            pw.WriteByte(0x04);
            pw.WriteString(Text);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }

        public static void SendNPCChatTextYesNo(Character chr, int NpcID, string Text)
        {
            chr.NpcSession.mLastSentType = 1;
            Packet pw = new Packet(0xA0);
            pw.WriteByte(0x04);
            pw.WriteInt(NpcID);
            pw.WriteByte(0x01);
            pw.WriteString(Text);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }

        public static void SendNPCChatTextRequestText(Character chr, int NpcID, string Text, string Default, short MinLength, short MaxLength)
        {
            chr.NpcSession.mLastSentType = 2;
            Packet pw = new Packet(0xA0);
            pw.WriteByte(0x04);
            pw.WriteInt(NpcID);
            pw.WriteByte(0x02);
            pw.WriteString(Text);
            pw.WriteString(Default);
            pw.WriteShort(MinLength);
            pw.WriteShort(MaxLength);
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }

        public static void SendNPCChatTextRequestInteger(Character chr, int NpcID, string Text, int Default, int MinValue, int MaxValue)
        {
            chr.NpcSession.mLastSentType = 3;
            Packet pw = new Packet(0xA0);
            pw.WriteByte(0x04);
            pw.WriteInt(NpcID);
            pw.WriteByte(0x03);
            pw.WriteString(Text);
            pw.WriteInt(Default);
            pw.WriteInt(MinValue);
            pw.WriteInt(MaxValue);
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }

        public static void SendNPCChatTextRequestStyle(Character chr, int NpcID, string Text, List<int> values)
        {
            chr.NpcSession.mLastSentType = 5;
            Packet pw = new Packet(0xA0);
            pw.WriteByte(0x04);
            pw.WriteInt(NpcID);
            pw.WriteByte(0x05);
            pw.WriteString(Text);
            pw.WriteByte((byte)values.Count);
            foreach (int value in values)
            {
                pw.WriteInt(value);
            }
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }
    }
}