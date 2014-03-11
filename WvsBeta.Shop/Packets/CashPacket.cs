using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Shop
{
    public class CashPacket
    {
        public enum CashErrors
        {
            UnknownError = 0x00,
            UnknownErrorDC_1 = 0x50,
            TimeRanOutTryingToProcessRequest_TryAgain,
            NotEnoughCash,
            CantGiftUnder14Year,
            ExceededAllottedLimitOfPriceForGifts,
            CheckExceededNumberOfCashItems,
            CheckCharacterNameOrItemRestrictions,
            CheckCouponNumber,

            DueGenderRestrictionsNoCouponUse = 0x5B,
            CouponOnlyForRegularItemsThusNoGifting,
            CheckFullInventory,
            ItemOnlyAvailableForUsersAtPremiumInternetCafe,
            CoupleItemsCanBeGivenAsAGiftToACharOfDiffGenderAtSameWorld,
            ItemsAreNotAvailableForPurchaseAtThisHour,
            OutOfStock,
            ExceededSpendingLimitOfCash,
            NotEnoughMesos,
            UnavailableDuringBetaTestPhase
        }


        public static void HandleCashPacket(Character chr, Packet packet)
        {

            SendError(chr, CashErrors.UnavailableDuringBetaTestPhase);
            short header = packet.ReadByte();
            switch (header)
            {
                case 0x02:
                    {
                       
                        
                        break;
                    }
                case 0x04:
                    {
                        chr.mWishlist.Clear();
                        for (byte i = 0; i < 10; i++)
                        {
                            int val = packet.ReadInt();
                            if (val != 0)
                            {
                                chr.mWishlist.Add(val);
                            }
                        }
                        SendWishlist(chr, true);
                        break;
                    }
                case 0x0B:
                    {
                        long cashid = packet.ReadLong();
                        byte inv = packet.ReadByte();
                        Item invitem = chr.mInventory.GetCashItem(cashid, inv);
                        if (invitem == null)
                        {
                            SendError(chr, CashErrors.UnknownErrorDC_1);
                        }
                        else
                        {
                            Item item = new Item(invitem);
                            if (invitem.Pet != null)
                            {
                                item.Name = item.Pet.Name; // Cashshop thingy
                                if (chr.mPets.GetEquippedPet() == item.Pet)
                                {
                                    invitem.Pet = null;
                                    chr.mPets.mPets.Remove(invitem);
                                    chr.mPets.mSpawned = 0;
                                    item.Pet.Spawned = false;
                                    chr.mPets.Save();
                                }
                            }
                            chr.mInventory.TakeItemAmountFromSlot(inv, invitem.InventorySlot, invitem.Amount, false);
                            chr.mStorage.AddCashItem(item);
                            SendPlacedItemInShop(chr, item);
                        }
                        break;
                    }
                case 0x0A:
                    {
                        long cashid = packet.ReadLong();
                        byte inv = packet.ReadByte();
                        short slot = packet.ReadShort();
                        if (!chr.mStorage.mCashStorageItems.ContainsKey(cashid) || inv < 1 || inv > 5 || slot < 0 || slot > 100 || chr.mInventory.mMaxSlots[inv - 1] < slot || chr.mInventory.GetItem(inv, slot) != null)
                        {
                            Item item = new Item(chr.mStorage.GetCashItem(cashid));
                            if (Constants.isPet(item.ItemID))
                            {
                                chr.mPets.LoadPet(item);
                                chr.mInventory.AddItem(inv, slot, item, false);
                                chr.mStorage.TakeCashItem(cashid);
                                SendPlacedItemInInventory(chr, item);
                                Program.MainForm.appendToLog("cashid : " + cashid);
                            }
                            else
                            {
                                SendError(chr, CashErrors.UnknownErrorDC_1);
                            }
                            //SendError(chr, CashErrors.UnknownErrorDC_1);
                        }
                        else
                        {
                            Item item = new Item(chr.mStorage.GetCashItem(cashid));
                            chr.mInventory.AddItem(inv, slot, item, false);
                            chr.mStorage.TakeCashItem(cashid);
                            SendPlacedItemInInventory(chr, item);
                            Program.MainForm.appendToLog("cashid : " + cashid);
                        }
                        break;
                    }
                default:
                    {
                        string what = "[" + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString("D3") + "] Unknown packet found: " + packet.ToString();
                        FileWriter.WriteLine(@"connection_log\" + chr.mID.ToString() + ".txt", what, true);
                        Console.WriteLine("Unknown packet received! " + packet.ToString());

                        break;
                    }
            }
        }

        public static void SendWishlist(Character chr, bool update)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xBB);
            pw.WriteByte((byte)(update ? 0x20 : 0x1E));
            int i = (byte)chr.mWishlist.Count;
            foreach (int val in chr.mWishlist)
            {
                pw.WriteInt(val);
            }
            for (; i <= 10; i++)
            {
                pw.WriteInt(0);
            }
            chr.sendPacket(pw);
        }

        public static void SendInfo(Character chr)
        {
            SendCashAmounts(chr);
            //SendWishlist(chr, false);
            //SendItems(chr);
            //ReceiveCouponNotice(chr);
           // ShowGifts(chr);
            //GiftTest(chr);
            //What(chr);
            //What2(chr);
            //What4(chr);
           // Unk(chr);
            //Unk2(chr);
            //Unk3(chr);
        }

        public static void SendItems(Character chr)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xC7);
            pw.WriteByte(0x1B);
            pw.WriteShort((short)chr.mStorage.mCashStorageItems.Count);

            foreach (KeyValuePair<long, Item> kvp in chr.mStorage.mCashStorageItems)
            {
                PacketHelper.AddCashItemData(chr, pw, kvp.Value, false);
            }
            pw.WriteShort(0); //for storage, 98 bytes OR GIFT ITEMS ????
            pw.WriteShort(0); //character slots
            chr.sendPacket(pw);
        }

        public static void SendBoughtItem(Character chr, Item item)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xC7);
            pw.WriteByte(0x21);
            PacketHelper.AddCashItemData(chr, pw, item, false);
            chr.sendPacket(pw);
        }

        public static void SendGiftsTest(Character chr)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xC7);
            pw.WriteByte(0x1D);
            pw.WriteShort(1);
            //PacketHelper.AddGiftItemTest(chr, pw);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            chr.sendPacket(pw);
        }

        public static void ReceiveCouponNotice(Character chr)
        {
            //Using coupon
            Packet pw = new Packet();
            pw.WriteByte(0xC7);
            pw.WriteByte(0x23);
            pw.WriteByte(1); //items size 

            pw.WriteLong(0); //Cash ID    //8
            pw.WriteInt(chr.mUserID);  //12
            pw.WriteHexString("01 01 01 01"); //Basically user ID is a long..  //16
            pw.WriteInt(1002186); //Item ID  //20
            pw.WriteHexString("01 01 01 01"); //SN //24
            pw.WriteShort(1);  //19 //Amount //26
            pw.WriteString("", 13); // //39
            pw.WriteLong(150842304000000000L); //47 //Expiration
            pw.WriteLong(0);

            pw.WriteInt(100);
            pw.WriteInt(100);
            pw.WriteInt(100);
            chr.sendPacket(pw);

        }
        public static void SendPlacedItemInInventory(Character chr, Item item)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xBB);
            pw.WriteByte(0x2F);
            pw.WriteShort(item.InventorySlot);
            pw.WriteByte(Constants.getItemTypeInPacket(item.ItemID));
            PacketHelper.AddItemData(pw, item, 0, false);
            chr.sendPacket(pw);
        }

        public static void SendPlacedItemInShop(Character chr, Item item)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xBB);
            pw.WriteByte(0x31);
            PacketHelper.AddCashItemData(chr, pw, item, false);
            chr.sendPacket(pw);
        }


        public static void SendError(Character chr, CashErrors error, int v = 0)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xC7);
            pw.WriteByte(0x22);
            pw.WriteByte((byte)error);
            pw.WriteInt(v);

            chr.sendPacket(pw);
        }

        public static void SendCashAmounts(Character chr)
        {
            //C5 gives error 38 0.0
            Packet pw = new Packet();
            pw.WriteByte(0xC6);
            pw.WriteInt(213030303);
            pw.WriteInt(chr.mStorage.mMaplePoints); //MapleCash is disabled :S
            chr.sendPacket(pw);
        }

        public static void ShowGiftSucceed(Character chr, bool failed, string toname, int iID, short iAmount, int price)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xBB);
            if (failed)
            {
                pw.WriteByte(0x28);
            }
            else
            {
                pw.WriteByte(0x29);
            }
            pw.WriteString(toname);
            pw.WriteInt(iID);
            pw.WriteShort(iAmount);
            pw.WriteInt(price);
            chr.sendPacket(pw);
        }

        public static void ShowGifts(Character chr)
        {
            //DecodeBuffer (40 bytes)
            Packet pw = new Packet();
            pw.WriteByte(0xBB);
            pw.WriteByte(0x1E);
            /**
            //pw.WriteShort(0);
            Item item = new Item(chr.mStorage.GetCashItem(42));
            PacketHelper.AddGiftList(pw, item);
             * **/
            pw.WriteString("fasfsa", 13);
            pw.WriteString("asfas", 73);
            chr.sendPacket(pw);
            
        }

        public static void GiftTest(Character chr)
        {

            //This minimizes your client :O 
            Packet pw = new Packet();
            pw.WriteByte(0xB9);
            pw.WriteString("C:\\Program Files (x86)\\Internet Explorer\\iexplore.exe");
            pw.WriteString("C:\\Program Files (x86)\\Internet Explorer\\iexplore.exe"); //path? xd
            pw.WriteByte(1);
            pw.WriteShort(1);
            pw.WriteInt(5000000);
            pw.WriteInt(5000000);
            chr.sendPacket(pw);
        }

        public static void What(Character chr)
        {
            //You have given "loltest" (null)
            Packet pw = new Packet();
            pw.WriteByte(0xBB);
            pw.WriteByte(0x48);
            pw.WriteByte(1);
            //48 bytes
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteShort(0);
            pw.WriteString("loltest");
            pw.WriteInt(chr.mUserID);
            pw.WriteShort(0);
            chr.sendPacket(pw);





        }

        public static void What2(Character chr)
        {
            //An error occured trying to communicate with the server, please try again later.
            Packet pw = new Packet();
            pw.WriteByte(0xBB);
            pw.WriteByte(0x2D);
            pw.WriteByte(0);
            pw.WriteShort(0);
            chr.sendPacket(pw);
        }

        public static void What3(Character chr)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xB1);
            pw.WriteByte(0);
            pw.WriteByte(1);
            pw.WriteByte(1);
            chr.sendPacket(pw);
        }

        public static void Unk(Character pCharacter)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xB3);
            pw.WriteByte(1);
            pw.WriteByte(1);
            pw.WriteInt(pCharacter.mUserID);
            pw.WriteInt(pCharacter.mID);
            pCharacter.sendPacket(pw);
        }

        public static void Unk2(Character pCharacter)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xB4);
            pw.WriteByte(1);
            pCharacter.sendPacket(pw);
        }

        public static void Unk3(Character pCharacter)
        {
            Packet pw = new Packet(0xB5);
            pw.WriteByte(1);
            pw.WriteByte(1);
            pw.WriteByte(1);
            pw.WriteInt(pCharacter.mID);
            pw.WriteString(pCharacter.mName);
            pCharacter.sendPacket(pw);

        }

        public static void What4(Character chr)
        {
            Packet pw = new Packet();
            pw.WriteByte(0xB2);

            /**
            pw.WriteLong(1);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            //20
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            //30
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            //40
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            //50
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            //60
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            //70
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            //80
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            //90
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
             * **/

            pw.WriteByte(0); //because 768 bytes isnt enough..
        }
    }
}