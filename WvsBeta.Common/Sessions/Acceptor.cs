using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace WvsBeta.Common.Sessions
{
    public abstract class Acceptor
    {
        public ushort Port { get; private set; }

        private TcpListener _listener;

        public Acceptor(ushort pPort)
        {
            _listener = new TcpListener(IPAddress.Any, pPort);
            _listener.Start();
            _listener.BeginAcceptSocket(EndAccept, null);
        }

        private void EndAccept(IAsyncResult pIAR)
        {
            OnAccept(_listener.EndAcceptSocket(pIAR));
            _listener.BeginAcceptSocket(EndAccept, null);
        }

        public abstract void OnAccept(Socket pSocket);
    }
}
