using System;

namespace test
{
    struct GamePadState
    {
        public int LeftThumbX { get; set; }
        public int LeftThumbY { get; set; }
        public int RightThumbX { get; set; }
        public int RightThumbY { get; set; }
        public GamepadButtonFlags Buttons { get; set; }

        public UInt64 RawState { get; set; }
        public bool Error { get; set; }

        public static GamePadState Parse(UInt64 rawState)
        {
            var state = new GamePadState();
            state.RawState = rawState;
            ushort chk = (ushort)((rawState >> 48) & 0xFFF0);
            state.Error = chk != 0xFD40;
            if (state.Error) rawState = 0xFD40000080808080;
            state.Buttons = (GamepadButtonFlags)((rawState >> 32) & 0x000FFFFF);
            state.LeftThumbX = (byte)(rawState & 0xFF);
            state.LeftThumbY = (byte)((rawState >> 8) & 0xFF);
            state.RightThumbX = (byte)((rawState >> 16) & 0xFF);
            state.RightThumbY = (byte)((rawState >> 24) & 0xFF);
            
            return state;
        }

        public void DebugOutput()
        {
            Console.WriteLine($"RAW: {RawState:X}");
            Console.WriteLine($"Buttons: {Buttons,10}");
            Console.WriteLine($"Left: {LeftThumbX,3} {LeftThumbY,3}");
            Console.WriteLine($"Right: {RightThumbX,3} {RightThumbY,3}");
        }
    }
}
