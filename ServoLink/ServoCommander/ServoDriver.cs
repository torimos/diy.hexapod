using ServoLink;
using System;

namespace ServoCommander
{
    public class ServoDriver: IDisposable
    {
        public static short[] CoxaOffset =  {   15, -50,   0, -15, -50,   0 };  //LF LM LR RR RM RF
        public static short[] FemurOffset = {   70,-100, -55,   0,  45, -40 }; //LF LM LR RR RM RF 
        public static short[] TibiaOffset = {    0,  65, -30,  40,   0,   0 }; //LF LM LR RR RM RF

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

        private void UpdateLegAngle(byte legNumber, double coxaAngle, double femurAngle, double tibiaAngle, ushort moveTime)
        {
            ushort coxaPos = (ushort)(1500 + (coxaAngle * 10));
            ushort femurPos = (ushort)(1500 + (femurAngle * 10));
            ushort tibiaPos = (ushort)(1500 + (tibiaAngle * 10));
            UpdateLegPos(legNumber, coxaPos, femurPos, tibiaPos, moveTime);
        }
        public void Commit()
        {
            if (!_controller.IsConnected) return;
            _controller.Commit();
        }

        public void Update(CoxaFemurTibia[] results, ushort moveTime)
        {
            UpdateLegAngle(0, results[3].Coxa, results[3].Femur, results[3].Tibia, moveTime);//LF
            UpdateLegAngle(1, results[4].Coxa, results[4].Femur, results[4].Tibia, moveTime);//LM
            UpdateLegAngle(2, results[5].Coxa, results[5].Femur, results[5].Tibia, moveTime);//LR
            UpdateLegAngle(5, results[0].Coxa, results[0].Femur, results[0].Tibia, moveTime);//RR
            UpdateLegAngle(4, results[1].Coxa, results[1].Femur, results[1].Tibia, moveTime);//RM
            UpdateLegAngle(3, results[2].Coxa, results[2].Femur, results[2].Tibia, moveTime);//RF
        }

        public void Dispose()
        {
            _controller.Disconnect();
        }
    }
}
