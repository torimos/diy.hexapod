#pragma once
#include <Arduino.h>
#include <HardwareSerial.h>
#include "StateLed.h"
#include "crc32.h"
#include "Stopwatch.h"

extern HardwareSerial Log;
extern HardwareSerial STM32uart;
extern HardwareSerial DEBUGuart;

void ResetSTM32();
void Platform_Init();