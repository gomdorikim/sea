using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Center
{
    public enum MessengerFunction : byte
    {
        SelfEnterResult = 0x00,
        Enter = 0x01,
        Leave = 0x02,
        Invite = 0x03,
        InviteResult = 0x04,
        Blocked = 0x05,
        Chat = 0x06,
        Avatar = 0x07,
        Migrated = 0x08,
    }

    public class Messenger
    {
        public static List<Messenger> Messengers = new List<Messenger>();

        public int ID { get; set; }
        public byte MaxUsers { get { return 3; } }
        public Dictionary<byte, Character> Users { get; set; }

        public Messenger(Character pOwner)
        {
            ID = pOwner.ID;
            Users = new Dictionary<byte, Character>();
            Users.Add(0, pOwner);
            if (Messengers.Contains(this))
            {
                Messengers.Remove(this);
                Messengers.Add(this);
            }
            else
            {
                Messengers.Add(this);
            }
        }

        public byte GetEmptySlot()
        {
            for (byte i = 0; i < MaxUsers; i++)
            {
                if (!Users.ContainsKey(i)) return i;
            }
            return 0xFF;
        }

        public static Messenger GetMessenger(int MessengerID)
        {
            foreach (Messenger m in Messengers)
            {
                if (m.ID == MessengerID) return m;
            }
            return null;
        }

        public void AddUser(Character pCharacter)
        {
            pCharacter.MessengerSlot = GetEmptySlot();
            Users.Add(pCharacter.MessengerSlot, pCharacter);

            foreach (KeyValuePair<byte, Character> kvp in Users)
            {
                LocalServer CharacterServer = CenterServer.Instance.Worlds[0].GameServers[kvp.Value.ChannelID];
                if (kvp.Value != pCharacter)
                {
                    CharacterServer.Connection.SendPacket(MessengerAddUser(pCharacter, kvp.Value.ID, (byte)(pCharacter.MessengerSlot), true));
                }
                else
                {
                    CharacterServer.Connection.SendPacket(MessengerAddUser(pCharacter, kvp.Value.ID, (byte)(pCharacter.MessengerSlot), true));
                }
            }
            ShowMessengerRoom(pCharacter);
        }

        public void ShowMessengerRoom(Character pCharacter)
        {
            foreach (KeyValuePair<byte, Character> kvp in Users)
            {
                LocalServer Character = CenterServer.Instance.Worlds[0].GameServers[pCharacter.ChannelID];
                Character.Connection.SendPacket(Messenger.ShowRoom(kvp.Value, pCharacter.ID));
            }
        }

        public void BroadcastPacket(Packet pPacket, byte WorldID)
        {
            foreach (KeyValuePair<byte, Character> kvp in Users)
            {
                LocalServer CharacterServer = CenterServer.Instance.Worlds[WorldID].GameServers[kvp.Value.ChannelID];
                CharacterServer.Connection.SendPacket(pPacket);
            }
        }

        public static Packet MessengerInvite(string From, Messenger m, int To)
        {
            Packet packet = new Packet(ISServerMessages.MessengerOperation);
            packet.WriteByte(0x03);
            packet.WriteInt(To);
            packet.WriteByte(0xB7);
            packet.WriteByte(0x03);
            packet.WriteString(From);
            packet.WriteByte(1);
            packet.WriteInt(m.ID);
            return packet;
        }

        public static Packet MessengerBlock(int CharID, string Who)
        {
            Packet packet = new Packet(ISServerMessages.MessengerOperation);
            packet.WriteByte(0x05);
            packet.WriteInt(CharID);
            packet.WriteByte(0xB7);
            packet.WriteByte(0x05);
            packet.WriteString(Who);
            packet.WriteByte(0);
            return packet;
        }

        public static Packet MessengerResponse(int CharID, string Who, byte Response)
        {
            Packet packet = new Packet(ISServerMessages.MessengerOperation);
            packet.WriteByte(0x05);
            packet.WriteInt(CharID);
            packet.WriteByte(0xB7);
            packet.WriteByte(0x04);
            packet.WriteString(Who);
            packet.WriteByte(Response);
            return packet;
        }

        public static Packet MessengerAddUser(Character pCharacter, int To, byte Slot, bool InChat)
        {
            Packet packet = new Packet(ISServerMessages.MessengerOperation);
            packet.WriteByte(0x00);
            packet.WriteInt(To);
            packet.WriteByte(0xB7);
            packet.WriteByte(0x07);
            packet.WriteByte(Slot);
            packet.WriteByte(pCharacter.Gender);
            packet.WriteByte(pCharacter.Skin);
            packet.WriteInt(pCharacter.Face);
            packet.WriteByte(0);
            packet.WriteInt(pCharacter.Hair);
            packet.WriteBytes(pCharacter.EquipData);
            packet.WriteString(pCharacter.Name);
            packet.WriteByte(pCharacter.ChannelID);
            packet.WriteBool(InChat); //Inchat bool
            return packet;
        }

        public static Packet ShowRoom(Character pCharacter, int To)
        {
            Packet packet = new Packet(ISServerMessages.MessengerOperation);
            packet.WriteByte(0x00);
            packet.WriteInt(To);
            packet.WriteByte(0xB7);
            packet.WriteByte(0x00);
            packet.WriteByte(pCharacter.MessengerSlot);
            packet.WriteByte(pCharacter.Gender);
            packet.WriteByte(pCharacter.Skin);
            packet.WriteInt(pCharacter.Face);
            packet.WriteByte(0);
            packet.WriteInt(pCharacter.Hair);
            packet.WriteBytes(pCharacter.EquipData);
            packet.WriteString(pCharacter.Name);
            packet.WriteByte(pCharacter.ChannelID);
            packet.WriteByte(0); //In chat bool
            return packet;
        }

        public static Packet ShowJoinMessage(Character pJoiner, int To)
        {
            Packet packet = new Packet(ISServerMessages.MessengerOperation);
            packet.WriteByte(0x01);
            packet.WriteInt(To);
            packet.WriteByte(0xB7);
            packet.WriteByte(0x01);
            packet.WriteByte(0); //To idk
            return packet;
        }
    }
}
