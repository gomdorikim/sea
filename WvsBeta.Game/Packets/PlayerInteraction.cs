/*
 * Author: Rice
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    enum Interaction : byte
    {
        CREATE = 0x00,
    }

    class PlayerInteractionPacket
    {
        public static void HandleInteraction(Character chr, Packet packet)
        {
            Interaction mode = (Interaction)packet.ReadByte();
            switch (mode)
            {
                case Interaction.CREATE:
                    byte function = packet.ReadByte();
                    if (function == 0x03) //Trade
                    {
                        //CharacterTrade.startTrade(chr);
                    }
                    else if (function == 0x01) //Minigame: Omok
                    {

                        string name = packet.ReadString();
                        byte locked = packet.ReadByte();
                        string password = string.Empty;
                        if (locked == 1)
                            password = packet.ReadString();
                        int x = packet.ReadInt();
                        int y = packet.ReadInt();
                        byte piecetype = packet.ReadByte();
                        addOmokBox(chr, 1, 0);
                    }
                    else if (function == 0x02) //Minigame: Match card
                    {

                    }
                    break;

            }
        }


        public static void addAnnounceBox(Character chr, int derp, int gametype, int type, int ammount, int joinable)
        {
            Packet pw = new Packet();
            pw.WriteInt(gametype);
            pw.WriteInt(1); //not a permananent solution its gametype (not sure what the gametype ID's are lol) i assume this is omok
            pw.WriteString("lolderp"); //this should be game description
            pw.WriteByte(0);
            pw.WriteInt(type);
            pw.WriteInt(ammount);
            pw.WriteByte(2);
            pw.WriteInt(joinable);
            chr.sendPacket(pw);

        }

        public static byte[] addOmokBox(Character chr, int ammount, int type)
        {
            Packet pw = new Packet(0xAF);
            pw.WriteInt(chr.ID);
            addAnnounceBox(chr, 1, 1, 0, ammount, type);
            return pw.ToArray();
        }

        public static void getMiniGame(Character chr, bool owner, int piece)
        {
            Packet pw = new Packet(0xAF);
            pw.WriteByte(5); //ROOM
            pw.WriteByte(1);
            pw.WriteByte(0);
            pw.WriteBool(true);
            pw.WriteByte(0);
            

        }
    }
}