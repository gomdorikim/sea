using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class MessengerPacket
    {
        public static void HandleMessenger(Character pCharacter, Packet pPacket)
        {
            //MessagePacket.SendNotice(pPacket.ToString(), pCharacter);
            MessagePacket.SendText(MessagePacket.MessageTypes.PopupBox, "Messengers have been disabled for now.", pCharacter, MessagePacket.MessageMode.ToPlayer);
            byte Operation = pPacket.ReadByte();
            /**
            switch (Operation)
            {
                case 0x00: //New Messenger Base
                    {
                        int messengerID = pPacket.ReadInt();
                        Server.Instance.CenterConnection.PlayerMessengerOperation(pCharacter, 0, messengerID);
                        break;
                    }
                case 0x03:
                    {
                        Server.Instance.CenterConnection.PlayerMessengerOperation(pCharacter, 3, 0, pPacket.ReadString());
                        break;
                    }
                case 0x06:
                    {
                        //string Message = pPacket.ReadString();
                        break;
                    }
            }
             * **/
        }

      

        public static void JoinMessenger(Character pCharacter, int messengerID)
        {
            Packet pw = new Packet(0xB7);
            pw.WriteByte(0x01);
            if (messengerID == 0)
                pw.WriteByte(0);
            else
                pw.WriteByte(1);
            pCharacter.sendPacket(pw);
        }
    }
}
