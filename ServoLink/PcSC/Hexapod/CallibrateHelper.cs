using Data;
using Drivers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PcSC.Hexapod
{
    class CallibrateHelper
    {        
        // LF RF
        // LM RM
        // LR RR
        private static int[] ServoMap = new int[] { 16, 17, 18, 19, 12, 13, 14, 15, 8, 3, 2, 1, 0, 7, 6, 5, 4, 11 }; //tfc   //RR RM RF LR LM LF
        private static int[] ServoInv = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1,  -1, -1, -1, -1, -1, -1, -1, -1, -1 }; //hack - inverse for calibration only
        private static int[] ServoOffset = new int[] { 10, -170, -30, -20, -130, -40, 0, -20, 0, 20, 80, 30, 70, 220, -40, -40, 90, 20 };
        private static int[] ServoPos = new int[] { -700, 700, 0, -700, 700, 0, -700, 700, 0, -700, 700, 0, -700, 700, 0, -700, 700, 0 };

        static void UpdateServos(ServoDriver sd, ushort moveTime)
        {
            // tfc-cft => /"*-*"\
            for (int i = 0; i < HexConfig.LegsCount; i++)
            {
                ushort tibiaPos = (ushort)(1500 + (ServoPos[i * 3] + ServoOffset[i * 3]) * ServoInv[i * 3]);
                ushort femurPos = (ushort)(1500 + (ServoPos[i * 3 + 1] + ServoOffset[i * 3 + 1]) * ServoInv[i * 3 + 1]);
                ushort coxaPos = (ushort)(1500 + (ServoPos[i * 3 + 2] + ServoOffset[i * 3 + 2]) * ServoInv[i * 3 + 2]);
                sd.Move(ServoMap[i * 3], tibiaPos, moveTime);
                sd.Move(ServoMap[i * 3 + 1], femurPos, moveTime);
                sd.Move(ServoMap[i * 3 + 2], coxaPos, moveTime);
            }
            sd.Commit();
        }
        const int LEFT_ARROW = 1;
        const int RIGHT_ARROW = 2;
        const int T_UP = 4;
        const int T_DN = 8;
        const int F_UP = 16;
        const int F_DN = 32;
        const int C_UP = 64;
        const int C_DN = 128;
        const int T_Z = 256;
        const int F_Z = 512;
        const int C_Z = 1024;
        const int TFC_SAVE = 4096;
        static int getKey()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        return LEFT_ARROW;
                    case ConsoleKey.RightArrow:
                        return RIGHT_ARROW;
                    case ConsoleKey.Q:
                        return T_UP;
                    case ConsoleKey.A:
                        return T_DN;
                    case ConsoleKey.W:
                        return F_UP;
                    case ConsoleKey.S:
                        return F_DN;
                    case ConsoleKey.E:
                        return C_UP;
                    case ConsoleKey.D:
                        return C_DN;
                    case ConsoleKey.Z:
                        return T_Z;
                    case ConsoleKey.X:
                        return F_Z;
                    case ConsoleKey.C:
                        return C_Z;
                    case ConsoleKey.Enter:
                        return TFC_SAVE;
                }
            }
            return -1;
        }

        public static void Run()
        {
            var offsets_config = File.ReadAllText("offsets.txt");
            if (!string.IsNullOrWhiteSpace(offsets_config))
            {
                var offsets = offsets_config.Split(',');
                for (int i = 0; i < offsets.Length; i++)
                {
                    ServoOffset[i] = int.Parse(offsets[i]);
                }
            }

            ServoDriver sd = new ServoDriver(20);
            sd.Init("COM11");
            bool run = true;
            int leg = 0, last_leg = 0;
            int offsetMax = 1000;
            int offsetStep = 10;
            Console.WriteLine($"[{leg}] ({ServoOffset[leg * 3]}) ({ServoOffset[leg * 3 + 1]}) ({ServoOffset[leg * 3 + 2]})");
            UpdateServos(sd, 0);
            while (run)
            {
                int key = getKey();
                if (key < 0) continue;
                switch (key)
                {
                    case LEFT_ARROW:
                        last_leg = leg;
                        leg--;
                        if (leg < 0) leg = 0;
                        break;
                    case RIGHT_ARROW:
                        last_leg = leg;
                        leg++;
                        if (leg >= HexConfig.LegsCount) leg = HexConfig.LegsCount - 1;
                        break;
                    case T_Z:
                        ServoOffset[leg * 3] = 0;
                        break;
                    case F_Z:
                        ServoOffset[leg * 3 + 1] = 0;
                        break;
                    case C_Z:
                        ServoOffset[leg * 3 + 2] = 0;
                        break;
                    case T_UP:
                        if (Math.Abs(ServoOffset[leg * 3]) < offsetMax)
                            ServoOffset[leg * 3] = ServoOffset[leg * 3] + offsetStep;
                        break;
                    case T_DN:
                        if (Math.Abs(ServoOffset[leg * 3]) < offsetMax)
                            ServoOffset[leg * 3] = ServoOffset[leg * 3] - offsetStep;
                        break;
                    case F_UP:
                        if (Math.Abs(ServoOffset[leg * 3]) < offsetMax)
                            ServoOffset[leg * 3 + 1] = ServoOffset[leg * 3 + 1] + offsetStep;
                        break;
                    case F_DN:
                        if (Math.Abs(ServoOffset[leg * 3 + 1]) < offsetMax)
                            ServoOffset[leg * 3 + 1] = ServoOffset[leg * 3 + 1] - offsetStep;
                        break;
                    case C_UP:
                        if (Math.Abs(ServoOffset[leg * 3 + 2]) < offsetMax)
                            ServoOffset[leg * 3 + 2] = ServoOffset[leg * 3 + 2] + offsetStep;
                        break;
                    case C_DN:
                        if (Math.Abs(ServoOffset[leg * 3 + 2]) < offsetMax)
                            ServoOffset[leg * 3 + 2] = ServoOffset[leg * 3 + 2] - offsetStep;
                        break;
                    case TFC_SAVE:
                        File.WriteAllText("offsets.txt", string.Join(',', ServoOffset));
                        Console.WriteLine("Offsets - Saved!");
                        break;
                }
                if (last_leg >= 0)
                {
                    ServoPos[last_leg * 3] = -700;
                    ServoPos[last_leg * 3 + 1] = 700;
                    ServoPos[last_leg * 3 + 2] = 0;
                    ServoPos[leg * 3] = 0;
                    ServoPos[leg * 3 + 1] = 0;
                    ServoPos[leg * 3 + 2] = 0;
                    last_leg = -1;
                }
                if (key != TFC_SAVE)
                {
                    Console.WriteLine($"[{leg}] ({ServoOffset[leg * 3]}) ({ServoOffset[leg * 3 + 1]}) ({ServoOffset[leg * 3 + 2]})");
                    UpdateServos(sd, 0);
                }
            }

            Console.ReadLine();
        }
    }
}
