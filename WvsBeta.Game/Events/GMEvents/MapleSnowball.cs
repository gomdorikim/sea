using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;

namespace WvsBeta.Game.Events
{
    class MapleSnowball
    {
        //Maps and Stages
        public const int sMap = 109060000;
        public const int wMap = 109060001;
        public const short FirstStage = 350;
        public const short SecondStage = 1090;
        public const short ThirdStage = 1857;
        public const short LastStage = 2626;
        public const short Finished = 3426;
        
        //Teams
        public const short Bottom = 154;
        public const short Top = -84;
        public static Snowball snowball0 { get; set; }
        public static Snowball snowball1 { get; set; }
        public static List<Character> Winners { get; set; }

        //Time and Rules
        public static int Countdown = 600; //10 Minutes
        public static int CurrentTick { get; set; }

        //Position
        public Pos pos { get; set; }
        public static Map map = DataProvider.Maps[sMap];
        public static Portal top = map.Portals["st01"];
        public static Portal bottom = map.Portals["st00"];

        public static bool EventOn = false;

        public MapleSnowball(Snowball team0, Snowball team1)
        {
            team0.Opponent = team1;
            team1.Opponent = team0;
            team0.AllowableDamage = 10;
            team1.AllowableDamage = 10;
            team0.SnowmanHP = 7500;
            team1.SnowmanHP = 7500;
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "GM Event : MapleSnowball",
                (date) => { NewMapleSnowball(team0, team1); },
                0, 1 * 1000));
                
        }

        public static void NewMapleSnowball(Snowball team0, Snowball team1)
        {
            Countdown = Countdown - 1;
            CurrentTick = Countdown;
            if (CurrentTick == 0) //Times up!
            {
                if (team0.pos.X > team1.pos.X)
                {
                    foreach (Character winner in EventRoom.TeamStory)
                    {
                        Winners = new List<Character>();
                        Winners.Add(winner);
                        MapPacket.SendChatMessage(winner, "Snowball position X : " + team0.pos.X);
                        MapPacket.MapEffect(winner, 4, "Coconut/Victory", true);
                        MapPacket.MapEffect(winner, 3, "event/coconut/victory", true);
                        SnowballPackets.RollSnowball(winner, 3, 0, 0, 0, 0);

                    }
                    foreach (Character loser in EventRoom.TeamMaple)
                    {
                        MapPacket.MapEffect(loser, 4, "Coconut/Failed", true);
                        MapPacket.MapEffect(loser, 3, "event/coconut/lose", true);
                    }

                }
            }
            if (CurrentTick == 150) //random notice based on Screenshots I found from 2005
            {
                MessagePacket.SendNoticeMap(EventStrings.SnowballNotice, MapleSnowball.sMap);
            } 
        }

        public static int AmountPlayersOnBottom()
        {
            int amount = 0;
            foreach(Character player in DataProvider.Maps[sMap].Characters)
            {
               
                if (player.Position.Y >= 94 && player.Position.Y <= Bottom)
                {
                    amount++;
                }
                
            }
            return amount;
        }

        public static int AmountPlayersOnTop()
        {
            int amount = 0;
            foreach (Character player in DataProvider.Maps[sMap].Characters)
            {
                if (player.Position.Y >= -146 && player.Position.Y <= Top)
                {
                    amount++;
                }
            }
            return amount;
        }

        public static void WarpPlayersDivide()
        {
            foreach (Character story in EventRoom.TeamStory)
            {
                story.ChangeMap(sMap, bottom);
            }
            foreach (Character maple in EventRoom.TeamMaple)
            {
                maple.ChangeMap(sMap, top);
            }
        }

        public static bool NearSnowman(Character chr)
        {
            if (chr.Position.X > -544 && chr.Position.X < -330)
            {
                if (chr.Position.X > -544 && chr.Position.X < -400 && chr.isFacingRight()) //Left side of snowman
                {
                    return true;
                }
                if (chr.Position.X > -400 && chr.Position.X < -330 && !chr.isFacingRight()) //Right side of snowman
                {
                    return true;
                }
            }
            return false;
        }
       
    }
}
