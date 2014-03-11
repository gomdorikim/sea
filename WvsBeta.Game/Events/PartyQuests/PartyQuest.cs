using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Game;

namespace WvsBeta.Game.Events
{
    public class PartyQuest
    {
        public int MaxTicks { get; set; }
        public int CurrentTicks { get; set; }

        public int mExit { get; set; }
        public int mBonus { get; set; }
        public bool Open { get; set; }

        public int[] Maps = new int[] { 103000805, 103000804, 103000803, 103000802, 103000801, 103000800 };

        public enum QuestType
        {
            Kerning,
            Ludi,
            Zakum
        }

        public static void RegisterPartyQuest(QuestType Type)
        {
            PartyQuest pq = new PartyQuest(Type);

            switch (Type)
            {
                case QuestType.Kerning:
                    {
                        pq.MaxTicks = 15;
                        pq.CurrentTicks = 0;
                        pq.mExit = 103000890;
                        pq.mBonus = 103000805;
                        pq.Open = true;
                        break;
                    }
            }

            pq.Start(Type);
        }

        public PartyQuest(QuestType Type)
        {
            EventManager.Instance.RegisteredQuests.Add(Type, this);
        }

        public bool CheckPlayers(Character pLeader)
        {
            int Count = 0;

                foreach (Character pMember in DataProvider.Maps[pLeader.Map].Characters)
                {
                    if (pMember.PartyID == pLeader.PartyID)
                    {
                        Count++;
                    }
                }

            if (Count == 2)
            {
                return true;
            }
            return false;
        }

        public bool CheckLevels(Character pLeader)
        {
            foreach (int i in this.Maps)
            {
                foreach (Character pMember in DataProvider.Maps[i].Characters)
                {
                    if (pMember.PartyID == pLeader.ID)
                    {
                        if (pMember.PrimaryStats.Level < 21 || pMember.PrimaryStats.Level > 30)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public void Start(QuestType Type)
        {
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                Type.ToString(),
                (date) => { Watch(Type); },
                0, 1 * 1000));

            StartPQ(Type);
        }

        public void Watch(QuestType type)
        {
            switch (type)
            {
                case QuestType.Kerning:
                    {
                        CurrentTicks++;

                        if (CurrentTicks >= MaxTicks)
                        {
                            foreach (int i in Maps)
                            {
                                foreach (Character pMember in DataProvider.Maps[i].Characters)
                                {
                                    pMember.ChangeMap(this.mExit);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        public void StartPQ(QuestType Type)
        {
            switch (Type)
            {
                case QuestType.Kerning:
                    {
                        foreach (Character pMember in DataProvider.Maps[103000000].Characters)
                        {
                            pMember.ChangeMap(103000800);
                            MapPacket.MapTimer(pMember, this.MaxTicks);
                        }
                        break;
                    }
            }
        }
    }
}
