using System;
using System.Linq;
using ServoLink.Contracts;

namespace ServoLink
{
    public class SerialPort: IPort
    {
        private BinaryHelper _binaryHelper = new BinaryHelper();
        private const int READ_BUFFER_SIZE = 4096;
        private readonly System.IO.Ports.SerialPort _io;
        private readonly byte[] _dataBuffer = new byte[READ_BUFFER_SIZE];
        public event PortDataReceivedEventHandler DataReceived;
        
        public SerialPort(string portName, int baudRate, int writeTimeout)
        {
            _io = new System.IO.Ports.SerialPort(portName, baudRate);
            _io.WriteTimeout = writeTimeout;
        }

        public bool IsOpen
        {
            get { return _io.IsOpen; }
        }

        public bool Open()
        {
            var ports = System.IO.Ports.SerialPort.GetPortNames();
            if (ports.Contains(_io.PortName))
            {
                try
                {
                    //_io.ReadBufferSize = READ_BUFFER_SIZE;
                    _io.DataReceived += OnDataReceived;
                    _io.Open();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return IsOpen;
        }

        public void Close()
        {
            _io.Close();
        }

        public void Write(byte[] data, int offset, int size)
        {
            _io.Write(data, offset, size);
        }

        private void OnDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (_io.BytesToRead == 0) return;
            var dataSize = _io.Read(_dataBuffer, 0, _io.BytesToRead);
            var handler = DataReceived;
            if (handler != null)
            {
                DataReceived(sender, new PortDataReceivedEventArgs { Data = _dataBuffer.Take(dataSize).ToArray() });    
            }
        }
    }
}
