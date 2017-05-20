using System;
using System.Linq;

namespace GcplTest
{

    public class FifoBuffer
    {
        public byte[] Data { get; set; }
        public int First { get; set; }
        public int Last { get; set; }
        public int DataSize { get; set; }
    }

    public class SerialPort
    {
        public class PortDataReceivedEventArgs : EventArgs
        {
            public byte[] Data { get; set; }
        }

        public delegate void PortDataReceivedEventHandler(object sender, PortDataReceivedEventArgs e);

        private const int READ_BUFFER_SIZE = 32;
        private readonly System.IO.Ports.SerialPort _io;
        private readonly byte[] _dataBuffer = new byte[READ_BUFFER_SIZE];
        public event PortDataReceivedEventHandler DataReceived;

        public SerialPort(string portName, int baudRate, int timeout)
        {
            _io = new System.IO.Ports.SerialPort(portName, baudRate);
            _io.WriteTimeout = timeout;
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

    class Program
    {
        static void Main(string[] args)
        {
            var port = new SerialPort("COM6", 115200, 200);
            port.DataReceived += Port_DataReceived;
            port.Open();
            while (!Console.KeyAvailable) {
            }
            port.Close();
        }

        private static void Port_DataReceived(object sender, SerialPort.PortDataReceivedEventArgs e)
        {
            UInt64 t = BitConverter.ToUInt64(e.Data, 0);
            Console.WriteLine($"{t:X}");
        }
    }
}
