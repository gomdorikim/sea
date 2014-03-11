using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common.Sessions
{
    public class InterPacket : Packet
    {
        public const ushort OPCODE_USER_START = 0x1000;
        public const ushort OPCODE_CHARACTER_START = 0x2000;
        public const ushort OPCODE_SERVER_START = 0x3000;
        public const ushort OPCODE_ETC_START = 0x9000;

        public enum EnumOpcode : ushort
        {
            User_Login = OPCODE_USER_START,
            User_SelectWorld,
            User_SelectChannel,
            User_SelectCharacter,

            Character_Load = OPCODE_CHARACTER_START,
            Character_RequestParty,


            Server_RequestTransfer = OPCODE_SERVER_START,

            Etc_ForwardPacket = OPCODE_ETC_START,
        }

        public EnumOpcode Opcode { get; private set; }
        public string RequestKey { get; private set; }

        public InterPacket(EnumOpcode pOpcode, string pRequestKey) :
            base()
        {
            this.Opcode = pOpcode;
            WriteUShort((ushort)pOpcode);
            WriteString(pRequestKey);
        }

        public InterPacket(InterPacket pInPacket) :
            this(pInPacket.Opcode, pInPacket.RequestKey)
        {
        }

        public void LoadFromPacket()
        {
            this.Opcode = (EnumOpcode)ReadUShort();
            this.RequestKey = ReadString();
        }
    }
}
