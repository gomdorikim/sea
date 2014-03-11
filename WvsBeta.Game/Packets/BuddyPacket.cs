using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;


namespace WvsBeta.Game
{
    class BuddyPacket
    {

        public static void HandleBuddy(Character chr, Packet packet)
        {
            byte header = packet.ReadByte(); //Which case
            switch (header)
            {
                case 1: //Invite
                    string Victim = packet.ReadString();
                    //Server.Instance.CenterConnection.BuddyInvite(Victim, chr.Name, chr.ID);
                    Server.Instance.CenterConnection.PlayerBuddyOperation(chr, 1, Victim);
                    break;
                case 2: //Accept
                    int who = packet.ReadInt();
                    Server.Instance.CenterConnection.PlayerBuddyOperation(chr, 2, "", who);
                    break;
                case 3:
                    int test = packet.ReadInt();
                    //MessagePacket.SendNotice(packet.ToString(), chr);  
                    Server.Instance.CenterConnection.PlayerBuddyOperation(chr, 3, "", test);
                    break;
            }
            
        }

        public static void UpdateBuddyList(Character chr, byte accepted)
        {
            Packet pw = new Packet(0x21);
            pw.WriteByte(0x07);
            pw.WriteByte(1); //buddylist size
            //foreach (Buddy bl in chr.Buddylist.Values)
            ////{
                pw.WriteInt(328);
                pw.WriteString("asdas", 13);
                pw.WriteByte(1); //accepted : 0, pending : 1, removed : 2
                pw.WriteInt(0); //channel

            pw.WriteInt(0); //amount of buddies
           
            chr.sendPacket(pw);
            
        }

        

        public static void BuddyReset(Character chr)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x21);
            pw.WriteByte(0x07);
            pw.WriteByte(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            chr.sendPacket(pw);
        }

        public static void BuddyChannel(Character chr, int BuddyID, int ChannelID)
        {
            //In order for this packet to actually work, the buddy has to be in your buddylist 
            Packet pw = new Packet(0x21);
            pw.WriteByte(0x14);
            pw.WriteInt(BuddyID);
            pw.WriteByte(0);
            pw.WriteInt(ChannelID);
            chr.sendPacket(pw);
        }


        public static void RequestBuddyListAdd(Character chr, int ID, string name)
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
            Packet pw = new Packet(0x21);
            pw.WriteByte(0x09);
            pw.WriteInt(ID);
            pw.WriteString(name);
            pw.WriteInt(ID);
            pw.WriteString(name, 13);
            pw.WriteByte(1); //1
            pw.WriteInt(0);//offline? lol
            pw.WriteByte(1);
            chr.sendPacket(pw);


        }

        public static void BuddyMessage(Character chr, byte Message)
        {
            Packet pw = new Packet(0x21);
            pw.WriteByte(Message);
            chr.sendPacket(pw);
        }
    }
}
