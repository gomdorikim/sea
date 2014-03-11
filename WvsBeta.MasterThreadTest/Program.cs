using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WvsBeta.MasterThread;

namespace WvsBeta.MasterThreadTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MasterThread.MasterThread.Load();


            while (true)
            {
                string str = Console.ReadLine();
                MasterThread.MasterThread.RepeatingAction action = new MasterThread.MasterThread.RepeatingAction(str, (a) =>
                {
                    Console.WriteLine("{0} - ! {1}", a.ToString(), str);
                }, 10*1000, 1, false);
                MasterThread.MasterThread.Instance.AddRepeatingAction(action);
            }
        }
    }
}
