using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public class CharacterBase : MovableLife
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public short Job { get; set; }
        public byte Level { get; set; }

        public byte Gender { get; set; }
        public byte Skin { get; set; }
        public int Face { get; set; }
        public int Hair { get; set; }

        public int MapID { get; set; }

        public int PartyID { get; set; }
        public bool Leader { get; set; }
        public MapleParty Party { get; set; }

        public bool IsConnected { get; set; }
    }
}
