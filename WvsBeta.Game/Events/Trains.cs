/**

**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events
{
    /// <summary>
    /// A manager for boat related events between Ludibrium and Ossyira (orbis)
    /// </summary>
    
    class Trains
    {
        //Constants
        /// <summary>
        /// Train1 = To Ludibrium
        /// Train2 = To Ossyria
        /// </summary>

        public const int Train1_Time_Prepare = 5; //time of accepting boarding passes
        public const int Train1_Time_Onboard = 8; //time before leaving
        public const int Train1_Time_Land = 30; //total time

        public const int Train2_Time_Prepare = 35; //time of accepting boarding passes
        public const int Train2_Time_Onboard = 38; //time before leaving
        public const int Train2_Time_Land = 59; //total time

        //To Ludibrium
        public const int Orbis_MainStation = 200000100;
        public const int Orbis_Station = 200000121;
        public const int Orbis_Prepare = 200000122;
        public const int Orbis_Onboard = 200090110;
        
    }
}
