using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServoCommander
{

// LF           RF
// 0 1500+20    31 1500-50
// 1 2260-30    30 740-60
// 2 1500-10    29 1500

// LM           RM
// 4 1500-5     27 1500+70
// 5 2260+40    26 740-120
// 6 1500-25    25 1500-20

// LB           RB
// 8 1500+5     23 1500+20
// 9 2260+120   22 740-95
// 10 1500      21 1500+50

    partial class Program
    {
        static SerialPort _port;
        static byte[] _buff = new byte[1024];

        static int[] servos = new int[18];
        static int[] last_servos = new int[18];
        static int[] sdev = new int[18] {20,-30,-10, -5,40,-25, 5,120,0, +50,-95,20, -20,-120,70, 0,-60,-50};
        static int[] sinv = new int[18] {1,-1,1, 1,-1,1, 1,-1,1, -1,1,-1, -1,1,-1, -1,1,-1};
        static int[] smap = new int[18] {0,1,2, 4,5,6, 8,9,10, 31,30,29, 27,26,25, 23,22,21};
        static string last_cmd;
        static string cmd;


        static void sendCmd(string cmd)
        {
            Console.WriteLine($"Sending cmd: {cmd}");
            _port.Write(cmd);
            Thread.Sleep(5);
            _port.Write("\r\n");
        }

        static string GetUpdatedCommand()
        {
            var x = new StringBuilder();
            int xnew = 0;
            for(int i=0;i<18;i++)
            {
                if (last_servos[i] == servos[i]) continue;
                uint pw = (uint)((1500 + sdev[i]) + servos[i]*sinv[i]);
                uint st = 0;
                x.Append($"#{1+smap[i]}P{pw}T{st}");
                //Console.Write($"{smap[i]}:{pw} ");
                last_servos[i] = servos[i];
                xnew ++;
            }
            //Console.WriteLine();
            if (xnew > 0)
                x.Append("\r\n");
            return x.ToString();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Open port");
            _port = new SerialPort("COM15", 115200);
            _port.DataReceived += OnDataReceived;
            _port.Open();
            Thread.Sleep(100);
            int sid = 0;
            const int smax = 750;
            for (int i=0;i<18;i++) servos[i] = 0;
            servos[1]=servos[4]=servos[7]=servos[10]=servos[13]=servos[16] = -smax;
            last_cmd = GetUpdatedCommand();
            Timer tm = new Timer(tc, null, 0, 20);
            Console.WriteLine($"{sid}:{servos[sid]}");
            while(true){
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        sid+=1;
                        if (sid>17) sid=0;
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        sid-=1;
                        if (sid<0) sid=17;
                    }
                    int pw = servos[sid];
                    if (key.Key == ConsoleKey.RightArrow)
                    {
                        pw+=50;
                        if (pw>smax) pw = smax;
                    }
                    else if (key.Key == ConsoleKey.LeftArrow)
                    {
                        pw-=50;
                        if (pw<-smax) pw = -smax;
                    }
                    servos[sid] = pw;
                    Console.WriteLine($"{sid}:{pw}");
                    cmd = GetUpdatedCommand();
                }
            }
            _port.Close();
            
            Console.WriteLine("Close port");
        }

        private static void tc(object state)
        {
            if (!string.IsNullOrEmpty(cmd) && last_cmd != cmd)
            {
                Console.WriteLine(cmd);
                _port.Write(cmd);
                last_cmd = cmd;
            }
        }

        private static void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            //Console.WriteLine($"{sp.ReadExisting()}");
        }
    }
}
