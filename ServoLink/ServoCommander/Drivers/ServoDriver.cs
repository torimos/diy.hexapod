using ServoCommander.Data;
using ServoLink;
using System;

namespace ServoCommander.Drivers
{
    public class ServoDriver: IDisposable
    {
        public static short[] CoxaOffset =  {   15, -50,   0, -15, -50,   0 };  //LF LM LR RR RM RF
        public static short[] FemurOffset = {   70,-100, -55,   0,  45, -40 }; //LF LM LR RR RM RF 
        public static short[] TibiaOffset = {    0,  65, -30,  40,   0,   0 }; //LF LM LR RR RM RF
        public static byte[] LegsMap = { 3, 4, 5, 2, 1, 0 };

        private ServoController _controller;

        public ServoDriver()
        {
            _controller = new ServoController(20, new BinaryHelper());
        }

        public bool Init()
        {
            if (!_controller.Connect(new SerialPort("COM3", 115200))) return false;
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

        public void UpdateLegPos(byte legNumber, ushort coxaPos, ushort femurPos, ushort tibiaPos, ushort moveTime)
        {
            _controller.Move(legNumber * 3, (ushort)(tibiaPos + TibiaOffset[legNumber]), moveTime);
            _controller.Move(legNumber * 3 + 1, (ushort)(femurPos + FemurOffset[legNumber]), moveTime);
            _controller.Move(legNumber * 3 + 2, (ushort)(coxaPos + CoxaOffset[legNumber]), moveTime);
        }

        private void UpdateLegAngle(byte legIndex, double coxaAngle, double femurAngle, double tibiaAngle, ushort moveTime)
        {
            ushort coxaPos = (ushort)(1500 + (coxaAngle * 10));
            ushort femurPos = (ushort)(1500 + (femurAngle * 10));
            ushort tibiaPos = (ushort)(1500 + (tibiaAngle * 10));
            UpdateLegPos(legIndex, coxaPos, femurPos, tibiaPos, moveTime);
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
                UpdateLegAngle(LegsMap[i], results[i].Coxa, results[i].Femur, results[i].Tibia, moveTime);
            }
        }

        public void Dispose()
        {
            _controller.Disconnect();
        }
    }
}
