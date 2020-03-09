using System;
using System.Linq;
using Drivers;
using Hexapod;
using PcSC.Hexapod;

namespace ServoCommander
{
    partial class Program
    {

        static void Main(string[] args)
        {
            //Console.SetWindowSize(120, 42);
            //using (var ctrl = new Controller())
            //{
            //    ctrl.Setup();
            //    while (true)
            //    {
            //        if (ctrl.Loop()) break;
            //

            //CallibrateHelper.Run();

            ServoDriver sd = new ServoDriver(20);
            sd.Init("COM13");
            sd.Move(18, 1500);
            sd.Commit();
        }

        private static void Sin_DataReceived(object sender, Contracts.PortDataReceivedEventArgs e)
        {
            Console.WriteLine(string.Join(' ', e.Data.Select(x=>$"{x:X}")));
        }
    }
}
