using System;
using ServoCommander.Data;
using ServoLink.Contracts;
using ServoLink;
using System.Threading;

namespace ServoCommander.Drivers
{
    public class SerialInputDriver : IInputDriver
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
                state.LeftThumbX = (byte)(rawState & 0xFF) + 8;
                state.LeftThumbY = (byte)((rawState >> 8) & 0xFF) - 21;
                state.RightThumbX = (byte)((rawState >> 16) & 0xFF);
                state.RightThumbY = (byte)((rawState >> 24) & 0xFF);
                if (state.LeftThumbX < 0) state.LeftThumbX = 0;
                if (state.LeftThumbX > 0xFF) state.LeftThumbX = 0xFF;
                if (state.LeftThumbY < 0) state.LeftThumbY = 0;
                if (state.LeftThumbY > 0xFF) state.LeftThumbY = 0xFF;
                return state;
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

        private IPort _port;

        private UInt64 _rawState = 0xFC00000880808080;

        public SerialInputDriver()
        {
            _port = new SerialPort("COM9", 9600, 200) { ReadChunkSize = 8 };
            bool opened = !_port.IsOpen ? _port.Open() : _port.IsOpen;
            if (opened)
            {
                _port.DataReceived += Serial_DataReceived;
            }
        }
        private GamePadState? State { get; set; }
        private GamePadState? PrevState { get; set; }

        public bool Terminate { get; set; }

        private bool HasPressed(GamepadButtonFlags button)
        {
            return State?.Buttons.HasFlag(button) == true && PrevState?.Buttons.HasFlag(button) == false;
        }
        private bool HasPressedOnly(GamepadButtonFlags button)
        {
            return State?.Buttons == button && PrevState?.Buttons != button;
        }

        public void ProcessInput(HexModel model)
        {
            var adjustLegsPosition = false;

            if (Console.KeyAvailable)
            {
                Terminate = Console.ReadKey(false).Key == ConsoleKey.Escape;
            }
            State = GetCurrentState();
            if (PrevState == null) PrevState = State;

            XY thumbLeft = new XY { x = State.HasValue ? State.Value.LeftThumbX - 128 : 0, y = - (State.HasValue ? State.Value.LeftThumbY - 128 : 0) };
            XY thumbRight = new XY { x = State.HasValue ? State.Value.RightThumbX - 128 : 0, y = - (State.HasValue ? State.Value.RightThumbY - 128 : 0) };

            if (HasPressed(GamepadButtonFlags.B10))
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
                if (HasPressed(GamepadButtonFlags.B5))
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
                else if (HasPressed(GamepadButtonFlags.B6))
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
                else if (HasPressed(GamepadButtonFlags.B3)) // Circle
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
                else if (HasPressed(GamepadButtonFlags.B2)) // Cross
                {
                    if (model.ControlMode != HexModel.ControlModeType.GPPlayer)
                    {
                        model.ControlMode = HexModel.ControlModeType.GPPlayer;
                        model.GPSeq = 0;
                    }
                    else
                        model.ControlMode = HexModel.ControlModeType.Walk;
                }
                else if (HasPressed(GamepadButtonFlags.B1)) // Square
                {
                    model.BalanceMode = !model.BalanceMode;
                }
                else if (HasPressed(GamepadButtonFlags.B4)) // Triangle
                {
                    if (model.BodyYOffset > 0)
                        model.BodyYOffset = 0;
                    else
                        model.BodyYOffset = HexConfig.BodyStandUpOffset;

                    adjustLegsPosition = true;
                }
                else if (State?.IsButtonPressed(GamepadButtonFlags.DPadUp, 50) == true)
                {
                    model.BodyYOffset += 5;
                    if (model.BodyYOffset > HexConfig.MaxBodyHeight)
                        model.BodyYOffset = HexConfig.MaxBodyHeight;
                    adjustLegsPosition = true;
                }
                else if (State?.IsButtonPressed(GamepadButtonFlags.DPadDown, 50) == true)
                {
                    if (model.BodyYOffset > 5)
                        model.BodyYOffset -= 10;
                    else
                        model.BodyYOffset = 0;
                    adjustLegsPosition = true;
                }
                else if (State?.IsButtonPressed(GamepadButtonFlags.DPadRight, 50) == true)
                {
                    if (model.Speed >= 50) model.Speed -= 50;
                }
                else if (State?.IsButtonPressed(GamepadButtonFlags.DPadLeft, 50) == true)
                {
                    if (model.Speed < 2000) model.Speed += 50;
                }

                model.BodyYShift = 0;
                if (model.ControlMode == HexModel.ControlModeType.Walk)
                {
                    if (model.BodyPos.y > 0)
                    {
                        if (HasPressed(GamepadButtonFlags.B9) &&
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

                        model.TravelLength.y = -thumbRight.x / 6; //Right Stick Left/Right 
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
                    model.BodyRot.x = thumbLeft.y;
                    model.BodyRot.y = thumbRight.y * 2;
                    model.BodyRot.z = -thumbLeft.x;
                    model.BodyYShift = thumbRight.y / 2;
                }
                else if (model.ControlMode == HexModel.ControlModeType.SingleLeg)
                {
                    if (HasPressed(GamepadButtonFlags.B9)) //Select
                    {
                        model.SelectedLeg++;
                        if (model.SelectedLeg >= HexConfig.LegsCount)
                        {
                            model.SelectedLeg = 0;
                        }
                    }

                    model.SingleLegHold = State?.IsButtonPressed(GamepadButtonFlags.B6) == true;
                    model.SingleLegPos.x = thumbLeft.x; //Left Stick Right/Left
                    model.SingleLegPos.y = -thumbRight.y; //Right Stick Up/Down
                    model.SingleLegPos.z = thumbLeft.y; //Left Stick Up/Down
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

        public void DebugOutput()
        {
            var state = GetCurrentState();
            Console.WriteLine($"Buttons: {state.Buttons,10}");
            Console.WriteLine($"Left: {state.LeftThumbX,3} {state.LeftThumbY,3}");
            Console.WriteLine($"Right: {state.RightThumbX,3} {state.RightThumbY,3}");
        }

        public void Release()
        {
            _port.Close();
        }

        private GamePadState GetCurrentState()
        {
            return GamePadState.Parse(_rawState);
        }

        private void Serial_DataReceived(object sender, PortDataReceivedEventArgs e)
        {
            _rawState = BitConverter.ToUInt64(e.Data, 0);
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

    }
}
