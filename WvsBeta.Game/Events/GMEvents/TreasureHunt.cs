using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events
{
    public class TreasureHunt : Event
    {
        public const int TMaxUsers = 1000;
        public const int TTime = 1000; //Don't know the correct time yet :S
        public int[] _Maps = { 0, 922010100 };

        public TreasureHunt() : base(EventType.FindTheJewel)
        {
            InitializeVariables();
        }

        protected override void InitializeVariables()
        {
            for (int i = 0; i < _Maps.Length; i++)
                base.Maps.Add(_Maps[i]);
                base.MaxUsers = TMaxUsers;
                base.Time = TTime;
        }
    }
}
