using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public class LoopingID
    {
        private int Current { get; set; }
        private int Minimum { get; set; }
        private int Maximum { get; set; }

        public LoopingID()
        {
            Minimum = Current = 0;
            Maximum = int.MaxValue;
        }

        public LoopingID(int min, int max)
        {
            Minimum = Current = min;
            Maximum = max;
        }

        public int NextValue()
        {
            int ret = Current;
            if (Current == Maximum)
            {
                Reset();
            }
            else
            {
                Current++;
            }
            return ret;
        }

        public void Reset()
        {
            Current = Minimum;
        }
    }
}
