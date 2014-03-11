using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game.Events
{
    public class EventRoom
    {
        //To be handled in Event.cs
        public static List<Character> TeamStory = new List<Character>();
        public static List<Character> TeamMaple = new List<Character>();

        public static void CheckTeamBeforeWarp(Character chr)
        {
            if (TeamStory.Count > TeamMaple.Count)
            {
                TeamMaple.Add(chr);
            }
            else
            {
                TeamStory.Add(chr);
            }
        }
    }
}
