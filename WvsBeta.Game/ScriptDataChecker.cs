using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using WvsBeta.BinaryData;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class ScriptDataChecker
    {
        private static Dictionary<string, DateTime> _fileModificationDates = new Dictionary<string, DateTime>();

        public static string ScriptDir = @"C:\Users\Administrator\Dropbox\Source\DataSvr\Scripts";

        public static void GenerateNewScriptHashes()
        {
            _fileModificationDates.Clear();
            foreach (string filename in Directory.GetFiles(ScriptDir, "*.s").Union(Directory.GetFiles(ScriptDir, "*.cs")))
            {
                _fileModificationDates.Add(filename, File.GetLastWriteTime(filename));
            }
        }


        public static IEnumerable<string> GetFilesNeedingRecompiling()
        {
            foreach (string filename in Directory.GetFiles(ScriptDir, "*.s").Union(Directory.GetFiles(ScriptDir, "*.cs")))
            {
                if (!_fileModificationDates.ContainsKey(filename) || _fileModificationDates[filename] != File.GetLastWriteTime(ScriptDir + filename))
                {
                    yield return filename;
                }
            }
        }

    }
}
