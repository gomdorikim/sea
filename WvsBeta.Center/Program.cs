using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Center
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

                    InitializeServer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Environment.Exit(123123);
                }
                
                Console.Read();
            }
        }

        static void InitializeServer()
        {
            try
            {
                CenterServer.Init(Program.IMGFilename);

                Console.WriteLine("Starting Ranking Calculator... ");
                RankingCalculator.StartRankingCalculator();
            }
            catch (Exception ex)
            {
                Program.LogFile.WriteLine("Got exception @ frmMain::InitServer : {0}", ex.ToString());
                MessageBox.Show(string.Format("[{0}][CENTER SERVER] Got exception @ frmMain::InitServer : {1}", DateTime.Now.ToString(), ex.ToString()));
                Environment.Exit(5);
            }
        }
    }
}