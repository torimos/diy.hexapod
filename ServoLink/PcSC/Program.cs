using System;
using Drivers;
using Hexapod;

namespace ServoCommander
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(120, 60);
            using (var ctrl = new Controller())
            {
                ctrl.Setup();
                while (true)
                {
                    if (ctrl.Loop()) break;
                }
            }
        }
    }
}
