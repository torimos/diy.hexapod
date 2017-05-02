using System;

namespace ServoCommander
{
    public class InputDriver
    {
        public bool? ProcessInput(HexModel model)
        {
            if (Console.KeyAvailable)
            {
                double step = 5;

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape) return false;
                else if (key == ConsoleKey.Enter)
                {
                    if (model.PowerOn)
                    {
                        TurnOff(model);
                        model.PowerOn = false;
                    }
                    else
                    {
                        model.PowerOn = true;
                    }
                }
                else if (key == ConsoleKey.A)
                {
                    model.BodyRot.x += step;
                }
                else if (key == ConsoleKey.Z)
                {
                    model.BodyRot.x -= step;
                }
                else if (key == ConsoleKey.S)
                {
                    model.BodyRot.z += step;
                }
                else if (key == ConsoleKey.X)
                {
                    model.BodyRot.z -= step;
                }
                else if (key == ConsoleKey.D)
                {
                    model.BodyRot.y += step;
                }
                else if (key == ConsoleKey.C)
                {
                    model.BodyRot.y -= step;
                }
                else if (key == ConsoleKey.F)
                {
                    model.BodyPos.x += step;
                }
                else if (key == ConsoleKey.V)
                {
                    model.BodyPos.x -= step;
                }
                else if (key == ConsoleKey.G)
                {
                    model.BodyPos.z += step;
                }
                else if (key == ConsoleKey.B)
                {
                    model.BodyPos.z -= step;
                }
                else if (key == ConsoleKey.H)
                {
                    model.BodyPos.y += step;
                }
                else if (key == ConsoleKey.N)
                {
                    model.BodyPos.y -= step;
                }
                else if (key == ConsoleKey.F1)
                {
                    model.SelectedLeg = 0;
                }
                else if (key == ConsoleKey.F2)
                {
                    model.SelectedLeg = 1;
                }
                else if (key == ConsoleKey.F3)
                {
                    model.SelectedLeg = 2;
                }
                else if (key == ConsoleKey.F4)
                {
                    model.SelectedLeg = 3;
                }
                else if (key == ConsoleKey.F5)
                {
                    model.SelectedLeg = 4;
                }
                else if (key == ConsoleKey.F6)
                {
                    model.SelectedLeg = 5;
                }
                else if (key == ConsoleKey.F7)
                {
                    model.SelectedLeg = 0xFF;
                }
                return true;
            }
            return null;
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
