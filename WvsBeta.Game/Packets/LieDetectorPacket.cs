using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class LieDetectorPacket
    {
        //Used to test botters, basically a captcha almost but uses 2 numbers to be compared, even though the numbers are sent through a packet which can easily be logged lol
        public class LieDetector
        {
            public Character Tester { get; set; }
            public short FirstNum { get; set; }
            public short SecondNum { get; set; }
            public short BiggerNum { get; set; }

            public static Dictionary<int, LieDetector> Detectors = new Dictionary<int, LieDetector>();

            public LieDetector(short first, short second, int TestID, Character pTester)
            {
                Tester = pTester;
                FirstNum = first;
                SecondNum = second;
                if (FirstNum > SecondNum)
                    BiggerNum = FirstNum;
                else
                    BiggerNum = SecondNum;
                if (!Detectors.ContainsKey(TestID))    
                Detectors.Add(TestID, this);
            }
        }

        public static void HandleUseLieDetector(Character chr, Packet pPacket)
        {
            Random rd = new Random();
            //MessagePacket.SendNotice(pPacket.ToString(), chr);
            Character Victim = Server.Instance.GetCharacter(pPacket.ReadString());
            Character From = Server.Instance.GetCharacter(pPacket.ReadString());

            LieDetector detector = new LieDetector((short)rd.Next(0, 999), (short)rd.Next(0, 999), Victim.ID, From);
            if (Victim.PrimaryStats.HasTest == 0)
            {
                ShowLieDetectorTest(Victim, detector);
                Victim.PrimaryStats.HasTest = 1;
            }
            else
            {
                LieDetectorMessage(chr, 0x02); //Been tested before
            }
        }

        public static void AnswerLieDetector(Character chr, Packet pPacket)
        {
            LieDetector detector = LieDetector.Detectors[chr.ID];

            short entNum = pPacket.ReadShort();

            if (entNum == detector.BiggerNum)
            {
                LieDetectorPassed(chr);
                chr.AddMesos(5000);
            }
            else
            {
                LieDetectorFailed(chr);
                if (Server.Instance.CharacterList.ContainsValue(detector.Tester))
                {
                    detector.Tester.AddMesos(7000);
                    LieDetectorReward(chr);
                }
            }
        }

        public static void LieDetectorFailed(Character chr)
        {
            Packet pw = new Packet(0x20);
            pw.WriteByte(0x05);
            chr.sendPacket(pw);
        }

        public static void LieDetectorReward(Character chr)
        {
            Packet pw = new Packet(0x20);
            pw.WriteByte(0x07);
            chr.sendPacket(pw);
        }            

        public static void LieDetectorPassed(Character chr)
        {
            Packet pw = new Packet(0x20);
            pw.WriteByte(0x06);
            chr.sendPacket(pw);
        }

        public static void LieDetectorMessage(Character chr, byte Message)
        {
            Packet pw = new Packet(0x20);
            pw.WriteByte(Message);
            chr.sendPacket(pw);
        }

        public static void ShowLieDetectorTest(Character chr, LieDetector detector)
        {
            Packet pw = new Packet(0x20);
            pw.WriteByte(0x04);
            pw.WriteShort(detector.FirstNum);
            pw.WriteShort(detector.SecondNum);
            pw.WriteByte(1); //Restart timer or no
            chr.sendPacket(pw);
        }
    }
}
