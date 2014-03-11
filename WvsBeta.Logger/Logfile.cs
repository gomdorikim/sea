using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace WvsBeta.Common
{
    public class Logfile
    {
        public string Filename { get; private set; }
        private StreamWriter _writer = null;

        const string EXTENSION = "txt";

        public Logfile(string pLogname, bool pAddDate = true, string pFolder = "Logs")
        {
            if (pFolder == "Logs")
            {
                pFolder += Path.DirectorySeparatorChar + pLogname;
            }
            Directory.CreateDirectory(pFolder);

            if (pAddDate)
            {
                Filename = string.Format("{0} - {1}.{2}", pLogname, DateTime.Now.ToString("yyyy-MM-dd HHmmssfff"), EXTENSION);
            }
            else
            {
                Filename = string.Format("{0}.{1}", pLogname, EXTENSION);
            }
            Filename = pFolder + Path.DirectorySeparatorChar + Filename;
            _writer = new StreamWriter(File.Open(Filename, FileMode.Append, FileAccess.Write, FileShare.Read));
            _writer.AutoFlush = true;
        }

        private bool lastWasNewline = true;

        public void Write(string pFormat, params object[] pParams)
        {
            string txt = "";
            if (lastWasNewline)
            {
                txt = string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), string.Format(pFormat, pParams));
            }
            else
            {
                txt = string.Format(pFormat, pParams);
            }
            _writer.Write(txt);
            //File.AppendAllText(Filename, txt);
            lastWasNewline = false;
        }

        public void WriteLine(string pFormat = null, params object[] pParams)
        {
            if (pFormat == null) // Creates a newline
            {
                pFormat = "{0}";
                pParams = new object[] { Environment.NewLine };
            }

            string txt = "";
            if (lastWasNewline)
            {
                txt = string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), string.Format(pFormat, pParams));
            }
            else
            {
                txt = string.Format(pFormat, pParams);
            }
            _writer.Write(txt + Environment.NewLine);
            //File.AppendAllText(Filename, txt + Environment.NewLine);
            lastWasNewline = true;
        }
    }
}
