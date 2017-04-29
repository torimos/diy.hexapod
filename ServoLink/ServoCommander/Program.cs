using System;
using System.Threading;
using ServoLink;
using Unity.Configurator;
using CRC;

namespace ServoCommander
{
    class Program
    {

        class Leg
        {
            private int _leg;
            private ServoController _controller;
            private bool _invertA;
            private bool _invertB;

            public Leg(ServoController controller, int leg, bool invertA = false, bool invertB = false)
            {
                _controller = controller;
                _leg = leg;
                _invertA = invertA;
                _invertB = invertB;
            }

            public void Move(ushort a, ushort b, ushort c, ushort t = 0)
            {
                _controller.Move(_leg * 3, (ushort)(_invertA ? 1500+(1500-a) : a), t);
                _controller.Move(_leg * 3 + 1, (ushort)(!_invertB ? 1500+(1500 - b) : b), t);
                _controller.Move(_leg * 3 + 2, c, t);
            }
        }
        static void Main(string[] args)
        {
            new UnityRuntimeConfiguration().SetupContainer();

            var sc = new ServoController(20, new BinaryHelper());
            if (!sc.Connect(new SerialPort("COM6", 115200))) return;
            Console.WriteLine("Connected");
            var legs = new Leg[]
            {
                new Leg(sc, 0),new Leg(sc, 1),new Leg(sc, 2), new Leg(sc, 3, true, true),new Leg(sc, 4, true, true),new Leg(sc, 5, true, true),
            };
            Console.ReadLine();
            sc.MoveAll(0, 0);
            Console.WriteLine("Commited: {0}", sc.Commit());

            Console.ReadLine();
            legs[0].Move(1500, 1000, 0);
            legs[1].Move(1500, 1000, 0);
            legs[2].Move(1500, 1000, 0);
            legs[3].Move(1500, 1000, 0);
            legs[4].Move(1500, 1000, 0);
            legs[5].Move(1500, 1000, 0);
            Console.WriteLine("Commited: {0}", sc.Commit());
            Thread.Sleep(500);

            legs[0].Move(1500, 1000, 1500);
            legs[1].Move(1500, 1000, 1500);
            legs[2].Move(1500, 1000, 1500);
            legs[3].Move(1500, 1000, 1500);
            legs[4].Move(1500, 1000, 1500);
            legs[5].Move(1500, 1000, 1500);
            Console.WriteLine("Commited: {0}", sc.Commit());
            Console.ReadLine();

            while (true)
            {
                legs[0].Move(1500, 1500, 1000, 500);
                legs[1].Move(1500, 1500, 1000, 500);
                legs[2].Move(1500, 1500, 1000, 500);
                legs[3].Move(1500, 1500, 1000, 500);
                legs[4].Move(1500, 1500, 1000, 500);
                legs[5].Move(1500, 1500, 1000, 500);
                Console.WriteLine("Commited: {0}", sc.Commit());
                if (Console.ReadLine() == "q") break;

                legs[0].Move(1800, 1500, 1500, 500);
                legs[1].Move(1800, 1500, 1500, 500);
                legs[2].Move(1800, 1500, 1500, 500);
                legs[3].Move(1800, 1500, 1500, 500);
                legs[4].Move(1800, 1500, 1500, 500);
                legs[5].Move(1800, 1500, 1500, 500);
                Console.WriteLine("Commited: {0}", sc.Commit());
                if (Console.ReadLine() == "q") break;

                legs[0].Move(1500, 1500, 2000, 500);
                legs[1].Move(1500, 1500, 2000, 500);
                legs[2].Move(1500, 1500, 2000, 500);
                legs[3].Move(1500, 1500, 2000, 500);
                legs[4].Move(1500, 1500, 2000, 500);
                legs[5].Move(1500, 1500, 2000, 500);
                Console.WriteLine("Commited: {0}", sc.Commit());
                if (Console.ReadLine() == "q") break;

                legs[0].Move(1800, 1500, 1500, 500);
                legs[1].Move(1800, 1500, 1500, 500);
                legs[2].Move(1800, 1500, 1500, 500);
                legs[3].Move(1800, 1500, 1500, 500);
                legs[4].Move(1800, 1500, 1500, 500);
                legs[5].Move(1800, 1500, 1500, 500);
                Console.WriteLine("Commited: {0}", sc.Commit());
                if (Console.ReadLine() == "q") break;
            }

            sc.MoveAll(0);
            sc.Commit();
            sc.Disconnect();
        }
    }
}
