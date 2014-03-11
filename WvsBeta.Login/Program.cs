using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Login
{
    class Program
    {
        public static frmMain MainForm { get; set; }

        public static string IMGFilename { get; set; }

        public static Logfile LogFile { get; private set; }

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
                MasterThread.Load(IMGFilename);

                Pinger.Init();
                LogFile = new Logfile(IMGFilename);
                try
                {
                    //Application.EnableVisualStyles();
                    //Application.SetCompatibleTextRenderingDefault(false);
                    System.Threading.Thread tr = new System.Threading.Thread(InitializeServer);
                    tr.IsBackground = true; // Prevents the server from hanging when you close it while it's loading.
                    tr.Start();
                    Console.Read();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    MainForm.Close();
                    Application.Exit();
                }
            }
        }

        static void InitializeServer()
        {

            Server.Init(Program.IMGFilename);

            // GameServerPinger.Pinger(); 

            //this.Invoke((MethodInvoker)delegate { this.Text += " (" + Program.IMGFilename + ")"; });
        }

    }
}
