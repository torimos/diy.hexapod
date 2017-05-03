using System;
using SlimDX.DirectInput;
using System.Globalization;
using System.Threading;
using System.Linq;

namespace ServoCommander
{
    public enum GamePadButton
    {
        Cross = 0,
        Circle = 1,
        Square = 2,
        Triangle = 3,
        FuncLeft1 = 4,
        FuncRight1 = 5,
        Select = 6,
        Start = 7,
        LeftStick = 8,
        RightStick = 9,
    }
    public enum CrossButton
    {
        None = -1,
        Up = 0x0000,// 0x0000 UP
        Down = 0x4650,// 0x4650 DOWN
        Left = 0x6978,// 0x6978 LEFT
        Right = 0x2328,// 0x2328 RIGHT
        UpLeft = 0x7B07, // 0x7B07 UP+LEFT
        UpRight = 0x1194,// 0x1194 UP+RIGHT
        DownLeft = 0x57E4,// 0x57E4 DOWN+LEFT
        DownRight = 0x34BC// 0x34BC DOWN+RIGHT
    }

    public static class JoystickExtension
    {
        public static bool IsGamePadPressed(this JoystickState state, GamePadButton button)
        {
            return state.IsPressed((int)button);
        }
    }

    public class InputDriver
    {
        private Joystick Gamepad;
      
        public InputDriver()
        {
            DirectInput dinput = new DirectInput();
            DeviceInstance device = dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly).Last();
            // Create device
            try
            {
                Gamepad = new Joystick(dinput, device.InstanceGuid);
            }
            catch (DirectInputException)
            {
            }

            foreach (DeviceObjectInstance deviceObject in Gamepad.GetObjects())
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    Gamepad.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-128, 128);
            }
            Gamepad.Acquire();
        }

        private JoystickState GetGamePadState()
        {
            var state = new JoystickState();
            if (Gamepad != null && !Gamepad.Acquire().IsFailure && !Gamepad.Poll().IsFailure && !SlimDX.Result.Last.IsFailure)
            {
                state = Gamepad.GetCurrentState();
                Gamepad.Unacquire();
            }
            // L,R 2 - +/-RotZ
            return state;
        }

        public bool? ProcessInput(HexModel model)
        {
            bool isKeyPressed = Console.KeyAvailable;
            model.BodyYShift = 0;
            var state = GetGamePadState();
            var pointer = (state.GetPointOfViewControllers()[1] == -1) ? (CrossButton)state.GetPointOfViewControllers()[0] : CrossButton.None;
            //var buttons = state.GetButtons();
            //var strText = string.Empty;
            //for (int b = 0; b < buttons.Length; b++)
            //{
            //    if (buttons[b])
            //        strText += b.ToString("00 ", CultureInfo.CurrentCulture);
            //}
            //Console.WriteLine("L:{0,4}:{1,4}:{2,4} R:{3,4}:{4,4}:{5,4} {6,8:X}", state.X, state.Y, state.Z, state.RotationX, state.RotationY, state.RotationZ, strText);
            if (isKeyPressed)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Escape: return null;
                    case ConsoleKey.F1:
                        {
                            if (model.SelectedLeg == 0xFF) model.SelectedLeg = 0;
                            else
                            {
                                model.SelectedLeg++;
                                if (model.SelectedLeg == IKMathConfig.LegsCount)
                                {
                                    model.SelectedLeg = 0xFF;
                                }
                            }
                        }
                        break;
                }
            }

            if (state.IsGamePadPressed(GamePadButton.Start))
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
                Thread.Sleep(500);
            }

            if (model.PowerOn)
            {

                if (state.IsGamePadPressed(GamePadButton.Select))
                {
                    model.ControlMode = (HexModel.ControlModeType)(((int)model.ControlMode + 1) % 5);
                    Thread.Sleep(500);
                }
                else if (state.IsGamePadPressed(GamePadButton.Triangle))
                {
                    if (model.BodyYOffset > 0)
                        model.BodyYOffset = 0;
                    else
                        model.BodyYOffset = IKMathConfig.BodyStandUpOffset;
                    model.AdjustLegsPosition = true;
                    Thread.Sleep(500);
                }
                else if (pointer == CrossButton.Up)
                {
                    model.BodyYOffset += 10;
                    if (model.BodyYOffset > IKMathConfig.MaxBodyHeight)
                        model.BodyYOffset = IKMathConfig.MaxBodyHeight;
                    model.AdjustLegsPosition = true;
                }
                else if (pointer == CrossButton.Down)
                {
                    if (model.BodyYOffset > 10)
                        model.BodyYOffset -= 10;
                    else
                        model.BodyYOffset = 0;
                    model.AdjustLegsPosition = true;
                }
                else if (pointer == CrossButton.Left)
                {
                    if (model.MoveTime > 50) model.MoveTime -= 50;
                }
                else if (pointer == CrossButton.Right)
                {
                    if (model.MoveTime < 2000) model.MoveTime += 50;
                }


                if (model.ControlMode == HexModel.ControlModeType.Translate)
                {
                    model.BodyPos.x = state.X / 2;
                    model.BodyPos.z = -state.Y / 3;
                    model.BodyRot.y = state.RotationX * 2;
                    model.BodyYShift = -state.RotationY / 2;
                }
                else if (model.ControlMode == HexModel.ControlModeType.Rotate)
                {
                    model.BodyRot.x = state.X;
                    model.BodyRot.y = state.RotationX * 2;
                    model.BodyRot.z = state.Y;
                    model.BodyYShift = -state.RotationY / 2;
                }
                model.InputTimeDelay = 128 - Math.Max(Math.Max(Math.Abs(state.X), Math.Abs(state.Y)), Math.Abs(state.RotationX));
            }

            model.BodyPos.y = Math.Min(Math.Max(model.BodyYOffset + model.BodyYShift, 0), IKMathConfig.MaxBodyHeight);


            if (model.AdjustLegsPosition)
            {
                //Thread.Sleep(200);
            }

            return isKeyPressed;
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
            if (Gamepad != null)
            {
                Gamepad.Unacquire();
                Gamepad.Dispose();
            }

            Gamepad = null;
        }
    }
}
