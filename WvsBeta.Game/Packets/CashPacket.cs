using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class CashPacket
    {
        // Thank you, Bui :D
        public enum RockModes
        {
            Delete = 0x02,
            Add = 0x03
        };

        public enum RockErrors
        {
            CannotGo2 = 0x05, // This is unused
            DifficultToLocate = 0x06,
            DifficultToLocate2 = 0x07, // This is unused
            CannotGo = 0x08,
            AlreadyThere = 0x09,
            CannotSaveMap = 0x0A
        };

        public static void HandleTeleRockFunction(Character chr, Packet packet)
        {
            bool AddCurrentMap = packet.ReadBool();
            if (AddCurrentMap)
            {
                int map = chr.Map;
                if (chr.Inventory.AddRockLocation(map))
                {
                    SendRockUpdate(chr, RockModes.Add);
                }
                else
                {
                    SendRockError(chr, RockErrors.CannotSaveMap);
                }
            }
            else
            {
                int map = packet.ReadInt();
                chr.Inventory.RemoveRockLocation(map);
                SendRockUpdate(chr, RockModes.Delete);
            }
        }

        public static void HandleCashItem(Character chr, Packet packet)
        {
            short slot = packet.ReadShort();
            int itemid = packet.ReadInt();

            Item item = chr.Inventory.GetItem(2, slot);
            if (item == null || item.ItemID != itemid || !DataProvider.Items.ContainsKey(itemid))
            {
                return;
            }

            ItemData data = DataProvider.Items[itemid];
            if (!data.Cash)
            {
                return; // lolwut :P
            }
            bool used = false;
            if (itemid >= 2090000 && itemid <= 2090008)
            {
                string message = packet.ReadString();
                used = DataProvider.Maps[chr.Map].MakeWeatherEffect(itemid, message, new TimeSpan(0, 0, 30));
            }
            else
            {
                switch (itemid)
                {
                    case 2150000:
                        { // Congrats song O.o
                            used = DataProvider.Maps[chr.Map].MakeJukeboxEffect(2150000, chr.Name);
                            break;
                        }
                    case 2150001:
                        { // Swag yolo
                            used = DataProvider.Maps[chr.Map].MakeJukeboxEffect(2150001, chr.Name);
                            break;
                        }
                    case 2150002:
                        { // Swag yolo2
                            used = DataProvider.Maps[chr.Map].MakeJukeboxEffect(2150002, chr.Name);
                            break;
                        }
                    case 2081000:
                        { // Megaphone
                            MessagePacket.SendMegaphoneMessage("", string.Format(packet.ReadString(), chr.Name), true);
                            used = true;
                            break;
                        }
                    case 2082000:
                        { // Super Megaphone
                            Server.Instance.CenterConnection.PlayerSuperMegaphone(chr.Name + " : " + packet.ReadString(), packet.ReadBool());
                            used = true;
                            break;
                        }
                    case 2130000:
                    case 2130001:
                    case 2130002:
                    case 2130003:
                        {
                            if (DataProvider.Maps[chr.Map].Kites.Count > 4)
                            {
                                //Todo : check for character positions..?
                                MapPacket.KiteMessage(chr);
                            }
                            else
                            {
                                foreach (Kite kite in DataProvider.Maps[chr.Map].Kites)
                                {
                                    if (kite.OID == chr.ID)
                                    {
                                        MapPacket.KiteMessage(chr);
                                    }
                                }
                                string message = packet.ReadString();
                                Kite pKite = new Kite(chr, chr.ID, itemid, message, chr.Map);
                                used = true;
                            }
                            break;
                        }
                    case 2140000: // Bronze Meso Sack
                    case 2140001: // Silver Meso Sack
                    case 2140002: // Gold Meso Sack
                        {
                            if (data.Mesos > 0)
                            {
                                int first = chr.Inventory.mMesos;
                                chr.AddMesos(data.Mesos);
                                int amountGot = chr.Inventory.mMesos - first;
                                MiscPacket.SendGotMesosFromLucksack(chr, amountGot);
                                used = true;
                            }
                            break;
                        }
                    case 2170000: // Teleport rock.
                        {
                            byte mode = packet.ReadByte();
                            int map = -1;
                            if (mode == 1)
                            {
                                string name = packet.ReadString();
                                Character target = Server.Instance.GetCharacter(name);
                                if (target != null && target != chr)
                                {
                                    map = target.Map;
                                    used = true;
                                }
                                else
                                {
                                    SendRockError(chr, RockErrors.DifficultToLocate);
                                }
                            }
                            else
                            {
                                map = packet.ReadInt();
                                if (!chr.Inventory.HasRockLocation(map))
                                {
                                    map = -1;
                                }
                            }

                            if (map != -1)
                            {
                                Map from = DataProvider.Maps.ContainsKey(chr.Map) ? DataProvider.Maps[chr.Map] : null;
                                Map to = DataProvider.Maps.ContainsKey(map) ? DataProvider.Maps[map] : null;

                                if (to == from)
                                {
                                    SendRockError(chr, RockErrors.AlreadyThere);
                                }
                                else if (chr.PrimaryStats.Level < 7)
                                {
                                    // Hacks.
                                }
                                else
                                {
                                    chr.ChangeMap(map);
                                    used = true;
                                }
                            }

                            break;
                        }
                    default: FileWriter.WriteLine("etclog\\cash_item_log.log", string.Format("[{0}] {1} used item ({2}) which is unknown to us, packet data: {3}", DateTime.Now.ToString(), chr.Name, itemid, packet.ToString())); break;
                }
            }
            if (used)
            {
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
                    chr.Inventory.TakeItem(item.ItemID, 1);
                    //chr.Inventory.SetItem(2, slot, null);
                    //InventoryPacket.SwitchSlots(chr, slot, 0, 2);
                }
                else
                {
                    InventoryPacket.AddItem(chr, 2, item, false);
                }
            }
            else
            {
                InventoryPacket.NoChange(chr);
            }
        }

        public static void SendRockError(Character chr, RockErrors code)
        {
            Packet pw = new Packet(0x1C);
            pw.WriteByte((byte)code);
            chr.sendPacket(pw);
        }

        public static void SendRockUpdate(Character chr, RockModes mode)
        {
            Packet pw = new Packet(0x1C);
            pw.WriteByte((byte)mode);
            chr.Inventory.AddRockPacket(pw);
            chr.sendPacket(pw);
        }
    }
}