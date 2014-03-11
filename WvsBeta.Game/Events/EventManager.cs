//Author: Vanlj95
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WvsBeta.Game;
using WvsBeta.Game.Events.EventObjects;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;
using MySql.Data.MySqlClient;

namespace WvsBeta.Game.Events
{
    /// <summary>
    ///A manager to organize gMS-Like events such as Ola Ola, Coconut Harvest, Snowball, etc.
    /// </summary>
    
    public class EventManager
    {
        public static EventManager Instance { get; set; }
        public static void Load()
        {
            Instance = new EventManager();
            RegisterShipEvents();

        }

        public Dictionary<Event.EventType, Event> RegisteredEvents { get; private set; }
        public Dictionary<PartyQuest.QuestType, PartyQuest> RegisteredQuests { get; set; }
        public Dictionary<ShipType, Ship> SceduledShipEvents { get; set; }
        public Dictionary<ShipType, Ship> RegisteredShips { get; set; }

        public EventManager()
        {
            Console.WriteLine("[EventManager] Initialized!");
            RegisteredEvents = new Dictionary<Event.EventType, Event>();
            RegisteredQuests = new Dictionary<PartyQuest.QuestType, PartyQuest>();
            SceduledShipEvents = new Dictionary<ShipType, Ship>();
            RegisteredShips = new Dictionary<ShipType, Ship>();
            //ReadSceduledEvents();
        }

        public void RegisterEvent(Event pEvent)
        {
            RegisteredEvents.Add(pEvent.Type, pEvent);


            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                pEvent.Type.ToString(),
                (date) => { CheckEvent(pEvent); },
                0, 1 * 1000));
        }

        public void UnregisterEvent(Event.EventType Type)
        {

        }

        public void CheckEvent(Event pEvent)
        {
            switch (pEvent.Type)
            {
                case Event.EventType.KerningCity:
                    {
                        pEvent.CurrentTicks++;
                        Console.WriteLine("KPQ Current ticks! : " + pEvent.CurrentTicks);
                        break;
                    }
            }
        }

        public static void RegisterShipEvents()
        {
            Ship OrbisShip = new Ship(ShipType.ToOrbis);
            Ship LudiShip = new Ship(ShipType.ToLudi);
            OrbisShip.SceduleShipEvent();
            LudiShip.SceduleShipEvent();


            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Orbis-Ellinia",
                (date) => { CheckShip(OrbisShip); },
                0, 1 * 1000));

            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Ossyria-Ludi",
                (date) => { CheckShip(LudiShip); },
                0, 1 * 1000)); 
        }

        public static void CheckShip(Ship pShip)
        {
            if (DateTime.Now <= pShip.DockCloseTime && pShip.AllowEntry && !pShip.Docked)
            {
                pShip.Docked = true;
                pShip.SendShipUpdate(2, pShip.GetDockMapID());

                pShip.SceduleShipEvent();
            }
            if (DateTime.Now >= pShip.DockCloseTime && pShip.AllowEntry)
            {
                pShip.AllowEntry = false;
                pShip.RegisterShip();
                
                Instance.SceduledShipEvents.Remove(pShip._Type);
            }
            if  (DateTime.Now >= pShip.DockDepartTime && !pShip.AllowEntry && !pShip.Moving)
            {
                pShip.SendShipUpdate(3, pShip.GetDockMapID());
                pShip.Docked = false;
                pShip.Moving = true;
                foreach (Character Passenger in pShip.Passengers)
                {
                    Passenger.ChangeMap(pShip.GetMovingMap());
                }
            }
            if (DateTime.Now >= pShip.BalrogSpawnTime && !pShip.HasSpawnBalrog && (pShip._Type == ShipType.ToOrbis || pShip._Type == ShipType.ToEllinia) && pShip.Moving)
            {
                Random SpawnChance = new Random();
                double Chance = SpawnChance.NextDouble();

                if (Chance < Ship.BalrogSpawnChance)
                {
                    pShip.HasSpawnBalrog = true;
                    switch (pShip._Type)
                    {
                        case ShipType.ToOrbis:
                            {
                                DataProvider.Maps[Ship.Ellinia_Onboard].spawnMob(8150000, new Pos(500, -500), 0, null, 1);
                                DataProvider.Maps[Ship.Ellinia_Onboard].spawnMob(8150000, new Pos(500, -500), 0, null, 1);
                                break;
                            }
                        case ShipType.ToEllinia:
                            {
                                DataProvider.Maps[Ship.Orbis_Onboard].spawnMob(8150000, new Pos(500, -500), 0, null, 1);
                                DataProvider.Maps[Ship.Orbis_Onboard].spawnMob(8150000, new Pos(500, -500), 0, null, 1);
                                break;
                            }
                    }
                }
            }

            if (DateTime.Now >= pShip.ArrivalTime && !pShip.Arrived)
            {
                foreach (Character Passenger in pShip.Passengers)
                {
                    Passenger.ChangeMap(pShip.GetArrivalMap());
                }
                pShip.Arrived = true;
                Instance.RegisteredShips.Remove(pShip._Type);
                
                //Re Run!
                pShip.NextRun();
            }
            
        }

        public void ReadSceduledEvents()
        {
            using (MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM events WHERE channel = '" + (Server.Instance.ID + 1) + "'") as MySqlDataReader)
            {

            if (!data.HasRows)
            {
                Console.WriteLine("[EventManager] No Sceduled Events!");
            }
            else
            {
                data.Read();
                string EventName = data.GetString("name");
                bool AdminOrganized = data.GetBoolean("gm");
                string AdminName = data.GetString("gmname");
                DateTime Sceduled = data.GetDateTime("sceduledtime");
                Event.EventType Type = (Event.EventType)data.GetByte("eventtype");
                //todo : read sceduled events from database

                DateTime now = DateTime.Now;
                
                //todo : if (event is less or equal to 72 hours away from current date)
                //make a masterthread check that loads all event values (messages, dictionaries, etc for the exact start date)
                //masterthread will check on the function CheckSceduledEvents
            }
            }
        }

        public void CheckSceduledEvents()
        {
            //todo : if time for event && gm is online

            //generate random message for the event
            //open access to all NPC's 
            //send a message to all gameservers
            //make a list for all characters
        }
        public void AddEventLog()
        {
            if (SceduledShipEvents.Count > 0)
            {
                foreach (KeyValuePair<ShipType, Ship> kvb in SceduledShipEvents)
                {
                    //Console.WriteLine(string.Format("[EventManager] Current Ship Scedule : " + kvb.Value.
                }
            }
        }
    }
}