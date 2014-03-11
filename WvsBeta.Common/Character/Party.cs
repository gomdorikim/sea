using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public class MapleParty
    {
        public static LoopingID pid = new LoopingID();
        public static Dictionary<int, MapleParty> LocalParties = new Dictionary<int, MapleParty>();
        
        public List<CharacterBase> Members { get; private set; }
        public CharacterBase Leader { get { return Members[0]; } }
        
        public int ID { get; protected set; }

        public MapleParty()
        {
            ID = pid.NextValue();
            LocalParties.Add(ID, this);
        }

        public MapleParty(CharacterBase pLeader)
        {
            MapleParty party = new MapleParty();
            pLeader.PartyID = party.ID;
            party.Members = new List<CharacterBase>();
            party.Members.Add(pLeader);
            //Party.InsertID(pLeader.ID, party);
        }

        public MapleParty(int PartyID)
        {
            this.ID = PartyID;
        }

        public void Disband()
        {
            Members.Clear();
        }

        public void AddMember(CharacterBase pCharacter)
        {
            Members.Add(pCharacter);
            //InsertID(pCharacter.ID, this);
            pCharacter.Party = this;
            pCharacter.PartyID = this.ID;
        }

        public bool IsLeader(CharacterBase pCharacter)
        {
            if (pCharacter.ID == Leader.ID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
