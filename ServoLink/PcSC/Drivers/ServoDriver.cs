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
            Reset();
            return true;
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

        public void Commit(int timeOut = 100)
        {
            _response = string.Empty;
            if (!IsConnected()) return;
            var crc = Crc.ComputeHash(CrcAlgorithms.Crc32Mpeg2, _servos);
            var buffer = _binaryHelper.ConvertToByteArray(_servos, (UInt32)crc);
            _port.Write(buffer, 0, buffer.Length);
        }

        public void Reset()
        {
            if (!IsConnected()) return;
            MoveAll(0);
            Commit();
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
