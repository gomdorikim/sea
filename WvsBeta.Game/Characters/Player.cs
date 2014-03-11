using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game
{
    public class Player
    {

        public string SessionHash { get; set; }

        public Character Character { get; set; }
        public ClientSocket Socket { get; set; }
        public bool SaveOnDisconnect { get; set; }

        public Player() { SaveOnDisconnect = true; }


    }
}
