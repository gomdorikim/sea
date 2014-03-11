using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WvsBeta.BinaryData;
using reNX;
using reNX.NXProperties;
using System.Collections;
using System.Drawing;
using WvsBeta.Common;

namespace WvsBeta.Game
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

    public enum SkillElement : byte
    {
        Normal,
        Ice,
        Fire,
        Poison,
        Lightning,
        Holy,
    }

    public class DataProvider
    {
        private static WzBinaryReader Reader { get; set; }
        public static Dictionary<int, Map> Maps { get; set; }
        public static Dictionary<int, EquipData> Equips { get; set; }
        public static Dictionary<int, NPCData> NPCs { get; set; }
        public static Dictionary<int, ItemData> Items { get; set; }
        public static List<int> UntradeableDrops { get; set; }
        public static Dictionary<int, PetData> Pets { get; set; }
        public static Dictionary<int, MobData> Mobs { get; set; }
        public static Dictionary<int, Dictionary<byte, SkillLevelData>> Skills { get; set; }
        public static Dictionary<byte, Dictionary<byte, MobSkillLevelData>> MobSkills { get; set; }
        public static Dictionary<string, List<DropData>> Drops { get; set; }
        public static Dictionary<int, Questdata> Quests { get; set; }
        public static NXFile pFile = new NXFile(@"C:\Users\Administrator\Dropbox\Source\BinSvr\Data.nx");
        public static NXFile pDropFile = new NXFile(@"C:\Users\Administrator\Dropbox\Source\BinSvr\DropData.nx");
        public static NXFile pNpcFile = new NXFile(@"C:\Users\Administrator\Dropbox\Source\BinSvr\NpcData.nx");
        public static DateTime startTime;
        public static DateTime endTime;

        public static void Load(string pPath)
        {
            startTime = DateTime.Now;

            ReadMobData();
            ReadMapData();
            //ReadMapFootholds(); //THIS NEEDS A REVAMP
            Equips = new Dictionary<int, EquipData>();
            UntradeableDrops = new List<int>();
            ReadEquips();
            NPCs = new Dictionary<int, NPCData>();
            try
            {
                ReadNpcs();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Items = new Dictionary<int, ItemData>();
            ReadItems();
            Skills = new Dictionary<int, Dictionary<byte, SkillLevelData>>();
            MobSkills = new Dictionary<byte, Dictionary<byte, MobSkillLevelData>>();
            ReadSkills();
            Pets = new Dictionary<int, PetData>();
            ReadPets();
            Drops = new Dictionary<string, List<DropData>>();
            ReadDrops();
            Quests = new Dictionary<int, Questdata>();
            try
            {
                ReadQuestData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            endTime = DateTime.Now;
            Console.WriteLine("Finished loading all WZ data in " + (endTime - startTime).TotalMilliseconds + " ms");
        }

        static void ReadMobData()
        {
            //Todo : MobAttackData
            Mobs = new Dictionary<int, MobData>();
            IEnumerator enumerator = pFile.ResolvePath("Mob").GetEnumerator();
            while (enumerator.MoveNext())
            {
                MobData data = new MobData();
                NXNode pNode = (NXNode)enumerator.Current;
                data.ID = (int)Utils.ConvertNameToID(pNode.Name);
                data.Level = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["level"]).Value;
                data.Undead = Utils.ReadBool((byte)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["undead"]).Value);
                data.BodyAttack = Utils.ReadBool((byte)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["bodyAttack"]).Value);
                data.SummonType = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["summonType"]).Value;
                data.EXP = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["exp"]).Value;
                data.MaxHP = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["maxHP"]).Value;
                data.MaxMP = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["maxMP"]).Value;
                if (pFile.BaseNode["Mob"][pNode.Name]["info"].ContainsChild("PDDamage"))
                {
                    data.PDD = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["PDDamage"]).Value;
                }
                if (pFile.BaseNode["Mob"][pNode.Name]["info"].ContainsChild("publicReward"))
                {
                    data.PublicReward = Utils.ReadBool((byte)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["publicReward"]).Value);
                }
                if (pFile.BaseNode["Mob"][pNode.Name]["info"].ContainsChild("flySpeed"))
                {
                    data.Flies = Utils.ReadBool((byte)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["flySpeed"]).Value);
                }
                else
                {
                    //If doesn't fly, read speed
                    data.Speed = (Int16)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["speed"]).Value;
                }
                if (pFile.BaseNode["Mob"][pNode.Name]["info"].ContainsChild("revive"))
                {
                    IEnumerator rEnumerator = pFile.ResolvePath("Mob/" + pNode.Name + "/info/revive").GetEnumerator();
                    data.Revive = new List<int>();
                    while (rEnumerator.MoveNext())
                    {
                        NXNode rNode = (NXNode)rEnumerator.Current;
                        data.Revive.Add((int)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["revive"][rNode.Name]).Value);
                    }
                }
                if (pFile.BaseNode["Mob"][pNode.Name]["info"].ContainsChild("skill"))
                {
                    data.Skills = new List<MobSkillData>();

                    IEnumerator pEnumerator = pFile.ResolvePath("Mob/" + pNode.Name + "/info/skill").GetEnumerator();
                    while (pEnumerator.MoveNext())
                    {
                        NXNode Node = (NXNode)pEnumerator.Current;
                        MobSkillData msd = new MobSkillData();

                        msd.SkillID = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["skill"][Node.Name]["skill"]).Value;
                        msd.Level = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["skill"][Node.Name]["level"]).Value;
                        msd.EffectAfter = (Int16)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["skill"][Node.Name]["effectAfter"]).Value;
                        data.Skills.Add(msd);
                    }
                }
                if (pFile.BaseNode["Mob"][pNode.Name]["info"].ContainsChild("boss"))
                {
                    //Read Boss Data!
                    if (pFile.BaseNode["Mob"][pNode.Name]["info"]["boss"].ContainsChild("hpRecovery"))
                    {
                        data.HPRecoverAmount = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["hpRecovery"]).Value;
                        data.MPRecoverAmount = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["mpRecovery"]).Value;
                    }
                    if (pFile.BaseNode["Mob"][pNode.Name]["info"].ContainsChild("hpTagColor"))
                    {
                        data.HPTagColor = (UInt32)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["hpTagColor"]).Value;
                        data.HPTagBgColor = (UInt32)((NXValuedNode<Int64>)pFile.BaseNode["Mob"][pNode.Name]["info"]["hpTagBgcolor"]).Value;
                    }
                }
                Mobs.Add(data.ID, data);
            }
            Console.WriteLine("Done Reading Mob Data!");
        }

        static void ReadMapData()
        {
            Maps = new Dictionary<int, Map>();
            IEnumerator enumerator = pFile.ResolvePath("Map/Map").GetEnumerator();

            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;

                IEnumerator mEnumerator = pFile.ResolvePath("Map/Map/" + pNode.Name).GetEnumerator();
                if (pNode.Name != "AreaCode.img")
                {
                    while (mEnumerator.MoveNext())
                    {
                        NXNode mNode = (NXNode)mEnumerator.Current;
                        int ID = (int)(Utils.ConvertNameToID(mNode.Name));
                        Map map = new Map(ID);
                        map.ForcedReturn = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["info"]["forcedReturn"]).Value;
                        map.ReturnMap = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["info"]["returnMap"]).Value;
                        map.Town = Utils.ReadBool((byte)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["info"]["town"]).Value);



                        if (pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["info"].ContainsChild("fieldType"))
                        {
                            map.FieldType = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["info"]["fieldType"]).Value;
                        }
                        if (pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name].ContainsChild("clock"))
                        {
                            map.HasClock = true;
                        }
                        double mobRate = (double)((NXValuedNode<double>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["info"]["mobRate"]).Value;
                        Maps.Add(map.ID, map);
                        ReadLife(mNode.Name, pNode.Name, map);
                        ReadPortals(mNode.Name, pNode.Name, map);
                        ReadSeats(mNode.Name, pNode.Name, map);
                        ReadReactors(mNode.Name, pNode.Name, map);
                        if (pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name].ContainsChild("reactor"))
                        {
                            if (pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["reactor"].ContainsChild("0"))
                            {
                                //200090011
                               //Console.WriteLine(mNode.Name.ToString());
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Done Reading Portals!");
            Console.WriteLine("Done Reading Maps!");
        }

        static void ReadMapFootholds()
        {
            //aka loop simulator 2000
            //so many loops -.-
            IEnumerator enumerator = pFile.ResolvePath("Map/Map").GetEnumerator();

            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;

                IEnumerator mEnumerator = pFile.ResolvePath("Map/Map/" + pNode.Name).GetEnumerator();
                if (pNode.Name != "AreaCode.img")
                {
                    while (mEnumerator.MoveNext())
                    {
                        NXNode mNode = (NXNode)mEnumerator.Current;
                        IEnumerator lEnumerator = pFile.ResolvePath("Map/Map/" + pNode.Name + "/" + mNode.Name + "/foothold").GetEnumerator();
                        while (lEnumerator.MoveNext())
                        {
                            NXNode anotherNode = (NXNode)lEnumerator.Current;
                            IEnumerator sEnumerator = pFile.ResolvePath("Map/Map/" + pNode.Name + "/" + mNode.Name + "/foothold/" + anotherNode.Name).GetEnumerator();
                            while (sEnumerator.MoveNext())
                            {
                                NXNode andAnotherNode = (NXNode)sEnumerator.Current;
                                IEnumerator fEnumerator = pFile.ResolvePath("Map/Map/" + pNode.Name + "/" + mNode.Name + "/foothold/" + anotherNode.Name + "/" + andAnotherNode.Name).GetEnumerator();

                                while (fEnumerator.MoveNext())
                                {
                                    NXNode andAnotherNode2 = (NXNode)fEnumerator.Current;
                                    Foothold ft = new Foothold();
                                    ft.ID = (ushort)Utils.ConvertNameToID(andAnotherNode2.Name);

                                    ft.NextIdentifier = (ushort)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["foothold"][anotherNode.Name][andAnotherNode.Name][andAnotherNode2.Name]["next"]).Value;
                                    ft.PreviousIdentifier = (ushort)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["foothold"][anotherNode.Name][andAnotherNode.Name][andAnotherNode2.Name]["prev"]).Value;
                                    ft.X1 = (short)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["foothold"][anotherNode.Name][andAnotherNode.Name][andAnotherNode2.Name]["x1"]).Value;
                                    ft.X2 = (short)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["foothold"][anotherNode.Name][andAnotherNode.Name][andAnotherNode2.Name]["x2"]).Value;
                                    ft.Y1 = (short)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["foothold"][anotherNode.Name][andAnotherNode.Name][andAnotherNode2.Name]["y1"]).Value;
                                    ft.Y2 = (short)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][pNode.Name][mNode.Name]["foothold"][anotherNode.Name][andAnotherNode.Name][andAnotherNode2.Name]["y2"]).Value;

                                    Map pMap = Maps[(int)Utils.ConvertNameToID(mNode.Name)];
                                    pMap.AddFoothold(ft);
                                }
                            }
                        }
                    }
                }
            }
        }

        static void ReadLife(string MapName, string MapParent, Map map)
        {
            IEnumerator enumerator = pFile.ResolvePath("Map/Map/" + MapParent + "/" + MapName + "/" + "life").GetEnumerator();
            if (pFile.BaseNode["Map"]["Map"][MapParent][MapName].ContainsChild("life"))
            {
                while (enumerator.MoveNext())
                {
                    NXNode pNode = (NXNode)enumerator.Current;


                    Int32 X = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["x"]).Value;
                    Int32 Y = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["y"]).Value;
                    Int32 ID = (Int32)Utils.ConvertNameToID(pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["id"].ValueOrDie<string>()); //Saved as a string ? wtf
                    Int32 Foothold = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["fh"]).Value;
                    Int32 Cy = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["cy"]).Value;
                    Int32 Rx0 = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["rx0"]).Value;
                    Int32 Rx1 = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["rx1"]).Value;


                    string type = pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["type"].ValueOrDie<string>();

                    Life lf = new Life();
                    lf.ID = ID;
                    lf.X = (short)X;
                    lf.Y = (short)Y;
                    lf.Foothold = (ushort)Foothold;
                    lf.Cy = (short)Cy;
                    lf.Rx0 = (short)Rx0;
                    lf.Rx1 = (short)Rx1;

                    if (pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name].ContainsChild("mobTime"))
                    {
                        lf.RespawnTime = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["mobTime"]).Value;
                    }
                    if (pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name].ContainsChild("f"))
                    {
                        lf.FacesLeft = Utils.ReadBool2((byte)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["life"][pNode.Name]["f"]).Value);
                    }
                    lf.Type = char.Parse(type);
                    map.AddLife(lf);
                }
            }
        }

        static void ReadPortals(string MapName, string MapParent, Map map)
        {
            IEnumerator enumerator = pFile.ResolvePath("Map/Map/" + MapParent + "/" + MapName + "/" + "portal").GetEnumerator();

            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;
                Portal pt = new Portal();
                int ID = (int)Utils.ConvertNameToID(pNode.Name);
                pt.ID = (byte)ID;
                string pName = pFile.BaseNode["Map"]["Map"][MapParent][MapName]["portal"][pNode.Name]["pn"].ValueOrDie<string>();
                pt.Name = pName;
                Int32 toMap = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["portal"][pNode.Name]["tm"]).Value;
                pt.ToMapID = toMap;
                string tName = pFile.BaseNode["Map"]["Map"][MapParent][MapName]["portal"][pNode.Name]["tn"].ValueOrDie<string>();
                pt.ToName = tName;
                Int32 X = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["portal"][pNode.Name]["x"]).Value;
                pt.X = (short)X;
                Int32 Y = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["portal"][pNode.Name]["y"]).Value;
                pt.Y = (short)Y;
                map.AddPortal(pt);
            }
        }

        static void ReadSeats(string MapName, string MapParent, Map map)
        {
            if (pFile.BaseNode["Map"]["Map"][MapParent][MapName].ContainsChild("seat"))
            {
                IEnumerator enumerator = pFile.ResolvePath("Map/Map/" + MapParent + "/" + MapName + "/" + "seat").GetEnumerator();
                while (enumerator.MoveNext())
                {
                    NXNode pNode = (NXNode)enumerator.Current;
                    Seat seat = new Seat();
                    seat.ID = (byte)Utils.ConvertNameToID(pNode.Name);
                    Point pPoint = pFile.ResolvePath("Map/Map/" + MapParent + "/" + MapName + "/seat/" + pNode.Name).ValueOrDie<Point>();
                    seat.X = (short)pPoint.X;
                    seat.Y = (short)pPoint.Y;
                    map.AddSeat(seat);
                }
            }
        }

        static void ReadReactors(string MapName, string MapParent, Map map)
        {
            if (pFile.BaseNode["Map"]["Map"][MapParent][MapName].ContainsChild("reactor"))
            {
                if (pFile.BaseNode["Map"]["Map"][MapParent][MapName]["reactor"].ChildCount > 0)
                {
                    IEnumerator enumerator = pFile.ResolvePath("Map/Map/" + MapParent + "/" + MapName + "/" + "reactor").GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        NXNode pNode = (NXNode)enumerator.Current;
                        Reactor reactor = new Reactor();
                        reactor.ID = int.Parse(pNode.Name);
                        try
                        {
                            reactor.ReactorID = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["reactor"][pNode.Name]["id"]).Value;
                        }
                        catch (Exception ex)
                        {
                            reactor.ReactorID = int.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["reactor"][pNode.Name]["id"]).ValueOrDie<string>());
                        }
                        reactor.ReactorTime = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["reactor"][pNode.Name]["reactorTime"]).Value;
                        reactor.X = (short)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["reactor"][pNode.Name]["x"]).Value;
                        reactor.Y = (short)((NXValuedNode<Int64>)pFile.BaseNode["Map"]["Map"][MapParent][MapName]["reactor"][pNode.Name]["y"]).Value;
                        map.Reactors.Add(reactor.ID, reactor);
                    }
                }
            }
        }

        static void ReadEquips()
        {
            //aka check similator 2000
            //some data are strings, not bytes, wtf
            IEnumerator enumerator = pFile.ResolvePath("Character").GetEnumerator();
            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;
                if (!pNode.Name.EndsWith(".img") && pNode.Name != "Afterimage")
                {
                    IEnumerator pEnumerator = pFile.ResolvePath("Character/" + pNode.Name).GetEnumerator();
                    while (pEnumerator.MoveNext())
                    {
                        NXNode mNode = (NXNode)pEnumerator.Current;

                        EquipData eq = new EquipData();

                        eq.ID = (int)Utils.ConvertNameToID(mNode.Name);
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("cash"))
                        {
                            eq.isCash = true;
                        }
                        eq.Type = pNode.Name;

                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("reqLevel"))
                        {
                            eq.RequiredLevel = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["reqLevel"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("tuc"))
                        {
                            eq.Scrolls = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["tuc"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("reqDEX"))
                        {
                            eq.RequiredDexterity = (ushort)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["reqDEX"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("reqINT"))
                        {
                            eq.RequiredIntellect = (ushort)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["reqINT"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("reqLUK"))
                        {
                            eq.RequiredLuck = (ushort)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["reqLUK"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("reqDEX"))
                        {
                            eq.RequiredStrength = (ushort)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["reqDEX"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("reqJob"))
                        {
                            try
                            {
                                eq.RequiredJob = (ushort)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["reqJob"]).Value;
                            }
                            catch (Exception ex)
                            {
                                eq.RequiredJob = (ushort)int.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["reqJob"]).ValueOrDie<string>());
                            }
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("price"))
                        {
                            try
                            {
                                eq.Price = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["price"]).Value;
                            }
                            catch (Exception ex)
                            {
                                eq.Price = int.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["price"]).ValueOrDie<string>());
                            }
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incSTR"))
                        {
                            eq.Strength = (short)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incSTR"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incDEX"))
                        {
                            eq.Dexterity = (short)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incDEX"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incLUK"))
                        {
                            try
                            {

                                eq.Luck = (short)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incLUK"]).Value;
                            }
                            catch (Exception ex)
                            {
                                eq.Luck = (short)(int.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incLUK"]).ValueOrDie<string>()));
                            }
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incINT"))
                        {
                            eq.Intellect = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incINT"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incMDD"))
                        {
                            eq.MagicDefense = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incMDD"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incPDD"))
                        {
                            try
                            {
                                eq.WeaponDefense = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incPDD"]).Value;
                            }
                            catch (Exception ex)
                            {
                                eq.WeaponDefense = (byte)(byte.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incPDD"]).ValueOrDie<string>()));
                            }
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incPAD"))
                        {
                            eq.WeaponAttack = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incPAD"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incMAD"))
                        {
                            eq.MagicAttack = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incMAD"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incSpeed"))
                        {
                            eq.Speed = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incSpeed"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incJump"))
                        {
                            eq.Jump = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incJump"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incACC"))
                        {
                            eq.Accuracy = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incACC"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incEVA"))
                        {
                            try
                            {
                                eq.Avoidance = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incEVA"]).Value;
                            }
                            catch (Exception ex)
                            {
                                eq.Avoidance = (byte)(byte.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incEVA"]).ValueOrDie<string>()));
                            }
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incMHP"))
                        {
                            eq.HP = (short)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incMHP"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("incMMP"))
                        {
                            eq.MP = (short)((NXValuedNode<Int64>)pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"]["incMMP"]).Value;
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("quest"))
                        {
                            UntradeableDrops.Add(eq.ID);
                        }
                        if (pFile.BaseNode["Character"][pNode.Name][mNode.Name]["info"].ContainsChild("only"))
                        {
                            UntradeableDrops.Add(eq.ID);
                        }
                        Equips.Add(eq.ID, eq);
                    }
                }
            }
        }

        static void ReadQuestData()
        {
            IEnumerator cEnumerator = pFile.ResolvePath("Quest/Check.img").GetEnumerator();
            while (cEnumerator.MoveNext())
            {

                NXNode cNode = (NXNode)cEnumerator.Current;
                IEnumerator pEnumerator = pFile.BaseNode["Quest"]["Check.img"][cNode.Name].GetEnumerator();
                Questdata qd = new Questdata();
                while (pEnumerator.MoveNext())
                {
                    NXNode stageNode = (NXNode)pEnumerator.Current;

                    if (stageNode.ContainsChild("mob"))
                    {
                        qd.Mobs = new List<QuestMob>();
                        IEnumerator enumerable = pFile.BaseNode["Quest"]["Check.img"][cNode.Name][stageNode.Name]["mob"].GetEnumerator();

                        while (enumerable.MoveNext())
                        {
                            NXNode mobNode = (NXNode)enumerable.Current;
                            QuestMob mob = new QuestMob();
                            mob.ReqKills = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Check.img"][cNode.Name][stageNode.Name]["mob"][mobNode.Name]["count"]).Value;
                            mob.MobID = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Check.img"][cNode.Name][stageNode.Name]["mob"][mobNode.Name]["id"]).Value;
                            qd.Mobs.Add(mob);
                        }
                    }
                }
                Quests.Add(int.Parse(cNode.Name), qd);
            }
              
            IEnumerator enumerator = pFile.ResolvePath("Quest/Act.img").GetEnumerator();
            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;
                IEnumerator pEnumerator = pFile.BaseNode["Quest"]["Act.img"][pNode.Name].GetEnumerator();
                Questdata qd = new Questdata();
                while (pEnumerator.MoveNext())
                {
                    NXNode iNode = (NXNode)pEnumerator.Current;
                    qd.Stage = byte.Parse(iNode.Name);

                    if (qd.Stage == 0)
                    {
                        if (iNode.ContainsChild("item"))
                        {
                            NXNode bNode = (NXNode)pEnumerator.Current;
                            IEnumerator sEnumerator = pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["item"].GetEnumerator();

                            while (sEnumerator.MoveNext())
                            {
                                NXNode lNode = (NXNode)sEnumerator.Current;
                                qd.ReqItems = new List<ItemReward>();
                                ItemReward ir = new ItemReward();
                                ir.ItemRewardCount = (short)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["item"][lNode.Name]["count"]).Value;
                                ir.Reward = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["item"][lNode.Name]["id"]).Value;
                                qd.ReqItems.Add(ir);

                            }
                        }
                    }
                    else
                    {
                        if (iNode.ContainsChild("item"))
                        {
                            NXNode bNode = (NXNode)pEnumerator.Current;
                            IEnumerator sEnumerator = pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["item"].GetEnumerator();
                            qd.ItemRewards = new List<ItemReward>();
                            qd.RandomRewards = new List<ItemReward>();
                            while (sEnumerator.MoveNext())
                            {
                                NXNode lNode = (NXNode)sEnumerator.Current;

                                if (lNode.ContainsChild("prop"))
                                {
                                    
                                    ItemReward ir = new ItemReward();
                                    ir.ItemRewardCount = (short)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["item"][lNode.Name]["count"]).Value;
                                    ir.Reward = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["item"][lNode.Name]["id"]).Value;
                                    qd.RandomRewards.Add(ir);
                                }
                                else
                                {
                                    
                                    ItemReward ir = new ItemReward();
                                    ir.ItemRewardCount = (short)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["item"][lNode.Name]["count"]).Value;
                                    ir.Reward = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["item"][lNode.Name]["id"]).Value;
                                    qd.ItemRewards.Add(ir);
                                }
                            }
                        }
                    }

                    if (iNode.ContainsChild("exp"))
                    {
                        qd.ExpReward = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["exp"]).Value;
                    }
                    if (iNode.ContainsChild("money"))
                    {
                        qd.MesoReward = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Quest"]["Act.img"][pNode.Name][iNode.Name]["money"]).Value;
                    }
                }

                if (pNode.Name == "1008")
                {
                    Console.WriteLine("MESO REWARD : " + qd.MesoReward);
                }
                if (Quests.ContainsKey(int.Parse(pNode.Name)))
                {
                    Quests[int.Parse(pNode.Name)].ExpReward = qd.ExpReward;
                    Quests[int.Parse(pNode.Name)].MesoReward = qd.MesoReward;
                    Quests[int.Parse(pNode.Name)].RandomRewards = qd.RandomRewards;
                    Quests[int.Parse(pNode.Name)].ItemRewards = qd.ItemRewards;
                    Quests[int.Parse(pNode.Name)].ReqItems = qd.ReqItems;
                }
                else
                {
                    Quests.Add(int.Parse(pNode.Name), qd);
                }
            }
        }

        static void ReadNpcs()
        {
            IEnumerator enumerator = pNpcFile.BaseNode.GetEnumerator();
            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;

                NPCData npc = new NPCData();
                int ID = (int)Utils.ConvertNameToID(pNode.Name);
                npc.ID = ID;
                if (pNpcFile.BaseNode[pNode.Name]["info"].ContainsChild("quest"))
                {
                    npc.Quest = (String)((NXValuedNode<String>)pNpcFile.BaseNode[pNode.Name]["info"]["quest"]).ValueOrDie<string>();
                }
                if (pNpcFile.BaseNode[pNode.Name]["info"].ContainsChild("trunk"))
                {
                    npc.Trunk = (Int32)((NXValuedNode<Int64>)pNpcFile.BaseNode[pNode.Name]["info"]["trunk"]).Value;
                }

                if (pNpcFile.BaseNode[pNode.Name]["info"].ContainsChild("shop"))
                {
                    npc.Shop = new List<ShopItemData>();
                    IEnumerator pEnumerator = pNpcFile.ResolvePath(pNode.Name + "/info").GetEnumerator();
                    while (pEnumerator.MoveNext())
                    {
                        NXNode mNode = (NXNode)pEnumerator.Current;
                        if (mNode.Name == "shop")
                        {
                            IEnumerator sEnumerator = pNpcFile.ResolvePath(pNode.Name + "/info/shop").GetEnumerator();
                            while (sEnumerator.MoveNext())
                            {
                                ShopItemData item = new ShopItemData();

                                NXNode iNode = (NXNode)sEnumerator.Current;
                                item.ID = (int)Utils.ConvertNameToID(iNode.Name);
                                if (pNpcFile.BaseNode[pNode.Name]["info"]["shop"][iNode.Name].ContainsChild("price"))
                                {
                                    item.Price = (Int32)((NXValuedNode<Int64>)pNpcFile.BaseNode[pNode.Name]["info"]["shop"][iNode.Name]["price"]).Value;
                                }
                                if (pNpcFile.BaseNode[pNode.Name]["info"]["shop"][iNode.Name].ContainsChild("stock"))
                                {
                                    item.Stock = (Int32)((NXValuedNode<Int64>)pNpcFile.BaseNode[pNode.Name]["info"]["shop"][iNode.Name]["stock"]).Value;
                                }
                                if (pNpcFile.BaseNode[pNode.Name]["info"]["shop"][iNode.Name].ContainsChild("unitPrice"))
                                {
                                    item.UnitRechargeRate = ((NXValuedNode<double>)pNpcFile.BaseNode[pNode.Name]["info"]["shop"][iNode.Name]["unitPrice"]).Value;
                                }
                                npc.Shop.Add(item);
                            }
                        }

                    }
                }
                else
                {
                    npc.Shop = new List<ShopItemData>();
                    //show 0
                }
                NPCs.Add(npc.ID, npc);
                foreach (KeyValuePair<int, NPCData> kvp in NPCs)
                {
                    if (kvp.Value.Shop.Count > 0)
                    {
                        //Console.WriteLine(kvp.Value.ID.ToString());
                    }
                }
            }
        }

        static void ReadItems()
        {
            IEnumerator enumerator = pFile.ResolvePath("Item").GetEnumerator();
            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;
                IEnumerator pEnumerator = pFile.ResolvePath("Item/" + pNode.Name).GetEnumerator();
                while (pEnumerator.MoveNext())
                {
                    NXNode mNode = (NXNode)pEnumerator.Current;
                    IEnumerator lEnumerator = pFile.ResolvePath("Item/" + pNode.Name + "/" + mNode.Name).GetEnumerator();
                    if (pNode.Name != "Pet") //Leave for seperate void
                    {
                        while (lEnumerator.MoveNext())
                        {
                            NXNode iNode = (NXNode)lEnumerator.Current;

                            ItemData item = new ItemData();
                            int ID = (int)Utils.ConvertNameToID(iNode.Name);
                            item.ID = ID;

                            if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name].ContainsChild("info"))
                            {
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("price"))
                                {
                                    //some strings, some ints, fuck
                                    try
                                    {
                                        item.Price = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["price"]).Value;
                                    }
                                    catch (Exception ex)
                                    {
                                        item.Price = int.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["price"]).ValueOrDie<string>());
                                    }
                                }
                                else
                                {
                                    //No price
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("cash"))
                                {
                                    item.Cash = true;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("slotMax"))
                                {
                                    item.MaxSlot = (ushort)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["slotMax"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("meso"))
                                {
                                    item.Mesos = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["meso"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("quest"))
                                {
                                    item.IsQuest = true;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("success"))
                                {
                                    item.ScrollSuccessRate = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["success"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("cursed"))
                                {
                                    item.ScrollCurseRate = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["cursed"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incSTR"))
                                {
                                    item.IncStr = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incSTR"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incDEX"))
                                {
                                    item.IncDex = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incDEX"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incLUK"))
                                {
                                    item.IncLuk = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incLUK"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incINT"))
                                {
                                    item.IncInt = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incINT"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incMHP"))
                                {
                                    item.IncMHP = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incMHP"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incMMP"))
                                {
                                    item.IncMMP = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incMMP"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incPAD"))
                                {
                                    //inc weapon att
                                    item.IncWAtk = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incPAD"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incMAD"))
                                {
                                    //inc magic att
                                    item.IncMAtk = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incMAD"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incPDD"))
                                {
                                    //inc weapon defense
                                    item.IncWDef = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incPDD"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incMDD"))
                                {
                                    item.IncMDef = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incMDD"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incACC"))
                                {
                                    item.IncAcc = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incACC"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incEVA"))
                                {
                                    item.IncAvo = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incEVA"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incJump"))
                                {
                                    item.IncJump = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incJump"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("incSpeed"))
                                {
                                    item.IncSpeed = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["incSpeed"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("rate"))
                                {
                                    item.Rate = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["rate"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("quest"))
                                {
                                    UntradeableDrops.Add(item.ID);
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("only"))
                                {
                                    UntradeableDrops.Add(item.ID);
                                }
                                item.RateTimes = new Dictionary<byte, List<KeyValuePair<byte, byte>>>();
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"].ContainsChild("time"))
                                {
                                    int pChildCount = pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["time"].ChildCount;

                                    IEnumerator sEnumerator = pFile.ResolvePath("Item/" + pNode.Name + "/" + mNode.Name + "/" + iNode.Name + "/info/time").GetEnumerator();
                                    while (sEnumerator.MoveNext())
                                    {
                                        NXNode lNode = (NXNode)sEnumerator.Current;

                                        string val = (string)((NXValuedNode<String>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["info"]["time"][lNode.Name]).ValueOrDie<string>();
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
                                        }
                                        if (!item.RateTimes.ContainsKey(dayid))
                                            item.RateTimes.Add(dayid, new List<KeyValuePair<byte, byte>>());

                                        item.RateTimes[dayid].Add(new KeyValuePair<byte, byte>(hourStart, hourEnd));
                                    }

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
                            if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name].ContainsChild("spec"))
                            {
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("moveTo"))
                                {
                                    item.MoveTo = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["moveTo"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("hp"))
                                {
                                    item.HP = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["hp"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("mp"))
                                {
                                    item.MP = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["mp"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("hpR"))
                                {
                                    //some strings some ints :/
                                    try
                                    {
                                        item.HPRate = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["hpR"]).Value;
                                    }
                                    catch (Exception ex)
                                    {
                                        item.HPRate = short.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["hpR"]).ValueOrDie<string>());
                                    }
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("mpR"))
                                {
                                    try
                                    {
                                        item.MPRate = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["mpR"]).Value;
                                    }
                                    catch (Exception ex)
                                    {
                                        item.MPRate = short.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["mpR"]).ValueOrDie<string>());
                                    }
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("speed"))
                                {
                                    item.Speed = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["speed"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("eva"))
                                {
                                    item.Avoidance = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["eva"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("acc"))
                                {
                                    item.Accuracy = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["acc"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("mad"))
                                {
                                    //magic attack
                                    item.MagicAttack = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["mad"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("pad"))
                                {
                                    //weapon attack
                                    item.WeaponAttack = (short)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["pad"]).Value;
                                }
                                if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"].ContainsChild("time"))
                                {
                                    //bufftime
                                    item.BuffTime = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["spec"]["time"]).Value;
                                }
                            }
                            else
                            {
                                //no spec, continue
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
                            if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name].ContainsChild("mob")) //summons
                            {
                                item.Summons = new List<ItemSummonInfo>();
                                int pChildCount = pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["mob"].ChildCount;
                                IEnumerator sEnumerator = pFile.ResolvePath("Item/" + pNode.Name + "/" + mNode.Name + "/" + iNode.Name + "/mob").GetEnumerator();
                                while (sEnumerator.MoveNext())
                                {
                                    ItemSummonInfo isi = new ItemSummonInfo();
                                    NXNode sNode = (NXNode)sEnumerator.Current;
                                    if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["mob"][sNode.Name].ContainsChild("id"))
                                    {
                                        try
                                        {
                                            isi.MobID = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["mob"][sNode.Name]["id"]).Value;
                                        }
                                        catch (Exception ex)
                                        {
                                            isi.MobID = int.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["mob"][sNode.Name]["id"]).ValueOrDie<string>());
                                        }
                                    }
                                    if (pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["mob"][sNode.Name].ContainsChild("prob"))
                                    {
                                        isi.Chance = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"][pNode.Name][mNode.Name][iNode.Name]["mob"][sNode.Name]["prob"]).Value;
                                    }
                                    item.Summons.Add(isi);
                                }

                            }
                            Items.Add(item.ID, item);
                        }
                    }
                }
            }
        }

        static void ReadSkills()
        {
            IEnumerator enumerator = pFile.ResolvePath("Skill").GetEnumerator();
            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;
                if (pNode.Name != "MobSkill.img")
                {
                    IEnumerator pEnumerator = pFile.ResolvePath("Skill/" + pNode.Name + "/skill").GetEnumerator();

                    while (pEnumerator.MoveNext())
                    {
                        NXNode mNode = (NXNode)pEnumerator.Current;
                        int SkillID = int.Parse(mNode.Name);
                        Skills.Add(SkillID, new Dictionary<byte, SkillLevelData>());
                        int levels = pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"].ChildCount;

                        IEnumerator lEnumerator = pFile.ResolvePath("Skill/" + pNode.Name + "/skill/" + mNode.Name + "/level").GetEnumerator();
                        while (lEnumerator.MoveNext())
                        {
                            NXNode iNode = (NXNode)lEnumerator.Current;

                            SkillLevelData sld = new SkillLevelData();

                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("x"))
                            {
                                sld.XValue = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["x"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("y"))
                            {
                                sld.YValue = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["y"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("attackCount"))
                            {
                                sld.HitCount = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["attackCount"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("mobCount"))
                            {
                                sld.MobCount = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["mobCount"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("time"))
                            {
                                sld.BuffTime = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["time"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("damage"))
                            {
                                sld.Damage = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["damage"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("range"))
                            {
                                sld.AttackRange = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["range"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("mastery"))
                            {
                                sld.Mastery = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["mastery"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("hp"))
                            {
                                sld.HPProperty = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["hp"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("mp"))
                            {
                                sld.MPProperty = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["mp"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("prop"))
                            {
                                sld.Property = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["prop"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("hpCon"))
                            {
                                sld.HPUsage = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["hpCon"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("mpCon"))
                            {
                                sld.MPUsage = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["mpCon"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("itemCon"))
                            {
                                sld.ItemIDUsage = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["itemCon"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("itemConNo"))
                            {
                                sld.ItemAmountUsage = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["itemConNo"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("bulletConsume"))
                            {
                                sld.BulletUsage = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["bulletConsume"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("moneyCon"))
                            {
                                sld.MesosUsage = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["moneyCon"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("speed"))
                            {
                                sld.Speed = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["speed"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("jump"))
                            {
                                sld.Jump = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["jump"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("eva"))
                            {
                                sld.Avoidability = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["eva"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("acc"))
                            {
                                try
                                {
                                    sld.Accurancy = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["acc"]).Value;
                                }
                                catch (Exception ex)
                                {
                                    sld.Accurancy = short.Parse((String)((NXValuedNode<String>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["acc"]).ValueOrDie<string>());
                                }
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("mad"))
                            {
                                //magic attack
                                sld.MagicAttack = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["mad"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("pad"))
                            {
                                //weapon attack
                                sld.WeaponAttack = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["pad"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("pdd"))
                            {
                                //weapon defense
                                sld.WeaponDefense = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name]["pdd"]).Value;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name].ContainsChild("elemAttr"))
                            {
                                string pbyte = (String)((NXValuedNode<String>)pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["elemAttr"]).Value;
                                switch (pbyte)
                                {
                                    case "i": sld.ElementFlags = 1; break;
                                    case "f": sld.ElementFlags = 2; break;
                                    case "s": sld.ElementFlags = 3; break;
                                    case "l": sld.ElementFlags = 4; break;
                                    case "h": sld.ElementFlags = 5; break;    
                                }
                                sld.ElementFlags = (byte)SkillElement.Ice;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("rb"))
                            {
                                Point pPoint = pFile.ResolvePath("Skill/" + pNode.Name + "/skill/" + mNode.Name + "/level/" + iNode.Name + "/rb").ValueOrDie<Point>();
                                sld.RBX = (short)pPoint.X;
                                sld.RBY = (short)pPoint.Y;
                            }
                            if (pFile.BaseNode["Skill"][pNode.Name]["skill"][mNode.Name]["level"][iNode.Name].ContainsChild("lt"))
                            {
                                Point pPoint = pFile.ResolvePath("Skill/" + pNode.Name + "/skill/" + mNode.Name + "/level/" + iNode.Name + "/lt").ValueOrDie<Point>();
                                sld.LTX = (short)pPoint.X;
                                sld.LTY = (short)pPoint.Y;
                            }
                            Skills[SkillID].Add(byte.Parse(iNode.Name), sld);
                        }

                    }
                }
                else
                {
                    //MobSkillData
                    IEnumerator eEnumerator = pFile.ResolvePath("Skill/" + pNode.Name).GetEnumerator();
                    while (eEnumerator.MoveNext())
                    {
                        NXNode eNode = (NXNode)eEnumerator.Current;
                        IEnumerator kEnumerator = pFile.ResolvePath("Skill/" + pNode.Name + "/" + eNode.Name + "/level").GetEnumerator();
                        while (kEnumerator.MoveNext())
                        {
                            NXNode sNode = (NXNode)kEnumerator.Current;
                            int SkillID = int.Parse(eNode.Name);
                            if (!MobSkills.ContainsKey((byte)SkillID))
                            {
                                MobSkills.Add((byte)SkillID, new Dictionary<byte, MobSkillLevelData>());
                            }
                            int Level = int.Parse(sNode.Name);
                            MobSkillLevelData msld = new MobSkillLevelData();
                            msld.SkillID = (byte)SkillID;
                            msld.Level = (byte)Level;
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("time"))
                            {
                                msld.Time = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["time"]).Value;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("mpCon"))
                            {
                                msld.MPConsume = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["mpCon"]).Value;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("x"))
                            {
                                msld.X = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["x"]).Value;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("y"))
                            {
                                msld.Y = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["y"]).Value;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("prop"))
                            {
                                msld.Prop = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["prop"]).Value;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("interval"))
                            {
                                msld.Cooldown = (short)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["interval"]).Value;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("lt"))
                            {
                                Point pPoint = pFile.ResolvePath("Skill/MobSkill.img/" + eNode.Name + "/level/" + sNode.Name + "/lt").ValueOrDie<Point>();
                                msld.LTX = (short)pPoint.X;
                                msld.LTY = (short)pPoint.Y;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("rb"))
                            {
                                Point pPoint = pFile.ResolvePath("Skill/MobSkill.img/" + eNode.Name + "/level/" + sNode.Name + "/rb").ValueOrDie<Point>();
                                msld.RBX = (short)pPoint.X;
                                msld.RBY = (short)pPoint.Y;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("hp"))
                            {
                                msld.HPLimit = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["hp"]).Value;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("limit"))
                            {
                                msld.SummonLimit = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["limit"]).Value;
                            }
                            if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild("summonEffect"))
                            {
                                Int32 Summoneffect = (Int32)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name]["summonEffect"]).Value;

                                int summons = sNode.ChildCount - 4;
                                msld.Summons = new List<int>();
                                for (int i = 0; i < summons; i++)
                                {
                                    if (pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name].ContainsChild(i.ToString()))
                                    {
                                        msld.Summons.Add((Int32)((NXValuedNode<Int64>)pFile.BaseNode["Skill"]["MobSkill.img"][eNode.Name]["level"][sNode.Name][i.ToString()]).Value);
                                    }
                                }
                            }
                            MobSkills[(byte)SkillID].Add((byte)msld.Level, msld);
                        }
                    }
                }
            }
        }

        static void ReadPets()
        {
            IEnumerator enumerator = pFile.ResolvePath("Item/Pet").GetEnumerator();
            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;
                PetData pd = new PetData();
                int ID = (int)Utils.ConvertNameToID(pNode.Name);
                pd.ItemID = ID;
                if (pFile.BaseNode["Item"]["Pet"][pNode.Name].ContainsChild("hungry"))
                {
                    pd.Hungry = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"]["Pet"][pNode.Name]["info"]["hungry"]).Value;
                }
                if (pFile.BaseNode["Item"]["Pet"][pNode.Name].ContainsChild("life"))
                {
                    pd.Life = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"]["Pet"][pNode.Name]["info"]["life"]).Value;
                }

                IEnumerator pEnumerator = pFile.ResolvePath("Item/Pet/" + pNode.Name + "/interact").GetEnumerator();
                int levels = pFile.BaseNode["Item"]["Pet"][pNode.Name]["interact"].ChildCount;
                while (pEnumerator.MoveNext())
                {
                    NXNode mNode = (NXNode)pEnumerator.Current;
                    PetReactionData prd = new PetReactionData();
                    pd.Reactions = new Dictionary<byte, PetReactionData>();
                    prd.ReactionID = byte.Parse(mNode.Name);
                    prd.Inc = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"]["Pet"][pNode.Name]["interact"][mNode.Name]["inc"]).Value;
                    prd.Prob = (byte)((NXValuedNode<Int64>)pFile.BaseNode["Item"]["Pet"][pNode.Name]["interact"][mNode.Name]["prob"]).Value;
                    pd.Reactions.Add(prd.ReactionID, prd);
                }
                Pets.Add(ID, pd);
            }
        }

        static void ReadDrops()
        {
            IEnumerator enumerator = pDropFile.ResolvePath("Reward.img").GetEnumerator();
            while (enumerator.MoveNext())
            {
                NXNode pNode = (NXNode)enumerator.Current;
                string dropper = pNode.Name;
                if (dropper.StartsWith("m"))
                {
                    string trimmed = dropper.Trim().StartsWith("m0") ? dropper.Trim().Replace("m0", "m") : dropper;
                    Drops.Add(trimmed, new List<DropData>());
                    short dropamount = (short)pNode.ChildCount;
                    IEnumerator pEnumerator = pDropFile.ResolvePath("Reward.img/" + pNode.Name).GetEnumerator();
                    while (pEnumerator.MoveNext())
                    {
                        NXNode iNode = (NXNode)pEnumerator.Current;
                        DropData dropdata = new DropData();
                        if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("money"))
                        {
                            dropdata.Mesos = (Int32)((NXValuedNode<Int64>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["money"]).Value;
                        }
                        if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("item"))
                        {
                            dropdata.ItemID = (Int32)((NXValuedNode<Int64>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["item"]).Value;
                        }
                        if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("min"))
                        {
                            dropdata.Min = (short)((NXValuedNode<Int64>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["min"]).Value;
                        }
                        if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("max"))
                        {
                            dropdata.Max = (short)((NXValuedNode<Int64>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["max"]).Value;
                        }
                        if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("premium"))
                        {
                            dropdata.Premium = true;
                        }
                        if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("prob"))
                        {
                            string prob = (string)((NXValuedNode<string>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["prob"]).Value;
                            string trimmedProb = prob.Trim().StartsWith("[R8]") ? prob.Trim().Replace("[R8]", "") : prob;
                            double dProb = double.Parse(trimmedProb);
                            dropdata.Chance = dProb;
                        }
                        Drops[trimmed].Add(dropdata);
                    }

                }
                if (dropper.StartsWith("r"))
                {
                    string trimmed = dropper.Trim().StartsWith("r000") ? dropper.Trim().Replace("r000", "r") : dropper;
                    Drops.Add(trimmed, new List<DropData>());

                    IEnumerator pEnumerator = pDropFile.ResolvePath("Reward.img/" + pNode.Name).GetEnumerator();
                    while (pEnumerator.MoveNext())
                    {
                        NXNode iNode = (NXNode)pEnumerator.Current;
                        DropData dropdata = new DropData();
  
                            if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("money"))
                            {
                                dropdata.Mesos = (Int32)((NXValuedNode<Int64>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["money"]).Value;
                            }
                            if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("item"))
                            {
                                dropdata.ItemID = (Int32)((NXValuedNode<Int64>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["item"]).Value;
                            }
                            if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("min"))
                            {
                                dropdata.Min = (short)((NXValuedNode<Int64>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["min"]).Value;
                            }
                            if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("max"))
                            {
                                dropdata.Max = (short)((NXValuedNode<Int64>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["max"]).Value;
                            }
                            if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("premium"))
                            {
                                dropdata.Premium = true;
                            }
                            if (pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name].ContainsChild("prob"))
                            {
                                string prob = (string)((NXValuedNode<string>)pDropFile.BaseNode["Reward.img"][pNode.Name][iNode.Name]["prob"]).Value;
                                string trimmedProb = prob.Trim().StartsWith("[R8]") ? prob.Trim().Replace("[R8]", "") : prob;
                                double dProb = double.Parse(trimmedProb);
                                dropdata.Chance = dProb; 
                            }
                            Drops[trimmed].Add(dropdata);
                    }
                }
            }
        }
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

