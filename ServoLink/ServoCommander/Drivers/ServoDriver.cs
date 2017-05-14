using ServoCommander.Data;
using ServoLink;
using System;

namespace ServoCommander.Drivers
{
    public class ServoDriver: IDisposable
    {
        public static int[] CoxaOffset = { 20, -40, 0, -20, -40, -20 }; //LF LM LR RR RM RF
        public static int[] FemurOffset = { 30, 20, 50, -170, -120, -20 };//{   70,-100, -55,   0,  45, -40 }; //LF LM LR RR RM RF 
        public static int[] TibiaOffset = { 20, 60, -50, 30, 20, 20 };//{    0,  65, -30,  40,   0,   0 }; //LF LM LR RR RM RF
        public static byte[] LegsMap = { 3, 4, 5, 2, 1, 0 };

        private ServoController _controller;

        public ServoController Controller { get { return _controller; } }

        public ServoDriver()
        {
            _controller = new ServoController(20, new BinaryHelper());
        }

        public bool Init()
        {
            if (!_controller.Connect(new SerialPort("COM3", 115200, 200))) return false;
            _controller.MoveAll(0);
            _controller.Commit();
            return true;
        }

        public void Reset()
        {
            if (!_controller.IsConnected) return;
            _controller.MoveAll(0);
            _controller.Commit();
        }

        public void Commit()
        {
            if (!_controller.IsConnected) return;
            _controller.Commit();
        }

        public void Update(CoxaFemurTibia[] results, ushort moveTime)
        {
            for (byte i=0;i<LegsMap.Length;i++)
            {
                ushort coxaPos = (ushort)(1500 + (results[i].Coxa * 10) + CoxaOffset[LegsMap[i]]);
                ushort femurPos = (ushort)(1500 + (results[i].Femur * 10) + FemurOffset[LegsMap[i]]);
                ushort tibiaPos = (ushort)(1500 + (results[i].Tibia * 10) + TibiaOffset[LegsMap[i]]);
                _controller.Move(LegsMap[i] * 3, tibiaPos, moveTime);
                _controller.Move(LegsMap[i] * 3 + 1, femurPos, moveTime);
                _controller.Move(LegsMap[i] * 3 + 2, coxaPos, moveTime);
            }
        }

        public void Dispose()
        {
            _controller.Disconnect();
        }
    }
}
