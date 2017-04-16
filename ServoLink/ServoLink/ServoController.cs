using System;
using System.IO;
using System.Linq;
using ServoLink.Contracts;

namespace ServoLink
{
    public class ServoController: IServoController
    {
        private readonly IBinaryHelper _binaryHelper;
        private IPort _port;
        private readonly ushort[] _servos;

        public ushort[] Servos
        {
            get { return _servos; }
        }

        public ServoController(ushort numberOfServos, IBinaryHelper binaryHelper)
        {
            if (binaryHelper == null) throw new ArgumentNullException("binaryHelper");
            _binaryHelper = binaryHelper;

            _servos = new ushort[numberOfServos == 0 ? 1 : numberOfServos];
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

        public void Sync()
        {
            if (_port == null || !_port.IsOpen) return;
            var buffer = _binaryHelper.ConvertToByteArray((uint)_servos.Length*2, _servos);
            _port.Write(buffer, 0, buffer.Length);
        }
        
        public void SetAll(ushort position)
        {
            for (var i = 0; i < _servos.Length; i++)
            {
                _servos[i] = position;
            }
        }

        private void OnDataReceived(object sender, PortDataReceivedEventArgs e)
        {
            Console.WriteLine(new String(e.Data.Select(d => (char) d).ToArray()));
        }
    }
}
