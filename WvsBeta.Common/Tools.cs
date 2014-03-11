using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public class Tools
    {
        public static long GetTicks()
        {
            return DateTime.Now.ToFileTime();
        }

        public static long GetTicksWithAddition(TimeSpan span)
        {
            return DateTime.Now.Add(span).ToFileTime();
        }


        private static DateTime _lastUpdate = DateTime.Now;
        public static long GetTimeAsMilliseconds(DateTime pNow)
        {
            if ((pNow - _lastUpdate).TotalSeconds >= 1)
                _lastUpdate = pNow;
            return _lastUpdate.ToFileTime() / 10000;
        }

        
    }
}
