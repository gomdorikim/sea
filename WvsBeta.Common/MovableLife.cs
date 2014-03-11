using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Common
{
    public class Pos // FFF
    {
        public short X { get; set; }
        public short Y { get; set; }
        public Pos()
        {
            X = 0;
            Y = 0;
        }

        public Pos(Pos basePos)
        {
            X = basePos.X;
            Y = basePos.Y;
        }

        public Pos(short x, short y)
        {
            X = x;
            Y = y;
        }

        public Pos(Packet pr, bool isInt = false)
        {
            X = isInt ? (short)pr.ReadInt() : pr.ReadShort();
            Y = isInt ? (short)pr.ReadInt() : pr.ReadShort();
        }

        public static int operator -(Pos p1, Pos p2)
        {
            return (int)(Math.Sqrt(Math.Pow((float)(p1.X - p2.X), 2) + Math.Pow((float)(p1.Y - p2.Y), 2)));
        }
    }

    public class MovableLife
    {
        public byte Stance { get; set; }
        public short Foothold { get; set; }
        public Pos Position { get; set; }
        public Pos Wobble { get; set; }
        public byte Jumps { get; set; }

        public MovableLife()
        {
            Stance = 0;
            Foothold = 0;
            Position = new Pos();
            Wobble = new Pos(0, 0);
        }

        public MovableLife(short pFH, Pos pPosition, byte pStance)
        {
            Stance = pStance;
            Position = new Pos(pPosition);
            Foothold = pFH;
            Wobble = new Pos(0, 0);
        }

        public bool isFacingRight()
        {
            return Stance % 2 == 0;
        }
    }
}
