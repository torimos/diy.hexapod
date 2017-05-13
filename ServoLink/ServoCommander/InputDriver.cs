using System;
using System.Threading;
using SlimDX.XInput;
using SlimDX.DirectInput;
using System.Linq;
using System.Diagnostics;

namespace ServoCommander
{

    public class InputDriver
    {
        private Controller _controller;
        private Keyboard _keyboard;
        public GamepadEx State { get; set; }
        public GamepadEx PrevState { get; set; }
        public bool Terminate { get; set; }

        private Stopwatch _stopWatch = new Stopwatch();
        public InputDriver()
        {
            _keyboard = new Keyboard(new DirectInput());
            _keyboard.Acquire();

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
            State = _controller.GetState().GetGamepadState(_keyboard, _stopWatch);
            if (PrevState == null) PrevState = State;

            XY thumbLeft = State.GetLeftThumbPos(127);
            XY thumbRight = State.GetRightThumbPos(127);
            int triggerLeft = State.GetLeftTriggerPos(127);
            int triggerRight = State.GetRightTriggerPos(127);

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
                    model.AdjustLegsPosition = true;
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

                    model.AdjustLegsPosition = true;
                }
                else if(State.IsButtonPressed(GamepadButtonFlags.DPadUp, 50))
                {
                    model.BodyYOffset += 5;
                    if (model.BodyYOffset > HexConfig.MaxBodyHeight)
                        model.BodyYOffset = HexConfig.MaxBodyHeight;
                    model.AdjustLegsPosition = true;
                }
                else if (State.IsButtonPressed(GamepadButtonFlags.DPadDown, 50))
                {
                    if (model.BodyYOffset > 5)
                        model.BodyYOffset -= 10;
                    else
                        model.BodyYOffset = 0;
                    model.AdjustLegsPosition = true;
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
                            model.TravelLength.z = thumbRight.y; //Right Stick Up/Down  
                        else
                        {
                            model.TravelLength.x = -thumbLeft.x;
                            model.TravelLength.z = thumbLeft.y;
                        }

                        if (!model.DoubleTravelOn)
                        {  //(Double travel length)
                            model.TravelLength.x = model.TravelLength.x / 2;
                            model.TravelLength.z = model.TravelLength.z / 2;
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
                    model.BodyPos.z = -thumbLeft.y / 3;
                    model.BodyRot.y = thumbRight.x * 2;
                    model.BodyYShift = -thumbRight.y / 2;
                }
                else if (model.ControlMode == HexModel.ControlModeType.Rotate)
                {
                    model.BodyRot.x = thumbLeft.x;
                    model.BodyRot.y = thumbRight.x * 2;
                    model.BodyRot.z = thumbLeft.y;
                    model.BodyYShift = -thumbRight.y / 2;
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
                    model.SingleLegPos.x = thumbLeft.x / 2; //Left Stick Right/Left
                    model.SingleLegPos.y = thumbRight.y / 5; //Right Stick Up/Down
                    model.SingleLegPos.z = thumbLeft.y / 2; //Left Stick Up/Down
                }

                model.InputTimeDelay = 128 - (int)Math.Max(Math.Max(Math.Abs(thumbLeft.x), Math.Abs(thumbLeft.y)), Math.Abs(thumbRight.x));
            }

            model.BodyPos.y = Math.Min(Math.Max(model.BodyYOffset + model.BodyYShift, 0), HexConfig.MaxBodyHeight);
            PrevState = State;
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
    }
}
