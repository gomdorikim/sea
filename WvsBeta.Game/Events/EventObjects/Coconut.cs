using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events
{
    class Coconut
    {
        public short ID { get; set; }
        public byte State { get; set; }

        public Coconut(short id, byte state)
        {
            ID = id;
            State = state;
        }
    }
}
