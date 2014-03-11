using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events
{
    class OlaOla : Event
    {
        public static int[] sMap = { 109030401, 109030301, 109030201, 109030101, 109030001 }; //Because there is 5 different Ola Ola maps

        public OlaOla() :
            base(EventType.OlaOla)
        {

        }

        protected override void OnUpdate(DateTime pNow)
        {
            
        }

    }
}
