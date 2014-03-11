
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class MiniRoomPacket
    {
        
        public static void HandlePacket(Character pCharacter, Packet pPacket)
        {
            //MessagePacket.SendNotice(pPacket.ToString(), pCharacter);
            byte Type = pPacket.ReadByte();
            switch (Type)
            {
                case 0:
                    {
                        byte RoomType = pPacket.ReadByte();
                        switch (RoomType)
                        {
                            case 0: 
                                
                                break;
                            case 1: 
                                //Minigame
                                MiniRoomBase omok = MiniRoomBase.CreateRoom(pCharacter, 1, pPacket, false, 0);
                                pCharacter.Room = omok;

                                MiniGamePacket.ShowWindow(pCharacter, omok, MiniRoomBase.Omoks[pCharacter.Room.ID].OmokType);
                                MiniGamePacket.AddAnnounceBox(pCharacter, (byte)MiniRoomBase.RoomType.Omok, omok.ID, omok.Title, omok.Private, omok.PieceType, false);
                                break;
                            case 2: 
                                //Minigame
                                string pTitle = pPacket.ReadString();
                                byte Password = pPacket.ReadByte();
                                if (Password == 0) //no password
                                {
                                    
                                }
                                else
                                {
                                    string pPassword = pPacket.ReadString();
                                     //
                                }
                                pPacket.Skip(7);
                                byte MatchCardType = pPacket.ReadByte();
                                break;
                            case 3: //Trade
                                {
                                    {
                                        if (pCharacter.Room != null)
                                        {
                                            return; // ALREADY OPENED.
                                        }
                                        MiniRoomBase mrb = MiniRoomBase.CreateRoom(pCharacter, RoomType, pPacket, false, 0);
                                        pCharacter.Room = mrb;
                                        MiniRoomPacket.ShowWindow(mrb, pCharacter);
                                        break;
                                    }
                                }
                            case 4:
                                //Player Shop
                                {
                                    MiniRoomBase mrb = MiniRoomBase.CreateRoom(pCharacter, RoomType, pPacket, false, 0);
                                    pCharacter.Room = mrb;
                                    PlayerShopPackets.OpenPlayerShop(pCharacter, mrb);
                                    break;
                                }
                        }
                        break;
                    }
                case 0x02:
                    {
                        if (pCharacter.Room == null)
                        {
                            InviteResult(pCharacter, 1);
                            return; // NOT OPENED OR FULL
                        }
                        int playerid = pPacket.ReadInt();
                        Character victim = DataProvider.Maps[pCharacter.Map].GetPlayer(playerid);

                        if (victim == null)
                        {
                            // Not found!
                            InviteResult(pCharacter, 1);
                        }
                        else if (pCharacter.Room.IsFull())
                        {
                            InviteResult(pCharacter, 2, victim.Name); // DEM REAL DEAL
                        }
                        else
                        {
                            Invite(pCharacter.Room, pCharacter, victim);
                        }

                        break;
                    }
                case 0x03:
                    {
                        int roomid = pPacket.ReadInt();
                        if (!MiniRoomBase.MiniRooms.ContainsKey(roomid))
                        {
                            // REPORt
                            ReportManager.FileNewReport("Tried opening a trade room without a proper ID.", pCharacter.ID, 0);
                            return;
                        }
                        MiniRoomBase mrb = MiniRoomBase.MiniRooms[roomid];
                        if (mrb.IsFull())
                        {

                        }
                        break;
                    }
                case 0x04: // Enter Room
                    {
                        if (pCharacter.Room != null)
                        {
                            return; // ALREADY OPENED.
                        }
                        int roomid = pPacket.ReadInt();
                        //MessagePacket.SendNotice(roomid.ToString(), pCharacter);
                        if (!MiniRoomBase.MiniRooms.ContainsKey(roomid))
                        {
                            // REPORt
                            ReportManager.FileNewReport("Tried entering a trade room without a proper ID.", pCharacter.ID, 0);
                            return;
                        }
                        MiniRoomBase mrb = MiniRoomBase.MiniRooms[roomid];
                        if (mrb.EnteredUsers != 0)
                        {
                            if (mrb.IsFull())
                            {

                            }
                            else
                            {
                               
                                pCharacter.Room = mrb;
                                byte rt = (byte)pCharacter.Room.Type;
                                switch (rt)
                                {
                                    case 1:
                                        {
                                            bool HasPassword = pPacket.ReadBool();

                                            Omok omok = MiniRoomBase.Omoks[pCharacter.Room.ID];

                                            if (HasPassword)
                                            {
                                                string pPassword = pPacket.ReadString();
                                                //MessagePacket.SendNotice(pPassword.ToString(), pCharacter);
                                                if (pPassword == omok.Password)
                                                {
                                                    if (pCharacter.Inventory.mMesos >= 100)
                                                    {
                                                        omok.AddPlayer(pCharacter);
                                                        MiniGamePacket.AddVisitor(pCharacter, mrb);
                                                        MiniGamePacket.ShowWindow(pCharacter, mrb, omok.OmokType);
                                                        pCharacter.AddMesos(-100);
                                                    }
                                                    else
                                                    {
                                                        MiniGamePacket.ErrorMessage(pCharacter, MiniGamePacket.MiniGameError.NotEnoughMesos);
                                                    }
                                                }
                                                else
                                                {
                                                    //Show some message ?
                                                    MiniGamePacket.ErrorMessage(pCharacter, MiniGamePacket.MiniGameError.IncorrectPassword);
                                                    pCharacter.Room = null;
                                                }
                                            }
                                            else
                                            {
                                                if (pCharacter.Inventory.mMesos >= 100)
                                                {
                                                    omok.AddPlayer(pCharacter);
                                                    MiniGamePacket.AddVisitor(pCharacter, mrb);
                                                    MiniGamePacket.ShowWindow(pCharacter, mrb, omok.OmokType);
                                                    pCharacter.AddMesos(-100);
                                                }
                                                else
                                                {
                                                    MiniGamePacket.ErrorMessage(pCharacter, MiniGamePacket.MiniGameError.NotEnoughMesos);
                                                    pCharacter.Room = null;
                                                }
                                            }
                                                //MiniGamePacket.Start(pCharacter, pCharacter.Room);
                                            break;  
                                        }
                                    case 3:
                                        {
                                            mrb.AddPlayer(pCharacter);
                                            MiniRoomPacket.ShowJoin(mrb, pCharacter);
                                            MiniRoomPacket.ShowWindow(mrb, pCharacter);
                                            break;
                                        }
                                    case 4:
                                        {
                                            PlayerShop ps = MiniRoomBase.PlayerShops[roomid];
                                            for (int i = 0; i < ps.EnteredUsers; i++)
                                            {
                                                Character pUser = mrb.Users[i];
                                                if (pUser != null && pUser != pCharacter)
                                                {
                                                    ps.AddPlayer(pCharacter);
                                                    PlayerShopPackets.AddPlayer(pCharacter, pUser);
                                                    PlayerShopPackets.OpenPlayerShop(pCharacter, mrb);
                                                    PlayerShopPackets.PersonalShopRefresh(pCharacter, ps); //Show items 
                                                }
                                            } break;
                                        }
                                    }
                                }
                            }
                            break;
                       }

                case 0x06: // Chat
                    {
                        if (pCharacter.Room != null)
                            Chat(pCharacter.Room, pCharacter, pPacket.ReadString(), -1);
                        break;
                    }
                case 0x12: //Add item to Player Shop
                    {
                        byte inventory = pPacket.ReadByte();
                        short inventoryslot = pPacket.ReadShort();
                        short bundleamount = pPacket.ReadShort();
                        short AmountPerBundle = pPacket.ReadShort();
                        int price = pPacket.ReadInt();
                        PlayerShop.HandleShopUpdateItem(pCharacter, inventory, inventoryslot, bundleamount, AmountPerBundle, price);
                        break;
                    }
                case 0x13: //Buy item from shop
                    {
                        byte slot = pPacket.ReadByte();
                        short bundleamount = pPacket.ReadShort();
                        PlayerShop ps = MiniRoomBase.PlayerShops[pCharacter.Room.ID];
                        if (ps != null)
                        {
                            ps.BuyItem(pCharacter, slot, bundleamount);
                        }
                        break;
                    }
                case 0xA: //Leave
                    {
                        MiniRoomBase mr = MiniRoomBase.MiniRooms[pCharacter.Room.ID];
                        if (mr.Type == MiniRoomBase.RoomType.Trade)
                        {
                            if (mr != null)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    Character chr = mr.Users[i];
                                    Character leader = mr.Users[0];
                                    if (chr == null) continue;
                                    mr.RemovePlayer(chr, 1);

                                    //mr.Users[i] = null; //send this after all characters are removed


                                }
                            }
                        }
                        else if (mr.Type == MiniRoomBase.RoomType.PersonalShop)
                        {
                            mr.RemovePlayerFromShop(pCharacter);
                        }
                        else if (mr.Type == MiniRoomBase.RoomType.Omok)
                        {
                            //MessagePacket.SendNotice("leave omok", pCharacter);
                            Omok omok = MiniRoomBase.Omoks[pCharacter.Room.ID];
                            if (pCharacter == omok.Users[0])
                            {
                                omok.CloseOmok(pCharacter);
                            }
                            else
                            {
                                ShowLeaveRoom(pCharacter.Room, pCharacter, 2);
                                omok.RemovePlayer(pCharacter, 1);
                            }
                        }
                    }
                    break;
                case 0xB: //Add announce box
                    {
                        if (pCharacter.Room != null)
                        {
                    
                            MiniGamePacket.AddAnnounceBox(pCharacter, (byte)pCharacter.Room.Type, pCharacter.Room.ID, pCharacter.Room.Title, pCharacter.Room.Private, 0, false);
                            byte RoomType = (byte)pCharacter.Room.Type;
                            switch (RoomType)
                            {
                                case 1:
                                    DataProvider.Maps[pCharacter.Map].Omoks.Add(pCharacter.Room.ID, MiniRoomBase.Omoks[pCharacter.Room.ID]);
                                    break;
                                case 4:
                                    DataProvider.Maps[pCharacter.Map].PlayerShops.Add(pCharacter.Room.ID, MiniRoomBase.PlayerShops[pCharacter.Room.ID]);
                                    break;
                            }
                            
                        }
                        break;
                    }
                case 0x17: //Move Item from player shop to inventory
                    {
                        byte slot = pPacket.ReadByte(); //reads as byte, sends as short... wtf lol
                        PlayerShop ps = MiniRoomBase.PlayerShops[pCharacter.Room.ID];
                        ps.HandleMoveItemBack(pCharacter, slot);
                        ps.Items.Remove(slot);
                        break; 
                    }
                case 0x19: //Request tie result
                    {
                        bool result = pPacket.ReadBool();
                        break;
                    }
                case 0x20: //Ready
                    {
                        MiniGamePacket.Ready(pCharacter, pCharacter.Room);
                        break;
                    }
                case 0x21:
                    {
                        MiniGamePacket.UnReady(pCharacter, pCharacter.Room);
                        break; 
                    }
                case 0x22: //Expell user
                    {
                        //Todo : expell
                        break;
                    }
                case 0x23:
                    {
                        Omok omok = MiniRoomBase.Omoks[pCharacter.Room.ID];
                        if (omok != null)
                        {
                            MiniGamePacket.Start(pCharacter, pCharacter.Room);
                            omok.GameStarted = true;
                        }
                        break;
                    }
                case 0x25:
                    {
                        Omok omok = MiniRoomBase.Omoks[pCharacter.Room.ID];
                        omok.UpdateGame(pCharacter);
                        omok.GameStarted = false;
                        break;
                    }
                case 0x26: //Place omok piece
                    {
                        Omok omok = MiniRoomBase.Omoks[pCharacter.Room.ID];
                        if (omok != null)
                        {
                            int X = pPacket.ReadInt();
                            int Y = pPacket.ReadInt();
                            byte Piece = pPacket.ReadByte();

                            if (omok.Stones[X, Y] != Piece && omok.Stones[X, Y] != omok.GetOtherPiece(Piece))
                            {
                                MiniGamePacket.MoveOmokPiece(pCharacter, pCharacter.Room, X, Y, Piece);
                                omok.AddStone(X, Y, Piece, pCharacter);
                            }
                            else
                            {
                                MiniGamePacket.OmokMessage(pCharacter, pCharacter.Room, 0);
                            }
                            //MessagePacket.SendNotice("X : " + X + " Y : " + Y, pCharacter);
                            if (omok.CheckStone(Piece))
                            {
                                //MessagePacket.SendNotice("Win!", pCharacter);
                                omok.UpdateGame(pCharacter);
                                Piece = 0xFF;
                                omok.GameStarted = false;
                            }      
                        }
                        break;
                    }
                case 0x1C:
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (pCharacter.Room.Users[i] != pCharacter)
                            {
                                MiniGamePacket.RequestHandicap(pCharacter.Room.Users[i], pCharacter.Room);
                            }
                        }
                        break;
                    }
                case 0x1D: //Request handicap result
                    {
                        bool result = pPacket.ReadBool();
                        Omok omok = MiniRoomBase.Omoks[pCharacter.Room.ID];
                        if (omok != null)
                        {
                            if (result == true)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    if (pCharacter.Room.Users[i] != pCharacter)
                                    {
                                        if (omok.PlacedStone[i] == false)
                                        {
                                            MiniGamePacket.RequestHandicapResult(pCharacter, pCharacter.Room, result, 2);
                                            omok.TotalStones-=2;
                                            //MessagePacket.SendNotice("removed", pCharacter);
                                        }
                                        else 
                                        {
                                            MiniGamePacket.RequestHandicapResult(pCharacter, pCharacter.Room, result, 1);
                                            omok.TotalStones--;
                                            //omok.Stones[omok.LastPlacedStone[(byte)(pCharacter.RoomSlotId + 1)].mX, omok.LastPlacedStone[(byte)(pCharacter.RoomSlotId + 1)].mY] = 0xFF;
                                            //MessagePacket.SendNotice("Removed stone", pCharacter);
                                        }
                                    }
                         
                                }
                            }
                        }
                        break;
                    }
                default:
                    {
                        if (pCharacter.Room != null)
                            pCharacter.Room.OnPacket(pCharacter, Type, pPacket);
                        break;
                    }
            }
        }
       
        public static void HandleTradePacket(Character pCharacter, Packet pPacket)
        {
            byte handleType = pPacket.ReadByte();
            switch (handleType)
            {
                case 0: // Open up
                    {
                        if (pCharacter.Room != null)
                        {
                            return; // ALREADY OPENED.
                        }

                        byte type = pPacket.ReadByte();
                        MiniRoomBase mrb = MiniRoomBase.CreateRoom(pCharacter, type, pPacket, false, 0);
                        pCharacter.Room = mrb;

                        break;
                    }
                case 0x02: // Invite
                    {
                        if (pCharacter.Room == null)
                        {
                            InviteResult(pCharacter, 1);
                            return; // NOT OPENED OR FULL
                        }
                        int playerid = pPacket.ReadInt();
                        Character victim = DataProvider.Maps[pCharacter.Map].GetPlayer(playerid);

                        if (victim == null)
                        {
                            // Not found!
                            InviteResult(pCharacter, 1);
                        }
                        else if (pCharacter.Room.IsFull())
                        {
                            InviteResult(pCharacter, 2, victim.Name); // DEM REAL DEAL
                        }
                        else
                        {
                            Invite(pCharacter.Room, pCharacter, victim);
                        }

                        break;
                    }

                case 0x03: // Invite Result
                    {
                        int roomid = pPacket.ReadInt();
                        if (!MiniRoomBase.MiniRooms.ContainsKey(roomid))
                        {
                            // REPORt
                            ReportManager.FileNewReport("Tried opening a trade room without a proper ID.", pCharacter.ID, 0);
                            return;
                        }
                        MiniRoomBase mrb = MiniRoomBase.MiniRooms[roomid];
                        if (mrb.IsFull())
                        {

                        }
                        break;
                    }

                case 0x04: // Enter Room
                    {
                        if (pCharacter.Room != null)
                        {
                            return; // ALREADY OPENED.
                        }
                        int roomid = pPacket.ReadInt();
                        if (!MiniRoomBase.MiniRooms.ContainsKey(roomid))
                        {
                            // REPORt
                            ReportManager.FileNewReport("Tried entering a trade room without a proper ID.", pCharacter.ID, 0);
                            return;
                        }
                        MiniRoomBase mrb = MiniRoomBase.MiniRooms[roomid];
                        if (mrb.EnteredUsers != 0)
                        {
                            if (mrb.IsFull())
                            {

                            }
                            else
                            {
                                mrb.AddPlayer(pCharacter);
                                pCharacter.Room = mrb;
                            }
                        }
                        break;
                    }

                case 0x06: // Chat
                    {
                        if (pCharacter.Room != null)
                            Chat(pCharacter.Room, pCharacter, pPacket.ReadString(), -1);
                        break;
                    }
                case 0xA: //Leave
                    {
                        MiniRoomBase mr = MiniRoomBase.MiniRooms[pCharacter.Room.ID];
                        if (mr != null)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Character chr = mr.Users[i];
                                Character leader = mr.Users[0];
                                if (chr == null) continue;
                                mr.RemovePlayer(chr, 1);
                                
                                //mr.Users[i] = null; //send this after all characters are removed
                                
                                
                            }
                        }
                    }
                    break;
                
              
                    default:
                    {
                        if (pCharacter.Room != null)
                            pCharacter.Room.OnPacket(pCharacter, handleType, pPacket);
                        break;
                    }
            }
        }


        public static void ShowWindowTest(Character pCharacter)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(5);
            pw.WriteByte(3);
            pw.WriteByte(2);

            pw.WriteByte(0);

            pw.WriteByte(0);
            PacketHelper.AddAvatar(pw, pCharacter);
            pw.WriteString("lolol");

            pw.WriteByte(0xFF);
            pCharacter.sendPacket(pw);
        }

        public static void ShowWindow(MiniRoomBase pRoom, Character pTo)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(5);
            pw.WriteByte((byte)pRoom.Type);
            pw.WriteByte(pRoom.MaxUsers);
            pw.WriteByte(pTo.RoomSlotId);

            for (int i = 0; i < pRoom.Users.Length; i++)
            {
                Character character = pRoom.Users[i];
                if (character == null) continue;
                pw.WriteByte(character.RoomSlotId);

                PacketHelper.AddAvatar(pw, character);

                pw.WriteString(character.Name);
            }

            
            pw.WriteByte(0xFF);

            pRoom.EncodeEnter(pTo, pw);

            pTo.sendPacket(pw);
            
        }

        public static void ShowJoin(MiniRoomBase pRoom, Character pWho)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(4);

            pw.WriteByte(pWho.RoomSlotId);

            PacketHelper.AddAvatar(pw, pWho);
            pw.WriteString(pWho.Name);

            pRoom.EncodeEnterResult(pWho, pw);


            pRoom.BroadcastPacket(pw, pWho);
        }

        public static void ShowLeave(MiniRoomBase pRoom, Character pWho, byte pReason)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(0xA);

            pw.WriteByte(pWho.RoomSlotId);
            pw.WriteByte(pReason);
            pWho.sendPacket(pw);
        }

        public static void ShowLeaveRoom(MiniRoomBase pRoom, Character pWho, byte pReason)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(0xA);

            pw.WriteByte(pWho.RoomSlotId);
            pw.WriteByte(pReason);
            pRoom.BroadcastPacket(pw);
        }

        public static void Invite(MiniRoomBase pRoom, Character pWho, Character pVictim)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(2);
            pw.WriteByte((byte)pRoom.Type);

            pw.WriteString(pWho.Name);
            pw.WriteInt(pRoom.ID);

            pVictim.sendPacket(pw);
        }

        public static void InviteResult(Character pWho, byte pFailID, string pName = "")
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(3);
            pw.WriteByte(pFailID);
            if (pFailID == 2 || pFailID == 0)
            {
                pw.WriteString(pName);
            }

            pWho.sendPacket(pw);
        }

        public static void Chat(MiniRoomBase pRoom, Character pCharacter, string pText, sbyte pMessageCode)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(6);
            if (pMessageCode < 0)
            {
                pw.WriteByte(8);
                pw.WriteByte(pCharacter.RoomSlotId);
                pw.WriteString(string.Format("{0} : {1}", pCharacter.Name, pText));
            }
            else
            {
                pw.WriteByte(7);
                pw.WriteSByte(pMessageCode);
                pw.WriteString(pCharacter.Name);
            }
            pRoom.BroadcastPacket(pw);
        }
    }


    class TradePacket
    {
        public static void AddItem(Character pPoster, Character pTo, byte pSlot, Item pItem)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(13);

            pw.WriteBool(pPoster != pTo);
            pw.WriteByte(pSlot);
            //pw.WriteByte(WvsBeta.Common.Constants.getItemTypeInPacket(pItem.ItemID));
            PacketHelper.AddItemData(pw, pItem, 0, false);

            pTo.sendPacket(pw);
        }
        public static void AddItemWithAmount(Character pPoster, Character pTo, byte pSlot, Item pItem, short amount)
        {
            //Used for items from the same stack
            Packet pw = new Packet(0xBA);
            pw.WriteByte(13);

            pw.WriteBool(pPoster != pTo);
            pw.WriteByte(pSlot);
            //pw.WriteByte(WvsBeta.Common.Constants.getItemTypeInPacket(pItem.ItemID));
            PacketHelper.AddItemDataWithAmount(pw, pItem, 0, false, amount);

            pTo.sendPacket(pw);
        }

        public static void PutCash(Character pPoster, Character pTo, int pAmount, byte test)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(14);

            pw.WriteByte(test);
            pw.WriteInt(pAmount);

            pTo.sendPacket(pw);
        }

        public static void SelectTrade(Character pTo)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(0xF);
            pTo.sendPacket(pw);
        }

        public static void TradeUnsuccessful(Character pTo)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(10);
            pw.WriteByte(pTo.RoomSlotId);
            pw.WriteByte(6);
            pTo.sendPacket(pw);
        }

        public static void TradeSuccessful(Character pCompleter)
        {
            Packet pw = new Packet(0xBA);
            pw.WriteByte(10);
            pw.WriteByte(pCompleter.RoomSlotId);
            pw.WriteByte(5);
            pCompleter.sendPacket(pw);
        }
    }
}