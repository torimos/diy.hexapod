using System;
using System.Threading;

namespace test
{

    [Flags]
    enum GamepadButtonFlags
    {
        None = 0,
        DPadUp = 1,
        DPadRight = 2,
        DPadDown = 4,
        DPadLeft = 8,
        B1 = 0x10,
        B2 = 0x20,
        B3 = 0x40,
        B4 = 0x80,
        B5 = 0x100,
        B6 = 0x200,
        B7 = 0x400,
        B8 = 0x800,
        B9 = 0x1000,
        B10 = 0x2000,
        LeftThumb = 0x4000,
        RightThumb = 0x8000,
        Vibration = 0x40000,
        Mode = 0x80000
    }

    struct GamePadState
    {
        public int LeftThumbX { get; set; }
        public int LeftThumbY { get; set; }
        public int RightThumbX { get; set; }
        public int RightThumbY { get; set; }
        public GamepadButtonFlags Buttons { get; set; }

        public static GamePadState Parse(UInt64 rawState)
        {
            var state = new GamePadState();
            ushort chk = (ushort)((rawState >> 48) & 0xFFF0);
            if (chk != 0xFD40) rawState = 0xFD40000080808080;
            state.Buttons = (GamepadButtonFlags)((rawState >> 32) & 0x000FFFFF);
            state.LeftThumbX = (byte)(rawState & 0xFF);
            state.LeftThumbY = (byte)((rawState >> 8) & 0xFF);
            state.RightThumbX = (byte)((rawState >> 16) & 0xFF);
            state.RightThumbY = (byte)((rawState >> 24) & 0xFF);
            return state;
        }

        public void DebugOutput()
        {
            Console.WriteLine($"Buttons: {Buttons,10}");
            Console.WriteLine($"Left: {LeftThumbX,3} {LeftThumbY,3}");
            Console.WriteLine($"Right: {RightThumbX,3} {RightThumbY,3}");
        }
    }

    class Program
    {
        static UInt64 _rawState;
        static void Main(string[] args)
        {
            var port = new SerialPort("COM6", 115200, 200) { ReadChunkSize = 8 };
            port.DataReceived += Port_DataReceived;

            if (!port.Open())
            {
                Console.WriteLine("ConnERROR!");
                return;
            }

            while(!Console.KeyAvailable)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                var state = GamePadState.Parse(_rawState);
                state.DebugOutput();
                Thread.Sleep(20);
            }
            port.Close();
        }

        private static void Port_DataReceived(object sender, SerialPort.PortDataReceivedEventArgs e)
        {
            _rawState = BitConverter.ToUInt64(e.Data, 0);
        }
    }
}
