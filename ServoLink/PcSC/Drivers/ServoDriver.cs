using Contracts;
using CRC;
using Data;
using System;
using System.Linq;
using Utils;

namespace Drivers
{
    public class ServoDriver: IDisposable
    {
        public static int[] CoxaOffset = { 20, -40, 0, -20, -40, -20 }; //LF LM LR RR RM RF
        public static int[] FemurOffset = { 30, 20, 50, -170, -120, -20 };//{   70,-100, -55,   0,  45, -40 }; //LF LM LR RR RM RF 
        public static int[] TibiaOffset = { 20, 60, -50, 30, 20, 20 };//{    0,  65, -30,  40,   0,   0 }; //LF LM LR RR RM RF
        public static byte[] LegsMap = { 3, 4, 5, 2, 1, 0 };
        private readonly uint[] _servos;
        private readonly IBinaryHelper _binaryHelper = new BinaryHelper();
        private ISerialPortDriver _port;
        string _response;

        public ServoDriver(ushort numberOfServos)
        {
            _servos = new uint[numberOfServos == 0 ? 1 : numberOfServos];
        }

        public bool Init(string port)
        {
            if (!Connect(new SerialPortDriver(port, 115200, 200))) return false;
            MoveAll(0, 0);
            Commit(100);
            return true;
        }

        public void Reset()
        {
            if (!IsConnected()) return;
            MoveAll(0, 0);
            Commit(100);
        }

        public void Commit()
        {
            if (!IsConnected()) return;
            Commit(100);
        }

        public void Update(CoxaFemurTibia[] results, ushort moveTime)
        {
            for (byte i=0;i<LegsMap.Length;i++)
            {
                ushort coxaPos = (ushort)(1500 + (results[i].Coxa * 10) + CoxaOffset[LegsMap[i]]);
                ushort femurPos = (ushort)(1500 + (results[i].Femur * 10) + FemurOffset[LegsMap[i]]);
                ushort tibiaPos = (ushort)(1500 + (results[i].Tibia * 10) + TibiaOffset[LegsMap[i]]);
                Move(LegsMap[i] * 3, tibiaPos, moveTime);
                Move(LegsMap[i] * 3 + 1, femurPos, moveTime);
                Move(LegsMap[i] * 3 + 2, coxaPos, moveTime);
            }
        }

        public string ReadLastResult()
        {
            if (!IsConnected()) return string.Empty;
            return GetLastResult();
        }

        public void Dispose()
        {
            if (IsConnected())
            {
                _port.Close();
            }
        }
        private bool IsConnected()
        {
            return (_port != null && _port.IsOpen);
        }

        private bool Connect(ISerialPortDriver port)
        {
            if (port == null) throw new ArgumentNullException("port");
            _port = port;

            bool opened = !_port.IsOpen ? _port.Open() : _port.IsOpen;

            if (opened)
            {
                _port.DataReceived += OnDataReceived;
            }

            return opened;
        }

        public int Commit(int timeOut = 200)
        {
            _response = string.Empty;
            int retry = 0;
            if (_port == null || !_port.IsOpen) return 0;
            var crc = Crc.ComputeHash(CrcAlgorithms.Crc32Mpeg2, _servos);
            var buffer = _binaryHelper.ConvertToByteArray(_servos, (UInt32)crc);
            _port.Write(buffer, 0, buffer.Length);
            return retry;
        }

        public void MoveAll(ushort position, ushort moveTime = 0)
        {
            for (var i = 0; i < _servos.Length; i++)
            {
                Move(i, position, moveTime);
            }
        }

        public void Move(int index, ushort position, ushort moveTime = 0)
        {
            _servos[index] = (uint)(moveTime << 16) | position;
        }

        public string GetLastResult()
        {
            return _response;
        }

        private void OnDataReceived(object sender, PortDataReceivedEventArgs e)
        {
            _response += new String(e.Data.Select(d => (char)d).ToArray());
        }
    }
}
