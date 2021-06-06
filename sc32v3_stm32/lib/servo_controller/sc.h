#pragma once
#include <Arduino.h>

void sc_init(HardwareSerial* inputSerial);
void sc_loop();
void sc_write(int sid, int us);
void sc_write_all(int us);