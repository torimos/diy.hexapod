using System;
using System.Threading;

namespace test
{
    public class FrameProtocol
    {
        enum RX_STATE: int
        {
            SEARCHING_SOF,
            RECEIVING_PAYLOAD,
        }

        byte[] rx_buffer = new byte[8];
        RX_STATE rx_state = RX_STATE.SEARCHING_SOF;
        int rx_header_bytes = 0;
        int rx_payload_bytes = 0;
        int rx_errors = 0;

        public int GetErrorsCount()
        {
            return rx_errors;
        }

        public byte[] GetBuffer()
        {
            return rx_buffer.Clone() as byte[];
        }

        public int rx_pool(byte data)
        {
            if (rx_header_bytes == 1)
            {
                rx_header_bytes = 0;
                if ((data & 0xF0) == 0x40)
                {
                    rx_payload_bytes = 0;
                    rx_buffer[7 - (rx_payload_bytes++)] = 0xFD;
                    rx_buffer[7 - (rx_payload_bytes++)] = data;
                    rx_state = RX_STATE.RECEIVING_PAYLOAD;
                    return 0;
                }
                else
                {
                    rx_errors++;
                    rx_payload_bytes = 0;
                    rx_state = RX_STATE.SEARCHING_SOF;
                    return 0;
                }
            }

            if (data == 0xFD)
            {
                rx_header_bytes++;
            }
            else
            {
                rx_header_bytes = 0;
            }
            switch (rx_state)
            {
                case RX_STATE.SEARCHING_SOF:
                    break;
                case RX_STATE.RECEIVING_PAYLOAD:
                    if (data == 0xFD)// ? start of next frame
                    {
                        rx_state = RX_STATE.SEARCHING_SOF;
                        return 1;
                    }
                    else if (rx_payload_bytes > 8)// ? overflow
                    {
                        rx_errors++;
                        rx_state = RX_STATE.SEARCHING_SOF;
                    }
                    else
                    {
                        rx_buffer[7 - (rx_payload_bytes++)] = data;
                    }
                    break;
            }
            return 0;
        }

        void rx_pool(byte[] data, int offset = 0)
        {
            for (int i = offset; i < data.Length; i++)
            {
                rx_pool(data[i]);
            }
        }

        public void Debug()
        {
            UInt64 _rawState = 1;
            var _buffer = new byte[] { 0xFD, 0x44, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00 };
            var rnd = new Random();
            int offset = 0;
            while (!Console.KeyAvailable)
            {
                if (_rawState % 4 == 0) offset = rnd.Next(8);
                else offset = 0;
                _buffer[7] = (byte)(_rawState & 0xFF);
                _buffer[6] = (byte)((_rawState >> 8) & 0xFF);
                _buffer[5] = (byte)((_rawState >> 16) & 0xFF);
                _buffer[4] = (byte)((_rawState >> 32) & 0xFF);
                rx_pool(_buffer, offset);
                _rawState++;
                Thread.Sleep(500);
            }
        }
    }
}
