using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using WvsBeta.Game.Events;
using WvsBeta.Game.Events.EventObjects;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class MapPacket
    {
        public static void HandleMove(Character chr, Packet packet)
        {
            if (packet.ReadByte() != chr.PortalCount) return;

            bool allowed = PacketHelper.ParseMovementData(chr, packet);
            if (!allowed && !chr.Admin)
            {
                //this.Session.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                //return;
                //Program.MainForm.AppendToLogFormat("Move incorrect: {0}", Player.Character.mName);
            }
            packet.Reset(2); // lol
            SendPlayerMove(chr, packet);
        }

        public static void HandleBoatStatusRequest(Character chr, Packet packet) //TODO = Ludi boats
        {
            int mapid = packet.ReadInt();
            byte balrog = packet.ReadByte();

            Ship pShip = Ship.GetDockedShip(mapid);
            Ship rShip = Ship.GetRegisteredShip(mapid);
            if (pShip != null)
            {
                if (pShip.Docked)
                    SendBoat(chr, 2);
            }
            else
            {
                SendBoat(chr, 3);
            }

            if (rShip != null)
            {
                if (balrog == 1 && rShip.HasSpawnBalrog)
                {
                    SendBoat(chr, 3);
                }
            }
        }

        public static void HandleNPCChat(Character chr, Packet packet)
        {
            int npcId = packet.ReadInt();
            Map map = DataProvider.Maps[chr.Map];
            if (map.GetNPC(npcId) != null && chr.ShopNPCID == 0)
            {
                int RealID = map.GetNPC(npcId).ID;
                if (!DataProvider.NPCs.ContainsKey(RealID)) return;
                NPCData npc = DataProvider.NPCs[RealID];
                if (npc.Shop.Count > 0)
                {
                    // It's a shop!
                    chr.ShopNPCID = RealID;
                    NpcPacket.SendShowNPCShop(chr, RealID);
                }
                else if (npc.Trunk > 0)
                {
                    chr.TrunkNPCID = RealID;
                    StoragePacket.SendShowStorage(chr, chr.TrunkNPCID);
                }
                    
                else if (npc.ID != 0 || npc.Quest != "")
                {
                    if (Server.Instance.mAvailableNPCScripts != null)
                    {
                        if (!Server.Instance.mAvailableNPCScripts.ContainsKey(npc.ID.ToString()))
                        {
                            if (chr.Admin)
                            {
                                MessagePacket.SendNotice("NPC script doesn't exist: " + npc.ID + "/" + (npc.Quest == "" ? "(null)" : npc.Quest) + "/" + npc.Trunk, chr);
                            }
                            else
                            {
                               // MessagePacket.SendNotice("This is an NPC in the works.", chr);
                            }
                        }
                        else
                        {
                            if (chr.Admin)
                            {
                                MessagePacket.SendNotice("Started NPC Chat Session with " + npc.ID.ToString() + "!", chr);
                            }
                            NpcChatSession session = new NpcChatSession(RealID, chr);
                            session.mCharacter = chr;
                            session.SetScript(Server.Instance.mAvailableNPCScripts[npc.ID != 0 ? npc.ID.ToString() : npc.Quest]);
                            session.HandleThing();
                        }
                    }
                    else
                    {
                        NpcPacket.SendNPCChatTextNote(chr, npc.ID, "Available Scripts shouldn't be null at this time!!!", false, false);
                    }
                }
            }
        }

        public static void OnEnterPortal(Packet packet, Character chr)
        {
            Map map = DataProvider.Maps[chr.Map];
            packet.Skip(1);
            int opcode = packet.ReadInt();
            switch (opcode)
            {
                case 0:
                    {
                        if (chr.PrimaryStats.HP == 0)
                        {
                            chr.HandleDeath();
                        }
                        break;
                    }
                case -1:
                    {
                        string portalname = packet.ReadString();
                        

                        
                        if (map.Portals.ContainsKey(portalname))
                        {
                            Portal portal = map.Portals[portalname];
                            Portal to = DataProvider.Maps[portal.ToMapID].Portals[portal.ToName];
                            //FileWriter.WriteLine("Logs\\MapShit.txt", portalname);
                            
                            if (chr.Map == 108010300 || chr.Map == 108010200 || chr.Map == 108010100 || chr.Map == 108010400) //Third job timer thing ugh
                            {
                                chr.ChangeMap(portal.ToMapID, to.ID);
                                
                                long ticks = ThirdJob.stopwatch.ElapsedMilliseconds / 1000;
                                int realticks = unchecked((int)ticks);
                                int timeleft = 1799 - realticks;
                                MapPacket.MapTimer(chr, timeleft);
                                ThirdJob.stopwatch.Stop();
                            }
                            else
                            {
                                if (DataProvider.Maps[chr.Map].PortalOpen == false)
                                {
                                    BlockedMessage(chr, 1); //the portal is closed for now
                                }
                                else
                                {
                                    if (DataProvider.Maps[chr.Map].PQPortalOpen == true)
                                      chr.ChangeMap(portal.ToMapID, to);

                                        /**
                                        if (chr.mParty != null)
                                        {
                                            chr.mParty.ReceivePartyMemberHP(chr);
                                            chr.mParty.UpdatePartyMemberHP(chr);
                                        }
                                         * **/
                                        if (chr.UsingTimer == true)
                                        {
                                            MapPacket.MapTimer(chr, KerningCity.CurrentTick);
                                        }
                                    }
                                }
                            }
                        

                        break;
                    }
                default:
                    {
                        if (chr.Admin)
                        {
                            chr.ChangeMap(opcode);
                        }
                        break;
                    }
            }
        }

        public static void OnMapTeleport(Character chr, Packet packet)
        {
            switch (chr.Map)
            {
                case 102000000:
                chr.ChangeMap(102000100);
                break;

                case 100000100:
                chr.ChangeMap(100000110);
                break;

                case 211000100:
                chr.ChangeMap(211000110);
                break;

                default:
                {
                    Console.WriteLine("no map to be warped to!");
                    break;
                }
            }
        }

        public static void HandleSitChair(Character chr, Packet packet)
        {
            short chair = packet.ReadShort();
            if (chair == -1)
            {
                if (chr.MapChair != -1)
                {
                    Map map = DataProvider.Maps[chr.Map];
                    map.UsedSeats.Remove(chr.MapChair);
                    chr.MapChair = -1;
                    SendCharacterSit(chr, -1);
                }
                else
                {
                    InventoryPacket.NoChange(chr);
                }
            }
            else
            {
                Map map = DataProvider.Maps[chr.Map];
                if (map != null && map.Seats.ContainsKey(chair) && !map.UsedSeats.Contains(chair))
                {
                    map.UsedSeats.Add(chair);
                    chr.MapChair = chair;
                    SendCharacterSit(chr, chair);
                }
                else
                {
                    InventoryPacket.NoChange(chr);
                }
            }
        }

        public static void OnMysticDoor(Character chr, Packet packet)
        {  
            int DoorID = packet.ReadInt();
            
                foreach(KeyValuePair<int, Portal> pt in DataProvider.Maps[chr.Door.ToMap].SpawnPoints)
                {
                   if (pt.Value != null)
                    {
                        if (pt.Value.Name == "sp")  //Town Portal : Mystic Door
                        {
                            chr.ChangeMap(chr.Door.ToMap, pt.Value);
                            Pos position = DataProvider.Maps[chr.Map].FindFloor(new Pos(pt.Value.X, pt.Value.Y));
                            chr.Door.ToMap = chr.Door.OriginalMap;
                            chr.Door.OriginalMap = chr.Map;
                            MapPacket.SpawnDoor(chr, true, position.X, position.Y);
                            MapPacket.SpawnPortal2(chr, position);
                            InventoryPacket.NoChange(chr); //-.-       
                            
                            
                        }
                    }
                
            }


        }

        public static void ShowNPC(Life NPC, Character victim)
        {
            Packet pw = new Packet(0x86);
            pw.WriteUInt(NPC.SpawnID);
            pw.WriteInt(NPC.ID);
            pw.WriteShort(NPC.X);
            pw.WriteShort(NPC.Y);
            pw.WriteBool(!NPC.FacesLeft);
            pw.WriteUShort(NPC.Foothold);
            pw.WriteShort(NPC.Rx0);
            pw.WriteShort(NPC.Rx1);

            victim.sendPacket(pw);

            pw = new Packet(0x7D);
            pw.WriteByte(0x01);
            pw.WriteUInt(NPC.SpawnID);
            pw.WriteInt(NPC.ID);
            pw.WriteShort(NPC.X);
            pw.WriteShort(NPC.Y);
            pw.WriteBool(!NPC.FacesLeft);
            pw.WriteUShort(NPC.Foothold);
            pw.WriteShort(NPC.Rx0);
            pw.WriteShort(NPC.Rx1);

            victim.sendPacket(pw);
        }

        public static void ShowReactor(Life Reactor, Character victim)
        {
            Packet pw = new Packet(0x97);
            pw.WriteUInt(Reactor.SpawnID);
            pw.WriteInt(Reactor.ID);
            pw.WriteShort(Reactor.X);
            pw.WriteShort(Reactor.Y);
            pw.WriteUShort(Reactor.Foothold);
            pw.WriteShort(Reactor.Rx0);
            pw.WriteShort(Reactor.Rx1);

            victim.sendPacket(pw);

        }



        public static void HandleNPCAnimation(Character controller, Packet packet)
        {
            Packet pw = new Packet(0x8A);
            byte[] data = packet.ReadLeftoverBytes();
            if (data.Length == 6)
            {
                pw.WriteInt(BitConverter.ToInt32(data, 0));
                pw.WriteShort(BitConverter.ToInt16(data, 4));
            }
            else
            {
                pw.WriteBytes(data);
            }

            controller.sendPacket(pw);
        }

        public static void SendWeatherEffect(int mapid, Character victim = null)
        {
            Map map = DataProvider.Maps[mapid];
            if (map == null) return;
            Packet pw = new Packet(0x31);
            pw.WriteBool(map.mWeatherIsAdmin);
            pw.WriteInt(map.mWeatherID);
            if (!map.mWeatherIsAdmin)
                pw.WriteString(map.mWeatherMessage);

            if (victim != null)
                victim.sendPacket(pw);
            else
                map.SendPacket(pw);
        }

        public static void SendPlayerMove(Character chr, Packet packet)
        {
            Packet pw = new Packet(0x5A);
            pw.WriteInt(chr.ID);
            pw.WriteBytes(packet.ReadLeftoverBytes());

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendChatMessage(Character who, string message)
        {
            Packet pw = new Packet(0x47);
            pw.WriteInt(who.ID);
            pw.WriteBool(who.Admin);
            pw.WriteString(message);

            DataProvider.Maps[who.Map].SendPacket(pw);
        }

        public static void SendEmotion(Character chr, int emotion)
        {
            Packet pw = new Packet(0x61);
            pw.WriteInt(chr.ID);
            pw.WriteInt(emotion);

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendCharacterLeavePacket(Character who)
        {
            Packet pw = new Packet(0x45);
            pw.WriteInt(who.ID);
            DataProvider.Maps[who.Map].SendPacket(pw, who, false);
        }

        public static void SendCharacterLeavePacket(int id, Character victim)
        {
            Packet pw = new Packet(0x45);
            pw.WriteInt(id);
            victim.sendPacket(pw);
        }

        public static void SendCharacterSit(Character chr, short chairid)
        {
            Packet pw = new Packet(0x61);
            pw.WriteBool(chairid != -1);
            if (chairid != -1)
            {
                pw.WriteShort(chairid);
            }
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void SendBossHPBar(int pMapid, int pHP, int pMaxHP, uint pColorBottom, uint pColorTop)
        {
            Packet pw = new Packet(0x39);
            pw.WriteByte(5);
            pw.WriteInt(pHP);
            pw.WriteInt(pMaxHP);
            pw.WriteUInt(pColorTop);
            pw.WriteUInt(pColorBottom);
            DataProvider.Maps[pMapid].SendPacket(pw);
        }
        
        public static void MapEffect(Character chr, byte type, string message, bool ToTeam)
        {
            //Sounds : Party1/Clear // Party1/Failed
            //Messages : quest/party/clear // quest/party/wrong_kor
            Packet pw = new Packet(0x39);
            pw.WriteByte(type); //4: sound 3: message
            pw.WriteString(message);
            if (!ToTeam)
            {
                DataProvider.Maps[chr.Map].SendPacket(pw);
            }
            else
            {
                chr.sendPacket(pw);
            }
        }

        public static void PortalEffect(int MapID, byte what, string message)
        {
            
            Packet pw = new Packet(0x39);
            pw.WriteByte(2); //2
            pw.WriteByte(what); //?? Unknown 
            pw.WriteString(message); //gate
            DataProvider.Maps[MapID].SendPacket(pw);
        }

        public static void Kite(Character chr, int id, int oid, string message, short X, short Y)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x93);
            pw.WriteInt(id);
            pw.WriteInt(oid);
            pw.WriteString(message);
            pw.WriteString(Server.Instance.CharacterDatabase.getCharacterNameByID(id));
            pw.WriteShort(X);
            pw.WriteShort(Y); //Should be close enough :P
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void RemoveKite(int mapid, int id)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x94);
            pw.WriteByte(0); //?
            pw.WriteInt(id);
            DataProvider.Maps[mapid].SendPacket(pw);
        }

        public static void KiteMessage(Character chr)
        {
            //Can't fly it here
            Packet pw = new Packet();
            pw.WriteByte(0x92);
            pw.WriteByte(0);
            chr.sendPacket(pw);
        }

        public static void UpdatePartyMemberHP(Character pCharacter)
        {
           for (int i = 0; i < DataProvider.Maps[pCharacter.Map].Characters.Count; i++)
           {
                if (DataProvider.Maps[pCharacter.Map].Characters[i].PartyID == pCharacter.PartyID)
                {
                   DataProvider.Maps[pCharacter.Map].Characters[i].sendPacket(PartyPacket.ReceivePartyMemberHP(pCharacter.PrimaryStats.HP, pCharacter.PrimaryStats.MaxHP, pCharacter.ID));
                }
            }
        }

        public static void ReceivePartyMemberHP(Character pCharacter)
        {
           for (int i = 0; i < DataProvider.Maps[pCharacter.Map].Characters.Count; i++)
           {
               if (DataProvider.Maps[pCharacter.Map].Characters[i].PartyID == pCharacter.PartyID)
               {
                   pCharacter.sendPacket(PartyPacket.ReceivePartyMemberHP(DataProvider.Maps[pCharacter.Map].Characters[i].PrimaryStats.HP, DataProvider.Maps[pCharacter.Map].Characters[i].PrimaryStats.MaxHP, DataProvider.Maps[pCharacter.Map].Characters[i].ID));
               }
            }
        }

        public static void MapTimer(Character chr, int time)
        {
            Packet pw = new Packet(0x3F);
            pw.WriteByte(0x02);
            pw.WriteInt(time);
            DataProvider.Maps[chr.Map].SendPacket(pw);

        }

        public static void SendBoat(Character chr, byte type) //rice's version of my packet lol
        {
            Packet pack = new Packet(0x41);
            pack.WriteByte(type);
            pack.WriteByte(1);
            DataProvider.Maps[chr.Map].SendPacket(pack);

        }

        public static void sendBoat(Character chr)
        {

            Packet pw = new Packet(0x38);
            pw.WriteInt(1);
            DataProvider.Maps[chr.Map].SendPacket(pw);

        }


        public static void leaveBoat(Character chr)
        {
            Packet pw = new Packet(0x38);
            pw.WriteShort(6);
            DataProvider.Maps[chr.Map].SendPacket(pw);

        }

        public static void balrogBoat(Character chr)
        {
            Packet pw = new Packet(0x38);
            pw.WriteByte(4);
            pw.WriteByte(1);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void SendMapClock(Character chr, int hour, int minute, int second)
        {
            Packet pw = new Packet(0x3F);
            pw.WriteByte(0x01);
            DateTime now = DateTime.Now;
            pw.WriteByte((byte)hour);
            pw.WriteByte((byte)minute);
            pw.WriteByte((byte)second);
            chr.sendPacket(pw);
        }

        public static void SendJukebox(int mapID)
        {
            Map map = DataProvider.Maps[mapID];
            Packet pw = new Packet(0x3B);
            pw.WriteInt(map.mJukeboxID);
            if (map.mJukeboxID != 0)
                pw.WriteString(map.mJukeboxUser);

            map.SendPacket(pw);
        }

        

        public static void BlockedMessage(Character chr, byte msg)
        {
            //1 : The portal is closed for now, 2 : You cannot go to that place.
            Packet pw = new Packet(0x2A);
            pw.WriteByte(msg);
            chr.sendPacket(pw);
        }
        
        public static void DoorPortal(Character chr, int townid, int from)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x91);
            pw.WriteByte(0);
            pw.WriteInt(townid);
            pw.WriteInt(from);
            pw.WriteShort(chr.Position.X);
            pw.WriteShort(chr.Position.Y);
            chr.sendPacket(pw);
        }

        public static void SpawnPortal(Character chr, Pos pos)
        {
            //spawns a portal (Spawnpoint in the map you are going to spawn in)
            Packet pw = new Packet(0x22);

            pw.WriteInt(103000000);
            pw.WriteInt(103000001);
            pw.WriteShort(pos.X);
            pw.WriteShort(pos.Y);
            chr.sendPacket(pw);
        }

        public static void SpawnPortal2(Character chr, Pos pos)
        {
            //spawns a portal (Spawnpoint in the map you are going to spawn in)
            Packet pw = new Packet(0x23);
            pw.WriteInt(103000001);
            pw.WriteInt(103000000);
            
            pw.WriteShort(pos.X);
            pw.WriteShort(pos.Y);
            chr.sendPacket(pw);
        }


        public static void SpawnDoor(Character chr, bool town, short X, short Y)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x91);
            pw.WriteBool(town);
            pw.WriteInt(chr.ID);
            pw.WriteShort(X);
            pw.WriteShort(Y);
            DataProvider.Maps[chr.Map].SendPacket(pw);

        }

        public static void SpawnDoor2(Character chr, bool town, short X, short Y)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x90);
            pw.WriteBool(town);
            pw.WriteInt(chr.ID);
            pw.WriteShort(X);
            pw.WriteShort(Y);
            DataProvider.Maps[chr.Map].SendPacket(pw);

        }

        public static void RemoveDoor(Character chr, int id)
        {
            Packet pw = new Packet(0x91);
            pw.WriteByte(1); //Remove?
            pw.WriteInt(id);
            chr.sendPacket(pw);
        }

        public static void SendPinkText(Character chr, string text) //needs work 
        {
            //!packet 2D 01 04 00 6A 6F 65 70 01003400000000000000
            Packet pw = new Packet(0x2D);
            pw.WriteByte(1);
            pw.WriteString(chr.Name);
            pw.WriteString(text);
            chr.sendPacket(pw);
        }

        public static void SendJukebox(Character chr, int itemid, string who)
        {
            Packet pw = new Packet(0x32);
            pw.WriteInt(itemid);
            if (itemid != 0)
                pw.WriteString(who);
            chr.sendPacket(pw);
        }

        public static void OnQuiz(Character chr, bool isQuestion)
        {
            Packet pw = new Packet(0x3D);
            pw.WriteBool(isQuestion);
            pw.WriteByte(1);
            pw.WriteShort(2);
            chr.sendPacket(pw);
        }
        public static void OnFieldDesc(Character chr)
        {
            Packet pw = new Packet(0x3E);
            pw.WriteByte(0);
            chr.sendPacket(pw);
        }

        public static void SendCharacterEnterPacket(Character player, Character victim)
        {
            Packet pw = new Packet(0x44);

            pw.WriteInt(player.ID);

            pw.WriteString(player.Name);
            pw.WriteString(""); 
            BuffPacket.AddMapBuffValues(player, pw);

            PacketHelper.AddAvatar(pw, player);
            pw.WriteInt(0); //item effect
            pw.WriteInt(0); //chair
            pw.WriteShort(player.Position.X); // X
            pw.WriteShort(player.Position.Y); // Y
            pw.WriteByte(player.Stance); // Stance
            pw.WriteShort(player.Foothold); // Foothold
            pw.WriteBool(false); // Pet
            if (player.GetPetID() != 0)
            {
                /**
                Pet pet = player.Pets.GetEquippedPet();
                pw.WriteInt(player.GetPetID());
                pw.WriteString(pet.Name);
                pw.WriteLong(pet.Item.CashId);
                pw.WriteShort(pet.Position.X);
                pw.WriteShort(pet.Position.Y);
                pw.WriteByte(pet.Stance);
                pw.WriteShort(pet.Foothold);
                 * **/
            }
            pw.WriteByte(0); //end of pet ? 
            
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
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
           

            victim.sendPacket(pw);
        }

        public static void SendPlayerInfo(Character chr, Packet packet)
        {
            int id = packet.ReadInt();
            Character victim = DataProvider.Maps[chr.Map].GetPlayer(id);
            if (victim == null) return;
            Packet pw = new Packet(0x26); 
            pw.WriteInt(victim.ID);
            pw.WriteByte(victim.PrimaryStats.Level);
            pw.WriteShort(victim.PrimaryStats.Job);
            pw.WriteShort(victim.PrimaryStats.Fame);
            pw.WriteString((victim.Admin ? "Administrator" : ""));

            pw.WriteBool(victim.GetPetID() != 0); // Pet
            if (victim.GetPetID() != 0)
            {
                Pet pet = victim.Pets.GetEquippedPet();
                pw.WriteInt(victim.GetPetID());
                pw.WriteString(pet.Name);
                pw.WriteByte(pet.Level);
                pw.WriteShort(pet.Closeness);
                pw.WriteByte(pet.Fullness);
                pw.WriteInt(victim.Inventory.GetEquippedItemID((short)Constants.EquipSlots.Slots.PetEquip1, true)); // Pet equip.
            }

            pw.WriteByte((byte)victim.Wishlist.Count);
            foreach (int serial in victim.Wishlist)
            {
                pw.WriteInt(serial);
            }

          
            //todo : rings
            pw.WriteLong(0);

            chr.sendPacket(pw);
        }

        public static void ItemEffect(Character chr)
        {
            Packet pw = new Packet(0x62);
            pw.WriteInt(chr.ID);
            pw.WriteInt(1602000);
            DataProvider.Maps[chr.Map].SendPacket(pw);
        }

        public static void SendPlayerChangeEquips(Character chr)
        {
            Packet pw = new Packet(0x63);
            pw.WriteInt(chr.ID);

            pw.WriteByte(1);

            pw.WriteByte(chr.Gender);
            pw.WriteByte(chr.Skin); // Skin
            pw.WriteInt(chr.Face); // Face
            pw.WriteByte(0);
            pw.WriteInt(chr.Hair); // Hair
            chr.Inventory.GeneratePlayerPacket(pw);
            pw.WriteByte(0xFF); // Equips shown end
            pw.WriteInt(chr.GetPetID()); // Pet
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

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }


        public static void SendPlayerLevelupAnim(Character chr)
        {
            Packet pw = new Packet(0x64); 
            pw.WriteInt(chr.ID);
            pw.WriteByte(0x00);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }


        public static void SendPlayerSkillAnim(Character chr, int skillid, byte level)
        {
            Packet pw = new Packet(0x64);
            pw.WriteInt(chr.ID);
            pw.WriteByte(0x01);
            pw.WriteInt(skillid);
            pw.WriteInt(level);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendPlayerSkillAnimThirdParty(Character chr, int skillid, byte level, bool party, bool self)
        {
            Packet pw = new Packet();
            if (party && self)
            {
                pw.WriteByte(0x6C);
            }
            else
            {
                pw.WriteByte(0x64);
                pw.WriteInt(chr.ID);
            }
            pw.WriteByte((byte)(party ? 0x02 : 0x01));
            pw.WriteInt(skillid);
            pw.WriteInt(level);
            pw.WriteLong(0);
            pw.WriteLong(0);
            if (self)
            {
                chr.sendPacket(pw);
            }
            else
            {
                DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
            }
        }


        public static void SendPlayerBuffed(Character chr, uint pBuffs)
        {
            Packet pw = new Packet(0x5D);
            pw.WriteInt(chr.ID);
            BuffPacket.AddMapBuffValues(chr, pw, pBuffs);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendPlayerDebuffed(Character chr, uint buffFlags)
        {
            Packet pw = new Packet(0x5E);
            pw.WriteInt(chr.ID);
            pw.WriteUInt(buffFlags);

            DataProvider.Maps[chr.Map].SendPacket(pw, chr, false);
        }

        public static void SendChangeMap(Character chr)
        {
            Packet pack = new Packet(0x2F);
            pack.WriteInt(0); // Channel ID
            pack.WriteByte(chr.PortalCount);
            pack.WriteBool(false); // Is not connecting
            pack.WriteInt(chr.Map);
            pack.WriteByte(chr.MapPosition);
            pack.WriteShort(chr.PrimaryStats.HP);

            chr.sendPacket(pack);
        }

        public static void sendchangemaptest(Character chr)
        {
            Packet pack = new Packet(0x2F);
            pack.WriteInt(0);
            pack.WriteByte(chr.PortalCount);
            pack.WriteBool(false);
            pack.WriteInt(922010100);
            pack.WriteByte(0);
            pack.WriteShort(chr.PrimaryStats.HP);
            chr.sendPacket(pack);
        }

        public static void EmployeeEnterField(Character chr) //hired merchant :D
        {
            Packet pw = new Packet(0x84); //not the right opcode
            pw.WriteByte(chr.PortalCount);
            pw.WriteInt(chr.ID);
            pw.WriteByte(0); //??
            pw.WriteInt(chr.Map);
            pw.WriteInt(295); //Wutang's ID
            pw.WriteByte(chr.MapPosition); //probably spawnpoint 
            pw.WriteShort(chr.Position.X);
            pw.WriteShort(chr.Position.Y);
            pw.WriteInt(1); //??
            pw.WriteShort(chr.PrimaryStats.HP);
            pw.WriteShort(chr.PrimaryStats.MP);
            pw.WriteShort(1); //??
            pw.WriteLong(0);
            pw.WriteLong(0);
            chr.sendPacket(pw);


        }

        public static void SendJoinGame(Character chr)
        {
            Packet pack = new Packet(0x2F);

            pack.WriteInt(Server.Instance.ID); // Channel ID
            pack.WriteByte(0); // 0 portals
            pack.WriteBool(true); // Is connecting

            
            chr.Randomizer.GenerateConnectPacket(pack);

            pack.WriteShort(-1); // Flags (contains everything: 0xFFFF)

            pack.WriteInt(chr.ID);
            pack.WriteString(chr.Name, 13);
            pack.WriteByte(chr.Gender); // Gender
            pack.WriteByte(chr.Skin); // Skin
            pack.WriteInt(chr.Face); // Face
            pack.WriteInt(chr.Hair); // Hair

            pack.WriteLong(0); // Pet Cash ID :/

            pack.WriteByte(chr.PrimaryStats.Level); // Level
            pack.WriteShort(chr.PrimaryStats.Job); // Jobid
            pack.WriteShort(chr.PrimaryStats.Str); //charc.str);
            pack.WriteShort(chr.PrimaryStats.Dex); //charc.dex);
            pack.WriteShort(chr.PrimaryStats.Int); //charc.intt);
            pack.WriteShort(chr.PrimaryStats.Luk); //charc.luk);
            pack.WriteShort(chr.PrimaryStats.HP); //charc.hp);
            pack.WriteShort(chr.PrimaryStats.BuffMHP); //charc.mhp); //Needs to be set to Original MAX HP before using hyperbody.
            pack.WriteShort(chr.PrimaryStats.MP); //charc.mp);
            pack.WriteShort(chr.PrimaryStats.GetMaxMP(true)); //charc.mmp);
            pack.WriteShort(chr.PrimaryStats.AP); //charc.ap);
            pack.WriteShort(chr.PrimaryStats.SP); //charc.sp);
            pack.WriteInt(chr.PrimaryStats.EXP); //charc.exp);
            pack.WriteShort(chr.PrimaryStats.Fame); //charc.fame);


            pack.WriteInt(chr.Map); //definitly map ID
            pack.WriteByte(chr.MapPosition);

            pack.WriteByte(20); //Buddylist slots ?
            pack.WriteInt(chr.Inventory.mMesos); //Mesos

            pack.WriteByte(24); //Slot 1
            pack.WriteByte(24);
            pack.WriteByte(24);
            pack.WriteByte(24);
            pack.WriteByte(52);
            


            chr.Inventory.GenerateInventoryPacket(pack);

            //Skills definitly start here
            chr.Skills.AddSkills(pack);

            
            pack.WriteShort((short)chr.Quests.RealQuests); // Running quests
            Console.WriteLine("wtf real quests : " + chr.Quests.RealQuests);
            foreach (KeyValuePair<int, QuestData> kvp in chr.Quests.mQuests)
            {
                if (!kvp.Value.Complete)
                {
                    pack.WriteShort((short)kvp.Key);
                    pack.WriteString(kvp.Value.Data);
                }
            }

            pack.WriteShort((short)chr.Quests.mCompletedQuests.Count); // Running quests
            foreach (KeyValuePair<int, QuestData> kvp in chr.Quests.mCompletedQuests)
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
                
           
                /*
                 * For every ring, 33 unkown bytes.
                */
               
            
            chr.Inventory.AddRockPacket(pack);
            
            /**
            pack.WriteByte(1);
            pack.WriteInt(1112001);
            pack.WriteInt(1112001);
            pack.WriteInt(327);
            pack.WriteInt(1112001);
            pack.WriteInt(1112001);
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

            chr.sendPacket(pack);
        }
    }
}