using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Shop
{
    class ShopAcceptor : Acceptor
    {
        public ShopAcceptor() : base(Server.Instance.Port)
        {
        }

        public override void OnAccept(System.Net.Sockets.Socket pSocket)
        {
            ClientSocket connection = new ClientSocket(pSocket);
            connection.Init();
        }
    }
}
