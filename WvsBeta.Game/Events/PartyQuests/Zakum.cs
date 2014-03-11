using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events
{
    class Zakum
    {
        public static int Countdown = 1800; //Not sure what the time is
        public static int CurrentTick { get; set; }
        
        public static void NewZakumPQ(/**Character chr**/)
        {
          
            /**
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                  "ZakumPQ",
                  (date) => { ZakumPQ(chr); },
                0, 1 * 1000));
            chr.UsingTimer = true;
             * **/
        }

        public Zakum(int[] Maps)
        {
            foreach (int i in Maps)
            {
                
            }
        }
        /**

        public static void ZakumPQ(Character chr)
        {
            Countdown = Countdown - 1;
            CurrentTick = Countdown;
            if (chr.mParty != null)
            {
                Party2 ZakumParty = Party2.LocalParties[chr.PartyID];
                foreach (Character pMember in ZakumParty.Members)
                {
                    if (pMember.UsingTimer == null || pMember.UsingTimer == false)
                    {
                        MapPacket.MapTimer(pMember, CurrentTick);
                        pMember.UsingTimer = true;
                    }
                }
            }
        }
         * */

        
    }
}
