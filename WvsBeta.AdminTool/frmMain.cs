using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WvsBeta.AdminTool
{
    public partial class frmMain : Form
    {
        public static frmMain Instance { get; private set; }

        private Connection _connection;

        public frmMain()
        {
            Instance = this;
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _connection = new Connection(txtIP.Text, Convert.ToUInt16(nudPort.Value));
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (_connection == null) return;
            double exprate = double.Parse(txtMobExp.Text);
            double mesosrate = double.Parse(txtMesosRate.Text);
            double droprate = double.Parse(txtDropRate.Text);

            foreach (ListViewItem lvi in lvServers.SelectedItems)
            {
                _connection.ApplyRateToServer(lvi.SubItems[0].Text, exprate, mesosrate, droprate);
            }

            _connection.RefreshStuff();
        }
    }
}
