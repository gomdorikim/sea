using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WvsBeta.BinaryData
{
    public class Constants
    {
        public static byte[] KEY = new byte[] { 0x13, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x37 };
    }

    public class WzBinaryReader : BinaryReader
    {
        public WzBinaryReader(Stream input) : base(input)
        {
        }
        public WzBinaryReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        public void DoCheck()
        {
            byte[] tmp = base.ReadBytes(Constants.KEY.Length);
            if (tmp[0] != Constants.KEY[0] || !tmp.SequenceEqual(tmp))
            {
                throw new Exception("Check Failed.");
            }
        }
    }

    public class WzBinaryWriter : BinaryWriter
    {
        public WzBinaryWriter() : base() { }

        public WzBinaryWriter(Stream input) : base(input)
        {
        }
        public WzBinaryWriter(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        public void AddCheck()
        {
            base.Write(Constants.KEY);
        }
    }
}
