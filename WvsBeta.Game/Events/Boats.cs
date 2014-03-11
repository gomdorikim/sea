/*
 * Author: Rice
 * Editor: vanlj95 (100% GMS-Like)
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WvsBeta.Common;

namespace WvsBeta.Game.Events
{
    /// <summary>
    /// A manager for boat related events between Victoria Island (Ellinia) and Ossyria (Orbis)
    /// </summary>
    public class Boat
    {
        //Constants
        public const int Boat_Time_Prepare = 5; //time of accepting boarding passes
        public const int Boat_Time_Onboard = 8; //time before leaving
        public const int Boat_Time_BalrogStart = 14; //start spawning balrog time
        public const int Boat_Time_BalrogStop = 25; //stop spawning balrog time
        public const int Boat_Time_Land = 30; //total time

        //To Ellinia
        public const int Orbis_MainStation = 200000100;
        public const int Orbis_Station = 200000111;
        public const int Orbis_Prepare = 200000112;
        public const int Orbis_Onboard = 200090000;
        public const int Orbis_Cabin = 200090001;

        //To Orbis
        public const int Ellinia_Station = 101000300;
        public const int Ellinia_Prepare = 101000301;
        public const int Ellinia_Onboard = 200090010;
        public const int Ellinia_Cabin = 200090011;

        public const int Orbis_Ticket = 4031045;//To Orbis
        public const int Ellinia_Ticket = 4031047;//To Ellinia

        public static byte boatDirection = 0; //0 = Not Set, 1 = Ellinia, 2 = Orbis
        public static bool announcedPrepare = false;
        public static bool announcedLeave = false;
        public static bool spawnedBalrog = false;

        public static void Initialize()
        {
            boatDirection = 2; //To Orbis

            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Boat Elinia-Orbis",
                EventWatcher,
                0, 1 * 1000));
        }

        /// <summary>
        /// Recursive thread to execute the event of a boat.
        /// On-user-enter events are not handled in here, such as boat spawn and boat depart packets. They are handled in their respective map instances.
        /// </summary>
        public static void EventWatcher(DateTime now)
        {
            int min = now.Minute;


            if (min == Boat_Time_Prepare - 5 && announcedPrepare == false)
            {
                
                foreach (Character ch in Server.Instance.CharacterList.Values)
                    if (ch.Map >= 100000000 && ch.Map <= 199999999 && boatDirection == 2)

                        MapPacket.SendBoat(ch, 2);
                announcedPrepare = true;

            }
            if (min == Boat_Time_Prepare + 25 && announcedPrepare == false)
            {
                
                foreach (Character ch in Server.Instance.CharacterList.Values)

                    if (ch.Map == 200000111 && boatDirection == 1)
                        MapPacket.SendBoat(ch, 2);
                announcedPrepare = true;

            }
            if (min >= Boat_Time_Onboard && min <= Boat_Time_Land && announcedLeave == false)
            {
                announcedLeave = true;
                foreach (Character ch in Server.Instance.CharacterList.Values)
                    if (boatDirection == 2 && ch.Map == Ellinia_Prepare)
                        ch.ChangeMap(Ellinia_Onboard);
                    else if (boatDirection == 2 && ch.Map == Ellinia_Station && announcedLeave == false)
                        MapPacket.SendBoat(ch, 3);


            }
            if (min >= 38 && min <= 40 && announcedLeave == false) //to orbis
            {
                announcedLeave = true;
                foreach (Character ch in Server.Instance.CharacterList.Values)
                    if (ch.Map == Orbis_Prepare)
                        ch.ChangeMap(Orbis_Onboard);
                    else if (ch.Map == Orbis_Station && announcedLeave == false)
                        MapPacket.SendBoat(ch, 3);

            }
            if (min >= Boat_Time_BalrogStart && min < Boat_Time_BalrogStop)
            {
                if (spawnedBalrog == false)
                {
                    if (boatDirection == 2)
                    {
                        DataProvider.Maps[Ellinia_Onboard].spawnMob(8150000, new Pos(500, -500), 0, null, 1);
                        DataProvider.Maps[Ellinia_Onboard].spawnMob(8150000, new Pos(500, -500), 0, null, 1);
                        foreach (Character ch in Server.Instance.CharacterList.Values)
                            if (ch.Map == Ellinia_Onboard && boatDirection == 2)
                                MapPacket.SendBoat(ch, 4);
                    }

                    spawnedBalrog = true;
                }
            }
            if (min >= 44 && min < 50) //to orbis
            {

                if (spawnedBalrog == false)
                {
                    if (boatDirection == 1)
                    {
                        DataProvider.Maps[Orbis_Onboard].spawnMob(8150000, new Pos(500, -500), 0, null, 1);
                        DataProvider.Maps[Orbis_Onboard].spawnMob(8150000, new Pos(500, -500), 0, null, 1);
                        spawnedBalrog = true;
                        foreach (Character ch in Server.Instance.CharacterList.Values)
                            if (ch.Map == Orbis_Onboard)
                                MapPacket.SendBoat(ch, 4);//whatever lol

                    }
                    spawnedBalrog = true;

                }
            }
            if (min >= Boat_Time_BalrogStop && min <= Boat_Time_Land)
            {
                if (DataProvider.Maps[Ellinia_Onboard].Mobs.Count > 0)
                    DataProvider.Maps[Ellinia_Onboard].KillAllMobs(Ellinia_Onboard, false);

            }
            if (min >= 50 && min <= 59)
            {
                if (DataProvider.Maps[Orbis_Onboard].Mobs.Count > 0)
                    DataProvider.Maps[Orbis_Onboard].KillAllMobs(Orbis_Onboard, false);
            }
            if (min >= 59)
            {
                foreach (Character ch in Server.Instance.CharacterList.Values)
                    if (ch.Map == Orbis_Onboard || ch.Map == Orbis_Cabin)
                        ch.ChangeMap(Ellinia_Station);

                if (announcedPrepare)
                    foreach (Character ch in Server.Instance.CharacterList.Values)
                        if (ch.Map == Ellinia_Station)
                            MapPacket.SendBoat(ch, 2);
                announcedPrepare = false;
                announcedLeave = false;
                spawnedBalrog = false;
            }
            if (min >= Boat_Time_Land)
            {
                foreach (Character ch in Server.Instance.CharacterList.Values)
                    if (boatDirection == 2 && ch.Map == Ellinia_Onboard || ch.Map == Ellinia_Cabin)
                        ch.ChangeMap(Orbis_MainStation);

                if (announcedPrepare)
                    if (boatDirection == 2)
                    {
                        foreach (Character ch in Server.Instance.CharacterList.Values)
                            if (boatDirection == 2 && ch.Map == Orbis_Station)
                                MapPacket.SendBoat(ch, 2);
                        boatDirection = 1;
                    }

                announcedPrepare = false;
                announcedLeave = false;
                spawnedBalrog = false;
            }
        }
    }
}