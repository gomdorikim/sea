/*
 * Author: Rice
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class CUIMessengerPacket
    {
        public static void OnPacket(Character chr, Packet packet)
        {
            byte function = packet.ReadByte();
            switch (function)
            {
                case 0x00:
                    int messengerid = packet.ReadInt();
                    Server.Instance.CenterConnection.MessengerJoin(messengerid, chr.ID, chr.Name, chr.Gender, chr.Skin, chr.Face, chr.Hair, chr.Inventory.GetVisibleEquips());
                    break;
                case 0x02:
                    Server.Instance.CenterConnection.MessengerLeave(chr.ID);
                    break;
                case 0x03:
                    string cinvitee = packet.ReadString();
                    Server.Instance.CenterConnection.MessengerInvite(chr.ID, cinvitee);
                    break;
                case 0x05:
                    string inviter = packet.ReadString();
                    string invitee = packet.ReadString();
                    byte blockmode = packet.ReadByte(); //0 = manual, 1 = automatic (game settings)
                    Server.Instance.CenterConnection.MessengerBlock(chr.ID, invitee, inviter, blockmode);
                    break;
                case 0x06:
                    string chatmsg = packet.ReadString();
                    Server.Instance.CenterConnection.MessengerChat(chr.ID, chatmsg);
                    break;
            }
        }
    }
}
