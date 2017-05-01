using System.Threading;
using ServoLink;
using Unity.Configurator;
using CRC;
using System;
using System.Threading.Tasks;

namespace ServoCommander
{
    partial class Program
    {
        public class ServoDriver
        {
            public static short[] CoxaOffset =  {   15, -50,   0, -15, -50,   0 };  //LF LM LR RR RM RF
            public static short[] FemurOffset = {   70,-100, -55,   0,  45, -40 }; //LF LM LR RR RM RF 
            public static short[] TibiaOffset = {    0,  65, -30,  40,   0,   0 }; //LF LM LR RR RM RF

            private ServoController _controller;

            public ServoDriver(ServoController controller)
            {
                _controller = controller;
            }

            public void UpdateLegPos(byte legNumber, ushort coxaPos, ushort femurPos, ushort tibiaPos, ushort moveTime)
            {
                _controller.Move(legNumber * 3, (ushort)(tibiaPos + TibiaOffset[legNumber]), moveTime);
                _controller.Move(legNumber * 3 + 1, (ushort)(femurPos + FemurOffset[legNumber]), moveTime);
                _controller.Move(legNumber * 3 + 2, (ushort)(coxaPos + CoxaOffset[legNumber]), moveTime);
            }

            public void UpdateLeg(byte legNumber, double coxaAngle, double femurAngle, double tibiaAngle, ushort moveTime)
            {
                ushort coxaPos = (ushort)(1500 + (coxaAngle * 10));
                ushort femurPos = (ushort)(1500 + (femurAngle * 10));
                ushort tibiaPos = (ushort)(1500 + (tibiaAngle * 10));
                UpdateLegPos(legNumber, coxaPos, femurPos, tibiaPos, moveTime);
            }
            public void Commit()
            {
                _controller.Commit();
            }
        }

        static short coxaPos = 0;
        static short femurPos = 0;
        static short tibiaPos = 0;
        static byte legNumber = 0;
        static ushort moveTime;
        static IKMath.IKResult[] results = new IKMath.IKResult[6];

        static void UpdateServos(ServoDriver sd)
        {
            //sd.UpdateLegPos(legNumber, (ushort)(1500 + coxaPos), (ushort)(1500 + femurPos), (ushort)(1500 + tibiaPos), 200);
            //sd.Commit();
            if (results[3].Solution != IKMath.IKSolutionResultType.Error) sd.UpdateLeg(0, results[3].CoxaAngle, results[3].FemurAngle, results[3].TibiaAngle, moveTime);//LF
            if (results[4].Solution != IKMath.IKSolutionResultType.Error) sd.UpdateLeg(1, results[4].CoxaAngle, results[4].FemurAngle, results[4].TibiaAngle, moveTime);//LM
            if (results[5].Solution != IKMath.IKSolutionResultType.Error) sd.UpdateLeg(2, results[5].CoxaAngle, results[5].FemurAngle, results[5].TibiaAngle, moveTime);//LR
            if (results[0].Solution != IKMath.IKSolutionResultType.Error) sd.UpdateLeg(3, results[0].CoxaAngle, results[0].FemurAngle, results[0].TibiaAngle, moveTime);//RR
            if (results[1].Solution != IKMath.IKSolutionResultType.Error) sd.UpdateLeg(4, results[1].CoxaAngle, results[1].FemurAngle, results[1].TibiaAngle, moveTime);//RM
            if (results[2].Solution != IKMath.IKSolutionResultType.Error) sd.UpdateLeg(5, results[2].CoxaAngle, results[2].FemurAngle, results[2].TibiaAngle, moveTime);//RF
            sd.Commit();
        }

        static void Main(string[] args)
        {
            new UnityRuntimeConfiguration().SetupContainer();
            var hm = new IKMath();
            
            var sc = new ServoController(20, new BinaryHelper());
            var sd = new ServoDriver(sc);
            if (!sc.Connect(new SerialPort("COM6", 115200))) return;
            Console.WriteLine("Connected");
            sc.MoveAll(0);
            Console.WriteLine("Commited: {0}", sc.Commit());

            double bodyYOffset = 0;
            moveTime = 100;
            Console.Clear();
            bool runUpdates = true;
            bool refresh = true;
            Task.Run(() =>
            {
                while(runUpdates)
                {
                    if (refresh)
                    {
                        UpdateServos(sd);
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
                    //if (key == ConsoleKey.Tab)
                    //{
                    //    legNumber = (byte)((legNumber + 1) % 6);
                    //    tibiaPos = femurPos = coxaPos = 0;
                    //    sc.MoveAll(0);
                    //}
                    //if (key == ConsoleKey.A) coxaPos += 1;
                    //else if (key == ConsoleKey.Z) coxaPos -= 1;
                    //if (key == ConsoleKey.S) femurPos += 1;
                    //else if (key == ConsoleKey.X) femurPos -= 1;
                    //if (key == ConsoleKey.D) tibiaPos += 1;
                    //else if (key == ConsoleKey.C) tibiaPos -= 1;
                    //refresh = true;
                    if (key == ConsoleKey.A)
                        bodyYOffset += 0.5;
                    else if (key == ConsoleKey.Z)
                        bodyYOffset -= 0.5;

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
                //Console.WriteLine("Leg[{0}] {1,4} {2,4} {3,4}", legNumber, coxaPos, femurPos, tibiaPos);
                Console.WriteLine("BodyY {0}", bodyYOffset);
                Console.WriteLine("MoveTime {0}", moveTime);
                Console.WriteLine("Leg{0} {1,12} {2,12} {3,12} {4,12}", 0, results[3].CoxaAngle, results[3].FemurAngle, results[3].TibiaAngle, results[3].Solution);
            }
            runUpdates = false;
            Thread.Sleep(150);
            sc.MoveAll(0);
            sc.Commit();
            sc.Disconnect();
        }
    }
}
