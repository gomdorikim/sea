using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public class FileWriter
    {
        private static Dictionary<string, Logfile> _logfiles = new Dictionary<string, Logfile>();

        public static void WriteLine(string pFilename, string pText, bool pNewline = true)
        {
            string folder = "Logs";
            if (pFilename.IndexOf('\\') != -1)
            {
                folder = pFilename.Substring(0, pFilename.LastIndexOf('\\'));
                pFilename = pFilename.Substring(pFilename.LastIndexOf('\\') + 1);
            }
            else if (pFilename.IndexOf('/') != -1)
            {
                folder = pFilename.Substring(0, pFilename.LastIndexOf('/'));
                pFilename = pFilename.Substring(pFilename.LastIndexOf('/') + 1);
            }

            if (!_logfiles.ContainsKey(pFilename))
            {
                _logfiles[pFilename] = new Logfile(pFilename, false, folder);
            }
            _logfiles[pFilename].WriteLine(pText);
        }
    }
}
