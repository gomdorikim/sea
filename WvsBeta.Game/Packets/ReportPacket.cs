//Author: Vanlj95
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common.Sessions;
using WvsBeta.Database;

namespace WvsBeta.Game
{
    public class ReportPacket
    {

        public static void HandleReport(Character chr, Packet packet)
        {

            int id = packet.ReadInt(); //Player ID
            byte header = packet.ReadByte(); //Which case
            ReportMessage(chr);
            Server.Instance.CharacterDatabase.RunQuery("INSERT INTO reports (charid, reportedID, reportid) VALUES (" + chr.ID + ", " + id + ", " + header + "')");
               
        }

        public static void ReportMessage(Character chr)
        {
            Packet pw = new Packet(0x1D);
            pw.WriteByte(0); //? 
            chr.sendPacket(pw);
        }

        
    }
}