#pragma once
#include <stdint.h>

uint16_t get_CRC16(void* data, uint16_t size);
uint32_t get_CRC32(void *data, uint16_t size);