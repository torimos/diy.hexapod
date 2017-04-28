using System;
using System.Threading;
using ServoLink;
using Unity.Configurator;
using CRC;

namespace ServoCommander
{
    class Program
    {
        static void Main(string[] args)
        {
            new UnityRuntimeConfiguration().SetupContainer();

            var sc = new ServoController(20, new BinaryHelper());
            if (!sc.Connect(new SerialPort("COM6", 115200))) return;
            Console.WriteLine("Connected");

            sc.MoveAll(0);
            Console.WriteLine("Commited: {0}", sc.Commit());
            while (!Console.KeyAvailable)
            {
                Console.ReadLine();
                sc.Move(0, 2200, 2000);
                sc.Move(3, 1300, 2000);
                // sc.Move(6, 2200, 2000);
                Console.WriteLine("Commited: {0}", sc.Commit());

                Console.ReadLine();
                sc.Move(0, 1300, 2000);
                sc.Move(3, 2200, 2000);
                //sc.Move(6, 1300, 2000);
                Console.WriteLine("Commited: {0}", sc.Commit());
            }

            sc.MoveAll(0);
            sc.Commit();
            sc.Disconnect();
        }
    }
}
