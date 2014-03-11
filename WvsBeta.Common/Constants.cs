using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public partial class Constants
    {
        //EXP formula for parties and such

        public static double PartyEXP(double PartyLevel, int PartyCount, double EXP, int KillerLevel, int GainerLevel)
        {
            double expBonus = 1.0;

            expBonus = expBonus = 1.10 + 0.05 * PartyCount;
            double AvgPartyLevel = PartyLevel;
            double expFraction = (EXP * expBonus) / (PartyCount+ 1);
            double ExpWeight = 1.0;
            double levelMod = GainerLevel / AvgPartyLevel;

            double iexp = (expFraction * ExpWeight * levelMod);

            return iexp;
        }

        public double GeneralDamage()
        {
            //todo!
            return ((BaseMinDamage(1,1,1,1,1,1,1) * ModifierA(1,1)) - 10);
        }

        public double PrimarySecondary(double Primary, double Secondary, bool isStar, bool isBow, bool isCrossbow)
        {
            if (isStar) return ((Primary * 3.6) + Secondary);
            if (isBow) return ((Primary * 3.4) + Secondary);
            if (isCrossbow) return ((Primary * 3.6) + Secondary);
            return 0.0;
        }

        public double Primary(double Primary, byte Type)
        { 
            switch (Type)
            {
                case 0: return (Primary * 3.6); //star
                case 1: return (Primary * 3.4); //bow
                case 2: return (Primary * 3.6); //crossbow
            }
            return 0.0;
        }

        public double Secondary(double pSecondary, byte Type, double Secondary2 = 0)
        {
            switch (Type)
            {
                case 0: return (pSecondary + Secondary2);
                case 1: return (pSecondary);
                case 2: return (pSecondary);
            }
            return 0.0;
        }

        public double BaseMaxDamage(double Prime, double Sec, double WeapAttack, bool isStar, bool isBow, bool isCrossbow)
        {
            return (PrimarySecondary(Prime, Sec, isStar, isBow, isCrossbow) * WeapAttack / 100);
        }

        public double BaseMinDamage(double Prime, double Sec, double WeapAttack, byte Type, int Mastery, double Secondary2, double WeaponAttack)
        {
            return (Primary(Prime, Type) + Mastery * 0.9 + Secondary(Sec, Type, Secondary2)) * WeaponAttack / 100;
        }

        public double ModifierA(int CharLevel, int MobLevel)
        {
            return LevelDisadvantage(CharLevel, MobLevel);
        }

        public double LevelDisadvantage(int CharLevel, int MobLevel)
        {
            if (MobLevel > CharLevel)
            {
                return 1 - 0.01 * (MobLevel - CharLevel);
            }
            else
            {
                return 1 - 0.01 * (0);
            }
        }

        public double ElementModifier()
        {
            //leave this for now... doesn't make a huge difference
            return 1.0;
        }

        public double DefenseDamage()
        {
            return 0.0;
        }

        public static int[] EXP = new int[] {
            15, 34, 57, 92, 135, 372, 560, 840, 1242, 1716,
	        2360, 3216, 4200, 5460, 7050, 8840, 11040, 13716, 16680, 20216,
	        24402, 28980, 34320, 40512, 47216, 54900, 63666, 73080, 83720, 95700,
	        108480, 122760, 138666, 155540, 174216, 194832, 216600, 240500, 266682, 294216,
	        324240, 356916, 391160, 428280, 468450, 510420, 555680, 604416, 655200, 709716,
	        748608, 789631, 832902, 878545, 926689, 977471, 1031036, 1087536, 1147132, 1209994,
	        1276301, 1346242, 1420016, 1497832, 1579913, 1666492, 1757815, 1854143, 1955750, 2062925,
	        2175973, 2295216, 2420993, 2553663, 2693603, 2841212, 2996910, 3161140, 3334370, 3517093,
	        3709829, 3913127, 4127566, 4353756, 4592341, 4844001, 5109452, 5389449, 5684790, 5996316,
	        6324914, 6671519, 7037118, 7422752, 7829518, 8258575, 8711144, 9188514, 9692044, 10223168,
	        10783397, 11374327, 11997640, 12655110, 13348610, 14080113, 14851703, 15665576, 16524049, 17429566,
	        18384706, 19392187, 20454878, 21575805, 22758159, 24005306, 25320796, 26708375, 28171993, 29715818,
	        31344244, 33061908, 34873700, 36784778, 38800583, 40926854, 43169645, 45535341, 48030677, 50662758,
	        53439077, 56367538, 59456479, 62714694, 66151459, 69776558, 73600313, 77633610, 81887931, 86375389,
	        91108760, 96101520, 101367883, 106992842, 112782213, 118962678, 125481832, 132358236, 139611467, 147262175,
	        155332142, 163844343, 172823012, 182293713, 192283408, 202820538, 213935103, 225658746, 238024845, 251068606,
	        264827165, 279339639, 294647508, 310794191, 327825712, 345790561, 364739883, 384727628, 405810702, 428049128,
	        451506220, 476248760, 502347192, 529875818, 558913012, 589541445, 621848316, 655925603, 691870326, 729784819,
	        769777027, 811960808, 856456260, 903390063, 952895838, 1005114529, 1060194805, 1118293480, 1179575962, 1244216724,
	        1312399800, 1384319309, 1460180007, 1540197871, 1624600714, 1713628833, 1807535693, 1906558648, 2011069705
        };

        public static string[] Suspicious = { "f425" };

        //This is taken straight from the WZ files.. dont laugh lol
        public static string[] Banned = { "asskisser","aiheh","assmaster","gaaay","gaay","gampang","gatal","gatal","gay","gizay","goddamn","goddmamn",
                                            "gook","assmuch","hanyun","haram","havesex","hayun","henjut","hisap","homo","hong gan","hoochie","hooters",
                                            "assmunch","iut","jackoff","jackoff","jalang","jantan","japs","jerkme","jerkoff","jiao","jilat","babi","jiz",
                                            "jizm","jubur","kanni","katak","kecing","kecut","kelentit","kepala","kerang","badus","kike","knn","kodok",
                                            "konek","kopek","kote","kulum","kurap","lahanat","asswipe","bahlul","lanchiau","lanjut","lebeh","lempuduk",
                                            "lendir","lesbian","lesbo","lezbo","loyot","mamak","balls","mampus","mangkuk","mastabate","mastarbate",
                                            "masterbate","masturbate","melancap","miang","missionary","mofucc","ballz","mothafuc","mulut","mutha","mytit",
                                            "nabei","negro","nenen","neraka","ngongkek","niga","bantaton","nigar","niger","nigga","niggar","nigger","nikah",
                                            "nipple","nonok","nutsack","nyah","bapuk","orgasm","orgy","palat","pantat","pelacur","peler","penis","pepek",
                                            "perempuan","pergi","amput","barua","phuck","pondan","porn","porno","pukek","puki","pussie","pussy","pussy",
                                            "retard","bastard","rubmy","schlong","sexfreak","sexmachine","sexual","sexwith","sh1t","shibal","shit",
                                            "shiz","batang","shlt","sial","spank","sperm","spum","ssh1t","sshit","sshlt","suckme","suckmy","bawah",
                                            "sundal","tantalau","telur","tersengih","tetek","titty","toceng","toli","tonton","tonyok","belakang","totok",
                                            "twat","vagina","wackoff","wackoff","wanker","whore","yetmeh","yourtit","aiheh","bengkok","amput","anak","anjuk",
                                            "babi","badus","bahlul","bantaton","bapuk","barua","batang","berapi","bawah","belakang","bengkok","berapi","bijik",
                                            "bodoh","burit","busuk","butoh","celaka","biaatch","chinhooi","cibai","cingkak","cingkolou","cipap","gampang","gatal",
                                            "gatal","hanyun","haram","biatch","hayun","henjut","hisap","iut","jalang","jantan","jilat","jubur","katak","kecing",
                                            "biiiitch","kecut","kelentit","kepala","kerang","kodok","konek","kopek","kote","kulum","kurap","anak","biiitch",
                                            "lahanat","lanchiau","lanjut","lebeh","lempuduk","lendir","loyot","mamak","mampus","mangkuk","biitch","melancap","miang",
                                            "mulut","nenen","neraka","ngongkek","nikah","nonok","nyah","palat","bijik","pantat","pelacur","peler","pepek","perempuan",
                                            "pergi","pondan","pukek","puki","sial","biotch","sundal","tantalau","telur","tersengih","tetek","toceng","toli","tonton",
                                            "tonyok","totok","bitch","yetmeh","jiao","asscrack","cheebye","cheeby","chebye","knn","cb","fark","fauk","bitchass",
                                            "ccb","ciao","cork","nabei","kanni","buttocks","masturbate","fauk","fark","bittch","biyaaatch","biyatch","biyotch",
                                            "anal","bizatch","biznatch","bllltch","blotch","blowjob","blowme","bltch","blyotch","bodoh","burit","analsex","busuk",
                                            "butoh","buttmunch","buttocks","bytch","c8","cb","ccb","celaka","chebye","anjuk","cheeby","cheebye","chinhooi","chink",
                                            "choochie","ciao","cibai","cingkak","cingkolou","cipap","asshole","clit","clitoris","cock","condom","cork","cottonpick",
                                            "cum","cunnt","cunt","damn","asslover","deepthroat","dick","dildo","dlldo","doggystyle","doggystyle","dumbfuck",
                                            "dyke","eatme","fag","asslover","faggot","fark","fauk","fetish","fuc","fuck","fucker","fuk","fuker","fuuk" };

        public const byte PlayerLevels = 200;
        public const byte PetLevels = 30;
        public const short MaxMaxHp = 30000;
        public const short MinMaxHp = 1;
        public const short MaxMaxMp = 30000;
        public const short MinMaxMp = 1;
        public const short MaxFame = 30000;
        public const short MinFame = -30000;
        public const short MaxCloseness = 30000;
        public const short ApPerLevel = 5;
        public const short SpPerLevel = 3;
        public const byte MaxFullness = 100;
        public const byte MinFullness = 0;
        public const byte PetFeedFullness = 30;
        public const int MaxDamage = 99999;

        public static short[] PetExp = new short[PetLevels - 1] {
		    1, 3, 6, 14, 31, 60, 108, 181, 287, 434,
		    632, 891, 1224, 1642, 2161, 2793, 3557, 4467, 5542, 6801,
		    8263, 9950, 11882, 14084, 16578, 19391, 22548, 26074, 30000
	    };

        public class BaseHp
        {
            public const short Variation = 4; // This is the range of HP that the server will give

            public const short Beginner = 12; // These are base HP values rewarded on level up
            public const short Warrior = 24;
            public const short Magician = 10;
            public const short Bowman = 20;
            public const short Thief = 20;
            public const short Gm = 150;

            public const short BeginnerAp = 8; // These are base HP values rewarded on AP distribution
            public const short WarriorAp = 20;
            public const short MagicianAp = 8;
            public const short BowmanAp = 16;
            public const short ThiefAp = 16;
            public const short GmAp = 16;
        }

        public class BaseMp
        {
            public const short Variation = 2; // This is the range of MP that the server will give

            public const short Beginner = 10; // These are base MP values rewarded on level up
            public const short Warrior = 4;
            public const short Magician = 6;
            public const short Bowman = 14;
            public const short Thief = 14;
            public const short Gm = 150;

            public const short BeginnerAp = 6; // These are base MP values rewarded on AP distribution
            public const short WarriorAp = 2;
            public const short MagicianAp = 18;
            public const short BowmanAp = 10;
            public const short ThiefAp = 10;
            public const short GmAp = 10;
        }


        public class EquipSlots
        {
            public enum Slots
            {
                Helm = 1,
                Face = 2,
                Eye = 3,
                Earring = 4,
                Top = 5,
                Bottom = 6,
                Shoe = 7,
                Glove = 8,
                Cape = 9,
                Shield = 10,
                Weapon = 11,
                Ring1 = 12,
                Ring2 = 13,
                PetEquip1 = 14,
                Ring3 = 15,
                Ring4 = 16,
                Pendant = 17,
                Mount = 18,
                Saddle = 19,
                PetCollar = 20,
                PetLabelRing1 = 21,
                PetItemPouch1 = 22,
                PetMesoMagnet1 = 23,
                PetAutoHp = 24,
                PetAutoMp = 25,
                PetWingBoots1 = 26,
                PetBinoculars1 = 27,
                PetMagicScales1 = 28,
                PetQuoteRing1 = 29,
                PetEquip2 = 30,
                PetLabelRing2 = 31,
                PetQuoteRing2 = 32,
                PetItemPouch2 = 33,
                PetMesoMagnet2 = 34,
                PetWingBoots2 = 35,
                PetBinoculars2 = 36,
                PetMagicScales2 = 37,
                PetEquip3 = 38,
                PetLabelRing3 = 39,
                PetQuoteRing3 = 40,
                PetItemPouch3 = 41,
                PetMesoMagnet3 = 42,
                PetWingBoots3 = 43,
                PetBinoculars3 = 44,
                PetMagicScales3 = 45,
                PetItemIgnore1 = 46,
                PetItemIgnore2 = 47,
                PetItemIgnore3 = 48,
                Medal = 49,
                Belt = 50
            };
        }

        public class Items
        {
            public class Types
            {
                public enum ItemTypes
                {
                    ArmorHelm = 100,
                    ArmorFace = 101,
                    ArmorEye = 102,
                    ArmorEarring = 103,
                    ArmorTop = 104,
                    ArmorOverall = 105,
                    ArmorBottom = 106,
                    ArmorShoe = 107,
                    ArmorGlove = 108,
                    ArmorShield = 109,
                    ArmorCape = 110,
                    ArmorRing = 111,
                    ArmorPendant = 112,
                    Medal = 114,
                    Weapon1hSword = 130,
                    Weapon1hAxe = 131,
                    Weapon1hMace = 132,
                    WeaponDagger = 133,
                    WeaponWand = 137,
                    WeaponStaff = 138,
                    Weapon2hSword = 140,
                    Weapon2hAxe = 141,
                    Weapon2hMace = 142,
                    WeaponSpear = 143,
                    WeaponPolearm = 144,
                    WeaponBow = 145,
                    WeaponCrossbow = 146,
                    WeaponClaw = 147,
                    ItemArrow = 206,
                    ItemStar = 207,
                    WeatherCash = 512,
                    CashPetFood = 524
                };
            }

            public class ScrollTypes
            {
                public enum Types
                {
                    Helm = 0,
                    Face = 100,
                    Eye = 200,
                    Earring = 300,
                    Topwear = 400,
                    Overall = 500,
                    Bottomwear = 600,
                    Shoes = 700,
                    Gloves = 800,
                    Shield = 900,
                    Cape = 1000,
                    Ring = 1100,
                    Pendant = 1200,
                    OneHandedSword = 3000,
                    OneHandedAxe = 3100,
                    OneHandedMace = 3200,
                    Dagger = 3300,
                    Wand = 3700,
                    Staff = 3800,
                    TwoHandedSword = 4000,
                    TwoHandedAxe = 4100,
                    TwoHandedMace = 4200,
                    Spear = 4300,
                    Polearm = 4400,
                    Bow = 4500,
                    Crossbow = 4600,
                    Claw = 4700,
                    PetEquip = 8000,
                };
            }

        }


        public class MobSkills
        {
            public enum Skills
            {
                WeaponAttackUp = 100,
                WeaponAttackUpAoe = 110,
                MagicAttackUp = 101,
                MagicAttackUpAoe = 111,
                WeaponDefenseUp = 102,
                WeaponDefenseUpAoe = 112,
                MagicDefenseUp = 103,
                MagicDefenseUpAoe = 113,
                HealAoe = 114,
                SpeedUpAoe = 115,
                Seal = 120,
                Darkness = 121,
                Weakness = 122,
                Stun = 123,
                Curse = 124,
                Poison = 125,
                PoisonMist = 131,
                WeaponImmunity = 140,
                MagicImmunity = 141,
                Summon = 200
            };
        }

        public static byte getItemTypeInPacket(int itemid)
        {
            if (isEquip(itemid)) return 1;
            else if (isPet(itemid)) return 5;
            else return 2;
        }

        public static string getDropName(int objectid, bool isMob)
        {
            return (isMob ? "m" : "r") + objectid.ToString();
        }


        public static byte getIsEquip(int itemid)
        {
            if (Constants.isEquip(itemid))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        public static byte getInventory(int itemid) { return (byte)(itemid / 1000000); }
        public static int getItemType(int itemid) { return (itemid / 10000); }
        public static int getScrollType(int itemid) { return ((itemid % 10000) - (itemid % 100)); }
        public static int itemTypeToScrollType(int itemid) { return ((getItemType(itemid) % 100) * 100); }
        public static bool isArrow(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.ItemArrow); }
        public static bool isStar(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.ItemStar); }
        public static bool isRechargeable(int itemid) { return isStar(itemid); }
        public static bool isEquip(int itemid) { return (getInventory(itemid) == 1); }
        public static bool isPet(int itemid) { return (getInventory(itemid) == 5); }
        public static bool isStackable(int itemid) { return !(isRechargeable(itemid) || isEquip(itemid) || isPet(itemid)); }
        public static bool isOverall(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.ArmorOverall); }
        public static bool isTop(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.ArmorTop); }
        public static bool isBottom(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.ArmorBottom); }
        public static bool isShield(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.ArmorShield); }
        public static bool is2hWeapon(int itemid) { return (getItemType(itemid) / 10 == 14); }
        public static bool is1hWeapon(int itemid) { return (getItemType(itemid) / 10 == 13); }
        public static bool isBow(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.WeaponBow); }
        public static bool isCrossbow(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.WeaponCrossbow); }
        public static bool isSword(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.Weapon1hSword || getItemType(itemid) == (int)Items.Types.ItemTypes.Weapon2hSword); }
        public static bool isMace(int itemid) { return (getItemType(itemid) == (int)Items.Types.ItemTypes.Weapon1hMace || getItemType(itemid) == (int)Items.Types.ItemTypes.Weapon2hMace); }
        public static bool isValidInventory(byte inv) { return (inv > 0 && inv <= 5); }

        public static bool isPuppet(int skillid) { return (skillid == (int)Sniper.Skills.Puppet || skillid == (int)Ranger.Skills.Puppet); }
        public static bool isSummon(int skillid) { return (isPuppet(skillid) || skillid == (int)Priest.Skills.SummonDragon || skillid == (int)Ranger.Skills.SilverHawk || skillid == (int)Sniper.Skills.GoldenEagle); }

        public static byte getMasteryDisplay(byte level) { return (byte)((level + 1) / 2); }

        public static short getJobTrack(short job, bool flatten = false) { return (short)(flatten ? ((job / 100) % 10) : (job / 100)); }


        public static int GetLevelEXP(byte level)
        {
            return EXP[level - 1];
        }

        public class JobTracks
        {
            public enum Tracks
            {
                Beginner = 0,
                Warrior = 1,
                Magician = 2,
                Bowman = 3,
                Thief = 4,
                Gm = 5,
            }
        }

        public class Swordsman
        {
            public const short ID = 100;
            public enum Skills
            {
                ImprovedMaxHpIncrease = 1000001,
                Endure = 1000002,
                IronBody = 1001003
            };
        }
        public class Fighter
        {
            public const short ID = 110;
            public enum Skills
            {
                AxeBooster = 1101005,
                AxeMastery = 1100001,
                PowerGuard = 1101007,
                Rage = 1101006,
                SwordBooster = 1101004,
                SwordMastery = 1100000
            };
        }
        public class Crusader
        {
            public const short ID = 111;
            public enum Skills
            {
                ImprovedMpRecovery = 1110000,
                ArmorCrash = 1111007,
                AxeComa = 1111006,
                AxePanic = 1111004,
                ComboAttack = 1111002,
                Shout = 1111008,
                SwordComa = 1111005,
                SwordPanic = 1111003
            };
        }
        public class Page
        {
            public const short ID = 120;
            public enum Skills
            {
                BwBooster = 1201005,
                BwMastery = 1200001,
                PowerGuard = 1201007,
                SwordBooster = 1201004,
                SwordMastery = 1200000,
                Threaten = 1201006
            };
        }
        public class WhiteKnight
        {
            public const short ID = 121;
            public enum Skills
            {
                ImprovedMpRecovery = 1210000,
                BwFireCharge = 1211004,
                BwIceCharge = 1211006,
                BwLitCharge = 1211008,
                ChargeBlow = 1211002,
                MagicCrash = 1211009,
                SwordFireCharge = 1211003,
                SwordIceCharge = 1211005,
                SwordLitCharge = 1211007
            };
        }
        public class Spearman
        {
            public const short ID = 130;
            public enum Skills
            {
                HyperBody = 1301007,
                IronWill = 1301006,
                PolearmBooster = 1301005,
                PolearmMastery = 1300001,
                SpearBooster = 1301004,
                SpearMastery = 1300000
            };
        }
        public class DragonKnight
        {
            public const short ID = 131;
            public enum Skills
            {
                DragonBlood = 1311008,
                DragonRoar = 1311006,
                ElementalResistance = 1310000,
                PowerCrash = 1311007,
                Sacrifice = 1311005
            };
        }
        public class Magician
        {
            public const short ID = 200;
            public enum Skills
            {
                ImprovedMpRecovery = 2000000,
                ImprovedMaxMpIncrease = 2000001,
                MagicArmor = 2001003,
                MagicGuard = 2001002
            };
        }
        public class FPWizard
        {
            public const short ID = 210;
            public enum Skills
            {
                Meditation = 2101001,
                MpEater = 2100000,
                PoisonBreath = 2101005,
                Slow = 2101003
            };
        }
        public class FPMage
        {
            public const short ID = 211;
            public enum Skills
            {
                ElementAmplification = 2110001,
                ElementComposition = 2111006,
                PartialResistance = 2110000,
                PoisonMist = 2111003,
                Seal = 2111004,
                SpellBooster = 2111005
            };
        }
        public class ILWizard
        {
            public const short ID = 220;
            public enum Skills
            {
                ColdBeam = 2201004,
                Meditation = 2201001,
                MpEater = 2200000,
                Slow = 2201003
            };
        }
        public class ILMage
        {
            public const short ID = 221;
            public enum Skills
            {
                ElementAmplification = 2210001,
                ElementComposition = 2211006,
                IceStrike = 2211002,
                PartialResistance = 2210000,
                Seal = 2211004,
                SpellBooster = 2211005
            };
        }
        public class Cleric
        {
            public const short ID = 230;
            public enum Skills
            {
                Bless = 2301004,
                Heal = 2301002,
                Invincible = 2301003,
                MpEater = 2300000
            };
        }
        public class Priest
        {
            public const short ID = 231;
            public enum Skills
            {
                Dispel = 2311001,
                Doom = 2311005,
                ElementalResistance = 2310000,
                HolySymbol = 2311003,
                MysticDoor = 2311002,
                SummonDragon = 2311006
            };
        }
        public class Archer
        {
            public const short ID = 300;
            public enum Skills
            {
                CriticalShot = 3000001,
                Focus = 3001003
            };
        }
        public class Hunter
        {
            public const short ID = 310;
            public enum Skills
            {
                ArrowBomb = 3101005,
                BowBooster = 3101002,
                BowMastery = 3100000,
                SoulArrow = 3101004
            };
        }
        public class Ranger
        {
            public const short ID = 311;
            public enum Skills
            {
                MortalBlow = 3110001,
                Puppet = 3111002,
                SilverHawk = 3111005
            };
        }
        public class Crossbowman
        {
            public const short ID = 320;
            public enum Skills
            {
                CrossbowBooster = 3201002,
                CrossbowMastery = 3200000,
                SoulArrow = 3201004
            };
        }
        public class Sniper
        {
            public const short ID = 321;
            public enum Skills
            {
                Blizzard = 3211003,
                GoldenEagle = 3211005,
                MortalBlow = 3210001,
                Puppet = 3211002
            };
        }
        public class Rogue
        {
            public const short ID = 400;
            public enum Skills
            {
                DarkSight = 4001003,
                Disorder = 4001002,
                DoubleStab = 4001334,
                LuckySeven = 4001344
            };
        }
        public class Assassin
        {
            public const short ID = 410;
            public enum Skills
            {
                ClawBooster = 4101003,
                ClawMastery = 4100000,
                CriticalThrow = 4100001,
                Endure = 4100002,
                Drain = 4101005,
                Haste = 4101004
            };
        }
        public class Hermit
        {
            public const short ID = 411;
            public enum Skills
            {
                Alchemist = 4110000,
                Avenger = 4111005,
                MesoUp = 4111001,
                ShadowMeso = 4111004,
                ShadowPartner = 4111002,
                ShadowWeb = 4111003
            };
        }
        public class Bandit
        {
            public const short ID = 420;
            public enum Skills
            {
                DaggerBooster = 4201002,
                DaggerMastery = 4200000,
                Endure = 4200001,
                Haste = 4201003,
                SavageBlow = 4201005,
                Steal = 4201004
            };
        }
        public class ChiefBandit
        {
            public const short ID = 421;
            public enum Skills
            {
                Assaulter = 4211002,
                BandOfThieves = 4211004,
                Chakra = 4211001,
                MesoExplosion = 4211006,
                MesoGuard = 4211005,
                Pickpocket = 4211003
            };
        }
        public class Gm
        {
            public const short ID = 500;
            public enum Skills
            {
                Bless = 5001003,
                Haste = 5001001,
                HealPlusDispell = 5001000,
                Hide = 5001004,
                HolySymbol = 5001002,
                Resurrection = 5001005,
                SuperDragonRoar = 5001006,
                Teleport = 5001007
            };
        }
    }
}