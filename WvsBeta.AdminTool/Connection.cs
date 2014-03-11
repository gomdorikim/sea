using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WvsBeta.Common.Sessions;

namespace WvsBeta.AdminTool
{
    class Connection : AbstractConnection
    {
        public Connection(string pIP, ushort pPort) :
            base(pIP, pPort)
        {
        }

        public override void OnDisconnect()
        {
            Environment.Exit(1);
        }

        public override void OnHandshakeInbound(Packet pPacket)
        {
            Packet packet = new Packet((byte)0);
            frmMain.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                packet.WriteString(frmMain.Instance.txtKey.Text);
            });
            SendPacket(packet);
        }

        public void ApplyRateToServer(string pName, double pExpRate, double pMesosRate, double pDropRate)
        {
            Packet packet = new Packet((byte)1);
            packet.WriteString(pName);
            packet.WriteDouble(pExpRate);
            packet.WriteDouble(pMesosRate);
            packet.WriteDouble(pDropRate);
            SendPacket(packet);
        }

        public void RefreshStuff()
        {
            Packet packet = new Packet((byte)2);
            SendPacket(packet);
        }

        public override void AC_OnPacketInbound(Packet pPacket)
        {
            switch (pPacket.ReadByte())
            {
                case 0:
                    {
                        short records = pPacket.ReadShort();

                        frmMain.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            frmMain.Instance.lvServers.Items.Clear();
                            for (short i = 0; i < records; i++)
                            {
                                frmMain.Instance.lvServers.Items.Add(new System.Windows.Forms.ListViewItem(new string[] { pPacket.ReadString(), pPacket.ReadInt().ToString(), pPacket.ReadDouble().ToString(), pPacket.ReadDouble().ToString(), pPacket.ReadDouble().ToString() }));
                            }
                        });
                        break;
                    }
            }
        }

    }
}
