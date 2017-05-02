using System.Threading;
using Unity.Configurator;
using System;
using System.Threading.Tasks;

namespace ServoCommander
{
    partial class Program
    {
        static void Main(string[] args)
        {
            new UnityRuntimeConfiguration().SetupContainer();
            var hm = new IKMath();
            var sd = new ServoDriver();
            IKMath.IKLegResult[] results = new IKMath.IKLegResult[6];
            ushort moveTime = 100;
            double bodyYOffset = 0;

            bool runUpdates = true;
            bool refresh = true;
            Task.Run(() =>
            {
                while(runUpdates)
                {
                    if (refresh)
                    {
                        sd.Update(results, moveTime);
                        refresh = false;
                    }
                    Thread.Sleep(20);
                }
            });

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape) break;
               
                    if (key == ConsoleKey.A)
                        bodyYOffset += 1;
                    else if (key == ConsoleKey.Z)
                        bodyYOffset -= 1;

                    if (bodyYOffset < -15) bodyYOffset = -15;
                    if (bodyYOffset > 110) bodyYOffset = 110;

                    results[0] = hm.LegIK(0, 56, 60 + bodyYOffset, 96);
                    results[1] = hm.LegIK(1, 111, 60 + bodyYOffset, 0);
                    results[2] = hm.LegIK(2, 56, 60 + bodyYOffset, -96);
                    results[3] = hm.LegIK(3, 56, 60 + bodyYOffset, 96);
                    results[4] = hm.LegIK(4, 111, 60 + bodyYOffset, 0);
                    results[5] = hm.LegIK(5, 56, 60 + bodyYOffset, -96);

                    refresh = true;
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("BodyY {0}", bodyYOffset);
                Console.WriteLine("MoveTime {0}", moveTime);
                Console.WriteLine("Leg{0} {1,12} {2,12} {3,12} {4,12}", 0, results[3].CoxaAngle, results[3].FemurAngle, results[3].TibiaAngle, results[3].Solution);
            }
            runUpdates = false;
            Thread.Sleep(150);
            sd.Reset();
            sd.Dispose();
        }
    }
}
