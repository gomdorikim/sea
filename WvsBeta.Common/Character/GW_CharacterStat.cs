using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WvsBeta.Common.Sessions;

namespace WvsBeta.Common.Character
{
    public class GW_CharacterStat
    {
        public int ID { get; protected set; }
        public string Name { get; protected set; }

        public byte Gender { get; protected set; }
        public byte Skin { get; protected set; }
        public int Face { get; protected set; }
        public int Hair { get; protected set; }

        public long PetCashId { get; protected set; }

        public byte Level { get; protected set; }
        public short Job { get; protected set; }
        public short Str { get; protected set; }
        public short Dex { get; protected set; }
        public short Int { get; protected set; }
        public short Luk { get; protected set; }
        public short HP { get; protected set; }
        public short MaxHP { get; protected set; }
        public short MP { get; protected set; }
        public short MaxMP { get; protected set; }
        public short AP { get; protected set; }
        public short SP { get; protected set; }
        public int EXP { get; protected set; }
        public short Fame { get; protected set; }

        public int MapID { get; protected set; }
        public byte MapPosition { get; protected set; }

        public int Money { get; protected set; }

        public void Encode(Packet pPacket)
        {
            pPacket.WriteInt(ID);
            pPacket.WriteString(Name, 13);


            pPacket.WriteByte(Gender); // Gender
            pPacket.WriteByte(Skin); // Skin
            pPacket.WriteInt(Face); // Face
            pPacket.WriteInt(Hair); // Hair

            pPacket.WriteLong(PetCashId);

            pPacket.WriteByte(Level);
            pPacket.WriteShort(Job);
            pPacket.WriteShort(Str);
            pPacket.WriteShort(Dex);
            pPacket.WriteShort(Int);
            pPacket.WriteShort(Luk);
            pPacket.WriteShort(HP);
            pPacket.WriteShort(MaxHP);
            pPacket.WriteShort(MP);
            pPacket.WriteShort(MaxMP);
            pPacket.WriteShort(AP);
            pPacket.WriteShort(SP);
            pPacket.WriteInt(EXP);
            pPacket.WriteShort(Fame);

            pPacket.WriteInt(MapID);
            pPacket.WriteByte(MapPosition);


            pPacket.WriteLong(0); // I have still no idea what these are
            pPacket.WriteInt(0);
            pPacket.WriteInt(0);
        }

        public void EncodeMoney(Packet pPacket)
        {
            pPacket.WriteInt(Money); // Hell yea
        }

        public void Decode(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            Name = pPacket.ReadString(13);


            Gender = pPacket.ReadByte(); // Gender
            Skin = pPacket.ReadByte(); // Skin
            Face = pPacket.ReadInt(); // Face
            Hair = pPacket.ReadInt(); // Hair

            PetCashId = pPacket.ReadLong();

            Level = pPacket.ReadByte();
            Job = pPacket.ReadShort();
            Str = pPacket.ReadShort();
            Dex = pPacket.ReadShort();
            Int = pPacket.ReadShort();
            Luk = pPacket.ReadShort();
            HP = pPacket.ReadShort();
            MaxHP = pPacket.ReadShort();
            MP = pPacket.ReadShort();
            MaxMP = pPacket.ReadShort();
            AP = pPacket.ReadShort();
            SP = pPacket.ReadShort();
            EXP = pPacket.ReadInt();
            Fame = pPacket.ReadShort();

            MapID = pPacket.ReadInt();
            MapPosition = pPacket.ReadByte();


            pPacket.ReadLong(); // I have still no idea what these are
            pPacket.ReadInt();
            pPacket.ReadInt();
        }

        public void DecodeMoney(Packet pPacket)
        {
            Money = pPacket.ReadInt();
        }
    }
}
