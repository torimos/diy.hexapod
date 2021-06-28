using SerialPortLib2;
using System;
using System.IO;

public class FrameReader
{
    private SerialPortInput serialPort = new SerialPortInput(false);
    private byte[] frame_buf = new byte[1024*2];
    private int frame_buf_len = 0;

    public delegate void FrameReadyEventHandler(object sender, FrameReadyEventArgs e);
    public event FrameReadyEventHandler OnFrameReady;

    public void Create()
    {
        serialPort.SetPort("COM3", 115200);
        serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
        serialPort.MessageReceived += SerialPort_MessageReceived;
        serialPort.Connect();
    }

    internal void Destroy()
    {
        serialPort.Disconnect();
    }

    private void Update(byte[] rx_buf)
    {
        int rx_len = rx_buf.Length;
        if (rx_len > 0)
        {
            if ((frame_buf_len + rx_len) > frame_buf.Length)
            {
                // data overflow
                frame_buf_len = 0;
            }
            Buffer.BlockCopy(rx_buf, 0, frame_buf, frame_buf_len, rx_len);

            frame_buf_len += rx_len;
        }

        if (frame_buf_len > 0)
        {
            var frame_br = new BinaryReader(new MemoryStream(frame_buf));
            int frame_start_offset = 0;
            bool header_found = false;
            while (frame_start_offset < (frame_buf_len - 4))
            {
                frame_br.BaseStream.Seek(frame_start_offset, SeekOrigin.Begin);
                var header = frame_br.ReadUInt16();
                if (header == 0x412B)//0x5332412B
                {
                    header_found = true;
                    break;
                }
                frame_start_offset++;
            }
            if (header_found)
            {
                // align frame start with 0 start index in buffer
                Buffer.BlockCopy(frame_buf, frame_start_offset, frame_buf, 0, frame_buf_len - frame_start_offset);

                frame_buf_len = frame_buf_len - frame_start_offset;
                if ((frame_buf.Length - frame_buf_len) >= 1)
                    for (int i = 0; i < frame_buf.Length - frame_buf_len; i++)
                        frame_buf[i + frame_buf_len] = 0x55;
                
                const int expectedDataSize = 177;
                if (frame_buf_len >= expectedDataSize)
                {
                    frame_br.BaseStream.Seek(2, SeekOrigin.Begin);
                    ushort data_size = frame_br.ReadUInt16();
                    if (data_size == expectedDataSize)
                    {
                        var data = frame_br.ReadBytes(data_size);
                        var expected_crc = Crc.Get_CRC16(data);
                        var actual_crc = frame_br.ReadUInt16();
                        bool crc_valid = actual_crc == expected_crc;
                        if (crc_valid)
                        {
                            OnFrameReady(this, FrameReadyEventArgsBuilder.Create(data));
                            frame_buf_len = 0;
                        }
                    }
                }
            }
        }
    }

    private void SerialPort_MessageReceived(object sender, MessageReceivedEventArgs args)
    {
        Update(args.Data);
    }

    private void SerialPort_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
    {
    }
}