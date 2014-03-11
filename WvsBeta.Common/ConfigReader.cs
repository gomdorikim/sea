using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace WvsBeta.Common
{
    public class ConfigReader
    {
        public string Filename { get; private set; }

        private List<string> ConfigText = new List<string>();

        public ConfigReader(string pFilename)
        {
            Filename = pFilename;
            if (!System.IO.File.Exists(Filename))
            {
                throw new FileNotFoundException("Cannot find config file '" + Filename + "'");
            }

            using (StreamReader Reader = new StreamReader(Filename, true))
            {
                string line = "";
                while (!Reader.EndOfStream)
                {
                    line = Reader.ReadLine().Replace("\t", ""); // Remove tabs
                    line = line.Trim();
                    ConfigText.Add(line);
                }
            }
        }

        private string getValue(string sBlock, string sParameter)
        {
            bool startPart = false;
            string ans = "";
            int Line = 0;
            foreach (string line in ConfigText)
            {
                Line++;
                if (sBlock != "" && !startPart && line == sBlock + " = {")
                {
                    // Found beginning of block
                    startPart = true;
                }
                else if (startPart && line == "}")
                {
                    // Found end of block while begin found already
                    ans = "";
                    break;
                    //throw new InvalidOperationException("Parameter '" + sParameter + "' not found in block '" + sBlock + "'. (line: " + Line.ToString() + ")");
                }
                else if (line.StartsWith(sParameter + " = "))
                {
                    if (sBlock == "")
                    {
                        ans = line.Replace(sParameter + " = ", "");
                        break;
                    }
                    else if (sBlock != "" && startPart)
                    {
                        ans = line.Replace(sParameter + " = ", "");
                        break;
                    }
                }
            }
            return ans.Trim();
        }

        public List<string> getBlocks(string sMainBlock, bool skipBlocksInsideBlock)
        {
            List<string> ret = new List<string>();
            int block = 0; // Start out of a block
            int Line = 0;
            foreach (string line in ConfigText)
            {
                Line++;
                if (block == 0 && line == sMainBlock + " = {")
                {
                    block = 1;
                }
                else if (block == 1 && line == "}")
                {
                    block = 0;
                    break;
                }
                else
                {
                    if (block >= 1)
                    {
                        if (line.Contains(" = {"))
                        {
                            // Another block found
                            block++;
                            ret.Add(line.Replace(" = {", ""));
                        }
                        else if (line == "}")
                        {
                            // Block end found
                            block--;
                        }
                    }
                }
            }
            return ret;
        }

        public List<string> getBlocksFromBlock(string sMainBlock, int innerBlock)
        {
            List<string> ret = new List<string>();
            int block = sMainBlock == "" ? 1 : 0;
            int skipBlock = 0;
            int Line = 0;
            foreach (string line in ConfigText)
            {
                Line++;
                if (block == 0 && line == sMainBlock + " = {")
                {
                    block = 1;
                }
                else if (block == 1 && line == "}")
                {
                    block = 0;
                    break;
                }
                else
                {
                    if (block >= 1)
                    {
                        if (line.Contains(" = {"))
                        {
                            // Another block found
                            if (block <= innerBlock)
                            {
                                block++;
                                ret.Add(line.Replace(" = {", ""));
                            }
                            else
                            {
                                skipBlock++; // For skipping the '}' 's
                            }
                        }
                        else if (line == "}")
                        {
                            // Block end found
                            if (skipBlock == 0)
                                block--;
                            else
                                skipBlock--;
                        }
                    }
                }
            }
            return ret;
        }

        public string getString(string sBlock, string sParameter)
        {
            return getValue(sBlock, sParameter);
        }

        public int getInt(string sBlock, string sParameter)
        {
            string val = getValue(sBlock, sParameter);
            int retval = 0;
            int.TryParse(val, out retval);
            return retval;
        }

        public uint getUInt(string sBlock, string sParameter)
        {
            string val = getValue(sBlock, sParameter);
            uint retval = 0;
            uint.TryParse(val, out retval);
            return retval;
        }

        public short getShort(string sBlock, string sParameter)
        {
            string val = getValue(sBlock, sParameter);
            short retval = 0;
            short.TryParse(val, out retval);
            return retval;
        }

        public ushort getUShort(string sBlock, string sParameter)
        {
            string val = getValue(sBlock, sParameter);
            ushort retval = 0;
            ushort.TryParse(val, out retval);
            return retval;
        }

        public byte getByte(string sBlock, string sParameter)
        {
            string val = getValue(sBlock, sParameter);
            byte retval = 0;
            byte.TryParse(val, out retval);
            return retval;
        }

        public bool getBool(string sBlock, string sParameter)
        {
            string val = getValue(sBlock, sParameter);
            bool retval = false;
            bool.TryParse(val, out retval);
            return retval;
        }
    }
}