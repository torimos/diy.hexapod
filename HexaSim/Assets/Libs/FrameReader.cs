using System;
using System.Collections.Generic;
using System.IO;

public class FrameReader
{
    byte[] frame_buf = new byte[228 * 20];
    int frame_buf_len = 0;
    public class FrameReadyEventArgs
    {
        public uint[] Data;
    }
    public delegate void FrameReadyEventHandler(object sender, FrameReadyEventArgs e);
    public event FrameReadyEventHandler OnFrameReady;

    public void Update(byte[] rx_buf)
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
                UInt32 header = frame_br.ReadUInt32();
                if (header == 0x5332412B)
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

                if (frame_buf_len >= 112)
                {
                    frame_br.BaseStream.Seek(4, SeekOrigin.Begin);
                    ushort data_size = frame_br.ReadUInt16();
                    if (data_size / 4 == 26)
                    {
                        var data = frame_br.ReadBytes(data_size);
                        uint expected_crc32 = Crc32.Get(data);
                        uint actual_crc32 = frame_br.ReadUInt32();
                        bool crc_valid = expected_crc32 == actual_crc32;
                        if (crc_valid)
                        {
                            var data32 = new uint[data.Length / 4];
                            Buffer.BlockCopy(data, 0, data32, 0, data.Length);
                            OnFrameReady(this, new FrameReadyEventArgs { Data = data32 });
                            frame_buf_len = 0;
                        }
                    }
                }
            }
        }
    }
}