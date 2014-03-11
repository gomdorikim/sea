using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Login
{

    public class PrimaryStats
    {
        public byte Level { get; set; }
        public short Job { get; set; }
        public short Str { get; set; }
        public short Dex { get; set; }
        public short Int { get; set; }
        public short Luk { get; set; }
        public short HP { get; set; }
        public short MaxHP { get; set; }
        public short MP { get; set; }
        public short MaxMP { get; set; }
        public short AP { get; set; }
        public short SP { get; set; }
        public int EXP { get; set; }
        public short Fame { get; set; }
    }

}