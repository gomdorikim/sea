using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Login {
	public partial class frmMain : Form {
		int load = 0;
		public frmMain() {
			InitializeComponent();
		}

		private void frmMain_Load(object sender, EventArgs e) {
			txtLoad.Text = "0";
			System.Threading.Thread tr = new System.Threading.Thread(InitializeServer);
			tr.IsBackground = true;
			tr.Start();
		}

		private void InitializeServer() {
			Server.Init(Program.IMGFilename);

            // GameServerPinger.Pinger(); 

			//this.Invoke((MethodInvoker)delegate { this.Text += " (" + Program.IMGFilename + ")"; });
		}

		public void setLoad() {
			txtLoad.BeginInvoke((MethodInvoker)delegate
			{
				txtLoad.Text = load.ToString();
			});
		}

		public void changeLoad(bool up) {
			if (up) {
				++load;
				Console.WriteLine(string.Format("[{0}] Client Connected. Current User count: {1}", DateTime.Now.ToString(), load));
			}
			else {
				--load;
                Console.WriteLine(string.Format("[{0}] Client Disconnected. Current User count: {1}", DateTime.Now.ToString(), load));
			}

			setLoad();
			Server.Instance.CenterConnection.updateConnections(load);
		}

		public void appendToLog(string what, bool newline = true) {
			Console.WriteLine(what);

			Server.Instance.LogToLogfile(what);

            /**
			txtLog.BeginInvoke((MethodInvoker)delegate
			{
				txtLog.Text += what + (newline ? Environment.NewLine : "");
				if (!txtLog.Focused) {
					txtLog.SelectionStart = txtLog.Text.Length;
					txtLog.ScrollToCaret();
					txtLog.Refresh();
				}
			});
             * 
             * **/
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
			Server.Instance.CharacterDatabase.Stop = true;
            MasterThread.Instance.Stop = true;
		}
	}
}
