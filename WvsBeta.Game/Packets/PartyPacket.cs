using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class PartyPacket
    {


        public static void HandleCreateParty(Character chr, Packet packet)
        {
            short header = packet.ReadByte();
            switch (header)
            {
                case 1: //Create
                    PartyCreated(chr);          
                    Server.Instance.CenterConnection.PlayerPartyOperation(chr, 1, 0);
                    //SilentUpdate(chr);
                    break;
                case 2: //Leave
                    Server.Instance.CenterConnection.PlayerPartyOperation(chr, 2, chr.PartyID);
                    break;
                case 3: //Join
                    int partyid = packet.ReadInt();
                    Server.Instance.CenterConnection.PlayerPartyOperation(chr, 3, partyid);
                    break;
                case 4: //Invite
                    int id = packet.ReadInt();
                    string name = Server.Instance.CharacterDatabase.getCharacterNameByID(id);
                    Character pVictim = Server.Instance.GetCharacter(name);
                    PartyInvite(chr, pVictim, packet, chr.PartyID);
                    //Server.Instance.CenterConnection.PlayerPartyOperation(
                    break;
                case 5: //Expell 
                    int victimid = packet.ReadInt();
                    Server.Instance.CenterConnection.PlayerPartyOperation(chr, 5, chr.PartyID, victimid);
                    break;
            }
        }

        public static void HandlePartyMessages(Character chr, Packet packet)
        {
            byte header = packet.ReadByte();
            //MessagePacket.SendNotice(header.ToString(), chr);
            switch (header)
            {
                case 0x13: //Blocking invitations
                    string leadername = packet.ReadString();
                    string inviteename = packet.ReadString();
                    Character pLeader = Server.Instance.GetCharacter(leadername);
                    CharacterPartyMessage(pLeader, inviteename, 0x13);
                    break;
                case 0x14: //Taking care of another invitation
                    string leader = packet.ReadString();
                    string invitee = packet.ReadString();
                    Character Leader = Server.Instance.GetCharacter(leader);
                    CharacterPartyMessage(Leader, invitee, 0x14);
                    break;
                case 0x15: //Deny party request
                    string pleader = packet.ReadString();
                    string pinvitee = packet.ReadString();
                    Server.Instance.CenterConnection.PlayerPartyOperation(chr, 0x15, 0, 0, false, pleader, pinvitee);
                    break;
            }
        }

        public static void PartyCreated(Character chr)
        {
            //20 07 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01
            Packet pw = new Packet();
            pw.WriteByte(0x27); 
            pw.WriteByte(7);
            pw.WriteInt(1); //PartyID
            pw.WriteInt(1); //Door town ID
            pw.WriteInt(1);  //Door target ID  
            pw.WriteShort(1); //Door X position
            pw.WriteShort(1); //Door Y position
            chr.sendPacket(pw);
        }

        public static void LeaveParty(Character chr, MapleParty party)
        {
            //20 0B 00 00 00 00 00 00 00 00 00
            Packet pw = new Packet();
            pw.WriteByte(0x20);
            pw.WriteByte(0xB); //leave
            pw.WriteInt(party.ID);
            pw.WriteInt(chr.ID);
            pw.WriteByte(1);
            pw.WriteByte(0);
            pw.WriteString(chr.Name);
            AddPartyData(pw);
            chr.sendPacket(pw);

        }


        public static void Disbanded(Character chr, MapleParty party) //it works, but i'm 99% sure the packet is wrong :P
        {
            //You have left since the party leader quit
            Packet pw = new Packet();
            pw.WriteByte(0x20);
            pw.WriteByte(0xB);
            pw.WriteInt(chr.ID);
            pw.WriteInt(party.ID);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteString(chr.Name);
            chr.sendPacket(pw);

        }

        public static void DisbandParty(Character chr, MapleParty party)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x20);
            pw.WriteByte(0xB); //leave
            pw.WriteInt(party.ID);
            pw.WriteInt(chr.ID);
            pw.WriteByte(0); //disband
            chr.sendPacket(pw);

        }

        public static void Expelled(Character chr, MapleParty party)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x20);
            pw.WriteByte(0xB); //leave
            pw.WriteInt(party.ID);
            pw.WriteInt(chr.ID);
            pw.WriteByte(1); //not disbanded
            pw.WriteByte(1); //expelled
            pw.WriteString(chr.Name);
            AddPartyData(pw);
            chr.sendPacket(pw);
        }

        public static byte[] Update(bool offline, Character chr, MapleParty party, byte disband, string victimname, int victimid)
        {
            Packet pw = new Packet(0x20);
            pw.WriteByte(0x0B);
            if (offline)
            {
                pw.WriteInt(victimid);
            }
            else
            {
                pw.WriteInt(chr.ID);
            }
            pw.WriteInt(party.ID);
            pw.WriteByte(disband); //disband ? 0 : 1
            pw.WriteByte(0);
            if (offline)
            {
                pw.WriteString(victimname);
            }
            else
            {
                pw.WriteString(chr.Name);
            }
            AddPartyData(pw);
            return pw.ToArray();
        }

        public static void PartyMessage(Character chr, byte Message)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x20);
            pw.WriteByte(Message); //08 : You have already joined a party. 10: The party you're trying to join is already full in capacity. 
            chr.sendPacket(pw);
        }

        public static void PartyPortal(Character chr, int fromid, int townid, Pos pos)
        { //not done yet. 
            Packet pw = new Packet();
            pw.WriteByte(0x20);
            pw.WriteByte(0x1A);
            pw.WriteInt(townid);
            pw.WriteInt(100000200);
            pw.WriteShort(pos.X);
            pw.WriteShort(pos.Y);
            chr.sendPacket(pw);
        }

        public static void SilentUpdate(Character chr)
        {
            //Lie detector test ?!
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0x06);
            pw.WriteInt(chr.ID); //? or party.ID? both works lol...
            AddPartyData(pw);
            chr.sendPacket(pw);
        }
        public static void PartyInvite(Character chr, Character pVictim, Packet packet, int id)
        {
            //Some fucking idk thing if opcode is 0x20
            //20 04 00 00 00 00 04 00 4D 65 67 61
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(4);
            pw.WriteInt(id);
            pw.WriteString(chr.Name);
            pVictim.sendPacket(pw);
        }

        public static byte[] JoinParty(Character target, MapleParty party)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x20);
            pw.WriteByte(0xE);
            pw.WriteInt(1);
            pw.WriteString(target.Name);
            //AddPartyData(pw, party);
            return pw.ToArray();
        }

        public static void CharacterPartyMessage(Character chr, string name, byte message)
        {
            //Message : 0x15, 0x14, 0x13
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(message);
            pw.WriteString(name);
            chr.sendPacket(pw);


        }

        public static byte[] ReceivePartyMemberHP(int curhp, int maxhp, int otherid)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x67);
            pw.WriteInt(otherid);
            pw.WriteInt(curhp);
            pw.WriteInt(maxhp);
            return pw.ToArray();
        }

        public static void SilentUpdateDoor(Character chr)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x20);
            pw.WriteByte(0x1A);
            pw.WriteInt(100000000);
            pw.WriteInt(chr.ID);
            pw.WriteShort(chr.Position.X);
            pw.WriteShort(chr.Position.Y);
            chr.sendPacket(pw);
        }



        public static void AddPartyData(Packet packet)
        {
            //222 bytes :S
            // Player IDs
            for (int i = 0; i < 2; i++)
            {
                packet.WriteInt(322+i);
            }
            for (int i = 2; i < 6; i++)
            {
                packet.WriteInt(0); //Amount
            }
            // Player names
            for (byte i = 0; i < 2; i++)
            {
                packet.WriteString("test", 13);
            }
            for (int i = 2; i < 6; i++)
            {
                packet.WriteString("", 13);
            }
            
            for (int i = 0; i < 2; i++)
            {
                packet.WriteInt(0);
            }

            for (int i = 2; i < 6; i++)
            {
                packet.WriteInt(-2);
            }

            packet.WriteInt(323);

            //no fucking clue 
            for (int i = 0; i < 2; i++)
            {
                packet.WriteInt(0);
            }
            for (int i = 2; i < 6; i++)
            {
                packet.WriteInt(-2);
            }
            


            // All portal data
            for (int i = 0; i < 2; i++)
            {
                
                
                    packet.WriteInt(2);
                    packet.WriteInt(0);
                    packet.WriteInt(0);
                    packet.WriteInt(0);
                
                //}
                //else
                //{
                //packet.WriteInt(999999999); //offline
                //packet.WriteInt(999999999);
                //packet.WriteInt(-1);
                //packet.WriteInt(-1);
                //}
            }

            

            //Not exactly sure what this part is for...
            for (int i = 2; i < 6; i++)
            {
                
                    packet.WriteInt(2);
                    packet.WriteInt(0);
                    packet.WriteInt(0);
                    packet.WriteInt(0);
                


            }

        }


        
        
        }
    }