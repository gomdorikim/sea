using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class MapleParty
    {
        int ID { get; set; }
        public List<Character> Players { get; set; }
        public Dictionary<int, MapleParty> Parties { get; set; }
        public Character Leader { get; set; }

        public MapleParty(int partyID, Character chr)
        {
            ID = partyID;
            Leader = chr;
            Players = new List<Character>();
            Players.Add(Leader);
        }

        public MapleParty CreateParty(Character chr)
        {
            int mpID = Server.Instance.PartyIDs.NextValue();
            MapleParty party = new MapleParty(mpID, chr);
            Parties.Add(party.ID, party);
            return party;

        }
    }
}
