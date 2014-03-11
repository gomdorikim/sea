//Author : Vanlj95

//HOLY SHIT THIS IS BAD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using WvsBeta.Game;
using WvsBeta.Common;
using System.Diagnostics;


namespace WvsBeta.Game.Events
{
    /// <summary>
    /// A manager for the Third job advancement
    /// </summary>
    class ThirdJob
    {

        public static Stopwatch stopwatch = new Stopwatch();
        public static void StartTimer(Character chr, int id)
        {
            switch (id)
            {
                case 1: //Warrior
                    DataProvider.Maps[108010301].KillAllMobs(chr, false);
                    DataProvider.Maps[108010301].ClearDrops();
                    DataProvider.Maps[108010301].spawnMobNoRespawn(9001000, new Pos(500, -500), 0);
                    break;
                case 2: //Mage
                    DataProvider.Maps[108010201].KillAllMobs(chr, false);
                    DataProvider.Maps[108010201].ClearDrops();
                    DataProvider.Maps[108010201].spawnMobNoRespawn(9001001, new Pos(500, -500), 0);
                    break;
                case 3: //Archer
                    DataProvider.Maps[108010101].KillAllMobs(chr, false);
                    DataProvider.Maps[108010101].ClearDrops();
                    DataProvider.Maps[108010101].spawnMobNoRespawn(9001002, new Pos(500, -500), 0);
                    break;
                case 4: //Thief
                    DataProvider.Maps[108010401].KillAllMobs(chr, false);
                    DataProvider.Maps[108010401].ClearDrops();
                    DataProvider.Maps[108010401].spawnMobNoRespawn(9001003, new Pos(500, -500), 0);
                    break;
            }
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Third Job Timer",
                (date) => { ThrowOut(chr); },
                1800 * 1000, 0));
            MapPacket.MapTimer(chr, 1800); //30 Minutes? o-o
            stopwatch.Start();
        }
        public static void ThrowOut(Character chr)
        {
            stopwatch.Stop();
            int warriormap = 108010301;
            int magemap = 108010201;
            int archermap = 108010101;
            int thiefmap = 108010401;
            if (DataProvider.Maps[chr.Map].ID == warriormap)
            {
                chr.ChangeMap(DataProvider.Maps[warriormap].ReturnMap);
            }
            else if (DataProvider.Maps[chr.Map].ID == magemap)
            {
                chr.ChangeMap(DataProvider.Maps[magemap].ReturnMap);
            }
            else if (DataProvider.Maps[chr.Map].ID == archermap)
            {
                chr.ChangeMap(DataProvider.Maps[archermap].ReturnMap);
            }
            else if (DataProvider.Maps[chr.Map].ID == thiefmap)
            {
                chr.ChangeMap(DataProvider.Maps[thiefmap].ReturnMap);
            }
        }
        public static void SetStage(int id, int charid)
        {
            Server.Instance.CharacterDatabase.RunQuery("INSERT INTO character_variables (charid, key, value) VALUES (" + charid + ", 1, " + id + ")");
        }
        public static int Stage(Character chr)
        {
            return 1;
        }

    }
}




