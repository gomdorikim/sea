using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace WvsBeta.Shop
{
    class Program
    {
        public static frmMain MainForm { get; set; }

        public static string IMGFilename { get; set; }
        public static Common.Logfile LogFile { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                MessageBox.Show("Invalid argument length.");
            }
            else
            {
                IMGFilename = args[0];
                LogFile = new Common.Logfile(IMGFilename);
                MasterThread.Load(IMGFilename);
                Common.Pinger.Init();
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(MainForm = new frmMain());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    MainForm.Close();
                    Application.Exit();
                }
            }
        }
    }
}
