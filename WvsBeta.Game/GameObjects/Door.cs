using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game
{
    public class Door
    {
        //Mystic door
        public Character DoorOwner { get; set; }
        public int OriginalMap { get; set; }
        public int ToMap { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        public Door(Character owner, int oMap, int toMap, short x, short y)
        {
            DoorOwner = owner;
            OriginalMap = oMap;
            ToMap = toMap;
            X = x;
            Y = y;
        }
    }
}
