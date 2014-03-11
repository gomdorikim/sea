using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public class Pinger
    {
        public static List<Sessions.AbstractConnection> Connections { get; private set; }
        private const int PING_CHECK_TIME = 15 * 1000;

        public static void Init()
        {
            Connections = new List<Sessions.AbstractConnection>();
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Pinger",
                (date) =>
                {
                    List<Sessions.AbstractConnection> d = new List<Sessions.AbstractConnection>(Connections);
                    foreach (Sessions.AbstractConnection session in d)
                    {
                        if (session.gotPong)
                        {
                            session.gotPong = false;
                            session.SendPong();
                        }
                        else
                        {
                            session.Disconnect();
                            
                        }
                    }
                }, PING_CHECK_TIME, PING_CHECK_TIME));
        }
    }
}
