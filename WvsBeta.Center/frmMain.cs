using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Center
{
    public partial class frmMain : Form
    {
        private int TotalConnections;
        private bool mStartedSavingConnections { get; set; }
        private System.Threading.Thread tr { get; set; }

        public frmMain()
        {
            mStartedSavingConnections = false;
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            tr = new System.Threading.Thread(InitializeServer);
            tr.IsBackground = true;
            tr.Start();
        }

        private void InitializeServer()
        {
            try
            {
                CenterServer.Init(Program.IMGFilename);

                appendToLog("Starting Ranking Calculator... ", false);
                RankingCalculator.StartRankingCalculator();
                appendToLog("DONE");


                this.Invoke((MethodInvoker)delegate
                {
                    Text += " (" + Program.IMGFilename + ")";
                });



                MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                    "Server List Updator",
                    (date) =>
                    {
                        this.Invoke((MethodInvoker)delegate { updateServerList(); });
                    },
                    0,
                    5 * 1000));

            }
            catch (Exception ex)
            {
                Program.LogFile.WriteLine("Got exception @ frmMain::InitServer : {0}", ex.ToString());
                MessageBox.Show(string.Format("[{0}][CENTER SERVER] Got exception @ frmMain::InitServer : {1}", DateTime.Now.ToString(), ex.ToString()));
                Environment.Exit(5);
            }
        }

        public void appendToLog(string what, bool newline = true)
        {
            Console.WriteLine(what);
            //CenterServer.Instance.LogToLogfile(what);
            txtLog.BeginInvoke((MethodInvoker)delegate
            {
                txtLog.Text += what + (newline ? Environment.NewLine : "");
                if (!txtLog.Focused)
                {
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                    txtLog.Refresh();
                }
            });
        }


        public void updateServerList()
        {
            TotalConnections = 0;
            ListViewItem item;
            lvServers.BeginUpdate();
            foreach (KeyValuePair<string, LocalServer> Server in CenterServer.Instance.LocalServers)
            {
                LocalServer ls = Server.Value;
                TotalConnections += ls.Connections;
                if (lvServers.Items.ContainsKey(Server.Key))
                {
                    item = lvServers.Items[Server.Key];
                }
                else
                {
                    item = new ListViewItem(new string[] { 
                        ls.Name, 
                        ls.PublicIP.ToString() + ":" + ls.Port.ToString(),
                        "0",
                        "N/A",
                    });
                    item.Name = ls.Name;

                    lvServers.Items.Add(item);
                }

                item.ImageIndex = ls.Connected ? 1 : 0;
                item.SubItems[2].Text = ls.Connections.ToString();
                if (ls.IsGameServer)
                {
                    item.SubItems[0].Text = ls.Name + (ls.Connected ? " (CH. " + ls.GameID + ")" : "");
                    item.SubItems[3].Text = string.Format("{0}/{1}/{2}", ls.RateMobEXP, ls.RateMesoAmount, ls.RateDropChance);
                }

                item = null;
            }
            lvServers.EndUpdate();

            txtTotalConnections.Text = TotalConnections.ToString();

            foreach (AdminSocket sock in CenterServer.Instance.AdminSockets)
            {
                sock.SendServers();
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            CenterServer.Instance.CharacterDatabase.Stop = true;
            MasterThread.Instance.Stop = true;
        }
    }
}