using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Shop {
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

			appendToLog("Loading data file... ", false);
			DataProvider.Load(@"..\DataSvr\OutputWvsShop.bin");
			appendToLog("DONE");

			this.Invoke((MethodInvoker)delegate { this.Text += " (" + Program.IMGFilename + ")"; });
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
				appendToLog(string.Format("[{0}] Got connection (++), got {1} connections atm.", DateTime.Now.ToString(), load));
			}
			else {
				--load;
				appendToLog(string.Format("[{0}] Lost connection (++), got {1} connections atm.", DateTime.Now.ToString(), load));
			}

			setLoad();
			Server.Instance.CenterConnection.updateConnections(load);
		}

		public void appendToLog(string what, bool newline = true) {
			Console.WriteLine(what);
			Server.Instance.LogToLogfile(what + (newline ? Environment.NewLine : ""));
			txtLog.BeginInvoke((MethodInvoker)delegate
			{
				txtLog.Text += what + (newline ? Environment.NewLine : "");
				if (!txtLog.Focused) {
					txtLog.SelectionStart = txtLog.Text.Length;
					txtLog.ScrollToCaret();
					txtLog.Refresh();
				}
			});
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
			Server.Instance.CharacterDatabase.Stop = true;
		}
	}
}
