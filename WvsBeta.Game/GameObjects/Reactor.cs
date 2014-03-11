using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game
{
    public class Reactor
    {
        public int ID { get; set; }
        public int ReactorID { get; set; }
        public byte State { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public int DropID { get; set; }
        public int MobID { get; set; }
        public int MapID { get; set; }
        public int ReactorTime { get; set; }
        public DateTime DestroyTime { get; set; }
        public bool Destroyed { get; set; }

        public Reactor()
        {
            this.ID = 0;
            this.ReactorID = 0;
            this.State = 0;
            this.X = 0;
            this.Y = 0;
            this.DropID = 0;
            this.MobID = 0;
            this.ReactorTime = 0;
            this.Destroyed = false;
        }
    }
}
