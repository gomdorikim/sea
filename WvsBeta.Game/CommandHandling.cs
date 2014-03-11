using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using WvsBeta.Database;
using WvsBeta.Game.Events;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class CommandHandling
    {
        public static Dictionary<string, int> mMapNameList { get; set; }

        public static int GetMapidFromName(string name)
        {
            if (mMapNameList == null)
            {
                mMapNameList = new Dictionary<string, int>();
				// Job maps
                mMapNameList.Add("gm", 180000000);
                mMapNameList.Add("3rd", 211000001);
                mMapNameList.Add("mage", 101000003);
                mMapNameList.Add("bowman", 100000201);
                mMapNameList.Add("thief", 103000003);
                mMapNameList.Add("warrior", 102000003);
				// Miscellaneous
                mMapNameList.Add("happyville", 209000000);
				mMapNameList.Add("cafe", 193000000);
                // Maple Island
                mMapNameList.Add("southperry", 60000);
                mMapNameList.Add("amherst", 1010000);
                // Victoria
                mMapNameList.Add("henesys", 100000000);
                mMapNameList.Add("perion", 102000000);
                mMapNameList.Add("ellinia", 101000000);
                mMapNameList.Add("sleepy", 105040300);
                mMapNameList.Add("lith", 104000000);
                mMapNameList.Add("florina", 110000000);
                mMapNameList.Add("kerning", 103000000);
                // Ossyria
                mMapNameList.Add("orbis", 200000000);
                mMapNameList.Add("elnath", 211000000);
                // Ludus Lake area
                mMapNameList.Add("ludi", 220000000);
                mMapNameList.Add("omega", 221000000);
				// Training Areas
				mMapNameList.Add("hhg1", 104040000);
				mMapNameList.Add("kerningconstruct", 103010000);
				mMapNameList.Add("westrockymountain1", 102020000);
				mMapNameList.Add("pigbeach", 104010001);
				mMapNameList.Add("fog", 106010102);
				// Free Markets
				mMapNameList.Add("henfm", 100000110);
				mMapNameList.Add("perionfm", 102000100);
				mMapNameList.Add("elnathfm", 211000110);
				mMapNameList.Add("ludifm", 220000200);
                // Dungeon areas
                mMapNameList.Add("dungeon", 105090200);
                mMapNameList.Add("mine", 211041400);
                // Area boss maps
                mMapNameList.Add("jrbalrog", 105090900);
				mMapNameList.Add("mushmom", 100000005);
                // PQ boss maps
                mMapNameList.Add("kingslime", 103000804);
                // Boss maps
                mMapNameList.Add("zakum", 280030000);
            }

            if (mMapNameList.ContainsKey(name)) return mMapNameList[name];
            else return -1;
        }


        static bool shuttingDown = false;
        public static bool HandleChat(Character character, string text)
        {
            string logtext = string.Format("[{0,-8}] {1,-13}: {2}", character.Map, character.Name, text);
            if (!System.IO.Directory.Exists("Chatlogs"))
            {
                System.IO.Directory.CreateDirectory("Chatlogs");
            }
            System.IO.File.AppendAllText("Chatlogs\\Map-" + character.Map + ".txt", logtext + Environment.NewLine);
            System.IO.File.AppendAllText("Chatlogs\\" + character.Name + ".txt", logtext + Environment.NewLine);

            try
            {
                string[] things = text.Split(' ');


                if (things[0] == "!shutdown" && character.Admin)
                {
                    if (!shuttingDown)
                    {
                        int len = 10;
                        if (things.Length == 2)
                        {
                            int.TryParse(things[1], out len);
                            if (len == 0)
                                len = 10;
                        }

                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, string.Format("Shutting down in {0} seconds", len), character, MessagePacket.MessageMode.ToPlayer);

                        MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction("Shutdown Thread", (a) =>
                        {
                            Environment.Exit(9001);

                        }, (ulong)len * 1000, 0));
                        shuttingDown = true;
                        return true;
                    }
                    else
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "Unable to shutdown now!", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    return false;

                }
                else if (things[0] == "!notice" && character.Admin)
                {
                    string data = "";
                    for (int i = 1; i < things.Length; i++)
                    {
                        data += things[i] + " ";
                    }

                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        MessagePacket.SendNotice(data, kvp.Value);
                    }
                    return true;
                }
                else if (things[0] == "!header" && character.Admin)
                {
                    string data = "";
                    for (int i = 1; i < things.Length; i++)
                    {
                        data += things[i] + " ";
                    }

                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        MessagePacket.SendScrollingHeader(data, kvp.Value);
                    }
                    Server.Instance.mScrollingHeader = data;
                    return true;
                }
                else if (things[0] == "!recompile" && character.Admin)
                {
                    Server.Instance.MakeAvailableScripts(character);
                    return true;
                }

                else if (things[0] == "!pet" && character.Admin)
                {
                    if (character.Inventory.GetOpenSlotsInInventory(5) != 0)
                    {
                        try
                        {
                            character.Inventory.CreateNewPet(5000005);
                        }
                        catch (Exception ex)
                        {
                            Program.MainForm.LogAppend(ex.ToString());
                        }
                        //Pet pet = new Pet();
                        //PetsPacket.SendSpawnPet(character, pet, character);
                    }
                    return true;
                }

                else if (things[0] == "!compile" && things.Length == 2 && character.Admin)
                {
                    Server.Instance.MakeAvailableScript(character, things[1]);
                    return true;
                }
                else if (things[0] == "!cleardrops" && character.Admin)
                {
                    DataProvider.Maps[character.Map].ClearDrops();
                    return true;
                }
                else if (things[0] == "!clock" && things.Length > 1 && character.Admin)
                {
                    int time = 0;
                    int.TryParse(things[1], out time);
                    MapPacket.MapTimer(character, time);
                    return true;
                }
                else if (things[0] == "/initializeboats" && character.Admin)
                {
                    Boat.Initialize();
                    MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "Boats initialized!", character, MessagePacket.MessageMode.ToPlayer);
                    return true;
                }

                else if (things[0] == "!worldnotice" && things.Length > 1 && character.Admin)
                {

                    string what = "";
                    for (int i = 1; i < things.Length; i++)
                    {
                        what += things[i] + " ";
                    }

                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.PopupBox, "[Notice] : " + what + "", character, MessagePacket.MessageMode.ToChannel);
                    }
                    return true;
                }

                else if (things[0] == "!gmworldnotice" && things.Length > 1 && character.Admin)
                {

                    string what = "";
                    for (int i = 1; i < things.Length; i++)
                    {
                        what += things[i] + " ";
                    }

                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.PopupBox, "[" + character.Name + "] : " + what + "", character, MessagePacket.MessageMode.ToChannel);
                    }
                    return true;
                }
                else if (things[0] == "!mapnotice" && things.Length > 1 && character.Admin)
                {
                    string what = "";
                    for (int i = 1; i < things.Length; i++)
                    {
                        what += things[i] + " ";
                    }
                    MessagePacket.SendText(MessagePacket.MessageTypes.PopupBox, "[" + character.Name + "] : " + what + "", character, MessagePacket.MessageMode.ToMap);

                }
                else if (things[0] == "!packet" && things.Length > 1  && character.Admin)
                {
                    string packdata = "";
                    for (int i = 1; i < things.Length; i++)
                    {
                        packdata += things[i] + " ";
                    }
                    if (packdata.Length == 0) return false;
                    Packet pw = new Packet();
                    pw.WriteHexString(packdata);
                    Console.WriteLine(packdata);
                    character.sendPacket(pw);
                    return true;
                }
                else if (things[0] == "!drop" && things.Length >= 2 && character.Admin)
                {
                    try
                    {
                        int itemid = -1;
                        int.TryParse(things[1], out itemid);
                        if (itemid == -1)
                        {
                            MessagePacket.SendNotice("Command syntax: !drop [itemid] {amount}", character);
                            return false;
                        }
                        byte inv = (byte)(itemid / 1000000);
                        if (inv <= 0 || inv > 5 || (!DataProvider.Equips.ContainsKey(itemid) && !DataProvider.Items.ContainsKey(itemid) && !DataProvider.Pets.ContainsKey(itemid)))
                        {
                            MessagePacket.SendNotice("Item not found :(", character);
                            return false;
                        }

                        short amount = 1;
                        if (things.Length >= 3)
                        {
                            short.TryParse(things[2], out amount);
                        }

                        Item dropItem = new Item();
                        dropItem.ItemID = itemid;
                        dropItem.Amount = amount;

                        Drop drop = new Drop(character.Map, dropItem, new Pos(character.Position), 0);
                        drop.DoDrop(drop.Position, true);

                        return true;
                    }
                    catch
                    {
                        MessagePacket.SendNotice("Item not found :(", character);
                        return false;
                    }
                }
                else if (things[0] == "!map" || (things[0] == "!goto" && things.Length == 2) && character.Admin)
                {
                    string mapstring = things[1];
                    int mapid = 0;
                    int tempMapid = GetMapidFromName(mapstring);
                    if (tempMapid == -1)
                    {
                        switch (mapstring)
                        {
                            case "here": mapid = character.Map; break;
                            case "town": mapid = DataProvider.Maps[character.Map].ReturnMap; break;
                        }
                    }
                    else
                    {
                        mapid = tempMapid;
                    }
                    if (mapid == 0) int.TryParse(mapstring, out mapid);
                    if (DataProvider.Maps.ContainsKey(mapid))
                    {
                        character.ChangeMap(mapid);
                        //MapPacket.sendchangemaptest(character);
                    }
                    else
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "Map not found.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    return true;
                }
             
                if (things[0] == "!svnupdate"  && character.Admin)
                {
                    System.Diagnostics.Process.Start(@"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe", "/command:update /path:\"C:\\Users\\Administrator\\Dropbox\\Source\\DataSvr\\Scripts\" /closeonend:1");
                    MessagePacket.SendNotice("Updating SVN, should be done in a few seconds.", character);
                    return true;
                }

                else if ((things[0] == "!chase" || things[0] == "!warp") && things.Length == 2 && character.Admin)
                {
                    string other = things[1].ToLower();
                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        if (kvp.Value.Name.ToLower() == other)
                        {
                            character.ChangeMap(kvp.Value.Map);
                            return true;
                        }
                    }

                    MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "Victim not found.", character, MessagePacket.MessageMode.ToPlayer);

                    return true;
                }
                else if ((things[0] == "!chasehere" || things[0] == "!warphere") && things.Length == 2 && character.Admin)
                {
                    string other = things[1].ToLower();
                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        if (kvp.Value.Name.ToLower() == other)
                        {
                            kvp.Value.ChangeMap(character.Map);
                            return true;
                        }
                    }

                    MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "Victim not found.", character, MessagePacket.MessageMode.ToPlayer);

                    return true;
                }


                else if (things[0] == "!online" && character.Admin)
                {
                    string playersonline = "Players online (" + Server.Instance.CharacterList.Count + "): ";
                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        playersonline += kvp.Value.Name + ", ";
                    }
                    playersonline = playersonline.Substring(0, playersonline.Length - 2) + ".";
                    MessagePacket.SendNotice(playersonline, character);
                    return true;
                }

                else if (things[0] == "!setsp" && things.Length == 3 && character.Admin)
                {
                    int skillid = Convert.ToInt32(things[1]);
                    byte level = 1;
                    byte maxLevel = (byte)(DataProvider.Skills.ContainsKey(skillid) ? DataProvider.Skills[skillid].Count : 0);
                    if (maxLevel == 0)
                    {
                        MessagePacket.SendNotice("Skill not found.", character);
                    }
                    else
                    {
                        if (things[2] == "max")
                        {
                            level = maxLevel;
                        }
                        else
                        {
                            level = Convert.ToByte(things[2]);
                        }
                        character.Skills.SetSkillPoint(skillid, level);
                    }
                    return true;
                }
                else if (things[0] == "!maxskills" && character.Admin)
                {
                    Dictionary<int, byte> mMaxedSkills = new Dictionary<int, byte>();
                    foreach (KeyValuePair<int, Dictionary<byte, SkillLevelData>> kvp in DataProvider.Skills)
                    {
                        character.Skills.SetSkillPoint(kvp.Key, (byte)kvp.Value.Count, false);
                        mMaxedSkills.Add(kvp.Key, (byte)kvp.Value.Count);
                    }
                    SkillPacket.SendSetSkillPoints(character, mMaxedSkills); // 1 packet for all skills
                    mMaxedSkills.Clear();
                    mMaxedSkills = null;
                    return true;
                }
                else if ((things[0] == "!summon" || things[0] == "!spawn") && things.Length >= 2 && character.Admin)
                {
                    int mobid = Convert.ToInt32(things[1]);
                    int amount = 1;
                    if (things.Length == 3) amount = Convert.ToInt32(things[2]);

                    if (DataProvider.Mobs.ContainsKey(mobid))
                    {
                        for (int i = 0; i < amount && (character.Admin ? true : i < 10); i++)
                        {
                            DataProvider.Maps[character.Map].spawnMobNoRespawn(mobid, character.Position, character.Foothold);

                        }
                    }
                    else
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "Mob not found.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    return true;
                }

                else if (things[0] == "!toggleportal" && character.Admin)
                {
                    if (DataProvider.Maps[character.Map].PortalOpen == false)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.Notice, "You have toggled the portal on.", character, MessagePacket.MessageMode.ToPlayer);
                        DataProvider.Maps[character.Map].PortalOpen = true;
                    }
                    else
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.Notice, "You have toggled the portal off.", character, MessagePacket.MessageMode.ToPlayer);
                        DataProvider.Maps[character.Map].PortalOpen = false;
                    }
                }

                else if (things[0] == "!job" && things.Length == 2 && character.Admin)
                {
                    short job = 0;
                    short.TryParse(things[1], out job);
                    character.SetJob(job);
                    return true;
                }
                else if (things[0] == "!ptinvite" && things.Length == 2 && character.Admin)
                {
                    string other = things[1].ToLower();
                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        if (kvp.Value.Name.ToLower() == other)
                        {

                            //PartyPacket.partyInvite(kvp.Value);
                            MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "Hey", kvp.Value, MessagePacket.MessageMode.ToPlayer);
                        }
                    }



                    return true;
                }
                else if (things[0] == "!getid" && things.Length == 2 && character.Admin)
                {

                    string name = things[1].ToLower();
                    Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM characters WHERE name = '" + MySqlHelper.EscapeString(name) + "'");
                    MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
                    data.Read();
                    int id = data.GetInt32("ID");
                    MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "ID is " + id + ".", character, MessagePacket.MessageMode.ToPlayer);
                    return true;
                }

                else if (things[0] == "!makedonator" && things.Length == 2 && character.Admin)
                {
                    string name = things[1].ToLower();
                    int derp = Server.Instance.CharacterDatabase.UserIDByName(name);
                    if (derp <= 1)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "You have entered an incorrect name.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    else if (derp > 1)
                    {
                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET donator = 1 WHERE ID = " + derp);
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "'" + name + "' is now set as a donator on the AccountID : " + derp, character, MessagePacket.MessageMode.ToPlayer);
                    }
                    return true;
                }

                else if (things[0] == "!dc" & character.Admin && things.Length == 2)
                {
                    string victim = things[1].ToLower();
                    Character who = Server.Instance.GetCharacter(victim);

                    if (who == null)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "You have entered an incorrect name.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    else
                    {
                        who.mPlayer.Socket.Disconnect();
                    }
                }

                else if (things[0] == "!ban" && character.Admin)
                {
                    if (things.Length < 3)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.Notice, "You have entered an incorrect syntax. Please use this syntax: !ban [charactername] [reasonid]. Reasons are as follows: [1]: Hacking [2]: Botting [3]: Impersonating a GM [4]: Advertising", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    string name = things[1].ToLower();
                    int reason = Convert.ToInt16(things[2]);
                    int charid = Server.Instance.CharacterDatabase.AccountIdByName(name);
                    int ID = Server.Instance.CharacterDatabase.UserIDByName(name);

                    if (ID <= 1)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "You have entered an incorrect name.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    else if (ID > 1)
                    {
                        if (Server.Instance.CharacterList.ContainsKey(charid))
                        {
                            Character victim = Server.Instance.GetCharacter(name);
                            victim.mPlayer.Socket.Disconnect();
                            Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET ban_reason = " + reason + " WHERE ID = " + ID);

                            MessagePacket.SendText(MessagePacket.MessageTypes.Notice, "You have banned '" + name + "' on UserID : " + ID + " with the CharacterID : " + charid + " for the " + reason + ".", character, MessagePacket.MessageMode.ToPlayer);
                        }
                        else
                        {
                            Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET ban_reason = " + reason + " WHERE ID = " + ID);

                            MessagePacket.SendText(MessagePacket.MessageTypes.Notice, "Offline Ban: You have banned '" + name + "' on UserID : " + ID + " with the CharacterID : " + charid + ".", character, MessagePacket.MessageMode.ToPlayer);
                        }


                    }
                }
                else if (things[0] == "!ipban" && things.Length == 3 && character.Admin)
                {

                    string name = things[1].ToLower();
                    int reason = Convert.ToInt16(things[2]);
                    int ID = Server.Instance.CharacterDatabase.UserIDByName(name);

                    if (!Server.Instance.CharacterList.ContainsKey(ID))
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "You have entered an incorrect name.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    else
                    {
                        //if (Server.Instance.CharacterList.ContainsKey(ID))
                        //{
                        Character victim = Server.Instance.GetCharacter(name);
                        string ip = victim.mPlayer.Socket.IP;
                        //if (!Server.Instance.CharacterDatabase.IsBanned(ID))
                        //{


                        // }
                        //}
                    }
                }
                else if (things[0] == "!unban" && things.Length == 2 && character.Admin)
                {
                    string name = things[1].ToLower();
                    int ID = Server.Instance.CharacterDatabase.UserIDByName(name);

                    if (ID <= 1)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "You have entered an incorrect name.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    else if (ID > 1)
                    {
                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET ban_reason = 0 WHERE ID = " + ID);
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "You have unbanned" + name + " on the UserID : " + ID + ".", character, MessagePacket.MessageMode.ToPlayer);
                    }
                }
                else if (things[0] == "!getip" && things.Length == 2 && character.Admin)
                {
                    string name = things[1].ToLower();
                    Character victim = Server.Instance.GetCharacter(name);
                    int charid = victim.ID;
                    if (Server.Instance.CharacterList.ContainsKey(charid))
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.Notice, "The IP of the user " + name + " is " + victim.mPlayer.Socket.IP, character, MessagePacket.MessageMode.ToPlayer);
                    }
                    else
                    {

                        MessagePacket.SendText(MessagePacket.MessageTypes.Notice, "Unable to get IP because the user is offline.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                }

                else if (things[0] == "!pm" && things.Length == 2 && character.Admin)
                {
                    string name = things[1].ToLower();
                    Character victim = Server.Instance.GetCharacter(name);

                    MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "[Message from GM] : Please log out then log in again.", victim, MessagePacket.MessageMode.ToPlayer);

                }

               
                else if (things[0] == "!participate" && things.Length == 2 && character.Admin)
                {
                    string name = things[1].ToLower();
                    /**
                    if (EventManager.hasParticipated(name) == true)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "HAS PARTICIPATED DUN DUN DUN.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                    else if (EventManager.hasParticipated(name) == false)
                    {
                        MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "NOT PARTICIPATED DUN DUN DUNNNN.", character, MessagePacket.MessageMode.ToPlayer);
                    }
                     * */

                }               
              


                else if (things[0] == "!getid2" && things.Length == 2 && character.Admin)
                {

                    string name = things[1].ToLower();
                    int id = Server.Instance.CharacterDatabase.AccountIdByName(name);
                    string name2 = character.Name;
                    MessagePacket.SendText(MessagePacket.MessageTypes.RedText, "ID is " + id + ".", character, MessagePacket.MessageMode.ToPlayer);
                    return true;
                }
          
                
               
                else if (things[0] == "!heal" && character.Admin)
                {
                    int hpHealed = character.PrimaryStats.GetMaxHP(false) - character.PrimaryStats.HP;
                    character.ModifyHP(character.PrimaryStats.GetMaxHP(false));
                    character.ModifyMP(character.PrimaryStats.GetMaxMP(false));
                    // CharacterStatsPacket.SendCharacterDamage(character, 0, -hpHealed, 0, 0, 0, 0, null);
                    return true;
                }
                else if (things[0] == "!mp" && things.Length == 2 && character.Admin)
                {
                    character.SetMP(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!hp" && things.Length == 2 && character.Admin)
                {
                    character.SetHP(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!str" && things.Length == 2 && character.Admin)
                {
                    character.SetStr(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!dex" && things.Length == 2 && character.Admin)
                {
                    character.SetDex(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!int" && things.Length == 2 && character.Admin)
                {
                    character.SetInt(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!luk" && things.Length == 2 && character.Admin)
                {
                    character.SetLuk(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!ap" && things.Length == 2 && character.Admin)
                {
                    character.SetAP(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!sp" && things.Length == 2 && character.Admin)
                {
                    character.SetSP(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!addsp" && things.Length == 2 && character.Admin)
                {
                    character.AddSP(Convert.ToInt16(things[1]));
                    return true;
                }
                else if (things[0] == "!giveexp" && things.Length == 2 && character.Admin)
                {
                    character.AddEXP(Convert.ToUInt32(things[1]));
                    return true;
                }
                else if (things[0] == "!mesos" && things.Length == 2 && character.Admin)
                {
                    character.SetMesos(Convert.ToInt32(things[1]));
                    return true;
                }
                else if (things[0] == "!maxslots" && character.Admin)
                {
                    character.Inventory.SetInventorySlots(1, 100);
                    character.Inventory.SetInventorySlots(2, 100);
                    character.Inventory.SetInventorySlots(3, 100);
                    character.Inventory.SetInventorySlots(4, 100);
                    character.Inventory.SetInventorySlots(5, 100);
                    return true;
                }
                else if (things[0] == "!maxstats" && character.Admin)
                {
                    character.SetHP(30000);
                    character.SetMP(30000);
                    character.SetLuk(30000);
                    character.SetStr(30000);
                    character.SetInt(30000);
                    character.SetDex(30000);
                    character.SetAP(0);
                    character.SetSP(2000);
                    return true;
                }
                /*else if (things[0] == "!givegmhaste" && character.Admin) {
                    character.mBuffs.AddBuff((int)Constants.Gm.Skills.Haste, 1);
                    character.mBuffs.AddBuff((int)Constants.Gm.Skills.Bless, 1);
                    return true;
                }*/
                else if (things[0] == "!killmobs" || (things[0] == "!killall") && character.Admin)
                {
                    int amount = DataProvider.Maps[character.Map].KillAllMobs(character, false);
                    MessagePacket.SendNotice("Amount of mobs killed: " + amount.ToString(), character);
                    return true;
                }
                else if (things[0] == "!killmobsdmg" && character.Admin)
                {
                    int amount = DataProvider.Maps[character.Map].KillAllMobs(character, true);
                    MessagePacket.SendNotice("Amount of mobs killed: " + amount.ToString(), character);
                    return true;
                }
                else if (things[0] == "!level" && things.Length == 2 && character.Admin)
                {
                    byte level = 1;
                    byte.TryParse(things[1], out level);
                    character.SetLevel(level);
                    return true;
                }
                else if (things[0] == "!save" && character.Admin)
                {
                    character.Save();
                    MessagePacket.SendNotice("Saved!", character);
                    return true;
                }
                else if (things[0] == "!saveall" && character.Admin)
                {
                    foreach (KeyValuePair<int, Character> kvp in Server.Instance.CharacterList)
                    {
                        kvp.Value.Save();
                        MessagePacket.SendNotice("Saved at : " + DateTime.Now + ".", character);
                    }
                }
                else if (things[0] == "!petname" && things.Length == 2 && character.Admin)
                {
                    string newname = things[1];
                    if (newname.Length > 13)
                    {
                        MessagePacket.SendNotice("Cannot change the name! It's too long :(", character);
                    }
                    else
                    {
                        character.Pets.ChangePetname(newname);
                        MessagePacket.SendNotice("Changed name lol", character);
                    }
                    return true;
                }


                else if (things[0] == "!search" && things.Length > 2 && character.Admin)
                {
                    string What = things[1].ToLower();
                    if (What != "map" && What != "item" && What != "mob" && What != "npc" && What != "skill" && What != "id" && What != "name")
                    {
                        MessagePacket.SendNotice("Incorrect lookup type. Valid types: map, item, mob, npc, skill, name or id.", character);
                    }
                    else
                    {
                        if (What == "id")
                        {
                            int id = Convert.ToInt32(things[2]);
                            Server.Instance.CharacterDatabase.RunQuery("SELECT COUNT(*) AS amount FROM data_ids WHERE objectid = " + id);

                            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
                            data.Read();
                            int rows = data.GetInt32("amount");
                            if (rows == 0)
                            {
                                MessagePacket.SendNotice("No results!", character);
                            }
                            else
                            {
                                MessagePacket.SendNotice(string.Format("{0} results.", rows), character);

                                Server.Instance.CharacterDatabase.RunQuery("SELECT objectname, objecttype FROM data_ids WHERE objectid = " + id);

                                data = Server.Instance.CharacterDatabase.Reader;
                                if (!data.HasRows)
                                {
                                    return false;
                                }
                                else
                                {
                                    while (data.Read())
                                    {
                                        MessagePacket.SendNotice(string.Format("{0}: {1}", data.GetString("objecttype"), data.GetString("objectname")), character);
                                    }
                                    return true;
                                }
                            }
                        }
                        else if (What == "name")
                        {
                            string search = "";
                            for (int i = 2; i < things.Length; i++) search += " " + things[i];
                            search = search.Trim();

                            Server.Instance.CharacterDatabase.RunQuery("SELECT COUNT(*) AS amount FROM data_ids WHERE objectname LIKE '%" + MySqlHelper.EscapeString(search) + "%'");

                            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
                            data.Read();
                            int rows = data.GetInt32("amount");
                            if (rows == 0)
                            {
                                MessagePacket.SendNotice("No results!", character);
                            }
                            else
                            {
                                MessagePacket.SendNotice(string.Format("{0} results.", rows), character);

                                Server.Instance.CharacterDatabase.RunQuery("SELECT objectid, objecttype, objectname FROM data_ids WHERE objectname LIKE '%" + MySqlHelper.EscapeString(search) + "%'");

                                data = Server.Instance.CharacterDatabase.Reader;
                                if (!data.HasRows)
                                {
                                    return false;
                                }
                                else
                                {
                                    while (data.Read())
                                    {
                                        MessagePacket.SendNotice(string.Format("[{0}] {1}: {2}", data.GetString("objecttype"), data.GetInt32("objectid"), data.GetString("objectname")), character);
                                    }
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            string search = "";
                            for (int i = 2; i < things.Length; i++) search += " " + things[i];
                            search = search.Trim();

                            Server.Instance.CharacterDatabase.RunQuery("SELECT COUNT(*) AS amount FROM data_ids WHERE objecttype = '" + What + "' AND objectname LIKE '%" + MySqlHelper.EscapeString(search) + "%'");

                            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
                            data.Read();
                            int rows = data.GetInt32("amount");
                            if (rows == 0)
                            {
                                MessagePacket.SendNotice("No results!", character);
                            }
                            else
                            {
                                MessagePacket.SendNotice(string.Format("{0} results.", rows), character);

                                Server.Instance.CharacterDatabase.RunQuery("SELECT objectid, objecttype, objectname FROM data_ids WHERE objecttype = '" + What + "' AND objectname LIKE '%" + MySqlHelper.EscapeString(search) + "%'");

                                data = Server.Instance.CharacterDatabase.Reader;
                                if (!data.HasRows)
                                {
                                    return false;
                                }
                                else
                                {
                                    while (data.Read())
                                    {
                                        MessagePacket.SendNotice(string.Format("[{0}] {1}: {2}", data.GetString("objecttype"), data.GetInt32("objectid"), data.GetString("objectname")), character);
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                    return true;
                }
                else if (things[0] == "!pickdrops" && character.Admin)
                {
                    bool pet = false;
                    if (things.Length == 2 && things[1] == "pet") pet = true;
                    Dictionary<int, Drop> dropBackup = new Dictionary<int, Drop>(DataProvider.Maps[character.Map].Drops);
                    foreach (KeyValuePair<int, Drop> kvp in dropBackup)
                    {
                        if (kvp.Value == null) continue;
                        Drop drop = kvp.Value;
                        short pickupAmount = drop.GetAmount();
                        if (drop.IsMesos())
                        {
                            character.AddMesos(drop.Mesos);
                        }
                        else
                        {
                            if (character.Inventory.AddItem2(drop.ItemData) == drop.ItemData.Amount)
                            {
                                DropPacket.CannotLoot(character, -1);
                                InventoryPacket.NoChange(character); // ._. stupid nexon
                                continue;
                            }
                        }
                        CharacterStatsPacket.SendGainDrop(character, drop.IsMesos(), drop.GetObjectID(), pickupAmount);
                        drop.TakeDrop(character, pet);
                    }
                }
                else if (things[0] == "!zakum" && character.Admin)
                {
                    DataProvider.Maps[character.Map].SpawnZakum(character.Position, character.Foothold);
                }
                else if (things[0] == "!itemtest" && character.Admin)
                {
                    InventoryPacket.AddItemTest(character);
                }
                else if (things[0] == "!item" && things.Length >= 2 && character.Admin)
                {
                    try
                    {
                        int itemid = -1;
                        int.TryParse(things[1], out itemid);
                        if (itemid == -1)
                        {
                            MessagePacket.SendNotice("Command syntax: !item [itemid] {amount}", character);
                            return false;
                        }
                        byte inv = (byte)(itemid / 1000000);
                        if (inv <= 0 || inv > 5 || (!DataProvider.Equips.ContainsKey(itemid) && !DataProvider.Items.ContainsKey(itemid) && !DataProvider.Pets.ContainsKey(itemid)))
                        {
                            MessagePacket.SendNotice("Item not found :(", character);
                            return false;
                        }

                        short amount = 1;
                        int freeslots = character.Inventory.ItemAmountAvailable(itemid);
                        if (things.Length >= 3)
                        {
                            if (things[2] == "max" || things[2] == "fill" || things[2] == "full")
                            {
                                amount = (short)(freeslots > short.MaxValue ? short.MaxValue : freeslots);
                            }
                            else
                            {
                                short.TryParse(things[2], out amount);
                                if (amount > freeslots) amount = (short)(freeslots > short.MaxValue ? short.MaxValue : freeslots);
                            }
                        }

                        if (amount == 0)
                        {
                            DropPacket.CannotLoot(character, -1);
                            InventoryPacket.NoChange(character);
                        }
                        else
                        {
                            character.Inventory.AddNewItem(itemid, amount);
                            CharacterStatsPacket.SendGainDrop(character, false, itemid, amount);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessagePacket.SendNotice("Command syntax: !item [itemid] {amount}", character);
                        if (character.Admin)
                        {
                            MessagePacket.SendNotice(string.Format("LOLEXCEPTION: {0}", ex.ToString()), character);

                        }
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessagePacket.SendNotice("Something went wrong while processing this command.", character);
                if (character.Admin)
                {
                    MessagePacket.SendNotice(ex.ToString(), character);
                }
                return false;
            }

        }




        public static void HandleAdminCommand(Character chr, Packet packet)
        {
            //  41 12 1E 00 00 00 
            byte opcode = packet.ReadByte();
            switch (opcode)
            {
                case 0x00: // /create (no idea what it does)
                    break;
                case 0x02:
                    {
                        // /exp (int amount) 
                        int exp = packet.ReadInt();
                        chr.AddEXP((double)exp);
                        break;
                    }
                case 0x03:
                    {
                        // /ban (user) (permanantly)
                        string name = packet.ReadString();
                        int charid = Server.Instance.CharacterDatabase.AccountIdByName(name);
                        int ID = Server.Instance.CharacterDatabase.UserIDByName(name);
                        using (MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM characters WHERE name = '" + MySqlHelper.EscapeString(name) + "'") as MySqlDataReader)
                        {
                            if (data.HasRows)
                            {
                                if (!Server.Instance.CharacterList.ContainsKey(charid))
                                {
                                    if (Server.Instance.CharacterList.ContainsKey(charid))
                                    {
                                        Character victim = Server.Instance.GetCharacter(name);
                                        victim.mPlayer.Socket.Disconnect();
                                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET ban_reason = 8 WHERE ID = " + ID); //8 : permanent ban
                                        AdminPacket.BanCharacterMessage(chr);
                                    }
                                    else
                                    {
                                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET ban_reason = 0 WHERE ID = " + ID);
                                        AdminPacket.BanCharacterMessage(chr);
                                    }
                                }
                            }
                            else
                            {
                                AdminPacket.InvalidNameMessage(chr);
                            }
                        }
                        break;
                    }
                case 0x04:
                    {
                        string name = packet.ReadString();
                        byte type = packet.ReadByte();
                        int duration = packet.ReadInt();
                        string comment = packet.ReadString();

                        int charid = Server.Instance.CharacterDatabase.AccountIdByName(name);
                        int ID = Server.Instance.CharacterDatabase.UserIDByName(name);
                        using (MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM characters WHERE name = '" + MySqlHelper.EscapeString(name) + "'") as MySqlDataReader)
                        {
                            if (data.HasRows)
                            {
                                if (!Server.Instance.CharacterList.ContainsKey(charid))
                                {
                                    if (Server.Instance.CharacterList.ContainsKey(charid))
                                    {
                                        Character victim = Server.Instance.GetCharacter(name);
                                        victim.mPlayer.Socket.Disconnect();
                                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET ban_reason = " + type + " WHERE ID = " + ID); //8 : permanent ban
                                        AdminPacket.BanCharacterMessage(chr);
                                    }
                                    else
                                    {
                                        Server.Instance.CharacterDatabase.RunQuery("UPDATE users SET ban_reason = " + type + " WHERE ID = " + ID);
                                        AdminPacket.BanCharacterMessage(chr);
                                    }
                                }
                            }
                            else
                            {
                                AdminPacket.InvalidNameMessage(chr);
                            }
                        }
                        break;

                    }
                    
                case 0x11: //not sure what this is supposed to do. The only thing that comes after the received string is an INT(0). the format is /send (something) (something)
                    {
                        // /send (user)
                        string To = packet.ReadString();
                        break;
                    }
                case 0x12:
                    {
                        // /snow
                        TimeSpan time = new TimeSpan(0, packet.ReadInt(), 0);
                        DataProvider.Maps[chr.Map].MakeWeatherEffect(2090000, "", time, true);
                        FileWriter.WriteLine(@"Logs\Admin Command Log.txt", string.Format("[{0}] Character {1} ({2}, UID: {3}) used admin command: /snow {4}", DateTime.Now.ToString(), chr.ID, chr.Name, chr.UserID, time.TotalMinutes));
                        break;
                    }
                case 0x0F:
                    {
                        // /hide 0/1
                        bool doHide = packet.ReadBool();
                        if (doHide == chr.GMHideEnabled) return;

                        if (doHide)
                        {
                            AdminPacket.Hide(chr, true);
                            DataProvider.Maps[chr.Map].RemovePlayer(chr, true);
                        }
                        else
                        {
                            AdminPacket.Hide(chr, false);
                            DataProvider.Maps[chr.Map].ShowPlayer(chr);
                        }
                        FileWriter.WriteLine(@"Logs\Admin Command Log.txt", string.Format("[{0}] Character {1} ({2}, UID: {3}) used admin command: /hide {4}", DateTime.Now.ToString(), chr.ID, chr.Name, chr.UserID, doHide));
                        break;
                    }
                case 0x0A:
                    {
                        // /block NAME TIME REASON
                        string name = packet.ReadString();
                        byte reason = packet.ReadByte();
                        int len = packet.ReadInt();
                        string reasonmsg = packet.ReadString();
                        break;
                    }
                default:
                    {
                        FileWriter.WriteLine(@"Logs\Admin Command Log.txt", string.Format("[{0}] Character {1} ({2}, UID: {3}) tried using an admin command. Packet: {4}", DateTime.Now.ToString(), chr.ID, chr.Name, chr.UserID, packet.ToString()));
                        break;
                    }
            }
        }

        public static void HandleAdminCommandLog(Character chr, Packet packet)
        {
            // 42 04 00 2F 70 6F 73 
            string line = packet.ReadString();
            FileWriter.WriteLine(@"Logs\Admin Command Log.txt", string.Format("[{0}] Character {1} ({2}, UID: {3}) used admin command: {4}", DateTime.Now.ToString(), chr.ID, chr.Name, chr.UserID, line));
            switch (line)
            {
                case "/block":
                    string bWho = packet.ReadString();
                    int len = packet.ReadInt();

                    break;
            }
        }
    }
}
