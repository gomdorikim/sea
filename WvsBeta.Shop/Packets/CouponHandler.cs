using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
//using WvsBeta.Game;

namespace WvsBeta.Shop
{
    class CouponHandler
    {
        public static void HandleCoupon(Character chr, Packet packet)
        {
            short header = packet.ReadShort();
            switch (header)
            {
                case 0: //Redeem
                    string serial = packet.ReadString();

                    if (IsValidSerial(serial))
                    {
                        chr.mStorage.mNX += Cash(serial);
                        chr.mStorage.SaveNXValues();
                        chr.mStorage.LoadNXValues();
                        CashPacket.SendCashAmounts(chr);
                        Server.Instance.CharacterDatabase.RunQuery("DELETE FROM cashshop_coupon_codes WHERE serial = '" + serial + "'");
                    }
                    else
                    {
                        CashPacket.SendError(chr, CashPacket.CashErrors.CheckCouponNumber);
                    }
                    break;
                case 5: //Todo : gift

                    break;

            }
        }

        public static bool IsValidSerial(string serial)
        {
            Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM cashshop_coupon_codes WHERE serial = '" + MySqlHelper.EscapeString(serial) + "'");
            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
            data.Read();
            if (data.HasRows)
            {
                int used = data.GetInt32("used");
                string real = data.GetString("serial");
                if (serial != real)
                {
                    return false;
                }
                else if (serial == real && data.HasRows && used == 0)
                {
                    return true;
                }
            }
            else if (!data.HasRows)
            {
                return false;
            }
            return false;
        }

        public static int Cash(string serial)
        {
            Server.Instance.CharacterDatabase.RunQuery("SELECT `nxcredit` FROM cashshop_coupon_codes WHERE serial = '" + serial + "'");
            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
            data.Read();
            int amount = data.GetInt32("nxcredit");
            return amount;
        }


    }
}