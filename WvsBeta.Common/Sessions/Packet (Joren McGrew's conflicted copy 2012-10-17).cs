﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace WvsBeta.Common.Sessions
{
    public class Packet
    {
        private MemoryStream _memoryStream;
        private BinaryReader _binReader;
        private BinaryWriter _binWriter;

        public Packet(byte[] pData)
        {
            _memoryStream = new MemoryStream(pData);
            _binReader = new BinaryReader(_memoryStream);
        }

        public Packet()
        {
            _memoryStream = new MemoryStream();
            _binWriter = new BinaryWriter(_memoryStream);
        }

        public Packet(byte pOpcode)
        {
            _memoryStream = new MemoryStream();
            _binWriter = new BinaryWriter(_memoryStream);
            WriteByte(pOpcode);
        }

        public Packet(ServerMessages pMessage) : this((byte)pMessage) { }
        public Packet(ISClientMessages pMessage) : this((byte)pMessage) { }
        public Packet(ISServerMessages pMessage) : this((byte)pMessage) { }

        public byte[] ToArray()
        {
            return _memoryStream.ToArray();
        }

        public int Length
        {
            get { return (int)_memoryStream.Length; }
        }

        public int Position
        {
            get { return (int)_memoryStream.Position; }
            set { _memoryStream.Position = value; }
        }

        public void Reset(int pPosition = 0)
        {
            _memoryStream.Position = pPosition;
        }

        public void Skip(int pAmount)
        {
            if (pAmount + _memoryStream.Position > Length)
                throw new Exception("!!! Cannot skip more bytes than there's inside the buffer!");
            _memoryStream.Position += pAmount;
        }

        public byte[] ReadLeftoverBytes()
        {
            return ReadBytes(Length - (int)_memoryStream.Position);
        }

        public override string ToString()
        {
            string ret = "";
            foreach (byte b in ToArray())
            {
                ret += string.Format("{0:X2} ", b);
            }
            return ret;
        }

        public void WriteBytes(byte[] val) { _binWriter.Write(val); }
        public void WriteByte(byte val) { _binWriter.Write(val); }
        public void WriteSByte(sbyte val) { _binWriter.Write(val); }
        public void WriteBool(bool val) { WriteByte(val == true ? (byte)1 : (byte)0); }
        public void WriteShort(short val) { _binWriter.Write(val); }
        public void WriteInt(int val) { _binWriter.Write(val); }
        public void WriteLong(long val) { _binWriter.Write(val); }
        public void WriteUShort(ushort val) { _binWriter.Write(val); }
        public void WriteUInt(uint val) { _binWriter.Write(val); }
        public void WriteULong(ulong val) { _binWriter.Write(val); }
        public void WriteString(string val) { WriteShort((short)val.Length); _binWriter.Write(val.ToCharArray()); }
        public void WriteString(string val, int maxlen) { var i = 0; for (; i < val.Length & i < maxlen; i++) _binWriter.Write(val[i]); for (; i < maxlen; i++) WriteByte(0); }

        public void WriteHexString(string pInput)
        {
            pInput = pInput.Replace(" ", "");
            if (pInput.Length % 2 != 0) throw new Exception("Hex String is incorrect (size)");
            for (int i = 0; i < pInput.Length; i += 2)
            {
                WriteByte(byte.Parse(pInput.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));
            }

        }

        public byte[] ReadBytes(int pLen) { return _binReader.ReadBytes(pLen); }
        public bool ReadBool() { return _binReader.ReadByte() != 0; }
        public byte ReadByte() { return _binReader.ReadByte(); }
        public sbyte ReadSByte() { return _binReader.ReadSByte(); }
        public short ReadShort() { return _binReader.ReadInt16(); }
        public int ReadInt() { return _binReader.ReadInt32(); }
        public long ReadLong() { return _binReader.ReadInt64(); }
        public ushort ReadUShort() { return _binReader.ReadUInt16(); }
        public uint ReadUInt() { return _binReader.ReadUInt32(); }
        public ulong ReadULong() { return _binReader.ReadUInt64(); }
        public string ReadString(short pLen = -1) { short len = pLen == -1 ? _binReader.ReadInt16() : pLen; return new string(_binReader.ReadChars(len)); }

        public void SetBytes(int pPosition, byte[] val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetByte(int pPosition, byte val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetSByte(int pPosition, sbyte val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetBool(int pPosition, bool val) { Reset(pPosition); WriteByte(val == true ? (byte)1 : (byte)0); }
        public void SetShort(int pPosition, short val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetInt(int pPosition, int val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetLong(int pPosition, long val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetUShort(int pPosition, ushort val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetUInt(int pPosition, uint val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetULong(int pPosition, ulong val) { Reset(pPosition); _binWriter.Write(val); }
        public void SetString(int pPosition, string val) { Reset(pPosition); WriteShort((short)val.Length); _binWriter.Write(val.ToCharArray()); }
        public void SetString(int pPosition, string val, int maxlen) { Reset(pPosition); var i = 0; for (; i < val.Length & i < maxlen; i++) _binWriter.Write(val[i]); for (; i < maxlen; i++) WriteByte(0); }
    }
}
