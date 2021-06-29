#include "SerialProtocol.h"
#include "crc.h"
#define EMPTY_BYTE 0x55

SerialProtocol::SerialProtocol(Stream* stream)
{
    this->stream = stream;
    memset(frame_buf, EMPTY_BYTE, sizeof(frame_buf));
}

void SerialProtocol::write(uint16_t header, void* data, uint16_t size)
{
    uint16_t checksum = get_CRC16(data, size);
    stream->write((uint8_t*)&header, sizeof(uint16_t));
    stream->write((uint8_t*)&size, sizeof(uint16_t));
    stream->write((uint8_t*)data, size);
    stream->write((uint8_t*)&checksum, sizeof(uint16_t));
}

bool SerialProtocol::read(uint16_t header, void* data, uint16_t size) {
    if (header == 0 || data == nullptr || size > (SERIAL_PROTOCOL_MAX_BUFFER_SIZE >> 1)) return false;
    frame_buf_read(size);
    return parse_frame(header, data, size);
}

bool SerialProtocol::parse_frame(uint16_t header, void* result_data, uint16_t expected_data_size)
{
    if (frame_buf_len > 0)
    {
        uint16_t rx_buf_max_len = (expected_data_size+6);
        uint16_t frame_buf_max_len = rx_buf_max_len << 1;
        int frame_start_offset = 0;
        bool header_found = false;
        while (frame_start_offset < (frame_buf_len - 4)) // offset should include at least 4 bytes of [header]+[data len]
        {
            uint16_t* hdr = (uint16_t*)&frame_buf[frame_start_offset];
            if (*hdr == header)
            {
                header_found = true;
                break;
            }
            frame_start_offset++;
        }
        if (header_found)
        {
            // align frame start with 0 start index in buffer
            move_frame(frame_start_offset, frame_buf_max_len);

            //process frame if buffer len is as expected + 6 bytes = [header](2)+[data len](2)+[crc](2)
            if (frame_buf_len >= rx_buf_max_len)
            {
                uint16_t data_size = *(uint16_t*)&frame_buf[2]; //skip 2 bytes of [header]
                if (data_size == expected_data_size)
                {
                    uint16_t expected_crc = get_CRC16(&frame_buf[4], data_size);  //skip 2 bytes of [data len]
                    uint16_t crc = *(uint16_t*)&frame_buf[4 + data_size];
                    if (crc == expected_crc)
                    {
                        memcpy(result_data, &frame_buf[4], data_size);
                        frame_buf_len = 0;
                        return true;
                    }
                }
            }
        }
    }
    return false;
}

uint16_t SerialProtocol::stream_read(uint8_t* buffer, uint16_t size)
{
    size_t avail = stream->available();
    if (size < avail) {
        avail = size;
    }
    size_t count = 0;
    while (count < avail) {
        *buffer++ = stream->read();
        count++;
    }
    stream->flush();
    return count;
}

uint16_t SerialProtocol::frame_buf_read(uint16_t expected_data_size)
{
    uint16_t rx_buf_max_len = (expected_data_size+6);
    uint16_t frame_buf_max_len = rx_buf_max_len << 1;
    size_t rx_buf_len = stream_read(rx_buf, rx_buf_max_len);
    if (rx_buf_len <= 0)
        return rx_buf_len;
    else
    {
        if ((frame_buf_len + rx_buf_len) > frame_buf_max_len)
        {
            // data overflow
            frame_buf_len = 0;
        }
        memcpy(frame_buf + frame_buf_len, rx_buf, rx_buf_len);
        frame_buf_len += rx_buf_len;
        return rx_buf_len;
    }
    return 0;
}

void SerialProtocol::move_frame(uint16_t offset, uint16_t size)
{
    if (offset <= 0) return;
    memmove(frame_buf, &frame_buf[offset], frame_buf_len - offset);
    frame_buf_len = frame_buf_len - offset;
    if ((size - frame_buf_len) >= 1)
        memset(frame_buf + frame_buf_len, EMPTY_BYTE, size - frame_buf_len);
}