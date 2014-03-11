using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WvsBeta.Common.Sessions;

namespace WvsBeta.Center
{
    public class Character : Common.CharacterBase
    {
        public byte WorldID { get; set; }
        public byte ChannelID { get; set; }
        public int bChannelID { get; set; }
        public bool isCCing { get; set; }
        public bool isConnectingFromLogin { get; set; } 
        public bool MovingToCashShop { get; set; }
        public byte LastChannel { get; set; }
       
        public int BuddyListCapacity { get; set; }
        public BuddyList FriendsList { get; set; }
        public bool IsOnline { get; set; }

        public Messenger Messenger { get; set; }
        public byte MessengerSlot { get; set; }

        public Dictionary<byte, int> Equips { get; set; }
        public byte[] EquipData { get; set; }

        public void SendPacket(Packet pPacket)
        {
            if (!IsOnline) return;
            Packet toserver = new Packet(ISServerMessages.PlayerSendPacket);
            toserver.WriteInt(base.ID);
            toserver.WriteInt(pPacket.Length);
            toserver.WriteBytes(pPacket.ReadLeftoverBytes());

            CenterServer.Instance.SendPacketToServer(toserver, WorldID, ChannelID);
        }
    }
}
