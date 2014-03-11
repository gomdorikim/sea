using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class SkillPacket
    {
        public static void HandleUseSkill(Character chr, Packet packet)
        {
            int SkillID = packet.ReadInt();
            byte SkillLevel = packet.ReadByte();

            if (!chr.Skills.mSkills.ContainsKey(SkillID) || SkillLevel < 1 || SkillLevel > chr.Skills.mSkills[SkillID])
            {
                Console.WriteLine("Player {0} tried to use a skill without having it.", chr.ID);
                ReportManager.FileNewReport("Player {0} tried to use a skill without having it.", chr.ID, 0);
                chr.mPlayer.Socket.Disconnect();
            }

            MapPacket.SendPlayerSkillAnim(chr, SkillID, SkillLevel);
            
            SkillLevelData sld = DataProvider.Skills[SkillID][SkillLevel];
            if (SkillID != (int)Constants.Spearman.Skills.HyperBody)
            {
                chr.Buffs.AddBuff(SkillID);
            }
            switch (SkillID)
            {
   
                case (int)Constants.Cleric.Skills.Heal:
                    {
                        ushort healRate = (ushort)sld.HPProperty;
                        if (healRate > 100) healRate = 100;

                        short healAmount = (short)(healRate * chr.PrimaryStats.GetMaxHP(false) / 100); // Party: / (amount players)

                        chr.ModifyHP(healAmount, true);
                        if (chr.PartyID != -1)
                        {
                            foreach (Character pCharacter in DataProvider.Maps[chr.Map].Characters)
                            {
                                if (pCharacter.PartyID == chr.PartyID)
                                {
                                    pCharacter.ModifyHP(healAmount, true);
                                }
                            }
                        }
                        break;
                    }
                case (int)Constants.Gm.Skills.Hide:
                    {
                        DataProvider.Maps[chr.Map].RemovePlayer(chr, true);
                        AdminPacket.Hide(chr, true);
                        break;
                    }
                case (int)Constants.Priest.Skills.MysticDoor:
                    {
                        Door door = new Door(chr, chr.Map, DataProvider.Maps[chr.Map].ReturnMap, chr.Position.X, chr.Position.Y);
                        chr.Door = door;
                        MapPacket.SpawnDoor(chr, true, chr.Position.X, chr.Position.Y);
                        MapPacket.SpawnPortal(chr, chr.Position);
                        InventoryPacket.NoChange(chr);
                        break;
                    }
                case (int)Constants.Gm.Skills.Haste:
                case (int)Constants.Gm.Skills.HolySymbol:
                case (int)Constants.Gm.Skills.Bless:
                    {
                        byte players = packet.ReadByte();
                        for (byte i = 0; i < players; i++)
                        {
                            int playerid = packet.ReadInt();
                            Character victim = DataProvider.Maps[chr.Map].GetPlayer(playerid);
                            if (victim != null && victim != chr)
                            {
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, true);
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, false);
                                victim.Buffs.AddBuff(SkillID, SkillLevel);
                            }
                        }
                        break;
                    }
                case (int)Constants.Gm.Skills.HealPlusDispell:
                    {
                        byte players = packet.ReadByte();
                        for (byte i = 0; i < players; i++)
                        {
                            int playerid = packet.ReadInt();
                            Character victim = DataProvider.Maps[chr.Map].GetPlayer(playerid);
                            if (victim != null)
                            {
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, true);
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, false);
                                victim.ModifyHP(victim.PrimaryStats.GetMaxMP(false), true);
                                victim.ModifyMP(victim.PrimaryStats.GetMaxMP(false), true);
                            }
                        }
                        chr.ModifyHP(chr.PrimaryStats.GetMaxMP(false), true);
                        chr.ModifyMP(chr.PrimaryStats.GetMaxMP(false), true);
                        break;
                    }
                case (int)Constants.Gm.Skills.Resurrection:
                    {
                        byte players = packet.ReadByte();
                        for (byte i = 0; i < players; i++)
                        {
                            int playerid = packet.ReadInt();
                            Character victim = DataProvider.Maps[chr.Map].GetPlayer(playerid);
                            if (victim != null && victim.PrimaryStats.HP <= 0)
                            {
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, true);
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, false);
                                victim.ModifyHP(victim.PrimaryStats.GetMaxHP(false), true);
                            }
                        }
                        break;
                    }
            }

            if (chr.PartyID != -1)
            {
                foreach (Character pCharacter in DataProvider.Maps[chr.Map].Characters)
                {
                    if (pCharacter.PartyID == chr.PartyID && pCharacter.ID != chr.ID)
                    {
                        if (SkillID != (int)Constants.Spearman.Skills.HyperBody)
                        {
                            pCharacter.Buffs.AddBuff(SkillID, SkillLevel);
                        }
                        InventoryPacket.NoChange(pCharacter);
                        pCharacter.Skills.DoSkillCost(SkillID, SkillLevel, true);
                        MapPacket.SendPlayerSkillAnimThirdParty(pCharacter, SkillID, SkillLevel, true, true);
                        MapPacket.SendPlayerSkillAnimThirdParty(pCharacter, SkillID, SkillLevel, true, false);
                    }
                }
            }

            InventoryPacket.NoChange(chr);
            chr.Skills.DoSkillCost(SkillID, SkillLevel);
            if (Constants.isSummon(SkillID))
            {
                chr.Summons.NewSummon(SkillID, SkillLevel);
            }        
        }

        public static void HandleStopSkill(Character chr, Packet packet)
        {
            //MessagePacket.SendNotice("stop skill!!", chr);
            int skillid = packet.ReadInt();
            chr.PrimaryStats.RemoveByValue(skillid);
            switch (skillid)
            {
                case (int)Constants.Gm.Skills.Hide:
                    {
                        DataProvider.Maps[chr.Map].ShowPlayer(chr);
                        AdminPacket.Hide(chr, false);
                        break;
                    }
            }
        }

        public static void HandleAddSkillLevel(Character chr, Packet packet)
        {
            int SkillID = packet.ReadInt(); // Todo, add check.
            if (!DataProvider.Skills.ContainsKey(SkillID) || (chr.Skills.mSkills.ContainsKey(SkillID) && chr.Skills.mSkills[SkillID] == DataProvider.Skills[SkillID].Count))
            {
                // Hacking of some sort.
                return;
            }
            chr.Skills.AddSkillPoint(SkillID);
            chr.AddSP(-1);
        }

        public static void SendAddSkillPoint(Character chr, int skillid, byte level)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x1A);
            pw.WriteByte(0x01);
            pw.WriteShort(0x01);
            pw.WriteInt(skillid);
            pw.WriteInt(level);
            pw.WriteByte(1);

            chr.sendPacket(pw);
        }

        public static void SendSetSkillPoints(Character chr, Dictionary<int, byte> skills)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x1A);
            pw.WriteByte(0x01);
            pw.WriteShort((short)skills.Count);
            foreach (KeyValuePair<int, byte> skill in skills)
            {
                pw.WriteInt(skill.Key);
                pw.WriteInt(skill.Value);
            }
            pw.WriteByte(1);

            chr.sendPacket(pw);
        }
    }
}