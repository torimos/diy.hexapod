#pragma once
#include "Stream.h"
#include <stdint.h>

#define SERIAL_PROTOCOL_MAX_BUFFER_SIZE 1024

class SerialProtocol
{
    Stream* stream;
    uint8_t frame_buf[SERIAL_PROTOCOL_MAX_BUFFER_SIZE];
    uint8_t rx_buf[SERIAL_PROTOCOL_MAX_BUFFER_SIZE >> 1];
    int frame_buf_len = 0;

public:
    SerialProtocol(Stream* stream);

    void write(uint16_t header, void* data, uint16_t size);

    bool read(uint16_t header, void* data, uint16_t size);

private:

    bool parse_frame(uint16_t header, void* result_data, uint16_t expected_data_size);

    uint16_t stream_read(uint8_t* buffer, uint16_t size);

    uint16_t frame_buf_read(uint16_t expected_data_size);

    void move_frame(uint16_t offset, uint16_t size);
};

