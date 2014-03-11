﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class Mist
    {
        public static LoopingID LoopingID = new LoopingID();
        public int LT_X { get; private set; }
        public int RB_X { get; private set; }
        public int LT_Y { get; private set; }
        public int RB_Y { get; private set; }
        public int SkillID { get; private set; }
        public byte SkillLevel { get; private set; }
        public int OwnerID { get; private set; }
        public int SpawnID { get; private set; }
        public long Time { get; private set; }
        public int MapID { get; private set; }
        public bool MobMist { get { return SkillID < 1000; } }


        public Mist(int pSkillID, byte pSkillLevel, int pMapID, int pOwnerID, int pDisplayTime, int pX1, int pY1, int pX2, int pY2)
        {
            SkillID = pSkillID;
            SkillLevel = pSkillLevel;
            MapID = pMapID;
            OwnerID = pOwnerID;
            LT_X = pX1;
            LT_Y = pY1;
            RB_X = pX2;
            RB_Y = pY2;
            Time = MasterThread.CurrentDate.AddSeconds(pDisplayTime).ToFileTime();
            SpawnID = LoopingID.NextValue();
        }
    }
}
