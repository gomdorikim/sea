/**
 * Author : Bibs001
 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events
{
    /// <summary>
    /// A manager for Party Quest related events such as Kerning City and Ludibrium
    /// </summary>

    public class KerningCity
    {
        public static int mExit = 103000890;
        public static int mBonus = 103000805; //Does this have a seperate timer? 
        public static int Countdown = 1800;
        public static int CurrentTick { get; set; }

        public bool Used { get; set; }
        public int Leader { get; set; }

        

        public static void ResetPortals()
        {
            foreach (KeyValuePair<int, Map> kvp in DataProvider.Maps)
            {
                if (kvp.Key >= 103000800 && kvp.Key <= 103000805)
                {
                    kvp.Value.PQPortalOpen = false; 
                }
            }
        }

        public static void OpenPortal(int MapID)
        {
            Map map = DataProvider.Maps[MapID];
            map.PQPortalOpen = true;
            MapPacket.PortalEffect(MapID, 2, "gate");
        }

        public static void ClosePortal(int MapID)
        {
            Map map = DataProvider.Maps[MapID];
            map.PQPortalOpen = false;
        }
        
        public static void StopPQ(DateTime Date)
        {
            MasterThread.Instance.RemoveRepeatingAction("PQWatcher", (date, name, removed) => { /*MasterThread.Instance._performanceLog.WriteLine("RemoveRepeatingAction Callback: Date: {0}; Name: {1}; Removed: {2}", date, name, removed);*/ });
        }

    }
}
