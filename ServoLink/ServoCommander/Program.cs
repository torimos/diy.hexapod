using System;
using System.Threading;
using ServoLink;
using Unity.Configurator;

namespace ServoCommander
{
    class Program
    {
        static void Main(string[] args)
        {
            new UnityRuntimeConfiguration().SetupContainer();

            var sc = new ServoController(20, new BinaryHelper());
            if (!sc.Connect(new SerialPort("COM4", 57600))) return;
            Console.WriteLine("Connected");
       
            sc.SetAll(0);
            sc.Sync();
            Console.WriteLine("Tick");
            int t = 150;
            while (!Console.KeyAvailable)
            {
                Console.ReadLine();
                sc.Servos[0] = 2200;
                sc.Sync();
                Console.WriteLine("Tick");

                Console.ReadLine();
                sc.Servos[0] = 1500;
                sc.Sync();
                Console.WriteLine("Tick");

                Console.ReadLine();
                sc.Servos[0] = 1200;
                sc.Sync();
                Console.WriteLine("Tick");
            }


            sc.SetAll(0);
            sc.Sync();
            sc.Disconnect();
        }
    }
}
