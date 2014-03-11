using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class CharacterCheatInspector
    {
        public Character mCharacter { get; set; }
        public DateTime LastSent { get; set; }
        public DateTime LastAttack { get; set; }
        public DateTime LastMoved { get; set; }
        public int MissCount { get; set; }

        public CharacterCheatInspector(Character chr)
        {
            mCharacter = chr;
            LastSent = DateTime.Now;
            LastAttack = DateTime.Now;
            MissCount = 0;
        }

        public void HandleMiss()
        {
            this.MissCount++;
            if (this.MissCount > 6)
            {
                ReportManager.FileNewReport("", this.mCharacter.ID, 5, "", MissCount);
            }
        }

        public void ResetMisses()
        {
            this.MissCount = 0;
        }
    }
    class CheatInspector
    {

        public static bool CheckSpeed(Pos PixelsPerSecond, float pAllowedSpeed)
        {
            float test = Math.Abs(PixelsPerSecond.X);
            float speedMod = Math.Abs(PixelsPerSecond.X) / 125f;
            return speedMod < pAllowedSpeed + 0.1f;
        }

        public static void CalculatePixelsPerSecond()
        {
         
        }

        public static bool CheckTextSpam(string text) //Unlimited text hacks
        {
            return (text.Length > 138);
        }

        public static bool CheckSpam(CharacterCheatInspector cci, DateTime Now)
        {
            DateTime NextAllowedSend = cci.LastSent.AddMilliseconds(10);
            if (Now < NextAllowedSend)
            {
                return true;
            }
            else
            {
                return false;
            }
            return false;
        }

        public static void CheckSuspiciousText(Character chr, string text)
        {
            text = text.ToLower();
            StringBuilder sb = new StringBuilder();

            foreach (char c in text)
            { 
                if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !CheckForUsedChars(c))
                {
                    sb.Append(c.ToString());
                }
            }

            for (int i = 0; i < Constants.Suspicious.Length; i++)
            {
                if (sb.ToString().Contains(Constants.Suspicious[i]))
                {
                    ReportManager.FileNewReport(Constants.Suspicious[i], chr.ID, 4, text);
                }
            }
        }

        public static bool CheckCurse(string text)
        {
            text = text.ToLower();
            StringBuilder sb = new StringBuilder();

            foreach (char c in text)
            {
                if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !CheckForUsedChars(c))
                {
                    sb.Append(c.ToString());
                }
            }

            for (int i = 0; i < Constants.Banned.Length; i++)
            {
                if (sb.ToString().Contains(Constants.Banned[i]))
                {
                    return false;
                }
            }
            return true;

        }
        public static bool CheckForUsedChars(char c)
        {
            if ((c == '_' || (c == '-') || (c == '.') || (c == '|'))) { return true; }
            else { return false; }
        }

        public static void WriteSuspicious(string text)
        {
            FileWriter.WriteLine(@"Suspicious/STRhacks.txt", string.Format("[{0}] : '{1}'  is under suspicion of using Cheat Engine to get 13 STR during character creation.", DateTime.Now, text));
        }

        //check speed attack



    }
    
    class ReportManager
    {
        public static void FileNewReport(string input, int charid, byte type, string fullmessage = "", int misses = 0)
        {
            ReportManager nr = new ReportManager();
            switch (type)
            {
                case 0: //Packet Edit 
                    nr.FilePacketEdit(input, charid);
                    break;
                case 1: //Mob movement

                    break;
                case 2: //Character movement
                    nr.FileCharacterMovement(input, charid);
                    break;
                case 3: //Spam check
                    nr.FileSpam(input, charid);
                    break;
                case 4: //Suspicious text
                    nr.FileSuspiciousText(input, fullmessage, charid);
                    break;
                case 5: //Godmode miss
                    nr.FileMissGodmode(input, charid, misses);
                    break;
            }
        }

        public void FilePacketEdit(string packet, int charid)
        {
            FileWriter.WriteLine(@"Suspicious/PacketEdits.txt", string.Format("[{0}] : '{1}' : " + packet, DateTime.Now, charid));
            foreach (KeyValuePair<int, Character> chr in Server.Instance.CharacterList)
            {
                if (chr.Value.Admin)
                {
                    MessagePacket.SendNotice(charid + " : " + packet, chr.Value); 
                }
            }
        }

        public void FileMobMovement(string movement, int charid)
        {

        }

        public void FileCharacterMovement(string movement, int charid)
        {
            FileWriter.WriteLine(@"Suspicious/PacketEdits.txt", string.Format("[{0}] : '{1}' : " + movement, DateTime.Now, charid));
        }

        public void FileSpam(string spam, int charid)
        {
            FileWriter.WriteLine(@"Suspicious/Spam.txt", string.Format("[{0}] : '{1}' : " + spam, DateTime.Now, charid));
        }

        public void FileSuspiciousText(string text, string fullmessage, int charid)
        {
            FileWriter.WriteLine(@"Suspicious/SuspiciousText.txt", string.Format("[{0}] : '{1}' : " + text + " full message was : " + fullmessage, DateTime.Now, charid));
        }

        public void FileMissGodmode(string text, int charid, int misses)
        {
            FileWriter.WriteLine(@"Suspicious/Godmode.txt", string.Format("[{0}] : '{1}'" , DateTime.Now, charid + misses + " misses"));
        }
    }
}
