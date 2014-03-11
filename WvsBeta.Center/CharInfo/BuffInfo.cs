using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Center
{
    public class BuffInfo
    {
        public Dictionary<int, BuffInfo> Buffs { get; set; }
        public int ID { get; set; }
        public DateTime TimeLeft { get; set; }

        public BuffInfo(int id, DateTime time)
        {
            ID = id;
            TimeLeft = time;
        }
    }
}
