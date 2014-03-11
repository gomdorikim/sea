using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WvsBeta.Game;

namespace WvsBeta.Game.Events
{
    public class Event
    {
        public enum EventType
        {
            OlaOla,
            Snowball,
            CoconutHarvest
        }

        public EventType Type { get; private set; }
        public List<int> Maps { get; private set; }

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

        protected void Register()
        {
            EventManager.Instance.RegisterEvent(this);
        }

        protected virtual void OnUpdate(DateTime pNow) { }
        protected virtual bool IsRunning() { return false; }

        protected void ClearMaps()
        {
            foreach (int mapid in Maps)
            {
                // DataProvider.Maps[mapid].
            }
        }
    }

        public class EventStrings
        {
            //Maple Administrator messages
            public const string Snowball = "#e[Snowball]#n \r\n\r\nHey there, thank you for participating in our event game. Here's a little introduction about the game.\r\n\r\n[Snowball] is a game were participants compete with each other by pushing their snowball the furthest within the time limit. The time limit during this event is #b10 MINUTES. #kDuring this event, you won't be able to use long range or magic attacks, only short rage attacks can be used. You won't #bbe able to use skills like haste, teleport, or boost your speed using potions or items. #kAlso, do not get too near the snowball or you will be knock backed to start.\r\n\r\nIf you have any other questions regarding this event, feel free to ask during the game.";
            
            //Random Notices //Based on screenshots, not sure what time during the event they are supposed to be displayed though. 
            public const string SnowballNotice = "You'll be able to disrupt your opponent's snowball once you attack the snowman.";
            
        }


    }

