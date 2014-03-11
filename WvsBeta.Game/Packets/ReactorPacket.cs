using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    class ReactorPacket
    {

        public static void HandleReactorChangeState(Character chr, Packet packet)
        {
            //Header : 6B 
            //[INT, ID]
            //[SHORT] : swing from
            //[SHORT] : jump or no
            //[SHORT] : Stance
            int ID = packet.ReadInt();
            short SwingFrom = packet.ReadShort(); //used for.. ?
            short Jump = packet.ReadShort(); //used for.. ?
            short Stance = packet.ReadShort();

            //MessagePacket.SendNotice(ID.ToString(), chr);
            if (DataProvider.Maps[chr.Map].Reactors.ContainsKey(ID))
            {
                Reactor Reactor = DataProvider.Maps[chr.Map].Reactors[ID];
                if (Reactor != null)
                {
                    if (Reactor.State < 4) 
                    {
                        Reactor.State++;
                        ChangeReactorStateTest(Reactor, chr, Reactor.X, Reactor.Y, Reactor.State, Stance);
                        Reactor.DestroyTime = DateTime.Now;
                    }
                    else if (Reactor.State == 4)
                    {
                        //MessagePacket.SendNotice("state 4", chr);
                        Reactor.Destroyed = true;
                        //ChangeReactorStateTest(chr, Reactor.X, Reactor.Y, Reactor.State, Stance);
                        //DestroyReactor(Reactor, DataProvider.Maps[chr.Map], Reactor.X, Reactor.Y);
                        //Reactor.State = 0;
                        //Reactor.DestroyTime = DateTime.Now;
                        //Reactor.AllowSpawn = false;
                       // Reactor.AllowDestroy = true;
                    }
                }
            }
        }

        public static void HandleReactorAction(Reactor reactor, Character chr)
        {
            
        }
        public static void SpawnReactor(Reactor reactor, bool toall, Map map, Character chr = null)
        {
            Packet packet = new Packet(0xA1);
            packet.WriteInt(reactor.ID); //ReactorID
            packet.WriteInt(reactor.ReactorID); //ObjectID
            packet.WriteByte(reactor.State); //State
            packet.WriteShort(reactor.X); //X Position
            packet.WriteShort(reactor.Y); //Y Position
            packet.WriteByte(2); //?
            if (toall)
            {
                DataProvider.Maps[map.ID].SendPacket(packet);
            }
            else
            {
                chr.sendPacket(packet);
            }
        }

        public static void ChangeReactorState(Reactor reactor, Character chr, short stance)
        {
            Packet packet = new Packet(0x9F);
            packet.WriteInt(reactor.ID);
            packet.WriteByte(reactor.State); //State
            packet.WriteShort(reactor.X);
            packet.WriteShort(reactor.Y);
            packet.WriteShort(stance);
            packet.WriteByte(0);
            packet.WriteByte(5); //Frame delay ?
            chr.sendPacket(packet);
        }

        public static void ChangeReactorStateTest(Reactor reactor, Character chr, short X, short Y, byte State, short Stance)
        {
            if (State == 4)
            {
                List<DropData> Drops;
                Console.WriteLine("reactor ID : " + reactor.ReactorID);
                if (reactor.ReactorID == 2000)
                {
                    Pos pos = new Pos();
                    pos.X = reactor.X;
                    pos.Y = (short)(reactor.Y - 10);
                    Drops = DataProvider.Drops["r2000"];
                    DropPacket.HandleDrops(chr, chr.Map, "r2000", reactor.ReactorID, chr.Position, false, false, true);
                    reactor.Destroyed = true;
                }
                else
                {
                    Drops = DataProvider.Drops["r" + reactor.ReactorID.ToString()];
                }

                
            }
            Packet packet = new Packet(0x9F);
            packet.WriteInt(reactor.ID);
            packet.WriteByte(State); //State
            packet.WriteShort(X);
            packet.WriteShort(Y);
            packet.WriteShort(Stance);
            packet.WriteByte(0);
            packet.WriteByte(5); //Frame delay ?
            chr.sendPacket(packet);
        }

        public static void DestroyReactor(Reactor reactor, Map map, short X, short Y)
        {
            Packet packet = new Packet(0xA2);
            packet.WriteInt(reactor.ID);
            packet.WriteByte(3); //Always 3 ?
            packet.WriteShort(X);
            packet.WriteShort(Y);
            DataProvider.Maps[map.ID].SendPacket(packet);
        }
    }
}
