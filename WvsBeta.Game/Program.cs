using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Game
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
                    //Application.Run(MainForm = new frmMain());
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
            //try {
            Server.Init(Program.IMGFilename);

            while (Server.Instance.ID == 0xFF)
            {
                System.Threading.Thread.Sleep(1000);
            }

            Server.Instance.MakeAvailableScripts(null);

            Console.WriteLine("[WZ LOADING]");
            Console.WriteLine("Loading the data file...", false);
            DataProvider.Load(@"..\DataSvr\Output.bin");


            Console.WriteLine("Setting up Map Checker thread", false);

            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Map Checker",
                Server.Instance.CheckMaps,
                0, 3000, true));
            Console.WriteLine("DONE");


            Console.WriteLine("Loading Buffs... ", false);
            BuffDataProvider.LoadBuffs();
            Console.WriteLine("Done loading Buffs!");

            //WvsBeta.Game.Events.Boat.Initialize();
            Console.WriteLine("Boat manager initialized!");

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
    }
}
