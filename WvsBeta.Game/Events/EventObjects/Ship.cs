using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events.EventObjects
{
    public enum ShipType
    {
        ToOrbis,
        ToEllinia,
        ToLudi,
        ToOssyria,
    }
    
    public class Ship
    {
        public const int Orbis_MainStation = 200000100;
        public const int Orbis_Station = 200000111;
        public const int Orbis_Prepare = 200000112;
        public const int Orbis_Onboard = 200090000;
        public const int Orbis_Cabin = 200090001;

        public const int Ellinia_Station = 101000300;
        public const int Ellinia_Prepare = 101000301;
        public const int Ellinia_Onboard = 200090010;
        public const int Ellinia_Cabin = 200090011;

        public const double BalrogSpawnChance = 0.7;

        public ShipType _Type { get; set; }
        public DateTime Time { get; set; }
        public DateTime DockCloseTime { get; set; }
        public DateTime DockDepartTime { get;set; }
        public DateTime BalrogSpawnTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public List<Character> Passengers { get; set; }
        public bool HasSpawnBalrog { get; set; }
        public bool Docked { get; set; }
        public bool AllowEntry { get; set; }
        public bool Moving { get; set; }
        public bool Arrived { get; set; }

        public ShipType LastRun { get; private set; }
        public Ship(ShipType type)
        {
            _Type = type;
            Passengers = new List<Character>();
            HasSpawnBalrog = false;
            Docked = false;
            Moving = false;
            AllowEntry = true;
            Arrived = false;
            Time = DateTime.Now;
            DockCloseTime = Time.AddMinutes(7);
            DockDepartTime = Time.AddMinutes(10);
            BalrogSpawnTime = Time.AddMinutes(13);
            ArrivalTime = Time.AddMinutes(30);

            LastRun = this._Type;
        }

        public static Ship GetDockedShip(int MapID)
        {
            switch (MapID)
            {
                case 200000111:
                    {
                        if (EventManager.Instance.SceduledShipEvents.ContainsKey(ShipType.ToEllinia))
                            return EventManager.Instance.SceduledShipEvents[ShipType.ToEllinia];
                        break;
                    }
                case 101000300:
                    {
                        if (EventManager.Instance.SceduledShipEvents.ContainsKey(ShipType.ToOrbis))
                            return EventManager.Instance.SceduledShipEvents[ShipType.ToOrbis];
                        break;
                    }
                case 200000121:
                    {
                        if (EventManager.Instance.SceduledShipEvents.ContainsKey(ShipType.ToLudi))
                            return EventManager.Instance.SceduledShipEvents[ShipType.ToLudi];
                        break;
                    }
            }
            return null;
        }

        public static Ship GetRegisteredShip(int MapID)
        {
            switch (MapID)
            {
                case 200000111:
                    {
                        if (EventManager.Instance.RegisteredShips.ContainsKey(ShipType.ToEllinia))
                            return EventManager.Instance.RegisteredShips[ShipType.ToEllinia];
                        break;
                    }
                case 101000300:
                    {
                        if (EventManager.Instance.RegisteredShips.ContainsKey(ShipType.ToOrbis))
                            return EventManager.Instance.RegisteredShips[ShipType.ToOrbis];
                        break;
                    }
                case 200000121:
                    {
                        if (EventManager.Instance.RegisteredShips.ContainsKey(ShipType.ToLudi))
                            return EventManager.Instance.RegisteredShips[ShipType.ToLudi];
                        break;
                    }
            }
            return null;
        }

        public void SceduleShipEvent()
        {
            if (EventManager.Instance.SceduledShipEvents.ContainsKey(this._Type))
            {
                //Console.WriteLine("[EventManager] A ship with this key type already exists in registered events!");
                return;
            }
            else
            {
                try
                {
                    EventManager.Instance.SceduledShipEvents.Add(this._Type, this);
                    Console.WriteLine("[EventManager] New Event Scedule! : Ship with type " + this._Type + " is docked.");
                    Console.WriteLine("[EventManager] Ship with type " + this._Type + " will be departing at " + DockDepartTime.TimeOfDay);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public void RegisterShip()
        {
            if (EventManager.Instance.RegisteredShips.ContainsKey(this._Type))
            {
                Console.WriteLine("[EventManager] A ship with this key type already exists in registered events!");
            }
            else
            {
                EventManager.Instance.RegisteredShips.Add(this._Type, this);
            }
        }

        public void RegisterPassenger(Character Passenger)
        {
            Passengers.Add(Passenger);
        }

        public void UnregisterPassenger(Character Passenger)
        {
            Passengers.Remove(Passenger);
        }

        public void SendShipUpdate(byte Type, int MapID)
        {
            foreach (Character Passenger in DataProvider.Maps[MapID].Characters)
            {
                MapPacket.SendBoat(Passenger, Type);
            }
        }

        public int GetDockMapID()
        {
            if (this._Type == ShipType.ToEllinia) { return 200000111; }
            if (this._Type == ShipType.ToOrbis) { return 101000300; }
            if (this._Type == ShipType.ToLudi) { return 200000121; }
            //if (this.Type == ShipType.ToOssyria) { return 
            return 0;
        }

        public int GetMovingMap()
        {
            if (this._Type == ShipType.ToEllinia) { return 200090000; }
            if (this._Type == ShipType.ToOrbis) { return 200090010; }
            return 0;
        }

        public int GetArrivalMap()
        {
            if (this._Type == ShipType.ToEllinia) { return 101000300; }
            if (this._Type == ShipType.ToOrbis) { return 200000111; }
            if (this._Type == ShipType.ToOssyria) { return 200000121; }
            //if (this.Type == ShipType.ToLudi) { return
            return 0;
        }

        public ShipType GetNextRun(ShipType LastRun)
        {
            if (LastRun == ShipType.ToOrbis) { return ShipType.ToEllinia; }
            if (LastRun == ShipType.ToEllinia) { return ShipType.ToOrbis;}
            return LastRun;
        }

        public void NextRun()
        {
            Passengers.Clear();
            DataProvider.Maps[this.GetMovingMap()].KillAllMobs(this.GetMovingMap(), false);
            DataProvider.Maps[this.GetMovingMap()].ClearDrops();

            _Type = GetNextRun(LastRun);
            HasSpawnBalrog = false;
            Docked = false;
            Moving = false;
            AllowEntry = true;
            Arrived = false;
            Time = DateTime.Now;
            DockCloseTime = Time.AddMinutes(7);
            DockDepartTime = Time.AddMinutes(10);
            BalrogSpawnTime = Time.AddMinutes(13);
            ArrivalTime = Time.AddMinutes(30);

            LastRun = this._Type;
            
        }
    }
}
