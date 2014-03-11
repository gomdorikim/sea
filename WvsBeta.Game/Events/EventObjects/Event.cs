using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Game;
using WvsBeta.Game.Events.EventObjects;

namespace WvsBeta.Game.Events
{
    public class Event
    {
        public enum EventType
        {
            OlaOla,
            Snowball,
            CoconutHarvest,
            FindTheJewel,
            KerningCity
        }

        public int MaxUsers { get; set; }
        public int Time { get; set; }
        public EventType Type { get; private set; }
        public List<int> Maps { get; private set; }
        public Dictionary<int, Reactor> Reactors { get; private set; }
        public Character Winner { get; private set; }

        public List<Character> TeamMaple { get; private set; }
        public List<Character> TeamStory { get; private set; }

        public int MaxTicks { get; set; }
        public int CurrentTicks { get; set; }

        public IEnumerable<Character> GetParticipants(Func<Character, bool> pPredicate)
        {
            foreach (int mapid in Maps)
                foreach (Character ch in DataProvider.Maps[mapid].Characters)
                    if (pPredicate(ch))
                        yield return ch;
        }

        public Event(EventType pType)
        {
            Type = pType;
            Maps = new List<int>();
        }

        protected virtual void Register()
        {
            EventManager.Instance.RegisterEvent(this);
        }

        protected virtual int GetTime(DateTime Time) { return Time.Second; }

        protected virtual void InitializeVariables() { Console.WriteLine("No way!" + Maps.Count ); }
        protected virtual void OnUpdate(DateTime pNow) { }
        protected virtual bool IsRunning() { return false; }

        protected void ClearMaps()
        {
            foreach (int mapid in Maps)
            {

            }
        }
    }

    public class EventStrings
    {

        //Random Notices //Based on screenshots, not sure what time during the event they are supposed to be displayed though. 
        public const string SnowballNotice = "You'll be able to disrupt your opponent's snowball once you attack the snowman.";

    }
}
