using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Game
{
    public class Kite
    {
        public Character Owner { get; set; }
        public int MapID { get; set; }
        public int ID { get; set; }
        public int OID { get; set; }
        public string Message { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        public Kite(Character owner, int id, int oid, string message, int mapid)
        {
            this.Owner = owner;
            this.ID = id;
            this.OID = oid;
            this.Message = message;
            this.MapID = mapid;
            this.X = owner.Position.X;
            this.Y = (short)(owner.Position.Y - 100);
            MapPacket.Kite(Owner, id, oid, message, X, Y);
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Remove Kite Effect",
                (date) => { RemoveKiteEffect(this); },
                300 * 1000, 0)); //5 minutes? idk 
            DataProvider.Maps[this.MapID].Kites.Add(this);
        }

        public void RemoveKiteEffect(Kite kite)
        {
            MapPacket.RemoveKite(kite.MapID, kite.ID);
            DataProvider.Maps[kite.ID].Kites.Remove(kite);
            MasterThread.Instance.RemoveRepeatingAction("Remove Kite Effect", (date, name, removed) => { });
            kite = null;
        }

    }
}
