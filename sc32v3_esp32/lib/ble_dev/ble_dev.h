#pragma once
#include <Arduino.h>
#include <BLEDevice.h>

typedef std::function<void(std::string source, uint8_t id, uint8_t* data, size_t length)> ble_data_callback;
void ble_begin(ble_data_callback callback);
void ble_run();