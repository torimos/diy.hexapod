using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static FrameProtocol fp = new FrameProtocol();
        static byte[] _buffer;
        static UInt64 _rawState;
        static GamePadState state;
        static Stopwatch sw = new Stopwatch();
        static long t, p = 0, pps = 0,err=0;
       
        static void Emulate()
        {
            var sp = new SerialPort("COM1", 9600, 200);
            if (sp.Open())
            {
                _rawState = 1;
                _buffer = new byte[] { 0xFD, 0x44, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                var rnd = new Random();
                int offset = 0;
                while (!Console.KeyAvailable)
                {
                    if (_rawState % 4 == 0) offset = rnd.Next(8);
                    else offset = 0;
                    _buffer[7] = (byte)(_rawState & 0xFF);
                    _buffer[6] = (byte)((_rawState >> 8) & 0xFF);
                    _buffer[5] = (byte)((_rawState >> 16) & 0xFF);
                    _buffer[4] = (byte)((_rawState >> 32) & 0xFF);
                    sp.Write(_buffer, 0, _buffer.Length);
                    Thread.Sleep(1000);
                    Console.WriteLine($"#{_rawState}");
                    _rawState++;
                }
                sp.Close();
            }
        }

        static void Test()
        {
            var port = new SerialPort("COM4", 9600, 200) { ReadChunkSize = 8*2 };
            port.DataReceived += Port_DataReceived;

            if (!port.Open(true))
            {
                Console.WriteLine("ConnERROR!");
                return;
            }
            while (!Console.KeyAvailable)
            {
                if ((sw.ElapsedMilliseconds - t) >= 1000)
                {
                    pps = p;
                    p = 0;
                    t = sw.ElapsedMilliseconds;
                }

                state = GamePadState.Parse(_rawState);
                Console.SetCursorPosition(0, 0);
                state.DebugOutput();
                Console.WriteLine($"pps: {pps,4}. errors: {err,4}");
            }
            port.Close();
            p = 0xDEAFBEAF;
        }

        private static void Port_DataReceived(object sender, SerialPort.PortDataReceivedEventArgs e)
        {
            for(int i=0;i<e.Data.Length;i++)
            {
                if (fp.rx_pool(e.Data[i]) > 0)
                {
                    var buff = fp.GetBuffer();
                    _rawState = BitConverter.ToUInt64(buff, 0);
                    err = fp.GetErrorsCount();
                    p++;
                }
            }
        }

        static void Main(string[] args)
        {
            sw.Start();
            Test();
        }
    }
}
