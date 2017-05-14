using System;
using System.Threading;
using SlimDX.XInput;
using SlimDX.DirectInput;
using System.Linq;
using System.Diagnostics;
using ServoCommander.Data;

namespace ServoCommander.Drivers
{
    public class GamepadEx
    {
        public static bool Emulated { get; set; }
        public bool Terminate { get; internal set; }

        public short RightThumbY { get; set; }
        public short RightThumbX { get; set; }
        public short LeftThumbY { get; set; }
        public short LeftThumbX { get; set; }
        public byte RightTrigger { get; set; }
        public byte LeftTrigger { get; set; }
        public GamepadButtonFlags Buttons { get; set; }
        public static int ThumbOffsetValue { get; internal set; }
        public int ThumbOffset
        {
            get
            {
                return ThumbOffsetValue;
            }
            set
            {
                ThumbOffsetValue = value;
            }
        }

        public XY GetLeftThumbPos(int scale)
        {
            return GetScaledPos(short.MaxValue, scale, LeftThumbX, LeftThumbY, Gamepad.GamepadLeftThumbDeadZone);
        }

        public XY GetRightThumbPos(int scale)
        {
            return GetScaledPos(short.MaxValue, scale, RightThumbX, RightThumbY, Gamepad.GamepadRightThumbDeadZone);
        }

        public int GetLeftTriggerPos(int scale)
        {
            return (int)GetScaledPos(byte.MaxValue, scale, LeftTrigger, 0, 0).x;
        }
        public int GetRightTriggerPos(int scale)
        {
            return (int)GetScaledPos(byte.MaxValue, scale, RightTrigger, 0, 0).x;
        }

        private XY GetScaledPos(int maxValue, int scale, int xpos, int ypos, int deadZone)
        {
            int scaleAxis = maxValue / scale;
            int absTx = Math.Abs(xpos);
            int absTy = Math.Abs(ypos);
            int x = Math.Sign(xpos) * Math.Min((absTx > deadZone ? absTx : 0) / scaleAxis, scale);
            int y = Math.Sign(ypos) * Math.Min((absTy > deadZone ? absTy : 0) / scaleAxis, scale);
            return new XY(x, y);
        }

        public bool IsButtonPressed(GamepadButtonFlags flag, int delayMilliseconds = 0)
        {
            bool f = Buttons.HasFlag(flag);
            if (delayMilliseconds > 0) Thread.Sleep(delayMilliseconds);
            return f;
        }
        public bool IsButtonPressedOnly(GamepadButtonFlags flag, int delayMilliseconds = 0)
        {
            bool f = Buttons == flag;
            if (delayMilliseconds > 0) Thread.Sleep(delayMilliseconds);
            return f;
        }
    }

    public static class GamepadExtension
    {
        public static GamepadEx GetGamepadState(this State state, Keyboard keyboard, Stopwatch stopwatch)
        {
            var gamepad = new GamepadEx
            {
                Buttons = state.Gamepad.Buttons,
                LeftThumbX = state.Gamepad.LeftThumbX,
                LeftThumbY = state.Gamepad.LeftThumbY,
                RightThumbX = state.Gamepad.RightThumbX,
                RightThumbY = state.Gamepad.RightThumbY,
                LeftTrigger = state.Gamepad.LeftTrigger,
                RightTrigger = state.Gamepad.RightTrigger,
            };
            ProcessKeyboard(keyboard, gamepad, stopwatch);
            
            return gamepad;
        }
        private static void ProcessKeyboard(Keyboard keyboard, GamepadEx gamepad, Stopwatch stopwatch)
        {
            var state = keyboard.GetCurrentState();
            gamepad.Terminate = state.IsPressed(Key.Escape);
            if (state.IsPressed(Key.F12))
            {
                GamepadEx.Emulated = !GamepadEx.Emulated;
                Thread.Sleep(200);
            }
           
            if (GamepadEx.Emulated)
            {
                var keysCount = state.PressedKeys.Count();
                if (keysCount == 0 || ((state.IsPressed(Key.LeftControl) || state.IsPressed(Key.LeftAlt)) && keysCount == 1))
                {
                    stopwatch.Restart();
                }
                gamepad.ThumbOffset = stopwatch.ElapsedMilliseconds > 0 ? short.MaxValue * 95 / 100 : 0;// (short)(stopwatch.ElapsedMilliseconds*5);
                if (gamepad.ThumbOffset > short.MaxValue) gamepad.ThumbOffset = short.MaxValue;

                if (state.IsPressed(Key.LeftControl))
                {
                    gamepad.LeftThumbY = (short)(state.IsPressed(Key.UpArrow) ? gamepad.ThumbOffset : 0);
                    gamepad.LeftThumbY += (short)(state.IsPressed(Key.DownArrow) ? -gamepad.ThumbOffset : 0);
                    gamepad.LeftThumbX = (short)(state.IsPressed(Key.LeftArrow) ? -gamepad.ThumbOffset : 0);
                    gamepad.LeftThumbX += (short)(state.IsPressed(Key.RightArrow) ? gamepad.ThumbOffset : 0);
                }
                else if (state.IsPressed(Key.LeftAlt))
                {
                    gamepad.RightThumbY = (short)(state.IsPressed(Key.UpArrow) ? gamepad.ThumbOffset : 0);
                    gamepad.RightThumbY += (short)(state.IsPressed(Key.DownArrow) ? -gamepad.ThumbOffset : 0);
                    gamepad.RightThumbX = (short)(state.IsPressed(Key.LeftArrow) ? -gamepad.ThumbOffset : 0);
                    gamepad.RightThumbX += (short)(state.IsPressed(Key.RightArrow) ? gamepad.ThumbOffset : 0);
                }
                else
                {
                    gamepad.Buttons |= state.IsPressed(Key.UpArrow) ? GamepadButtonFlags.DPadUp : 0;
                    gamepad.Buttons |= state.IsPressed(Key.DownArrow) ? GamepadButtonFlags.DPadDown : 0;
                    gamepad.Buttons |= state.IsPressed(Key.LeftArrow) ? GamepadButtonFlags.DPadLeft : 0;
                    gamepad.Buttons |= state.IsPressed(Key.RightArrow) ? GamepadButtonFlags.DPadRight : 0;
                }
                gamepad.Buttons |= state.IsPressed(Key.Return) ? GamepadButtonFlags.Start : 0;
                gamepad.Buttons |= state.IsPressed(Key.Tab) ? GamepadButtonFlags.Back : 0;

                gamepad.Buttons |= state.IsPressed(Key.F1) ? GamepadButtonFlags.X : 0;
                gamepad.Buttons |= state.IsPressed(Key.F2) ? GamepadButtonFlags.Y : 0;
                gamepad.Buttons |= state.IsPressed(Key.F3) ? GamepadButtonFlags.A : 0;
                gamepad.Buttons |= state.IsPressed(Key.F4) ? GamepadButtonFlags.B : 0;
                gamepad.Buttons |= state.IsPressed(Key.F5) ? GamepadButtonFlags.LeftShoulder : 0;
                gamepad.Buttons |= state.IsPressed(Key.F6) ? GamepadButtonFlags.LeftThumb : 0;
                gamepad.Buttons |= state.IsPressed(Key.F7) ? GamepadButtonFlags.RightShoulder : 0;
                gamepad.Buttons |= state.IsPressed(Key.F8) ? GamepadButtonFlags.RightThumb : 0;

                
            }
        }
    }
}
