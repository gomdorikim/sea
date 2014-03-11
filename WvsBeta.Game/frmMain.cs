using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public partial class frmMain : Form
    {
        int load = 0;
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            System.Threading.Thread tr = new System.Threading.Thread(InitializeServer);
            tr.IsBackground = true; // Prevents the server from hanging when you close it while it's loading.
            tr.Start();
        }

        private void InitializeServer()
        {
            //try {
            Server.Init(Program.IMGFilename);

            while (Server.Instance.ID == 0xFF)
            {
                System.Threading.Thread.Sleep(1000);
            }
            
            Server.Instance.MakeAvailableScripts(null);

            LogAppend("[WZ LOADING]");
            LogAppend("Loading the data file...", false);
            DataProvider.Load(@"..\DataSvr\Output.bin");


            LogAppend("Setting up Map Checker thread", false);
            
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Map Checker",
                Server.Instance.CheckMaps,
                0, 3000, true));
            LogAppend("DONE");


            LogAppend("Loading Buffs... ", false);
            BuffDataProvider.LoadBuffs();
            LogAppend("Done loading Buffs!");

            //WvsBeta.Game.Events.Boat.Initialize();
            LogAppend("Boat manager initialized!");
            
            Server.Instance.CharacterDatabase.ClearParties();
            Server.Instance.CharacterDatabase.ClearLeaders();

            WvsBeta.Game.Events.EventManager.Load();

            
            
         

            //RunTimedFunction.AddTimedFunction(delegate { Server.Instance.CharacterDatabase.Ping(); }, new TimeSpan(), new TimeSpan(0, 30, 0), BetterTimerTypes.Pinging, 0, 0);
            /**
            this.Invoke((MethodInvoker)delegate {
                this.txtLoad.Text = "0";
                this.Text += " Channel " + Server.Instance.ID + " (" + Program.IMGFilename + ")"; 
            });
            /*
        }
        catch (Exception ex) {
            FileWriter.WriteLine("Logs\\crashes.txt", string.Format("[{0}][GAME SERVER] Got exception @ frmMain::InitServer :\r\n{1}", DateTime.Now.ToString(), ex.ToString()));
            MessageBox.Show(string.Format("[{0}][GAME SERVER] Got exception @ frmMain::InitServer:\r\n {1}", DateTime.Now.ToString(), ex.ToString()));
            Environment.Exit(5);
        }*/
        }

        public void ChangeLoad(bool up)
        {
            if (up)
            {
                //++load;
                LogAppend(string.Format("[{0}] Received a connection! The server now has {1} connections.", DateTime.Now.ToString(), load));
            }
            else
            {
                //--load;
                LogAppend(string.Format("[{0}] Lost a connection! The server now has {1} connections.", DateTime.Now.ToString(), load));
            }
            /**
            txtLoad.Invoke((MethodInvoker)delegate
            {
                txtLoad.Text = load.ToString();
            });

            Server.Instance.CenterConnection.SendUpdateConnections(load);
             * **/
        }

        public void LogAppend(string what, bool newline = true)
        {
            Server.Instance.LogToLogfile(what);
            /**
            txtLog.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += what + (newline ? Environment.NewLine : "");
                if (!txtLog.Focused)
                {
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                    txtLog.Refresh();
                }
            });
             * **/
            Console.WriteLine(what);
        }

        public void LogAppendFormat(string pFormat, params object[] pParams)
        {
            LogAppend(string.Format(pFormat, pParams));
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Server.Instance.CharacterDatabase.Stop = true;
        }
    }
}