#pragma once
#include "Stream.h"
#include <stdint.h>

class SerialProtocol
{
    Stream* stream;
    uint8_t frame_buf[512];
    uint8_t rx_buf[256];
    int frame_buf_len = 0;

public:
    SerialProtocol(Stream* stream);

    void write(uint16_t header, void* data, uint16_t size);

    bool read(uint16_t header, void* data, uint16_t size);

private:

    bool parse_frame(uint16_t header, void* result_data, uint16_t expected_size);

    uint16_t stream_read(uint8_t* buffer, uint16_t size);

    uint16_t frame_buf_read();

    void move_frame(uint16_t offset);
};

