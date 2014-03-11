using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class MessengerHandler
    {
        public static void HandleMessenger(Character chr, Packet packet)
        {
            MessagePacket.SendNotice("Messengers are not available right now.", chr);
            return;
          
            short header = packet.ReadByte();
            //int test = 1;
            //string input = packet.ReadString();

            switch (header)
            {
             
                
            }
        }

    }
}
 
