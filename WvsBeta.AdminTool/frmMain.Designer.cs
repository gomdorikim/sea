namespace WvsBeta.AdminTool
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.lvServers = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nudPort = new System.Windows.Forms.NumericUpDown();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMobExp = new System.Windows.Forms.TextBox();
            this.txtDropRate = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtMesosRate = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvServers
            // 
            this.lvServers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lvServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.lvServers.FullRowSelect = true;
            this.lvServers.GridLines = true;
            this.lvServers.HideSelection = false;
            this.lvServers.Location = new System.Drawing.Point(12, 66);
            this.lvServers.Name = "lvServers";
            this.lvServers.Size = new System.Drawing.Size(713, 268);
            this.lvServers.TabIndex = 0;
            this.lvServers.UseCompatibleStateImageBehavior = false;
            this.lvServers.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 368;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Connections";
            this.columnHeader2.Width = 87;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP:";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(38, 12);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(155, 20);
            this.txtIP.TabIndex = 2;
            this.txtIP.Text = "83.80.148.175";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(199, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = ":";
            // 
            // nudPort
            // 
            this.nudPort.Location = new System.Drawing.Point(215, 12);
            this.nudPort.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudPort.Name = "nudPort";
            this.nudPort.Size = new System.Drawing.Size(82, 20);
            this.nudPort.TabIndex = 4;
            this.nudPort.Value = new decimal(new int[] {
            7070,
            0,
            0,
            0});
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(550, 12);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(175, 48);
            this.btnConnect.TabIndex = 5;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Mob EXP Rate:";
            // 
            // txtMobExp
            // 
            this.txtMobExp.Location = new System.Drawing.Point(109, 19);
            this.txtMobExp.Name = "txtMobExp";
            this.txtMobExp.Size = new System.Drawing.Size(100, 20);
            this.txtMobExp.TabIndex = 7;
            // 
            // txtDropRate
            // 
            this.txtDropRate.Location = new System.Drawing.Point(109, 71);
            this.txtDropRate.Name = "txtDropRate";
            this.txtDropRate.Size = new System.Drawing.Size(100, 20);
            this.txtDropRate.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Drop Chance Rate:";
            // 
            // txtMesosRate
            // 
            this.txtMesosRate.Location = new System.Drawing.Point(109, 45);
            this.txtMesosRate.Name = "txtMesosRate";
            this.txtMesosRate.Size = new System.Drawing.Size(100, 20);
            this.txtMesosRate.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(94, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Mesos Amnt Rate:";
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(215, 19);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(79, 68);
            this.btnApply.TabIndex = 12;
            this.btnApply.Text = "Apply to Selected";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnApply);
            this.groupBox1.Controls.Add(this.txtMobExp);
            this.groupBox1.Controls.Add(this.txtMesosRate);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtDropRate);
            this.groupBox1.Location = new System.Drawing.Point(12, 340);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(306, 122);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            // 
            // txtKey
            // 
            this.txtKey.Location = new System.Drawing.Point(12, 40);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(532, 20);
            this.txtKey.TabIndex = 14;
            this.txtKey.Text = resources.GetString("txtKey.Text");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(109, 106);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Use \'0\' as a \'skip\'";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Mob EXP Rate";
            this.columnHeader3.Width = 86;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Mesos Rate";
            this.columnHeader4.Width = 70;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Mesos Rate";
            this.columnHeader5.Width = 73;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 469);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.nudPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lvServers);
            this.MinimumSize = new System.Drawing.Size(507, 38);
            this.Name = "frmMain";
            this.Text = "WvsBeta Admin Tool";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        public System.Windows.Forms.ListView lvServers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMobExp;
        private System.Windows.Forms.TextBox txtDropRate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMesosRate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TextBox txtKey;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
    }
}

