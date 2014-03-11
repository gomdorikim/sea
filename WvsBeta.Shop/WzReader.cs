using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using WvsBeta.BinaryData;

namespace WvsBeta.Shop
{

    enum ParseTypes : byte
    {
        Maps = 0,
        Equips,
        NPCs,
        Items,
        Mobs,
        Skills,
        Drops,
        Pets,
        Cash
    }

    enum ConsumeCurseTypes : byte
    {
        Curse = 0x01,
        Seal = 0x02,
        Weakness = 0x04,
        Darkness = 0x08,
        Poison = 0x10
    }

    public class DataProvider
    {
        private static WzBinaryReader Reader { get; set; }
        public static Dictionary<int, EquipData> Equips { get; set; }
        public static Dictionary<int, ItemData> Items { get; set; }
        public static Dictionary<int, CommodityInfo> Commodity { get; set; }
        public static List<int> Pets { get; set; }

        public static void Load(string pPath)
        {
            if (!File.Exists(pPath)) throw new FileNotFoundException();
            using (Reader = new WzBinaryReader(File.Open(pPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                while (Reader.BaseStream.Position < Reader.BaseStream.Length)
                {
                    ParseTypes Type = (ParseTypes)Reader.ReadByte();
                    switch (Type)
                    {
                        case ParseTypes.Equips:
                            Console.Write("Starting reading equips...");
                            Equips = new Dictionary<int, EquipData>();
                            ReadEquips();
                            Reader.DoCheck();
                            Console.WriteLine("Done.");
                            break;
                        case ParseTypes.Items:
                            Console.Write("Starting reading items...");
                            Items = new Dictionary<int, ItemData>();
                            ReadItems();
                            Reader.DoCheck();
                            Console.WriteLine("Done.");
                            break;
                        case ParseTypes.Cash:
                            Console.Write("Starting reading commodity...");
                            Commodity = new Dictionary<int, CommodityInfo>();
                            ReadCommodity();
                            Reader.DoCheck();
                            Console.WriteLine("Done.");
                            break;
                        case ParseTypes.Pets:
                            Console.Write("Starting reading pets...");
                            Pets = new List<int>();
                            ReadPets();
                            Reader.DoCheck();
                            Console.WriteLine("Done.");
                            break;
                        default:
                            throw new OutOfMemoryException("Could not find parsetype.");
                    }
                }
                Reader = null; // Just to be sure it's dereferenced
            }
        }

        private static void ReadPets()
        {
            short Count = Reader.ReadInt16();
            for (short i = 0; i < Count; i++)
            {
                Pets.Add(Reader.ReadInt32()); // Only need the ID
                Reader.ReadByte();
                Reader.ReadByte();

                byte interacts = Reader.ReadByte();
                for (byte j = 0; j < interacts; j++)
                {
                    Reader.ReadByte();
                    Reader.ReadByte();
                    Reader.ReadByte();
                }
            }
        }

        private static void ReadCommodity()
        {
            ushort Count = Reader.ReadUInt16();
            for (ushort i = 0; i < Count; i++)
            {
                CommodityInfo ci = new CommodityInfo();
                ci.SerialNumber = Reader.ReadInt32();
                ci.ItemID = Reader.ReadInt32();
                ci.Amount = Reader.ReadInt16();
                ci.Period = Reader.ReadInt16();
                ci.Price = Reader.ReadInt32();
                ci.Gender = (CommodityGenders)Reader.ReadByte();
                Commodity.Add(ci.SerialNumber, ci);
            }
        }

        private static void ReadItems()
        {
            ushort Count = Reader.ReadUInt16();
            for (ushort i = 0; i < Count; i++)
            {
                ItemData item = new ItemData();
                item.ID = Reader.ReadInt32();
                bool hasInfo = Reader.ReadBoolean();
                if (hasInfo)
                {
                    item.Price = Reader.ReadInt32();
                    item.Cash = Reader.ReadBoolean();
                    item.MaxSlot = Reader.ReadUInt16();
                    item.Mesos = Reader.ReadInt32();
                    item.IsQuest = Reader.ReadBoolean();

                    item.ScrollSuccessRate = Reader.ReadByte();
                    item.ScrollCurseRate = Reader.ReadByte();
                    item.IncStr = Reader.ReadByte();
                    item.IncDex = Reader.ReadByte();
                    item.IncInt = Reader.ReadByte();
                    item.IncLuk = Reader.ReadByte();
                    item.IncMHP = Reader.ReadByte();
                    item.IncMMP = Reader.ReadByte();
                    item.IncWAtk = Reader.ReadByte();
                    item.IncMAtk = Reader.ReadByte();
                    item.IncWDef = Reader.ReadByte();
                    item.IncMDef = Reader.ReadByte();
                    item.IncAcc = Reader.ReadByte();
                    item.IncAvo = Reader.ReadByte();
                    item.IncJump = Reader.ReadByte();
                    item.IncSpeed = Reader.ReadByte();


                    item.Rate = Reader.ReadByte();
                    item.RateTimes = new Dictionary<byte, List<KeyValuePair<byte, byte>>>();
                    byte tamount = Reader.ReadByte();
                    for (int s = 0; s < tamount; s++)
                    {
                        string val = ReadString();
                        string day = val.Substring(0, 3);
                        byte hourStart = byte.Parse(val.Substring(4, 2));
                        byte hourEnd = byte.Parse(val.Substring(7, 2));

                        byte dayid = 0;
                        switch (day)
                        {
                            case "MON": dayid = 0; break;
                            case "TUE": dayid = 1; break;
                            case "WED": dayid = 2; break;
                            case "THU": dayid = 3; break;
                            case "FRI": dayid = 4; break;
                            case "SAT": dayid = 5; break;
                            case "SUN": dayid = 6; break;
                            case "HOL": dayid = ItemData.HOLIDAY_DAY; break;
                            default: Console.WriteLine("WasDit: {0}", val); continue;
                        }

                        if (!item.RateTimes.ContainsKey(dayid))
                            item.RateTimes.Add(dayid, new List<KeyValuePair<byte, byte>>());

                        item.RateTimes[dayid].Add(new KeyValuePair<byte, byte>(hourStart, hourEnd));
                    }


                }
                else
                {
                    item.Price = 0;
                    item.Cash = false;
                    item.MaxSlot = 1;
                    item.Mesos = 0;
                    item.IsQuest = false;

                    item.ScrollSuccessRate = 0;
                    item.ScrollCurseRate = 0;
                    item.IncStr = 0;
                    item.IncDex = 0;
                    item.IncInt = 0;
                    item.IncLuk = 0;
                    item.IncMHP = 0;
                    item.IncMMP = 0;
                    item.IncWAtk = 0;
                    item.IncMAtk = 0;
                    item.IncWDef = 0;
                    item.IncMDef = 0;
                    item.IncAcc = 0;
                    item.IncAvo = 0;
                    item.IncJump = 0;
                    item.IncSpeed = 0;
                    item.RateTimes = new Dictionary<byte, List<KeyValuePair<byte, byte>>>();
                    item.Rate = 0;
                }

                hasInfo = Reader.ReadBoolean();
                if (hasInfo)
                {

                    item.MoveTo = Reader.ReadInt32();

                    item.CureFlags = Reader.ReadByte();

                    item.HP = Reader.ReadInt16();
                    item.MP = Reader.ReadInt16();
                    item.HPRate = Reader.ReadInt16();
                    item.MPRate = Reader.ReadInt16();
                    item.Speed = Reader.ReadInt16();
                    item.Avoidance = Reader.ReadInt16();
                    item.Accuracy = Reader.ReadInt16();
                    item.MagicAttack = Reader.ReadInt16();
                    item.WeaponAttack = Reader.ReadInt16();
                    item.BuffTime = Reader.ReadInt32();
                }
                else
                {
                    item.MoveTo = 0;
                    item.CureFlags = 0;
                    item.HP = 0;
                    item.MP = 0;
                    item.HPRate = 0;
                    item.MPRate = 0;
                    item.Speed = 0;
                    item.Avoidance = 0;
                    item.Accuracy = 0;
                    item.MagicAttack = 0;
                    item.WeaponAttack = 0;
                    item.BuffTime = 0;
                }
                {
                    item.Summons = new List<ItemSummonInfo>();
                    byte amount = Reader.ReadByte();
                    for (int s = 0; s < amount; s++)
                    {
                        ItemSummonInfo isi = new ItemSummonInfo();
                        isi.MobID = Reader.ReadInt32();
                        isi.Chance = Reader.ReadByte();
                        item.Summons.Add(isi);
                    }
                }

                Items.Add(item.ID, item);
            }
        }

        private static void ReadEquips()
        {
            ushort Count = Reader.ReadUInt16();
            for (ushort i = 0; i < Count; i++)
            {
                EquipData eq = new EquipData();
                eq.ID = Reader.ReadInt32();
                eq.isCash = Reader.ReadBoolean();
                eq.Type = ReadString();
                eq.RequiredLevel = Reader.ReadByte();
                eq.Scrolls = (byte)Reader.ReadUInt16();
                eq.RequiredDexterity = Reader.ReadUInt16();
                eq.RequiredIntellect = Reader.ReadUInt16();
                eq.RequiredLuck = Reader.ReadUInt16();
                eq.RequiredStrength = Reader.ReadUInt16();
                eq.RequiredJob = Reader.ReadUInt16();
                eq.Price = Reader.ReadInt32();
                eq.Strength = Reader.ReadInt16();
                eq.Dexterity = Reader.ReadInt16();
                eq.Intellect = Reader.ReadInt16();
                eq.Luck = Reader.ReadInt16();
                eq.MagicDefense = Reader.ReadByte();
                eq.WeaponDefense = Reader.ReadByte();
                eq.WeaponAttack = Reader.ReadByte();
                eq.MagicAttack = Reader.ReadByte();
                eq.Speed = Reader.ReadByte();
                eq.Jump = Reader.ReadByte();
                eq.Accuracy = Reader.ReadByte();
                eq.Avoidance = Reader.ReadByte();
                eq.HP = Reader.ReadInt16();
                eq.MP = Reader.ReadInt16();
                Equips.Add(eq.ID, eq);
            }
        }

        private static string ReadString()
        {
            ushort len = Reader.ReadByte();
            if (len == 0xff)
                return string.Empty;
            if (len == 0)
                len = Reader.ReadUInt16();
            return System.Text.Encoding.ASCII.GetString(Reader.ReadBytes(len));
        }
    }





    public class EquipData
    {
        public int ID { get; set; }
        public bool isCash { get; set; }
        public string Type { get; set; }
        public byte HealHP { get; set; }
        public byte Scrolls { get; set; }
        public byte RequiredLevel { get; set; }
        public ushort RequiredStrength { get; set; }
        public ushort RequiredDexterity { get; set; }
        public ushort RequiredIntellect { get; set; }
        public ushort RequiredLuck { get; set; }
        public ushort RequiredJob { get; set; }
        public int Price { get; set; }
        public byte RequiredFame { get; set; }
        public short HP { get; set; }
        public short MP { get; set; }
        public short Strength { get; set; }
        public short Dexterity { get; set; }
        public short Intellect { get; set; }
        public short Luck { get; set; }
        public byte Hands { get; set; }
        public byte WeaponAttack { get; set; }
        public byte MagicAttack { get; set; }
        public byte WeaponDefense { get; set; }
        public byte MagicDefense { get; set; }
        public byte Accuracy { get; set; }
        public byte Avoidance { get; set; }
        public byte Speed { get; set; }
        public byte Jump { get; set; }
    }

    public class ItemData
    {
        public int ID { get; set; }
        public int Price { get; set; }
        public bool Cash { get; set; }
        public ushort MaxSlot { get; set; }
        public bool IsQuest { get; set; }
        public short HP { get; set; }
        public short MP { get; set; }
        public short HPRate { get; set; }
        public short MPRate { get; set; }
        public short WeaponAttack { get; set; }
        public short MagicAttack { get; set; }
        public short Accuracy { get; set; }
        public short Avoidance { get; set; }
        public short Speed { get; set; }
        public int BuffTime { get; set; }

        public byte CureFlags { get; set; }

        public int MoveTo { get; set; }
        public int Mesos { get; set; }

        public byte ScrollSuccessRate { get; set; }
        public byte ScrollCurseRate { get; set; }
        public byte IncStr { get; set; }
        public byte IncDex { get; set; }
        public byte IncInt { get; set; }
        public byte IncLuk { get; set; }
        public byte IncMHP { get; set; }
        public byte IncMMP { get; set; }
        public byte IncWAtk { get; set; }
        public byte IncMAtk { get; set; }
        public byte IncWDef { get; set; }
        public byte IncMDef { get; set; }
        public byte IncAcc { get; set; }
        public byte IncAvo { get; set; }
        public byte IncJump { get; set; }
        public byte IncSpeed { get; set; }
        public byte Rate { get; set; }

        public List<ItemSummonInfo> Summons { get; set; }

        public Dictionary<byte, List<KeyValuePair<byte, byte>>> RateTimes { get; set; }
        public const byte HOLIDAY_DAY = 20;
        public static bool RateCardEnabled(ItemData pItemData, bool pIsHoliday = false)
        {
            DateTime now = DateTime.Now;
            byte currentDay = pIsHoliday && pItemData.RateTimes.ContainsKey(HOLIDAY_DAY) ? HOLIDAY_DAY : (byte)now.DayOfWeek;


            if (!pItemData.RateTimes.ContainsKey(currentDay)) return false;

            foreach (var kvp in pItemData.RateTimes[currentDay])
            {
                if (kvp.Key <= now.Hour && kvp.Value >= now.Hour)
                {
                    Console.WriteLine("Found rate for {0} - {1}", kvp.Key, kvp.Value);
                    return true;
                }
            }
            return false;
        }
    }

    public class ItemSummonInfo
    {
        public int MobID { get; set; }
        public byte Chance { get; set; }
    }

    public class SkillLevelData
    {
        public byte MobCount { get; set; }
        public byte HitCount { get; set; }

        public int BuffTime { get; set; }
        public short Damage { get; set; }
        public short AttackRange { get; set; }
        public byte Mastery { get; set; }

        public short HPProperty { get; set; }
        public short MPProperty { get; set; }
        public short Property { get; set; }

        public short HPUsage { get; set; }
        public short MPUsage { get; set; }
        public int ItemIDUsage { get; set; }
        public short ItemAmountUsage { get; set; }
        public short BulletUsage { get; set; }
        public short MesosUsage { get; set; }

        public short XValue { get; set; }
        public short YValue { get; set; }

        public short Speed { get; set; }
        public short Jump { get; set; }
        public short WeaponAttack { get; set; }
        public short MagicAttack { get; set; }
        public short WeaponDefense { get; set; }
        public short MagicDefense { get; set; }
        public short Accurancy { get; set; }
        public short Avoidability { get; set; }

        public byte ElementFlags { get; set; }

        public short LTX { get; set; }
        public short LTY { get; set; }
        public short RBX { get; set; }
        public short RBY { get; set; }


    }


    public class PetData
    {
        public int ItemID { get; set; }
        public byte Hungry { get; set; }
        public byte Life { get; set; }
        public Dictionary<byte, PetReactionData> Reactions { get; set; }
    }

    public class PetReactionData
    {
        public byte ReactionID { get; set; }
        public byte Inc { get; set; }
        public byte Prob { get; set; }
    }
    public enum CommodityGenders
    {
        Male = 0,
        Female = 1,
        Both = 2
    }

    public class CommodityInfo
    {
        public int SerialNumber { get; set; }
        public int ItemID { get; set; }
        public short Amount { get; set; }
        public short Period { get; set; }
        public int Price { get; set; }
        public CommodityGenders Gender { get; set; }
    }
}