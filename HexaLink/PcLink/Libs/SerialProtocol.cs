using SerialPortLib2;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class SerialProtocol: IDisposable
{
    private SerialPortInput serialPort = new SerialPortInput(false);
    private byte[] frame_buf = new byte[1024*4];
    private int frame_buf_len = 0;

    public delegate void FrameReadyEventHandler(object sender, FrameReadyEventArgs e);
    public event FrameReadyEventHandler OnFrameReady;

    Stopwatch sw = new Stopwatch();
    Stopwatch fpssw = new Stopwatch();
    int fpsc = 0, fps = 0;

    public int FPS => fps;

    CancellationTokenSource tokenSource;
    CancellationToken token;
    Task task;

    public void Start()
    {
        sw.Start();
        fpssw.Start();
        serialPort.SetPort("COM3", 115200, readerTaskTime: 25);
        serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
        serialPort.MessageReceived += SerialPort_MessageReceived;
        serialPort.Connect();
        
        tokenSource = new CancellationTokenSource();
        token = tokenSource.Token;
        task = Task.Run(() =>
        {
            while (true)
            {
                Loop();
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        });
    }

    public void Stop()
    {
        serialPort.Disconnect();

        tokenSource.Cancel();
        task.Wait();
        task.Dispose();
    }

    public void SendFrame(FrameHeaderType header, byte[] data)
    {
        MemoryStream ms = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write((ushort)header);
        bw.Write((ushort)data.Length);
        bw.Write(data);
        bw.Write(Crc.Get_CRC16(data));

        serialPort.SendMessage(ms.ToArray());
    }

    public void Dispose()
    {
        Stop();
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
    }

    private void Loop()
    {
        if (sw.ElapsedMilliseconds < 10) return;
        if (frame_buf_len > 0)
        {
            var frame_br = new BinaryReader(new MemoryStream(frame_buf));
            int frame_start_offset = 0;
            FrameHeaderType header = FrameHeaderType.Unknown;
            while (frame_start_offset < (frame_buf_len - 4))
            {
                frame_br.BaseStream.Seek(frame_start_offset, SeekOrigin.Begin);
                header = (FrameHeaderType)frame_br.ReadUInt16();
                if (header == FrameHeaderType.STM32Debug ||
                    header == FrameHeaderType.ESP32Debug)
                {
                    break;
                }
                else
                {
                    header = FrameHeaderType.Unknown;
                }
                frame_start_offset++;
            }
            if (header != FrameHeaderType.Unknown)
            {
                // align frame start with 0 start index in buffer
                MoveFrame(frame_start_offset, (ushort)frame_buf.Length);
                ushort expectedDataSize = GetExpectedDataSize(header);
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
                            OnFrameReady(this, FrameReadyEventArgsBuilder.Create(header, data));
                            frame_buf_len = 0;
                        }
                    }
                }
            }
        }
        sw.Restart();
    }

    private void MoveFrame(int offset, ushort size)
    {
        if (offset <= 0) return;
        Buffer.BlockCopy(frame_buf, offset, frame_buf, 0, frame_buf_len - offset);
        frame_buf_len = frame_buf_len - offset;
        if ((size - frame_buf_len) >= 1)
            for (int i = 0; i < (size - frame_buf_len); i++)
                frame_buf[i + frame_buf_len] = 0x55;
    }

    private ushort GetExpectedDataSize(FrameHeaderType header)
    {
        switch (header)
        {
            case FrameHeaderType.ESP32Debug:
                return 26 * 4 + 8 * 3 * 3 + 1 + 4;
            case FrameHeaderType.STM32Debug:
                return 26 * 4;
        }
        return 0;
    }

    private void SerialPort_MessageReceived(object sender, MessageReceivedEventArgs args)
    {
        if (fpssw.ElapsedMilliseconds >= 1000)
        {
            fps = fpsc;
            fpsc = 0;
            fpssw.Restart();
        }
        fpsc++;
        Update(args.Data);
    }

    private void SerialPort_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
    {
    }
}