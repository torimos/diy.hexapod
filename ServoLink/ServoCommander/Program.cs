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
            var model = new HexModel(6);
            model.LegsPos[0] = new XYZ(56, 60, 96);
            model.LegsPos[1] = new XYZ(111, 60, 0);
            model.LegsPos[2] = new XYZ(56, 60, -96);
            model.LegsPos[3] = new XYZ(56, 60, 96);
            model.LegsPos[4] = new XYZ(111, 60, 0);
            model.LegsPos[5] = new XYZ(56, 60, -96);

            var hm = new IKMath();
            var sd = new ServoDriver();

            bool runUpdates = true;
            bool refresh = true;
            Task.Run(() =>
            {
                while(runUpdates)
                {
                    if (refresh)
                    {
                        sd.Update(model.LegsAngle, model.MoveTime);
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

                    for (byte leg = 0; leg < model.LegsCount/2; leg++)
                    {
                        model.LegsAngle[leg] = hm.LegIK(leg, model.LegsPos[leg].X, model.LegsPos[leg].Y, model.LegsPos[leg].Z);
                    }
                    for (byte leg = (byte)(model.LegsCount / 2); leg < model.LegsCount; leg++)
                    {
                        model.LegsAngle[leg] = hm.LegIK(leg, model.LegsPos[leg].X, model.LegsPos[leg].Y, model.LegsPos[leg].Z);
                    }
                    refresh = true;
                }
                Console.SetCursorPosition(0, 0);
             
            }
            runUpdates = false;
            Thread.Sleep(150);
            sd.Reset();
            sd.Dispose();
        }
    }
}
