using System;
using System.Linq;

namespace test
{
    public class SerialPort
    {
        public class PortDataReceivedEventArgs : EventArgs
        {
            public byte[] Data { get; set; }
        }

        public delegate void PortDataReceivedEventHandler(object sender, PortDataReceivedEventArgs e);

        private const int READ_BUFFER_SIZE = 1024;
        private readonly System.IO.Ports.SerialPort _io;
        private readonly byte[] _dataBuffer = new byte[READ_BUFFER_SIZE];
        public event PortDataReceivedEventHandler DataReceived;

        public int ReadChunkSize { get; set; }

        public SerialPort(string portName, int baudRate, int timeout)
        {
            _io = new System.IO.Ports.SerialPort(portName, baudRate);
            _io.WriteTimeout = _io.ReadTimeout = timeout;
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
            if (_io.BytesToRead == 0 || (ReadChunkSize > 0 ? _io.BytesToRead < ReadChunkSize : false)) return;
            var dataSize = _io.Read(_dataBuffer, 0, _io.BytesToRead);
            var handler = DataReceived;
            if (handler != null)
            {
                DataReceived(sender, new PortDataReceivedEventArgs { Data = _dataBuffer.Take(dataSize).ToArray() });
            }
        }
    }

}
