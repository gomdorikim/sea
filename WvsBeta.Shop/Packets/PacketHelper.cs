using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Shop {
	class PacketHelper {
		public static void AddCashItemData(Character chr, Packet packet, Item item, bool isgift) {
            //works :D
            //todo : pets

            //47 bytes
			packet.WriteLong(0); //Cash ID    //8
            packet.WriteInt(chr.mUserID);  //12
            packet.WriteHexString("01 01 01 01"); //Basically user ID is a long..  //16


            packet.WriteInt(1002186); //Item ID  //20
            packet.WriteHexString("01 01 01 01"); //SN //24
            packet.WriteShort(1);  //19 //Amount //26

			packet.WriteString("", 13); // //39
            packet.WriteLong(150842304000000000L); //47 //Expiration
            
		}

        public static void GiftItemTest(Character chr, Packet packet, Item item)
        {
            packet.WriteLong(0); //8
            packet.WriteInt(1002186); //12
            packet.WriteString("", 13); //25
            packet.WriteString("", 73); //98 
            //missing expiration ? :O
        }

        public static void AddGiftItemTest(Character chr, Packet packet)
        {
            packet.WriteLong(0); //10
            packet.WriteInt(1002186); //14
            packet.WriteString("", 18); // 32
            packet.WriteLong(150842304000000000L); //40
        }
        public static void AddCashItem(Character chr, Packet packet)
        {
            packet.WriteLong(Server.Instance.GetNewCashSerial()); //8
            packet.WriteInt(chr.mUserID); //12
            packet.WriteInt(1000000); //16
            packet.WriteShort(1); //18
            packet.WriteString(chr.mName, 13); //31
            packet.WriteLong(150842304000000000L); // 39
            packet.WriteLong(0); //? 47

        }

        public static void AddGiftList(Packet packet, Item item)
        {
            packet.WriteShort(1); 
            packet.WriteLong(42); // 10
            packet.WriteInt(item.ItemID); //14
            packet.WriteString("wahatatas", 13); //27
            packet.WriteLong(0); //35
            packet.WriteInt(400967355); //39
            packet.WriteByte(2); //40 :D
            
            
           
        }

		public static void AddItemData(Packet packet, Item item, short slot, bool shortslot, bool Lol = false) {
			if (slot != 0) {
				if (shortslot) {
					packet.WriteShort(slot); // 2
				}
				else {
					slot = Math.Abs(slot);
					if (slot > 100) slot -= 100;
					packet.WriteByte((byte)slot); // 1
				}
			}
			bool isEquip = (item.ItemID / 1000000 == 1);
			bool isPet = (item.ItemID / 1000000 == 5);

            int itemType = (item.ItemID / 1000000);
            if (itemType == 5)
            {
                packet.WriteByte(3);
            }
            else if (itemType == 1)
            {
                packet.WriteByte(1);
            }
            else
            {
                packet.WriteByte(2);
            }
			packet.WriteInt(item.ItemID);
			if (item.CashId > 0) {
				packet.WriteBool(true);
				packet.WriteLong(item.CashId);
			}
			else {
				packet.WriteBool(false);
			}
			packet.WriteLong(item.Expiration); 

			if (isEquip) {
				// 33 Bytes
				packet.WriteByte(item.Slots); // 1
				packet.WriteByte(item.Scrolls); // 3
				packet.WriteShort(item.Str); // 5
				packet.WriteShort(item.Dex); // 7
				packet.WriteShort(item.Int); // 9
				packet.WriteShort(item.Luk); // 11
				packet.WriteShort(item.HP); // 13
				packet.WriteShort(item.MP); // 15
				packet.WriteShort(item.Watk); // 17
				packet.WriteShort(item.Matk); // 19
				packet.WriteShort(item.Wdef); // 21
				packet.WriteShort(item.Mdef); // 23
				packet.WriteShort(item.Acc); // 25
				packet.WriteShort(item.Avo); // 27
				packet.WriteShort(item.Hands); // 29
				packet.WriteShort(item.Speed); // 31
				packet.WriteShort(item.Jump); // 33
                packet.WriteString("");
			}
			else if (isPet) {
				packet.WriteString(item.Pet.Name, 13);
				packet.WriteByte(item.Pet.Level);
				packet.WriteShort(item.Pet.Closeness);
				packet.WriteByte(item.Pet.Fullness);
				packet.WriteLong(item.Pet.Expiration);
                //packet.WriteString("");
			}
			else {
				// 2 bytes
				packet.WriteShort(item.Amount); // 2
                packet.WriteString("");
			}

		}

	}
}
