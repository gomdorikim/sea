using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public class Utils
    {
        static string pRemove = ".img";
        public static long ConvertNameToID(string pName)
        {
            if (pName.EndsWith(pRemove))
            {
                pName = pName.Substring(0, pName.LastIndexOf(pRemove));
            }
            return Convert.ToInt64(pName);
        }

        public static bool ReadBool(byte input) { return input == 0; }
        public static bool ReadBool2(byte input) { return input == 1; }
    }
}
