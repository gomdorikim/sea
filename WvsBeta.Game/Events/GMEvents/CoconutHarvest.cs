using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events
{
    public enum CoconutOperation : byte
    {
        Spawn = 0x00,
        Hit = 0x01,
        Break = 0x02,
        Destroy = 0x03,
    }

    class CoconutHarvest : Event
    {
        public CoconutHarvest() : base(EventType.CoconutHarvest)
        {

        }
    }
}
