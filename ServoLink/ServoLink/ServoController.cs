using System;
using System.IO;
using System.Linq;
using ServoLink.Contracts;
using System.Security.Cryptography;
using CRC;
using System.Diagnostics;
using System.Threading;

namespace ServoLink
{
    public class ServoController: IServoController
    {
        private readonly IBinaryHelper _binaryHelper;
        private IPort _port;
        private readonly uint[] _servos;
        public ServoController(ushort numberOfServos, IBinaryHelper binaryHelper)
        {
            if (binaryHelper == null) throw new ArgumentNullException("binaryHelper");
            _binaryHelper = binaryHelper;

            _servos = new uint[numberOfServos == 0 ? 1 : numberOfServos];
        }

        public bool Connect(IPort port)
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

        public void Disconnect()
        {
            if (_port != null && _port.IsOpen)
            {
                _port.Close();
            }
        }
        string _response;
        public int Commit(int timeOut = 100)
        {
            int retry = 0;
            if (_port == null || !_port.IsOpen) return 0;
            var crc = Crc.ComputeHash(CrcAlgorithms.Crc32Mpeg2, _servos);
            var buffer = _binaryHelper.ConvertToByteArray(_servos, (UInt32)crc);

            //var sw = new Stopwatch();
            //sw.Start();
            //_response = "ER";
            //while (sw.ElapsedMilliseconds < timeOut)
            //{
            //    if (_response == "OK") break;
            //    if (_response == "ER")
            //    {
            //        sw.Restart();
            //        _response = "";
            //        _port.Write(buffer, 0, buffer.Length);
            //        retry++;
            //        Thread.Sleep(50);
            //    }
            //}
            _port.Write(buffer, 0, buffer.Length);
            //Thread.Sleep(50);
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

        private void OnDataReceived(object sender, PortDataReceivedEventArgs e)
        {
            _response += new String(e.Data.Select(d => (char)d).ToArray());
        }
    }
}
