using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public class Ring
    {
        public List<int> Rings { get; set; }
        public int RingID { get; set; }
        public int ItemID { get; set; }
        public int CharacterID { get; set; }
        public int PartnerID { get; set; }
        public bool Equipped { get; set; }

        public Ring(int rID, int iID, int charID, int pID, bool equipped)
        {
            Rings = new List<int>();
            Rings.Add(rID);
            RingID = rID;
            iID = ItemID;
            CharacterID = charID;
            PartnerID = pID;
            Equipped = equipped;
        }

        public static void LoadRings(Character chr)
        {
            MySqlDataReader data = Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM rings WHERE charid = '" + chr.ID + "'") as MySqlDataReader;
            if (data.HasRows)
            {
                while (data.Read())
                {
                    if ((chr.Inventory.GetEquippedItemID((short)Constants.EquipSlots.Slots.Ring1, true) == 1112001) ||
                       (chr.Inventory.GetEquippedItemID((short)Constants.EquipSlots.Slots.Ring2, true) == 1112001) ||
                       (chr.Inventory.GetEquippedItemID((short)Constants.EquipSlots.Slots.Ring3, true) == 1112001) ||
                       (chr.Inventory.GetEquippedItemID((short)Constants.EquipSlots.Slots.Ring4, true) == 1112001))
                    {
                        chr.pRing = new Ring(data.GetInt32("id"), data.GetInt32("itemid"), chr.ID, data.GetInt32("partnerid"), true);
                    }
                    else
                    {
                        chr.pRing = new Ring(data.GetInt32("id"), data.GetInt32("itemid"), chr.ID, data.GetInt32("partnerid"), false);
                    }
                }
            }
        }
    }
}