public class ShopItemData
{
    public int ID { get; set; }
    public int Stock { get; set; }
    public int Price { get; set; }
    public double UnitRechargeRate { get; set; }
}

public class NPCData
{
    public int ID { get; set; }
    public string Quest { get; set; }
    public int Trunk { get; set; }
    public List<ShopItemData> Shop { get; set; }
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

public class MobSkillLevelData
{
    public byte SkillID { get; set; }
    public byte Level { get; set; }
    public short Time { get; set; }
    public short MPConsume { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public byte Prop { get; set; }
    public short Cooldown { get; set; }

    public short LTX { get; set; }
    public short LTY { get; set; }
    public short RBX { get; set; }
    public short RBY { get; set; }

    public byte HPLimit { get; set; }
    public byte SummonLimit { get; set; }
    public byte SummonEffect { get; set; }
    public List<int> Summons { get; set; }
}

public class MobSkillData
{
    public byte Level { get; set; }
    public byte SkillID { get; set; }
    public short EffectAfter { get; set; }
}

public class MobAttackData
{
    public byte ID { get; set; }
    public short MPConsume { get; set; }
    public short MPBurn { get; set; }
    public short SkillID { get; set; }
    public byte SkillLevel { get; set; }
}

public class MobData
{
    public int ID { get; set; }
    public byte Level { get; set; }
    public bool Boss { get; set; }
    public bool Undead { get; set; }
    public bool BodyAttack { get; set; }
    public int EXP { get; set; }
    public int MaxHP { get; set; }
    public int MaxMP { get; set; }
    public int HPRecoverAmount { get; set; }
    public int MPRecoverAmount { get; set; }
    public uint HPTagColor { get; set; }
    public uint HPTagBgColor { get; set; }
    public short Speed { get; set; }
    public byte SummonType { get; set; }
    public bool Flies { get; set; }
    public bool PublicReward { get; set; }
    public bool ExplosiveReward { get; set; }
    public List<int> Revive { get; set; }
    public Dictionary<byte, MobAttackData> Attacks { get; set; }
    public List<MobSkillData> Skills { get; set; }
    public DateTime ReviveTime { get; set; }
    public short XRevive { get; set; }
    public short YRevive { get; set; }
    public bool AllowRevive { get; set; }
    public int PDD { get; set; }
}

public class Questdata
{
    public byte Stage { get; set; } 
    public int ReqItem { get; set; }
    public int ItemReward { get; set; }
    public short ItemRewardCount { get; set; }
    public int MesoReward { get; set; }
    public int FameReward { get; set; }
    public int ExpReward { get; set; }
    public List<ItemReward> ReqItems { get; set; }
    public List<ItemReward> ItemRewards { get; set; }
    public List<ItemReward> RandomRewards { get; set; }
    public List<QuestMob> Mobs { get; set; }
}

public class QuestMob
{
    public int ReqKills { get; set; }
    public int KillsCount { get; set; }
    public int MobID { get; set; }
}

public class ItemReward
{
    public int Reward { get; set; }
    public int ItemRewardCount { get; set; }
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

public class DropData
{
    public int ItemID { get; set; }
    public int Mesos { get; set; }
    public short Min { get; set; }
    public short Max { get; set; }
    public bool Premium { get; set; }
    public double Chance { get; set; }
}

public class ReactorData
{
    public int ItemID { get; set; }
    public int MobID { get; set; }
    public bool Premium { get; set; }
    public double Chance { get; set; }
    public bool SpawnMob { get; set; }
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
