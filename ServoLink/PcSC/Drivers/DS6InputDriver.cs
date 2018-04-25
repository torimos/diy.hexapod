using System;
using System.Linq;
using System.Threading;
using SlimDX.XInput;
using SlimDX.DirectInput;
using System.Diagnostics;
using Data;
using Contracts;

namespace Drivers
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

    public class DS6InputDriver : IInputDriver
    {
        private Controller _controller;
        public Keyboard Keyboard;
        public GamepadEx State { get; set; }
        public GamepadEx PrevState { get; set; }
        public bool Terminate { get; set; }

        private Stopwatch _stopWatch = new Stopwatch();
        public DS6InputDriver()
        {
            Keyboard = new Keyboard(new DirectInput());
            Keyboard.Acquire();

            _controller = new SlimDX.XInput.Controller(UserIndex.One);
            GamepadEx.Emulated = true;
            _stopWatch.Start();
        }

        private bool HasPressed(GamepadButtonFlags button)
        {
            return State.IsButtonPressed(button) && !PrevState.IsButtonPressed(button);
        }
        private bool HasPressedOnly(GamepadButtonFlags button)
        {
            return State.IsButtonPressedOnly(button) && !PrevState.IsButtonPressed(button);
        }

        public void ProcessInput(HexModel model)
        {
            var adjustLegsPosition = false;

            State = _controller.GetState().GetGamepadState(Keyboard, _stopWatch);
            if (PrevState == null) PrevState = State;

            XY thumbLeft = State.GetLeftThumbPos(127);
            XY thumbRight = State.GetRightThumbPos(127);
            XY thumbLeftPresize = State.GetLeftThumbPos(10000);
            XY thumbRightPresize = State.GetRightThumbPos(10000);

            if (State.Terminate)
            {
                Terminate = true;
            }

            if (HasPressed(GamepadButtonFlags.Start))
            {
                if (model.PowerOn)
                {
                    TurnOff(model);
                    model.PowerOn = false;
                }
                else
                {
                    model.PowerOn = true;
                    adjustLegsPosition = true;
                }
                Thread.Sleep(200);
            }
            else if (model.PowerOn)
            {
                if (HasPressed(GamepadButtonFlags.LeftShoulder))
                {
                    if (model.ControlMode != HexModel.ControlModeType.Translate)
                    {
                        model.ControlMode = HexModel.ControlModeType.Translate;
                    }
                    else if (model.SelectedLeg == 0xFF)
                    {
                        model.ControlMode = HexModel.ControlModeType.Walk;
                    }
                    else
                    {
                        model.ControlMode = HexModel.ControlModeType.SingleLeg;
                    }
                    Thread.Sleep(200);
                }
                else if (HasPressed(GamepadButtonFlags.RightShoulder))
                {
                    if (model.ControlMode != HexModel.ControlModeType.Rotate)
                    {
                        model.ControlMode = HexModel.ControlModeType.Rotate;
                    }
                    else if (model.SelectedLeg == 0xFF)
                    {
                        model.ControlMode = HexModel.ControlModeType.Walk;
                    }
                    else
                    {
                        model.ControlMode = HexModel.ControlModeType.SingleLeg;
                    }
                    Thread.Sleep(200);
                }
                else if (HasPressed(GamepadButtonFlags.B)) // Circle
                {
                    if ((Math.Abs(model.TravelLength.x) < HexConfig.TravelDeadZone)
                      || (Math.Abs(model.TravelLength.z) < HexConfig.TravelDeadZone)
                      || (Math.Abs(model.TravelLength.y) < HexConfig.TravelDeadZone))
                    {
                        if (model.ControlMode != HexModel.ControlModeType.SingleLeg)
                        {
                            model.ControlMode = HexModel.ControlModeType.SingleLeg;
                            if (model.SelectedLeg == 0xFF)  //Select leg if none is selected
                            {
                                model.SelectedLeg = 0; //Startleg
                            }
                        }
                        else
                        {
                            model.ControlMode = HexModel.ControlModeType.Walk;
                            model.SelectedLeg = 0xFF;
                        }
                    }
                }
                else if (HasPressed(GamepadButtonFlags.A)) // Cross
                {
                    if (model.ControlMode != HexModel.ControlModeType.GPPlayer)
                    {
                        model.ControlMode = HexModel.ControlModeType.GPPlayer;
                        model.GPSeq = 0;
                    }
                    else
                        model.ControlMode = HexModel.ControlModeType.Walk;
                }
                else if (HasPressed(GamepadButtonFlags.X)) // Square
                {
                    model.BalanceMode = !model.BalanceMode;
                }
                else if (HasPressed(GamepadButtonFlags.Y)) // Triangle
                {
                    if (model.BodyYOffset > 0)
                        model.BodyYOffset = 0;
                    else
                        model.BodyYOffset = HexConfig.BodyStandUpOffset;

                    adjustLegsPosition = true;
                }
                else if(State.IsButtonPressed(GamepadButtonFlags.DPadUp, 50))
                {
                    model.BodyYOffset += 5;
                    if (model.BodyYOffset > HexConfig.MaxBodyHeight)
                        model.BodyYOffset = HexConfig.MaxBodyHeight;
                    adjustLegsPosition = true;
                }
                else if (State.IsButtonPressed(GamepadButtonFlags.DPadDown, 50))
                {
                    if (model.BodyYOffset > 5)
                        model.BodyYOffset -= 10;
                    else
                        model.BodyYOffset = 0;
                    adjustLegsPosition = true;
                }
                else if (State.IsButtonPressed(GamepadButtonFlags.DPadRight, 50))
                {
                    if (model.Speed >= 50) model.Speed -= 50;
                }
                else if (State.IsButtonPressed(GamepadButtonFlags.DPadLeft, 50))
                {
                    if (model.Speed < 2000) model.Speed += 50;
                }

                model.BodyYShift = 0;
                if (model.ControlMode == HexModel.ControlModeType.Walk)
                {
                    if (model.BodyPos.y > 0)
                    {
                        if (HasPressed(GamepadButtonFlags.Back) &&
                            Math.Abs(model.TravelLength.x) < HexConfig.TravelDeadZone //No movement
                            && Math.Abs(model.TravelLength.z) < HexConfig.TravelDeadZone
                            && Math.Abs(model.TravelLength.y * 2) < HexConfig.TravelDeadZone) //Select
                        {
                            model.GaitType++;
                            if ((int)model.GaitType >= model.Gaits.Keys.Count)
                            {
                                model.GaitType = GaitType.Ripple12;
                            }
                            model.gaitCur = model.Gaits[model.GaitType];
                        }
                        else if (HasPressedOnly(GamepadButtonFlags.LeftThumb)) //Double leg lift height
                        {
                            model.DoubleHeightOn = !model.DoubleHeightOn;
                            if (model.DoubleHeightOn)
                                model.LegLiftHeight = HexConfig.LegLiftDoubleHeight;
                            else
                                model.LegLiftHeight = HexConfig.LegLiftHeight;
                        }
                        else if (HasPressedOnly(GamepadButtonFlags.RightThumb)) //Double Travel Length
                        {
                            model.DoubleTravelOn = !model.DoubleTravelOn;
                        }
                        else if (HasPressed(GamepadButtonFlags.LeftThumb | GamepadButtonFlags.RightThumb)) // Switch between Walk method 1 && Walk method 2
                        {
                            model.WalkMethod = !model.WalkMethod;
                        }

                        //Walking
                        if (model.WalkMethod)  //(Walk Methode) 
                            model.TravelLength.z = -thumbRight.y; //Right Stick Up/Down  
                        else
                        {
                            model.TravelLength.x = -thumbLeft.x;
                            model.TravelLength.z = -thumbLeft.y;
                        }

                        if (!model.DoubleTravelOn)
                        {  //(Double travel length)
                            model.TravelLength.x = model.TravelLength.x / 1.75;
                            model.TravelLength.z = model.TravelLength.z / 1.75;
                        }

                        model.TravelLength.y = -thumbRight.x / 4; //Right Stick Left/Right 
                    }
                    else
                    {
                        Console.WriteLine("!!!Lift hexapod UP first!!!");
                    }
                }
                else if (model.ControlMode == HexModel.ControlModeType.Translate)
                {
                    model.BodyPos.x = thumbLeft.x / 2;
                    model.BodyPos.z = thumbLeft.y / 3;
                    model.BodyRot.y = thumbRight.x * 2;
                    model.BodyYShift = -thumbRight.y / 2;
                }
                else if (model.ControlMode == HexModel.ControlModeType.Rotate)
                {
                    model.BodyRot.x = thumbLeft.x;
                    model.BodyRot.y = thumbRight.x * 2;
                    model.BodyRot.z = -thumbLeft.y;
                    model.BodyYShift = thumbRight.y / 2;
                }
                else if (model.ControlMode == HexModel.ControlModeType.SingleLeg)
                {
                    if (HasPressed(GamepadButtonFlags.Back)) //Select
                    {
                        model.SelectedLeg++;
                        if (model.SelectedLeg >= HexConfig.LegsCount)
                        {
                            model.SelectedLeg = 0;
                        }
                    }

                    model.SingleLegHold = State.IsButtonPressed(GamepadButtonFlags.RightShoulder);
                    model.SingleLegPos.x = thumbLeftPresize.x / 100; //Left Stick Right/Left
                    model.SingleLegPos.y = -thumbRightPresize.y / 100; //Right Stick Up/Down
                    model.SingleLegPos.z = thumbLeftPresize.y / 100; //Left Stick Up/Down
                }
                model.InputTimeDelay = 128 - (int)Math.Max(Math.Max(Math.Abs(thumbLeft.x), Math.Abs(thumbLeft.y)), Math.Max(Math.Abs(thumbRight.x), Math.Abs(thumbRight.y)));
            }

            model.BodyPos.y = Math.Min(Math.Max(model.BodyYOffset + model.BodyYShift, 0), HexConfig.MaxBodyHeight);
            if (adjustLegsPosition)
            {
                AdjustLegPositionsToBodyHeight(model);
            }
            PrevState = State;
        }
        private void AdjustLegPositionsToBodyHeight(HexModel model)
        {
            const double MIN_XZ_LEG_ADJUST = HexConfig.CoxaLength;
            const double MAX_XZ_LEG_ADJUST = HexConfig.CoxaLength + HexConfig.TibiaLength + HexConfig.FemurLength / 4;
            double[] hexIntXZ = { 111, 88, 86 };
            double[] hexMaxBodyY = { 20, 50, HexConfig.MaxBodyHeight };

            // Lets see which of our units we should use...
            // Note: We will also limit our body height here...
            model.BodyPos.y = Math.Min(model.BodyPos.y, HexConfig.MaxBodyHeight);
            double XZLength = hexIntXZ[2];
            int i;
            for (i = 0; i < 2; i++)
            {    // Don't need to look at last entry as we already init to assume this one...
                if (model.BodyPos.y <= hexMaxBodyY[i])
                {
                    XZLength = hexIntXZ[i];
                    break;
                }
            }
            if (i != model.LegInitIndex)
            {
                model.LegInitIndex = i;  // remember the current index...

                //now lets see what happens when we change the leg positions...
                if (XZLength > MAX_XZ_LEG_ADJUST)
                    XZLength = MAX_XZ_LEG_ADJUST;
                if (XZLength < MIN_XZ_LEG_ADJUST)
                    XZLength = MIN_XZ_LEG_ADJUST;


                // see if same length as when we came in
                if (XZLength == model.LegsXZLength)
                    return;

                model.LegsXZLength = XZLength;

                for (var legIndex = 0; legIndex < HexConfig.LegsCount; legIndex++)
                {
                    model.LegsPos[legIndex].x = Math.Cos(Math.PI * HexConfig.CoxaDefaultAngle[legIndex] / 180) * XZLength;  //Set start positions for each leg
                    model.LegsPos[legIndex].z = -Math.Sin(Math.PI * HexConfig.CoxaDefaultAngle[legIndex] / 180) * XZLength;
                }

                // Make sure we cycle through one gait to have the legs all move into their new locations...
                model.ForceGaitStepCnt = model.gaitCur.StepsInGait;
            }
        }

        void TurnOff(HexModel model)
        {
            model.BodyPos.x = 0;
            model.BodyPos.y = 0;
            model.BodyPos.z = 0;
            model.BodyRot.x = 0;
            model.BodyRot.y = 0;
            model.BodyRot.z = 0;
            model.TravelLength.x = 0;
            model.TravelLength.z = 0;
            model.TravelLength.y = 0;
            model.BodyYOffset = 0;
            model.BodyYShift = 0;
            model.SelectedLeg = 255;
        }

        public void Release()
        {
            _controller = null;
        }

        public void DebugOutput()
        {
            if (GamepadEx.Emulated)
            {
                Console.WriteLine("GAME PAD - EMULATION");
            }
            Console.WriteLine($"Buttons: {State?.Buttons,10}");
            Console.WriteLine($"Left: {State?.GetLeftThumbPos(127)}");
            Console.WriteLine($"Right: {State?.GetRightThumbPos(127)}");
            Console.WriteLine($"LeftTrigger: {State?.GetLeftTriggerPos(127)}");
            Console.WriteLine($"RightTrigger: {State?.GetRightTriggerPos(127)}");
        }
    }
}
