﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game {
    class GameAcceptor : Acceptor
    {
        public GameAcceptor()
            : base(Server.Instance.Port)
        {
        }

        public override void OnAccept(System.Net.Sockets.Socket pSocket)
        {
            new ClientSocket(pSocket);
        }
	}
}
