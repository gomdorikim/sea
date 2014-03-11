using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class Party
    {

        public List<Character> Members { get; private set; }
        public Character Leader { get { return Members[0]; } }
        /**
        public static Dictionary<int, Party> LocalParties = new Dictionary<int, Party>();

        public List<Character> Members { get; private set; }
        public Character Leader { get { return Members[0]; } }
        public int ID { get; protected set; }

        public Party()
        {
            ID = Server.Instance.PartyIDs.NextValue();
            LocalParties.Add(ID, this);
        }

        public Party(Character pLeader)
        {
            Party party = new Party();
            pLeader.PartyID = party.ID;
            party.Members = new List<Character>();
            party.Members.Add(pLeader);
            Party.InsertID(pLeader.ID, party);
        }

        public void Disband()
        {
            Members.Clear();
        }

        
        public void RemoveMember(Character pCharacter)
        {
            Members.Remove(pCharacter);
            PartyNull(pCharacter.ID);
            pCharacter.Party = null;
            pCharacter.PartyID = -1;
        }

        public void AddMember(CharacterBase pCharacter)
        {
            Members.Add(pCharacter);
            InsertID(pCharacter.ID, this);
            pCharacter.Party = this;
            pCharacter.PartyID = this.ID;
        }

        public static void InsertID(int pCharacterID, Party pParty) //This is needed because the server creates a new instance of your character when you log in
        {
            Server.Instance.CharacterDatabase.RunQuery("UPDATE characters SET party =  " + pParty.ID + " WHERE ID = " + pCharacterID + "");
        }

        public static void PartyNull(int pCharacterID)
        {
           Server.Instance.CharacterDatabase.RunQuery("UPDATE characters SET party = -1 WHERE ID = " + pCharacterID + "");
        }

        public bool IsLeader(Character pCharacter)
        {
            return pCharacter.ID == Leader.ID;
        }

        public void UpdatePartyMemberHP(Character pCharacter)
        {
            foreach (Character member in pCharacter.mParty.Members)
            {
                if (member != pCharacter && member != null)
                {
                    if (member.Map == pCharacter.Map) //TODO : channel check
                        member.sendPacket(PartyPacket.ReceivePartyMemberHP(pCharacter.PrimaryStats.HP, pCharacter.PrimaryStats.MaxHP, pCharacter.ID));
                }
            }
        }

        public void ReceivePartyMemberHP(Character pCharacter)
        {
            foreach (Character member in pCharacter.mParty.Members)
            {
                if (member.Map == pCharacter.Map && member != null) //TODO : channel check
                    pCharacter.sendPacket(PartyPacket.ReceivePartyMemberHP(member.PrimaryStats.HP, member.PrimaryStats.MaxHP, member.ID));
            }
        }
        **/

        public void SendPQSign(Character chr, bool clear)
        {
            string Sound;
            string Message;
            if (clear)
            {
                Sound = "Party1/Clear";
                Message = "quest/party/clear";
            }
            else
            {
                Sound = "Party1/Failed";
                Message = "quest/party/wrong_kor";
            }
            MapPacket.MapEffect(chr, 4, Sound, false);
            MapPacket.MapEffect(chr, 3, Message, false);
        }
    }     
    }

