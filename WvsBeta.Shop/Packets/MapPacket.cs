using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Shop {
	public class MapPacket {
		public static void SendJoinCashServer(Character chr) {
			Packet pack = new Packet();
			pack.WriteByte(0x30);


			pack.WriteShort(-1); // Flags (contains everything: 0xFFFF)

			pack.WriteInt(chr.mID);
			pack.WriteString(chr.mName, 13);
			pack.WriteByte(chr.mGender); // Gender
			pack.WriteByte(chr.mSkin); // Skin
			pack.WriteInt(chr.mFace); // Face
			pack.WriteInt(chr.mHair); // Hair

			pack.WriteLong(chr.mPets.GetEquippedPetCashid());

			pack.WriteByte(chr.mPrimaryStats.Level); // Level
			pack.WriteShort(chr.mPrimaryStats.Job); // Jobid
			pack.WriteShort(chr.mPrimaryStats.Str); //charc.str);
			pack.WriteShort(chr.mPrimaryStats.Dex); //charc.dex);
			pack.WriteShort(chr.mPrimaryStats.Int); //charc.intt);
			pack.WriteShort(chr.mPrimaryStats.Luk); //charc.luk);
			pack.WriteShort(chr.mPrimaryStats.HP); //charc.hp);
			pack.WriteShort(chr.mPrimaryStats.GetMaxHP(true)); //charc.mhp);
			pack.WriteShort(chr.mPrimaryStats.MP); //charc.mp);
			pack.WriteShort(chr.mPrimaryStats.GetMaxMP(true)); //charc.mmp);
			pack.WriteShort(chr.mPrimaryStats.AP); //charc.ap);
			pack.WriteShort(chr.mPrimaryStats.SP); //charc.sp);
			pack.WriteInt(chr.mPrimaryStats.EXP); //charc.exp);
			pack.WriteShort(chr.mPrimaryStats.Fame); //charc.fame);

			pack.WriteInt(chr.mMap); // Mapid
			pack.WriteByte(chr.mMapPosition); // Mappos

			//pack.WriteLong(0);
			//pack.WriteInt(13);
			//pack.WriteInt(12);

			pack.WriteByte(20); // Buddylist slots
            pack.WriteInt(chr.mInventory.mMesos); //Mesos

            pack.WriteByte(24); //Slot 1
            pack.WriteByte(24);
            pack.WriteByte(24);
            pack.WriteByte(24);
            pack.WriteByte(52);
            //pack.WriteByte(0);

            chr.mInventory.GenerateInventoryPacket(pack); //So far so good

			chr.mSkills.AddSkills(pack);


            pack.WriteShort((short)chr.mQuests.RealQuests); // Running quests
            //Console.WriteLine("wtf real quests : " + chr.Quests.RealQuests);
            foreach (KeyValuePair<int, QuestData> kvp in chr.mQuests.mQuests)
            {
                if (!kvp.Value.Complete)
                {
                    pack.WriteShort((short)kvp.Key);
                    pack.WriteString(kvp.Value.Data);
                }
            }

            pack.WriteShort((short)chr.mQuests.mCompletedQuests.Count); // Running quests
            foreach (KeyValuePair<int, QuestData> kvp in chr.mQuests.mCompletedQuests)
            {
                pack.WriteShort((short)kvp.Key);
                pack.WriteInt(0);
                pack.WriteInt(0);
            }
            
			pack.WriteShort(0); // RPS Game(s)
			/*
			 * For every game stat:
			 * pack.WriteInt(); // All unknown values
			 * pack.WriteInt();
			 * pack.WriteInt();
			 * pack.WriteInt();
			 * pack.WriteInt();
			*/

			//pack.WriteShort(0); // Rings?

			chr.mInventory.AddRockPacket(pack);

            pack.WriteByte(1);
            pack.WriteByte(1);
           
            pack.WriteLong(0); //8
            pack.WriteLong(0); //16
            pack.WriteLong(0); //24
            pack.WriteLong(0); //32

            pack.WriteLong(0); //40
            pack.WriteLong(0); //48
            pack.WriteLong(0); //56
            pack.WriteLong(0); //64

            pack.WriteLong(0); //72
            pack.WriteLong(0); //80
            pack.WriteLong(0); //88
            pack.WriteLong(0); //96

            pack.WriteLong(0); //104
            pack.WriteLong(0); //112
            pack.WriteLong(0); //120

            pack.WriteByte(0); 
            for (byte i = 1; i <= 8; i++)
            {
                for (byte j = 0; j <= 1; j++)
                {
                    pack.WriteInt(i);
                    pack.WriteInt(j);
                    pack.WriteInt(1000000);

                    pack.WriteInt(i);
                    pack.WriteInt(j);
                    pack.WriteInt(1000000);

                    pack.WriteInt(i);
                    pack.WriteInt(j);
                    pack.WriteInt(1000000);

                    pack.WriteInt(i);
                    pack.WriteInt(j);
                    pack.WriteInt(1000000);

                    pack.WriteInt(i);
                    pack.WriteInt(j);
                    pack.WriteInt(1000000);
                }
            }

            /**
            pack.WriteShort(5); // Stock 
            pack.WriteInt(-1); // 1 = Sold Out, 2 = Not Sold      
            pack.WriteInt(1002186);
            pack.WriteInt(0); // 1 = Sold Out, 2 = Not Sold
            pack.WriteInt(1002186);
            pack.WriteInt(2); // 1 = Sold Out, 2 = Not Sold
            pack.WriteInt(1002186);
            pack.WriteInt(4); // 1 = Sold Out, 2 = Not Sold
            pack.WriteInt(1002186);
            pack.WriteInt(5); // 1 = Sold Out, 2 = Not Sold
            pack.WriteInt(1002186);
           
            **/
			pack.WriteLong(0);
			pack.WriteLong(0);
			pack.WriteLong(0);
			pack.WriteLong(0);
			pack.WriteLong(0);
			pack.WriteLong(0);
			pack.WriteLong(0);
			pack.WriteLong(0);
			pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);

            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0); pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
            pack.WriteLong(0);
			chr.sendPacket(pack);
		}
	}
}
