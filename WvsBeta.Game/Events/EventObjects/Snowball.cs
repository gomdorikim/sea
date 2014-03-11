using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Game.Events
{

    public class Snowball
    {
        //Position
        public Pos pos { get; set; }
        public int IncrementFromStage = 0;
        public short Stage { get; set; }

        //Snowman stuff
        public int SnowmanHP { get; set; }
        public int LastSnowmanHit { get; set; }
        public static int Countdown = 30; //30 seconds, amount of time that the opponents snowball will be unhittable for when your team's snowman's HP reaches 0
        public static int CurrentTick { get; set; }
        public bool HasThread = false;

        //Snowball
        public int AllowableDamage { get; set; }
        public bool Winner = false;
        public Snowball Opponent { get; set; }

        public Snowball(int team)
        {
            switch (team)
            {
                case 0:
                    pos = new Pos();
                    pos.X = 310;
                    pos.Y = 155;
                break;
                case 1:
                    pos = new Pos();
                    pos.X = 310;
                    pos.Y = -84;
                    break;

            }
        }

        public static int Team(Character chr)
        {
            return chr.Position.X > -80 ? 0 : 1;
        }

        public void HealSnowmanRunnable()
        {
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "HealSnowman",
                (date) => { HealSnowman(); },
                0, 1 * 1000)); 
        }

        public void HealSnowman()
        {
            Countdown = Countdown - 1;
            CurrentTick = Countdown;
            Opponent.AllowableDamage = 0;
            MessagePacket.SendNoticeMap("CurrentTick : " + CurrentTick, MapleSnowball.sMap);
            if (Countdown == 0)
            {
                this.SnowmanHP = 7500;
                Opponent.AllowableDamage = 10;
                MessagePacket.SendNoticeMap("Snowman's HP is back to 7500!", MapleSnowball.sMap);
                MasterThread.Instance.RemoveRepeatingAction("HealSnowman", (date, name, removed) => { /*MasterThread.Instance._performanceLog.WriteLine("RemoveRepeatingAction Callback: Date: {0}; Name: {1}; Removed: {2}", date, name, removed);*/ });
            }
        }

        

        public void CalculateSnowmanDamage(Character chr, byte up)
        {
            Random rd = new Random();
            double next = rd.NextDouble();
            {
                if (next < 0.3)
                {
                    if (SnowmanHP > 0)
                    {
                        SnowmanHP = SnowmanHP -45;
                        SnowballPackets.HitSnowman(chr, up, 45, (short)0x5E);
                        MapPacket.SendChatMessage(chr, "Snowman's HP : " + SnowmanHP);
                    }
                    else
                    {
                        if (!HasThread)
                            HasThread = true;
                        HealSnowmanRunnable();
                    }
                }
                else
                {
                    if (SnowmanHP > 0)
                    {
                        SnowmanHP = SnowmanHP - 15;
                        SnowballPackets.HitSnowman(chr, up, 15, (short)0x5E);
                        MapPacket.SendChatMessage(chr, "Snowman's HP : " + SnowmanHP);
                    }
                    else
                    {
                        if (!HasThread)
                            HasThread = true;
                        HealSnowmanRunnable();
                        
                    }
                }
            }
        }
    }
}




