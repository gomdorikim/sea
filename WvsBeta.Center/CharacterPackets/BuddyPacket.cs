using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WvsBeta.Common.Sessions;

namespace WvsBeta.Center
{
    class BuddyPacket
    {
        public static void BuddyReset(Character chr)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x28);
            pw.WriteByte(0x07);
            pw.WriteByte(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            chr.SendPacket(pw);
        }

        public static byte[] UpdateChannel(int Channel, int BuddyID)
        {
            //In order for this packet to actually work, the buddy has to be in your buddylist 
            Packet pw = new Packet(0x28);
            pw.WriteByte(0x14);
            pw.WriteInt(BuddyID);
            pw.WriteByte(0);
            pw.WriteInt(Channel);
            return pw.ToArray();
        }


        public static void RequestBuddyListAdd(Character chr, int ID, string name, int channel)
        {/**
          *  Packet pw = new Packet(0x21);
            pw.WriteByte(0x09);
            pw.WriteInt(ID);
            pw.WriteString("asdsa");
            pw.WriteInt(ID);
            pw.WriteString("asdsa", 13);
            pw.WriteByte(1);
            pw.WriteInt(0);
            pw.WriteByte(1);
            chr.sendPacket(pw);
          * **/
            Packet pw = new Packet(0x28);
            pw.WriteByte(0x09);
            pw.WriteInt(ID);
            pw.WriteString(name);
            pw.WriteInt(ID);
            pw.WriteString(name, 13);
            pw.WriteByte(1);
            pw.WriteInt(channel);
            pw.WriteByte(1);
            chr.SendPacket(pw);


        }

        public static byte[] RequestAddBuddy(Character to, Character from)
        {
            Packet pw = new Packet(0x28);
            pw.WriteByte(0x09);
            pw.WriteInt(from.ID);
            pw.WriteString(from.Name);
            pw.WriteInt(from.ID);
            pw.WriteString(from.Name, 13);
            pw.WriteByte(1);
            pw.WriteInt(0); // Channel
            pw.WriteByte(1);
            return pw.ToArray();
        }

        public static byte[] RequestAddBuddy2()
        {
            Packet pw = new Packet(0x28);
            pw.WriteByte(0x09);
            pw.WriteInt(327);
            pw.WriteString("asdas");
            pw.WriteInt(327);
            pw.WriteString("asdas", 13);
            pw.WriteByte(1);
            pw.WriteInt(0); // Channel
            pw.WriteByte(1);
            return pw.ToArray();
        }

        public static byte[] UpdateBuddyList(Character pCharacter)
        {
            Packet pw = new Packet(0x28);
            pw.WriteByte(0x07);
            pw.WriteByte((byte)pCharacter.FriendsList.Count);
            foreach(KeyValuePair<int, Buddy> kvp in pCharacter.FriendsList) //22 bytes ?
            {
                pw.WriteInt(kvp.Value.CharacterID); //4
                pw.WriteString(kvp.Value.Name, 13); //17
                if (kvp.Value.Assigned)
                    pw.WriteByte(0);
                else
                    pw.WriteByte(1);
                
                Character Buddy = CenterServer.Instance.GetCharacterByCID(kvp.Value.CharacterID);
                if (CenterServer.Instance.IsOnline(Buddy) && Buddy.FriendsList.ContainsKey(pCharacter.ID))
                {
                    if (Buddy.FriendsList[pCharacter.ID].Assigned)
                        pw.WriteInt(kvp.Value.Channel);
                    else
                        pw.WriteInt(-1);
                }
                else
                {
                    pw.WriteInt(-1);
                }
            }

            for (int i = 0; i < pCharacter.FriendsList.Count; i++)
            {
                pw.WriteInt(0);
            }
            return pw.ToArray();
        }

        public static byte[] BuddyMessage(BuddyResults Message)
        {
            Packet pw = new Packet(0x28);
            pw.WriteByte((byte)Message);
            return pw.ToArray();
        }
    }
}
    

