#pragma once
#include <Arduino.h>
#include <HardwareSerial.h>
#include "StateLed.h"
#include "CRC.h"
#include "Stopwatch.h"

extern HardwareSerial Log;
extern HardwareSerial STM32uart;

void ResetSTM32();
void Platform_Init();