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

            //var sp = new SerialPort("COM4", 115200);
            //if (sp.Open())
            //{

            //    var h = new BinaryHelper();
            //    var data = h.ConvertToByteArray((uint)6, new ushort[] { 5, 6, 7 });
            //    sp.Write(data, 0, data.Length);

            //    data = h.ConvertToByteArray((uint)6, new ushort[] { 1005, 1006, 1007 });
            //    sp.Write(data, 0, data.Length);

            //    data = h.ConvertToByteArray((uint)6, new ushort[] { 10005, 10006, 10007 });
            //    sp.Write(data, 0, data.Length);

            //    sp.Close();
            //}

            var sc = new ServoController(20, new BinaryHelper());
            if (!sc.Connect(new SerialPort("COM4", 115200))) return;
            Console.WriteLine("Connected");
            //for (var i = 0; i < sc.Servos.Length; i++)
            //{
            //    sc.Servos[i] = (ushort)i;
            //}
            //sc.Sync();


            sc.SetAll(0);
            sc.Sync();
            int t = 500;
            while (!Console.KeyAvailable)
            {
                //sc.Servos[0] = sc.Servos[3] = sc.Servos[6] = 1700;//end
                //sc.Servos[1] = sc.Servos[4] = sc.Servos[7] = 1700;//mid
                //sc.Servos[2] = sc.Servos[5] = sc.Servos[8] = 1500;//beg
                //sc.Servos[9] = sc.Servos[12] = sc.Servos[15] = 1200;//end
                //sc.Servos[10] = sc.Servos[13] = sc.Servos[16] = 1200;//mid
                //sc.Servos[11] = sc.Servos[14] = sc.Servos[17] = 1500;//beg
                //sc.Sync();
                //Thread.Sleep(t);

                //sc.Servos[0] = sc.Servos[3] = sc.Servos[6] = 1200;//end
                //sc.Servos[1] = sc.Servos[4] = sc.Servos[7] = 1200;//mid
                //sc.Servos[2] = sc.Servos[5] = sc.Servos[8] = 1500;//beg
                //sc.Servos[9] = sc.Servos[12] = sc.Servos[15] = 1700;//end
                //sc.Servos[10] = sc.Servos[13] = sc.Servos[16] = 1700;//mid
                //sc.Servos[11] = sc.Servos[14] = sc.Servos[17] = 1500;//beg
                //sc.Sync();
                //Thread.Sleep(t);


                sc.Servos[0] = 1500;
                sc.Servos[1] = 1800;
                sc.Servos[2] = 1500;
                sc.Sync();
                Thread.Sleep(t);

                sc.Servos[0] = 1100;
                sc.Servos[1] = 1500;
                sc.Servos[2] = 1500;
                sc.Sync();
                Thread.Sleep(t);
                sc.Servos[0] = 2200;
                sc.Servos[1] = 1100;
                sc.Servos[2] = 1500;
                sc.Sync();
                Thread.Sleep(t);

                Console.WriteLine("Tick");
            }
            sc.Disconnect();
        }
    }
}
