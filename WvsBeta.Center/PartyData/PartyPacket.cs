using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Center
{
    public class PartyPacket
    {
        public static byte[] PartyResult(Character chr, byte Message)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(Message); //08 : You have already joined a party. 10: The party you're trying to join is already full in capacity. 
            return pw.ToArray();
        }


        public static byte[] LeaveParty(Character chr, MapleParty party)
        {
            //20 0B 00 00 00 00 00 00 00 00 00
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0xB); //leave
            pw.WriteInt(party.ID);
            pw.WriteInt(chr.ID);
            pw.WriteByte(1);
            pw.WriteByte(0);
            pw.WriteString(chr.Name);
            AddPartyData(pw, party);
            return pw.ToArray();

        }


        public static byte[] Disbanded(Character chr, MapleParty party) //it works, but i'm 99% sure the packet is wrong :P
        {
            //You have left since the party leader quit
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0xB);
            pw.WriteInt(chr.ID);
            pw.WriteInt(party.ID);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteString(chr.Name);
            return pw.ToArray();

        }

        public static byte[] DisbandParty(Character chr, int PartyID)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0xB); //leave
            pw.WriteInt(PartyID);
            pw.WriteInt(chr.ID);
            pw.WriteByte(0); //disband
            return pw.ToArray();
        }

        public static byte[] Expelled(Character chr, MapleParty party)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0xB); //leave
            pw.WriteInt(party.ID);
            pw.WriteInt(chr.ID);
            pw.WriteByte(1); //not disbanded
            pw.WriteByte(1); //expelled
            pw.WriteString(chr.Name);
            AddPartyData(pw, party);
            return pw.ToArray();
        }

        public static byte[] Update(bool offline, Character chr, MapleParty party, byte disband, byte expelled, string victimname, int victimid)
        {
            Packet pw = new Packet(0x27);
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
            pw.WriteByte(expelled);
            if (offline)
            {
                pw.WriteString(victimname);
            }
            else
            {
                pw.WriteString(chr.Name);
            }
            AddPartyData(pw, party);
            return pw.ToArray();
        }

        public static void PartyMessage(Character chr, byte Message)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(Message); //08 : You have already joined a party. 10: The party you're trying to join is already full in capacity. 
            // chr.sendPacket(pw);
        }

        public static void PartyPortal(Character chr, int fromid, int townid, Pos pos)
        { //not done yet. 
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0x1A);
            pw.WriteInt(townid);
            pw.WriteInt(100000200);
            pw.WriteShort(pos.X);
            pw.WriteShort(pos.Y);
            //chr.sendPacket(pw);
        }

        public static byte[] SilentUpdate(Character chr, MapleParty party)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0x06);
            pw.WriteInt(chr.ID); //? or party.ID? both works lol...
            AddPartyData(pw, party);
            return pw.ToArray();
        }
        public static void PartyInvite(Character chr, Character pVictim, Packet packet, int id)
        {
            //20 04 00 00 00 00 04 00 4D 65 67 61
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(4);
            pw.WriteInt(id);
            pw.WriteString(chr.Name);
            //pVictim.sendPacket(pw);
        }

        public static byte[] JoinParty(Character target, MapleParty party)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0xE);
            pw.WriteInt(1);
            pw.WriteString(target.Name);
            AddPartyData(pw, party);
            return pw.ToArray();
        }

        public static byte[] CharacterPartyMessage(Character chr, string name, byte message)
        {
            //Message : 0x15, 0x14, 0x13
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(message);
            pw.WriteString(name);
            return pw.ToArray();


        }

        public static byte[] ReceivePartyMemberHP(int curhp, int maxhp, int otherid)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x66);
            pw.WriteInt(otherid);
            pw.WriteInt(curhp);
            pw.WriteInt(maxhp);
            return pw.ToArray();
        }

        public static void SilentUpdateDoor(Character chr)
        {
            Packet pw = new Packet();
            pw.WriteByte(0x27);
            pw.WriteByte(0x1A);
            pw.WriteInt(100000000);
            pw.WriteInt(chr.ID);
            pw.WriteShort(chr.Position.X);
            pw.WriteShort(chr.Position.Y);
            // chr.sendPacket(pw);
        }



        public static void AddPartyData(Packet packet, MapleParty party)
        {

            // Player IDs
            for (int i = 0; i < party.Members.Count; i++)
            {
                packet.WriteInt(party.Members[i].ID);
            }
            for (int i = party.Members.Count; i < 6; i++)
            {
                packet.WriteInt(0); //Amount
            }
            // Player names
            for (byte i = 0; i < party.Members.Count; i++)
            {
                packet.WriteString(party.Members[i].Name, 13);
            }
            for (int i = party.Members.Count; i < 6; i++)
            {
                packet.WriteString("", 13);
            }

            //Maps 
            foreach (Character pCharacter in party.Members)
            {
                packet.WriteInt(pCharacter.MapID);
            }

            for (int i = party.Members.Count; i < 6; i++)
            {
                packet.WriteInt(-2);
            }

            packet.WriteInt(party.Leader.ID);

            //no fucking clue 
            for (int i = 0; i < party.Members.Count; i++)
            {
                packet.WriteInt(0);
            }
            for (int i = party.Members.Count; i < 6; i++)
            {
                packet.WriteInt(-2);
            }



            // All portal data
            for (int i = 0; i < party.Members.Count; i++)
            {

                foreach (Character chr in party.Members)
                {
                    packet.WriteInt(chr.MapID);
                    packet.WriteInt(0);
                    packet.WriteInt(0);
                    packet.WriteInt(0);
                }
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
            for (int i = party.Members.Count; i < 6; i++)
            {
                foreach (Character chr in party.Members)
                {
                    packet.WriteInt(chr.MapID);
                    packet.WriteInt(0);
                    packet.WriteInt(0);
                    packet.WriteInt(0);
                }


            }

        }
    }
}
