using ServoLink;
using System;

namespace ServoCommander
{
    partial class Program
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
                if (!_controller.Connect(new SerialPort("COM6", 115200))) return false;
                _controller.MoveAll(0);
                _controller.Commit();
                Console.WriteLine("Connected");
                return true;
            }

            public void Reset()
            {
                _controller.MoveAll(0);
                _controller.Commit();
            }

            private void UpdateLegPos(byte legNumber, ushort coxaPos, ushort femurPos, ushort tibiaPos, ushort moveTime)
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
            private void Commit()
            {
                _controller.Commit();
            }

            public void Update(IKMath.IKLegResult[] results, ushort moveTime)
            {
                if (results[3].Solution != IKMath.IKSolutionResultType.Error) UpdateLegAngle(0, results[3].CoxaAngle, results[3].FemurAngle, results[3].TibiaAngle, moveTime);//LF
                if (results[4].Solution != IKMath.IKSolutionResultType.Error) UpdateLegAngle(1, results[4].CoxaAngle, results[4].FemurAngle, results[4].TibiaAngle, moveTime);//LM
                if (results[5].Solution != IKMath.IKSolutionResultType.Error) UpdateLegAngle(2, results[5].CoxaAngle, results[5].FemurAngle, results[5].TibiaAngle, moveTime);//LR
                if (results[0].Solution != IKMath.IKSolutionResultType.Error) UpdateLegAngle(3, results[0].CoxaAngle, results[0].FemurAngle, results[0].TibiaAngle, moveTime);//RR
                if (results[1].Solution != IKMath.IKSolutionResultType.Error) UpdateLegAngle(4, results[1].CoxaAngle, results[1].FemurAngle, results[1].TibiaAngle, moveTime);//RM
                if (results[2].Solution != IKMath.IKSolutionResultType.Error) UpdateLegAngle(5, results[2].CoxaAngle, results[2].FemurAngle, results[2].TibiaAngle, moveTime);//RF
                Commit();
            }

            public void Dispose()
            {
                _controller.Disconnect();
            }
        }
    }
}
