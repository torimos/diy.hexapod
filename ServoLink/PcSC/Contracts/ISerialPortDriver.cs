using System;

namespace Contracts
{
    public interface ISerialPortDriver
    {
        event PortDataReceivedEventHandler DataReceived;

        bool IsOpen { get; }
        bool Open();
        void Close();
        void Write(byte[] data, int offset, int size);
    }

    public class PortDataReceivedEventArgs: EventArgs
    {
        public byte[] Data { get; set; }
    }

    public delegate void PortDataReceivedEventHandler(object sender, PortDataReceivedEventArgs e);
}
