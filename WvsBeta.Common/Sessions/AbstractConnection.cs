using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common.Sessions
{
    public abstract class AbstractConnection : Session
    {
        public bool gotPong = true;
        public const byte HEADER_PING = 0x0B, HEADER_PONG = 0x0B;

        public AbstractConnection(System.Net.Sockets.Socket pSocket)
            : base(pSocket, "")
        {

        }

        public AbstractConnection(string pIP, ushort pPort)
            : base(pIP, pPort, "")
        {

        }

        public override void OnPacketInbound(Packet pPacket)
        {
            if (pPacket.Length == 0)
                return;

            byte header = pPacket.ReadByte();

            if (header == HEADER_PING)
            {
                //SendPong();
                gotPong = true;
            }
            pPacket.Reset(0);

            AC_OnPacketInbound(pPacket);
        }

        public abstract void AC_OnPacketInbound(Packet pPacket);


        private static Packet _pingPacket = new Packet(HEADER_PING);
        private static Packet _pongPacket = new Packet(HEADER_PONG);

        public void SendPing()
        {
            SendPacket(_pingPacket);
        }

        public void SendPong()
        {
            SendPacket(_pongPacket);
        }
    }
}