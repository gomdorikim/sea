using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game {
	public class QuestPacket {

        public static void HandleStartQuest(Character pCharacter, Packet pPacket)
        {
            //MessagePacket.SendNotice(pPacket.ToString(), pCharacter);
            byte pOperation = pPacket.ReadByte();
            short Quest = pPacket.ReadShort();

            switch (pOperation)
            {
                case 0x00: //Lost Required Item
                    {
                        int count = pPacket.ReadInt();
                        int item = pPacket.ReadInt();

                        pCharacter.Inventory.AddNewItem(item, (short)count);
                        break;
                    }
                case 0x01:
                    {
                        //MessagePacket.SendNotice(Quest.ToString(), pCharacter);
                        int npcID = pPacket.ReadInt();
                        //NPCData npc = DataProvider.NPCs[npcID];

                        pCharacter.Quests.AddNewQuest(Quest);
                        pCharacter.Quests.RealQuests++;
                        if (!DataProvider.Quests.ContainsKey(Quest))
                            return;

                        if (DataProvider.Quests[Quest].ReqItems != null)
                        {
                            foreach (ItemReward item in DataProvider.Quests[Quest].ReqItems)
                            {
                                if (pCharacter.Inventory.HasSlotsFreeForItem(item.Reward, (short)item.ItemRewardCount, Constants.isStackable(item.Reward)))
                                    pCharacter.Inventory.AddNewItem(item.Reward, (short)item.ItemRewardCount);
                                else
                                    MessagePacket.SendNotice("You need to make room in your inventory.", pCharacter);
                            }
                        }
                        if (DataProvider.Quests[Quest].Mobs != null)
                        {
                            foreach (QuestMob mob in DataProvider.Quests[Quest].Mobs)
                            {
                                //MessagePacket.SendNotice("not null!", pCharacter);
                                pCharacter.Quests.AddOrSetQuestMob(Quest, mob.MobID, mob.ReqKills);
                            }
                            //pCharacter.Quests.questmobs(Quest);
                        }
                        break;
                    }
                case 0x02:
                    {
                        if (!DataProvider.Quests.ContainsKey(Quest))
                        {
                            return;
                        }

                        if (DataProvider.Quests[Quest].ItemRewards != null)
                        {
                            foreach (ItemReward item in DataProvider.Quests[Quest].ItemRewards)
                            {
                                //MessagePacket.SendNotice(item.Reward.ToString() + " " + item.ItemRewardCount.ToString(), pCharacter);
                                if (item.ItemRewardCount <= -1)
                                {
                                    pCharacter.Inventory.TakeItem(item.Reward, (short)System.Math.Abs(item.ItemRewardCount));
                                }
                                else
                                {
                                    pCharacter.Inventory.AddNewItem(item.Reward, (short)item.ItemRewardCount);
                                }
                            }
                        }
                        if (DataProvider.Quests[Quest].ExpReward != 0)
                        {
                            pCharacter.AddEXP(DataProvider.Quests[Quest].ExpReward);
                        }
                        if (DataProvider.Quests[Quest].MesoReward != 0)
                        {
                            Console.WriteLine("gave meso reward!!");
                            pCharacter.AddMesos(DataProvider.Quests[Quest].MesoReward);
                        }
                        if (DataProvider.Quests[Quest].RandomRewards != null)
                        {
                            Console.WriteLine("random rewards!!!");
                            Random rd = new Random();
                            foreach (ItemReward item in DataProvider.Quests[Quest].RandomRewards)
                            {
                                int Index = rd.Next(1, DataProvider.Quests[Quest].RandomRewards.Count);
                                //MessagePacket.SendNotice(DataProvider.Quests[Quest].RandomRewards[Index].Reward.ToString(), pCharacter);
                                pCharacter.Inventory.AddNewItem(item.Reward, 1);
                                break;
                            }
                        }
                        CompleteQuest(pCharacter, Quest, 0);
                        pCharacter.Quests.CompleteQuest(Quest);
                        pCharacter.Quests.RealQuests--;
                        break;
                    }
                case 0x03:
                    {
                        break;
                    }
            }
        }

        public static void CompleteQuest(Character chr, short QuestID, long Time)
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(0x01);
            pw.WriteShort(QuestID);
            pw.WriteByte(0x02);
            pw.WriteLong(DateTime.Now.Ticks);
            //pw.WriteHexString("A0 67 B9 DA 69 3A C8 01");
            chr.sendPacket(pw);
        }

		public static void SendQuestDataUpdate(Character chr, int QuestID, string Data) {
			Packet pw = new Packet(0x1D);
			pw.WriteByte(0x01);
            pw.WriteShort((short)QuestID);
			pw.WriteBool(true);
            pw.WriteString("");
			chr.sendPacket(pw);
		}

        public static void SendQuestMobUpdate(Character chr, short QuestID)
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(0x01);
            pw.WriteShort(QuestID);
            pw.WriteBool(true);
            pw.WriteString(chr.Quests.QuestMobKilledData(QuestID));
            pw.WriteInt(0);
            pw.WriteInt(0);
            chr.sendPacket(pw);
        }

		public static void SendQuestRemove(Character chr, int QuestID) {
			Packet pw = new Packet(0x1D); //??
			pw.WriteByte(0x01);
			pw.WriteBool(false);
			pw.WriteInt(QuestID);
			chr.sendPacket(pw);
		}

        public static void UpdateTest(Character chr, short QuestID, int npcId)
        {
            Packet pw = new Packet(0x72);
            pw.WriteByte(8);
            pw.WriteShort(QuestID);
            pw.WriteInt(npcId);
            pw.WriteInt(0);
            chr.sendPacket(pw);
        }

        public static void SendGainItemChat(Character chr, params KeyValuePair<int, int>[] pItems)
        {
            Packet pw = new Packet(0x62);
            pw.WriteByte(0x03);
            pw.WriteByte((byte)pItems.Length);
            foreach (var kvp in pItems)
            {
                pw.WriteInt(kvp.Key);
                pw.WriteInt(kvp.Value);
            }
            chr.sendPacket(pw);
        }
	}
}
